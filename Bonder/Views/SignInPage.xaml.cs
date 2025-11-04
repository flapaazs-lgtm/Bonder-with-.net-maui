using Bonder.ViewModels;

namespace Bonder.Views;

public partial class SignInPage : ContentPage
{
    public SignInPage(SignInViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnEmailButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button emailButton)
        {
            EmailForm.IsVisible = true;
            emailButton.IsVisible = false;
        }
    }

    private async void OnSignInTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert("Info", "Sign in functionality will be implemented soon.", "OK");
    }
}