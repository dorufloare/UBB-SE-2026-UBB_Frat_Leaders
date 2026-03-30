namespace matchmaking.UserStatus.Models;

public class MissingSkillModel
{
    public string SkillName { get; set; } = string.Empty;
    public int RejectedJobCount { get; set; }
}
