using MoreSpans;

namespace UnitTests;

[TestFixture(TestOf = typeof(ConvertingSpan<,>))]
public class ConvertingSpanTests
{
    private readonly int[] neg50ToPos50 = new int[101];

    [SetUp]
    public void Setup()
    {
        for (int i = -50, index = 0; i <= 50; i++, index++)
            neg50ToPos50[index] = i;
    }

    [Test]
    public void ConvertSpanNegativeTest()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg50ToPos50Span();

            for (int i = 0; i < span.Length; i++)
                Assert.That(span[i], Is.EqualTo(-neg50ToPos50[i]));
        });
    }

    [Test]
    public void IndexGivesProperResultTest()
    {
        Assert.Multiple(() =>
        {
            var span = GetNeg50ToPos50Span();

            Assert.That(neg50ToPos50[^1], Is.EqualTo(50));
            Assert.That(neg50ToPos50[^2], Is.EqualTo(49));
            Assert.That(neg50ToPos50[^100], Is.EqualTo(-49));
            Assert.That(neg50ToPos50[^101], Is.EqualTo(-50));

            Assert.That(span[^1], Is.EqualTo(-50));
            Assert.That(span[^2], Is.EqualTo(-49));
            Assert.That(span[^100], Is.EqualTo(49));
            Assert.That(span[^101], Is.EqualTo(50));
        });
    }

    [Test]
    public void SliceGivesProperSegmentTest()
    {
        Assert.Multiple(() =>
        {
            var subSpan1 = GetNeg50ToPos50Span()[40..^40];

            Assert.That(subSpan1.Length, Is.EqualTo(21));
            Assert.That(subSpan1[0], Is.EqualTo(10));
            Assert.That(subSpan1[^1], Is.EqualTo(-10));
        });

        Assert.Multiple(() =>
        {
            var subSpan2 = GetNeg50ToPos50Span()[10..21];

            Assert.That(subSpan2.Length, Is.EqualTo(11));
            Assert.That(subSpan2[0], Is.EqualTo(40));
            Assert.That(subSpan2[^1], Is.EqualTo(30));
        });

        Assert.Multiple(() =>
        {
            var subSpan2 = GetNeg50ToPos50Span()[..31];

            Assert.That(subSpan2.Length, Is.EqualTo(31));
            Assert.That(subSpan2[0], Is.EqualTo(50));
            Assert.That(subSpan2[^1], Is.EqualTo(20));
        });

        Assert.Multiple(() =>
        {
            var subSpan2 = GetNeg50ToPos50Span()[10..];

            Assert.That(subSpan2.Length, Is.EqualTo(91));
            Assert.That(subSpan2[0], Is.EqualTo(40));
            Assert.That(subSpan2[^1], Is.EqualTo(-50));
        });

        Assert.Multiple(() =>
        {
            var subSpan2 = GetNeg50ToPos50Span().Slice(1);

            Assert.That(subSpan2.Length, Is.EqualTo(100));
            Assert.That(subSpan2[0], Is.EqualTo(49));
            Assert.That(subSpan2[^1], Is.EqualTo(-50));
        });
    }

    [Test]
    public void AdditionGivesProperSegmentTest()
    {
        Assert.Multiple(() =>
        {
            var subSpan2 = GetNeg50ToPos50Span() + 10;

            Assert.That(subSpan2.Length, Is.EqualTo(91));
            Assert.That(subSpan2[0], Is.EqualTo(40));
            Assert.That(subSpan2[^1], Is.EqualTo(-50));
        });
    }

    [Test]
    public void IncrementGivesProperSegmentTest()
    {
        Assert.Multiple(() =>
        {
            var subSpan2 = GetNeg50ToPos50Span();
            subSpan2++;

            Assert.That(subSpan2.Length, Is.EqualTo(100));
            Assert.That(subSpan2[0], Is.EqualTo(49));
            Assert.That(subSpan2[^1], Is.EqualTo(-50));
        });
    }

    [Test]
    public void SetterConvertsProperly()
    {
        Assert.Multiple(() =>
        {
            var span = new ConvertingSpan<int, int>(neg50ToPos50, i => -i, i => i * 2);

            Assert.That(neg50ToPos50[0], Is.EqualTo(-50));
            Assert.That(neg50ToPos50[^1], Is.EqualTo(50));

            Assert.That(span[0], Is.EqualTo(50));
            Assert.That(span[^1], Is.EqualTo(-50));

            span[0] = -21;
            span[^1] = 7;

            Assert.That(span[0], Is.EqualTo(42));
            Assert.That(span[^1], Is.EqualTo(-14));
        });
    }

    [Test]
    public void EqualityWorksAsExpected()
    {
        static int func(int i) => -i;

        Assert.Multiple(() =>
        {
            var span1 = new ConvertingSpan<int, int>(neg50ToPos50, func, func);
            var span2 = span1[30..71];
            var span3 = new ConvertingSpan<int, int>(neg50ToPos50, i => i * 2, func);
            var span4 = new ConvertingSpan<int, int>(neg50ToPos50, func, i => i * 2);

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
            Assert.Throws<NotSupportedException>(() => GetNeg50ToPos50Span().Equals(null));
            Assert.Throws<NotSupportedException>(() => GetNeg50ToPos50Span().GetHashCode());
        });
    }

    [Test]
    public void IsEmptyIsFalseWhenSpanHasValues()
    {
        var span = GetNeg50ToPos50Span();
        Assert.That(span.IsEmpty, Is.False);
    }

    [Test]
    public void IsEmptyIsTrueWhenSpanHasNoValues()
    {
        var span = ConvertingSpan<int, int>.Empty;
        Assert.That(span.IsEmpty, Is.True);
    }

    [Test]
    public void CopyToThrowsExceptionWhenDestinationIsTooShort()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Span<int> buf1 = stackalloc int[10];
                Span<int> buf2 = stackalloc int[9];

                var span = new ConvertingSpan<int, int>(buf1, i => -i, i => -i);
                span.CopyTo(buf2);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Span<int> buf1 = stackalloc int[10];
                Span<int> buf2 = stackalloc int[9];

                var span1 = new ConvertingSpan<int, int>(buf1, i => -i, i => -i);
                var span2 = new ConvertingSpan<int, int>(buf2, i => -i, i => -i);

                span1.CopyTo(span2);
            });
        });
    }

    [Test]
    public void TryCopyToReturnsFalseWhenDestinationIsTooShort()
    {
        Assert.Multiple(() =>
        {
            Span<int> buf1 = stackalloc int[10];
            Span<int> buf2 = stackalloc int[9];

            var span1 = new ConvertingSpan<int, int>(buf1, i => -i, i => -i);
            var span2 = new ConvertingSpan<int, int>(buf2, i => -i, i => -i);

            Assert.That(span1.TryCopyTo(buf2), Is.False);
            Assert.That(span1.TryCopyTo(span2), Is.False);
        });
    }

    [Test]
    public void CopyToCopiesTheValuesToASpan()
    {
        Assert.Multiple(() =>
        {
            Span<int> buf1 = Enumerable.Range(4, 6).ToArray();
            Span<int> buf2 = stackalloc int[10];

            var span = new ConvertingSpan<int, int>(buf1, i => -i, i => -i);
            span.CopyTo(buf2);

            for (var i = 0; i < buf2.Length; i++)
                Assert.That(buf2[i], Is.EqualTo(i < span.Length ? span[i] : 0));
        });
    }

    [Test]
    public void CopyToCopiesTheValuesToAConvertingSpan()
    {
        Assert.Multiple(() =>
        {
            Span<int> buf1 = Enumerable.Range(4, 6).ToArray();
            Span<int> buf2 = stackalloc int[10];

            var span1 = new ConvertingSpan<int, int>(buf1, i => -i, i => -i);
            var span2 = new ConvertingSpan<int, int>(buf2, i => -i, i => -i);
            span1.CopyTo(span2);

            for (var i = 0; i < span2.Length; i++)
            {
                Assert.That(span2[i], Is.EqualTo(i < span1.Length ? span1[i] : 0));
                Assert.That(buf2[i], Is.EqualTo(i < buf1.Length ? buf1[i] : 0));
            }
        });
    }

    [Test]
    public void TryCopyToCopiesTheValuesToASpanAndReturnsTrue()
    {
        Assert.Multiple(() =>
        {
            Span<int> buf1 = Enumerable.Range(4, 6).ToArray();
            Span<int> buf2 = stackalloc int[10];

            var span = new ConvertingSpan<int, int>(buf1, i => -i, i => -i);
            Assert.That(span.TryCopyTo(buf2), Is.True);

            for (var i = 0; i < buf2.Length; i++)
                Assert.That(buf2[i], Is.EqualTo(i < span.Length ? span[i] : 0));
        });
    }

    [Test]
    public void TryCopyToCopiesTheValuesToAConvertingSpanAndReturnsTrue()
    {
        Assert.Multiple(() =>
        {
            Span<int> buf1 = Enumerable.Range(4, 6).ToArray();
            Span<int> buf2 = stackalloc int[10];

            var span1 = new ConvertingSpan<int, int>(buf1, i => -i, i => -i);
            var span2 = new ConvertingSpan<int, int>(buf2, i => -i, i => -i);
            Assert.That(span1.TryCopyTo(span2), Is.True);

            for (var i = 0; i < span2.Length; i++)
            {
                Assert.That(span2[i], Is.EqualTo(i < span1.Length ? span1[i] : 0));
                Assert.That(buf2[i], Is.EqualTo(i < buf1.Length ? buf1[i] : 0));
            }
        });
    }

    [Test]
    public void ToArrayCopiesASpanIntoANewArray()
    {
        Assert.Multiple(() =>
        {
            var tenToZero = GetNeg20ToPos20Span()[10..21];
            var array = tenToZero.ToArray();

            for (int i = 0, j = 10; j >= 0; j--, i++)
                Assert.That(array[i], Is.EqualTo(j));
        });
    }

    [Test]
    public void ToArrayOnAnEmptySpanReturnsEmptyArray()
    {
        var array = ConvertingSpan<int, int>.Empty.ToArray();
        Assert.That(array, Is.EqualTo(Array.Empty<int>()));
    }

    [Test]
    public void ForEachReturnsTheRightValues()
    {
        Assert.Multiple(() =>
        {
            Span<int> buf = Enumerable.Range(0, 10).ToArray();
            var span = new ConvertingSpan<int, int>(buf, i => i * 2, i => i * 2);

            var j = 0;
            foreach (var i in span)
                Assert.That(i, Is.EqualTo(j++ * 2));
        });
    }

    [Test]
    public unsafe void IndexGivesProperResultWhenCreatingWithPointerTest()
    {
        Assert.Multiple(() =>
        {
            fixed (void* ptr = &neg50ToPos50[0])
            {
                var span = new ConvertingSpan<int, int>(ptr, 101, i => -i, i => -i);

                Assert.That(span[^1], Is.EqualTo(-50));
                Assert.That(span[^2], Is.EqualTo(-49));
                Assert.That(span[^100], Is.EqualTo(49));
                Assert.That(span[^101], Is.EqualTo(50));
            }
        });
    }

    [Test]
    public void ConvertingSpanDownCastsToReadOnlyConvertingSpan()
    {
        Span<int> buf = stackalloc int[10];
        static int func(int i) => -i;

        var span1 = new ConvertingSpan<int, int>(buf, func, func);
        var span2 = new ReadOnlyConvertingSpan<int, int>(buf, func);

        Assert.That(span1 == span2);
    }

    private ConvertingSpan<int, int> GetNeg50ToPos50Span() =>
        new(neg50ToPos50, i => -i, i => -i);

    private ConvertingSpan<int, int> GetNeg20ToPos20Span() =>
        new(neg50ToPos50, 30, 41, i => -i, i => -i);
}