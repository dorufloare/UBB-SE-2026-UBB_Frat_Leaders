using System;
using matchmaking.Domain.Enums;

namespace matchmaking.Domain.Entities;

public class Match
{
    public int MatchId { get; set; }
    public int UserId { get; set; }
    public int JobId { get; set; }
    public MatchStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string FeedbackMessage { get; set; } = string.Empty;
}
