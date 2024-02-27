namespace SwaggerRequests;

public class OpenApiRequest
{
    // /// <summary>
    // /// Адрес эндпоинта на который будет отправлен запрос
    // /// </summary>
    // /// <example>/WeatherForecast</example>
    // public string? EndpointPath { get; set; }
    
    /// <summary>
    /// HTTP метод запроса
    /// </summary>
    public HttpMethod? HttpMethod { get; set; }

    /// <summary>
    /// Параметры из URL
    /// </summary>
    public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    
    /// <summary>
    /// Тело запроса. Поддерживается только JSON
    /// </summary>
    public string? RequestsBody { get; set; }
}