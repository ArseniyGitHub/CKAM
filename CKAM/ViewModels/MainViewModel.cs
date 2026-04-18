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
            foreach (var message in history) Messages.Add(message);
        });
    }

    public ObservableCollection<Message> Messages { get; } = new();
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
                chatService.OnMessageReceived += (message => Dispatcher.UIThread.Post(() => Messages.Add(message)));
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
        await chatService.SendMessageAsync(MessageContent, SelectedChat.Id);
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
    private void CreateChat() => IsCreatingChatOverlay = true;
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
            Chats.Add(newChat);
            SelectedChat = newChat;
            await chatService.GetChatHistoryAsync(newChat.Id);
        }
    }
    [RelayCommand]
    async Task AddFile()
    {

    }
}
