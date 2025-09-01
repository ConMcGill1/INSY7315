using System.Net;
using FluentAssertions;
using Xunit;

namespace INSY7315.Tests;

public class AppSmokeTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;
    public AppSmokeTests(TestAppFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task HomePage_Returns_Ok()
    {
        var resp = await _client.GetAsync("/");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Export_Returns_Csv_With_Header()
    {
        var resp = await _client.GetAsync("/Export");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().StartWith("Id,Name,Price,Category,Model,Owner,CreatedOn");
    }
}
