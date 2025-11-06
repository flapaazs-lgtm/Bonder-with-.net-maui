using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bonder.Models;

public class Book : INotifyPropertyChanged
{
    private bool _isSelected;

    public string Id { get; set; }
    public string Title { get; set; }
    public List<string> Authors { get; set; } = new();
    public string Description { get; set; }
    public List<string> Genres { get; set; } = new();
    public string CoverUrl { get; set; }
    public int? FirstPublishYear { get; set; }
    public double? Rating { get; set; }
    public int? RatingCount { get; set; }
    public string OpenLibraryKey { get; set; }
    public string Publisher { get; set; }
    public string PublishedDate { get; set; }
    public int? PageCount { get; set; }
    public string ISBN { get; set; }
    public double ReadProgress { get; set; }
    public DateTime? FinishedDate { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public string AuthorsDisplay => Authors?.Any() == true
        ? string.Join(", ", Authors)
        : "Unknown Author";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

