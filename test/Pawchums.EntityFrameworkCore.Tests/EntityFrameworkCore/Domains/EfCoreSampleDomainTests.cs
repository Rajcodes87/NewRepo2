using Pawchums.Samples;
using Xunit;

namespace Pawchums.EntityFrameworkCore.Domains;

[Collection(PawchumsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<PawchumsEntityFrameworkCoreTestModule>
{

}
