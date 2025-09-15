using System.Net;
using FluentAssertions;
using Xunit;

namespace INSY7315.Tests;

public class ExportCsvTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;
    public ExportCsvTests(TestAppFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Export_Returns_Csv_With_Header()
    {
        var res = await _client.GetAsync("/api/products/export.csv");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("Id,Name,Owner,Category,Model,Price,CreatedOn");
    }
}
