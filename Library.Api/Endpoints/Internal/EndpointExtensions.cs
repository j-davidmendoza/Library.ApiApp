using Microsoft.Extensions.Configuration;

namespace Library.Api.Endpoints.Internal;

public static class EndpointExtensions
{
    //To provide a better api for the user to call, copy, and remove type from below, and provide as tmarker to attribute
    public static void AddEndpoints<TMarker>(this IServiceCollection services,
      IConfiguration configuration)
    {
        AddEndpoints(services, typeof(TMarker), configuration);
    }



    //This method will be used to scaen everything in a given assembly and find every class that implements the iendpoints and dynamically call the services and endpoints to automatically assemble
    public static void AddEndpoints(this IServiceCollection services,
        Type typeMarker, IConfiguration configuration)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.AddServices))!
                .Invoke(null, new object[] { services, configuration });
        }
    }


    public static void UseEndpoints<TMarker>(this IApplicationBuilder app)
    {
        UseEndpoints(app, typeof(TMarker));
    }

    public static void UseEndpoints(this IApplicationBuilder app, Type typeMarker)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!
                .Invoke(null, new object[] { app });
        }
    }


    private static IEnumerable<System.Reflection.TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
    {
        var endpointTypes = typeMarker.Assembly.DefinedTypes
                    .Where(x => !x.IsAbstract && !x.IsInterface &&
                        typeof(IEndpoints).IsAssignableFrom(x));
        return endpointTypes;
    }
}
