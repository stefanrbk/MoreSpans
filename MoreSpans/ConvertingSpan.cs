using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MoreSpans;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
[DebuggerTypeProxy(typeof(ConvertingSpan<,>.DebugView))]
public readonly ref struct ConvertingSpan<Tfrom, Tto>
{
    private readonly ConvertFunc<Tfrom, Tto> _funcTo;
    private readonly ConvertFunc<Tto, Tfrom> _funcFrom;

    public Span<Tfrom> Span { get; }

    public bool IsEmpty => 0 >= Span.Length;

    public static ConvertingSpan<Tfrom, Tto> Empty => default;

    public ConvertingSpan(Span<Tfrom> span, ConvertFunc<Tfrom, Tto> funcTo, ConvertFunc<Tto, Tfrom> funcFrom)
    {
        Span = span;
        _funcTo = funcTo;
        _funcFrom = funcFrom;
    }

    public ConvertingSpan(Span<Tfrom> span, int start, int length, ConvertFunc<Tfrom, Tto> funcTo, ConvertFunc<Tto, Tfrom> funcFrom)
    {
        Span = span.Slice(start, length);
        _funcTo = funcTo;
        _funcFrom = funcFrom;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ConvertingSpan(void* pointer, int length, ConvertFunc<Tfrom, Tto> funcTo, ConvertFunc<Tto, Tfrom> funcFrom)
    {
        Span = new(pointer, length);
        _funcTo = funcTo;
        _funcFrom = funcFrom;
    }

    public static bool operator !=(ConvertingSpan<Tfrom, Tto> left, ConvertingSpan<Tfrom, Tto> right) =>
        !(left == right);

    public static bool operator ==(ConvertingSpan<Tfrom, Tto> left, ConvertingSpan<Tfrom, Tto> right) =>
        left.Span == right.Span && left._funcTo == right._funcTo && left._funcFrom == right._funcFrom;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

    [Obsolete("Equals() on ConvertingSpan will always throw an exception. Use the equality operator instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) =>
        Span.Equals(obj);

    [Obsolete("GetHashCode() on ConvertingSpan will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() =>
        Span.GetHashCode();

#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    public int Length =>
        Span.Length;

    public Tto this[int index]
    {
        get => _funcTo(Span[index]);
        set => Span[index] = _funcFrom(value);
    }

    public ConvertingSpan<Tfrom, Tto> this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Span.Length);
            return Slice(start, length);
        }
    }

    public Tto this[Index index]
    {
        get => this[index.GetOffset(Length)];
        set => this[index.GetOffset(Length)] = value;
    }

    public ConvertingSpan<Tfrom, Tto> Slice(int start) =>
        new(Span[start..], _funcTo, _funcFrom);

    public ConvertingSpan<Tfrom, Tto> Slice(int start, int length) =>
        new(Span.Slice(start, length), _funcTo, _funcFrom);

    public static ConvertingSpan<Tfrom, Tto> operator +(ConvertingSpan<Tfrom, Tto> span, int increase) =>
        span[increase..];

    public static ConvertingSpan<Tfrom, Tto> operator ++(ConvertingSpan<Tfrom, Tto> span) =>
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

    public Tto[] ToArray()
    {
        if (Length == 0)
            return Array.Empty<Tto>();

        Tto[] array = new Tto[Length];
        CopyTo(array);
        return array;
    }

    public static implicit operator ReadOnlyConvertingSpan<Tfrom, Tto>(ConvertingSpan<Tfrom, Tto> span) =>
        new(span.Span, span._funcTo);

    public Enumerator GetEnumerator() =>
        new Enumerator(this);

    public ref struct Enumerator
    {
        private readonly ConvertingSpan<Tfrom, Tto> _span;
        private int _index;
        public readonly Tto Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get =>
                _span[_index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ConvertingSpan<Tfrom, Tto> span)
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
    public override string ToString() =>
        $"MoreSpans.ConvertingSpan<{typeof(Tfrom).Name},{typeof(Tto).Name}>[{Length}]";

    [ExcludeFromCodeCoverage]
    private string GetDebuggerDisplay() =>
        ToString();

    [ExcludeFromCodeCoverage]
    private sealed class DebugView
    {
        public DebugView(ConvertingSpan<Tfrom, Tto> span)
        {
            Items = new string[span.Length];
            GetterFunction = span._funcTo;
            SetterFunction= span._funcFrom;

            for (var i = 0; i < span.Length; i++)
            {
                var value = span.Span[i];
                object? get;
                object? set;
                try
                {
                    get = span._funcTo(value);
                    try
                    {
                        set = span._funcFrom((Tto)get!);
                    }
                    catch (Exception e)
                    {
                        set = e;
                    }
                }
                catch (Exception e)
                {
                    get = e;
                    set = null;
                }

                Items[i] = get is not Exception ?
                    $"{value} -Get-> {get} -Set-> {set}" :
                    $"{value} -Get-> {get}";
            }
        }

        public ConvertFunc<Tfrom, Tto> GetterFunction { get; }
        public ConvertFunc<Tto, Tfrom> SetterFunction { get; }

        public string[] Items { get; }
    }
}
