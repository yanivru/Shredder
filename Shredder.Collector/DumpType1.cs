class DumpType
{
    public string Name { get; }
    public int Count { get; }
    public decimal Size { get; }

    public DumpType(string typeName, int count, decimal size)
    {
        Name = typeName;
        Count = count;
        Size = size;
    }
}
