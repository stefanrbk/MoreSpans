using System.Runtime.CompilerServices;

namespace MoreSpans;
public readonly ref struct BufferedSpan<Tfrom, Tto>
{
    private readonly FromBufferFunc<Tfrom, Tto> _funcFromBuffer;
    private readonly ToBufferFunc<Tfrom, Tto> _funcToBuffer;
    private readonly int _size;

    public Span<Tfrom> Span { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BufferedSpan(Span<Tfrom> span, FromBufferFunc<Tfrom, Tto> funcFromBuffer, ToBufferFunc<Tfrom, Tto> funcToBuffer)
    {
        _funcFromBuffer = funcFromBuffer;
        _funcToBuffer = funcToBuffer;
        Span= span;
        _size = Unsafe.SizeOf<Tto>() / Unsafe.SizeOf<Tfrom>();

        if (_size == 0)
            throw new NotSupportedException($"The first type arguement \"{typeof(Tfrom).Name}\" cannot be larger than the second type arguement \"{typeof(Tto).Name}\".");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BufferedSpan(Span<Tfrom> span, int start, int length, FromBufferFunc<Tfrom, Tto> funcFromBuffer, ToBufferFunc<Tfrom, Tto> funcToBuffer)
        : this(span.Slice(start, length), funcFromBuffer, funcToBuffer) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe BufferedSpan(void* pointer, int length, FromBufferFunc<Tfrom, Tto> funcFromBuffer, ToBufferFunc<Tfrom, Tto> funcToBuffer)
        : this(new(pointer, length), funcFromBuffer, funcToBuffer) { }

    public int Length =>
        Span.Length / _size;

    public bool IsEmpty =>
        Span.IsEmpty;

    public static BufferedSpan<Tfrom, Tto> Empty => default;
}
