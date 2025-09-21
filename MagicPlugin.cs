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
    
    private static SSTextArea? _selectedColorTextArea;

    // A refaire
    public static HashSet<ReferenceHub> BusyCasting = new();
    public static Dictionary<ReferenceHub, DateTime> CastingIceSpike = new();
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
            new SSGroupHeader((int)SettingId.Title, "Sort"),
            new SSKeybindSetting((int)SettingId.FireBall,"Boule de feu",allowSpectatorTrigger: false),
            new SSKeybindSetting((int)SettingId.ExplosiveBall, "Boule explosive", allowSpectatorTrigger: false),
            new SSKeybindSetting((int)SettingId.IceSpike, "Peak de glace", allowSpectatorTrigger: false),
            new SSKeybindSetting((int)SettingId.Heal, "Soiiin !", allowSpectatorTrigger: false),
#if DEBUG
            new SSGroupHeader((int)SettingId.DebugTitle, "Sort DEBUG"),
            _selectedColorTextArea = new SSTextArea((int)SettingId.DebugColorInfo, "Couleur choisie: None", SSTextArea.FoldoutMode.NotCollapsable, null, (TextAlignmentOptions)257),
            new SSPlaintextSetting((int)SettingId.DebugColorInput, "Couleur (R G B)", "...", 12, TMP_InputField.ContentType.Standard, "Vide pour le défaut, sinon trois chifre entre 0 et 255."),
#endif
        ];
        ServerSpecificSettingsSync.SendToAll();
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += ProcessUserInput;
    }

    private void ProcessUserInput(ReferenceHub sender, ServerSpecificSettingBase setting)
    {
        switch (setting.SettingId)
        {


            case (int)SettingId.FireBall:
                if (setting is SSKeybindSetting { SyncIsPressed: true })
                {
                    if (BusyCasting.Contains(sender)) return;
                    var speed = 40;
                    CastHelper.FireBall(GetPositionCameraForward(sender), sender.PlayerCameraReference.forward, speed, sender);
                    Logger.Info($"La boule de feu de {sender.GetNickname()} à été envoyer en {GetPositionCameraForward(sender)}");
                }
                break;

            case (int)SettingId.ExplosiveBall:
                if (setting is SSKeybindSetting { SyncIsPressed: true })
                {
                    if (BusyCasting.Contains(sender)) return;
                    var speed = 10;
                    CastHelper.ExplosivBall(GetPositionCameraForward(sender), sender.PlayerCameraReference.forward, speed, sender);
                    Logger.Info($"La boule explosive de {sender.GetNickname()} à été envoyer en {GetPositionCameraForward(sender)}");
                }
                break;

            case (int)SettingId.IceSpike:
                if (setting is SSKeybindSetting { SyncIsPressed: true })
                {
                    if (CastingIceSpike.TryGetValue(sender, out var time))
                    {
                        BusyCasting.Remove(sender);
                        CastingIceSpike.Remove(sender);
                        var speed = 1; // (DateTime.Now - time).Seconds;
                        CastHelper.IceBall(GetPositionCameraForward(sender), sender.PlayerCameraReference.forward, speed, sender);
                        sender.playerEffectsController.DisableEffect<Slowness>();
                        Logger.Info($"La pique de glace de {sender.GetNickname()} à été envoyer en {GetPositionCameraForward(sender)}");
                        return;
                    }

                    if (BusyCasting.Contains(sender)) return;
                    var slowness = sender.playerEffectsController.EnableEffect<Slowness>();
                    slowness.Intensity = 100;
                    CastingIceSpike.Add(sender, DateTime.Now);
                    Logger.Info($"La pique de glace de {sender.GetNickname()} ce prépare");
                }
                break;
            case (int)SettingId.Heal:
                if (setting is SSKeybindSetting { SyncIsPressed: true })
                {
                    if (BusyCasting.Contains(sender)) return;

                    CastHelper.Heal(sender.PlayerCameraReference.position, null, 3);
                    Logger.Info($"La soins de {sender.GetNickname()} à été envoyer");
                }
                break;
            // Confusion, le mec est blind + marche a l'envère

            case (int)SettingId.DebugColorInput:
                CastHelper.ExplosiveBallLightColor = GetColorForUser(sender);
                _selectedColorTextArea?.SendTextUpdate(GetColorInfoForUser(sender), receiveFilter: (hub) => hub == sender) ;
                break;
        }
    }

    private Vector3 GetPositionCameraForward(ReferenceHub player)
    {
        var result = player.PlayerCameraReference.position; //+ player.PlayerCameraReference.forward * 3;
        result += player.PlayerCameraReference.forward * player.transform.localScale.z;
        return result;
    }

    private string GetColorInfoForUser(ReferenceHub hub)
    {
        Color colorForUser = GetColorForUser(hub);
        return "Selected color: <color=" + colorForUser.ToHex() + ">███████████</color>";
    }

    private Color GetColorForUser(ReferenceHub user)
    {
        string[] array = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(user, (int)SettingId.DebugColorInput).SyncInputText.Split(' ');
        if (array.Length == 0)
            return new Color(0.75f, 0.5f, 0, 0.5f);
        float r = (array.TryGet(0, out string element1) && float.TryParse(element1, out float result1)) ? (result1 / 255f) : 0;
        float g = (array.TryGet(1, out string element2) && float.TryParse(element2, out float result2)) ? (result2 / 255f) : 0;
        float b = (array.TryGet(2, out string element3) && float.TryParse(element3, out float result3)) ? (result3 / 255f) : 0;
        return new Color(r, g, b);
    }
    #endregion
}

enum SettingId
{
    Title = 1,
    FireBall,
    ExplosiveBall,
    IceSpike,
    Heal,
    DebugTitle = 50,
    DebugColorInfo,
    DebugColorInput,
}