namespace matchmaking.Tests.Views.Converters;

public class PostTypeToBadgeBackgroundConverterTests
{
    private readonly PostTypeToBadgeBackgroundConverter converter = new();

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
        var jobBrush = converter.Convert(true, typeof(object), null, string.Empty) as SolidColorBrush;
        var devBrush = converter.Convert(false, typeof(object), null, string.Empty) as SolidColorBrush;

        jobBrush!.Color.Should().NotBe(devBrush!.Color);
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
