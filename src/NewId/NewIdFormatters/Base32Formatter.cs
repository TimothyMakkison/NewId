namespace MassTransit.NewIdFormatters
{
    using System;
    using System.Threading;
#if NET6_0_OR_GREATER
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Intrinsics;
    using System.Runtime.Intrinsics.X86;
#endif


    public class Base32Formatter :
        INewIdFormatter
    {
        const string LowerCaseChars = "abcdefghijklmnopqrstuvwxyz234567";
        const string UpperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        static readonly ThreadLocal<char[]> _formatBuffer = new ThreadLocal<char[]>(() => new char[26]);

        readonly string _chars;
        readonly bool _isCustom;
        readonly bool _isUpperCase;
#if NET6_0_OR_GREATER
        readonly Vector256<byte> _lower;
        readonly Vector256<byte> _upper;
#endif
        public Base32Formatter(bool upperCase = false)
        {
            _chars = upperCase ? UpperCaseChars : LowerCaseChars;
            _isUpperCase = upperCase;
        }

        public Base32Formatter(in string chars)
        {
            if (chars.Length != 32)
                throw new ArgumentException("The character string must be exactly 32 characters", nameof(chars));

            _chars = chars;

#if NET6_0_OR_GREATER
            if (Avx2.IsSupported && BitConverter.IsLittleEndian)
            {
                _isCustom = true;
                var bytes = MemoryMarshal.Cast<char, byte>(chars);
                var lower = MemoryMarshal.Read<Vector256<byte>>(bytes);
                var upper = MemoryMarshal.Read<Vector256<byte>>(bytes[32..]);

                _lower = IntrinsicsHelper.GetByteLutFromChar(lower);
                _upper = IntrinsicsHelper.GetByteLutFromChar(upper);
            }
#endif
        }

        public string Format(in byte[] bytes)
        {
#if NET6_0_OR_GREATER
            if (Avx2.IsSupported)
            {
                if (_isCustom)
                {
                    return string.Create(26, (bytes, _lower, _upper), (span, state) =>
                    {
                        var (bytes, lower, upper) = state;
                        EncodeCustom(bytes, span, lower, upper);
                    });
                }
                return string.Create(26, (bytes, _isUpperCase), (span, state) =>
                {
                    var (bytes, isUpperCase) = state;
                    Encode(bytes, span, isUpperCase);
                });
            }
#endif
            var result = _formatBuffer.Value;

            var offset = 0;
            for (var i = 0; i < 3; i++)
            {
                var indexed = i * 5;
                long number = (bytes[indexed] << 12) | (bytes[indexed + 1] << 4) | (bytes[indexed + 2] >> 4);
                ConvertLongToBase32(result, offset, number, 4, _chars);

                offset += 4;

                number = ((bytes[indexed + 2] & 0xf) << 16) | (bytes[indexed + 3] << 8) | bytes[indexed + 4];
                ConvertLongToBase32(result, offset, number, 4, _chars);

                offset += 4;
            }

            ConvertLongToBase32(result, offset, bytes[15], 2, _chars);

            return new string(result, 0, 26);
        }

        static void ConvertLongToBase32(in char[] buffer, int offset, long value, int count, string chars)
        {
            for (var i = count - 1; i >= 0; i--)
            {
                //30, 26, 25, 7, 24, 31, 4, 10, 23, 1, 2, 9, 17, 11, 4, 23, 7, 9, 16, 16, 15, 8, 16, 19, 2, 4,
                var index = (int)(value % 32);
                buffer[offset + i] = chars[index];
                value /= 32;
            }
        }

#if NET6_0_OR_GREATER
        private static void Encode(ReadOnlySpan<byte> span, Span<char> output, bool isUpperCase)
        {
            Debug.Assert(span.Length >= 16);
            Debug.Assert(output.Length >= 26);

            Span<byte> buffer = stackalloc byte[64];
            span.CopyTo(buffer[6..]);

            var inputVector = MemoryMarshal.Read<Vector256<byte>>(buffer);
            var splitVector = Split130Bits5x26(inputVector);
            var encodedVector = isUpperCase ? EncodeBaseUpper32(splitVector) : EncodeBaseLower32(splitVector);


            var lower = IntrinsicsHelper.ToCharUtf16(encodedVector.GetLower());
            var upper = IntrinsicsHelper.ToCharUtf16(encodedVector.GetUpper());

            MemoryMarshal.Write(buffer, ref lower);
            MemoryMarshal.Write(buffer[32..], ref upper);

            var byteSpan = MemoryMarshal.Cast<char, byte>(output);
            buffer[..52].CopyTo(byteSpan);
        }

        private static void EncodeCustom(ReadOnlySpan<byte> span, Span<char> output, Vector256<byte> lowLut, Vector256<byte> upperLut)
        {
            Debug.Assert(span.Length >= 16);
            Debug.Assert(output.Length >= 26);

            Span<byte> buffer = stackalloc byte[64];
            span.CopyTo(buffer[6..]);

            var inputVector = MemoryMarshal.Read<Vector256<byte>>(buffer);
            var splitVector = Split130Bits5x26(inputVector);
            var encodedVector = EncodeCustom32(splitVector, lowLut, upperLut);

            var lower = IntrinsicsHelper.ToCharUtf16(encodedVector.GetLower());
            var upper = IntrinsicsHelper.ToCharUtf16(encodedVector.GetUpper());

            MemoryMarshal.Write(buffer, ref lower);
            MemoryMarshal.Write(buffer[32..], ref upper);

            var byteSpan = MemoryMarshal.Cast<char, byte>(output);
            buffer[..52].CopyTo(byteSpan);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<byte> Split130Bits5x26(Vector256<byte> input)
        {
            var splitShuffle = Vector256.Create((byte)
                0x07, 0x06, 0x08, 0x07, 0x09, 0x08, 0x0A, 0x09,
                0x0C, 0x0B, 0x0D, 0x0C, 0x0E, 0x0D, 0x0F, 0x0E,
                0x01, 0x00, 0x02, 0x01, 0x03, 0x02, 0x04, 0x03,
                0x05, 0x06,
                0x07, 0x06, 0x08, 0x07, 0x09, 0x08);
 
            var splitM1 = Vector256.Create((ulong)0x0800_0200_0080_0020, 0x0800_0200_0080_0020, 0x0800_0200_0080_0020, 0x0800).AsUInt16();
            var splitM2 = Vector256.Create((ulong)0x0100_0040_0010_0004, 0x0100_0040_0010_0004, 0x0100_0040_0010_0004, 0x0100).AsInt16();

            var maskM1 = Vector256.Create((ushort)0x00_1F);
            var maskM2 = Vector256.Create((ushort)0x1F_00);

            var x1 = Avx2.Shuffle(input, splitShuffle).AsUInt16();
            var x2 = Avx2.MultiplyHigh(x1, splitM1);
            var x3 = Avx2.MultiplyLow(x1.AsInt16(), splitM2).AsUInt16();
            var x4 = Avx2.And(x2, maskM1);
            var x5 = Avx2.And(x3, maskM2);

            var x6 = Avx2.Or(x4, x5).AsByte();
            return x6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<byte> EncodeBaseUpper32(Vector256<byte> x)
        {
            var mask16 = Vector256.Create((sbyte)0x10);

            var low = Vector256.Create((byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O', (byte)'P', (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O', (byte)'P');

            var high = Vector256.Create((byte)'Q', (byte)'R', (byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'Q', (byte)'R', (byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7');

            var x1 = Avx2.Shuffle(low, x);
            var x2 = Avx2.Shuffle(high, x);
            var x3 = Avx2.CompareGreaterThan(mask16, x.AsSByte()).AsByte();

            return Avx2.BlendVariable(x2, x1, x3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<byte> EncodeBaseLower32(Vector256<byte> x)
        {
            var mask16 = Vector256.Create((sbyte)0x10);

            var low = Vector256.Create((byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p');

            var high = Vector256.Create((byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7');

            var x1 = Avx2.Shuffle(low, x);
            var x2 = Avx2.Shuffle(high, x);
            var x3 = Avx2.CompareGreaterThan(mask16, x.AsSByte()).AsByte();

            return Avx2.BlendVariable(x2, x1, x3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<byte> EncodeCustom32(Vector256<byte> x, Vector256<byte> lower, Vector256<byte> upper)
        {
            var mask16 = Vector256.Create((sbyte)0x10);
            var x1 = Avx2.Shuffle(lower, x);
            var x2 = Avx2.Shuffle(upper, x);
            var x3 = Avx2.CompareGreaterThan(mask16, x.AsSByte()).AsByte();

            return Avx2.BlendVariable(x2, x1, x3);
        }
#endif
    }
}
