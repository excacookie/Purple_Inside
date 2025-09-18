using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using Respawning.Objectives;
using UserSettings.ServerSpecific;

namespace Magic;

internal class HelloWorldPlugin : Plugin
{
    #region Properties & Variables
    public override string Author => PluginInfo.PLUGIN_AUTHORS;
    public override string Description => PluginInfo.PLUGIN_DESCRIPTION;
    public MyCustomEventsHandler Events { get; } = new();
    public override string Name => PluginInfo.PLUGIN_NAME;
    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
    public override Version Version { get; } = new Version(PluginInfo.PLUGIN_VERSION);
    #endregion

    #region Methods
    public override void Disable()
    {
        CustomHandlersManager.UnregisterEventsHandler(Events);
    }

    public override void Enable()
    {
        CustomHandlersManager.RegisterEventsHandler(Events);
        ServerSpecificSettingsSync.DefinedSettings = [
            new SSGroupHeader(1, "Sort"),
            new SSKeybindSetting(2, "boulle de feu", allowSpectatorTrigger: false)
        ];
        ServerSpecificSettingsSync.SendToAll();
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += ProcessUserInput;
    }

    private void ProcessUserInput(ReferenceHub sender, ServerSpecificSettingBase setting)
    {
        switch (setting.SettingId)
        {
            case 2:

                if (setting is SSKeybindSetting { SyncIsPressed: not false })
                {
                    CastHelper.CastFireBall(sender.PlayerCameraReference.position, sender.PlayerCameraReference.forward, caster: sender);
                    Logger.Info($"La boule de feu de {sender.GetNickname()} à été envoyer");
                }
                break;
        }

    }
    #endregion
}