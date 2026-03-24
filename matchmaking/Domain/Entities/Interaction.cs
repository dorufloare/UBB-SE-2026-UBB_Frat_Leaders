namespace matchmaking.Domain.Entities;

public class Interaction
{
    public int InteractionId { get; set; }
    public int DeveloperId { get; set; }
    public int PostId { get; set; }
    public bool Type { get; set; }
}
