using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class HistoryPage : ViewModelPage
{
    public HistoryPage(HistoryViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
