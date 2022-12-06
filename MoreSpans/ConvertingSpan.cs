using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MoreSpans;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
[DebuggerTypeProxy(typeof(ConvertingSpan<,>.DebugView))]
public readonly ref struct ConvertingSpan<Tfrom, Tto>
{
    private readonly ConvertFunc<Tfrom, Tto> _funcTo;
    private readonly ConvertFunc<Tto, Tfrom> _funcFrom;

    public Span<Tfrom> Span { get; }

    public ConvertingSpan(Span<Tfrom> span, ConvertFunc<Tfrom, Tto> funcTo, ConvertFunc<Tto, Tfrom> funcFrom)
    {
        Span = span;
        _funcTo = funcTo;
        _funcFrom = funcFrom;
    }

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
