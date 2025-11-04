using Bonder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Services
{
    public interface IBookService
    {
        Task<List<Book>> SearchBooksAsync(string query, int limit = 20);
        Task<List<Book>> GetBooksByGenreAsync(string genre, int limit = 20);
        Task<Book> GetBookDetailsAsync(string bookId);
    }
}
