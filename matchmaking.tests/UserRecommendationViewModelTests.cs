namespace matchmaking.Tests;

[Collection("AppState")]
public sealed class UserRecommendationViewModelTests
{
    [Fact]
    public async Task InitializeAsync_WhenRecommendationExists_PopulatesCurrentJob()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var viewModel = CreateViewModel(user, new[] { job });

            await viewModel.InitializeAsync();

            viewModel.HasCard.Should().BeTrue();
            viewModel.CurrentJob.Should().NotBeNull();
            viewModel.ShowEmptyDeck.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task InitializeAsync_WhenUserSessionIsMissing_SetsErrorMessage()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            GetSession(viewModel).Logout();

            await viewModel.InitializeAsync();

            viewModel.HasError.Should().BeTrue();
            viewModel.ErrorMessage.Should().Be("User session is not available.");
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task InitializeAsync_WhenNoJobsExist_ShowsEmptyDeck()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, Array.Empty<Job>());

            await viewModel.InitializeAsync();

            viewModel.CurrentJob.Should().BeNull();
            viewModel.ShowEmptyDeck.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void LoadRecommendations_WhenDatabaseUnavailable_RaisesError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), false);
        SetAppFlag(nameof(App.DatabaseConnectionError), "Database unavailable.");

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            var raised = string.Empty;
            viewModel.ErrorOccurred += message => raised = message;

            viewModel.LoadRecommendations();

            viewModel.HasError.Should().BeTrue();
            viewModel.ErrorMessage.Should().Be("Database unavailable.");
            raised.Should().Be("Database unavailable.");
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task LikeAsync_WhenCurrentJobExists_ClearsCurrentJobAndEnablesUndo()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var viewModel = CreateViewModel(user, new[] { job });
            await viewModel.InitializeAsync();

            await viewModel.LikeAsync();

            viewModel.CanUndo.Should().BeTrue();
            viewModel.CurrentJob.Should().BeNull();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task UndoAsync_WhenLastActionWasLike_RestoresOriginalCard()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var viewModel = CreateViewModel(user, new[] { job });
            await viewModel.InitializeAsync();
            var originalCard = viewModel.CurrentJob;
            await viewModel.LikeAsync();

            await viewModel.UndoAsync();

            viewModel.CurrentJob.Should().Be(originalCard);
            viewModel.CanUndo.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task ResetDraftFilters_WhenFiltersWereSelected_ClearsAllSelections()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            viewModel.DraftEmploymentSelections[0].IsChecked = true;
            viewModel.DraftExperienceSelections[0].IsChecked = true;
            viewModel.DraftSkillSelections[0].IsChecked = true;
            viewModel.DraftLocation = "Cluj";

            viewModel.ResetDraftFilters();

            viewModel.DraftEmploymentSelections.Should().OnlyContain(item => !item.IsChecked);
            viewModel.DraftExperienceSelections.Should().OnlyContain(item => !item.IsChecked);
            viewModel.DraftSkillSelections.Should().OnlyContain(item => !item.IsChecked);
            viewModel.DraftLocation.Should().BeEmpty();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task DismissAsync_WhenCurrentJobExists_EnablesUndo()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var viewModel = CreateViewModel(user, new[] { job });
            await viewModel.InitializeAsync();

            await viewModel.DismissAsync();

            viewModel.CanUndo.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task UndoAsync_WhenLastActionWasDismiss_RestoresOriginalCard()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var viewModel = CreateViewModel(user, new[] { job });
            await viewModel.InitializeAsync();
            var originalCard = viewModel.CurrentJob;
            await viewModel.DismissAsync();

            await viewModel.UndoAsync();

            viewModel.CurrentJob.Should().Be(originalCard);
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void LoadRecommendations_WhenSessionIsNotUserMode_SetsErrorMessage()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            GetSession(viewModel).Logout();

            viewModel.LoadRecommendations();

            viewModel.HasError.Should().BeTrue();
            viewModel.ErrorMessage.Should().Be("User session is not available.");
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task ApplyFiltersAsync_WhenFiltersAreSelected_SetsFilterState()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            viewModel.DraftEmploymentSelections[0].IsChecked = true;
            viewModel.DraftExperienceSelections[0].IsChecked = true;
            viewModel.DraftSkillSelections[0].IsChecked = true;
            viewModel.DraftLocation = "Cluj";

            await viewModel.ApplyFiltersAsync();

            viewModel.IsFilterOpen.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task DismissAsync_WhenNoCurrentJob_DoesNothing()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            await viewModel.DismissAsync();

            viewModel.CurrentJob.Should().BeNull();
            viewModel.CanUndo.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task InitializeAsync_WhenFirstJobIsOnCooldown_UsesFallbackCard()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var firstJob = TestDataFactory.CreateJob(jobId: 100);
            var secondJob = TestDataFactory.CreateJob(jobId: 101);
            var viewModel = CreateViewModel(user, new[] { firstJob, secondJob });
            var recommendationRepository = GetRecommendationRepository(viewModel);
            recommendationRepository.InsertReturningId(TestDataFactory.CreateRecommendation(userId: user.UserId, jobId: firstJob.JobId, timestamp: DateTime.UtcNow));

            await viewModel.InitializeAsync();

            viewModel.CurrentJob.Should().NotBeNull();
            viewModel.CurrentJob!.Job.JobId.Should().Be(secondJob.JobId);
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task LikeAsync_WhenNoCurrentJob_DoesNothing()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            GetSession(viewModel).LoginAsCompany(1);

            await viewModel.LikeAsync();

            viewModel.ErrorMessage.Should().BeEmpty();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void ResetDraftFilters_WhenNothingIsSelected_KeepsSelectionsCleared()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            viewModel.ResetDraftFilters();

            viewModel.DraftEmploymentSelections.Should().OnlyContain(item => !item.IsChecked);
            viewModel.DraftLocation.Should().BeEmpty();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task LikeAsync_WhenSessionIsNotUserMode_RaisesError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            SetSessionMode(GetSession(viewModel), AppMode.CompanyMode);

            await viewModel.LikeAsync();

            viewModel.ErrorMessage.Should().BeEmpty();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task DismissAsync_WhenSessionIsNotUserMode_RaisesError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            await viewModel.InitializeAsync();
            SetSessionMode(GetSession(viewModel), AppMode.CompanyMode);

            await viewModel.DismissAsync();

            viewModel.ErrorMessage.Should().Be("Invalid session for this action.");
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task OpenDetailCommand_WhenExecuted_SetsIsDetailOpenTrue()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            await viewModel.InitializeAsync();

            viewModel.OpenDetailCommand.Execute(null);

            viewModel.IsDetailOpen.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task CloseDetailCommand_WhenDetailIsOpen_SetsIsDetailOpenFalse()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            await viewModel.InitializeAsync();
            viewModel.OpenDetailCommand.Execute(null);

            viewModel.CloseDetailCommand.Execute(null);

            viewModel.IsDetailOpen.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task UndoAsync_WhenNoUndoAvailable_DoesNothing()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            await viewModel.UndoAsync();

            viewModel.CanUndo.Should().BeFalse();
            viewModel.CurrentJob.Should().BeNull();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void OpenFiltersCommand_WhenExecuted_SetsIsFilterOpenTrue()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            viewModel.OpenFiltersCommand.Execute(null);

            viewModel.IsFilterOpen.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void CommandProperties_WhenAccessed_ReturnCommandInstances()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            viewModel.RefreshCommand.Should().NotBeNull();
            viewModel.LikeCommand.Should().NotBeNull();
            viewModel.DismissCommand.Should().NotBeNull();
            viewModel.UndoCommand.Should().NotBeNull();
            viewModel.ApplyFiltersCommand.Should().NotBeNull();
            viewModel.ResetFiltersCommand.Should().NotBeNull();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task InitializeAsync_WhenDatabaseUnavailable_SetsErrorAndNoCard()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), false);
        SetAppFlag(nameof(App.DatabaseConnectionError), "Database unavailable.");

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            await viewModel.InitializeAsync();

            viewModel.ErrorMessage.Should().Be("Database unavailable.");
            viewModel.CurrentJob.Should().BeNull();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void LoadRecommendations_WhenServiceThrows_RaisesError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var session = new SessionContext();
            session.LoginAsUser(1);
            var viewModel = CreateViewModelWithCustomService(
                new UserRecommendationService(
                    new FakeUserRepository(Array.Empty<User>()),
                    new FakeJobRepository(new[] { TestDataFactory.CreateJob() }),
                    new FakeSkillRepository(Array.Empty<Skill>()),
                    new FakeJobSkillRepository(Array.Empty<JobSkill>()),
                    new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany() }),
                    new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new JobService(new FakeJobRepository(new[] { TestDataFactory.CreateJob() }))),
                    new FakeRecommendationRepository(Array.Empty<Recommendation>()),
                    new CooldownService(new FakeRecommendationRepository(Array.Empty<Recommendation>()), TimeSpan.FromHours(1)),
                    new RecommendationAlgorithm()),
                session);
            var raised = string.Empty;
            viewModel.ErrorOccurred += message => raised = message;

            viewModel.LoadRecommendations();

            viewModel.HasError.Should().BeTrue();
            raised.Should().NotBeEmpty();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task LikeAsync_WhenSessionSwitchesAwayFromUserMode_RaisesError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            await viewModel.InitializeAsync();
            SetSessionMode(GetSession(viewModel), AppMode.CompanyMode);

            await viewModel.LikeAsync();

            viewModel.ErrorMessage.Should().Be("Invalid session for this action.");
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task DismissAsync_WhenServiceThrows_RaisesError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var session = new SessionContext();
            session.LoginAsUser(user.UserId);
            var throwingRepo = new ThrowingRecommendationRepository(throwOnInsert: true, throwOnRemove: true, allowFirstInsert: false);
            var job = TestDataFactory.CreateJob();
            var jobRepo = new FakeJobRepository(new[] { job });
            var viewModel = CreateViewModelWithCustomService(
                new UserRecommendationService(
                    new FakeUserRepository(new[] { user }),
                    jobRepo,
                    new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) }),
                    new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) }),
                    new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany(job.CompanyId) }),
                    new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new JobService(jobRepo)),
                    throwingRepo,
                    new CooldownService(throwingRepo, TimeSpan.FromHours(1)),
                    new RecommendationAlgorithm()),
                session);

            await viewModel.InitializeAsync();
            await viewModel.DismissAsync();

            viewModel.HasError.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void CanActDrivenCommands_WhenSessionInvalid_AreNotExecutable()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            GetSession(viewModel).Logout();

            viewModel.LikeCommand.CanExecute(null).Should().BeFalse();
            viewModel.DismissCommand.CanExecute(null).Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void LoadRecommendations_WhenDeckIsEmpty_KeepsErrorMessageEmpty()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, Array.Empty<Job>());

            viewModel.LoadRecommendations();

            viewModel.CurrentJob.Should().BeNull();
            viewModel.ErrorMessage.Should().BeEmpty();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task DismissAsync_WhenApplyDismissThrowsInAction_CatchesAndSetsError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var session = new SessionContext();
            session.LoginAsUser(user.UserId);
            var repo = new ThrowingRecommendationRepository(throwOnInsert: true, throwOnRemove: false, allowFirstInsert: true);
            var jobRepo = new FakeJobRepository(new[] { job });
            var viewModel = CreateViewModelWithCustomService(
                new UserRecommendationService(
                    new FakeUserRepository(new[] { user }),
                    jobRepo,
                    new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) }),
                    new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) }),
                    new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany(job.CompanyId) }),
                    new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new JobService(jobRepo)),
                    repo,
                    new CooldownService(repo, TimeSpan.FromHours(1)),
                    new RecommendationAlgorithm()),
                session);

            await viewModel.InitializeAsync();
            await viewModel.DismissAsync();

            viewModel.HasError.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task UndoAsync_WhenUndoOperationThrows_CatchesAndSetsError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var session = new SessionContext();
            session.LoginAsUser(user.UserId);
            var repo = new ThrowingRecommendationRepository(throwOnInsert: false, throwOnRemove: true, allowFirstInsert: false);
            var jobRepo = new FakeJobRepository(new[] { job });
            var viewModel = CreateViewModelWithCustomService(
                new UserRecommendationService(
                    new FakeUserRepository(new[] { user }),
                    jobRepo,
                    new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) }),
                    new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) }),
                    new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany(job.CompanyId) }),
                    new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new JobService(jobRepo)),
                    repo,
                    new CooldownService(repo, TimeSpan.FromHours(1)),
                    new RecommendationAlgorithm()),
                session);

            await viewModel.InitializeAsync();
            await viewModel.DismissAsync();
            await viewModel.UndoAsync();

            viewModel.HasError.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task LikeAsync_WhenApplyLikeThrows_CatchesAndSetsError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var viewModel = CreateViewModel(user, new[] { job });
            await viewModel.InitializeAsync();

            var matchRepository = GetMatchRepository(viewModel);
            matchRepository.Add(TestDataFactory.CreateMatch(matchId: 777, userId: user.UserId, jobId: job.JobId, status: MatchStatus.Applied));

            await viewModel.LikeAsync();

            viewModel.HasError.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    private static UserRecommendationViewModel CreateViewModel(User user, IReadOnlyList<Job> jobs)
    {
        var company = TestDataFactory.CreateCompany();
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90);
        var jobSkill = TestDataFactory.CreateJobSkill(jobs.Count > 0 ? jobs[0].JobId : 100, 1, "C#", 80);

        var userRepository = new FakeUserRepository(new[] { user });
        var jobRepository = new FakeJobRepository(jobs);
        var skillRepository = new FakeSkillRepository(new[] { skill });
        var jobSkillRepository = new FakeJobSkillRepository(jobs.Count == 0 ? Array.Empty<JobSkill>() : new[] { jobSkill });
        var companyRepository = new FakeCompanyRepository(new[] { company });
        var matchRepository = new FakeMatchRepository(Array.Empty<Match>());
        var recommendationRepository = new FakeRecommendationRepository(Array.Empty<Recommendation>());
        var jobService = new JobService(jobRepository);
        var matchService = new MatchService(matchRepository, jobService);
        var cooldownService = new CooldownService(recommendationRepository, TimeSpan.FromHours(24));
        var algorithm = new RecommendationAlgorithm();

        var recommendationService = new UserRecommendationService(
            userRepository,
            jobRepository,
            skillRepository,
            jobSkillRepository,
            companyRepository,
            matchService,
            recommendationRepository,
            cooldownService,
            algorithm);

        var session = new SessionContext();
        session.LoginAsUser(user.UserId);

        return new UserRecommendationViewModel(recommendationService, session);
    }

    private static UserRecommendationViewModel CreateViewModelWithCustomService(UserRecommendationService service, SessionContext session)
    {
        return new UserRecommendationViewModel(service, session);
    }

    private static T GetAppFlag<T>(string name)
    {
        return (T)typeof(App).GetProperty(name)!.GetValue(null)!;
    }

    private static void SetAppFlag<T>(string name, T value)
    {
        typeof(App).GetProperty(name)!.SetValue(null, value);
    }

    private static SessionContext GetSession(UserRecommendationViewModel viewModel)
    {
        return (SessionContext)typeof(UserRecommendationViewModel).GetField("_session", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;
    }

    private static FakeRecommendationRepository GetRecommendationRepository(UserRecommendationViewModel viewModel)
    {
        var service = typeof(UserRecommendationViewModel).GetField("_service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;
        var field = service.GetType().GetField("recommendationRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (FakeRecommendationRepository)field.GetValue(service)!;
    }

    private static IMatchRepository GetMatchRepository(UserRecommendationViewModel viewModel)
    {
        var service = typeof(UserRecommendationViewModel).GetField("_service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;
        var matchServiceField = service.GetType().GetField("matchService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var matchService = matchServiceField.GetValue(service)!;
        var repositoryField = matchService.GetType().GetField("matchRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (IMatchRepository)repositoryField.GetValue(matchService)!;
    }

    private static void SetSessionMode(SessionContext session, AppMode mode)
    {
        typeof(SessionContext).GetProperty(nameof(SessionContext.CurrentMode))!.SetValue(session, mode);
    }

    private sealed class ThrowingRecommendationRepository : IRecommendationRepository
    {
        private readonly List<Recommendation> items = new();
        private readonly bool throwOnInsert;
        private readonly bool throwOnRemove;
        private readonly bool allowFirstInsert;
        private int insertCount;

        public ThrowingRecommendationRepository(bool throwOnInsert, bool throwOnRemove, bool allowFirstInsert)
        {
            this.throwOnInsert = throwOnInsert;
            this.throwOnRemove = throwOnRemove;
            this.allowFirstInsert = allowFirstInsert;
        }

        public Recommendation? GetById(int recommendationId) => items.FirstOrDefault(item => item.RecommendationId == recommendationId);
        public IReadOnlyList<Recommendation> GetAll() => items;
        public void Add(Recommendation recommendation) => items.Add(recommendation);
        public void Update(Recommendation recommendation)
        {
        }

        public void Remove(int recommendationId)
        {
            if (throwOnRemove)
            {
                throw new InvalidOperationException("forced remove failure");
            }

            items.RemoveAll(item => item.RecommendationId == recommendationId);
        }
        public Recommendation? GetLatestByUserIdAndJobId(int userId, int jobId) => items.LastOrDefault(item => item.UserId == userId && item.JobId == jobId);
        public int InsertReturningId(Recommendation recommendation)
        {
            insertCount++;
            if (throwOnInsert && (!allowFirstInsert || insertCount > 1))
            {
                throw new InvalidOperationException("forced insert failure");
            }

            recommendation.RecommendationId = items.Count == 0 ? 1 : items.Max(item => item.RecommendationId) + 1;
            items.Add(recommendation);
            return recommendation.RecommendationId;
        }
    }
}
