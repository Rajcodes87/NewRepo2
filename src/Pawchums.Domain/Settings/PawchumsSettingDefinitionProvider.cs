using Volo.Abp.Settings;

namespace Pawchums.Settings;

public class PawchumsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(PawchumsSettings.MySetting1));
    }
}
