using Microsoft.Extensions.Localization;
using Pawchums.Localization;
using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace Pawchums;

[Dependency(ReplaceServices = true)]
public class PawchumsBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<PawchumsResource> _localizer;

    public PawchumsBrandingProvider(IStringLocalizer<PawchumsResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
