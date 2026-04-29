using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace matchmaking.Tests;

public class UserRecommendationServiceTests
{
    private readonly FakeUsers users = new();
    private readonly FakeJobs jobs = new();
    private readonly FakeUserSkills userSkills = new();
    private readonly FakeJobSkills jobSkills = new();
    private readonly FakeCompanies companies = new();
    private readonly FakeMatchService matches = new();
    private readonly FakeRecommendationRepo recommendations = new();
    private readonly FakeCooldown cooldown = new();
    private readonly FakeAlgo algorithm = new();
    private readonly UserRecommendationService service;

    public UserRecommendationServiceTests()
    {
        users.Items.Add(new User { UserId = 1, Name = "Alice", YearsOfExperience = 3 });
        companies.Items.Add(new Company { CompanyId = 10, CompanyName = "Acme" });
        service = new UserRecommendationService(
            users, jobs, userSkills, jobSkills, companies,
            matches, recommendations, cooldown, algorithm);
    }

    [Fact]
    public void GetNextCard_returns_null_when_there_are_no_jobs()
    {
        var card = service.GetNextCard(1, UserMatchmakingFilters.Empty());
        card.Should().BeNull();
    }

    [Fact]
    public void GetNextCard_returns_the_highest_scoring_job()
    {
        AddJob(100);
        AddJob(200);
        algorithm.ScoreByJobId[100] = 30.0;
        algorithm.ScoreByJobId[200] = 80.0;

        var card = service.GetNextCard(1, UserMatchmakingFilters.Empty());

        card.Should().NotBeNull();
        card!.Job.JobId.Should().Be(200);
        card.CompatibilityScore.Should().Be(80.0);
    }

    [Fact]
    public void GetNextCard_skips_jobs_the_user_already_applied_to()
    {
        AddJob(100);
        AddJob(200);
        algorithm.ScoreByJobId[100] = 90.0;
        algorithm.ScoreByJobId[200] = 50.0;
        matches.ExistingMatches[(1, 100)] = new Match { MatchId = 7, UserId = 1, JobId = 100 };

        var card = service.GetNextCard(1, UserMatchmakingFilters.Empty());

        card!.Job.JobId.Should().Be(200);
    }

    [Fact]
    public void GetNextCard_skips_jobs_currently_on_cooldown()
    {
        AddJob(100);
        AddJob(200);
        algorithm.ScoreByJobId[100] = 90.0;
        algorithm.ScoreByJobId[200] = 50.0;
        cooldown.OnCooldown.Add(100);

        var card = service.GetNextCard(1, UserMatchmakingFilters.Empty());

        card!.Job.JobId.Should().Be(200);
    }

    [Fact]
    public void Employment_type_filter_excludes_jobs_with_a_different_type()
    {
        jobs.Items.Add(MakeJob(100, employmentType: "Full-time"));
        jobs.Items.Add(MakeJob(200, employmentType: "Internship"));
        algorithm.ScoreByJobId[100] = 50.0;
        algorithm.ScoreByJobId[200] = 90.0;
        var filters = UserMatchmakingFilters.Empty();
        filters.EmploymentTypes.Add("Internship");

        var card = service.GetNextCard(1, filters);

        Assert.NotNull(card);
        Assert.Equal(200, card!.Job.JobId);
    }

    [Fact]
    public void Experience_filter_uses_user_years_to_pick_a_bucket()
    {
        AddJob(100);
        var filters = UserMatchmakingFilters.Empty();
        filters.ExperienceLevels.Add("Internship");
        var blocked = service.GetNextCard(1, filters);
        blocked.Should().BeNull();

        filters.ExperienceLevels.Clear();
        filters.ExperienceLevels.Add("Entry");
        var allowed = service.GetNextCard(1, filters);
        allowed.Should().NotBeNull();
    }

    [Fact]
    public void Location_substring_filter_is_case_insensitive_and_trimmed()
    {
        jobs.Items.Add(MakeJob(100, location: "Cluj-Napoca"));
        jobs.Items.Add(MakeJob(200, location: "Bucharest"));
        algorithm.ScoreByJobId[100] = 30.0;
        algorithm.ScoreByJobId[200] = 80.0;
        var filters = UserMatchmakingFilters.Empty();
        filters.LocationSubstring = "  cluj  ";

        var card = service.GetNextCard(1, filters);

        card!.Job.JobId.Should().Be(100);
    }

    [Fact]
    public void Skill_filter_keeps_jobs_that_share_at_least_one_skill_with_the_filter()
    {
        AddJob(100);
        AddJob(200);
        jobSkills.SkillsByJobId[100] = new List<JobSkill>
        {
            new() { JobId = 100, SkillId = 7, SkillName = "C#", Score = 70 }
        };
        jobSkills.SkillsByJobId[200] = new List<JobSkill>
        {
            new() { JobId = 200, SkillId = 9, SkillName = "Go", Score = 70 }
        };
        algorithm.ScoreByJobId[100] = 50.0;
        algorithm.ScoreByJobId[200] = 50.0;
        var filters = UserMatchmakingFilters.Empty();
        filters.SkillIds.Add(9);

        var card = service.GetNextCard(1, filters);

        card!.Job.JobId.Should().Be(200);
    }

    [Fact]
    public void GetNextCard_inserts_a_shown_recommendation_for_the_returned_job()
    {
        AddJob(100);
        algorithm.ScoreByJobId[100] = 50.0;

        var card = service.GetNextCard(1, UserMatchmakingFilters.Empty());

        recommendations.Inserted.Should().HaveCount(1);
        recommendations.Inserted[0].UserId.Should().Be(1);
        recommendations.Inserted[0].JobId.Should().Be(100);
        card!.DisplayRecommendationId.Should().NotBeNull();
    }

    [Fact]
    public void RecalculateTopCardIgnoringCooldown_ignores_cooldown_but_still_skips_existing_matches()
    {
        AddJob(100);
        AddJob(200);
        algorithm.ScoreByJobId[100] = 90.0;
        algorithm.ScoreByJobId[200] = 50.0;
        cooldown.OnCooldown.Add(100);
        matches.ExistingMatches[(1, 200)] = new Match { MatchId = 1, UserId = 1, JobId = 200 };

        var card = service.RecalculateTopCardIgnoringCooldown(1, UserMatchmakingFilters.Empty());

        card.Should().NotBeNull();
        card!.Job.JobId.Should().Be(100);
    }

    [Fact]
    public void ApplyLike_throws_when_the_user_already_applied_to_that_job()
    {
        var card = MakeCard(jobId: 100);
        matches.ExistingMatches[(1, 100)] = new Match { MatchId = 99, UserId = 1, JobId = 100 };

        var act = () => service.ApplyLike(1, card);

        act.Should().Throw<System.InvalidOperationException>();
    }

    [Fact]
    public void UndoLike_removes_the_match_and_the_display_recommendation()
    {
        service.UndoLike(matchId: 42, displayRecommendationId: 7);

        matches.RemovedApplications.Should().Contain(42);
        recommendations.Removed.Should().Contain(7);
    }

    private void AddJob(int jobId) => jobs.Items.Add(MakeJob(jobId));

    private static Job MakeJob(int jobId, string location = "Cluj", string employmentType = "Full-time") => new()
    {
        JobId = jobId,
        JobTitle = $"Job {jobId}",
        JobDescription = "desc",
        Location = location,
        EmploymentType = employmentType,
        CompanyId = 10,
        PromotionLevel = 0
    };

    private static JobRecommendationResult MakeCard(int jobId) => new()
    {
        Job = MakeJob(jobId),
        Company = new Company { CompanyId = 10, CompanyName = "Acme" }
    };

    private sealed class FakeUsers : IUserRepository
    {
        public List<User> Items { get; } = new();
        public User? GetById(int userId) => Items.FirstOrDefault(u => u.UserId == userId);
        public IReadOnlyList<User> GetAll() => Items;
        public void Add(User user) => Items.Add(user);
        public void Update(User user) { }
        public void Remove(int userId) { }
    }

    private sealed class FakeJobs : IJobRepository
    {
        public List<Job> Items { get; } = new();
        public Job? GetById(int jobId) => Items.FirstOrDefault(j => j.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => Items;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => Items.Where(j => j.CompanyId == companyId).ToList();
        public void Add(Job job) => Items.Add(job);
        public void Update(Job job) { }
        public void Remove(int jobId) { }
    }

    private sealed class FakeUserSkills : ISkillRepository
    {
        public Dictionary<int, List<Skill>> ByUser { get; } = new();
        public IReadOnlyList<Skill> GetByUserId(int userId) =>
            ByUser.TryGetValue(userId, out var skills) ? skills : new List<Skill>();
        public IReadOnlyList<Skill> GetAll() => ByUser.SelectMany(kv => kv.Value).ToList();
        public Skill? GetById(int userId, int skillId) => GetByUserId(userId).FirstOrDefault(s => s.SkillId == skillId);
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => System.Array.Empty<(int, string)>();
        public void Add(Skill skill) { }
        public void Update(Skill skill) { }
        public void Remove(int userId, int skillId) { }
    }

    private sealed class FakeJobSkills : IJobSkillRepository
    {
        public Dictionary<int, List<JobSkill>> SkillsByJobId { get; } = new();
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) =>
            SkillsByJobId.TryGetValue(jobId, out var list) ? list : new List<JobSkill>();
        public JobSkill? GetById(int jobId, int skillId) => GetByJobId(jobId).FirstOrDefault(s => s.SkillId == skillId);
        public IReadOnlyList<JobSkill> GetAll() => SkillsByJobId.SelectMany(kv => kv.Value).ToList();
        public void Add(JobSkill jobSkill) { }
        public void Update(JobSkill jobSkill) { }
        public void Remove(int jobId, int skillId) { }
    }

    private sealed class FakeCompanies : ICompanyRepository
    {
        public List<Company> Items { get; } = new();
        public Company? GetById(int companyId) => Items.FirstOrDefault(c => c.CompanyId == companyId);
        public IReadOnlyList<Company> GetAll() => Items;
        public void Add(Company company) => Items.Add(company);
        public void Update(Company company) { }
        public void Remove(int companyId) { }
    }

    private sealed class FakeMatchService : IMatchService
    {
        public Dictionary<(int UserId, int JobId), Match> ExistingMatches { get; } = new();
        public List<int> RemovedApplications { get; } = new();
        public Match? GetByUserIdAndJobId(int userId, int jobId) =>
            ExistingMatches.TryGetValue((userId, jobId), out var match) ? match : null;
        public int CreatePendingApplication(int userId, int jobId) => 1;
        public void RemoveApplication(int matchId) => RemovedApplications.Add(matchId);
        public Match? GetById(int matchId) => null;
        public IReadOnlyList<Match> GetAllMatches() => new List<Match>();
        public Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId) =>
            Task.FromResult<IReadOnlyList<Match>>(new List<Match>());
        public Task AcceptAsync(int matchId, string feedback) => Task.CompletedTask;
        public void Advance(int matchId) { }
        public bool IsDecisionTransitionAllowed(Match current, MatchStatus next) => true;
        public void Reject(int matchId, string feedback) { }
        public Task RejectAsync(int matchId, string feedback) => Task.CompletedTask;
        public void RevertToApplied(int matchId) { }
        public void SubmitDecision(int matchId, MatchStatus decision, string feedback) { }
        public Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback) => Task.CompletedTask;
    }

    private sealed class FakeRecommendationRepo : IRecommendationRepository
    {
        private int nextId = 1;
        public List<Recommendation> Inserted { get; } = new();
        public List<int> Removed { get; } = new();
        public int InsertReturningId(Recommendation recommendation)
        {
            Inserted.Add(recommendation);
            return nextId++;
        }
        public void Remove(int recommendationId) => Removed.Add(recommendationId);
        public Recommendation? GetById(int recommendationId) => null;
        public IReadOnlyList<Recommendation> GetAll() => new List<Recommendation>();
        public Recommendation? GetLatestByUserIdAndJobId(int userId, int jobId) => null;
        public void Add(Recommendation recommendation) { }
        public void Update(Recommendation recommendation) { }
    }

    private sealed class FakeCooldown : ICooldownService
    {
        public HashSet<int> OnCooldown { get; } = new();
        public bool IsOnCooldown(int userId, int jobId, System.DateTime utcNow) => OnCooldown.Contains(jobId);
    }

    private sealed class FakeAlgo : IRecommendationAlgorithm
    {
        public Dictionary<int, double> ScoreByJobId { get; } = new();
        public double CalculateCompatibilityScore(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills) =>
            ScoreByJobId.TryGetValue(job.JobId, out var score) ? score : 0.0;
        public CompatibilityBreakdown CalculateScoreBreakdown(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills) =>
            throw new System.NotImplementedException();
    }
}
