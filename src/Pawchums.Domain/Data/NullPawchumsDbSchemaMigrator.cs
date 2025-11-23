using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Pawchums.Data;

/* This is used if database provider does't define
 * IPawchumsDbSchemaMigrator implementation.
 */
public class NullPawchumsDbSchemaMigrator : IPawchumsDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
