using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace matchmaking.Tests;

[Collection("AppState")]
public class UserRecommendationViewModelTests : IDisposable
{
    private readonly bool originalDb;
    private readonly AppMode originalMode;
    private readonly int? originalUserId;
    private readonly FakeRecommendationService recommendationService = new();
    private readonly UserRecommendationViewModel vm;

    public UserRecommendationViewModelTests()
    {
        originalDb = App.IsDatabaseConnectionAvailable;
        originalMode = App.Session.CurrentMode;
        originalUserId = App.Session.CurrentUserId;
        SetDatabaseAvailable(true);
        App.Session.LoginAsUser(1);
        vm = new UserRecommendationViewModel(recommendationService, App.Session);
    }

    public void Dispose()
    {
        SetDatabaseAvailable(originalDb);
        if (originalUserId is { } id && originalMode == AppMode.UserMode)
        {
            App.Session.LoginAsUser(id);
        }
        else
        {
            App.Session.Logout();
        }
    }

    [Fact]
    public void LoadRecommendations_reports_an_error_when_the_database_is_not_available()
    {
        SetDatabaseAvailable(false);
        SetDatabaseError("Database is not available.");
        var errors = new List<string>();
        vm.ErrorOccurred += errors.Add;

        vm.LoadRecommendations();

        vm.HasError.Should().BeTrue();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void LoadRecommendations_reports_an_error_when_session_is_not_in_user_mode()
    {
        App.Session.LoginAsCompany(99);
        var errors = new List<string>();
        vm.ErrorOccurred += errors.Add;

        vm.LoadRecommendations();

        vm.ErrorMessage.Should().NotBeEmpty();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void LoadRecommendations_sets_CurrentJob_from_the_service()
    {
        var card = MakeCard(1);
        recommendationService.NextCard = card;

        vm.LoadRecommendations();

        vm.CurrentJob.Should().BeSameAs(card);
        vm.HasCard.Should().BeTrue();
    }

    [Fact]
    public void LoadRecommendations_falls_back_to_recalculate_when_first_call_returns_null()
    {
        recommendationService.NextCard = null;
        var fallback = MakeCard(2);
        recommendationService.RecalculatedCard = fallback;

        vm.LoadRecommendations();

        recommendationService.RecalculateCalls.Should().Be(1);
        vm.CurrentJob.Should().BeSameAs(fallback);
    }

    [Fact]
    public void Loading_a_card_enables_the_like_and_dismiss_commands()
    {
        recommendationService.NextCard = MakeCard(1);
        vm.LikeCommand.CanExecute(null).Should().BeFalse();

        vm.LoadRecommendations();

        vm.LikeCommand.CanExecute(null).Should().BeTrue();
        vm.DismissCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task ApplyFiltersAsync_copies_draft_selections_into_applied_filters()
    {
        foreach (var item in vm.DraftEmploymentSelections)
        {
            if (item.Label == "Internship")
            {
                item.IsChecked = true;
            }
        }
        vm.DraftLocation = "  Cluj  ";

        await vm.ApplyFiltersAsync();

        recommendationService.LastFilters.Should().NotBeNull();
        recommendationService.LastFilters!.EmploymentTypes.Should().Contain("Internship");
        recommendationService.LastFilters.LocationSubstring.Should().Be("Cluj");
        vm.IsFilterOpen.Should().BeFalse();
    }

    [Fact]
    public void ResetDraftFilters_clears_all_draft_items_and_location()
    {
        foreach (var item in vm.DraftEmploymentSelections)
        {
            item.IsChecked = true;
        }
        foreach (var item in vm.DraftExperienceSelections)
        {
            item.IsChecked = true;
        }
        vm.DraftLocation = "Bucharest";

        vm.ResetDraftFilters();

        vm.DraftEmploymentSelections.Should().OnlyContain(item => !item.IsChecked);
        vm.DraftExperienceSelections.Should().OnlyContain(item => !item.IsChecked);
        vm.DraftLocation.Should().BeEmpty();
    }

    [Fact]
    public void ErrorOccurred_event_carries_the_same_message_as_ErrorMessage()
    {
        SetDatabaseAvailable(false);
        SetDatabaseError("Database is not available.");
        string? captured = null;
        vm.ErrorOccurred += msg => captured = msg;

        vm.LoadRecommendations();

        captured.Should().Be(vm.ErrorMessage);
    }

    [Fact]
    public async Task LikeAsync_calls_ApplyLike_and_advances_to_the_next_card()
    {
        var first = MakeCard(1);
        var second = MakeCard(2);
        recommendationService.NextCard = first;
        vm.LoadRecommendations();
        recommendationService.NextCard = second;

        await vm.LikeAsync();

        recommendationService.AppliedLikeJobId.Should().Be(1);
        vm.CurrentJob.Should().BeSameAs(second);
        vm.CanUndo.Should().BeTrue();
    }

    [Fact]
    public async Task DismissAsync_calls_ApplyDismiss_and_advances_to_the_next_card()
    {
        var first = MakeCard(1);
        var second = MakeCard(2);
        recommendationService.NextCard = first;
        vm.LoadRecommendations();
        recommendationService.NextCard = second;

        await vm.DismissAsync();

        recommendationService.AppliedDismissJobId.Should().Be(1);
        vm.CurrentJob.Should().BeSameAs(second);
    }

    [Fact]
    public async Task UndoAsync_after_a_like_calls_UndoLike_on_the_service()
    {
        recommendationService.NextCard = MakeCard(1);
        recommendationService.MatchIdToReturn = 42;
        vm.LoadRecommendations();
        await vm.LikeAsync();

        await vm.UndoAsync();

        recommendationService.UndoLikeMatchId.Should().Be(42);
        vm.CanUndo.Should().BeFalse();
    }

    [Fact]
    public async Task LikeAsync_returns_quietly_when_there_is_no_current_card()
    {
        await vm.LikeAsync();

        recommendationService.AppliedLikeJobId.Should().BeNull();
        vm.HasError.Should().BeFalse();
    }

    private static void SetDatabaseAvailable(bool value) =>
        typeof(App).GetProperty(nameof(App.IsDatabaseConnectionAvailable))!.SetValue(null, value);

    private static void SetDatabaseError(string message) =>
        typeof(App).GetProperty(nameof(App.DatabaseConnectionError))!.SetValue(null, message);

    private static JobRecommendationResult MakeCard(int jobId) => new()
    {
        Job = new Job
        {
            JobId = jobId,
            JobTitle = "T",
            JobDescription = "d",
            Location = "Cluj",
            EmploymentType = "Full-time",
            CompanyId = 10
        },
        Company = new Company { CompanyId = 10, CompanyName = "Acme" }
    };

    private sealed class FakeRecommendationService : IUserRecommendationService
    {
        public JobRecommendationResult? NextCard { get; set; }
        public JobRecommendationResult? RecalculatedCard { get; set; }
        public int RecalculateCalls { get; private set; }
        public UserMatchmakingFilters? LastFilters { get; private set; }
        public int? AppliedLikeJobId { get; private set; }
        public int? AppliedDismissJobId { get; private set; }
        public int? UndoLikeMatchId { get; private set; }
        public int MatchIdToReturn { get; set; } = 1;

        public static string MapUserYearsToExperienceBucket(int yearsOfExperience) => "Entry";

        public JobRecommendationResult? GetNextCard(int userId, UserMatchmakingFilters filters)
        {
            LastFilters = filters;
            return NextCard;
        }

        public JobRecommendationResult? RecalculateTopCardIgnoringCooldown(int userId, UserMatchmakingFilters filters)
        {
            RecalculateCalls++;
            return RecalculatedCard;
        }

        public int ApplyLike(int userId, JobRecommendationResult card)
        {
            AppliedLikeJobId = card.Job.JobId;
            return MatchIdToReturn;
        }

        public int ApplyDismiss(int userId, JobRecommendationResult card)
        {
            AppliedDismissJobId = card.Job.JobId;
            return 7;
        }

        public void UndoDismiss(int dismissRecommendationId, int? displayRecommendationId) { }

        public void UndoLike(int matchId, int? displayRecommendationId)
        {
            UndoLikeMatchId = matchId;
        }
    }
}
