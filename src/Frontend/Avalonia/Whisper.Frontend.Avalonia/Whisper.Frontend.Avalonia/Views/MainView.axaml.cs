using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;

namespace Whisper.Frontend.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            app_logo.IsVisible = false;
    }
}