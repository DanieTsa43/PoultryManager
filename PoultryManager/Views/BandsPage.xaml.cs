using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class BandsPage : ViewModelPage
{
    public BandsPage(BandsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
