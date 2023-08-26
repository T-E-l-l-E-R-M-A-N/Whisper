using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Whisper.Frontend.Avalonia;

public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        if (data is null)
            return null;

        var name = data.GetType().Name!.Replace("ViewModel", "");
        var type = this.GetType().Assembly.DefinedTypes.FirstOrDefault(x => x.Name == name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = name };
    }

    public bool Match(object? data)
    {
        return data is INotifyPropertyChanged;
    }
}