using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MoreSpans;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
[DebuggerTypeProxy(typeof(ReadOnlyConvertingSpan<,>.DebugView))]
public readonly ref struct ReadOnlyConvertingSpan<Tfrom, Tto>
{
    private readonly ConvertFunc<Tfrom, Tto> _funcTo;

    public ReadOnlySpan<Tfrom> Span { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyConvertingSpan(ReadOnlySpan<Tfrom> span, ConvertFunc<Tfrom, Tto> funcTo)
    {
        Span = span;
        _funcTo = funcTo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyConvertingSpan(ReadOnlySpan<Tfrom> span, int start, int length, ConvertFunc<Tfrom, Tto> funcTo)
    {
        Span = span.Slice(start, length);
        _funcTo = funcTo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ReadOnlyConvertingSpan(void* pointer, int length, ConvertFunc<Tfrom, Tto> funcTo)
    {
        Span = new(pointer, length);
        _funcTo = funcTo;
    }

    public static bool operator !=(ReadOnlyConvertingSpan<Tfrom, Tto> left, ReadOnlyConvertingSpan<Tfrom, Tto> right) =>
        !(left == right);

    public static bool operator ==(ReadOnlyConvertingSpan<Tfrom, Tto> left, ReadOnlyConvertingSpan<Tfrom, Tto> right) =>
        left.Span == right.Span && left._funcTo == right._funcTo;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

    [Obsolete("Equals() on ReadOnlyConvertingSpan will always throw an exception. Use the equality operator instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) =>
        Span.Equals(obj);

    [Obsolete("GetHashCode() on ReadOnlyConvertingSpan will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() =>
        Span.GetHashCode();

#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    public bool IsEmpty => 0 >= Span.Length;

    public static ReadOnlyConvertingSpan<Tfrom, Tto> Empty => default;

    public int Length =>
        Span.Length;

    public Tto this[int index] =>
        _funcTo(Span[index]);

    public ReadOnlyConvertingSpan<Tfrom, Tto> this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Span.Length);
            return Slice(start, length);
        }
    }

    public Tto this[Index index] =>
        this[index.GetOffset(Length)];

    public ReadOnlyConvertingSpan<Tfrom, Tto> Slice(int start) =>
        new(Span[start..], _funcTo);

    public ReadOnlyConvertingSpan<Tfrom, Tto> Slice(int start, int length) =>
        new(Span.Slice(start, length), _funcTo);

    public static ReadOnlyConvertingSpan<Tfrom, Tto> operator +(ReadOnlyConvertingSpan<Tfrom, Tto> span, int increase) =>
        span[increase..];

    public static ReadOnlyConvertingSpan<Tfrom, Tto> operator ++(ReadOnlyConvertingSpan<Tfrom, Tto> span) =>
        span[1..];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<Tto> destination)
    {
        if (Length <= destination.Length)
        {
            for (int i = 0; i < Length; i++)
            {
                destination[i] = this[i];
            }
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
            {
                destination[i] = this[i];
            }
        }
        else
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
    }

    public bool TryCopyTo(Span<Tto> destination)
    {
        if (Length <= destination.Length)
        {
            for (int i = 0; i < Length; i++)
            {
                destination[i] = this[i];
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    public bool TryCopyTo<T>(ConvertingSpan<T, Tto> destination)
    {
        if (Length <= destination.Length)
        {
            for (int i = 0; i < Length; i++)
            {
                destination[i] = this[i];
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    public Tto[] ToArray()
    {
        if (Length == 0)
            return Array.Empty<Tto>();

        Tto[] array = new Tto[Length];
        CopyTo(array);
        return array;
    }

    public Enumerator GetEnumerator() =>
        new Enumerator(this);

    public ref struct Enumerator
    {
        private readonly ReadOnlyConvertingSpan<Tfrom, Tto> _span;
        private int _index;
        public readonly Tto Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get =>
                _span[_index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlyConvertingSpan<Tfrom, Tto> span)
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
                _index= num;
                return true;
            }
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() =>
        $"MoreSpans.ReadOnlyConvertingSpan<{typeof(Tfrom).Name},{typeof(Tto).Name}>[{Length}]";

    [ExcludeFromCodeCoverage]
    private string GetDebuggerDisplay() =>
        ToString();

    [ExcludeFromCodeCoverage]
    private sealed class DebugView
    {
        public DebugView(ReadOnlyConvertingSpan<Tfrom, Tto> span)
        {
            Items = new string[span.Length];
            GetterFunction = span._funcTo;

            for (var i = 0; i < span.Length; i++)
            {
                var value = span.Span[i];
                object? get;
                try
                {
                    get = span._funcTo(value);
                }
                catch (Exception e)
                {
                    get = e;
                }

                Items[i] = $"{value} -Get-> {get}";
            }
        }

        public ConvertFunc<Tfrom, Tto> GetterFunction { get; }

        public string[] Items { get; }
    }
}
