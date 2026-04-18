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

    public ObservableCollection<Message> Messages { get; } = new();
    public ObservableCollection<Chat> Chats { get; } = new();
    async Task LoginAsync()
    {
        try
        {
            if (await chatService.LoginAsync(Username, Password))
            {
                var history = await chatService.GetHistoryAsync();
                Dispatcher.UIThread.Post(() =>
                {
                    Messages.Clear();
                    foreach (var message in history) Messages.Add(message);
                });
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
        await chatService.SendMessageAsync(MessageContent);
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
    async Task CreateChat()
    {

    }
    [RelayCommand]
    async Task AddFile()
    {

    }
}
