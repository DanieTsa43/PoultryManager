using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class BackupPage : ViewModelPage
{
    public BackupPage(BackupViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
