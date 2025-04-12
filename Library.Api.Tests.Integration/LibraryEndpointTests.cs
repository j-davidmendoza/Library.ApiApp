using Microsoft.AspNetCore.Mvc.Testing;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointTests: IClassFixture<WebApplicationFactory<IApiMarker>>
{
    private readonly WebApplicationFactory<IApiMarker> _factory;

    public LibraryEndpointTests(WebApplicationFactory<IApiMarker> factory)
    {
        _factory = factory;
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
