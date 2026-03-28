namespace CKAM.ViewModels;
using CKAM.Services;
using CKAM.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Threading;
using System;

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
    public ObservableCollection<Message> Messages { get; } = new();
    [RelayCommand]
    async Task LoginAsync()
    {
        try
        {
            if (await chatService.LoginAsync(Username, Password))
            {
                var history = await chatService.GetHistoryAsync();
                foreach (var message in history) Messages.Add(message);
                await chatService.ConnectWebSocketAsync();
                chatService.OnMessageReceived += (message => Dispatcher.UIThread.Post(() => Messages.Add(message)));
                IsLoggedIn = true;
            }
        }
        catch(System.Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }
    [RelayCommand]
    async Task SendMessageAsync()
    {
        if(string.IsNullOrWhiteSpace(MessageContent)) return;
        await chatService.SendMessageAsync(0, MessageContent);
        MessageContent = "";
    }
}
