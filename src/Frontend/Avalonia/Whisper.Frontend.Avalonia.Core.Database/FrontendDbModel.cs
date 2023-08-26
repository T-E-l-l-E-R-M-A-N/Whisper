namespace Whisper.Frontend.Avalonia.Core.Database;

public class FrontendDbModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }

    public FrontendDbModel()
    {
        Id = Guid.NewGuid().ToString();
    }
}