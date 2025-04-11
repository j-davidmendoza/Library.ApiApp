using Library.Api.Endpoints.Internal;

namespace Library.Api.Endpoints;

public class HelloEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
       
    }

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("helloAdded", () => "Hello World from newly added endpoint");
    }
}
