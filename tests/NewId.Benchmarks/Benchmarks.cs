namespace MassTransit.Benchmarks
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Environments;
    using BenchmarkDotNet.Jobs;
    using MassTransit.NewIdProviders;

    public class Config : ManualConfig
    {
        public Config()
        {
            // Run with intrinsics disabled
            AddJob(
                Job.Default
                .WithEnvironmentVariable(new EnvironmentVariable("COMPlus_EnableSSE2", "0"))
                .WithRuntime(CoreRuntime.Core60)
                .AsBaseline());

            // Run with intrinsics
            AddJob(
               Job.Default.WithRuntime(CoreRuntime.Core60));
        }
    }

    [Config(typeof(Config))]
    [MemoryDiagnoser(false)]
    [HideColumns(Column.Job, Column.RatioSD, Column.AllocRatio)]
    public class Benchmarks
    {
        public NewId Min = NewId.Empty;
        public Guid Guid = Guid.NewGuid();
        public NewId Max = NewId.Next();

        [Benchmark]
        public NewIdGenerator CreateIdGenerator()
        {
            return new NewIdGenerator(new DateTimeTickProvider(), new BestPossibleWorkerIdProvider());
        }

        //[Benchmark]
        //public Guid ToGuid()
        //{
        //    return Max.ToGuid();
        //}

        //[Benchmark]
        //public Guid ToSequentialGuid()
        //{
        //    return Max.ToSequentialGuid();
        //}

        //[Benchmark]
        //public byte[] ToByteArray()
        //{
        //    return Max.ToByteArray();
        //}

        //[Benchmark]
        //public NewId FromGuid()
        //{
        //    return NewId.FromGuid(Guid);
        //}

        //[Benchmark]
        //public NewId FromSequentialGuid()
        //{
        //    return NewId.FromSequentialGuid(Guid);
        //}

        //[Benchmark]
        //public string ToString()
        //{
        //    return Max.ToString();
        //}

        //[Benchmark]
        //public string ToStringHex()
        //{
        //    return Max.ToString("N");
        //}

        //private readonly ZBase32Formatter _zBase32 = new ZBase32Formatter();
        //[Benchmark]
        //public string ToStringZBase()
        //{
        //    return Max.ToString(_zBase32);
        //}

        //private readonly Base32Formatter _base32 = new Base32Formatter();
        //[Benchmark]
        //public string ToStringBase32()
        //{
        //    return Max.ToString(_base32);
        //}

        //[Benchmark]
        //public ZBase32Formatter CreateZBase32()
        //{
        //    return new ZBase32Formatter();
        //}

        //[Benchmark]
        //public string ToStringBrackets()
        //{
        //    return Max.ToString("");
        //}

        //[Benchmark]
        //public byte[] GetFormatterArray()
        //{
        //    return Max.GetSequentialFormatterArray();
        //}

        //[Benchmark]
        //public Guid NextGuid()
        //{
        //    return NewId.NextGuid();
        //}

        //[Benchmark]
        //public Guid NextGuidBulk()
        //{
        //    Guid g;
        //    for (int i = 0; i < 100_000; i++)
        //    {
        //        g = NewId.NextGuid();
        //    }
        //    return NewId.NextGuid();
        //}

        //[Benchmark]
        //public Guid NextSequentialGuid()
        //{
        //    return NewId.NextSequentialGuid();
        //}

        //[Benchmark]
        //public Guid NextSequentialGuidBulk()
        //{
        //    Guid g;
        //    for (int i = 0; i < 100_000; i++)
        //    {
        //        g = NewId.NextSequentialGuid();
        //    }
        //    return NewId.NextSequentialGuid();
        //}
    }
}
