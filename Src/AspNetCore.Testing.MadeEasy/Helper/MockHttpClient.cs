using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Testing.MadeEasy.Helper;

/// <summary>
/// Model to hold detaild of mock http client.
/// </summary>
public class MockClientDetail
{
    /// <summary>
    /// Base address of the url.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    /// <summary>
    /// Subdirectory of the url.
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// Response of the mocked url, it will be overrided if <see cref="ResponseMessage"/> is passed.
    /// </summary>
    public string Response { get; set; } = string.Empty;
    /// <summary>
    /// Http method to mock.
    /// </summary>
    public HttpMethod Method { get; set; }
    /// <summary>
    /// Response status code, it will be overrided if <see cref="ResponseMessage"/> is passed.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }
    /// <summary>
    /// To verify header for the api call
    /// </summary>
    public List<KeyValuePair<string, string>> Headers { get; set; } = new();

    /// <summary>
    /// Custom <see cref="HttpResponseMessage"/>. It will override <see cref="Response"/> and  <see cref="StatusCode"/>.
    /// </summary>
    public HttpResponseMessage ResponseMessage { get; set; } = default;
}

/// <summary>
/// Helper method for http client
/// </summary>
public class MockHttpClient
{
    /// <summary>
    /// Setup mocked HttpClientFactory and HttpMessageHandler
    /// </summary>
    /// <param name="baseUrl">base url</param>
    /// <param name="subUrl">path of the url</param>
    /// <param name="response">response content to be mocked</param>
    /// <param name="responseStatusCode">response http status code to be mocked</param>
    /// <param name="clientName">name of the client for Http client factory</param>
    /// <param name="httpMethod">http method to be mocked</param>
    /// <param name="headers">verify headers client should get called with</param>
    /// <param name="responseMessage"><see cref="HttpResponseMessage"></see>, it will override the response and response status code that is passed</param>
    /// <returns></returns>
    public static (Mock<IHttpClientFactory>, Mock<HttpMessageHandler>) GetMockedNamedHttpClientFactory(
        string baseUrl,
        string subUrl,
        string response,
        HttpStatusCode responseStatusCode,
        HttpMethod httpMethod,
        List<KeyValuePair<string, string>> headers = default,
        HttpResponseMessage responseMessage = default,
        string clientName = "")
    {
        if ((string.IsNullOrWhiteSpace(baseUrl) && string.IsNullOrWhiteSpace(subUrl)) || httpMethod == default)
        {
            throw new ArgumentException("Invalid data! BaseUrl or Path mandatory along with method type");
        }

        var mockClientDetail = new MockClientDetail
        {
            BaseUrl = baseUrl,
            Path = subUrl,
            StatusCode = responseStatusCode,
            Response = response,
            Headers = headers,
            Method = httpMethod,
            ResponseMessage = responseMessage,
        };

        var handlerMock = MockHandler(new List<MockClientDetail> { mockClientDetail });
        var httpClientFactory = new Mock<IHttpClientFactory>();

        httpClientFactory.Setup(x => x.CreateClient(clientName))
            .Returns(new HttpClient(handlerMock.Object))
            .Verifiable();

        return (httpClientFactory, handlerMock);
    }

    /// <summary>
    /// Setup mock HttpClientFactory and HttpMessageHandler with multiple address and http method.
    /// </summary>
    /// <param name="mockClientDetails"></param>
    /// <param name="clientName"></param>
    /// <returns></returns>
    public static (Mock<IHttpClientFactory>, Mock<HttpMessageHandler>) GetMockedNamedHttpClientFactory(
        List<MockClientDetail> mockClientDetails, string clientName = "")
    {
        if (!ValidateMockClientDetails(mockClientDetails))
        {
            throw new ArgumentException("Invalid data! BaseUrl or Path mandatory along with method type");
        }

        var handlerMock = MockHandler(mockClientDetails);
        var _httpClientFactory = new Mock<IHttpClientFactory>();

        _httpClientFactory.Setup(x => x.CreateClient(clientName))
            .Returns(new HttpClient(handlerMock.Object))
            .Verifiable();

        return (_httpClientFactory, handlerMock);
    }

    /// <summary>
    /// Get <see cref="HttpClient"/> and <see cref="HttpMessageHandler"/> with mocked http message handler
    /// </summary>
    /// <param name="mockClientDetails">details to mock the request</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static (HttpClient, Mock<HttpMessageHandler>) GetMockedHttpClient(List<MockClientDetail> mockClientDetails)
    {
        if (!ValidateMockClientDetails(mockClientDetails))
        {
            throw new ArgumentException("Invalid data! BaseUrl or Path mandatory along with method type");
        }

        var handler = MockHandler(mockClientDetails);


        return (new HttpClient(handler.Object), handler);

    }

    /// <summary>
    /// Get <see cref="HttpClient"/> and <see cref="HttpMessageHandler"/> with mocked http message handler
    /// </summary>
    /// <param name="baseUrl">base url</param>
    /// <param name="subUrl">sub url</param>
    /// <param name="response">reponse body</param>
    /// <param name="responseStatusCode">reponse http status code</param>
    /// <param name="httpMethod">request http method</param>
    /// <param name="headers">request headers to be verified</param>
    /// <param name="responseMessage"><see cref="HttpResponseMessage"></see>, it will override the response and response status code that is passed</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>

    public static (HttpClient, Mock<HttpMessageHandler>) GetMockedHttpClient(string baseUrl,
        string subUrl,
        string response,
        HttpStatusCode responseStatusCode,
        HttpMethod httpMethod,
        List<KeyValuePair<string, string>> headers = default,
        HttpResponseMessage responseMessage = default)
    {
        var mockClientDetail = new MockClientDetail
        {
            BaseUrl = baseUrl,
            Path = subUrl,
            StatusCode = responseStatusCode,
            Response = response,
            Headers = headers,
            Method = httpMethod,
            ResponseMessage = responseMessage,
        };

        var details = new List<MockClientDetail> { mockClientDetail };

        if (mockClientDetail == null || !ValidateMockClientDetails(details))
        {
            throw new ArgumentException("Invalid data! BaseUrl or Path and method type should not be empty");
        }

        var handler = MockHandler(details);


        return (new HttpClient(handler.Object), handler);

    }

    private static bool ValidateMockClientDetails(List<MockClientDetail> mockClientDetails)
    {
        var validData = mockClientDetails.Where(
         x => (!string.IsNullOrWhiteSpace(x.BaseUrl) || !string.IsNullOrWhiteSpace(x.Path)) &&
         x.Method != default).ToList();

        if (validData.Count != mockClientDetails.Count)
        {
            return false;
        }

        return true;
    }

    private static Mock<HttpMessageHandler> MockHandler(List<MockClientDetail> mockClientDetails)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        foreach (var detail in mockClientDetails)
        {
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(x =>
                    x.RequestUri == new Uri($"{detail.BaseUrl}{detail.Path}") &&
                    (x.Method == detail.Method) && CheckHeaders(x, detail.Headers)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(
                    detail.ResponseMessage ??
                    new HttpResponseMessage
                    {
                        Content = new StringContent(detail.Response),
                        StatusCode = detail.StatusCode,
                    }
                 ).Verifiable();
        }

        return handlerMock;
    }

    private static readonly Func<HttpRequestMessage, List<KeyValuePair<string, string>>, bool> CheckHeaders =
        delegate (HttpRequestMessage httpRequest, List<KeyValuePair<string, string>> headers) {

            if (headers == null || !headers.Any())
            {
                return true;
            }

            var count = headers
               .Where(h => httpRequest.Headers.Any(p => p.Key == h.Key && p.Value.Contains(h.Value)))
               .Count();
            return count > 0;
        };
}
