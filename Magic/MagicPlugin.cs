using System.Configuration;
using CustomPlayerEffects;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using Respawning.Objectives;
using TMPro;
using UserSettings.ServerSpecific;

namespace Magic;

internal class MagicPlugin : Plugin
{
    #region Properties & Variables
    public override string Author => PluginInfo.PLUGIN_AUTHORS;
    public override string Description => PluginInfo.PLUGIN_DESCRIPTION;
    public EventsHandler Events { get; } = new();
    public override string Name => PluginInfo.PLUGIN_NAME;
    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
    public override Version Version { get; } = new Version(PluginInfo.PLUGIN_VERSION);
    #endregion

    #region Methods
    public override void Enable()
    {
        CustomHandlersManager.RegisterEventsHandler(Events);
        MagicUserSettings.Register();
        ManaStat.Register();

        /*CoolDown cd = new CoolDown(10);
        Timing.CallDelayed(12, () =>
        {
            cd.Start();
            Logger.Info(cd.IsActive);
            Logger.Info(cd.CurrentCooldown);

            Timing.CallDelayed(5, () =>
            {
                Logger.Info(cd.IsActive);
            });

            Timing.CallDelayed(12, () =>
            {
                Logger.Info(cd.IsActive);
            });
        });*/

    }

    public override void Disable()
    {
        CustomHandlersManager.UnregisterEventsHandler(Events);
        MagicUserSettings.UnRegister();
        ManaStat.UnRegister();
    }
    #endregion
}

