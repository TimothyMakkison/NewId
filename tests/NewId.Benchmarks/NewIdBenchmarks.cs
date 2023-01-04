namespace MassTransit.Benchmarks
{
    using System;
    using System.Linq;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Environments;
    using BenchmarkDotNet.Jobs;


    public class Config :
        ManualConfig
    {
        public Config()
        {
            // Run with intrinsics disabled
            AddJob(Job.Default.WithEnvironmentVariable(new EnvironmentVariable("COMPlus_EnableSSE2", "0")).WithRuntime(CoreRuntime.Core60).AsBaseline());

            // Run with intrinsics
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core60));
        }
    }

    [Config(typeof(Config))]
    [MemoryDiagnoser(false)]
    [HideColumns(Column.Job, Column.RatioSD, Column.AllocRatio)]
    public class NewIdBenchmarks
    {
        public Guid Guid = Guid.NewGuid();
        public NewId Max = NewId.Next();
        public NewId Min = NewId.Empty;

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
        //public byte[] GetFormatterArray()
        //{
        //    return Max.GetSequentialFormatterArray();
        //}

        //[Benchmark]
        //public Guid GuidGetNewGuid()
        //{
        //    return Guid.NewGuid();
        //}

        [Benchmark]
        public Guid NextGuid()
        {
            return NewId.NextGuid();
        }

        [Benchmark]
        public Guid NextGuidBulk()
        {
            Guid g;
            for (int i = 0; i < 100_000; i++)
            {
                g = NewId.NextGuid();
            }
            return NewId.NextGuid();
        }

        [Benchmark]
        public Guid NextSequentialGuid()
        {
            return NewId.NextSequentialGuid();
        }

        [Benchmark]
        public Guid NextSequentialGuidBulk()
        {
            Guid g;
            for (int i = 0; i < 100_000; i++)
            {
                g = NewId.NextSequentialGuid();
            }
            return NewId.NextSequentialGuid();
        }

        [Benchmark]
        public Guid[] NextGuidParallel()
        {
            var threadCount = 20;

            var loopCount = 1024 * 256;

            var limit = loopCount * threadCount;

            var ids = new Guid[limit];

            ParallelEnumerable
                .Range(0, limit)
                    .WithDegreeOfParallelism(8)
                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                    .ForAll(x =>
                    {
                        ids[x] = NewId.NextGuid();
                    });
            return ids;
        }

        [Benchmark]
        public Guid[] NextSequentialGuidParallel()
        {
            var threadCount = 20;

            var loopCount = 1024 * 256;

            var limit = loopCount * threadCount;

            var ids = new Guid[limit];

            ParallelEnumerable
                .Range(0, limit)
                    .WithDegreeOfParallelism(8)
                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                    .ForAll(x =>
                    {
                        ids[x] = NewId.NextSequentialGuid();
                    });
            return ids;
        }
    }
}
