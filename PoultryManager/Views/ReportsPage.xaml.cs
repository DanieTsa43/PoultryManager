using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class ReportsPage : ViewModelPage
{
    public ReportsPage(ReportsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
