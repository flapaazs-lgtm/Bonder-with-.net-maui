using Bonder.ViewModels;

namespace Bonder.Views;

public partial class GenreSelectionPage : ContentPage
{
    public GenreSelectionPage(GenreSelectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}