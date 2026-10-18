using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class DashboardPage : ViewModelPage
{
    public DashboardPage(DashboardViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
