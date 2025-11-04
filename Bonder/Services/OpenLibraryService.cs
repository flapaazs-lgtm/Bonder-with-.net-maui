using System.Text.Json;
using Bonder.Models;

namespace Bonder.Services;

public class OpenLibraryService : IBookService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://openlibrary.org";

    public OpenLibraryService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<Book>> SearchBooksAsync(string query, int limit = 20)
    {
        try
        {
            var url = $"{BaseUrl}/search.json?q={Uri.EscapeDataString(query)}&limit={limit}";
            var response = await _httpClient.GetStringAsync(url);

            var searchResult = JsonSerializer.Deserialize<OpenLibrarySearchResult>(response);

            return searchResult?.Docs?.Select(doc => new Book
            {
                Id = doc.Key?.Replace("/works/", "") ?? Guid.NewGuid().ToString(),
                Title = doc.Title,
                Authors = doc.AuthorName ?? new List<string>(),
                FirstPublishYear = doc.FirstPublishYear,
                CoverUrl = doc.CoverI > 0 ? $"https://covers.openlibrary.org/b/id/{doc.CoverI}-M.jpg" : null,
                OpenLibraryKey = doc.Key
            }).ToList() ?? new List<Book>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching books: {ex.Message}");
            return new List<Book>();
        }
    }

    public async Task<List<Book>> GetBooksByGenreAsync(string genre, int limit = 20)
    {
        return await SearchBooksAsync($"subject:{genre}", limit);
    }

    public async Task<Book> GetBookDetailsAsync(string bookId)
    {
        throw new NotImplementedException();
    }

    private class OpenLibrarySearchResult
    {
        public List<OpenLibraryDoc> Docs { get; set; } = new();
    }

    private class OpenLibraryDoc
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public List<string> AuthorName { get; set; }
        public int? FirstPublishYear { get; set; }
        public int CoverI { get; set; }
    }
}