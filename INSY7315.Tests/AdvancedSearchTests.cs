using System.Net.Http.Json;
using FluentAssertions;
using INSY7315.Models;
using Xunit;

namespace INSY7315.Tests;

public class AdvancedSearchTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;
    public AdvancedSearchTests(TestAppFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Query_MinMaxPrice_Works()
    {
        await _client.PostAsJsonAsync("/api/products", new Product { Name = "Cheap", Owner = "Shop", Price = 100 });
        await _client.PostAsJsonAsync("/api/products", new Product { Name = "Mid", Owner = "Shop", Price = 1000 });
        await _client.PostAsJsonAsync("/api/products", new Product { Name = "Expensive", Owner = "Shop", Price = 10000 });

        var list = await _client.GetFromJsonAsync<List<Product>>("/api/products/search?minPrice=500&maxPrice=5000");
        list!.Select(p => p.Name).Should().BeEquivalentTo(new[] { "Mid" });
    }
}
