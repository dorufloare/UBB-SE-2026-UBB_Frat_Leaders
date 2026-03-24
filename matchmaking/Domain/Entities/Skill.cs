namespace matchmaking.Domain.Entities;

public class Skill
{
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int Score { get; set; }
}
