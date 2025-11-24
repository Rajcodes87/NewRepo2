using Volo.Abp.Modularity;

namespace Pawchums;

[DependsOn(
    typeof(PawchumsDomainModule),
    typeof(PawchumsTestBaseModule)
)]
public class PawchumsDomainTestModule : AbpModule
{

}
