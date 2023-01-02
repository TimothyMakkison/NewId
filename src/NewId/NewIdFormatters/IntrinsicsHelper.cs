#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace MassTransit.NewIdFormatters
{
    internal static class IntrinsicsHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<byte> ToCharUtf16(Vector128<byte> value)
        {
            var widened = Avx2.ConvertToVector256Int16(value);
            return widened.AsByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<byte> EncodeBytesHex(Vector128<byte> bytes, bool isUpper)
        {
            var lowerCharSet = Vector256.Create((byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f');

            var upperCharSet = Vector256.Create((byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F');

            var x = Avx2.ConvertToVector256Int16(bytes);
            var high = Avx2.ShiftLeftLogical(x, 8);
            var low = Avx2.ShiftRightLogical(x, 4);
            var values = Avx2.And(Avx2.Or(high, low).AsByte(), Vector256.Create((byte)0x0F));
            return Avx2.Shuffle(isUpper ? upperCharSet : lowerCharSet, values);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static Vector128<T> LoadUnsafe<T>(ref T source)
        //   where T : struct
        //{
        //    return Unsafe.ReadUnaligned<Vector128<T>>(ref Unsafe.As<T, byte>(ref source));
        //}
    }
}
#endif
