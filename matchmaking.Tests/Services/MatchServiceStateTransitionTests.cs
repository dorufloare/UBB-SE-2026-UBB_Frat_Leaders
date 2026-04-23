using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class MatchServiceStateTransitionTests
{
    [Fact]
    public void CreatePendingApplication_WhenNoExistingMatch_InsertsAppliedMatch()
    {
        var repository = new FakeMatchRepository([]);
        var service = new MatchService(repository, new FakeJobService([]));

        var createdId = service.CreatePendingApplication(1, 100);

        createdId.Should().Be(1);
        repository.InsertedMatches.Should().ContainSingle();
        repository.InsertedMatches[0].UserId.Should().Be(1);
        repository.InsertedMatches[0].JobId.Should().Be(100);
        repository.InsertedMatches[0].Status.Should().Be(MatchStatus.Applied);
        repository.InsertedMatches[0].FeedbackMessage.Should().BeEmpty();
    }

    [Fact]
    public void CreatePendingApplication_WhenExistingMatch_ThrowsInvalidOperationException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 2, userId: 1, jobId: 100, status: MatchStatus.Applied)
        ]);
        var service = new MatchService(repository, new FakeJobService([]));

        Action act = () => service.CreatePendingApplication(1, 100);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SubmitDecision_WhenRejectedWithoutFeedback_ThrowsArgumentException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 6, status: MatchStatus.Applied)
        ]);
        var service = new MatchService(repository, new FakeJobService([]));

        Action act = () => service.SubmitDecision(6, MatchStatus.Rejected, "   ");

        act.Should().Throw<ArgumentException>();
        repository.UpdatedMatches.Should().BeEmpty();
    }

    [Fact]
    public void SubmitDecision_WhenDecisionIsNotAcceptedOrRejected_ThrowsArgumentException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 7, status: MatchStatus.Applied)
        ]);
        var service = new MatchService(repository, new FakeJobService([]));

        Action act = () => service.SubmitDecision(7, MatchStatus.Advanced, "ok");

        act.Should().Throw<ArgumentException>();
        repository.UpdatedMatches.Should().BeEmpty();
    }

    [Fact]
    public void SubmitDecision_WhenTransitionNotAllowed_ThrowsInvalidOperationException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 8, status: MatchStatus.Accepted)
        ]);
        var service = new MatchService(repository, new FakeJobService([]));

        Action act = () => service.SubmitDecision(8, MatchStatus.Rejected, "No longer needed");

        act.Should().Throw<InvalidOperationException>();
        repository.UpdatedMatches.Should().BeEmpty();
    }

    [Fact]
    public void SubmitDecision_WhenValidInput_UpdatesStatusFeedbackAndTimestamp()
    {
        var match = TestDataFactory.CreateMatch(matchId: 10, status: MatchStatus.Applied, feedback: "");
        var before = match.Timestamp;
        var repository = new FakeMatchRepository([match]);
        var service = new MatchService(repository, new FakeJobService([]));

        service.SubmitDecision(10, MatchStatus.Accepted, "  Great fit  ");

        repository.UpdatedMatches.Should().ContainSingle();
        match.Status.Should().Be(MatchStatus.Accepted);
        match.FeedbackMessage.Should().Be("Great fit");
        match.Timestamp.Should().BeAfter(before);
    }

    [Fact]
    public void Advance_WhenApplied_ChangesStatusToAdvanced()
    {
        var match = TestDataFactory.CreateMatch(matchId: 11, status: MatchStatus.Applied);
        var repository = new FakeMatchRepository([match]);
        var service = new MatchService(repository, new FakeJobService([]));

        service.Advance(11);

        repository.UpdatedMatches.Should().ContainSingle();
        match.Status.Should().Be(MatchStatus.Advanced);
    }

    [Fact]
    public void Advance_WhenNotApplied_ThrowsInvalidOperationException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 12, status: MatchStatus.Rejected)
        ]);
        var service = new MatchService(repository, new FakeJobService([]));

        Action act = () => service.Advance(12);

        act.Should().Throw<InvalidOperationException>();
        repository.UpdatedMatches.Should().BeEmpty();
    }

    [Fact]
    public void RevertToApplied_WhenCalled_ResetsFeedbackAndStatus()
    {
        var match = TestDataFactory.CreateMatch(matchId: 13, status: MatchStatus.Rejected, feedback: "not enough exp");
        var repository = new FakeMatchRepository([match]);
        var service = new MatchService(repository, new FakeJobService([]));

        service.RevertToApplied(13);

        repository.UpdatedMatches.Should().ContainSingle();
        match.Status.Should().Be(MatchStatus.Applied);
        match.FeedbackMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCompanyIdAsync_WhenCompanyHasJobs_ReturnsOnlyMatchingJobsSortedByTimestamp()
    {
        var now = DateTime.UtcNow;
        var matchingOlder = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        matchingOlder.Timestamp = now.AddMinutes(-20);
        var matchingNewer = TestDataFactory.CreateMatch(matchId: 2, userId: 2, jobId: 101, status: MatchStatus.Advanced);
        matchingNewer.Timestamp = now.AddMinutes(-5);
        var otherCompany = TestDataFactory.CreateMatch(matchId: 3, userId: 3, jobId: 999, status: MatchStatus.Applied);
        otherCompany.Timestamp = now.AddMinutes(-1);

        var repository = new FakeMatchRepository([matchingOlder, matchingNewer, otherCompany]);
        IReadOnlyList<Job> jobs = [TestDataFactory.CreateJob(jobId: 100, companyId: 1), TestDataFactory.CreateJob(jobId: 101, companyId: 1)];
        var service = new MatchService(repository, new FakeJobService(jobs));

        var result = await service.GetByCompanyIdAsync(1);

        result.Select(item => item.MatchId).Should().Equal(2, 1);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_WhenCompanyHasNoJobs_ReturnsEmptyList()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied)
        ]);
        var service = new MatchService(repository, new FakeJobService([]));

        var result = await service.GetByCompanyIdAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void RemoveApplication_DelegatesToRepository()
    {
        var repository = new FakeMatchRepository([]);
        var service = new MatchService(repository, new FakeJobService([]));

        service.RemoveApplication(55);

        repository.RemovedIds.Should().ContainSingle().Which.Should().Be(55);
    }

    [Fact]
    public void IsDecisionTransitionAllowed_RespectsDefinedTransitions()
    {
        var service = new MatchService(new FakeMatchRepository([]), new FakeJobService([]));
        var applied = TestDataFactory.CreateMatch(status: MatchStatus.Applied);
        var advanced = TestDataFactory.CreateMatch(status: MatchStatus.Advanced);
        var accepted = TestDataFactory.CreateMatch(status: MatchStatus.Accepted);

        service.IsDecisionTransitionAllowed(applied, MatchStatus.Accepted).Should().BeTrue();
        service.IsDecisionTransitionAllowed(applied, MatchStatus.Rejected).Should().BeTrue();
        service.IsDecisionTransitionAllowed(applied, MatchStatus.Advanced).Should().BeTrue();
        service.IsDecisionTransitionAllowed(advanced, MatchStatus.Accepted).Should().BeTrue();
        service.IsDecisionTransitionAllowed(advanced, MatchStatus.Rejected).Should().BeTrue();
        service.IsDecisionTransitionAllowed(advanced, MatchStatus.Applied).Should().BeFalse();
        service.IsDecisionTransitionAllowed(accepted, MatchStatus.Rejected).Should().BeFalse();
    }

    private sealed class FakeMatchRepository : IMatchRepository
    {
        private readonly List<Match> _matches;

        public FakeMatchRepository(IReadOnlyList<Match> matches)
        {
            _matches = matches.ToList();
        }

        public List<Match> InsertedMatches { get; } = [];
        public List<Match> UpdatedMatches { get; } = [];
        public List<int> RemovedIds { get; } = [];

        public Match? GetById(int matchId) => _matches.FirstOrDefault(item => item.MatchId == matchId);
        public IReadOnlyList<Match> GetAll() => _matches;
        public void Add(Match match) => _matches.Add(match);
        public void Update(Match match) => UpdatedMatches.Add(match);
        public void Remove(int matchId) => RemovedIds.Add(matchId);

        public int InsertReturningId(Match match)
        {
            var nextId = _matches.Count == 0 ? 1 : _matches.Max(item => item.MatchId) + 1;
            match.MatchId = nextId;
            _matches.Add(match);
            InsertedMatches.Add(match);
            return nextId;
        }

        public Match? GetByUserIdAndJobId(int userId, int jobId) =>
            _matches.FirstOrDefault(item => item.UserId == userId && item.JobId == jobId);
    }

    private sealed class FakeJobService : IJobService
    {
        private readonly IReadOnlyList<Job> _jobs;

        public FakeJobService(IReadOnlyList<Job> jobs)
        {
            _jobs = jobs;
        }

        public Job? GetById(int jobId) => _jobs.FirstOrDefault(job => job.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => _jobs;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => _jobs.Where(job => job.CompanyId == companyId).ToList();
        public void Add(Job job) { }
        public void Update(Job job) { }
        public void Remove(int jobId) { }
    }
}
