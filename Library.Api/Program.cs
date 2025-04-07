using FluentValidation;
using FluentValidation.Results;
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


builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // This will register all the validors in the assembly

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("books", async (Book book, IBookService bookService,
    IValidator<Book> validator) =>
{
    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
    {
        //return Results.ValidationProblem();
        return Results.BadRequest(validationResult.Errors); //You may You might not want to expose to your consumers the validator result obejct and you might wanna have yourt own contract, you would do the mapping for that contract here 
    }

    var created = await bookService.CreateAsync(book);
    if(!created)
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
});

app.MapGet("/", () => "Hello World!");

app.MapGet("books", async (IBookService bookService) =>
{
    var books = await bookService.GetAllAsync();
    return Results.Ok(books);
});


// Db init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.IntializeAsync();

app.Run();

