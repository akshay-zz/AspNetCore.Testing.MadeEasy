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
    /// <returns></returns>
    public static (Mock<IHttpClientFactory>, Mock<HttpMessageHandler>) GetMockedNamedHttpClientFactory(
        string baseUrl,
        string subUrl,
        string response,
        HttpStatusCode responseStatusCode,
        HttpMethod httpMethod = default,
        IList<KeyValuePair<string, string>> headers = default,
        string clientName = "")
    {
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
            .ReturnsAsync(contentToReturn)
            .Verifiable();

        _httpClientFactory.Setup(x => x.CreateClient(clientName))
            .Returns(new HttpClient(_handlerMock.Object))
            .Verifiable();

        return (_httpClientFactory, _handlerMock);
    }

    private static readonly Func<HttpRequestMessage, IList<KeyValuePair<string, string>>, bool> CheckHeaders =
        delegate (HttpRequestMessage httpRequest, IList<KeyValuePair<string, string>> headers) {
            if (headers == default)
                return true;

            var count = headers
               .Where(h => httpRequest.Headers.Any(p => p.Key == h.Key))
               .Count();
            return count > 0;
        };
}
