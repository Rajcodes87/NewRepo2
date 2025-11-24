using Xunit;

namespace Pawchums.EntityFrameworkCore;

[CollectionDefinition(PawchumsTestConsts.CollectionDefinitionName)]
public class PawchumsEntityFrameworkCoreCollection : ICollectionFixture<PawchumsEntityFrameworkCoreFixture>
{

}
