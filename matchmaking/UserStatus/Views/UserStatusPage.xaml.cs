using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.UserStatus.Models;
using matchmaking.UserStatus.Repositories;
using matchmaking.UserStatus.Services;

namespace matchmaking.UserStatus.Views;

public sealed partial class UserStatusPage : Page
{
    private readonly UserStatusService _userStatusService;
    private readonly SkillGapService _skillGapService;
    private readonly JobSkillService _jobSkillService;
    private List<ApplicationCardModel> _allApplications = new();
    private string _currentFilter = "All";

    public UserStatusPage()
    {
        InitializeComponent();

        var connectionString = App.Configuration.SqlConnectionString;
        var matchRepo = new UserStatusMatchRepository(connectionString);
        var jobRepo = new JobRepository();
        var companyRepo = new CompanyRepository();
        var skillRepo = new SkillRepository();
        var jobSkillRepo = new JobSkillRepository();

        var jobService = new JobService(jobRepo);
        var companyService = new CompanyService(companyRepo);
        var skillService = new SkillService(skillRepo);
        _jobSkillService = new JobSkillService(jobSkillRepo);

        _userStatusService = new UserStatusService(matchRepo, jobService, companyService, skillService, _jobSkillService);
        _skillGapService = new SkillGapService(matchRepo, _jobSkillService, skillService);

        Loaded += (_, _) => LoadDataAsync();
        SetActiveFilter(FilterAll);
    }

    // ── Data Loading ────────────────────────────────────────────────────────────

    private async void LoadDataAsync()
    {
        ShowLoading();
        try
        {
            var userId = App.Session.CurrentUserId ?? 1;

            var applications = await Task.Run(() => _userStatusService.GetApplicationsForUser(userId));
            var summary      = await Task.Run(() => _skillGapService.GetSummary(userId));
            var missing      = await Task.Run(() => _skillGapService.GetMissingSkills(userId));
            var underscored  = await Task.Run(() => _skillGapService.GetUnderscoredSkills(userId));

            _allApplications = applications.ToList();
            ApplyFilter(_currentFilter);
            BuildSkillGapPanel(summary, missing, underscored);
        }
        catch
        {
            ShowError();
        }
    }

    // ── State Helpers ───────────────────────────────────────────────────────────

    private void ShowLoading()
    {
        LoadingRing.Visibility    = Visibility.Visible;
        LoadingRing.IsActive      = true;
        CardsScrollViewer.Visibility = Visibility.Collapsed;
        EmptyState.Visibility     = Visibility.Collapsed;
        ErrorText.Visibility      = Visibility.Collapsed;
    }

    private void ShowCards()
    {
        LoadingRing.Visibility    = Visibility.Collapsed;
        LoadingRing.IsActive      = false;
        CardsScrollViewer.Visibility = Visibility.Visible;
        EmptyState.Visibility     = Visibility.Collapsed;
        ErrorText.Visibility      = Visibility.Collapsed;
    }

    private void ShowEmpty(string message = "You haven't applied to any jobs yet. Head to the Recommendations page to get started.")
    {
        LoadingRing.Visibility    = Visibility.Collapsed;
        LoadingRing.IsActive      = false;
        CardsScrollViewer.Visibility = Visibility.Collapsed;
        EmptyState.Visibility     = Visibility.Visible;
        ErrorText.Visibility      = Visibility.Collapsed;
        EmptyMessage.Text         = message;
        GoToRecommendationsButton.Visibility = message.Contains("Recommendations")
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ShowError()
    {
        LoadingRing.Visibility    = Visibility.Collapsed;
        LoadingRing.IsActive      = false;
        CardsScrollViewer.Visibility = Visibility.Collapsed;
        EmptyState.Visibility     = Visibility.Collapsed;
        ErrorText.Visibility      = Visibility.Visible;
    }

    // ── Filter Logic ────────────────────────────────────────────────────────────

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            _currentFilter = btn.Tag?.ToString() ?? "All";
            SetActiveFilter(btn);
            ApplyFilter(_currentFilter);
        }
    }

    private void SetActiveFilter(Button activeBtn)
    {
        foreach (var btn in new[] { FilterAll, FilterApplied, FilterAccepted, FilterRejected })
        {
            if (btn == activeBtn)
            {
                btn.Background   = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
                btn.Foreground   = new SolidColorBrush(Colors.White);
                btn.BorderBrush  = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
            }
            else
            {
                btn.Background   = new SolidColorBrush(Colors.Transparent);
                btn.Foreground   = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
                btn.BorderBrush  = new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
            }
        }
    }

    private void ApplyFilter(string filter)
    {
        var filtered = filter switch
        {
            "Applied"  => _allApplications.Where(a => a.Status == MatchStatus.Applied).ToList(),
            "Accepted" => _allApplications.Where(a => a.Status == MatchStatus.Accepted).ToList(),
            "Rejected" => _allApplications.Where(a => a.Status == MatchStatus.Rejected).ToList(),
            _          => _allApplications
        };

        CardsGrid.Children.Clear();
        CardsGrid.RowDefinitions.Clear();

        if (filtered.Count == 0)
        {
            if (_allApplications.Count == 0)
                ShowEmpty();
            else
                ShowEmpty("No applications match this filter.");
            return;
        }

        ShowCards();

        var rowCount = (int)Math.Ceiling(filtered.Count / 2.0);
        for (var i = 0; i < rowCount; i++)
            CardsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (var i = 0; i < filtered.Count; i++)
        {
            var card = CreateCard(filtered[i]);
            Grid.SetRow(card, i / 2);
            Grid.SetColumn(card, i % 2);
            CardsGrid.Children.Add(card);
        }
    }

    // ── Card Builder ────────────────────────────────────────────────────────────

    private Border CreateCard(ApplicationCardModel model)
    {
        var (statusText, r, g, b) = model.Status switch
        {
            MatchStatus.Accepted => ("Accepted",  76, 175,  80),
            MatchStatus.Rejected => ("Rejected", 244,  67,  54),
            _                    => ("Applied",   33, 150, 243)
        };

        var badgeBrush = new SolidColorBrush(Color.FromArgb(255, (byte)r, (byte)g, (byte)b));
        var titleStr   = model.JobDescription.Length > 120
            ? model.JobDescription[..120] + "..."
            : model.JobDescription;

        var card = new Border
        {
            Background      = new SolidColorBrush(Colors.White),
            CornerRadius    = new CornerRadius(10),
            Padding         = new Thickness(20),
            BorderBrush     = new SolidColorBrush(Color.FromArgb(255, 225, 225, 225)),
            BorderThickness = new Thickness(1)
        };

        var content = new StackPanel { Spacing = 6 };

        // Title + Badge
        var headerRow = new Grid();
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleBlock = new TextBlock
        {
            Text              = titleStr,
            FontWeight        = FontWeights.Bold,
            FontSize          = 15,
            TextTrimming      = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleBlock, 0);

        var badge = new Border
        {
            Background        = badgeBrush,
            CornerRadius      = new CornerRadius(10),
            Padding           = new Thickness(9, 3, 9, 3),
            VerticalAlignment = VerticalAlignment.Center,
            Margin            = new Thickness(10, 0, 0, 0)
        };
        badge.Child = new TextBlock
        {
            Text       = statusText,
            Foreground = new SolidColorBrush(Colors.White),
            FontSize   = 11,
            FontWeight = FontWeights.SemiBold
        };
        Grid.SetColumn(badge, 1);

        headerRow.Children.Add(titleBlock);
        headerRow.Children.Add(badge);
        content.Children.Add(headerRow);

        // Company
        content.Children.Add(new TextBlock
        {
            Text       = model.CompanyName,
            FontSize   = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80))
        });

        // Applied date
        content.Children.Add(new TextBlock
        {
            Text       = $"Applied on {model.AppliedDate:dd MMM yyyy}",
            FontSize   = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160))
        });

        // Compatibility score
        content.Children.Add(new TextBlock
        {
            Text       = $"{model.CompatibilityScore}% match",
            FontSize   = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 21, 101, 192)),
            Margin     = new Thickness(0, 2, 0, 0)
        });

        // Feedback (if present)
        if (!string.IsNullOrEmpty(model.FeedbackMessage))
        {
            content.Children.Add(new TextBlock
            {
                Text         = model.FeedbackMessage,
                FontSize     = 12,
                Foreground   = new SolidColorBrush(Color.FromArgb(255, 117, 117, 117)),
                TextWrapping = TextWrapping.Wrap,
                FontStyle    = Windows.UI.Text.FontStyle.Italic,
                Margin       = new Thickness(0, 2, 0, 0)
            });
        }

        // Action buttons (vary by status)
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing     = 8,
            Margin      = new Thickness(0, 12, 0, 0)
        };

        var detailsBtn = new Button
        {
            Content      = "View Job Details",
            FontSize     = 12,
            Padding      = new Thickness(12, 6, 12, 6),
            CornerRadius = new CornerRadius(6)
        };
        detailsBtn.Click += async (_, _) => await ShowJobDetailsAsync(model);

        var skillGapBtn = new Button
        {
            Content      = "View Skill Gap",
            FontSize     = 12,
            Padding      = new Thickness(12, 6, 12, 6),
            CornerRadius = new CornerRadius(6)
        };
        skillGapBtn.Click += (_, _) => Frame.Navigate(typeof(SkillGapPage));

        buttonPanel.Children.Add(detailsBtn);
        buttonPanel.Children.Add(skillGapBtn);

        content.Children.Add(buttonPanel);
        card.Child = content;
        return card;
    }

    // ── Job Details Dialog ──────────────────────────────────────────────────────

    private async Task ShowJobDetailsAsync(ApplicationCardModel model)
    {
        var jobSkills = _jobSkillService.GetByJobId(model.JobId);

        ContentDialog? dialog = null;

        // Title row: job title on the left, close button pinned to the right
        var shortTitle = model.JobDescription.Length > 50
            ? model.JobDescription[..50] + "..."
            : model.JobDescription;

        var titleGrid = new Grid { MinWidth = 380 };
        titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleText = new TextBlock
        {
            Text              = shortTitle,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping      = TextWrapping.Wrap
        };
        Grid.SetColumn(titleText, 0);

        var closeBtn = new Button
        {
            Content           = "✕",
            Padding           = new Thickness(10, 4, 10, 4),
            VerticalAlignment = VerticalAlignment.Center,
            Margin            = new Thickness(12, 0, 0, 0)
        };
        closeBtn.Click += (_, _) => dialog?.Hide();
        Grid.SetColumn(closeBtn, 1);

        titleGrid.Children.Add(titleText);
        titleGrid.Children.Add(closeBtn);

        // Scrollable body content
        var content = new StackPanel { Spacing = 8, Width = 380 };

        content.Children.Add(new TextBlock
        {
            Text       = model.CompanyName,
            FontWeight = FontWeights.SemiBold,
            FontSize   = 14
        });

        content.Children.Add(new TextBlock
        {
            Text         = model.JobDescription,
            TextWrapping = TextWrapping.Wrap,
            FontSize     = 13
        });

        content.Children.Add(new TextBlock
        {
            Text       = $"{model.CompatibilityScore}% match",
            Foreground = new SolidColorBrush(Color.FromArgb(255, 21, 101, 192)),
            FontWeight = FontWeights.SemiBold
        });

        if (jobSkills.Count > 0)
        {
            content.Children.Add(new TextBlock
            {
                Text       = "Required Skills:",
                FontWeight = FontWeights.SemiBold,
                Margin     = new Thickness(0, 8, 0, 0)
            });

            foreach (var skill in jobSkills)
            {
                content.Children.Add(new TextBlock
                {
                    Text     = $"• {skill.SkillName}  —  min score: {skill.Score}",
                    FontSize = 13
                });
            }
        }

        dialog = new ContentDialog
        {
            Title    = titleGrid,
            Content  = new ScrollViewer { Content = content, MaxHeight = 420 },
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }

    // ── Skill Gap Panel Builder ─────────────────────────────────────────────────

    private void BuildSkillGapPanel(
        SkillGapSummaryModel summary,
        IReadOnlyList<MissingSkillModel> missing,
        IReadOnlyList<UnderscoredSkillModel> underscored)
    {
        SkillBarsPanel.Children.Clear();

        if (!summary.HasRejections)
        {
            SkillGapMessage.Text       = "No rejections yet — keep applying to see your skill insights.";
            SkillGapMessage.Visibility = Visibility.Visible;
            return;
        }

        if (!summary.HasSkillGaps)
        {
            SkillGapMessage.Text       = "Great news — your skills meet the requirements of all jobs you've applied to.";
            SkillGapMessage.Visibility = Visibility.Visible;
            return;
        }

        SkillGapMessage.Visibility = Visibility.Collapsed;

        // Summary line
        SkillBarsPanel.Children.Add(new TextBlock
        {
            Text       = $"{summary.MissingSkillsCount} missing skills · {summary.SkillsToImproveCount} skills to improve",
            FontSize   = 12,
            Foreground = new SolidColorBrush(Colors.DarkOrange),
            Margin     = new Thickness(0, 0, 0, 4)
        });

        // Skills to improve — progress bars
        if (underscored.Count > 0)
        {
            SkillBarsPanel.Children.Add(new TextBlock
            {
                Text       = "Skills to Improve:",
                FontWeight = FontWeights.SemiBold,
                FontSize   = 12,
                Margin     = new Thickness(0, 0, 0, 2)
            });

            foreach (var skill in underscored)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 2) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

                var name = new TextBlock
                {
                    Text              = skill.SkillName,
                    FontSize          = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming      = TextTrimming.CharacterEllipsis
                };
                Grid.SetColumn(name, 0);

                var bar = new ProgressBar
                {
                    Value             = skill.UserScore,
                    Maximum           = 100,
                    Foreground        = new SolidColorBrush(Colors.Black),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin            = new Thickness(4, 0, 4, 0)
                };
                Grid.SetColumn(bar, 1);

                var score = new TextBlock
                {
                    Text                = skill.UserScore.ToString(),
                    FontSize            = 12,
                    VerticalAlignment   = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                Grid.SetColumn(score, 2);

                row.Children.Add(name);
                row.Children.Add(bar);
                row.Children.Add(score);
                SkillBarsPanel.Children.Add(row);
            }
        }

        // Missing skills — text list
        if (missing.Count > 0)
        {
            SkillBarsPanel.Children.Add(new TextBlock
            {
                Text       = "Missing Skills:",
                FontWeight = FontWeights.SemiBold,
                FontSize   = 12,
                Margin     = new Thickness(0, 8, 0, 2)
            });

            foreach (var skill in missing)
            {
                SkillBarsPanel.Children.Add(new TextBlock
                {
                    Text       = $"• {skill.SkillName} — required in {skill.RejectedJobCount} rejected job(s)",
                    FontSize   = 12,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 211, 47, 47)),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }
    }

    // ── Event Handlers ──────────────────────────────────────────────────────────

    private void RefreshButton_Click(object sender, RoutedEventArgs e) => LoadDataAsync();

    private void SkillInsightsButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SkillGapPage));
    }
}
