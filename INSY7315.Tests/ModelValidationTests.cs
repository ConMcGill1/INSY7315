using System.ComponentModel.DataAnnotations;
using FluentAssertions;
// TODO: change to your actual model namespace:
using INSY7315.Models;
using Xunit;

namespace INSY7315.Tests;

public class ModelValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Product_Required_And_Range_Validated()
    {
        var p = new Product { Name = "", Owner = "", Price = -1 };
        var results = Validate(p);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Product.Name)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(Product.Owner)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(Product.Price)));
    }
}
