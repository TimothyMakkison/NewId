namespace MassTransit.Benchmarks
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;


    [SimpleJob(RuntimeMoniker.Net60)]
    [MemoryDiagnoser]
    [GcServer(true)]
    [GcForce]
    public class Benchmarks
    {
        public NewId Min = NewId.Empty;
        public Guid Guid = Guid.NewGuid();
        public NewId Max = NewId.Next();


        //[Benchmark]
        //public Guid ToGuid()
        //{
        //    return Max.ToGuid();
        //}
        //[Benchmark]
        //public Guid ToGuidUpdate()
        //{
        //    return Max.ToGuidUpdate();
        //}

        //[Benchmark]
        //public Guid ToSequentialGuid()
        //{
        //    return Max.ToSequentialGuid();
        //}

        //[Benchmark]
        //public Guid ToSequentialGuidUpdate()
        //{
        //    return Max.ToSequentialGuidUpdate();
        //}

        //[Benchmark]
        //public byte[] ToByteArray()
        //{
        //    return Max.ToByteArray();
        //}

        //[Benchmark]
        //public byte[] ToByteArrayUpdate()
        //{
        //    return Max.ToByteArrayUpdated();
        //}

        //[Benchmark]
        //public NewId FromGuid()
        //{
        //    return NewId.FromGuid(Guid);
        //}

        //[Benchmark]
        //public NewId FromGuidUpdate()
        //{
        //    return NewId.FromGuidUpdate(Guid);
        //}

        //[Benchmark]
        //public NewId FromSequentialGuid()
        //{
        //    return NewId.FromSequentialGuid(Guid);
        //}

        //[Benchmark]
        //public NewId FromSequentialGuidUpdate()
        //{
        //    return NewId.FromSequentialGuidUpdate(Guid);
        //}

        //[Benchmark]
        //public string ToStringBefore()
        //{
        //    return Max.ToString();
        //}

        //[Benchmark]
        //public string ToStringUpdate()
        //{
        //    return Max.ToStringUpdate();
        //}

        [Benchmark]
        public byte[] GetFormatterArray()
        {
            return Max.GetSequentialFormatterArray();
        }

        [Benchmark]
        public byte[] GetFormatterArrayOther()
        {
            return Max.GetSequentialFormatterArrayOther();
        }

        [Benchmark]
        public byte[] GetFormatterArrayUpdate()
        {
            return Max.GetSequentialFormatterArrayUpdate();
        }
    }
}
