using CKAM.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CKAM.ViewModels
{
    public partial class AttachmentItemViewModel : ViewModelBase
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
    public partial class MessageViewModel : ViewModelBase
    {
        

        [ObservableProperty]
        private string content = string.Empty;
        [ObservableProperty]
        private long id;
        [ObservableProperty]
        private string senderName = string.Empty;
        [ObservableProperty]
        private string createdAt = string.Empty;
        [ObservableProperty]
        private long chatId;
        [ObservableProperty]
        private bool isSending;
        [ObservableProperty]
        private bool isError;
        [ObservableProperty]
        private double uploadProgress;
        [ObservableProperty]
        private bool isForarded;
        [ObservableProperty]
        private string forwardedFrom;
        public ObservableCollection<AttachmentItemViewModel> Attachments { get; } = new();
        public MessageViewModel(Message message, long userId)
        {
            Id = message.Id;
            SenderName = message.SenderName;
            CreatedAt = message.CreatedAt;
            Content = message.Content;
            
        }
        public MessageViewModel() { }
    }
}
