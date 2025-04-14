﻿using FluentAssertions;
using FluentValidation.Results;
using Library.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointTests: IClassFixture<WebApplicationFactory<IApiMarker>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<IApiMarker> _factory;
    private readonly List<string> _createdIsbns = new();


    public LibraryEndpointTests(WebApplicationFactory<IApiMarker> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        //Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();

        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location.Should().Be($"/books/{book.Isbn}");

    }

    [Fact]
    public async Task CreateBook_Fails_WhenIsbnIsInvalid()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        book.Isbn = "invalid-isbn";


        //Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Value was not valid ISBN-13");


    }


    [Fact]
    public async Task CreateBook_Fails_WhenBookExists()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        //Act
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("A book with the same ISBN-13 already exists or invalid data provided.");


    }

    [Fact]
    public async Task GetBook_ReturnsBook_WhenBookExists()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        //Act
        var result = await httpClient.GetAsync($"/books/{book.Isbn}");
        var existingBook = await result.Content.ReadFromJsonAsync<Book>();  


        //Assert
        existingBook.Should().BeEquivalentTo(book);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBook_ReturnsBookNotFound_WhenBookDoesNotExist()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();

        //Act
        var result = await httpClient.GetAsync($"/books/{isbn}");


        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task GetBooks_ReturnsAllBooks_WhenBooksExist()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book> { book };

        //Act
        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        //Assert

        result.StatusCode.Should().Be(HttpStatusCode.OK); 
        returnedBooks.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task GetBooks_ReturnsNoBooks_WhenNoBooksExist()
    {
        //Arrange
        var httpClient = _factory.CreateClient();

        //Act
        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        //Assert

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchBooks_ReturnBooks_WhenTitleMatches()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book> { book };

        //Act
        var result = await httpClient.GetAsync("/books?searchTerm=oder");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        //Assert

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task UpdateBook_UpdatesBook_WhenDataIsCorrect()
    {
        //Arrage
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        //Act
        book.PageCount = 69;
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var updatedBook = await result.Content.ReadFromJsonAsync<Book>();

        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedBook.Should().BeEquivalentTo(book);

    }


    [Fact]
    public async Task UpdateBook_DoesNotUpdateBook_WhenDataIsIncorrect()
    {
        //Arrage
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        //Act
        book.Title = string.Empty;
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        //var updatedBook = await result.Content.ReadFromJsonAsync<Book>();
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        //updatedBook.Should().BeEquivalentTo(book);
        error.PropertyName.Should().Be("Title");
        error.ErrorMessage.Should().Be("'Title' must not be empty.");

    }


    [Fact]
    public async Task UpdateBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        //Arrage
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        //Act
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);


        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);

    }


    [Fact]
    public async Task DeleteBook_ReturnsBookNotFound_WhenBookDoesNotExist()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();

        //Act
        var result = await httpClient.DeleteAsync($"/books/{isbn}");


        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenBookDoesExist()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        //Act
        var result = await httpClient.DeleteAsync($"/books/{book.Isbn}");


        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private Book GenerateBook(string title = "The Dirty Coder")
    {
        return new Book
        {
            Isbn = GenerateIsbn(),
            Title = title,
            Author = "David Mendoza",
            PageCount = 420,
            ShortDescription = "All my tricks in one book",
            ReleaseDate = new DateTime(2026, 10, 1)
        };
    }

    private string GenerateIsbn()
    {
        return $"{Random.Shared.Next(100, 999)}-" +
               $"{Random.Shared.Next(1000000000, 2100999999)}";
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();
        foreach (var createdIsbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"/books/{createdIsbn}");
        }
    }




    //This shows how to get httpclient, you can do all testing here, avoid using in memory db, test integration to the real db instead
    //[Fact]
    //public void Test()
    //{
    //    var httpClient = _factory.CreateClient();

    //}
    //You dont need this all, instead do the above, with an implementation of IClassFixture
    ////You can seperate these methods to seperate classes but we'll keep them together for simplicity
    //private readonly WebApplicationFactory<IApiMarker> _factory;

    //public LibraryEndpointTests()
    //{
    //    _factory = new WebApplicationFactory<IApiMarker>();
    //    var httpClient = _factory.CreateClient();
    //    //The factory will create a fully fledged api with all the endpoints via the httpClient, only can be called via httpclient, making it an in memory call top the api
    //    //Combine this with xunit to create on app per test and teh call that app without worring of start up and ready and return


    //}


}
