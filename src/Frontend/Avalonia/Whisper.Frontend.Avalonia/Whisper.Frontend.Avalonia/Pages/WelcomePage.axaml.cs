using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Whisper.Frontend.Avalonia.Views;

public partial class WelcomePage : UserControl
{
    public WelcomePage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}