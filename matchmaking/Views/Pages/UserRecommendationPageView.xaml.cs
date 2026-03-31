using matchmaking.Domain.Enums;
using matchmaking.Services;
using matchmaking.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace matchmaking.Views.Pages;

public sealed partial class UserRecommendationPageView : Page
{
    public UserRecommendationViewModel ViewModel { get; }

    public UserRecommendationPageView()
    {
        InitializeComponent();
        ViewModel = new UserRecommendationViewModel(
            MatchmakingComposition.CreateUserRecommendationService(App.Configuration.SqlConnectionString),
            App.Session);
        ViewModel.ErrorOccurred += OnViewModelErrorOccurred;
        DataContext = ViewModel;
        Loaded += OnLoadedAsync;
    }

    private async void OnLoadedAsync(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (App.Session.CurrentMode != AppMode.UserMode || App.Session.CurrentUserId is null)
        {
            App.Session.LoginAsUser(1);
        }

        await ViewModel.InitializeAsync();
    }

    private async void OnViewModelErrorOccurred(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }
}
