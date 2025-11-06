using Bonder.ViewModels;

namespace Bonder.Views;

public partial class BookDetailsPage : ContentPage
{
    public BookDetailsPage(BookDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}