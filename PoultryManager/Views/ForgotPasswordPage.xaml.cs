using PoultryManager.ViewModels;
using PoultryManager.Views.Base;

namespace PoultryManager.Views;

public partial class ForgotPasswordPage : ViewModelPage
{
    public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
