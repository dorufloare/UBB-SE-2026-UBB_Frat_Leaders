namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlMatchRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlMatchRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void SelectMapInsertUpdateDeletePaths_WhenMatchRoundTrip_ShouldPersistAgainstDatabase()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        var insertedId = repository.InsertReturningId(new Match
        {
            UserId = 15,
            JobId = 100,
            Status = MatchStatus.Applied,
            Timestamp = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc),
            FeedbackMessage = string.Empty
        });

        var match = repository.GetById(insertedId);
        match.Should().NotBeNull();
        match!.Status.Should().Be(MatchStatus.Applied);

        match.Status = MatchStatus.Accepted;
        match.FeedbackMessage = "Great fit";
        repository.Update(match);

        var byUserAndJob = repository.GetByUserIdAndJobId(15, 100);
        byUserAndJob.Should().NotBeNull();
        byUserAndJob!.Status.Should().Be(MatchStatus.Accepted);
        byUserAndJob.FeedbackMessage.Should().Be("Great fit");

        repository.GetAll().Should().ContainSingle(item => item.MatchId == insertedId);

        repository.Remove(insertedId);
        repository.GetById(insertedId).Should().BeNull();
    }

    [Fact]
    public void Add_WhenCalled_AssignsIdentityAndPersistsRow()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        var match = new Match
        {
            UserId = 19,
            JobId = 220,
            Status = MatchStatus.Rejected,
            Timestamp = new DateTime(2026, 3, 2, 8, 0, 0, DateTimeKind.Utc),
            FeedbackMessage = "No fit"
        };

        repository.Add(match);

        match.MatchId.Should().BeGreaterThan(0);
        repository.GetById(match.MatchId)!.Status.Should().Be(MatchStatus.Rejected);
    }

    [Fact]
    public void GetById_WhenStatusIsAdvancedOrPending_MapsToExpectedEnum()
    {
        var advancedId = database.ExecuteScalar<int>(
            "INSERT INTO Matches (UserID, JobID, Status, Timestamp, Feedback) VALUES (10, 10, 'advanced', @Timestamp, NULL); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            parameters => parameters.AddWithValue("@Timestamp", new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc)));
        var pendingId = database.ExecuteScalar<int>(
            "INSERT INTO Matches (UserID, JobID, Status, Timestamp, Feedback) VALUES (11, 11, 'pending', @Timestamp, NULL); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            parameters => parameters.AddWithValue("@Timestamp", new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc)));

        var repository = new SqlMatchRepository(database.ConnectionString);

        repository.GetById(advancedId)!.Status.Should().Be(MatchStatus.Advanced);
        repository.GetById(pendingId)!.Status.Should().Be(MatchStatus.Applied);
    }

    [Fact]
    public void Add_WhenStatusIsAdvanced_PersistsAdvancedStatus()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        var match = new Match
        {
            UserId = 88,
            JobId = 99,
            Status = MatchStatus.Advanced,
            Timestamp = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc),
            FeedbackMessage = "Advance to interview"
        };

        repository.Add(match);

        repository.GetById(match.MatchId)!.Status.Should().Be(MatchStatus.Advanced);
    }
}
