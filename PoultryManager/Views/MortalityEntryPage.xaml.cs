using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class MortalityEntryPage : ViewModelPage
{
    public MortalityEntryPage(MortalityEntryViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
