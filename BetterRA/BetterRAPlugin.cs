using System.Configuration;
using BetterRA.Category;
using CustomPlayerEffects;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using Respawning.Objectives;
using TMPro;
using UserSettings.ServerSpecific;

namespace BetterRA;

internal class BetterRAPlugin : Plugin
{
    #region Properties & Variables
    public Harmony Harmony { get; } = new Harmony(PluginInfo.PLUGIN_GUID);

    public override string Author => PluginInfo.PLUGIN_AUTHORS;
    public override string Description => PluginInfo.PLUGIN_DESCRIPTION;
    public override string Name => PluginInfo.PLUGIN_NAME;
    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
    public override Version Version { get; } = new Version(PluginInfo.PLUGIN_VERSION);
    #endregion

    #region Methods
    public override void Enable()
    {
        RaCategoryService.Register();
        Harmony.PatchAll();
    }

    public override void Disable()
    {

    }
    #endregion
}