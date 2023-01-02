#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System;
#endif

using System;

namespace MassTransit.NewIdFormatters
{
    public class DashedHexFormatter :
        INewIdFormatter
    {
        readonly int _alpha;
        readonly int _length;
        readonly char _prefix;
        readonly char _suffix;

        public DashedHexFormatter(char prefix = '\0', char suffix = '\0', bool upperCase = false)
        {
            if (prefix == '\0' || suffix == '\0')
                _length = 36;
            else
            {
                _prefix = prefix;
                _suffix = suffix;
                _length = 38;
            }

            _alpha = upperCase ? 'A' : 'a';
        }

        public string Format(in byte[] bytes)
        {
#if NET6_0_OR_GREATER
            if (Avx2.IsSupported && BitConverter.IsLittleEndian)
            {
                return string.Create(_length, (bytes, _alpha, _prefix, _suffix), (span, st) =>
                {
                    var (state, _alpha, _prefix, _suffix) = st;
                    var swizzle = Vector256.Create((byte)
                        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                        0x80, 0x08, 0x09, 0x0a, 0x0b, 0x80, 0x0c, 0x0d,
                        0x80, 0x80, 0x80, 0x00, 0x01, 0x02, 0x03, 0x80,
                        0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b);

                    var dash = Vector256.Create((byte)
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x2d, 0x00, 0x00, 0x00, 0x00, 0x2d, 0x00, 0x00,
                        0x00, 0x00, 0x2d, 0x00, 0x00, 0x00, 0x00, 0x2d,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);


                    var a = IntrinsicsHelper.EncodeBytesHex(Unsafe.As<byte, Vector128<byte>>(ref MemoryMarshal.GetArrayDataReference<byte>(state)), _alpha == 'A');

                    var a1 = Avx2.Shuffle(a, swizzle);
                    var a2 = Avx2.Or(a1, dash);

                    //var padMask = Vector256.Create((byte)0, 0xFF, 1, 0xFF, 2, 0xFF, 3, 0xFF, 4, 0xFF, 5, 0xFF, 6, 0xFF, 7, 0xFF, 8, 0xFF, 9, 0xFF, 10, 0xFF, 11, 0xFF, 12, 0xFF, 13, 0xFF, 14, 0xFF, 15, 0xFF);

                    //var lower = Avx2.Permute2x128(a2, a2, 0b10_00_00);
                    //var upper = Avx2.Permute2x128(a2, a2, 0b11_00_01);

                    //var lowerPadded = Avx2.Shuffle(lower, padMask);
                    //var upperPadded = Avx2.Shuffle(upper, padMask);

                    var lowerPadded = IntrinsicsHelper.ToCharUtf16(a2.GetLower());
                    var upperPadded = IntrinsicsHelper.ToCharUtf16(a2.GetUpper());

                    var charSpan = span.Length == 38 ? span[1..^1] : span;
                    var spanBytes = MemoryMarshal.Cast<char, byte>(charSpan);
                    MemoryMarshal.TryWrite(spanBytes, ref lowerPadded);
                    MemoryMarshal.TryWrite(spanBytes[32..], ref upperPadded);

                    charSpan[16] = HexToChar(state[7] >> 4, _alpha);
                    charSpan[17] = HexToChar(state[7], _alpha);

                    charSpan[32] = HexToChar(state[14] >> 4, _alpha);
                    charSpan[33] = HexToChar(state[14], _alpha);
                    charSpan[34] = HexToChar(state[15] >> 4, _alpha);
                    charSpan[35] = HexToChar(state[15], _alpha);

                    if (span.Length == 38)
                    {
                        span[0] = _prefix;
                        span[^1] = _suffix;
                    }
                });
            }

            return string.Create(_length, (bytes, _alpha, _prefix, _suffix), (span, st) =>
            {
                var (bytes, _alpha, _prefix, _suffix) = st;

                var result = new char[_length];

                var i = 0;
                var offset = 0;
                if (_prefix != '\0')
                    result[offset++] = _prefix;

                if(result.Length == 38)
                {
                    result[36] = HexToChar(bytes[15], _alpha);
                }
                else
                {
                    result[35] = HexToChar(bytes[15], _alpha);
                }

                result[offset++] = HexToChar(bytes[0] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[0], _alpha);
                result[offset++] = HexToChar(bytes[1] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[1], _alpha);
                result[offset++] = HexToChar(bytes[2] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[2], _alpha);
                result[offset++] = HexToChar(bytes[3] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[3], _alpha);

                result[offset++] = '-';

                result[offset++] = HexToChar(bytes[4] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[4], _alpha);
                result[offset++] = HexToChar(bytes[5] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[5], _alpha);

                result[offset++] = '-';

                result[offset++] = HexToChar(bytes[6] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[6], _alpha);
                result[offset++] = HexToChar(bytes[7] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[7], _alpha);

                result[offset++] = '-';

                result[offset++] = HexToChar(bytes[8] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[8], _alpha);
                result[offset++] = HexToChar(bytes[9] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[9], _alpha);

                result[offset++] = '-';

                result[offset++] = HexToChar(bytes[10] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[10], _alpha);
                result[offset++] = HexToChar(bytes[11] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[11], _alpha);
                result[offset++] = HexToChar(bytes[12] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[12], _alpha);
                result[offset++] = HexToChar(bytes[13] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[13], _alpha);
                result[offset++] = HexToChar(bytes[14] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[14], _alpha);
                result[offset++] = HexToChar(bytes[15] >> 4, _alpha);
                result[offset++] = HexToChar(bytes[15], _alpha);
            });
#endif
            throw new NotImplementedException();
        }


        static char HexToChar(int value, int alpha)
        {
            value &= 0xf;
            return (char)(value > 9 ? value - 10 + alpha : value + 0x30);
        }
    }
}
