using Library.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
//    new SqliteConnectionFactory("hardCodedConectionString"));
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory
    (builder.Configuration.GetValue<string>("Database:ConnectionString")));

builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();



app.MapGet("/", () => "Hello World!");


// Db init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.IntializeAsync();

app.Run();

