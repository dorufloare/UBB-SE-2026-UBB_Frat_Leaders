using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class CooldownServiceTests
{
    [Fact]
    public void IsOnCooldown_WhenNoRecommendationExists_ReturnsFalse()
    {
        var repository = new FakeRecommendationRepository(Array.Empty<Recommendation>());
        var service = new CooldownService(repository, TimeSpan.FromHours(24));

        service.IsOnCooldown(1, 100, DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsOnCooldown_WhenRecommendationIsRecent_ReturnsTrue()
    {
        var repository = new FakeRecommendationRepository(new[]
        {
            TestDataFactory.CreateRecommendation(1, 1, 100, DateTime.UtcNow.AddHours(-1))
        });
        var service = new CooldownService(repository, TimeSpan.FromHours(24));

        service.IsOnCooldown(1, 100, DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsOnCooldown_WhenRecommendationIsOld_ReturnsFalse()
    {
        var repository = new FakeRecommendationRepository(new[]
        {
            TestDataFactory.CreateRecommendation(1, 1, 100, DateTime.UtcNow.AddDays(-2))
        });
        var service = new CooldownService(repository, TimeSpan.FromHours(24));

        service.IsOnCooldown(1, 100, DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsOnCooldown_WhenRecommendationTimestampIsExactlyAtCutoff_ReturnsFalse()
    {
        var cooldown = TimeSpan.FromHours(24);
        var utcNow = DateTime.UtcNow;
        var timestamp = utcNow - cooldown;
        var repository = new FakeRecommendationRepository(new[]
        {
            TestDataFactory.CreateRecommendation(1, 1, 100, timestamp)
        });
        var service = new CooldownService(repository, cooldown);

        service.IsOnCooldown(1, 100, utcNow).Should().BeFalse();
    }

    [Fact]
    public void IsOnCooldown_WhenTimestampKindIsUnspecified_TreatsAsLocalAndChecks()
    {
        var cooldown = TimeSpan.FromHours(24);
        var utcNow = DateTime.UtcNow;
        var unspecifiedTimestamp = new DateTime(utcNow.AddDays(-2).Ticks);
        var repository = new FakeRecommendationRepository(new[]
        {
            TestDataFactory.CreateRecommendation(1, 1, 100, unspecifiedTimestamp)
        });
        var service = new CooldownService(repository, cooldown);

        service.IsOnCooldown(1, 100, utcNow).Should().BeFalse();
    }

    private sealed class FakeRecommendationRepository : IRecommendationRepository
    {
        private readonly IReadOnlyList<Recommendation> recommendations;

        public FakeRecommendationRepository(IReadOnlyList<Recommendation> recommendations)
        {
            this.recommendations = recommendations;
        }

        public Recommendation? GetById(int recommendationId) => recommendations.FirstOrDefault(recommendation => recommendation.RecommendationId == recommendationId);
        public IReadOnlyList<Recommendation> GetAll() => recommendations;
        public void Add(Recommendation recommendation)
        {
        }

        public void Update(Recommendation recommendation)
        {
        }

        public void Remove(int recommendationId)
        {
        }

        public Recommendation? GetLatestByUserIdAndJobId(int userId, int jobId) => recommendations.Where(recommendation => recommendation.UserId == userId && recommendation.JobId == jobId).OrderByDescending(recommendation => recommendation.Timestamp).FirstOrDefault();
        public int InsertReturningId(Recommendation recommendation) => 1;
    }
}
