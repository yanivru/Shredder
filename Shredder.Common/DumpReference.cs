namespace Shredder.Common;

public class DumpReference
{
    public string Type { get; }
    public string ReferencedType { get; }
    public int Count { get; }

    public DumpReference(string Type, string ReferencedType, int Count)
    {
        this.Type = Type;
        this.ReferencedType = ReferencedType;
        this.Count = Count;
    }
}