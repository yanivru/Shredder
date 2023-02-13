internal class DumpReference
{
    public string Type { get; }
    public string ReferencedType { get; }
    public int Count { get; }

    public DumpReference(string type, string referencedType, int count)
    {
        Type = type;
        ReferencedType = referencedType;
        Count = count;
    }
}