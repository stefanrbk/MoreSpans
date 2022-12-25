using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MoreSpans;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
[DebuggerTypeProxy(typeof(ReadOnlyBufferedSpan<,>.DebugView))]
public readonly ref partial struct ReadOnlyBufferedSpan<Tfrom, Tto>
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
        Span.Length == 0
            ? 0
            : Span.Length / _size;

    public bool IsEmpty =>
        Span.IsEmpty;

    public static ReadOnlyBufferedSpan<Tfrom, Tto> Empty => default;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

    [Obsolete("Equals() on ReadOnlyBufferedSpan will always throw an exception. Use the equality operator instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) =>
        Span.Equals(obj);

    [Obsolete("GetHashCode() on ReadOnlyBufferedSpan will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() =>
        Span.GetHashCode();

#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    public static bool operator !=(ReadOnlyBufferedSpan<Tfrom, Tto> left, ReadOnlyBufferedSpan<Tfrom, Tto> right) =>
        !(left == right);

    public static bool operator ==(ReadOnlyBufferedSpan<Tfrom, Tto> left, ReadOnlyBufferedSpan<Tfrom, Tto> right) =>
        left.Span == right.Span && left._funcFromBuffer == right._funcFromBuffer;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<Tto> destination)
    {
        if (Length <= destination.Length)
        {
            for (int i = 0; i < Length; i++)
                destination[i] = this[i];
        }
        else
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo<T>(BufferedSpan<T, Tto> destination)
    {
        if (Length <= destination.Length)
        {
            for (int i = 0; i < Length; i++)
                destination[i] = this[i];
        }
        else
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo<T>(ConvertingSpan<T, Tto> destination)
    {
        if (Length <= destination.Length)
        {
            for (int i = 0; i < Length; i++)
                destination[i] = this[i];
        }
        else
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryCopyTo(Span<Tto> destination)
    {
        if (Length <= destination.Length)
        {
            for (int i = 0; i < Length; i++)
                destination[i] = this[i];

            return true;
        }
        else
        {
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryCopyTo<T>(BufferedSpan<T, Tto> destination)
    {
        if (Length <= destination.Length)
        {
            for (int i = 0; i < Length; i++)
                destination[i] = this[i];

            return true;
        }
        else
        {
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryCopyTo<T>(ConvertingSpan<T, Tto> destination)
    {
        if (Length <= destination.Length)
        {
            for (int i = 0; i < Length; i++)
                destination[i] = this[i];

            return true;
        }
        else
        {
            return false;
        }
    }

    public Tto[] ToArray()
    {
        var length = Length;
        var array = new Tto[length];
        for (var i = 0; i < length; i++)
            array[i] = this[i];

        return array;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() =>
        $"MoreSpans.ReadOnlyBufferedSpan<{typeof(Tfrom).Name},{typeof(Tto).Name}>[{Span.Length} -> {Length}]";

    [ExcludeFromCodeCoverage]
    private string GetDebuggerDisplay() =>
        ToString();

    public Enumerator GetEnumerator() =>
        new(this);

    public ref struct Enumerator
    {
        private readonly ReadOnlyBufferedSpan<Tfrom, Tto> _span;
        private int _index;
        public readonly Tto Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get =>
                _span[_index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlyBufferedSpan<Tfrom, Tto> span)
        {
            _span = span;
            _index = -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int num = _index + 1;
            if (num < _span.Length)
            {
                _index = num;
                return true;
            }
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    private sealed class DebugView
    {
        public DebugView(ReadOnlyBufferedSpan<Tfrom, Tto> span)
        {
            Items = new string[span.Length];
            GetterFunction = span._funcFromBuffer;
            var size = span._size;

            for (var i = 0; i < span.Length; i++)
            {
                var temp = span.Span[(i * size)..];
                var value = temp[..size].ToArray();
                object? get;
                try
                {
                    get = span._funcFromBuffer(value);
                }
                catch (Exception e)
                {
                    get = e;
                }

                Items[i] = $"{value} -Get-> {get}";
            }
        }

        public FromBufferFunc<Tfrom, Tto> GetterFunction { get; }

        public string[] Items { get; }
    }
}
