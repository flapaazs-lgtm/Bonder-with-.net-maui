using Bonder.Models;
using System.Text.Json;

namespace Bonder.Services;

public interface IStorageService
{
    Task<UserPreferences> LoadUserPreferencesAsync();
    Task SaveUserPreferencesAsync(UserPreferences preferences);
    Task<List<Book>> GetLikedBooksAsync();
    Task AddToLikedBooksAsync(Book book);
    Task RemoveFromLikedBooksAsync(string bookId);
    Task<List<Book>> GetSavedBooksAsync();
    Task AddToSavedBooksAsync(Book book);
    Task<List<Book>> GetCurrentlyReadingAsync();
    Task AddToCurrentlyReadingAsync(Book book, double progress);
    Task UpdateReadingProgressAsync(string bookId, double progress);
    Task<List<Book>> GetFinishedBooksAsync();
    Task MarkAsFinishedAsync(Book book);
}

public class StorageService : IStorageService
{
    private const string PreferencesKey = "user_preferences";
    private const string LikedBooksKey = "liked_books";
    private const string SavedBooksKey = "saved_books";
    private const string CurrentlyReadingKey = "currently_reading";
    private const string FinishedBooksKey = "finished_books";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public async Task<UserPreferences> LoadUserPreferencesAsync()
    {
        await Task.CompletedTask;
        var json = Preferences.Default.Get(PreferencesKey, string.Empty);

        if (string.IsNullOrEmpty(json))
            return new UserPreferences();

        try
        {
            return JsonSerializer.Deserialize<UserPreferences>(json, _jsonOptions)
                   ?? new UserPreferences();
        }
        catch
        {
            return new UserPreferences();
        }
    }

    public async Task SaveUserPreferencesAsync(UserPreferences preferences)
    {
        await Task.CompletedTask;
        var json = JsonSerializer.Serialize(preferences, _jsonOptions);
        Preferences.Default.Set(PreferencesKey, json);
    }

    public async Task<List<Book>> GetLikedBooksAsync()
    {
        return await GetBooksFromStorageAsync(LikedBooksKey);
    }

    public async Task AddToLikedBooksAsync(Book book)
    {
        await AddBookToStorageAsync(LikedBooksKey, book);
    }

    public async Task RemoveFromLikedBooksAsync(string bookId)
    {
        await RemoveBookFromStorageAsync(LikedBooksKey, bookId);
    }

    public async Task<List<Book>> GetSavedBooksAsync()
    {
        return await GetBooksFromStorageAsync(SavedBooksKey);
    }

    public async Task AddToSavedBooksAsync(Book book)
    {
        await AddBookToStorageAsync(SavedBooksKey, book);
    }

    public async Task<List<Book>> GetCurrentlyReadingAsync()
    {
        return await GetBooksFromStorageAsync(CurrentlyReadingKey);
    }

    public async Task AddToCurrentlyReadingAsync(Book book, double progress)
    {
        book.ReadProgress = progress;
        await AddBookToStorageAsync(CurrentlyReadingKey, book);
    }

    public async Task UpdateReadingProgressAsync(string bookId, double progress)
    {
        var books = await GetCurrentlyReadingAsync();
        var book = books.FirstOrDefault(b => b.Id == bookId);

        if (book != null)
        {
            book.ReadProgress = progress;

            // If finished, move to finished books
            if (progress >= 1.0)
            {
                await RemoveBookFromStorageAsync(CurrentlyReadingKey, bookId);
                await MarkAsFinishedAsync(book);
            }
            else
            {
                await SaveBooksToStorageAsync(CurrentlyReadingKey, books);
            }
        }
    }

    public async Task<List<Book>> GetFinishedBooksAsync()
    {
        return await GetBooksFromStorageAsync(FinishedBooksKey);
    }

    public async Task MarkAsFinishedAsync(Book book)
    {
        book.ReadProgress = 1.0;
        book.FinishedDate = DateTime.UtcNow;
        await AddBookToStorageAsync(FinishedBooksKey, book);
        await RemoveBookFromStorageAsync(CurrentlyReadingKey, book.Id);
    }

    private async Task<List<Book>> GetBooksFromStorageAsync(string key)
    {
        await Task.CompletedTask;
        var json = Preferences.Default.Get(key, string.Empty);

        if (string.IsNullOrEmpty(json))
            return new List<Book>();

        try
        {
            return JsonSerializer.Deserialize<List<Book>>(json, _jsonOptions)
                   ?? new List<Book>();
        }
        catch
        {
            return new List<Book>();
        }
    }

    private async Task AddBookToStorageAsync(string key, Book book)
    {
        var books = await GetBooksFromStorageAsync(key);

        // Remove if already exists (to update)
        books.RemoveAll(b => b.Id == book.Id);

        // Add to beginning
        books.Insert(0, book);

        await SaveBooksToStorageAsync(key, books);
    }

    private async Task RemoveBookFromStorageAsync(string key, string bookId)
    {
        var books = await GetBooksFromStorageAsync(key);
        books.RemoveAll(b => b.Id == bookId);
        await SaveBooksToStorageAsync(key, books);
    }

    private async Task SaveBooksToStorageAsync(string key, List<Book> books)
    {
        await Task.CompletedTask;
        var json = JsonSerializer.Serialize(books, _jsonOptions);
        Preferences.Default.Set(key, json);
    }
}