using MoreSpans;

namespace UnitTests;

[TestFixture(TestOf = typeof(BufferedSpan<,>))]
public class BufferedSpanTests
{
    private readonly byte[] neg25ToPos25AsShort = new byte[102];
    private readonly BufferedSpan<byte, short>.Factory byteToShortFactory = new(BitConverter.ToInt16, BitConverter.GetBytes);
    private readonly ConvertingSpan<short, short>.Factory negationFactory = new(i => (short)-i, i => (short)-i);
    private readonly BufferedSpan<byte, short>.Factory byteToShortWriteDoubleFactory = new(BitConverter.ToInt16, i => BitConverter.GetBytes((short)(i * 2)));

    [SetUp]
    public void Setup()
    {
        for (int i = -25, index = 0; i <= 25; i++, index += 2)
            BitConverter.GetBytes((short)i).CopyTo(neg25ToPos25AsShort, index);
    }

    [Test]
    public void CreatingABufferedSpanFromShortToByteThrowsException()
    {
        Assert.Throws<NotSupportedException>(
            () =>
                new BufferedSpan<short, byte>(Array.Empty<short>(), null!, null!));
    }

    [Test]
    public void BufferedSpanIndexReturnsCorrectValues()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg25ToPos25AsShortSpan();

            for (int i = 0; i < span.Length; i++)
            {
                Assert.That(span[i], Is.EqualTo(BitConverter.ToInt16(neg25ToPos25AsShort.AsSpan()[(i * 2)..])));
            }
        });
    }

    [Test]
    public void IndexGivesProperResult()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg25ToPos25AsShortSpan();

            Assert.That(span[^1], Is.EqualTo(25));
            Assert.That(span[^2], Is.EqualTo(24));
            Assert.That(span[^50], Is.EqualTo(-24));
            Assert.That(span[^51], Is.EqualTo(-25));
        });
    }

    [Test]
    public void SliceGivesProperSegment1()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg25ToPos25AsShortSpan()[15..^15];

            Assert.That(span.Length, Is.EqualTo(21));
            Assert.That(span[0], Is.EqualTo(-10));
            Assert.That(span[^1], Is.EqualTo(10));
        });
    }

    [Test]
    public void SliceGivesProperSegment2()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg25ToPos25AsShortSpan()[5..16];

            Assert.That(span.Length, Is.EqualTo(11));
            Assert.That(span[0], Is.EqualTo(-20));
            Assert.That(span[^1], Is.EqualTo(-10));
        });
    }

    [Test]
    public void SliceGivesProperSegment3()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg25ToPos25AsShortSpan()[..21];

            Assert.That(span.Length, Is.EqualTo(21));
            Assert.That(span[0], Is.EqualTo(-25));
            Assert.That(span[^1], Is.EqualTo(-5));
        });
    }

    [Test]
    public void SliceGivesProperSegment4()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg25ToPos25AsShortSpan()[5..];

            Assert.That(span.Length, Is.EqualTo(46));
            Assert.That(span[0], Is.EqualTo(-20));
            Assert.That(span[^1], Is.EqualTo(25));
        });
    }

    [Test]
    public void SliceGivesProperSegment5()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg25ToPos25AsShortSpan().Slice(1);

            Assert.That(span.Length, Is.EqualTo(50));
            Assert.That(span[0], Is.EqualTo(-24));
            Assert.That(span[^1], Is.EqualTo(25));
        });
    }

    [Test]
    public void AdditionGivesProperSegment()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg25ToPos25AsShortSpan() + 10;

            Assert.That(span.Length, Is.EqualTo(41));
            Assert.That(span[0], Is.EqualTo(-15));
            Assert.That(span[^1], Is.EqualTo(25));
        });
    }

    [Test]
    public void IncrementGivesProperSegment()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg25ToPos25AsShortSpan();
            span++;

            Assert.That(span.Length, Is.EqualTo(50));
            Assert.That(span[0], Is.EqualTo(-24));
            Assert.That(span[^1], Is.EqualTo(25));
        });
    }

    [Test]
    public void SetterBuffersProperly()
    {
        Assert.Multiple(() =>
        {
            var span = byteToShortWriteDoubleFactory.Build(neg25ToPos25AsShort);

            Assert.That(neg25ToPos25AsShort[0], Is.EqualTo(231));
            Assert.That(neg25ToPos25AsShort[1], Is.EqualTo(255));
            Assert.That(neg25ToPos25AsShort[^2], Is.EqualTo(25));
            Assert.That(neg25ToPos25AsShort[^1], Is.EqualTo(0));

            Assert.That(span[0], Is.EqualTo(-25));
            Assert.That(span[^1], Is.EqualTo(25));

            span[0] = -21;
            span[^1] = 7;

            Assert.That(neg25ToPos25AsShort[0], Is.EqualTo(214));
            Assert.That(neg25ToPos25AsShort[1], Is.EqualTo(255));
            Assert.That(neg25ToPos25AsShort[^2], Is.EqualTo(14));
            Assert.That(neg25ToPos25AsShort[^1], Is.EqualTo(0));

            Assert.That(span[0], Is.EqualTo(-42));
            Assert.That(span[^1], Is.EqualTo(14));
        });
    }

    [Test]
    public void EqualityWorksAsExpected()
    {
        Assert.Multiple(() =>
        {
            var span1 = GetNeg25ToPos25AsShortSpan();
            var span2 = GetNeg10ToPos10AsShortSpan();
            var span3 = new BufferedSpan<byte, short>(span1.Span, i => (short)(BitConverter.ToInt16(i) * 2), BitConverter.GetBytes);
            var span4 = new BufferedSpan<byte, short>(span1.Span, BitConverter.ToInt16, i => BitConverter.GetBytes((short)(i + 1)));

            Assert.That(span1 != span2, Is.True);
            Assert.That(span1 == span2, Is.False);
            Assert.That(span2 != span3, Is.True);
            Assert.That(span1 != span4, Is.True);
        });
    }

    [Test]
    public void EqualsAndGetHashCodeThrowsExceptions()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<NotSupportedException>(() => GetNeg25ToPos25AsShortSpan().Equals(null));
            Assert.Throws<NotSupportedException>(() => GetNeg25ToPos25AsShortSpan().GetHashCode());
        });
    }

    [Test]
    public void IsEmptyIsFalseWhenSpanHasValues()
    {
        var span = GetNeg25ToPos25AsShortSpan();
        Assert.That(span.IsEmpty, Is.False);
    }

    [Test]
    public void IsEmptyIsTrueWhenSpanHasNoValues()
    {
        var span = BufferedSpan<byte, short>.Empty;
        Assert.That(span.IsEmpty, Is.True);
    }

    [Test]
    public void CopyToThrowsExceptionWhenDestinationIsTooShort()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Span<byte> buf1 = stackalloc byte[20];
                Span<short> buf2 = stackalloc short[9];

                var span = byteToShortFactory.Build(buf1);
                span.CopyTo(buf2);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Span<byte> buf1 = stackalloc byte[20];
                Span<byte> buf2 = stackalloc byte[18];

                var span1 = byteToShortFactory.Build(buf1);
                var span2 = byteToShortFactory.Build(buf2);
                span1.CopyTo(span2);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Span<byte> buf1 = stackalloc byte[20];
                Span<short> buf2 = stackalloc short[9];

                var span1 = byteToShortFactory.Build(buf1);
                var span2 = negationFactory.Build(buf2);

                span1.CopyTo(span2);
            });
        });
    }

    [Test]
    public void TryCopyToReturnsFalseWhenDestinationIsTooShort()
    {
        Assert.Multiple(() =>
        {
            Span<byte> buf1 = stackalloc byte[20];
            Span<byte> buf2 = stackalloc byte[18];
            Span<short> buf3 = stackalloc short[9];

            var span1 = byteToShortFactory.Build(buf1);
            var span2 = negationFactory.Build(buf3);
            var span3 = byteToShortFactory.Build(buf2);

            Assert.That(span1.TryCopyTo(buf3), Is.False);
            Assert.That(span1.TryCopyTo(span2), Is.False);
            Assert.That(span1.TryCopyTo(span3), Is.False);
        });
    }

    [Test]
    public void CopyToCopiesTheValuesToASpan()
    {
        Assert.Multiple(() =>
        {
            Span<byte> buf1 = stackalloc byte[] { 04, 00, 05, 00, 06, 00 };
            Span<short> buf2 = stackalloc short[5];

            var span = byteToShortFactory.Build(buf1);
            span.CopyTo(buf2);

            for (var i = 0; i < buf2.Length; i++)
            {
                Assert.That(buf2[i], Is.EqualTo(i < span.Length ? BitConverter.ToInt16(buf1[(i * 2)..]) : 0));
                Assert.That(buf2[i], Is.EqualTo(i < span.Length ? span[i] : 0));
            }
        });
    }

    [Test]
    public void CopyToCopiesTheValuesToAConvertingSpan()
    {
        Assert.Multiple(() =>
        {
            Span<byte> buf1 = stackalloc byte[] { 04, 00, 05, 00, 06, 00 };
            Span<short> buf2 = stackalloc short[5];

            var span1 = byteToShortFactory.Build(buf1);
            var span2 = negationFactory.Build(buf2);
            span1.CopyTo(span2);

            for (var i = 0; i < buf2.Length; i++)
            {
                Assert.That(span2[i], Is.EqualTo(i < span1.Length ? span1[i] : 0));
                Assert.That(buf2[i], Is.EqualTo(i < buf1.Length / 2 ? -BitConverter.ToInt16(buf1[(i * 2)..]) : 0));
            }
        });
    }

    [Test]
    public void CopyToCopiesTheValuesToABufferedSpan()
    {
        Assert.Multiple(() =>
        {
            Span<byte> buf1 = stackalloc byte[] { 04, 00, 05, 00, 06, 00 };
            Span<byte> buf2 = stackalloc byte[10];

            var span1 = byteToShortFactory.Build(buf1);
            var span2 = byteToShortFactory.Build(buf2);
            span1.CopyTo(span2);

            for (var i = 0; i < buf2.Length; i++)
                Assert.That(buf2[i], Is.EqualTo(i < buf1.Length ? buf1[i] : 0), $"buf2[{i}]");

            for (var i = 0; i < span2.Length; i++)
                Assert.That(span2[i], Is.EqualTo(i < span1.Length ? span1[i] : 0), $"span2[{i}]");
        });
    }

    [Test]
    public void TryCopyToCopiesTheValuesToASpanAndReturnsTrue()
    {
        Assert.Multiple(() =>
        {
            Span<byte> buf1 = stackalloc byte[] { 04, 00, 05, 00, 06, 00 };
            Span<short> buf2 = stackalloc short[5];

            var span = byteToShortFactory.Build(buf1);
            Assert.That(span.TryCopyTo(buf2), Is.True);

            for (var i = 0; i < buf2.Length; i++)
            {
                Assert.That(buf2[i], Is.EqualTo(i < span.Length ? BitConverter.ToInt16(buf1[(i * 2)..]) : 0));
                Assert.That(buf2[i], Is.EqualTo(i < span.Length ? span[i] : 0));
            }
        });
    }

    [Test]
    public void TryCopyToCopiesTheValuesToAConvertingSpanAndReturnsTrue()
    {
        Assert.Multiple(() =>
        {
            Span<byte> buf1 = stackalloc byte[] { 04, 00, 05, 00, 06, 00 };
            Span<short> buf2 = stackalloc short[5];

            var span1 = byteToShortFactory.Build(buf1);
            var span2 = negationFactory.Build(buf2);
            Assert.That(span1.TryCopyTo(span2), Is.True);

            for (var i = 0; i < buf2.Length; i++)
            {
                Assert.That(span2[i], Is.EqualTo(i < span1.Length ? span1[i] : 0));
                Assert.That(buf2[i], Is.EqualTo(i < buf1.Length / 2 ? -BitConverter.ToInt16(buf1[(i * 2)..]) : 0));
            }
        });
    }

    [Test]
    public void TryCopyToCopiesTheValuesToABufferedSpanAndReturnsTrue()
    {
        Assert.Multiple(() =>
        {
            Span<byte> buf1 = stackalloc byte[] { 04, 00, 05, 00, 06, 00 };
            Span<byte> buf2 = stackalloc byte[10];

            var span1 = byteToShortFactory.Build(buf1);
            var span2 = byteToShortFactory.Build(buf2);
            Assert.That(span1.TryCopyTo(span2), Is.True);

            for (var i = 0; i < buf2.Length; i++)
                Assert.That(buf2[i], Is.EqualTo(i < buf1.Length ? buf1[i] : 0), $"buf2[{i}]");

            for (var i = 0; i < span2.Length; i++)
                Assert.That(span2[i], Is.EqualTo(i < span1.Length ? span1[i] : 0), $"span2[{i}]");
        });
    }

    [Test]
    public void ToArrayCopiesASpanIntoANewArray()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg10ToPos10AsShortSpan();
            var array = span.ToArray();

            for (int i = 0, j = -10; j <= 10; j++, i++)
                Assert.That(array[i], Is.EqualTo(j));
        });
    }

    [Test]
    public void ToArrayOnAnEmptySpanReturnsEmptyArray()
    {
        var array = BufferedSpan<byte, short>.Empty.ToArray();
        Assert.That(array, Is.EqualTo(Array.Empty<short>()));
    }

    [Test]
    public void ForEachReturnsTheRightValues()
    {
        Assert.Multiple(() =>
        {
            Span<byte> buf = stackalloc byte[] { 00, 00, 01, 00, 02, 00, 03, 00, 04, 00, 05, 00 };
            var span = byteToShortFactory.Build(buf);

            var j = 0;
            foreach (var i in span)
                Assert.That(i, Is.EqualTo(j++));
        });
    }

    [Test]
    public unsafe void IndexGivesProperResultWhenCreatingWithPointerTest()
    {
        Assert.Multiple(() =>
        {
            fixed (void* ptr = &neg25ToPos25AsShort[0])
            {
                var span = byteToShortFactory.Build(ptr, 102);

                Assert.That(span[^1], Is.EqualTo(25));
                Assert.That(span[^2], Is.EqualTo(24));
                Assert.That(span[^50], Is.EqualTo(-24));
                Assert.That(span[^51], Is.EqualTo(-25));
            }
        });
    }

    [Test]
    public void BufferedSpanDownCastsToReadOnlyBufferedSpan()
    {
        Span<byte> buf = stackalloc byte[10];

        var span1 = new BufferedSpan<byte, short>(buf, BitConverter.ToInt16, BitConverter.GetBytes);
        var span2 = new ReadOnlyBufferedSpan<byte, short>(buf, BitConverter.ToInt16);

        Assert.That(span1 == span2);
    }

    private BufferedSpan<byte, short> GetNeg25ToPos25AsShortSpan() =>
        byteToShortFactory.Build(neg25ToPos25AsShort);

    private BufferedSpan<byte, short> GetNeg10ToPos10AsShortSpan() =>
        byteToShortFactory.Build(neg25ToPos25AsShort, 30, 42);
}
