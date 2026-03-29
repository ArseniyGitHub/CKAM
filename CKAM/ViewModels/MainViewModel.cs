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
    public ObservableCollection<Message> Messages { get; } = new();
    [RelayCommand]
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
    async Task SendMessageAsync()
    {
        if(string.IsNullOrWhiteSpace(MessageContent)) return;
        await chatService.SendMessageAsync(MessageContent);
        // Messages.Add(new Message { SenderName = Username, Content = MessageContent, CreatedAt = DateTime.Now.ToString("HH:mm:ss") });
        MessageContent = "";
    }
}
