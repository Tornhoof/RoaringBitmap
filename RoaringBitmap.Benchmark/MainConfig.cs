using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

namespace RoaringBitmap.Benchmark
{
    public class MainConfig : ManualConfig
    {
        public MainConfig()
        {
            Add(Job.Default.With(Runtime.Clr)
                   .With(Jit.RyuJit)
                   .With(Platform.X64)
                   .WithId("NET4.7_RyuJIT-x64"));

            Add(Job.Default.With(Runtime.Core)
                   .With(CsProjCoreToolchain.NetCoreApp20)
                   .WithId("Core2.0-x64"));

            Add(Job.Default.With(Runtime.Core)
                   .With(CsProjCoreToolchain.From(NetCoreAppSettings.NetCoreApp21))
                   .WithId("Core2.1-x64"));

            Add(DefaultColumnProviders.Instance);
            Add(MarkdownExporter.GitHub);
            Add(new ConsoleLogger());
            Add(new HtmlExporter());
            Add(MemoryDiagnoser.Default);
        }
    }
}
