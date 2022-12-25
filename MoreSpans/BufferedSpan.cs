using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MoreSpans;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
[DebuggerTypeProxy(typeof(BufferedSpan<,>.DebugView))]
public readonly ref partial struct BufferedSpan<Tfrom, Tto>
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
        Span.Length == 0
            ? 0
            : Span.Length / _size;

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
        get =>
            _funcFromBuffer(Span[(index * _size)..]);
        set =>
            _funcToBuffer(value).CopyTo(Span[(index * _size)..]);
    }

    public Tto this[Index index]
    {
        get =>
            this[index.GetOffset(Length)];
        set =>
            this[index.GetOffset(Length)] = value;
    }

    public BufferedSpan<Tfrom, Tto> this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Length);
            return Slice(start, length);
        }
    }

    public BufferedSpan<Tfrom, Tto> Slice(int start) =>
        new(Span[(start * _size)..], _funcFromBuffer, _funcToBuffer);

    public BufferedSpan<Tfrom, Tto> Slice(int start, int length) =>
        new(Span.Slice(start * _size, length * _size), _funcFromBuffer, _funcToBuffer);

    public static BufferedSpan<Tfrom, Tto> operator +(BufferedSpan<Tfrom, Tto> span, int start) =>
        span[start..];

    public static BufferedSpan<Tfrom, Tto> operator ++(BufferedSpan<Tfrom, Tto> span) =>
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

    public static implicit operator ReadOnlyBufferedSpan<Tfrom, Tto>(BufferedSpan<Tfrom, Tto> span) =>
        new(span.Span, span._funcFromBuffer);

    [ExcludeFromCodeCoverage]
    public override string ToString() =>
        $"MoreSpans.BufferedSpan<{typeof(Tfrom).Name},{typeof(Tto).Name}>[{Span.Length} <-> {Length}]";

    [ExcludeFromCodeCoverage]
    private string GetDebuggerDisplay() =>
        ToString();

    public Enumerator GetEnumerator() =>
        new(this);

    public ref struct Enumerator
    {
        private readonly BufferedSpan<Tfrom, Tto> _span;
        private int _index;
        public readonly Tto Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get =>
                _span[_index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(BufferedSpan<Tfrom, Tto> span)
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
        public DebugView(BufferedSpan<Tfrom, Tto> span)
        {
            Items = new string[span.Length];
            GetterFunction = span._funcFromBuffer;
            SetterFunction = span._funcToBuffer;
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
                object? set = null;
                if (get is Tto v)
                {
                    try
                    {
                        set = span._funcToBuffer(v);
                    }
                    catch (Exception e)
                    {
                        set = e;
                    }
                }

                Items[i] = get is not Tto
                    ? $"{value} -Get-> {get}"
                    : $"{value} -Get-> {get} -Set-> {set}";
            }
        }

        public FromBufferFunc<Tfrom, Tto> GetterFunction { get; }

        public ToBufferFunc<Tfrom, Tto> SetterFunction { get; }

        public string[] Items { get; }
    }
}
