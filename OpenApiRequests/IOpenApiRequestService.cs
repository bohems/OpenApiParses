using System.Net;

namespace SwaggerRequests;

public interface IOpenApiRequestService
{
    public Task<HttpStatusCode> SendRequestAsync(string endpoint, string requestJson);
}