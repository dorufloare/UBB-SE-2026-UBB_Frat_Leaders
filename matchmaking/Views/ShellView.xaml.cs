using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using matchmaking.ViewModels;
using matchmaking.Views.Pages;
using System;
namespace matchmaking.Views;

public sealed partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
        DataContext = new ShellViewModel();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var input = new NumberBox
        {
            Header = "Developer ID",
            Value = 1,
            Minimum = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline
        };

        var dialog = new ContentDialog
        {
            Title = "Log in as Developer",
            Content = input,
            PrimaryButtonText = "Continue",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();

        var devId = (int)input.Value;
        App.Session.LoginAsDeveloper(devId);

        if (ContentHostFrame.Content is null)
            ContentHostFrame.Navigate(typeof(DeveloperPage));
    }
}
