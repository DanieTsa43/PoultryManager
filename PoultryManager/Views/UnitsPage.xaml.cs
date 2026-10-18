using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class UnitsPage : ViewModelPage
{
    public UnitsPage(UnitsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
