using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class ProfileEditPage : ViewModelPage
{
    public ProfileEditPage(ProfileEditViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
