using Xunit;

namespace DevIO.IntegrationTests.Setups.Fixtures
{
    [CollectionDefinition(nameof(InfraSingleInstanceCollection))]
    public class InfraSingleInstanceCollection : ICollectionFixture<ApiWebApplicationFactory> { }
}
