namespace matchmaking.Tests.Views.Converters;

public class BoolToActiveBrushConverterTests
{
    private readonly BoolToActiveBrushConverter converter = new();

    [Fact]
    public void Convert_True_ReturnsSolidColorBrush()
    {
        var result = converter.Convert(true, typeof(object), null, string.Empty);

        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
    }

    [Fact]
    public void Convert_False_ReturnsSolidColorBrush()
    {
        var result = converter.Convert(false, typeof(object), null, string.Empty);

        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
    }

    [Fact]
    public void Convert_TrueAndFalse_ReturnDifferentColors()
    {
        var activeBrush = converter.Convert(true, typeof(object), null, string.Empty) as SolidColorBrush;
        var inactiveBrush = converter.Convert(false, typeof(object), null, string.Empty) as SolidColorBrush;

        activeBrush!.Color.Should().NotBe(inactiveBrush!.Color);
    }

    [Fact]
    public void Convert_NonBoolValue_ReturnsSolidColorBrush()
    {
        var result = converter.Convert("not a bool", typeof(object), null, string.Empty);

        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
    }

    [Fact]
    public void Convert_NullValue_ReturnsSolidColorBrush()
    {
        var result = converter.Convert(null, typeof(object), null, string.Empty);

        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotSupportedException>();
    }
}
