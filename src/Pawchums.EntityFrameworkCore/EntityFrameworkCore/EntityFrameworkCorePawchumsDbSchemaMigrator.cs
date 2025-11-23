using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pawchums.Data;
using Volo.Abp.DependencyInjection;

namespace Pawchums.EntityFrameworkCore;

public class EntityFrameworkCorePawchumsDbSchemaMigrator
    : IPawchumsDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCorePawchumsDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the PawchumsDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<PawchumsDbContext>()
            .Database
            .MigrateAsync();
    }
}
