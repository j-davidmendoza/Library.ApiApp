using Library.Api.Data;
using Library.Api.Models;
using Library.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IBookService, BookService>();

//builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
//    new SqliteConnectionFactory("hardCodedConectionString"));
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory
    (builder.Configuration.GetValue<string>("Database:ConnectionString")));

builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("books", async (Book book, IBookService bookService) =>
{
    var created = await bookService.CreateAsync(book);
    if(!created)
    {
        return Results.BadRequest(new
        {
            errorMessage = "A book with the same ISBN-13 already exists or invalid data provided.",
        });
    }

    return Results.Created($"/books/{book.Isbn}", book);
});

app.MapGet("/", () => "Hello World!");


// Db init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.IntializeAsync();

app.Run();

