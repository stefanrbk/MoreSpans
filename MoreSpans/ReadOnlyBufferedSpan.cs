using System.Runtime.CompilerServices;

namespace MoreSpans;
public readonly ref struct ReadOnlyBufferedSpan<Tfrom, Tto>
{
    private readonly FromBufferFunc<Tfrom, Tto> _funcFromBuffer;
    private readonly int _size;

    public ReadOnlySpan<Tfrom> Span { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyBufferedSpan(ReadOnlySpan<Tfrom> span, FromBufferFunc<Tfrom, Tto> funcFromBuffer)
    {
        _funcFromBuffer = funcFromBuffer;
        Span = span;
        _size = Unsafe.SizeOf<Tto>() / Unsafe.SizeOf<Tfrom>();

        if (_size == 0)
            throw new NotSupportedException($"The first type arguement \"{typeof(Tfrom).Name}\" cannot be larger than the second type arguement \"{typeof(Tto).Name}\".");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyBufferedSpan(ReadOnlySpan<Tfrom> span, int start, int length, FromBufferFunc<Tfrom, Tto> funcFromBuffer)
        : this(span.Slice(start, length), funcFromBuffer) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ReadOnlyBufferedSpan(void* pointer, int length, FromBufferFunc<Tfrom, Tto> funcFromBuffer)
        : this(new(pointer, length), funcFromBuffer) { }

    public int Length =>
        Span.Length / _size;

    public Tto this[int index] =>
        _funcFromBuffer(Span[(index * _size)..]);

    public Tto this[Index index] =>
        this[index.GetOffset(Length)];

    public ReadOnlyBufferedSpan<Tfrom, Tto> this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Length);
            return Slice(start, length);
        }
    }

    public ReadOnlyBufferedSpan<Tfrom, Tto> Slice(int start) =>
        new(Span[(start * _size)..], _funcFromBuffer);

    public ReadOnlyBufferedSpan<Tfrom, Tto> Slice(int start, int length) =>
        new(Span.Slice(start * _size, length * _size), _funcFromBuffer);

    public static ReadOnlyBufferedSpan<Tfrom, Tto> operator +(ReadOnlyBufferedSpan<Tfrom, Tto> span, int start) =>
        span[start..];

    public static ReadOnlyBufferedSpan<Tfrom, Tto> operator ++(ReadOnlyBufferedSpan<Tfrom, Tto> span) =>
        span[1..];

    public 
}
