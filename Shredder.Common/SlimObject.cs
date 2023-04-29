namespace Shredder.Common;

public struct SlimObject
{
    public int TypeId { get; set; }
    public ulong Address { get; set; }
    public ulong Size { get; set; }
    public List<ulong>? References { get; set; }
}

public struct Obj
{
    public ulong Source { get; set; }
    public ulong Target { get; set; }
}