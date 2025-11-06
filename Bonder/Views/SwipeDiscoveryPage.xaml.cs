// SwipeDiscoveryPage.xaml.cs
using Bonder.Controls;
using Bonder.Models;
using Bonder.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace Bonder.Views;

public partial class SwipeDiscoveryPage : ContentPage
{
    private readonly SwipeDiscoveryViewModel _viewModel;
    private readonly Stack<SwipeableBookCard> _cardPool = new();

    public SwipeDiscoveryPage(SwipeDiscoveryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Subscribe to view model changes
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RenderBookCards();
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SwipeDiscoveryViewModel.BookQueue))
        {
            RenderBookCards();
        }
    }

    private void RenderBookCards()
    {
        CardStack.Children.Clear();
        _cardPool.Clear();

        var books = _viewModel.BookQueue.Take(3).Reverse().ToList();

        for (int i = 0; i < books.Count; i++)
        {
            var book = books[i];
            var card = new SwipeableBookCard
            {
                Book = book,
                ZIndex = books.Count - i
            };

            card.Swiped += OnCardSwiped;

            AbsoluteLayout.SetLayoutBounds(card, new Rect(0.5, 0.5, 340, 550));
            AbsoluteLayout.SetLayoutFlags(card, AbsoluteLayoutFlags.PositionProportional);

            // Add slight offset for depth effect
            card.TranslationY = i * 10;
            card.Scale = 1 - (i * 0.02);

            CardStack.Children.Add(card);
            _cardPool.Push(card);
        }
    }

    private void OnCardSwiped(object sender, SwipeEventArgs e)
    {
        // Convert Microsoft.Maui.SwipeDirection to Bonder.ViewModels.SwipeDirection
        var bonderDirection = (Bonder.ViewModels.SwipeDirection)(int)e.Direction;
        _viewModel.OnCardSwiped(e.Book, bonderDirection);

        // Delay before rendering next card
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
        {
            if (CardStack.Children.Count > 0)
            {
                CardStack.Children.RemoveAt(CardStack.Children.Count - 1);
            }
            RenderBookCards();
        });
    }
}
