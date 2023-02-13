Console.WriteLine("Starting to create objects...");

var nestedObjects = Enumerable.Range(1, 1000).Select(x => new NestedObject()).ToList();

Console.WriteLine("Objects created. Take dump now.");
Console.ReadLine();
Console.WriteLine(nestedObjects.Count);