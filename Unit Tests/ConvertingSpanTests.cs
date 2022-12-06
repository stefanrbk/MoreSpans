using MoreSpans;

namespace Unit_Tests;

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

    private ConvertingSpan<int, int> GetNeg50ToPos50Span() =>
        new(neg50ToPos50, i => -i, i => -i);
}