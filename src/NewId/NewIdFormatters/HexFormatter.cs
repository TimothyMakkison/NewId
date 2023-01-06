using System.Diagnostics;
using System.Runtime.CompilerServices;
#if NET6_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif


namespace MassTransit.NewIdFormatters
{
    public class HexFormatter :
        INewIdFormatter
    {
        readonly uint _alpha;

        public HexFormatter(bool upperCase = false)
        {
            _alpha = upperCase ? 0 : 0x2020U;
        }

        public string Format(in byte[] bytes)
        {
            Debug.Assert(bytes.Length == 16);

#if NET6_0_OR_GREATER
            if (Avx2.IsSupported && BitConverter.IsLittleEndian)
            {
                return string.Create(32, (bytes, _alpha == 'A'), (span, state) =>
                {
                    var (bytes, isUpper) = state;

                    var inputVec = MemoryMarshal.Read<Vector128<byte>>(bytes);
                    var hexVec = IntrinsicsHelper.EncodeBytesHex(inputVec, isUpper);

                    var byteSpan = MemoryMarshal.Cast<char, byte>(span);
                    IntrinsicsHelper.Vector256ToCharUtf16(hexVec, byteSpan);
                });
            }
#endif
            var result = new char[32];

            for (int pos = 0; pos < bytes.Length; pos++)
            {
                HexToChar(bytes[pos], result, pos * 2, _alpha);
            }

            return new string(result, 0, 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void HexToChar(byte value, char[] buffer, int startingIndex, uint casing)
        {
            uint difference = (((uint)value & 0xF0U) << 4) + ((uint)value & 0x0FU) - 0x8989U;
            uint packedResult = ((((uint)(-(int)difference) & 0x7070U) >> 4) + difference + 0xB9B9U) | (uint)casing;

            buffer[startingIndex + 1] = (char)(packedResult & 0xFF);
            buffer[startingIndex] = (char)(packedResult >> 8);
        }
    }
}
