using AspNetCore.Testing.MadeEasy.Helper;
using System.Net;

namespace AspNetCore.Testing.MadeEasy.Test.Helper;

public class MockHttpClientTests
{
    private const string BaseUrl = "https://localhost.xyz";
    private const string RequestUri = $"{BaseUrl}/data";

    [Fact]
    public async Task GetMockedNamedHttpClientFactory_should_httpclient_as_configured_for_unnamed_client()
    {
        var data = @"{""name"":""Chintu""}";

        var (factory, _) = MockHttpClient.GetMockedNamedHttpClientFactory(
            baseUrl: BaseUrl,
            subUrl: "/data",
            response: data,
            responseStatusCode: HttpStatusCode.OK,
            httpMethod: HttpMethod.Get);

        var response = await CallHttpClient(factory.Object);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(data, content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Fact]
    public async Task GetMockedNamedHttpClientFactory_should_return_mockedhttpclient_as_configured_for_named_client()
    {
        var data = @"{""name"":""Chintu""}";

        var (factory, _) = MockHttpClient.GetMockedNamedHttpClientFactory(
            baseUrl: BaseUrl,
            subUrl: "/data",
            response: data,
            responseStatusCode: HttpStatusCode.OK,
            clientName: "Test",
            httpMethod: HttpMethod.Get);

        var response = await CallNamedHttpClient(factory.Object);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(data, content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMockedNamedHttpClientFactory_should_able_to_verify_headers()
    {
        var data = @"{""name"":""Chintu""}";
        var headers = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Accept", "application/json")
        };

        var (factory, handler) = MockHttpClient.GetMockedNamedHttpClientFactory(
            baseUrl: BaseUrl,
            subUrl: "/data",
            response: data,
            responseStatusCode: HttpStatusCode.OK,
            clientName: "Test",
            headers: headers,
            httpMethod: HttpMethod.Get);

        var response = await CallNamedHttpClientWithHeader(factory.Object);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(data, content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        handler.VerifyAll();

    }

    private static async Task<HttpResponseMessage> CallNamedHttpClientWithHeader(IHttpClientFactory factory)
    {
        var client = factory.CreateClient("Test");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return await client.GetAsync(RequestUri);
    }

    private static async Task<HttpResponseMessage> CallNamedHttpClient(IHttpClientFactory factory)
    {
        var client = factory.CreateClient("Test");
        return await client.GetAsync(RequestUri);
    }

    private static async Task<HttpResponseMessage> CallHttpClient(IHttpClientFactory factory)
    {
        var client = factory.CreateClient();
        return await client.GetAsync(RequestUri);
    }
}
