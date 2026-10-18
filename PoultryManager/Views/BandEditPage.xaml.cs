using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class BandEditPage : ViewModelPage
{
    public BandEditPage(BandEditViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
