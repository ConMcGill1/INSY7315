using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using INSY7315.Data;
using INSY7315.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace INSY7315.Tests;

public class ProductApiTests : IClassFixture<TestAppFactory>
{
    private readonly TestAppFactory _factory;
    private readonly HttpClient _client;

    public ProductApiTests(TestAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private AppDbContext CreateDb() =>
        _factory.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

    private sealed record IdResponse(int Id);

    private static async Task<string> ReadBody(HttpResponseMessage resp) =>
        await resp.Content.ReadAsStringAsync();

    [Fact]
    public async Task Post_Creates_Product_And_Persists_To_Database()
    {
        var dto = new ProductDto(null, "iPhone 14", 19999m, "CoreThree", "A2882", "Phones");

        var post = await _client.PostAsJsonAsync("/api/products", dto);

     
        if (post.StatusCode != HttpStatusCode.OK)
        {
            var body = await ReadBody(post);
            throw new Xunit.Sdk.XunitException($"POST failed: {(int)post.StatusCode} {post.StatusCode}\nBody:\n{body}");
        }

        var idObj = await post.Content.ReadFromJsonAsync<IdResponse>();
        idObj.Should().NotBeNull();
        idObj!.Id.Should().BeGreaterThan(0);

        using var db = CreateDb();
        var fromDb = await db.Products.FindAsync(idObj.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("iPhone 14");
        fromDb.Price.Should().Be(19999m);
        fromDb.Owner.Should().Be("CoreThree");
    }

    [Fact]
    public async Task Put_Updates_Product_And_Writes_PriceHistory_When_Price_Changes()
    {
       
        var create = new ProductDto(null, "Dell Laptop", 8999m, "CoreThree", "XPS", "Computers");
        var post = await _client.PostAsJsonAsync("/api/products", create);
        if (post.StatusCode != HttpStatusCode.OK)
        {
            var body = await ReadBody(post);
            throw new Xunit.Sdk.XunitException($"POST failed: {(int)post.StatusCode} {post.StatusCode}\nBody:\n{body}");
        }
        var idObj = await post.Content.ReadFromJsonAsync<IdResponse>();
        idObj!.Id.Should().BeGreaterThan(0);

       
        var update = new ProductDto(idObj.Id, "Dell Laptop", 7999m, "CoreThree", "XPS", "Computers");
        var put = await _client.PutAsJsonAsync($"/api/products/{idObj.Id}", update);
        if (put.StatusCode != HttpStatusCode.OK)
        {
            var body = await ReadBody(put);
            throw new Xunit.Sdk.XunitException($"PUT failed: {(int)put.StatusCode} {put.StatusCode}\nBody:\n{body}");
        }

    
        using var db = CreateDb();
        var p = await db.Products.FindAsync(idObj.Id);
        p.Should().NotBeNull();
        p!.Price.Should().Be(7999m);

        var history = db.PriceHistories.Where(h => h.ProductId == p.Id)
                                       .OrderByDescending(h => h.ChangedOn)
                                       .ToList();
        history.Should().HaveCount(1);
        history[0].OldPrice.Should().Be(8999m);
        history[0].NewPrice.Should().Be(7999m);
    }

    [Fact]
    public async Task Delete_Removes_Product_From_Database()
    {
        var create = new ProductDto(null, "Galaxy Tab", 4999m, "CoreThree", "S7", "Tablets");
        var post = await _client.PostAsJsonAsync("/api/products", create);
        if (post.StatusCode != HttpStatusCode.OK)
        {
            var body = await ReadBody(post);
            throw new Xunit.Sdk.XunitException($"POST failed: {(int)post.StatusCode} {post.StatusCode}\nBody:\n{body}");
        }
        var idObj = await post.Content.ReadFromJsonAsync<IdResponse>();

        var del = await _client.DeleteAsync($"/api/products/{idObj!.Id}");
        if (del.StatusCode != HttpStatusCode.OK)
        {
            var body = await ReadBody(del);
            throw new Xunit.Sdk.XunitException($"DELETE failed: {(int)del.StatusCode} {del.StatusCode}\nBody:\n{body}");
        }

        using var db = CreateDb();
        var removed = await db.Products.FindAsync(idObj.Id);
        removed.Should().BeNull();
    }
}
