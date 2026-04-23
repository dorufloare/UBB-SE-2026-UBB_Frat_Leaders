namespace matchmaking.Tests.Converters;

public class MatchStatusToColorConverterTests
{
    private readonly MatchStatusToColorConverter converter = new();

    [Theory]
    [InlineData(MatchStatus.Accepted)]
    [InlineData(MatchStatus.Rejected)]
    [InlineData(MatchStatus.Applied)]
    [InlineData(MatchStatus.Advanced)]
    public void Convert_WithMatchStatus_ReturnsSolidColorBrush(MatchStatus status)
    {
        var result = converter.Convert(status, typeof(object), null, string.Empty);

        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
    }

    [Fact]
    public void Convert_WithNonMatchStatusValue_ReturnsSolidColorBrush()
    {
        var result = converter.Convert("not a status", typeof(object), null, string.Empty);

        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsSolidColorBrush()
    {
        var result = converter.Convert(null, typeof(object), null, string.Empty);

        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
    }

    [Fact]
    public void Convert_AcceptedAndRejected_ReturnDifferentColors()
    {
        var acceptedBrush = converter.Convert(MatchStatus.Accepted, typeof(object), null, string.Empty) as SolidColorBrush;
        var rejectedBrush = converter.Convert(MatchStatus.Rejected, typeof(object), null, string.Empty) as SolidColorBrush;

        acceptedBrush!.Color.Should().NotBe(rejectedBrush!.Color);
    }

    [Fact]
    public void Convert_AppliedAndAccepted_ReturnDifferentColors()
    {
        var appliedBrush = converter.Convert(MatchStatus.Applied, typeof(object), null, string.Empty) as SolidColorBrush;
        var acceptedBrush = converter.Convert(MatchStatus.Accepted, typeof(object), null, string.Empty) as SolidColorBrush;

        appliedBrush!.Color.Should().NotBe(acceptedBrush!.Color);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}
