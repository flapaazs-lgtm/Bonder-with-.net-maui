namespace Bonder.ViewModels;

public class SignInViewModel : BaseViewModel
{
    private string _email;
    private string _password;
    private bool _isLoading;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public Command SignInWithEmailCommand { get; }
    public Command SignInWithGoogleCommand { get; }
    public Command SignInWithAppleCommand { get; }

    public SignInViewModel()
    {
        SignInWithEmailCommand = new Command(async () => await SignInWithEmailAsync());
        SignInWithGoogleCommand = new Command(async () => await SignInWithGoogleAsync());
        SignInWithAppleCommand = new Command(async () => await SignInWithAppleAsync());
    }

    private async Task SignInWithEmailAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter both email and password.", "OK");
            return;
        }

        IsLoading = true;

        try
        {
            // TODO: Implement actual authentication
            await Task.Delay(1000); // Simulate API call

            // For now, just navigate to genre selection
            await Shell.Current.GoToAsync("//GenreSelection");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Sign in failed: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SignInWithGoogleAsync()
    {
        // TODO: Implement Google authentication
        await Shell.Current.DisplayAlert("Info", "Google sign in will be implemented soon.", "OK");
    }

    private async Task SignInWithAppleAsync()
    {
        // TODO: Implement Apple authentication
        await Shell.Current.DisplayAlert("Info", "Apple sign in will be implemented soon.", "OK");
    }
}