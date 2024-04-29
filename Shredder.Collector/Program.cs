using Cocona;
using CsvHelper;
using Microsoft.Diagnostics.Runtime;
using System.Diagnostics;
using System.Globalization;

CoconaApp coconaApp = CoconaApp.Create();
coconaApp.AddCommands<ShredderCommand>();
coconaApp.Run();

