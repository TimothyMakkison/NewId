namespace MassTransit.Benchmarks
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Intrinsics;
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

        [Benchmark]
        public byte[] Convert2x256()
        {
            var lowerPadded = IntrinsicsHelper.ToCharUtf16(HexVec.GetLower());
            var upperPadded = IntrinsicsHelper.ToCharUtf16(HexVec.GetUpper());

            MemoryMarshal.Write(result, ref lowerPadded);
            MemoryMarshal.Write(result.AsSpan()[Vector256<byte>.Count..], ref upperPadded);
            return result;
        }

        [Benchmark]
        public byte[] Convert2x256Inline()
        {
            IntrinsicsHelper.ToCharUtf16(HexVec.GetLower(), result);
            IntrinsicsHelper.ToCharUtf16(HexVec.GetUpper(), result.AsSpan()[Vector256<byte>.Count..]);

            return result;
        }

        [Benchmark]
        public byte[] Unpack2x128()
        {
            IntrinsicsHelper.Vector128ToCharUtf16(HexVec.GetLower(), result);
            IntrinsicsHelper.Vector128ToCharUtf16(HexVec.GetUpper(), result.AsSpan()[32..]);

            return result;
        }

        [Benchmark]
        public byte[] UnpackBulk256()
        {
            IntrinsicsHelper.Vec256ToCharUtf16(HexVec, result);

            return result;
        }

        [Benchmark]
        public byte[] Convert128()
        {
            var lowerPadded = IntrinsicsHelper.ToCharUtf16(SmallVec);

            MemoryMarshal.Write(result, ref lowerPadded);
            return result;
        }

        [Benchmark]
        public byte[] Convert128Inline()
        {
            IntrinsicsHelper.ToCharUtf16(SmallVec, result);
            return result;
        }

        [Benchmark]
        public byte[] Unpack128()
        {
            IntrinsicsHelper.Vector128ToCharUtf16(SmallVec, result);

            return result;
        }



        private readonly ZBase32Formatter _zBase32 = new();
        [Benchmark]
        public string ToStringZBase()
        {
            return Max.ToString(_zBase32);
        }

        private readonly Base32Formatter _base32 = new();
        [Benchmark]
        public string ToStringBase32()
        {
            return Max.ToString(_base32);
        }

        private readonly DashedHexFormatter _dashedHexFormatter = new();
        [Benchmark]
        public string ToStringDashedHex()
        {
            return Max.ToString(_dashedHexFormatter);
        }

        private readonly HexFormatter _hexFormatter = new();
        [Benchmark]
        public string ToStringHex()
        {
            return Max.ToString(_hexFormatter);
        }


        private readonly Base32Formatter _customFormatter = new("YBNDRFG8EJKMCPQXOT1UWISZA345H769");
        [Benchmark]
        public string ToStringCustom()
        {
            return Max.ToString(_customFormatter);
        }



        //[Benchmark]
        //public string EncodeFields()
        //{
        //    Span<char> destination = stackalloc char[26];
        //    Span<byte> source = stackalloc byte[16];
        //    var g = Guid.Parse("F6B27C7C-8AB8-4498-AC97-3A6107A21320");
        //    MemoryMarshal.Write(source, ref g);

        //    Debug.Assert(source.Length >= 16);
        //    Debug.Assert(destination.Length >= 26);

        //    Span<byte> buffer = stackalloc byte[64];
        //    source.CopyTo(buffer[6..]);

        //    var inputVector = MemoryMarshal.Read<Vector256<byte>>(buffer);
        //    var splitVector = IntrinsicsHelper.Split130Bits5x26(inputVector);

        //    var encodedVector = EncodeCustom32(splitVector, LowerMut, UpperMut);

        //    var lower = IntrinsicsHelper.ToCharUtf16(encodedVector.GetLower());
        //    var upper = IntrinsicsHelper.ToCharUtf16(encodedVector.GetUpper());

        //    MemoryMarshal.Write(buffer, ref lower);
        //    MemoryMarshal.Write(buffer[32..], ref upper);

        //    var byteSpan = MemoryMarshal.Cast<char, byte>(destination);
        //    buffer[..52].CopyTo(byteSpan);

        //    return new string(destination);
        //}

        //[Benchmark]
        //public string EncodeFunc()
        //{
        //    _functor = () => EncodeParams(LowerMut, UpperMut);
        //    return _functor();
        //}

        //[Benchmark]
        //public string EncodePass()
        //{
        //    Span<char> destination = stackalloc char[26];
        //    Span<byte> source = stackalloc byte[16];
        //    var g = Guid.Parse("F6B27C7C-8AB8-4498-AC97-3A6107A21320");
        //    MemoryMarshal.Write(source, ref g);

        //    Debug.Assert(source.Length >= 16);
        //    Debug.Assert(destination.Length >= 26);

        //    Span<byte> buffer = stackalloc byte[64];
        //    source.CopyTo(buffer[6..]);

        //    var inputVector = MemoryMarshal.Read<Vector256<byte>>(buffer);
        //    var splitVector = IntrinsicsHelper.Split130Bits5x26(inputVector);

        //    var low = Vector256.Create((byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p');

        //    var high = Vector256.Create((byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7');

        //    var lowUp = Vector256.Create((byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O', (byte)'P', (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O', (byte)'P');

        //    var highUp = Vector256.Create((byte)'Q', (byte)'R', (byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'Q', (byte)'R', (byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7');

        //    var encodedVector = Case ? EncodeCustom32(splitVector, lowUp, highUp) : EncodeCustom32(splitVector, low, high);

        //    var lower = IntrinsicsHelper.ToCharUtf16(encodedVector.GetLower());
        //    var upper = IntrinsicsHelper.ToCharUtf16(encodedVector.GetUpper());

        //    MemoryMarshal.Write(buffer, ref lower);
        //    MemoryMarshal.Write(buffer[32..], ref upper);

        //    var byteSpan = MemoryMarshal.Cast<char, byte>(destination);
        //    buffer[..52].CopyTo(byteSpan);

        //    return new string(destination);
        //}

        //[Benchmark]
        //public string EncodeBranch()
        //{
        //    Span<char> destination = stackalloc char[26];
        //    Span<byte> source = stackalloc byte[16];
        //    var g = Guid.Parse("F6B27C7C-8AB8-4498-AC97-3A6107A21320");
        //    MemoryMarshal.Write(source, ref g);

        //    Debug.Assert(source.Length >= 16);
        //    Debug.Assert(destination.Length >= 26);

        //    Span<byte> buffer = stackalloc byte[64];
        //    source.CopyTo(buffer[6..]);

        //    var inputVector = MemoryMarshal.Read<Vector256<byte>>(buffer);
        //    var splitVector = IntrinsicsHelper.Split130Bits5x26(inputVector);

        //    var encodedVector = Case ? EncodeBaseUpper32(splitVector) : EncodeBaseLower32(splitVector);

        //    var lower = IntrinsicsHelper.ToCharUtf16(encodedVector.GetLower());
        //    var upper = IntrinsicsHelper.ToCharUtf16(encodedVector.GetUpper());

        //    MemoryMarshal.Write(buffer, ref lower);
        //    MemoryMarshal.Write(buffer[32..], ref upper);

        //    var byteSpan = MemoryMarshal.Cast<char, byte>(destination);
        //    buffer[..52].CopyTo(byteSpan);

        //    return new string(destination);
        //}

        //[Benchmark]
        //public string EncodeFixed()
        //{
        //    Span<char> destination = stackalloc char[26];
        //    Span<byte> source = stackalloc byte[16];
        //    var g = Guid.Parse("F6B27C7C-8AB8-4498-AC97-3A6107A21320");
        //    MemoryMarshal.Write(source, ref g);

        //    Debug.Assert(source.Length >= 16);
        //    Debug.Assert(destination.Length >= 26);

        //    Span<byte> buffer = stackalloc byte[64];
        //    source.CopyTo(buffer[6..]);

        //    var inputVector = MemoryMarshal.Read<Vector256<byte>>(buffer);
        //    var splitVector = IntrinsicsHelper.Split130Bits5x26(inputVector);

        //    var low = Vector256.Create((byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p');

        //    var high = Vector256.Create((byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7');

        //    var mask16 = Vector256.Create((sbyte)0x10);
        //    var x1 = Avx2.Shuffle(low, splitVector);
        //    var x2 = Avx2.Shuffle(high, splitVector);
        //    var x3 = Avx2.CompareGreaterThan(mask16, splitVector.AsSByte()).AsByte();

        //    var encodedVector = Avx2.BlendVariable(x2, x1, x3);

        //    var lower = IntrinsicsHelper.ToCharUtf16(encodedVector.GetLower());
        //    var upper = IntrinsicsHelper.ToCharUtf16(encodedVector.GetUpper());

        //    MemoryMarshal.Write(buffer, ref lower);
        //    MemoryMarshal.Write(buffer[32..], ref upper);

        //    var byteSpan = MemoryMarshal.Cast<char, byte>(destination);
        //    buffer[..52].CopyTo(byteSpan);

        //    return new string(destination);
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static Vector256<byte> EncodeBaseUpper32(Vector256<byte> x)
        //{
        //    var mask16 = Vector256.Create((sbyte)0x10);

        //    var low = Vector256.Create((byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O', (byte)'P', (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O', (byte)'P');

        //    var high = Vector256.Create((byte)'Q', (byte)'R', (byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'Q', (byte)'R', (byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7');

        //    var x1 = Avx2.Shuffle(low, x);
        //    var x2 = Avx2.Shuffle(high, x);
        //    var x3 = Avx2.CompareGreaterThan(mask16, x.AsSByte()).AsByte();

        //    return Avx2.BlendVariable(x2, x1, x3);
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static Vector256<byte> EncodeBaseLower32(Vector256<byte> x)
        //{
        //    var mask16 = Vector256.Create((sbyte)0x10);

        //    var low = Vector256.Create((byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p');

        //    var high = Vector256.Create((byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7');

        //    var x1 = Avx2.Shuffle(low, x);
        //    var x2 = Avx2.Shuffle(high, x);
        //    var x3 = Avx2.CompareGreaterThan(mask16, x.AsSByte()).AsByte();

        //    return Avx2.BlendVariable(x2, x1, x3);
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
