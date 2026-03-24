using System;

namespace matchmaking.Domain.Entities;

public class Recommendation
{
    public int RecommendationId { get; set; }
    public int UserId { get; set; }
    public int JobId { get; set; }
    public DateTime Timestamp { get; set; }
}
