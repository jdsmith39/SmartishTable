using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using SmartishTable.Samples.Shared;
using System.Net.Http.Headers;


namespace SmartishTable.Tests.Client
{
    /// <summary>
    /// Base class for client project tests conducted with bUnit
    /// </summary>
    public abstract class TestBase : TestContext
    {
        protected TestBase()
        {
            MockApiCalls();
        }

        /// <summary>
        /// A set of mocked api calls and responses used to unit test the client project so no interaction with the
        /// server's web api is required.
        /// </summary>
        private void MockApiCalls()
        {
            var mockHttpMessageHandler = Services.AddMockHttpClient();

            mockHttpMessageHandler.When("/People").RespondJson(PeopleData);

            mockHttpMessageHandler.When("/People/AFew").RespondJson(PeopleData.Take(20));

            mockHttpMessageHandler.When("/People/AFewMore").RespondJson(PeopleData.Skip(10).Take(30));

            mockHttpMessageHandler.When("/People/Some/*").RespondJson(PeopleData.Take(10));
        }

        public List<Person> PeopleData = JsonSerializer.Deserialize<List<Person>>(System.IO.File.ReadAllText("data.json"));
    }

    /// <summary>
    /// Helper methods for creating HttpClient mocks
    /// </summary>
    public static class MockHttpClientBunitHelpers
    {
        public static MockHttpMessageHandler AddMockHttpClient(this TestServiceProvider services)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var httpClient = mockHttpHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");
            services.AddSingleton(httpClient);
            return mockHttpHandler;
        }

        public static MockedRequest RespondJson<T>(this MockedRequest request, T content)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(content))
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });
            return request;
        }

        public static MockedRequest RespondJson<T>(this MockedRequest request, Func<T> contentProvider)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(contentProvider()))
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });
            return request;
        }
    }
}
