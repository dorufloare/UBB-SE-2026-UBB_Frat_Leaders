namespace matchmaking.Domain.Entities;

public class Post
{
    public int PostId { get; set; }
    public int DeveloperId { get; set; }
    public string Parameter { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
