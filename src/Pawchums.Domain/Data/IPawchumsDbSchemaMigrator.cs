using System.Threading.Tasks;

namespace Pawchums.Data;

public interface IPawchumsDbSchemaMigrator
{
    Task MigrateAsync();
}
