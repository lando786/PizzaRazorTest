using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace PizzaApp.UITests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class SeleniumCollection : ICollectionFixture<PizzaAppFixture>
{
    public const string Name = "Selenium UI";
}
