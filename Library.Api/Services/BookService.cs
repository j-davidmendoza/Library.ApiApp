
using Dapper;
using Library.Api.Data;
using Library.Api.Models;

namespace Library.Api.Services;

public class BookService : IBookService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BookService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(Book book)
    {
        //var existingBook = await GetByIsbnAsync(book.Isbn);
        //if (existingBook != null)
        //{
        //    // Book with the same ISBN already exists
        //    return false;
        //}

        using var connection = await _connectionFactory.CreateConnectionAsync();
        //await connection.ExecuteAsync(
        //    @"INSERT INTO Books (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate)
        //        VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate)", book);
        //return true; // approach below will allow you to know if it was created or not created
        var result = await connection.ExecuteAsync(
            @"INSERT INTO Books (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate)
                VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate)", book);
        return result > 0;

    }

    public Task<bool> DeleteAsync(string isbn)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<Book>("SELECT * FROM Books");

    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return connection.QuerySingleOrDefault<Book>(
            "SELECT * FROM Books WHERE Isbn = @Isbn LIMIT 1", new { Isbn = isbn });
    }

    public Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Book book)
    {
        throw new NotImplementedException();
    }
}
