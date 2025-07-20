using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;

public class MessageIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MessageIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();

        // Optional: add auth header for testing if needed
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

}
