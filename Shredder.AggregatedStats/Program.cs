using System.Globalization;
using CsvHelper;
using QuikGraph;
using Shredder.Common;
using CsvHelper.Configuration;
using Parquet.Serialization;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;

var commandLineArgs = Environment.GetCommandLineArgs();
var filePath = "";
if (commandLineArgs.Length > 1)
{
    filePath = commandLineArgs[1];
}

Dictionary<(DumpType, DumpType), TaggedEdge<DumpType, double>> cache = new();

var dumpTypes = (await ReadDumpTypes(Path.Combine(filePath, "Types.csv"))).ToDictionary(x => x.Name);
var dumpReferences = await ReadDumpReferences(Path.Combine(filePath, "References.csv"));

var edges = dumpReferences.Select(x => new TaggedEdge<DumpType, double>(dumpTypes[x.Type], dumpTypes[x.ReferencedType], x.Count));
var graph = edges.ToAdjacencyGraph<DumpType, TaggedEdge<DumpType, double>>();

var condensateStronglyConnected = graph.CondensateStronglyConnected<DumpType, TaggedEdge<DumpType, double>, AdjacencyGraph<DumpType, TaggedEdge<DumpType, double>>>();
var topologicalSort = condensateStronglyConnected.TopologicalSort();

var reverseTopologicalSort = topologicalSort.Reverse().ToArray();

AdjacencyGraph<DumpType, TaggedEdge<DumpType, double>> outGraph = new();

outGraph.AddVertexRange(graph.Vertices);

foreach (var vertex in reverseTopologicalSort)
{
    AddChildrenAndDescendants(condensateStronglyConnected, vertex, outGraph);
}

var descendants = outGraph.Edges.Select(x => new TypeDescendent(x.Source, x.Target, x.Tag, x.Target.Size * (decimal)x.Tag / x.Target.Count));
WriteDescendantsCsv(descendants);
Console.WriteLine("finished");
Console.ReadLine();

//Console.WriteLine(topologicalSort.Count());

//WriteDescendantsCsv(typeDescendants);

void AddChildrenAndDescendants(IMutableBidirectionalGraph<AdjacencyGraph<DumpType, TaggedEdge<DumpType, double>>, CondensedEdge<DumpType, TaggedEdge<DumpType, double>, AdjacencyGraph<DumpType, TaggedEdge<DumpType, double>>>> condensedGraph,
    AdjacencyGraph<DumpType, TaggedEdge<DumpType, double>> currentVertex,
    AdjacencyGraph<DumpType, TaggedEdge<DumpType, double>> outGraph1)
{
    var outEdges = condensateStronglyConnected.OutEdges(currentVertex);
    foreach (var outEdge in outEdges)
    {
        AddEdgesTargetsToAllSources(currentVertex.Vertices, outEdge.Target.Vertices, outGraph1, outEdge.Edges.Sum(x => x.Tag));

        foreach (var targetVertex in outEdge.Target.Vertices)
        {
            var childRatio = outEdge.Edges.Sum(x => x.Tag) / outEdge.Target.Vertices.Sum(x => x.Count);
            var taggedEdges = outGraph1.OutEdges(targetVertex);
            var descendants = taggedEdges.Select(x => x.Target)
                .Except(outEdge.Target.Vertices).ToArray();
            AddEdgesTargetsToAllSources(currentVertex.Vertices, descendants, outGraph1, childRatio);
        }
    }
}

void AddEdgesTargetsToAllSources(IEnumerable<DumpType> sourceVertices,
    IEnumerable<DumpType> targets,
    AdjacencyGraph<DumpType, TaggedEdge<DumpType, double>> outGraph2,
    double tagNormalization)
{
    foreach (var source in sourceVertices)
    {
        foreach (var target in targets)
        {
            var newEdge = new TaggedEdge<DumpType, double>(source, target, tagNormalization);
            
            if (cache.TryGetValue((source, target), out var existingEdge))
            {
                existingEdge.Tag += tagNormalization;
            }
            else
            {
                cache.Add((source, target), newEdge);
                outGraph2.AddEdge(newEdge);
            }
        }
    }
}


async Task<IList<SlimObject>> ReadMemoryGraph()
{
    await using (var reader = File.OpenRead(Path.Combine(filePath, "1.parquet")))
    {
        return await ParquetSerializer.DeserializeAsync<SlimObject>(reader);
    }
}

Console.WriteLine("Done");

void WriteDescendantsCsv(IEnumerable<TypeDescendent> typeDescendents)
{
    using (var stream = new StreamWriter("type-descendants.csv"))
    {
        using (var writer = new CsvWriter(stream, CultureInfo.InvariantCulture))
        {
            writer.WriteRecords(typeDescendents);
        }
    }
}

async ValueTask<DumpReference[]> ReadDumpReferences(string filePath)
{
    using (var stream = new StreamReader(filePath))
    {
        using (var csvReader = new CsvReader(stream, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            return await csvReader.GetRecordsAsync<DumpReference>().ToArrayAsync();
        }
    }
}


async ValueTask<DumpType[]> ReadDumpTypes(string filePath1)
{
    using (var stream = new StreamReader(filePath1))
    {
        using (var csvReader = new CsvReader(stream, CultureInfo.InvariantCulture))
        {
            return await csvReader.GetRecordsAsync<DumpType>().ToArrayAsync();
        }
    }
}

public class TypeDescendent
{
    public DumpType Source { get; }
    public DumpType Target { get; }
    public double Count { get; }
    public decimal Size { get; }

    public TypeDescendent(DumpType source, DumpType target, double count, decimal size)
    {
        Source = source;
        Target = target;
        Count = count;
        Size = size;
    }
}