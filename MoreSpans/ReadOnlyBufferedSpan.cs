using System.Runtime.CompilerServices;

namespace MoreSpans;
public readonly ref struct ReadOnlyBufferedSpan<Tfrom, Tto>
{
    private readonly Func<Tfrom, Tto> _funcFromBuffer;
    private readonly int _size;

    public ReadOnlySpan<Tfrom> Span { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyBufferedSpan(ReadOnlySpan<Tfrom> span, Func<Tfrom, Tto> funcFromBuffer)
    {
        _funcFromBuffer = funcFromBuffer;
        Span = span;
        _size = Unsafe.SizeOf<Tto>() / Unsafe.SizeOf<Tfrom>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyBufferedSpan(ReadOnlySpan<Tfrom> span, int start, int length, Func<Tfrom, Tto> funcFromBuffer)
        : this(span.Slice(start, length), funcFromBuffer) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ReadOnlyBufferedSpan(void* pointer, int length, Func<Tfrom, Tto> funcFromBuffer)
        : this(new(pointer, length), funcFromBuffer) { }
}
