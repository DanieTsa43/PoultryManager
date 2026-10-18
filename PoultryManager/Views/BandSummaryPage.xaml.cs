using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class BandSummaryPage : ViewModelPage
{
    public BandSummaryPage(BandSummaryViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
