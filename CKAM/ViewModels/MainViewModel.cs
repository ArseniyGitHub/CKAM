namespace CKAM.ViewModels;
using CKAM.Services;
using CKAM.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;

public partial class MainViewModel : ViewModelBase
{
    private readonly ChatService chatService = new();
    [ObservableProperty]
    private string username = "";
    [ObservableProperty]
    private string password = "";
    [ObservableProperty]
    private string messageContent = "";
    [ObservableProperty]
    private bool isLoggedIn = false;
    private long user_id;
    [ObservableProperty]
    private string statusMessage = "";
    [ObservableProperty]
    private bool isRegisterMode = false;
    [ObservableProperty]
    private string authButtonText = "Выйти";
    [ObservableProperty]
    private string switchModeText = "Нет аккаунта? Пожаловаться";
    [ObservableProperty]
    private string currentChatName = "";
    [ObservableProperty]
    private string currentChatStatus = "";
    [ObservableProperty]
    private Chat? selectedChat;
    [ObservableProperty]
    private bool isCreatingChatOverlay = false;
    [ObservableProperty]
    private string newChatName = "";
    [ObservableProperty]
    private string newChatDescr = "";
    [ObservableProperty]
    private string newChatType = "";
    [ObservableProperty]
    private List<string> variantsOfChatType = new() { "group", "channel", "private" };
    [ObservableProperty]
    private ObservableCollection<FileAttachmentViewModel> attachments = new();
    [RelayCommand]
    async Task AddFile()
    {
        var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d ? d.MainWindow : null);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions { AllowMultiple = true });
        
        foreach (var file in files)
        {
            var vm = new FileAttachmentViewModel { Name = file.Name, Path = file.Path.LocalPath  };
            
        }
    }

    partial void OnSelectedChatChanged(Chat? value)
    {
        if (value != null)
        {
            SelectedChat = value;
            CurrentChatName = value.Name;
            LoadChatHistoryAsync(value.Id);
        }
    }

    private async Task LoadChatHistoryAsync(long chat_id)
    {
        var history = await chatService.GetChatHistoryAsync(chat_id);
        Dispatcher.UIThread.Post(() =>
        {
            Messages.Clear();
            foreach (var message in history) Messages.Add(new(message, user_id));
        });
    }

    public ObservableCollection<MessageViewModel> Messages { get; } = new();
    public ObservableCollection<Chat> Chats { get; } = new();
    async Task LoginAsync()
    {
        try
        {
            if (await chatService.LoginAsync(Username, Password))
            {
                var chats = await chatService.GetChatsAsync();
                Dispatcher.UIThread.Post(() =>
                {
                    Chats.Clear();
                    foreach (var chat in chats) Chats.Add(chat);
                });
                // var history = await chatService.GetChatHistoryAsync();
                /*
                 * Dispatcher.UIThread.Post(() =>
                {
                    Messages.Clear();
                    foreach (var message in history) Messages.Add(message);
                });
                */
                await chatService.ConnectWebSocketAsync();
                chatService.OnMessageReceived += (message => Dispatcher.UIThread.Post(() => Messages.Add(new(message, user_id))));
                IsLoggedIn = true;
            }
            else IsLoggedIn = false;
        }
        catch(System.Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    [RelayCommand]
    async Task ExecAuthAsync()
    {
        if (IsRegisterMode)
        {
            var error = await chatService.RegisterAsync(Username, Password);
            if (error == null)
            {
                await LoginAsync();
            }
            else
            {
                StatusMessage = error;
            }
        }
        else await LoginAsync();
    }
    [RelayCommand]
    async Task SendMessageAsync()
    {
        if(string.IsNullOrWhiteSpace(MessageContent)) return;
        if (SelectedChat == null) return;
        var selectedFiles = new List<string>();
        foreach (var e in attachments) selectedFiles.Append(e.Path);
        var tempMsg = new MessageViewModel{ Content = MessageContent, ChatId = SelectedChat.Id, CreatedAt = DateTime.Now.ToString("HH:mm"), IsSending = true };
        Messages.Add(tempMsg);
        _ = Task.Run(async () => 
        {
            try
            {
                List<string> uploaded_ids = new();
                foreach (var e in attachments)
                {
                    var id = await chatService.UploadFileAsync(e.Path, p => { });
                    if (id != null)
                    {
                        e.Id = id;
                        uploaded_ids.Append(id);
                    }
                }
                await chatService.SendMessageAsync(MessageContent, SelectedChat.Id, selectedFiles);
                Dispatcher.UIThread.Post(() =>
                {
                    tempMsg.IsSending = false;
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Dispatcher.UIThread.Post(() =>
                {
                    tempMsg.IsSending = false;
                    tempMsg.IsError = true;
                });
                //TODO: Доделать отправку сообщений с вложениями.
            }
            
        });

        // Messages.Add(new Message { SenderName = Username, Content = MessageContent, CreatedAt = DateTime.Now.ToString("HH:mm:ss") });
        MessageContent = "";
    }

    [RelayCommand]
    async Task SwitchAuthMode()
    {
        IsRegisterMode = !IsRegisterMode;
        AuthButtonText = IsRegisterMode ? "Зарегистрироваться" : "Войти";
        SwitchModeText = IsRegisterMode ? "Уже есть аккаунт? Войти" : "Нет аккаунта? Пожаловаться";
        StatusMessage = "";
    }

    [RelayCommand]
    async Task OpenSettings()
    {

    }

    [RelayCommand]
    private void OpenNewChatOverlayWindow()
    {
        NewChatName = "";
        NewChatDescr = "";
        NewChatType = "chat";
        IsCreatingChatOverlay = true;
    }
    [RelayCommand]
    private void CancelCreateChat()
    {
        IsCreatingChatOverlay = false;
        NewChatName = "";
        NewChatDescr = "";
        NewChatType = "";
    }
    [RelayCommand]
    private async Task ConfirmCreateChat()
    {
        if(string.IsNullOrWhiteSpace(NewChatName) || string.IsNullOrWhiteSpace(NewChatType)) return;
        var newChat = await chatService.CreateChatAsync(NewChatName, NewChatType, NewChatDescr);
        if(newChat is not null)
        {
            await chatService.SendJoinRoomAsync(newChat.Id);
            SelectedChat = newChat;
            // await chatService.GetChatHistoryAsync(newChat.Id);
            OnSelectedChatChanged(newChat);
            Debug.WriteLine($"{newChat.ToString()}");
            Chats.Add(newChat);
            IsCreatingChatOverlay = false;
        }
    }
}
