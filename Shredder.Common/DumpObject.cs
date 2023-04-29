namespace Shredder.Common;

public class DumpObject
{
    public DumpObject(string type, ulong size, IEnumerable<string> referencedTypes)
    {
        Type = type;
        Size = size;
        ReferencedTypes = referencedTypes;
    }

    public string Type { get; }
    public ulong Size { get; }
    public IEnumerable<string> ReferencedTypes { get; }
}