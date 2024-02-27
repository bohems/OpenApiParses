using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace SwaggerRequests;

public class OpenApiRequestService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public OpenApiRequestService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task<bool> SendRequestAsync(IRequest request)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new RequestJsonConverter());
        var openApiRequest = JsonSerializer.Deserialize<OpenApiRequest>(request.OpenApiRequestJson, options);
        
        var httpClient = _httpClientFactory.CreateClient();
        
        var httpRequestMessage = new HttpRequestMessage();

        foreach (var httpHeader in httpHeaders)
        {
            httpRequestMessage.Headers.Add(httpHeader.Key, httpHeader.Value);
        }

        if (openApiRequest.HttpMethod is null)
        {
            throw new InvalidDataException("Метод запроса не указан или указан неверно");
        }
        
        httpRequestMessage.Method = openApiRequest.HttpMethod;
        
        var queryString = endpoint + QueryString.Create(openApiRequest.Parameters).ToUriComponent();
        
        httpRequestMessage.RequestUri = new Uri(baseUri, queryString);
        
        if (openApiRequest.RequestsBody is not null)
            httpRequestMessage.Content = new StringContent(openApiRequest.RequestsBody, Encoding.UTF8, "application/json");

        try
        {
            var result = await httpClient.SendAsync(httpRequestMessage);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}