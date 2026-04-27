using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using matchmaking.ViewModels;

namespace matchmaking.Views.Pages;

public sealed partial class SkillGapPage : Page
{
    private readonly SkillGapViewModel _vm;

    public SkillGapPage()
    {
        InitializeComponent();

        _vm = new SkillGapViewModel();
        DataContext = _vm;

        Loaded += OnLoaded;
    }

    private void BackToStatus_Click(object sender, RoutedEventArgs eventArgs)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs eventArgs)
        => _vm.Refresh();

    private void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
        _ = _vm.LoadData();
    }
}
