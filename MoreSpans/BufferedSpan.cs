using System.ComponentModel;
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

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

    [Obsolete("Equals() on BufferedSpan will always throw an exception. Use the equality operator instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) =>
        Span.Equals(obj);

    [Obsolete("GetHashCode() on BufferedSpan will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() =>
        Span.GetHashCode();

#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    public static bool operator !=(BufferedSpan<Tfrom, Tto> left, BufferedSpan<Tfrom, Tto> right) =>
        !(left == right);

    public static bool operator ==(BufferedSpan<Tfrom, Tto> left, BufferedSpan<Tfrom, Tto> right) =>
        left.Span == right.Span && left._funcFromBuffer == right._funcFromBuffer && left._funcToBuffer == right._funcToBuffer;

    public Tto this[int index]
    {
        get
        {
            return _funcFromBuffer(Span[(index * _size)..]);
        }
        set
        {
            _funcToBuffer(value).CopyTo(Span[(index * _size)..]);
        }
    }

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
}
