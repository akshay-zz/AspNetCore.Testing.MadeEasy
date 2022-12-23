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

        var client = factory.Object.CreateClient();
        var response = await client.GetAsync(RequestUri);

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

        var client = factory.Object.CreateClient("Test");
        var response = await client.GetAsync(RequestUri);
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

        var client = factory.Object.CreateClient("Test");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        var response = await client.GetAsync(RequestUri);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(data, content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        handler.VerifyAll();

    }

    [Theory]
    [InlineData("", "")]
    [InlineData("  ", "  ")]
    public void GetMockedNamedHttpClientFactory_able_to_validate_invalid_address(
       string baseUrl, string path)
    {
        Assert.Throws<ArgumentException>(() => MockHttpClient.GetMockedNamedHttpClientFactory(
              baseUrl: baseUrl,
              subUrl: path,
              response: "{}",
              responseStatusCode: HttpStatusCode.OK,
              clientName: "Test",
              headers: default,
              httpMethod: HttpMethod.Get));
    }

    [Fact]
    public void GetMockedNamedHttpClientFactory_able_to_validate_invalid_httpmethod()
    {
        Assert.Throws<ArgumentException>(() => MockHttpClient.GetMockedNamedHttpClientFactory(
              baseUrl: BaseUrl,
              subUrl: "/somepath",
              response: "{}",
              responseStatusCode: HttpStatusCode.OK,
              clientName: "Test",
              headers: default,
              httpMethod: default));
    }
}


public class MockHttpClientForMultipleUrlTests
{
    private const string BaseUrl = "https://localhost.xyz";

    [Fact]
    public async Task GetMockedNamedHttpClientFactory_should_able_to_configure_multiple_url()
    {
        var firstSetup = new MockClientDetail()
        {
            BaseUrl = BaseUrl,
            Path = "/response1",
            Method = HttpMethod.Get,
            Response = @"{""name"":""response 1""}",
            StatusCode = HttpStatusCode.OK,
        };

        var secondSetup = new MockClientDetail()
        {
            BaseUrl = BaseUrl,
            Path = "/response2",
            Method = HttpMethod.Get,
            Response = @"{""error"":""Invalid request""}",
            StatusCode = HttpStatusCode.BadRequest,
        };

        var setups = new List<MockClientDetail> { firstSetup, secondSetup };

        var (factory, _) = MockHttpClient.GetMockedNamedHttpClientFactory(setups);

        var client = factory.Object.CreateClient();
        var response1 = await client.GetAsync($"{BaseUrl}/response1");
        var content1 = await response1.Content.ReadAsStringAsync();
        var response2 = await client.GetAsync($"{BaseUrl}/response2");
        var content2 = await response2.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);

        Assert.Equal(@"{""name"":""response 1""}", content1);
        Assert.Equal(@"{""error"":""Invalid request""}", content2);
    }

    [Fact]
    public async Task GetMockedNamedHttpClientFactory_should_able_to_configure_different_baseurl()
    {
        var firstSetup = new MockClientDetail()
        {
            BaseUrl = $"{BaseUrl}new",
            Path = "/response1",
            Method = HttpMethod.Get,
            Response = @"{""name"":""response 1""}",
            StatusCode = HttpStatusCode.OK,
        };

        var secondSetup = new MockClientDetail()
        {
            BaseUrl = BaseUrl,
            Path = "/response2",
            Method = HttpMethod.Get,
            Response = @"{""error"":""Invalid request""}",
            StatusCode = HttpStatusCode.BadRequest,
        };

        var setups = new List<MockClientDetail> { firstSetup, secondSetup };

        var (factory, _) = MockHttpClient.GetMockedNamedHttpClientFactory(setups);

        var client = factory.Object.CreateClient();
        var response1 = await client.GetAsync($"{BaseUrl}new/response1");
        var content1 = await response1.Content.ReadAsStringAsync();
        var response2 = await client.GetAsync($"{BaseUrl}/response2");
        var content2 = await response2.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);

        Assert.Equal(@"{""name"":""response 1""}", content1);
        Assert.Equal(@"{""error"":""Invalid request""}", content2);
    }

    [Fact]
    public async Task GetMockedNamedHttpClientFactory_should_able_to_handle_if_mocked_url_is_not_getting_called()
    {
        var firstSetup = new MockClientDetail()
        {
            BaseUrl = $"{BaseUrl}new",
            Path = "/response1",
            Method = HttpMethod.Get,
            Response = @"{""name"":""response 1""}",
            StatusCode = HttpStatusCode.OK,
        };

        var secondSetup = new MockClientDetail()
        {
            BaseUrl = BaseUrl,
            Path = "/response2",
            Method = HttpMethod.Get,
            Response = @"{""error"":""Invalid request""}",
            StatusCode = HttpStatusCode.BadRequest,
        };

        var setups = new List<MockClientDetail> { firstSetup, secondSetup };

        var (factory, _) = MockHttpClient.GetMockedNamedHttpClientFactory(setups);

        var client = factory.Object.CreateClient();
        var response1 = await client.GetAsync($"{BaseUrl}new/response1");
        var content1 = await response1.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(@"{""name"":""response 1""}", content1);
    }


    [Fact]
    public async Task GetMockedNamedHttpClientFactory_should_able_to_configure_headers()
    {
        var firstSetup = new MockClientDetail()
        {
            BaseUrl = BaseUrl,
            Path = "/response1",
            Method = HttpMethod.Get,
            Response = @"{""name"":""response 1""}",
            StatusCode = HttpStatusCode.OK,
            Headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Accept", "application/json")
            }
        };

        var secondSetup = new MockClientDetail()
        {
            BaseUrl = BaseUrl,
            Path = "/response2",
            Method = HttpMethod.Get,
            Response = @"{""error"":""Invalid request""}",
            StatusCode = HttpStatusCode.BadRequest,
            Headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Accept", "application/xml")
            }
        };

        var setups = new List<MockClientDetail> { firstSetup, secondSetup };

        var (factory, _) = MockHttpClient.GetMockedNamedHttpClientFactory(setups);

        var client = factory.Object.CreateClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Accept", "application/xml");

        var response1 = await client.GetAsync($"{BaseUrl}/response1");
        var content1 = await response1.Content.ReadAsStringAsync();
        var response2 = await client.GetAsync($"{BaseUrl}/response2");
        var content2 = await response2.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);

        Assert.Equal(@"{""name"":""response 1""}", content1);
        Assert.Equal(@"{""error"":""Invalid request""}", content2);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("  ", "  ")]
    public void GetMockedNamedHttpClientFactory_able_to_validate_multiple_invalid_address(
        string baseUrl, string path)
    {
        var setup = new MockClientDetail()
        {
            BaseUrl = baseUrl,
            Path = path,
            Method = HttpMethod.Get,
            Response = @"{""name"":""response 1""}",
            StatusCode = HttpStatusCode.OK,
            Headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Accept", "application/json")
            }
        };

        var setups = new List<MockClientDetail> { setup };

        Assert.Throws<ArgumentException>(() => MockHttpClient.GetMockedNamedHttpClientFactory(setups));
    }

    [Fact]
    public void GetMockedNamedHttpClientFactory_able_to_validate_multiple_invalid_HttpMethod()
    {
        var setup = new MockClientDetail()
        {
            BaseUrl = BaseUrl,
            Path = "/path",
            Response = @"{""name"":""response 1""}",
            StatusCode = HttpStatusCode.OK,
            Headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Accept", "application/json")
            }
        };

        var setups = new List<MockClientDetail> { setup };

        Assert.Throws<ArgumentException>(() => MockHttpClient.GetMockedNamedHttpClientFactory(setups));

    }
}
