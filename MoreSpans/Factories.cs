using System.Runtime.CompilerServices;

namespace MoreSpans;

public readonly ref partial struct ReadOnlyConvertingSpan<Tfrom, Tto>
{
    public class Factory
    {
        private readonly ConvertFunc<Tfrom, Tto> _funcTo;

        public Factory(ConvertFunc<Tfrom, Tto> funcTo) =>
            _funcTo = funcTo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyConvertingSpan<Tfrom, Tto> Build(ReadOnlySpan<Tfrom> span) =>
            new(span, _funcTo);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyConvertingSpan<Tfrom, Tto> Build(ReadOnlySpan<Tfrom> span, int start, int length) =>
            new(span, start, length, _funcTo);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlyConvertingSpan<Tfrom, Tto> Build(void* pointer, int length) =>
            new(pointer, length, _funcTo);
    }
}

public readonly ref partial struct ConvertingSpan<Tfrom, Tto>
{
    public class Factory
    {
        private readonly ConvertFunc<Tfrom, Tto> _funcTo;
        private readonly ConvertFunc<Tto, Tfrom> _funcFrom;

        public Factory(ConvertFunc<Tfrom, Tto> funcTo, ConvertFunc<Tto, Tfrom> funcFrom) =>
            (_funcTo, _funcFrom) = (funcTo, funcFrom);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConvertingSpan<Tfrom, Tto> Build(Span<Tfrom> span) =>
            new(span, _funcTo, _funcFrom);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConvertingSpan<Tfrom, Tto> Build(Span<Tfrom> span, int start, int length) =>
            new(span, start, length, _funcTo, _funcFrom);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ConvertingSpan<Tfrom, Tto> Build(void* pointer, int length) =>
            new(pointer, length, _funcTo, _funcFrom);
    }
}

public readonly ref partial struct ReadOnlyBufferedSpan<Tfrom, Tto>
{
    public class Factory
    {
        private readonly FromBufferFunc<Tfrom, Tto> _funcFromBuffer;

        public Factory(FromBufferFunc<Tfrom, Tto> funcFromBuffer) =>
            _funcFromBuffer = funcFromBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyBufferedSpan<Tfrom, Tto> Build(ReadOnlySpan<Tfrom> span) =>
            new(span, _funcFromBuffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyBufferedSpan<Tfrom, Tto> Build(ReadOnlySpan<Tfrom> span, int start, int length) =>
            new(span, start, length, _funcFromBuffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlyBufferedSpan<Tfrom, Tto> Build(void* pointer, int length) =>
            new(pointer, length, _funcFromBuffer);
    }
}

public readonly ref partial struct BufferedSpan<Tfrom, Tto>
{
    public class Factory
    {
        private readonly FromBufferFunc<Tfrom, Tto> _funcFromBuffer;
        private readonly ToBufferFunc<Tfrom, Tto> _funcToBuffer;

        public Factory(FromBufferFunc<Tfrom, Tto> funcFromBuffer, ToBufferFunc<Tfrom, Tto> funcToBuffer) =>
            (_funcFromBuffer, _funcToBuffer) = (funcFromBuffer, funcToBuffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BufferedSpan<Tfrom, Tto> Build(Span<Tfrom> span) =>
            new(span, _funcFromBuffer, _funcToBuffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BufferedSpan<Tfrom, Tto> Build(Span<Tfrom> span, int start, int length) =>
            new(span, start, length, _funcFromBuffer, _funcToBuffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe BufferedSpan<Tfrom, Tto> Build(void* pointer, int length) =>
            new(pointer, length, _funcFromBuffer, _funcToBuffer);
    }
}
