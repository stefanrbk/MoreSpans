using MoreSpans;

namespace UnitTests;

[TestFixture]
public class SpanExtensionsTests
{
    private readonly int[] neg50ToPos50 = new int[101];
    private readonly short[] neg25ToPos25 = new short[51];
    private readonly ConvertingSpan<int, int>.Factory negationFactory = new(i => -i, i => -i);
    private readonly BufferedSpan<byte, short>.Factory byteToShortFactory = new(BitConverter.ToInt16, BitConverter.GetBytes);

    [SetUp]
    public void Setup()
    {
        for (int i = -50, index = 0; i <= 50; i++, index++)
            neg50ToPos50[index] = i;

        for (int i = -25, index = 0; i <= 25; i++, index++)
            neg25ToPos25[index] = (short)i;
    }

    [Test]
    public void CopyToThrowsExceptionWhenDestinationIsTooShort()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Span<int> buf = stackalloc int[10];
                ReadOnlySpan<int> span1 = neg50ToPos50;

                var span2 = negationFactory.Build(buf);
                span1.CopyTo(span2);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Span<int> buf = stackalloc int[10];
                Span<int> span1 = neg50ToPos50;

                var span2 = negationFactory.Build(buf);
                span1.CopyTo(span2);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Span<byte> buf = stackalloc byte[20];
                ReadOnlySpan<short> span1 = neg25ToPos25;

                var span2 = byteToShortFactory.Build(buf);
                span1.CopyTo(span2);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Span<byte> buf = stackalloc byte[20];
                Span<short> span1 = neg25ToPos25;

                var span2 = byteToShortFactory.Build(buf);
                span1.CopyTo(span2);
            });
        });
    }

    [Test]
    public void TryCopyToReturnsFalseWhenDestinationIsTooShort()
    {
        Assert.Multiple(() =>
        {
            ReadOnlySpan<int> span1 = neg50ToPos50;
            ReadOnlySpan<short> span2 = neg25ToPos25;
            Span<int> span3 = neg50ToPos50;
            Span<short> span4 = neg25ToPos25;

            Span<int> buf1 = stackalloc int[10];
            Span<byte> buf2 = stackalloc byte[20];

            var span5 = negationFactory.Build(buf1);
            var span6 = byteToShortFactory.Build(buf2);

            Assert.That(span1.TryCopyTo(span5), Is.False);
            Assert.That(span2.TryCopyTo(span6), Is.False);
            Assert.That(span3.TryCopyTo(span5), Is.False);
            Assert.That(span4.TryCopyTo(span6), Is.False);
        });
    }

    [Test]
    public void CopyToCopiesTheValuesToAConvertingSpan()
    {
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                Span<int> buf = stackalloc int[8];
                ReadOnlySpan<int> span1 = stackalloc int[] { 0, -1, 2, -3, 4, -5 };
                var span2 = buf.AsConverter<int, int>(null!, i => i * 2);
                var span3 = span1.AsConverter(i => -i);

                span1.CopyTo(span2);

                Assert.That(buf[0], Is.EqualTo(0));
                Assert.That(buf[1], Is.EqualTo(-2));
                Assert.That(buf[2], Is.EqualTo(4));
                Assert.That(buf[3], Is.EqualTo(-6));
                Assert.That(buf[4], Is.EqualTo(8));
                Assert.That(buf[5], Is.EqualTo(-10));
                Assert.That(buf[6], Is.EqualTo(0));
                Assert.That(buf[7], Is.EqualTo(0));

                Assert.That(span3[0], Is.EqualTo(0));
                Assert.That(span3[1], Is.EqualTo(1));
                Assert.That(span3[2], Is.EqualTo(-2));
                Assert.That(span3[3], Is.EqualTo(3));
                Assert.That(span3[4], Is.EqualTo(-4));
                Assert.That(span3[5], Is.EqualTo(5));
            });

            Assert.Multiple(() =>
            {
                Span<int> buf = stackalloc int[8];
                Span<int> span1 = stackalloc int[] { 0, -1, 2, -3, 4, -5 };
                var span2 = buf.AsConverter<int, int>(null!, i => i * 2);

                span1.CopyTo(span2);

                Assert.That(buf[0], Is.EqualTo(0));
                Assert.That(buf[1], Is.EqualTo(-2));
                Assert.That(buf[2], Is.EqualTo(4));
                Assert.That(buf[3], Is.EqualTo(-6));
                Assert.That(buf[4], Is.EqualTo(8));
                Assert.That(buf[5], Is.EqualTo(-10));
                Assert.That(buf[6], Is.EqualTo(0));
                Assert.That(buf[7], Is.EqualTo(0));
            });
        });
    }

    [Test]
    public void CopyToCopiesTheValuesToABufferedSpan()
    {
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                Span<byte> buf = stackalloc byte[10];
                ReadOnlySpan<short> span1 = stackalloc short[] { 4, 5, 6, 0 };
                var span2 = buf.AsBuffer<byte, short>(null!, BitConverter.GetBytes);
                var span3 = span1.AsBuffer(i =>
                {
                    var a = BitConverter.GetBytes(i[0]);
                    var b = BitConverter.GetBytes(i[1]);
                    Span<byte> buf = stackalloc byte[4];
                    a.CopyTo(buf);
                    b.CopyTo(buf[2..]);

                    return BitConverter.ToInt32(buf);
                });

                span1.CopyTo(span2);

                Assert.That(buf[0], Is.EqualTo(4));
                Assert.That(buf[1], Is.EqualTo(0));
                Assert.That(buf[2], Is.EqualTo(5));
                Assert.That(buf[3], Is.EqualTo(0));
                Assert.That(buf[4], Is.EqualTo(6));
                Assert.That(buf[5], Is.EqualTo(0));
                Assert.That(buf[6], Is.EqualTo(0));
                Assert.That(buf[7], Is.EqualTo(0));
                Assert.That(buf[8], Is.EqualTo(0));
                Assert.That(buf[9], Is.EqualTo(0));

                Assert.That(span3[0], Is.EqualTo((5 << 16) | 4));
                Assert.That(span3[1], Is.EqualTo(6));
            });

            Assert.Multiple(() =>
            {
                Span<byte> buf = stackalloc byte[8];
                Span<short> span1 = stackalloc short[] { 4, 5, 6 };
                var span2 = buf.AsBuffer<byte, short>(null!, BitConverter.GetBytes);

                span1.CopyTo(span2);

                Assert.That(buf[0], Is.EqualTo(4));
                Assert.That(buf[1], Is.EqualTo(0));
                Assert.That(buf[2], Is.EqualTo(5));
                Assert.That(buf[3], Is.EqualTo(0));
                Assert.That(buf[4], Is.EqualTo(6));
                Assert.That(buf[5], Is.EqualTo(0));
                Assert.That(buf[6], Is.EqualTo(0));
                Assert.That(buf[7], Is.EqualTo(0));
            });
        });
    }

    [Test]
    public void TryCopyToCopiesTheValuesToAConvertingSpanAndReturnsTrue()
    {
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                Span<int> buf = stackalloc int[8];
                ReadOnlySpan<int> span1 = stackalloc int[] { 0, -1, 2, -3, 4, -5 };
                var span2 = buf.AsConverter<int, int>(null!, i => i * 2);

                Assert.That(span1.TryCopyTo(span2), Is.True);

                Assert.That(buf[0], Is.EqualTo(0));
                Assert.That(buf[1], Is.EqualTo(-2));
                Assert.That(buf[2], Is.EqualTo(4));
                Assert.That(buf[3], Is.EqualTo(-6));
                Assert.That(buf[4], Is.EqualTo(8));
                Assert.That(buf[5], Is.EqualTo(-10));
                Assert.That(buf[6], Is.EqualTo(0));
                Assert.That(buf[7], Is.EqualTo(0));
            });

            Assert.Multiple(() =>
            {
                Span<int> buf = stackalloc int[8];
                Span<int> span1 = stackalloc int[] { 0, -1, 2, -3, 4, -5 };
                var span2 = buf.AsConverter<int, int>(null!, i => i * 2);

                Assert.That(span1.TryCopyTo(span2), Is.True);

                Assert.That(buf[0], Is.EqualTo(0));
                Assert.That(buf[1], Is.EqualTo(-2));
                Assert.That(buf[2], Is.EqualTo(4));
                Assert.That(buf[3], Is.EqualTo(-6));
                Assert.That(buf[4], Is.EqualTo(8));
                Assert.That(buf[5], Is.EqualTo(-10));
                Assert.That(buf[6], Is.EqualTo(0));
                Assert.That(buf[7], Is.EqualTo(0));
            });
        });
    }

    [Test]
    public void TryCopyToCopiesTheValuesToABufferedSpanAndReturnsTrue()
    {
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                Span<byte> buf = stackalloc byte[8];
                ReadOnlySpan<short> span1 = stackalloc short[] { 4, 5, 6 };
                var span2 = buf.AsBuffer<byte, short>(null!, BitConverter.GetBytes);

                Assert.That(span1.TryCopyTo(span2), Is.True);

                Assert.That(buf[0], Is.EqualTo(4));
                Assert.That(buf[1], Is.EqualTo(0));
                Assert.That(buf[2], Is.EqualTo(5));
                Assert.That(buf[3], Is.EqualTo(0));
                Assert.That(buf[4], Is.EqualTo(6));
                Assert.That(buf[5], Is.EqualTo(0));
                Assert.That(buf[6], Is.EqualTo(0));
                Assert.That(buf[7], Is.EqualTo(0));
            });

            Assert.Multiple(() =>
            {
                Span<byte> buf = stackalloc byte[8];
                Span<short> span1 = stackalloc short[] { 4, 5, 6 };
                var span2 = buf.AsBuffer<byte, short>(null!, BitConverter.GetBytes);

                Assert.That(span1.TryCopyTo(span2), Is.True);

                Assert.That(buf[0], Is.EqualTo(4));
                Assert.That(buf[1], Is.EqualTo(0));
                Assert.That(buf[2], Is.EqualTo(5));
                Assert.That(buf[3], Is.EqualTo(0));
                Assert.That(buf[4], Is.EqualTo(6));
                Assert.That(buf[5], Is.EqualTo(0));
                Assert.That(buf[6], Is.EqualTo(0));
                Assert.That(buf[7], Is.EqualTo(0));
            });
        });
    }
}
