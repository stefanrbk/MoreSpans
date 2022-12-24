using System.Runtime.CompilerServices;

namespace MoreSpans;
public readonly ref struct ReadOnlyBufferedSpan<Tfrom, Tto>
{
    private readonly Func<Tfrom, Tto> _funcFromBuffer;

    public ReadOnlySpan<Tfrom> Span { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyBufferedSpan(ReadOnlySpan<Tfrom> span, Func<Tfrom, Tto> funcFromBuffer)
    {
        _funcFromBuffer = funcFromBuffer;
        Span = span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyBufferedSpan(ReadOnlySpan<Tfrom> span, int start, int length, Func<Tfrom, Tto> funcFromBuffer)
    {
        Span = span.Slice(start, length);
        _funcFromBuffer = funcFromBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ReadOnlyBufferedSpan(void* pointer, int length, Func<Tfrom, Tto> funcFromBuffer)
    {
        Span = new(pointer, length);
        _funcFromBuffer = funcFromBuffer;
    }
}
