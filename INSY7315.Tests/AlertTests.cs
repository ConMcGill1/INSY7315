using System.Net.Http.Json;
using FluentAssertions;
using INSY7315.Models;
using Xunit;

namespace INSY7315.Tests;

public class AlertTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;
    public AlertTests(TestAppFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Big_Price_Change_Creates_Alert()
    {
        var post = await _client.PostAsJsonAsync("/api/products", new Product { Name = "X", Owner = "Y", Price = 100m });
        var created = await post.Content.ReadFromJsonAsync<Product>();

        created!.Price = 130m; // +30% change
        await _client.PutAsJsonAsync($"/api/products/{created.Id}", created);

        var alerts = await _client.GetFromJsonAsync<List<Alert>>("/api/alerts");
        alerts!.Any(a => a.ProductId == created.Id && a.DeltaPercent >= 10m).Should().BeTrue();
    }
}
