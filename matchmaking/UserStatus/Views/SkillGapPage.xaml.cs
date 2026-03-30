using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.UserStatus.Models;
using matchmaking.UserStatus.Repositories;
using matchmaking.UserStatus.Services;

namespace matchmaking.UserStatus.Views;

public sealed partial class SkillGapPage : Page
{
    private readonly SkillGapService _skillGapService;

    public SkillGapPage()
    {
        InitializeComponent();

        var connectionString = App.Configuration.SqlConnectionString;
        var matchRepo        = new UserStatusMatchRepository(connectionString);
        var skillRepo        = new SkillRepository();
        var jobSkillRepo     = new JobSkillRepository();
        var skillService     = new SkillService(skillRepo);
        var jobSkillService  = new JobSkillService(jobSkillRepo);

        _skillGapService = new SkillGapService(matchRepo, jobSkillService, skillService);

        Loaded += (_, _) => LoadDataAsync();
    }

    // ── Data Loading ────────────────────────────────────────────────────────────

    private async void LoadDataAsync()
    {
        ShowLoading();
        try
        {
            var userId      = App.Session.CurrentUserId ?? 1;
            var summary     = await Task.Run(() => _skillGapService.GetSummary(userId));
            var missing     = await Task.Run(() => _skillGapService.GetMissingSkills(userId));
            var underscored = await Task.Run(() => _skillGapService.GetUnderscoredSkills(userId));

            BuildPage(summary, missing, underscored);
        }
        catch
        {
            SummaryText.Text                = "Unable to load skill gap data. Please try again.";
            SummaryText.Foreground          = new SolidColorBrush(Colors.Red);
            SummaryMessageBorder.Visibility = Visibility.Visible;
            ShowContent();
        }
    }

    // ── Page Builder ────────────────────────────────────────────────────────────

    private void BuildPage(
        SkillGapSummaryModel summary,
        IReadOnlyList<MissingSkillModel> missing,
        IReadOnlyList<UnderscoredSkillModel> underscored)
    {
        if (!summary.HasRejections)
        {
            SummaryText.Text                = "No rejections yet — keep applying to see your skill insights.";
            SummaryMessageBorder.Visibility = Visibility.Visible;
            ShowContent();
            return;
        }

        if (!summary.HasSkillGaps)
        {
            SummaryText.Text                = "Great news — your skills meet the requirements of all jobs you've applied to.";
            SummaryMessageBorder.Visibility = Visibility.Visible;
            ShowContent();
            return;
        }

        // Stat cards
        SummaryStatsPanel.Children.Add(CreateStatCard(
            summary.MissingSkillsCount.ToString(), "Missing Skills",
            Color.FromArgb(255, 211, 47,  47),
            Color.FromArgb(255, 255, 235, 235)));

        SummaryStatsPanel.Children.Add(CreateStatCard(
            summary.SkillsToImproveCount.ToString(), "Skills to Improve",
            Color.FromArgb(255, 230, 81,   0),
            Color.FromArgb(255, 255, 243, 224)));

        SummaryStatsPanel.Visibility = Visibility.Visible;

        // Skills to Improve
        if (underscored.Count > 0)
        {
            UnderscoredSection.Visibility = Visibility.Visible;
            foreach (var skill in underscored)
                UnderscoredPanel.Children.Add(CreateUnderscoredCard(skill));
        }

        // Missing Skills
        if (missing.Count > 0)
        {
            MissingSection.Visibility = Visibility.Visible;
            foreach (var skill in missing)
                MissingPanel.Children.Add(CreateMissingCard(skill));
        }

        ShowContent();
    }

    // ── Card Builders ───────────────────────────────────────────────────────────

    private static Border CreateStatCard(string count, string label, Color accentColor, Color bgColor)
    {
        var card = new Border
        {
            Background      = new SolidColorBrush(bgColor),
            CornerRadius    = new CornerRadius(10),
            Padding         = new Thickness(24, 18, 24, 18),
            MinWidth        = 160,
            BorderBrush     = new SolidColorBrush(accentColor),
            BorderThickness = new Thickness(0, 0, 0, 3)
        };

        var stack = new StackPanel { Spacing = 4 };
        stack.Children.Add(new TextBlock
        {
            Text       = count,
            FontSize   = 40,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(accentColor)
        });
        stack.Children.Add(new TextBlock
        {
            Text       = label,
            FontSize   = 13,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80))
        });

        card.Child = stack;
        return card;
    }

    private static Border CreateUnderscoredCard(UnderscoredSkillModel skill)
    {
        var card = new Border
        {
            Background      = new SolidColorBrush(Colors.White),
            CornerRadius    = new CornerRadius(8),
            Padding         = new Thickness(18),
            BorderBrush     = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230)),
            BorderThickness = new Thickness(1)
        };

        var stack = new StackPanel { Spacing = 10 };

        // Header: skill name + gap badge
        var headerRow = new Grid();
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var nameBlock = new TextBlock
        {
            Text              = skill.SkillName,
            FontSize          = 14,
            FontWeight        = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(nameBlock, 0);

        var gap = (int)(skill.AverageRequiredScore - skill.UserScore);
        var gapBadge = new Border
        {
            Background        = new SolidColorBrush(Color.FromArgb(255, 255, 243, 224)),
            CornerRadius      = new CornerRadius(6),
            Padding           = new Thickness(8, 3, 8, 3),
            VerticalAlignment = VerticalAlignment.Center,
            Margin            = new Thickness(10, 0, 0, 0)
        };
        gapBadge.Child = new TextBlock
        {
            Text       = $"Gap: {gap} pts",
            FontSize   = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 230, 81, 0))
        };
        Grid.SetColumn(gapBadge, 1);

        headerRow.Children.Add(nameBlock);
        headerRow.Children.Add(gapBadge);
        stack.Children.Add(headerRow);

        // Progress bar
        stack.Children.Add(new ProgressBar
        {
            Value      = skill.UserScore,
            Maximum    = 100,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 151, 167)),
            Height     = 8
        });

        // Score labels
        var scoresRow = new Grid();
        scoresRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        scoresRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var yourScore = new TextBlock
        {
            Text       = $"Your score: {skill.UserScore}",
            FontSize   = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120))
        };
        Grid.SetColumn(yourScore, 0);

        var reqScore = new TextBlock
        {
            Text                = $"average required: {skill.AverageRequiredScore}",
            FontSize            = 12,
            Foreground          = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120)),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(reqScore, 1);

        scoresRow.Children.Add(yourScore);
        scoresRow.Children.Add(reqScore);
        stack.Children.Add(scoresRow);

        card.Child = stack;
        return card;
    }

    private static Border CreateMissingCard(MissingSkillModel skill)
    {
        var card = new Border
        {
            Background      = new SolidColorBrush(Colors.White),
            CornerRadius    = new CornerRadius(8),
            Padding         = new Thickness(0),
            BorderBrush     = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230)),
            BorderThickness = new Thickness(1)
        };

        var innerGrid = new Grid();
        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var accentBar = new Border
        {
            Background   = new SolidColorBrush(Color.FromArgb(255, 211, 47, 47)),
            CornerRadius = new CornerRadius(7, 0, 0, 7)
        };
        Grid.SetColumn(accentBar, 0);

        var contentStack = new StackPanel
        {
            Spacing = 4,
            Padding = new Thickness(16, 14, 16, 14)
        };
        Grid.SetColumn(contentStack, 1);

        contentStack.Children.Add(new TextBlock
        {
            Text       = skill.SkillName,
            FontSize   = 14,
            FontWeight = FontWeights.SemiBold
        });
        contentStack.Children.Add(new TextBlock
        {
            Text         = $"Required in {skill.RejectedJobCount} rejected jobs",
            FontSize     = 12,
            Foreground   = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
            TextWrapping = TextWrapping.Wrap
        });

        innerGrid.Children.Add(accentBar);
        innerGrid.Children.Add(contentStack);
        card.Child = innerGrid;
        return card;
    }

    // ── State Helpers ───────────────────────────────────────────────────────────

    private void ShowLoading()
    {
        LoadingRing.Visibility         = Visibility.Visible;
        LoadingRing.IsActive           = true;
        ContentScrollViewer.Visibility = Visibility.Collapsed;
    }

    private void ShowContent()
    {
        LoadingRing.Visibility         = Visibility.Collapsed;
        LoadingRing.IsActive           = false;
        ContentScrollViewer.Visibility = Visibility.Visible;
    }

    // ── Event Handlers ──────────────────────────────────────────────────────────

    private void BackToStatus_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
            Frame.GoBack();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        SummaryStatsPanel.Children.Clear();
        SummaryStatsPanel.Visibility    = Visibility.Collapsed;
        SummaryMessageBorder.Visibility = Visibility.Collapsed;
        UnderscoredPanel.Children.Clear();
        MissingPanel.Children.Clear();
        UnderscoredSection.Visibility = Visibility.Collapsed;
        MissingSection.Visibility     = Visibility.Collapsed;
        LoadDataAsync();
    }
}
