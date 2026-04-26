using matchmaking.Models;
using System.Collections.Generic;

namespace matchmaking.Services
{
    public interface ISkillGapService
    {
        IReadOnlyList<MissingSkillModel> GetMissingSkills(int userId);
        SkillGapSummaryModel GetSummary(int userId);
        IReadOnlyList<UnderscoredSkillModel> GetUnderscoredSkills(int userId);
    }
}