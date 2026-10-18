using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class RemindersPage : ViewModelPage
{
    public RemindersPage(RemindersViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
