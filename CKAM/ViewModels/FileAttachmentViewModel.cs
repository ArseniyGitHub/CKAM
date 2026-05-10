using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CKAM.ViewModels
{
    public partial class FileAttachmentViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string path;
        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private long size;

        public string Id { get; set; }
        [ObservableProperty]
        private bool isUploaded = false;
    }
}
