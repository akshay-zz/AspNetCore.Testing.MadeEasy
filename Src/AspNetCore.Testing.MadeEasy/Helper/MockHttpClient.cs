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
/// Model to hold detaild of mock http client
/// </summary>
public class MockClientDetail
{
    /// <summary>
    /// 
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Response { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public HttpMethod Method { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<KeyValuePair<string, string>> Headers { get; set; } = new();

    /// <summary>
    /// 
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
    /// <param name="baseUrl"></param>
    /// <param name="subUrl"></param>
    /// <param name="response"></param>
    /// <param name="responseStatusCode"></param>
    /// <param name="clientName"></param>
    /// <param name="httpMethod"></param>
    /// <param name="headers"></param>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public static (Mock<IHttpClientFactory>, Mock<HttpMessageHandler>) GetMockedNamedHttpClientFactory(
        string baseUrl,
        string subUrl,
        string response,
        HttpStatusCode responseStatusCode,
        HttpMethod httpMethod = default,
        IList<KeyValuePair<string, string>> headers = default,
        HttpResponseMessage responseMessage = default,
        string clientName = "")
    {
        if ((string.IsNullOrWhiteSpace(baseUrl) && string.IsNullOrWhiteSpace(subUrl)) && httpMethod == default)
        {
            throw new ArgumentException("Invalid data! BaseUrl or Path mandatory along with method type");
        }

        var _handlerMock = new Mock<HttpMessageHandler>();
        var _httpClientFactory = new Mock<IHttpClientFactory>();

        var contentToReturn = new HttpResponseMessage
        {
            Content = new StringContent(response),
            StatusCode = responseStatusCode
        };

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x =>
                x.RequestUri == new Uri($"{baseUrl}{subUrl}") &&
                (httpMethod == default || x.Method == httpMethod) &&
                CheckHeaders(x, headers)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage ?? contentToReturn)
            .Verifiable();

        _httpClientFactory.Setup(x => x.CreateClient(clientName))
            .Returns(new HttpClient(_handlerMock.Object))
            .Verifiable();

        return (_httpClientFactory, _handlerMock);
    }

    /// <summary>
    /// Setup mock HttpClientFactory and HttpMessageHandler
    /// </summary>
    /// <param name="mockClientDetails"></param>
    /// <param name="clientName"></param>
    /// <returns></returns>
    public static (Mock<IHttpClientFactory>, Mock<HttpMessageHandler>) GetMockedNamedHttpClientFactory(
        List<MockClientDetail> mockClientDetails, string clientName = "")
    {
        var validData = mockClientDetails.Where(
            x => (!string.IsNullOrWhiteSpace(x.BaseUrl) || !string.IsNullOrWhiteSpace(x.Path)) &&
            x.Method != default).ToList();

        if (validData.Count != mockClientDetails.Count)
        {
            throw new ArgumentException("Invalid data! BaseUrl or Path mandatory along with method type");
        }

        var _handlerMock = new Mock<HttpMessageHandler>();
        var _httpClientFactory = new Mock<IHttpClientFactory>();


        foreach (var detail in mockClientDetails)
        {
            _handlerMock.Protected()
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

        _httpClientFactory.Setup(x => x.CreateClient(clientName))
            .Returns(new HttpClient(_handlerMock.Object))
            .Verifiable();

        return (_httpClientFactory, _handlerMock);
    }


    private static readonly Func<HttpRequestMessage, IList<KeyValuePair<string, string>>, bool> CheckHeaders =
        delegate (HttpRequestMessage httpRequest, IList<KeyValuePair<string, string>> headers) {

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
