using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class FarmInfoPage : ViewModelPage
{
    public FarmInfoPage(FarmInfoViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
