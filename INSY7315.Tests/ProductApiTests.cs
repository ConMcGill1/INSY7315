using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
// TODO: change to your actual model namespace:
using INSY7315.Models;
using Xunit;

namespace INSY7315.Tests;

public class ProductApiTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;

    public ProductApiTests(TestAppFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Post_Then_Get_By_Id_Works()
    {
        var p = new Product { Name = "iPhone 14", Owner = "CoreThree", Price = 19999, Category = "Phones", Model = "A2882" };

        var post = await _client.PostAsJsonAsync("/api/products", p);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await post.Content.ReadFromJsonAsync<Product>();
        created!.Id.Should().BeGreaterThan(0);

        var get = await _client.GetAsync($"/api/products/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await get.Content.ReadFromJsonAsync<Product>();
        fetched!.Name.Should().Be("iPhone 14");
        fetched.Price.Should().Be(19999);
    }

    [Fact]
    public async Task Put_Changes_Price_And_Appends_History_When_Endpoint_Exists()
    {
        var p = new Product { Name = "Galaxy S24", Owner = "CoreThree", Price = 14999 };
        var post = await _client.PostAsJsonAsync("/api/products", p);
        var created = await post.Content.ReadFromJsonAsync<Product>();

        created!.Price = 15999;
        var put = await _client.PutAsJsonAsync($"/api/products/{created.Id}", created);
        put.EnsureSuccessStatusCode();

        var hist = await _client.GetAsync($"/api/products/{created.Id}/history");
        if (hist.StatusCode == HttpStatusCode.OK)
        {
            var items = await hist.Content.ReadFromJsonAsync<List<object>>();
            items!.Count.Should().BeGreaterThan(0);
        }
        else
        {
            var get = await _client.GetFromJsonAsync<Product>($"/api/products/{created.Id}");
            get!.Price.Should().Be(15999);
        }
    }

    [Fact]
    public async Task Delete_Removes_Product()
    {
        var p = new Product { Name = "Budget Phone", Owner = "Shop", Price = 1999 };
        var post = await _client.PostAsJsonAsync("/api/products", p);
        var created = await post.Content.ReadFromJsonAsync<Product>();

        var del = await _client.DeleteAsync($"/api/products/{created!.Id}");
        del.EnsureSuccessStatusCode();

        var get = await _client.GetAsync($"/api/products/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
