using Pawchums.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Pawchums.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class PawchumsController : AbpControllerBase
{
    protected PawchumsController()
    {
        LocalizationResource = typeof(PawchumsResource);
    }
}
