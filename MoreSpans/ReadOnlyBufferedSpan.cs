﻿using System.Runtime.CompilerServices;

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
}