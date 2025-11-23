using System;
using System.Collections.Generic;
using System.Text;
using Pawchums.Localization;
using Volo.Abp.Application.Services;

namespace Pawchums;

/* Inherit your application services from this class.
 */
public abstract class PawchumsAppService : ApplicationService
{
    protected PawchumsAppService()
    {
        LocalizationResource = typeof(PawchumsResource);
    }
}
