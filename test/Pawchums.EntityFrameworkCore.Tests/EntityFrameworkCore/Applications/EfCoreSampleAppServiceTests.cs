using Pawchums.Samples;
using Xunit;

namespace Pawchums.EntityFrameworkCore.Applications;

[Collection(PawchumsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<PawchumsEntityFrameworkCoreTestModule>
{

}
