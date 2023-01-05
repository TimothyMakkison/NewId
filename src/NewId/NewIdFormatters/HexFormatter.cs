#if NET6_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace MassTransit.NewIdFormatters
{
    public class HexFormatter :
        INewIdFormatter
    {
        readonly int _alpha;

        public HexFormatter(bool upperCase = false)
        {
            _alpha = upperCase ? 'A' : 'a';
        }

        public string Format(in byte[] bytes)
        {
#if NET6_0_OR_GREATER
            if (Avx2.IsSupported && BitConverter.IsLittleEndian)
            {
                return string.Create(32, (bytes, _alpha == 'A'), (span, state) =>
                {
                    var (bytes, isUpper) = state;

                    var inputVec = MemoryMarshal.Read<Vector128<byte>>(bytes);
                    var hexVec = IntrinsicsHelper.EncodeBytesHex(inputVec, isUpper);

                    var byteSpan = MemoryMarshal.Cast<char, byte>(span);
                    IntrinsicsHelper.Vec256ToCharUtf16(hexVec, byteSpan);
                });
            }
#endif
            var result = new char[32];

            var offset = 0;
            for (var i = 0; i < 16; i++)
            {
                var value = bytes[i];
                result[offset++] = HexToChar(value >> 4, _alpha);
                result[offset++] = HexToChar(value, _alpha);
            }

            return new string(result, 0, 32);
        }

        private static char HexToChar(int value, int alpha)
        {
            value &= 0xf;
            return (char)(value > 9 ? value - 10 + alpha : value + 0x30);
        }
    }
}
