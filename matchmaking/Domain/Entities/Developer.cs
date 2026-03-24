namespace matchmaking.Domain.Entities;

public class Developer
{
    public int DeveloperId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
