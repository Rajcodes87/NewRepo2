using Volo.Abp.Modularity;

namespace Pawchums;

/* Inherit from this class for your domain layer tests. */
public abstract class PawchumsDomainTestBase<TStartupModule> : PawchumsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
