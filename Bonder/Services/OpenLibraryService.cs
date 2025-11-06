using System.Text.Json;
using System.Text.Json.Serialization;
using Bonder.Models;

namespace Bonder.Services;

public class OpenLibraryService : IBookService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://openlibrary.org";
    private readonly Dictionary<string, List<Book>> _cache = new();

    public OpenLibraryService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<List<Book>> SearchBooksAsync(string query, int limit = 20)
    {
        // Check cache first
        var cacheKey = $"search_{query}_{limit}";
        if (_cache.TryGetValue(cacheKey, out var cachedBooks))
            return cachedBooks;

        try
        {
            var url = $"{BaseUrl}/search.json?q={Uri.EscapeDataString(query)}&limit={limit}";
            var response = await _httpClient.GetStringAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var searchResult = JsonSerializer.Deserialize<OpenLibrarySearchResult>(response, options);

            var books = searchResult?.Docs?.Select(doc => new Book
            {
                Id = doc.Key?.Replace("/works/", "") ?? Guid.NewGuid().ToString(),
                Title = doc.Title ?? "Unknown Title",
                Authors = doc.AuthorName ?? new List<string> { "Unknown Author" },
                FirstPublishYear = doc.FirstPublishYear,
                CoverUrl = doc.CoverI > 0
                    ? $"https://covers.openlibrary.org/b/id/{doc.CoverI}-L.jpg"
                    : GeneratePlaceholderCover(doc.Title),
                OpenLibraryKey = doc.Key,
                Genres = doc.Subject?.Take(5).ToList() ?? new List<string>(),
                Rating = GenerateRating(),
                RatingCount = Random.Shared.Next(100, 10000),
                Description = GenerateDescription(doc.Title, doc.AuthorName),
                ISBN = doc.Isbn?.FirstOrDefault(),
                PageCount = doc.NumberOfPagesMedian,
                Publisher = doc.Publisher?.FirstOrDefault()
            }).ToList() ?? new List<Book>();

            // Cache the results
            _cache[cacheKey] = books;
            return books;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching books: {ex.Message}");
            return new List<Book>();
        }
    }

    public async Task<List<Book>> GetBooksByGenreAsync(string genre, int limit = 20)
    {
        var cacheKey = $"genre_{genre}_{limit}";
        if (_cache.TryGetValue(cacheKey, out var cachedBooks))
            return cachedBooks;

        var books = await SearchBooksAsync($"subject:{genre}", limit);
        _cache[cacheKey] = books;
        return books;
    }

    public async Task<Book> GetBookDetailsAsync(string bookId)
    {
        try
        {
            var url = $"{BaseUrl}/works/{bookId}.json";
            var response = await _httpClient.GetStringAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var work = JsonSerializer.Deserialize<OpenLibraryWork>(response, options);

            if (work == null)
                return null;

            var book = new Book
            {
                Id = bookId,
                Title = work.Title ?? "Unknown Title",
                Description = work.Description.HasValue
                    ? (work.Description.Value.ValueKind == JsonValueKind.String
                        ? work.Description.Value.GetString()
                        : (work.Description.Value.ValueKind == JsonValueKind.Object && work.Description.Value.TryGetProperty("value", out var valueProp)
                            ? valueProp.GetString()
                            : "No description available."))
                    : "No description available.",
                Genres = work.Subjects?.Take(5).ToList() ?? new List<string>(),
                Authors = new List<string>(),
                FirstPublishYear = work.FirstPublishDate != null
                    ? int.TryParse(work.FirstPublishDate.Substring(0, 4), out var year) ? year : null
                    : null,
                CoverUrl = work.Covers?.Any() == true
                    ? $"https://covers.openlibrary.org/b/id/{work.Covers.First()}-L.jpg"
                    : GeneratePlaceholderCover(work.Title),
                Rating = GenerateRating(),
                RatingCount = Random.Shared.Next(500, 50000),
                OpenLibraryKey = $"/works/{bookId}"
            };

            // Fetch author details
            if (work.Authors?.Any() == true)
            {
                foreach (var author in work.Authors)
                {
                    var authorKey = author.Author?.Key?.Replace("/authors/", "");
                    if (!string.IsNullOrEmpty(authorKey))
                    {
                        try
                        {
                            var authorUrl = $"{BaseUrl}/authors/{authorKey}.json";
                            var authorResponse = await _httpClient.GetStringAsync(authorUrl);
                            var authorData = JsonSerializer.Deserialize<OpenLibraryAuthor>(authorResponse, options);
                            if (!string.IsNullOrEmpty(authorData?.Name))
                            {
                                book.Authors.Add(authorData.Name);
                            }
                        }
                        catch { /* Skip author if fetch fails */ }
                    }
                }
            }

            if (!book.Authors.Any())
                book.Authors.Add("Unknown Author");

            return book;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting book details: {ex.Message}");
            return null;
        }
    }

    private static double GenerateRating()
    {
        // Generate realistic ratings between 3.5 and 5.0
        return Math.Round(3.5 + (Random.Shared.NextDouble() * 1.5), 1);
    }

    private static string GenerateDescription(string title, List<string> authors)
    {
        var author = authors?.FirstOrDefault() ?? "the author";
        return $"An engaging work by {author}, '{title}' offers readers a compelling narrative that captivates from the first page to the last. This thought-provoking book explores themes that resonate with contemporary audiences while maintaining timeless appeal.";
    }

    private static string GeneratePlaceholderCover(string title)
    {
        // Return a placeholder image URL
        var encodedTitle = Uri.EscapeDataString(title ?? "Book");
        return $"https://via.placeholder.com/300x450/D4A574/FFFFFF?text={encodedTitle}";
    }

    #region API Response Models

    private class OpenLibrarySearchResult
    {
        [JsonPropertyName("docs")]
        public List<OpenLibraryDoc> Docs { get; set; }
    }

    private class OpenLibraryDoc
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("author_name")]
        public List<string> AuthorName { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("cover_i")]
        public int CoverI { get; set; }

        [JsonPropertyName("subject")]
        public List<string> Subject { get; set; }

        [JsonPropertyName("isbn")]
        public List<string> Isbn { get; set; }

        [JsonPropertyName("publisher")]
        public List<string> Publisher { get; set; }

        [JsonPropertyName("number_of_pages_median")]
        public int? NumberOfPagesMedian { get; set; }
    }

    private class OpenLibraryWork
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public JsonElement? Description { get; set; }

        [JsonPropertyName("subjects")]
        public List<string> Subjects { get; set; }

        [JsonPropertyName("authors")]
        public List<OpenLibraryAuthorRef> Authors { get; set; }

        [JsonPropertyName("covers")]
        public List<long> Covers { get; set; }

        [JsonPropertyName("first_publish_date")]
        public string FirstPublishDate { get; set; }
    }

    private class OpenLibraryAuthorRef
    {
        [JsonPropertyName("author")]
        public OpenLibraryAuthorKey Author { get; set; }
    }

    private class OpenLibraryAuthorKey
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
    }

    private class OpenLibraryAuthor
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    #endregion
}