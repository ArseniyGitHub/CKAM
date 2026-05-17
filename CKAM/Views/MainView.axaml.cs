using Avalonia.Controls;
using Avalonia.Input;
using System.Linq;

namespace CKAM.Views;

using CKAM.ViewModels;
using System.Diagnostics;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        // Check if we can accept the data
        if (e.DataTransfer.Formats.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Formats.Contains(DataFormat.File))
        {
            var files = e.DataTransfer.TryGetFiles();
            if (files != null)
            {
                var vm = DataContext as MainViewModel;
                if (vm == null) return;
                foreach (var file in files)
                    vm.AddAttachment(file.Path.LocalPath);
            }
        }
    }
}
