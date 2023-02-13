internal class NestedObject
{
    public string Name { get; }
    public AnotherObject AnotherObject { get; }
    public double NotInt { get; }

    public NestedObject()
    {
        AnotherObject = new AnotherObject();
        Name  = Guid.NewGuid().ToString();
        NotInt = 12;
    }
}