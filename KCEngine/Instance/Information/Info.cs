namespace KCEngine.Instance.Information;

public class Info : Instance
{
    public string Description { get; set; } = string.Empty;

    public Info(string name, Instance parent) : base(name, parent)
    { }
}
