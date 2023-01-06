namespace MassTransit.Benchmarks
{
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Intrinsics;
    using System.Text;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Environments;
    using BenchmarkDotNet.Jobs;
    using MassTransit.NewIdFormatters;

    public class Config : ManualConfig
    {
        public Config()
        {
            //Run with intrinsics disabled
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

        private Vector256<byte> HexVec;
        private Vector128<byte> SmallVec;
        private byte[] result = new byte[64];
        public Benchmarks()
        {
            var bytes = new byte[32];
            Random rand = new Random(0);
            rand.NextBytes(bytes);
            var hexVec = MemoryMarshal.Read<Vector256<byte>>(bytes);
        }


        //private readonly ZBase32Formatter _zBase32 = new();
        //[Benchmark]
        //public string ToStringZBase()
        //{
        //    return Max.ToString(_zBase32);
        //}

        //private readonly Base32Formatter _base32 = new();
        //[Benchmark]
        //public string ToStringBase32()
        //{
        //    return Max.ToString(_base32);
        //}

        public class Bit : INewIdFormatter
        {
            public string Format(in byte[] bytes)
            {
                return Convert.ToHexString(bytes);
            }
        }

        //private readonly DashedHexFormatter _dashedHexFormatter = new();
        //[Benchmark]
        //public string ToStringDashedHex()
        //{
        //    return Max.ToString(_dashedHexFormatter);
        //}

        //private readonly DashedHexFormatter _customDashFormatter = new('{','}');
        //[Benchmark]
        //public string ToStringDashCustom()
        //{
        //    return Max.ToString(_customDashFormatter);
        //}

        private readonly Bit _bitFormatter = new();
        [Benchmark]
        public string ToStringBitConvert()
        {
            return Max.ToString(_bitFormatter);
        }


        private readonly HexFormatter _hexFormatter = new();
        [Benchmark]
        public string ToStringHex()
        {
            return Max.ToString(_hexFormatter);
        }

        //private readonly Base32Formatter _customFormatter = new("YBNDRFG8EJKMCPQXOT1UWISZA345H769");
        //[Benchmark]
        //public string ToStringCustom()
        //{
        //    return Max.ToString(_customFormatter);
        //}






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
