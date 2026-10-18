using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class SettingsPage : ViewModelPage
{
    public SettingsPage(SettingsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
