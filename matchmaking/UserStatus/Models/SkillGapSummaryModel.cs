namespace matchmaking.UserStatus.Models;

public class SkillGapSummaryModel
{
    public int MissingSkillsCount { get; set; }
    public int SkillsToImproveCount { get; set; }
    public bool HasRejections { get; set; }
    public bool HasSkillGaps { get; set; }
}
