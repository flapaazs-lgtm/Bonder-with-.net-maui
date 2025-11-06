using Bonder.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace Bonder.Views;

public partial class TopBooksSelectionPage : ContentPage
{
    public TopBooksSelectionPage(TopBooksSelectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}