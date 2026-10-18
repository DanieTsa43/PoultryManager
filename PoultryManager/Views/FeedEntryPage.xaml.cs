using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class FeedEntryPage : ViewModelPage
{
    public FeedEntryPage(FeedEntryViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
