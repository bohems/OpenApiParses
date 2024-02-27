using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SwaggerRequests;

public class RequestJsonConverter : JsonConverter<OpenApiRequest>
{
    private const string ValidEndpointRegexPattern = @"^\/[a-zA-Z0-9-._~:\/?#[\]@!$&'()*+,;=%]+$";
    
    private static readonly byte[] RequestBodyUtf8 = "requestBody"u8.ToArray();
    private static readonly byte[] ParametersUtf8 = "parameters"u8.ToArray();
    private static readonly byte[] ExampleUtf8 = "example"u8.ToArray();
    private static readonly byte[] NameUtf8 = "name"u8.ToArray();
    

    private readonly HttpMethod[] _httpMethods = { HttpMethod.Get, HttpMethod.Post, HttpMethod.Delete, HttpMethod.Put, HttpMethod.Patch, 
        HttpMethod.Head, HttpMethod.Options, HttpMethod.Trace };

    private readonly Regex _regex = new(ValidEndpointRegexPattern);
    
    public override OpenApiRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var request = new OpenApiRequest();

        do
        {
            if (reader.TokenType is not JsonTokenType.PropertyName)
            {
                continue;
            }

            // if (request.EndpointPath is null)
            // {
            //     var propertyName = reader.GetString();
            //
            //     request.EndpointPath = TryGetEndpointUri(propertyName!);
            //     
            //     if (request.EndpointPath is not null) continue;
            // }

            if (request.HttpMethod is null)
            {
                var propertyName = reader.GetString();
                
                request.HttpMethod = TryGetHttpMethod(propertyName!);
                
                if (request.HttpMethod is not null) continue;
            }
            
            if (reader.ValueTextEquals(ParametersUtf8))
            {
                request.Parameters = TryGetParameters(ref reader);
                
                continue;
            }

            if (reader.ValueTextEquals(RequestBodyUtf8))
            {
                request.RequestsBody = TryGetRequestBody(ref reader);
            }

        } while (reader.Read());
        
        return request;
    }
    
    public override void Write(Utf8JsonWriter writer, OpenApiRequest value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
    
    private string? TryGetEndpointUri(string propertyName)
    {
        return _regex.IsMatch(propertyName) ? propertyName : null;
    }
    
    private HttpMethod? TryGetHttpMethod(string propertyName)
    {
        return _httpMethods.FirstOrDefault(m => m.ToString().ToLower() == propertyName);
    }

    private IDictionary<string, string> TryGetParameters(ref Utf8JsonReader reader)
    {
        IDictionary<string, string> parameters = new Dictionary<string, string>();
        
        byte squareBracketsBalance = 0;
        
        do
        {
            reader.Read();
            
            switch (reader.TokenType)
            {
                case JsonTokenType.StartArray:
                    squareBracketsBalance++;
                    break;
                case JsonTokenType.EndArray:
                    squareBracketsBalance--;
                    break;
                case JsonTokenType.StartObject:
                    parameters.Add(GetParameter(ref reader));
                    break;
            }
            
        } while (squareBracketsBalance is not 0);
        
        return parameters;
    }

    private KeyValuePair<string, string> GetParameter(ref Utf8JsonReader reader)
    {
        var isParameterNameExist = false;
        var isParameterValueExist = false;
        
        var parameterKey = string.Empty;
        var parameterValue = string.Empty;
        
        byte parameterCurlyBraceBalance = 0;

        do
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    parameterCurlyBraceBalance++;
                    break;
                case JsonTokenType.EndObject:
                    parameterCurlyBraceBalance--;
                    if (parameterCurlyBraceBalance is 0) continue;
                    break;
                case JsonTokenType.PropertyName:
                    if (reader.ValueTextEquals(NameUtf8))
                    {
                        isParameterNameExist = true;
                        parameterKey = GetParameterName(ref reader);
                        break;
                    }
                    if (reader.ValueTextEquals(ExampleUtf8))
                    {
                        isParameterValueExist = true;
                        parameterValue = GetParameterValue(ref reader);
                    }
                    break;
            }
            
            reader.Read();

        } while (parameterCurlyBraceBalance is not 0);

        if (isParameterNameExist is false || isParameterValueExist is false)
        {
            throw new InvalidDataException(
                "В объекте \"Parameters\", в одном из параметров отсутствует поле \"name\" или \"example\"");
        }
        
        return new KeyValuePair<string, string>(parameterKey, parameterValue);
    }

    private string GetParameterName(ref Utf8JsonReader reader)
    {
        reader.Read();

        if (reader.TokenType is not JsonTokenType.String)
        {
            throw new ArgumentException(
                "В объекте \"Parameters\", один из параметров содержит неверное значение в поле \"name\"");
        }
            
        return reader.GetString()!;
    }
    
    private string GetParameterValue(ref Utf8JsonReader reader)
    {
        reader.Read();

        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString()!;
            case JsonTokenType.Number:
                return reader.GetDouble().ToString();
            case JsonTokenType.True:
                return true.ToString();
            case JsonTokenType.False:
                return false.ToString();
            case JsonTokenType.Null:
                return "null";
            default:
                throw new ArgumentException(
                    "В объекте \"Parameters\", один из параметров содержит неверное значение в поле \"value\"");

        }
    }
    
    private string TryGetRequestBody(ref Utf8JsonReader reader)
    {
        byte requestBodyCurlyBraceBalance = 0;
        byte exampleCurlyBraceBalance = 0;
        
        var requestBody = new StringBuilder();

        do
        {
            reader.Read();
            
            if (reader.TokenType is JsonTokenType.StartObject)
            {
                requestBodyCurlyBraceBalance++;
                continue;
            }
            
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                requestBodyCurlyBraceBalance--;
                continue;
            }

            if (reader.TokenType is JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals(ExampleUtf8))
                {
                    do
                    {
                        reader.Read();
                        
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.StartObject:
                                requestBodyCurlyBraceBalance++;
                                exampleCurlyBraceBalance++;
                                requestBody.Append('{');
                                break;
                            
                            case JsonTokenType.EndObject:
                                requestBodyCurlyBraceBalance--;
                                exampleCurlyBraceBalance--;
                                requestBody.Append('}');
                                break;
                            
                            case JsonTokenType.StartArray:
                                requestBody.Append('[');
                                break;
                            
                            case JsonTokenType.EndArray:
                                requestBody.Append(']');
                                break;
                            
                            case JsonTokenType.PropertyName:
                                if (requestBody[^1] is '[' || requestBody[^1] is '{')
                                {
                                    requestBody.Append($"\"{reader.GetString()}\":");
                                    break;
                                }
                                requestBody.Append($",\"{reader.GetString()}\":");
                                break;
                            
                            case JsonTokenType.String:
                                requestBody.Append($"\"{reader.GetString()}\"");
                                break;
                            
                            case JsonTokenType.Number:
                                requestBody.Append($"{reader.GetDouble()}");
                                break;
                        }
                    } while (exampleCurlyBraceBalance is not 0);

                    if (requestBody is not default(StringBuilder))
                    {
                        return requestBody.ToString();
                    }
                }
            }
        } while (requestBodyCurlyBraceBalance is not 0);

        return requestBody.ToString();
    }
    
    /// <summary>
    /// Создать метод который будет определять границы блока
    /// и мб возвращать текущий элемент. Типо в цикле вызываем метод, получаем текущий элемент
    /// в теле цикла выполняем нужные действия
    /// </summary>
    private void BorderCounter() {}
}