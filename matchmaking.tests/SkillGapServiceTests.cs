using System.Collections.Generic;
using System.Linq;
using matchmaking.Models;

namespace matchmaking.Tests;

public class SkillGapServiceTests
{
    [Fact]
    public void GetMissingSkills_returns_empty_when_user_has_no_rejections()
    {
        var service = new SkillGapService(new FakeUserMatches(), new FakeJobSkills(), new FakeSkills());
        var missing = service.GetMissingSkills(userId: 1);
        missing.Should().BeEmpty();
    }

    [Fact]
    public void GetMissingSkills_orders_by_rejected_job_count_descending()
    {
        var skills = new FakeSkills();
        skills.SkillsByUserId[1] = new()
        {
            new Skill { UserId = 1, SkillId = 1, SkillName = "C#", Score = 80 }
        };
        var matchRepo = new FakeUserMatches();
        matchRepo.RejectedByUserId[1] = new()
        {
            new Match { MatchId = 1, UserId = 1, JobId = 100 },
            new Match { MatchId = 2, UserId = 1, JobId = 200 },
            new Match { MatchId = 3, UserId = 1, JobId = 300 }
        };
        var jobSkills = new FakeJobSkills();
        jobSkills.SkillsByJobId[100] = new() { new() { JobId = 100, SkillId = 3, SkillName = "SQL", Score = 60 } };
        jobSkills.SkillsByJobId[200] = new()
        {
            new() { JobId = 200, SkillId = 3, SkillName = "SQL", Score = 60 },
            new() { JobId = 200, SkillId = 4, SkillName = "Docker", Score = 60 }
        };
        jobSkills.SkillsByJobId[300] = new() { new() { JobId = 300, SkillId = 3, SkillName = "SQL", Score = 60 } };
        var service = new SkillGapService(matchRepo, jobSkills, skills);

        var missing = service.GetMissingSkills(1);

        missing.Should().HaveCount(2);
        missing[0].SkillName.Should().Be("SQL");
        missing[0].RejectedJobCount.Should().Be(3);
        missing[1].SkillName.Should().Be("Docker");
        missing[1].RejectedJobCount.Should().Be(1);
    }

    [Fact]
    public void GetUnderscoredSkills_skips_skills_where_the_user_already_meets_the_bar()
    {
        var skills = new FakeSkills();
        skills.SkillsByUserId[1] = new()
        {
            new Skill { UserId = 1, SkillId = 1, SkillName = "C#", Score = 40 },
            new Skill { UserId = 1, SkillId = 3, SkillName = "SQL", Score = 80 }
        };
        var matchRepo = new FakeUserMatches();
        matchRepo.RejectedByUserId[1] = new()
        {
            new Match { MatchId = 1, UserId = 1, JobId = 100 }
        };
        var jobSkills = new FakeJobSkills();
        jobSkills.SkillsByJobId[100] = new()
        {
            new() { JobId = 100, SkillId = 1, SkillName = "C#", Score = 70 },
            new() { JobId = 100, SkillId = 3, SkillName = "SQL", Score = 60 }
        };
        var service = new SkillGapService(matchRepo, jobSkills, skills);

        var underscored = service.GetUnderscoredSkills(1);

        underscored.Should().ContainSingle();
        underscored[0].SkillName.Should().Be("C#");
        underscored[0].UserScore.Should().Be(40);
        underscored[0].AverageRequiredScore.Should().Be(70);
    }

    [Fact]
    public void GetSummary_says_no_rejections_when_match_repo_returns_nothing()
    {
        var service = new SkillGapService(new FakeUserMatches(), new FakeJobSkills(), new FakeSkills());

        var summary = service.GetSummary(userId: 42);

        summary.HasRejections.Should().BeFalse();
        summary.HasSkillGaps.Should().BeFalse();
    }

    private sealed class FakeUserMatches : IUserStatusMatchRepository
    {
        public Dictionary<int, List<Match>> RejectedByUserId { get; } = new();
        public IReadOnlyList<Match> GetRejectedByUserId(int userId) =>
            RejectedByUserId.TryGetValue(userId, out var list) ? list : new List<Match>();
        public IReadOnlyList<Match> GetByUserId(int userId) =>
            RejectedByUserId.TryGetValue(userId, out var list) ? list : new List<Match>();
    }

    private sealed class FakeJobSkills : IJobSkillService
    {
        public Dictionary<int, List<JobSkill>> SkillsByJobId { get; } = new();
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) =>
            SkillsByJobId.TryGetValue(jobId, out var list) ? list : new List<JobSkill>();
        public IReadOnlyList<JobSkill> GetAll() => SkillsByJobId.SelectMany(kv => kv.Value).ToList();
        public JobSkill? GetById(int jobId, int skillId) => GetByJobId(jobId).FirstOrDefault(s => s.SkillId == skillId);
        public void Add(JobSkill jobSkill) { }
        public void Update(JobSkill jobSkill) { }
        public void Remove(int jobId, int skillId) { }
    }

    private sealed class FakeSkills : ISkillService
    {
        public Dictionary<int, List<Skill>> SkillsByUserId { get; } = new();
        public IReadOnlyList<Skill> GetByUserId(int userId) =>
            SkillsByUserId.TryGetValue(userId, out var list) ? list : new List<Skill>();
        public Skill? GetById(int userId, int skillId) => GetByUserId(userId).FirstOrDefault(s => s.SkillId == skillId);
        public IReadOnlyList<Skill> GetAll() => SkillsByUserId.SelectMany(kv => kv.Value).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => System.Array.Empty<(int, string)>();
        public void Add(Skill skill) { }
        public void Update(Skill skill) { }
        public void Remove(int userId, int skillId) { }
    }
}
