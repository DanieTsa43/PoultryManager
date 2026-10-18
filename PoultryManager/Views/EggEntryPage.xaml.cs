using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class EggEntryPage : ViewModelPage
{
    public EggEntryPage(EggEntryViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
