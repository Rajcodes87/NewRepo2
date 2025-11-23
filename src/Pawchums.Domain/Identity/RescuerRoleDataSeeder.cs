using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace Pawchums.Identity;

public class RescuerRoleDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;

    public RescuerRoleDataSeeder(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Create Rescuer role if it doesn't exist
        var rescuerRole = await _roleRepository.FindByNormalizedNameAsync("RESCUER");
        if (rescuerRole == null)
        {
            rescuerRole = new IdentityRole(
                id: System.Guid.NewGuid(),
                name: "Rescuer",
                tenantId: null
            )
            {
                IsStatic = true,
                IsPublic = true,
                IsDefault = false
            };

            await _roleManager.CreateAsync(rescuerRole);
        }
    }
}
