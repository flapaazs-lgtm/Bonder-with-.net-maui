using Bonder.Models;
using Bonder.ViewModels;
using Microsoft.Maui.Layouts;
using SwipeDirection = Microsoft.Maui.SwipeDirection;

namespace Bonder.Controls;

public class SwipeableBookCard : ContentView
{
    private readonly PanGestureRecognizer _panGesture;
    private double _startX, _startY;
    private const double SwipeThreshold = 100;
    private const double RotationFactor = 0.3;

    public static readonly BindableProperty BookProperty =
        BindableProperty.Create(nameof(Book), typeof(Book), typeof(SwipeableBookCard),
            propertyChanged: OnBookChanged);

    public Book Book
    {
        get => (Book)GetValue(BookProperty);
        set => SetValue(BookProperty, value);
    }

    public event EventHandler<SwipeEventArgs> Swiped;

    public SwipeableBookCard()
    {
        _panGesture = new PanGestureRecognizer();
        _panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(_panGesture);

        BuildCard();
    }

    private static void OnBookChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SwipeableBookCard card && newValue is Book book)
        {
            card.UpdateCardContent(book);
        }
    }

    private void BuildCard()
    {
        var card = new Frame
        {
            CornerRadius = 20,
            Padding = 0,
            HasShadow = true,
            BackgroundColor = Colors.White,
            HeightRequest = 550,
            WidthRequest = 340
        };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(400) },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        // Book Cover
        var coverImage = new Image
        {
            Aspect = Aspect.AspectFill,
            HeightRequest = 400
        };
        coverImage.SetBinding(Image.SourceProperty, new Binding("Book.CoverUrl", source: this));
        grid.Add(coverImage, 0, 0);

        // Gradient overlay for text readability
        var overlay = new BoxView
        {
            VerticalOptions = LayoutOptions.End,
            HeightRequest = 150,
            Opacity = 0.7
        };
        overlay.Background = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Transparent, Offset = 0 },
                new GradientStop { Color = Colors.Black, Offset = 1 }
            }
        };
        grid.Add(overlay, 0, 0);

        // Book Info
        var infoStack = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 8
        };

        var titleLabel = new Label
        {
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            MaxLines = 2,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        titleLabel.SetBinding(Label.TextProperty, new Binding("Book.Title", source: this));

        var authorLabel = new Label
        {
            FontSize = 16,
            TextColor = Colors.White,
            Opacity = 0.9
        };
        authorLabel.SetBinding(Label.TextProperty, new Binding("Book.AuthorsDisplay", source: this));

        var genresLayout = new FlexLayout
        {
            Wrap = FlexWrap.Wrap,
            JustifyContent = FlexJustify.Start,
            AlignItems = FlexAlignItems.Start,
            Margin = new Thickness(0, 8, 0, 0)
        };

        infoStack.Add(titleLabel);
        infoStack.Add(authorLabel);
        infoStack.Add(genresLayout);

        grid.Add(infoStack, 0, 0);
        Grid.SetRowSpan(infoStack, 1);
        infoStack.VerticalOptions = LayoutOptions.End;

        // Description section
        var descStack = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 8
        };

        var descLabel = new Label
        {
            FontSize = 14,
            TextColor = Color.FromArgb("#666666"),
            MaxLines = 3,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        descLabel.SetBinding(Label.TextProperty, new Binding("Book.Description", source: this));

        var ratingLayout = new HorizontalStackLayout
        {
            Spacing = 4
        };

        var starLabel = new Label
        {
            Text = "⭐",
            FontSize = 16
        };

        var ratingValueLabel = new Label
        {
            FontSize = 14,
            TextColor = Color.FromArgb("#666666"),
            VerticalOptions = LayoutOptions.Center
        };
        ratingValueLabel.SetBinding(Label.TextProperty,
            new Binding("Book.Rating", source: this, stringFormat: "{0:F1}"));

        ratingLayout.Add(starLabel);
        ratingLayout.Add(ratingValueLabel);

        descStack.Add(descLabel);
        descStack.Add(ratingLayout);

        grid.Add(descStack, 0, 1);

        card.Content = grid;
        Content = card;
    }

    private void UpdateCardContent(Book book)
    {
        // Genre tags are updated through binding
        if (book?.Genres != null && Content is Frame frame &&
            frame.Content is Grid grid)
        {
            var infoStack = grid.Children
                .OfType<VerticalStackLayout>()
                .FirstOrDefault(v => v.VerticalOptions == LayoutOptions.End);

            var genresLayout = infoStack?.Children.OfType<FlexLayout>().FirstOrDefault();
            if (genresLayout != null)
            {
                genresLayout.Children.Clear();
                foreach (var genre in book.Genres.Take(3))
                {
                    var tag = new Frame
                    {
                        BackgroundColor = Color.FromArgb("#D4A574"),
                        CornerRadius = 12,
                        Padding = new Thickness(12, 6),
                        Margin = new Thickness(0, 0, 8, 0),
                        HasShadow = false
                    };

                    tag.Content = new Label
                    {
                        Text = genre,
                        FontSize = 12,
                        TextColor = Colors.White,
                        FontAttributes = FontAttributes.Bold
                    };

                    genresLayout.Children.Add(tag);
                }
            }
        }
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startX = this.TranslationX;
                _startY = this.TranslationY;
                break;

            case GestureStatus.Running:
                this.TranslationX = _startX + e.TotalX;
                this.TranslationY = _startY + e.TotalY;
                this.Rotation = RotationFactor * (this.TranslationX / Width) * 20;

                // Visual feedback
                this.Opacity = 1 - (Math.Abs(this.TranslationX) / (Width * 2));
                break;

            case GestureStatus.Completed:
                var absX = Math.Abs(this.TranslationX);
                var absY = Math.Abs(this.TranslationY);

                if (absX > SwipeThreshold || absY > SwipeThreshold)
                {
                    // Determine direction
                    SwipeDirection direction;
                    if (absX > absY)
                    {
                        direction = this.TranslationX > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                    }
                    else
                    {
                        direction = this.TranslationY < 0 ? SwipeDirection.Up : SwipeDirection.Down;
                    }

                    // Animate off screen
                    var targetX = this.TranslationX > 0 ? Width * 2 : -Width * 2;
                    var targetY = this.TranslationY;

                    this.Animate("swipeOut",
                        d => {
                            this.TranslationX = d;
                            this.Opacity = 1 - Math.Abs(d) / (Width * 2);
                        },
                        this.TranslationX, targetX, 16, 250, Easing.CubicOut,
                        (d, b) => {
                            Swiped?.Invoke(this, new SwipeEventArgs(Book, direction));
                        });
                }
                else
                {
                    // Snap back
                    this.Animate("snapBack",
                        d => {
                            this.TranslationX = d;
                            this.TranslationY = _startY + (e.TotalY * d / this.TranslationX);
                            this.Rotation = RotationFactor * (d / Width) * 20;
                            this.Opacity = 1;
                        },
                        this.TranslationX, 0, 16, 200, Easing.SpringOut);
                }
                break;
        }
    }
}

public class SwipeEventArgs : EventArgs
{
    public Book Book { get; }
    public SwipeDirection Direction { get; }

    public SwipeEventArgs(Book book, SwipeDirection direction)
    {
        Book = book;
        Direction = direction;
    }
}