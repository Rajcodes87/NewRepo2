using Volo.Abp.Modularity;

namespace Pawchums;

[DependsOn(
    typeof(PawchumsApplicationModule),
    typeof(PawchumsDomainTestModule)
)]
public class PawchumsApplicationTestModule : AbpModule
{

}
