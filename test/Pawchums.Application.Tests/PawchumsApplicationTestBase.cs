using Volo.Abp.Modularity;

namespace Pawchums;

public abstract class PawchumsApplicationTestBase<TStartupModule> : PawchumsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
