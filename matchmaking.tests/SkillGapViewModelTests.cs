using System.Collections.Generic;
using System.Threading.Tasks;
using matchmaking.Models;

namespace matchmaking.Tests;

[Collection("AppState")]
public class SkillGapViewModelTests
{
    [Fact]
    public async Task LoadData_populates_collections_when_service_returns_skill_gaps()
    {
        var service = new FakeSkillGapService
        {
            Summary = new SkillGapSummaryModel
            {
                HasRejections = true,
                HasSkillGaps = true,
                MissingSkillsCount = 1,
                SkillsToImproveCount = 1
            },
            Missing = new() { new MissingSkillModel { SkillName = "SQL", RejectedJobCount = 4 } },
            Underscored = new() { new UnderscoredSkillModel { SkillName = "C#", UserScore = 40, AverageRequiredScore = 70 } }
        };
        var vm = new SkillGapViewModel(service, App.Session);

        await vm.LoadData();

        vm.HasSkillData.Should().BeTrue();
        vm.HasSummaryMessage.Should().BeFalse();
        vm.MissingSkills.Should().ContainSingle().Which.SkillName.Should().Be("SQL");
        vm.SkillsToImprove.Should().ContainSingle().Which.SkillName.Should().Be("C#");
        vm.MissingCount.Should().Be(1);
        vm.ImproveCount.Should().Be(1);
    }

    [Fact]
    public async Task LoadData_shows_no_rejections_message_when_summary_says_so()
    {
        var service = new FakeSkillGapService
        {
            Summary = new SkillGapSummaryModel { HasRejections = false, HasSkillGaps = false }
        };
        var vm = new SkillGapViewModel(service, App.Session);

        await vm.LoadData();

        vm.HasSkillData.Should().BeFalse();
        vm.HasSummaryMessage.Should().BeTrue();
        vm.SummaryMessage.Should().Contain("No rejections");
        vm.MissingSkills.Should().BeEmpty();
        vm.SkillsToImprove.Should().BeEmpty();
    }

    private sealed class FakeSkillGapService : ISkillGapService
    {
        public SkillGapSummaryModel Summary { get; set; } = new();
        public List<MissingSkillModel> Missing { get; set; } = new();
        public List<UnderscoredSkillModel> Underscored { get; set; } = new();
        public IReadOnlyList<MissingSkillModel> GetMissingSkills(int userId) => Missing;
        public SkillGapSummaryModel GetSummary(int userId) => Summary;
        public IReadOnlyList<UnderscoredSkillModel> GetUnderscoredSkills(int userId) => Underscored;
    }
}
