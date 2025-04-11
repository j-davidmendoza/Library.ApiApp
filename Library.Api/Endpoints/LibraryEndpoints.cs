using FluentValidation;
using FluentValidation.Results;
using Library.Api.Endpoints.Internal;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Cors;

namespace Library.Api.Endpoints;

public class LibraryEndpoints : IEndpoints
{
    private const string ContentType = "application/json";
    private const string Tag = "Books";
    private const string BaseRoute = "books";

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBookService, BookService>();
    }

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("hello", () => "Hello World");

        app.MapPost(BaseRoute,CreateBookAsync)
        .WithName("CreateBook")
        .Accepts<Book>(ContentType)
        .Produces<Book>(201)
        .Produces<IEnumerable<ValidationFailure>>(400)
        .WithTags(Tag);


        app.MapGet(BaseRoute, GetAllBooksAsync)
        .WithName("GetBooks")
        .Produces<IEnumerable<Book>>(200)
        .WithTags(Tag);

        app.MapGet($"{BaseRoute}/{{isbn}}", GetBookByIsbnAsync)
        .WithName("GetBook")
        .Produces<Book>(200)
        .Produces(404)
        .WithTags(Tag);


        app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
        .WithName("UpdateBook")
        .Accepts<Book>(ContentType)
        .Produces<Book>(201)
        .Produces<IEnumerable<ValidationFailure>>(400)
        .WithTags(Tag);

        app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync).WithName("DeleteBook")
        .Produces(204)
        .Produces(404)
        .WithTags(Tag);


        //app.MapGet("status1", () => 
        //{
        //    return Results.Extensions.Html(@"<!doctype html>
        //<html>
        //    <head><title>Status page</title></head>
        //    <body>
        //        <h1>Status</h1>
        //        <p>The server is working fine. Bye bye!</p>
        //    </body>
        //</html>");
        //});

        //app.MapGet("status2", [EnableCors("AnyOrigin")] () =>
        //{
        //    return Results.Extensions.Html(@"<!doctype html>
        //<html>
        //    <head><title>Status page</title></head>
        //    <body>
        //        <h1>Status</h1>
        //        <p>The server is working fine. Bye bye!</p>
        //    </body>
        //</html>");
        //}).RequireCors("AnyOrigin"); 
        //app.MapGet("status3", [EnableCors("AnyOrigin")] () =>
        //{
        //    return Results.Extensions.Html(@"<!doctype html>
        //<html>
        //    <head><title>Status page</title></head>
        //    <body>
        //        <h1>Status</h1>
        //        <p>The server is working fine. Bye bye!</p>
        //    </body>
        //</html>");
        //}).ExcludeFromDescription();

    }

    //You can extract these to seperate files if you want 
    //Good thing about this approach below is that when testing them you only need to inject all the things that this merthod will explicitly sue while witha  controller approach you might be injehcting a bunch of things and not all are actually being used in your invocation which is significnalty better apporach imho
    internal static async Task<IResult> CreateBookAsync(Book book, IBookService bookService,
            IValidator<Book> validator, LinkGenerator linker, HttpContext context)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var created = await bookService.CreateAsync(book);
        if (!created)
        {
            return Results.BadRequest(new List<ValidationFailure>
                {
                    new ValidationFailure("Isbn", "A book with the same ISBN-13 already exists or invalid data provided.")
                });
        }


        //var path = linker.GetPathByName("GetBook", new { isbn = book.Isbn })!;
        //var locationUri = linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn })!;
        //return Results.Created(locationUri, book);
        //Or do the below approach
        return Results.Created($"/{BaseRoute}/{book.Isbn}", book);

    }

    internal static async Task<IResult> GetAllBooksAsync(IBookService bookService, string? searchTerm)
    {
        if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
            return Results.Ok(matchedBooks);
        }

        var books = await bookService.GetAllAsync();
        return Results.Ok(books);
    }

    internal static async Task<IResult> GetBookByIsbnAsync(string isbn, IBookService bookService)
    {
        var book = await bookService.GetByIsbnAsync(isbn);
        return book is not null ? Results.Ok(book) : Results.NotFound();
    }

    internal static async Task<IResult> UpdateBookAsync(string isbn, Book book, IBookService bookService,
            IValidator<Book> validator)
    {
        book.Isbn = isbn;
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var updated = await bookService.UpdateAsync(book);
        return updated ? Results.Ok(book) : Results.NotFound();
    }

    internal static async Task<IResult> DeleteBookAsync(string isbn, IBookService bookSerivce)
    {
        var deleted = await bookSerivce.DeleteAsync(isbn);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}
