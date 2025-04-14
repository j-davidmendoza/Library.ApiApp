using Library.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Library.Api.Tests.Integration;

public class LibraryApiFactory : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(collection =>
        {
            // You can replace here dependencies already registered in the api's program.cs just for test execuution to not use real db for example
            collection.RemoveAll(typeof(IDbConnectionFactory));
            collection.AddSingleton<IDbConnectionFactory>(_ =>
                new SqliteConnectionFactory("DataSource=file:inmem?mode=memory&cache=shared"));

        });
    }
}
