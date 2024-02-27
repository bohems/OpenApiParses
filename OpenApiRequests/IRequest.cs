namespace SwaggerRequests;

public interface IRequest
{
    /// <summary>Базовый Uri в формате хост + базовый Path</summary>
    /// <example>
    /// petstore.swagger.io/v2<br/>
    /// </example>
    public Uri HostAndBasePath { get; set; }
    
    /// <summary>Path запроса</summary>
    /// <example>
    /// /WeatherForecast<br/>
    /// /BirthdayPersons/Get
    /// </example>
    public string Path { get; set; }
    
    /// <summary>
    /// Заголовки запроса
    /// </summary>
    Dictionary<string, string> HttpHeaders { get; set; }
    
    /// <summary>
    /// Запрос из OpenApi схемы, посмотрите пример по ссылке
    /// https://i.imgur.com/8UDUSvk.jpeg
    /// </summary>
    public string OpenApiRequestJson { get; set; }
}