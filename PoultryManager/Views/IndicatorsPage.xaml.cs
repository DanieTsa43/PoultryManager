using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class IndicatorsPage : ViewModelPage
{
    public IndicatorsPage(IndicatorsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
