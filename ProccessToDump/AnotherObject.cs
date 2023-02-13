internal class AnotherObject
{
    public string AnotherString { get; }
    public int Index { get; }
    public AnotherObject()
    {
        AnotherString = Guid.NewGuid().ToString();
        Index = new Random().Next();
    }
}