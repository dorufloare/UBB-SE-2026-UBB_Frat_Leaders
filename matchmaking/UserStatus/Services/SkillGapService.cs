using System.Collections.Generic;
using System.Linq;
using matchmaking.Services;
using matchmaking.UserStatus.Models;
using matchmaking.UserStatus.Repositories;

namespace matchmaking.UserStatus.Services;

public class SkillGapService
{
    private readonly UserStatusMatchRepository _matchRepository;
    private readonly JobSkillService _jobSkillService;
    private readonly SkillService _skillService;

    public SkillGapService(
        UserStatusMatchRepository matchRepository,
        JobSkillService jobSkillService,
        SkillService skillService)
    {
        _matchRepository = matchRepository;
        _jobSkillService = jobSkillService;
        _skillService = skillService;
    }

    public IReadOnlyList<MissingSkillModel> GetMissingSkills(int userId)
    {
        var rejectedMatches = _matchRepository.GetRejectedByUserId(userId);
        if (rejectedMatches.Count == 0)
            return new List<MissingSkillModel>();

        var userSkillIds = _skillService.GetByUserId(userId)
            .Select(s => s.SkillId)
            .ToHashSet();

        // count how many rejected jobs required each skill the user doesn't have
        var missingCount = new Dictionary<string, int>();
        foreach (var match in rejectedMatches)
        {
            var jobSkills = _jobSkillService.GetByJobId(match.JobId);
            foreach (var jobSkill in jobSkills)
            {
                if (!userSkillIds.Contains(jobSkill.SkillId))
                {
                    if (!missingCount.ContainsKey(jobSkill.SkillName))
                        missingCount[jobSkill.SkillName] = 0;
                    missingCount[jobSkill.SkillName]++;
                }
            }
        }

        return missingCount
            .Select(kv => new MissingSkillModel { SkillName = kv.Key, RejectedJobCount = kv.Value })
            .OrderByDescending(m => m.RejectedJobCount)
            .ToList();
    }

    public IReadOnlyList<UnderscoredSkillModel> GetUnderscoredSkills(int userId)
    {
        var rejectedMatches = _matchRepository.GetRejectedByUserId(userId);
        if (rejectedMatches.Count == 0)
            return new List<UnderscoredSkillModel>();

        var userSkills = _skillService.GetByUserId(userId);
        var userSkillMap = userSkills.ToDictionary(s => s.SkillId, s => s);

        // collect required scores per skill where user score < required
        var requiredScoresPerSkill = new Dictionary<int, (string Name, int UserScore, List<int> RequiredScores)>();
        foreach (var match in rejectedMatches)
        {
            var jobSkills = _jobSkillService.GetByJobId(match.JobId);
            foreach (var jobSkill in jobSkills)
            {
                if (!userSkillMap.TryGetValue(jobSkill.SkillId, out var userSkill))
                    continue;

                if (userSkill.Score >= jobSkill.Score)
                    continue;

                if (!requiredScoresPerSkill.ContainsKey(jobSkill.SkillId))
                    requiredScoresPerSkill[jobSkill.SkillId] = (jobSkill.SkillName, userSkill.Score, new List<int>());

                requiredScoresPerSkill[jobSkill.SkillId].RequiredScores.Add(jobSkill.Score);
            }
        }

        return requiredScoresPerSkill
            .Select(kv => new UnderscoredSkillModel
            {
                SkillName = kv.Value.Name,
                UserScore = kv.Value.UserScore,
                AverageRequiredScore = (int)kv.Value.RequiredScores.Average()
            })
            .OrderByDescending(u => u.AverageRequiredScore - u.UserScore)
            .ToList();
    }

    public SkillGapSummaryModel GetSummary(int userId)
    {
        var rejectedMatches = _matchRepository.GetRejectedByUserId(userId);
        if (rejectedMatches.Count == 0)
            return new SkillGapSummaryModel { HasRejections = false, HasSkillGaps = false };

        var missing = GetMissingSkills(userId);
        var underscored = GetUnderscoredSkills(userId);
        var hasGaps = missing.Count > 0 || underscored.Count > 0;

        return new SkillGapSummaryModel
        {
            HasRejections = true,
            HasSkillGaps = hasGaps,
            MissingSkillsCount = missing.Count,
            SkillsToImproveCount = underscored.Count
        };
    }
}
