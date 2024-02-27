using System.Collections;
using System.Net;
using SwaggerRequests;

namespace UnitTests.OpenApiRequestTests;

public class OpenApiRequestPositive
{
    /// <summary>
    /// Отправить запрос, который содержит 1 параметр
    /// </summary>
    [Theory]
    [ClassData(typeof(OpenApiRequestJson))]
    public async Task SendRequest_Should_ReturnStatusCode200_WhenParameter(string requestJson)
    {
        var baseUri = new Uri("http://localhost:5214/");
        var requestClient = new OpenApiRequestService(baseUri);
        
        var responseStatusCode = await requestClient.SendRequestAsync("/Example/OnePrimitiveParameter", requestJson);
        Assert.Equal(HttpStatusCode.OK, responseStatusCode);
    }
}

public class OpenApiRequestJson : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // Запрос содержащий 1 параметр.
        yield return new object[]
        {
            """
            {
                "get": {
                    "tags": [
                        "Example"
                    ],
                    "parameters": [
                        {
                            "name": "firstValue",
                            "in": "query",
                            "description": "",
                            "style": "form",
                            "schema": {
                                "type": "integer",
                                "format": "int32"
                            },
                            "example": 7
                        }
                    ],
                    "responses": {
                        "200": {
                            "description": "Операция выполнена"
                        }
                    }
                }
            }
            """
        };

        
        // Запрос содержащий 1 параметр
        // Допускает нарушение спецификации OpenApi,
        // чтобы объект "parameters" имел только свойства "name" и "example"
        yield return new object[]
        {
           """
           {
              {
               "get": {
                   "tags": [
                       "Example"
                   ],
                   "parameters": [
                       {
                           "name": "firstValue",
                           "example": 7
                       }
                   ],
                   "responses": {
                       "200": {
                           "description": "Операция выполнена"
                       }
                   }
               }
           }
           """
        };

    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}