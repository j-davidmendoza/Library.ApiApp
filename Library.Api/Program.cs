using FluentValidation;
using FluentValidation.Results;
using Library.Api.Auth;
using Library.Api.Data;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true); //This is everything you need to add more configuration options for your own or 3rd party loads


builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { }); // May not need this because you have JWT auth or a quick authentication you can use here
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IBookService, BookService>();

//builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
//    new SqliteConnectionFactory("hardCodedConectionString"));
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory
    (builder.Configuration.GetValue<string>("Database:ConnectionString")));

builder.Services.AddSingleton<DatabaseInitializer>();


builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // This will register all the validors in the assembly

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();//After swagger because you dont want swagger to be behind authentication


app.MapPost("books",
//[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)] //This is all you need to authorize this endpoint 
//[AllowAnonymous] // If you have security by default, you can use this to allow anonymous access to this endpoint, same way as good ole mvc
async (Book book, IBookService bookService,
    IValidator<Book> validator) =>
{
    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
    {
        //return Results.ValidationProblem();
        return Results.BadRequest(validationResult.Errors); //You may You might not want to expose to your consumers the validator result obejct and you might wanna have yourt own contract, you would do the mapping for that contract here 
    }

    var created = await bookService.CreateAsync(book);
    if (!created)
    {
        //return Results.BadRequest(new
        //{
        //    errorMessage = "A book with the same ISBN-13 already exists or invalid data provided.",
        //});
        return Results.BadRequest(new List<ValidationFailure>
        {
            new ValidationFailure("Isbn", "A book with the same ISBN-13 already exists or invalid data provided.")
        });
    }

    return Results.Created($"/books/{book.Isbn}", book);
});//.AllowAnonymous(); //You can also do this to allow anonymous access to this endpoint using fluent method, approach is up to you

app.MapGet("/", () => "Hello World!");

//app.MapGet("books", async (IBookService bookService) =>
//{
//    var books = await bookService.GetAllAsync();
//    return Results.Ok(books);
//});

app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
{
    if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
    {
        var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
        return Results.Ok(matchedBooks);
    }

    var books = await bookService.GetAllAsync();
    return Results.Ok(books);
});

//Possible to add regex checking here but would need it in the service layer also, for now just not having it unless it is only api layer specific 
app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
{ 
    var book = await bookService.GetByIsbnAsync(isbn);
    return book is not null ? Results.Ok(book) : Results.NotFound(); //200 if it does exist 404 if it does not exist
});


app.MapPut("books/{isbn}", async (string isbn, Book book, IBookService bookService,
    IValidator<Book> validator) =>
{
    book.Isbn = isbn;
    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors); //You may You might not want to expose to your consumers the validator result obejct and you might wanna have yourt own contract, you would do the mapping for that contract here 
    }

    var updated = await bookService.UpdateAsync(book); //id is mutable so it ill be picked based on id
    return updated ? Results.Ok(book) : Results.NotFound();

});

app.MapDelete("books/{isbn}", async (string isbn, IBookService bookSerivce) =>
{
    var deleted = await bookSerivce.DeleteAsync(isbn);
    return deleted ? Results.NoContent() : Results.NotFound();
});


// Db init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.IntializeAsync();

app.Run();

