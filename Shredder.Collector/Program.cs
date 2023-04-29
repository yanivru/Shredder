using CsvHelper;
using Microsoft.Diagnostics.Runtime;
using System.Diagnostics;
using System.Globalization;
using Shredder.Common;

Console.WriteLine("Starting memory dump essence extraction...");

var processingStopwatch = Stopwatch.StartNew();

var commandLineArgs = Environment.GetCommandLineArgs();
if (commandLineArgs.Length == 1)
{
    Console.WriteLine("Error: Dump path not specified.");
    return;
}

using (var dataTarget = DataTarget.LoadDump(commandLineArgs[1]))
{
    var clrRuntime = dataTarget.ClrVersions[0].CreateRuntime();
    IEnumerable<ClrObject> objects = clrRuntime.Heap.EnumerateObjects();

    var objectsPerType = objects.ToLookup(x => x.Type?.Name ?? "", x => new DumpObject(x.Type?.Name ?? "", x.Size, GetRefencedTypesNames(x).ToArray()));

    WriteGeneralInfoCsv(clrRuntime);

    WriteTypesCsv(objectsPerType);

    WriteReferencesCsv(objectsPerType);
}

void WriteGeneralInfoCsv(ClrRuntime clrRuntime)
{
    Dictionary<string, string> generalInfo = new();

    generalInfo["CanWalkHeap"] = clrRuntime.Heap.CanWalkHeap.ToString();
    generalInfo["IsServer"] = clrRuntime.Heap.IsServer.ToString();
    generalInfo["ClrInfo.Version"] = clrRuntime.ClrInfo.Version.ToString();
    generalInfo["ClrInfo.Version"] = clrRuntime.ClrInfo.Version.ToString();

    using (var writer = new StreamWriter("GeneralInfo.csv"))
    {
        using (CsvWriter csvWriter = new(writer, CultureInfo.InvariantCulture))
        {
            csvWriter.WriteRecords(generalInfo);
        }
    }
}

void WriteReferencesCsv(ILookup<string, DumpObject> objectsPerType)
{
    var referencesStatistics = objectsPerType.SelectMany(objectsByType => objectsByType
                                                                            .SelectMany(x => x.ReferencedTypes)
                                                                            .GroupBy(referenceType => referenceType)
                                                                            .Select(z => new DumpReference(objectsByType.Key, z.Key, z.Count())));

    using (var writer = new StreamWriter("References.csv"))
    {
        using (CsvWriter csvWriter = new(writer, CultureInfo.InvariantCulture))
        {
            csvWriter.WriteRecords(referencesStatistics);
        }
    }
}

static IEnumerable<string> GetRefencedTypesNames(ClrObject x)
{
    return x.EnumerateReferences().Select(y => y.Type?.Name ?? "");
}

static void WriteTypesCsv(ILookup<string, DumpObject> objectsPerType)
{
    var typesStatistics = objectsPerType.Select(x => new DumpType(x.Key, x.Count(), x.Sum(y => (decimal)y.Size)));

    using (var writer = new StreamWriter("Types.csv"))
    {
        using (CsvWriter csvWriter = new(writer, CultureInfo.InvariantCulture))
        {
            csvWriter.WriteRecords(typesStatistics);
        }
    }
}