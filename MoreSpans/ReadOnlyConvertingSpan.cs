using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MoreSpans;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
[DebuggerTypeProxy(typeof(ReadOnlyConvertingSpan<,>.DebugView))]
public readonly ref struct ReadOnlyConvertingSpan<Tfrom, Tto>
{
    private readonly ConvertFunc<Tfrom, Tto> _funcTo;

    public ReadOnlySpan<Tfrom> Span { get; }

    public ReadOnlyConvertingSpan(ReadOnlySpan<Tfrom> span, ConvertFunc<Tfrom, Tto> funcTo)
    {
        Span = span;
        _funcTo = funcTo;
    }

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
