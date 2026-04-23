namespace matchmaking.Tests.Views.Converters;

public class MatchStatusDisplayConverterTests
{
    private readonly MatchStatusDisplayConverter converter = new();

    [Theory]
    [InlineData(MatchStatus.Accepted, "Accepted")]
    [InlineData(MatchStatus.Rejected, "Rejected")]
    [InlineData(MatchStatus.Advanced, "In Review")]
    [InlineData(MatchStatus.Applied, "Pending Review")]
    public void Convert_LabelMode_ReturnsExpectedLabel(MatchStatus status, string expected)
    {
        var result = converter.Convert(status, typeof(string), "Label", string.Empty);

        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_LabelMode_WithNonMatchStatus_ReturnsPendingReview()
    {
        var result = converter.Convert("not a status", typeof(string), "Label", string.Empty);

        result.Should().Be("Pending Review");
    }

    [Fact]
    public void Convert_LabelMode_WithNull_ReturnsPendingReview()
    {
        var result = converter.Convert(null, typeof(string), "Label", string.Empty);

        result.Should().Be("Pending Review");
    }

    [Theory]
    [InlineData(MatchStatus.Accepted)]
    [InlineData(MatchStatus.Rejected)]
    [InlineData(MatchStatus.Applied)]
    [InlineData(MatchStatus.Advanced)]
    public void Convert_BackgroundMode_ReturnsSolidColorBrush(MatchStatus status)
    {
        var result = converter.Convert(status, typeof(object), "Background", string.Empty);

        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
    }

    [Theory]
    [InlineData(MatchStatus.Accepted)]
    [InlineData(MatchStatus.Rejected)]
    [InlineData(MatchStatus.Applied)]
    [InlineData(MatchStatus.Advanced)]
    public void Convert_ForegroundMode_ReturnsSolidColorBrush(MatchStatus status)
    {
        var result = converter.Convert(status, typeof(object), "Foreground", string.Empty);

        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
    }

    [Fact]
    public void Convert_BackgroundMode_AcceptedAndRejected_HaveDifferentColors()
    {
        var acceptedBrush = converter.Convert(MatchStatus.Accepted, typeof(object), "Background", string.Empty) as SolidColorBrush;
        var rejectedBrush = converter.Convert(MatchStatus.Rejected, typeof(object), "Background", string.Empty) as SolidColorBrush;

        acceptedBrush!.Color.Should().NotBe(rejectedBrush!.Color);
    }

    [Fact]
    public void Convert_ForegroundMode_AcceptedAndRejected_HaveDifferentColors()
    {
        var acceptedBrush = converter.Convert(MatchStatus.Accepted, typeof(object), "Foreground", string.Empty) as SolidColorBrush;
        var rejectedBrush = converter.Convert(MatchStatus.Rejected, typeof(object), "Foreground", string.Empty) as SolidColorBrush;

        acceptedBrush!.Color.Should().NotBe(rejectedBrush!.Color);
    }

    [Fact]
    public void Convert_UnknownMode_ReturnsEmptyString()
    {
        var result = converter.Convert(MatchStatus.Accepted, typeof(string), "SomeOtherMode", string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Convert_NullMode_ReturnsEmptyString()
    {
        var result = converter.Convert(MatchStatus.Accepted, typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotSupportedException>();
    }
}
