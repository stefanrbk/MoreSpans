using System.Runtime.CompilerServices;

namespace MoreSpans;
public static class SpanExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo<T, Tto>(this Span<Tto> src, BufferedSpan<T, Tto> destination)
    {
        if (src.Length <= destination.Length)
        {
            for (int i = 0; i < src.Length; i++)
                destination[i] = src[i];
        }
        else
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo<T, Tto>(this ReadOnlySpan<Tto> src, BufferedSpan<T, Tto> destination)
    {
        if (src.Length <= destination.Length)
        {
            for (int i = 0; i < src.Length; i++)
                destination[i] = src[i];
        }
        else
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo<T, Tto>(this Span<Tto> src, ConvertingSpan<T, Tto> destination)
    {
        if (src.Length <= destination.Length)
        {
            for (int i = 0; i < src.Length; i++)
                destination[i] = src[i];
        }
        else
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo<T, Tto>(this ReadOnlySpan<Tto> src, ConvertingSpan<T, Tto> destination)
    {
        if (src.Length <= destination.Length)
        {
            for (int i = 0; i < src.Length; i++)
                destination[i] = src[i];
        }
        else
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCopyTo<T, Tto>(this Span<Tto> src, BufferedSpan<T, Tto> destination)
    {
        if (src.Length <= destination.Length)
        {
            for (int i = 0; i < src.Length; i++)
                destination[i] = src[i];
            return true;
        }
        else
        {
            return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCopyTo<T, Tto>(this ReadOnlySpan<Tto> src, BufferedSpan<T, Tto> destination)
    {
        if (src.Length <= destination.Length)
        {
            for (int i = 0; i < src.Length; i++)
                destination[i] = src[i];
            return true;
        }
        else
        {
            return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCopyTo<T, Tto>(this Span<Tto> src, ConvertingSpan<T, Tto> destination)
    {
        if (src.Length <= destination.Length)
        {
            for (int i = 0; i < src.Length; i++)
                destination[i] = src[i];
            return true;
        }
        else
        {
            return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCopyTo<T, Tto>(this ReadOnlySpan<Tto> src, ConvertingSpan<T, Tto> destination)
    {
        if (src.Length <= destination.Length)
        {
            for (int i = 0; i < src.Length; i++)
                destination[i] = src[i];
            return true;
        }
        else
        {
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyBufferedSpan<Tfrom, Tto> AsBuffer<Tfrom, Tto>(this ReadOnlySpan<Tfrom> span, FromBufferFunc<Tfrom, Tto> fromBufferFunc) =>
        new(span, fromBufferFunc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BufferedSpan<Tfrom, Tto> AsBuffer<Tfrom, Tto>(this Span<Tfrom> span, FromBufferFunc<Tfrom, Tto> fromBufferFunc, ToBufferFunc<Tfrom, Tto> toBufferFunc) =>
        new(span, fromBufferFunc, toBufferFunc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyConvertingSpan<Tfrom, Tto> AsConverter<Tfrom, Tto>(this ReadOnlySpan<Tfrom> span, ConvertFunc<Tfrom, Tto> forwardConvertFunc) =>
        new(span, forwardConvertFunc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConvertingSpan<Tfrom, Tto> AsConverter<Tfrom, Tto>(this Span<Tfrom> span, ConvertFunc<Tfrom, Tto> forwardConvertFunc, ConvertFunc<Tto, Tfrom> backwardConvertFunc) =>
        new(span, forwardConvertFunc, backwardConvertFunc);
}
