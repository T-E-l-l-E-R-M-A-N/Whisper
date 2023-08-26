using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Whisper.Frontend.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Window_ControlBox.IsVisible = true;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;

            Window_Titlebar.PointerPressed += (s, e) =>
            {
                BeginMoveDrag(e);
            };

            Window_Titlebar.DoubleTapped += (s, e) =>
            {
                if (WindowState is not WindowState.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }
            };
            
            Window_ControlBox_CloseButton.Tapped += (_, _) =>
            {
                Close();
            };

            Window_ControlBox_ExpandButton.Tapped += (_, _) =>
            {
                PropertyChanged += (_, _) =>
                {
                    if (WindowState == WindowState.FullScreen)
                    {
                        Window_Titlebar.IsVisible = false;
                        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                    }
                    else
                    {
                        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                        Window_ControlBox.IsVisible = true;
                        Window_Titlebar.IsVisible = true;
                    }
                };
                Window_ControlBox.IsVisible = false;
                WindowState = WindowState.FullScreen;
            };

            Window_ControlBox_MinimizeButton.Tapped += (_, _) =>
            {
                WindowState = WindowState.Minimized;
            };
        }
    }
    
    
}