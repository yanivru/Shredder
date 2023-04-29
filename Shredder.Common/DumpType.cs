namespace Shredder.Common;

public class DumpType
{
    public string Name { get; }
    public int Count { get; }
    public decimal Size { get; }

    public DumpType(string Name, int Count, decimal Size)
    {
        this.Name = Name;
        this.Count = Count;
        this.Size = Size;
    }
}