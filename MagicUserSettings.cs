using CustomPlayerEffects;
using Respawning.Objectives;
using UserSettings.ServerSpecific;
using static UnityEngine.Rendering.RayTracingAccelerationStructure;

namespace Magic;

public static class MagicUserSettings
{
    // A refaire
    public static HashSet<ReferenceHub> BusyCasting = new();
    public static Dictionary<ReferenceHub, DateTime> CastingIceSpike = new();

    public static bool IsRegister { get; private set; } = false;

    private static ServerSpecificSettingBase[] _instance = [
            new SSGroupHeader((int)Id.Title, "Sort"),
            new SSKeybindSetting((int)Id.FireBall,"Boule de feu",allowSpectatorTrigger: false),
            new SSKeybindSetting((int)Id.ExplosiveBall, "Boule explosive", allowSpectatorTrigger: false),
            new SSKeybindSetting((int)Id.IceSpike, "Peak de glace", allowSpectatorTrigger: false),
            new SSKeybindSetting((int)Id.Heal, "Soiiin !", allowSpectatorTrigger: false),
#if DEBUG
            new SSGroupHeader((int)Id.DebugTitle, "Sort DEBUG"),
            _selectedColorTextArea = new SSTextArea((int)Id.DebugColorInfo, "Couleur choisie: None", SSTextArea.FoldoutMode.NotCollapsable, null, (TextAlignmentOptions)257),
            new SSPlaintextSetting((int)Id.DebugColorInput, "Couleur (R G B)", "...", 12, TMP_InputField.ContentType.Standard, "Vide pour le défaut, sinon trois chifre entre 0 et 255."),
#endif
    ];

    private static SSTextArea? _selectedColorTextArea;


    internal static void Register()
    {
        if (IsRegister) return;
        ServerSpecificSettingsSync.DefinedSettings = ServerSpecificSettingsSync.DefinedSettings.AddRangeToArray(_instance);
        //Logger.Info($"chuis la {ServerSpecificSettingsSync.DefinedSettings.Length}");
        ServerSpecificSettingsSync.SendToAll();
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += ProcessUserInput;
        IsRegister = true;

    }

    internal static void UnRegister()
    {
        IsRegister = false;
        ServerSpecificSettingsSync.DefinedSettings = ServerSpecificSettingsSync.DefinedSettings.Except(_instance).ToArray();
        ServerSpecificSettingsSync.SendToAll();
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= ProcessUserInput;
    }

    private static void ProcessUserInput(ReferenceHub sender, ServerSpecificSettingBase setting)
    {
        switch (setting.SettingId)
        {
            // TODO: generaliser la chose en une class Spell
            case (int)Id.FireBall:
                if (setting is SSKeybindSetting { SyncIsPressed: true })
                {
                    if (BusyCasting.Contains(sender)) return;
                    var speed = 20;
                    CastHelper.FireBall(GetPositionCameraForward(sender), sender.PlayerCameraReference.forward, speed, sender);
                    Logger.Info($"La boule de feu de {sender.GetNickname()} à été envoyer en {GetPositionCameraForward(sender)}");
                }
                break;

            case (int)Id.ExplosiveBall:
                if (setting is SSKeybindSetting { SyncIsPressed: true })
                {
                    if (BusyCasting.Contains(sender)) return;
                    var speed = 10;
                    CastHelper.ExplosivBall(GetPositionCameraForward(sender), sender.PlayerCameraReference.forward, speed, sender);
                    Logger.Info($"La boule explosive de {sender.GetNickname()} à été envoyer en {GetPositionCameraForward(sender)}");
                }
                break;

            case (int)Id.IceSpike:
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
            case (int)Id.Heal:
                if (setting is SSKeybindSetting { SyncIsPressed: true })
                {
                    if (BusyCasting.Contains(sender)) return;

                    CastHelper.Heal(sender.PlayerCameraReference.position, null, 3);
                    Logger.Info($"La soins de {sender.GetNickname()} à été envoyer");
                }
                break;
            // Confusion, le mec est blind + marche a l'envère

            case (int)Id.DebugColorInput:
                CastHelper.ExplosiveBallLightColor = GetColorForUser(sender);
                _selectedColorTextArea?.SendTextUpdate(GetColorInfoForUser(sender), receiveFilter: (hub) => hub == sender);
                break;
        }
    }
    
    private static Vector3 GetPositionCameraForward(ReferenceHub player)
    {
        var result = player.PlayerCameraReference.position; //+ player.PlayerCameraReference.forward * 3;
        result += player.PlayerCameraReference.forward * player.transform.localScale.z;
        return result;
    }

    private static string GetColorInfoForUser(ReferenceHub hub)
    {
        Color colorForUser = GetColorForUser(hub);
        return "Selected color: <color=" + colorForUser.ToHex() + ">███████████</color>";
    }

    private static Color GetColorForUser(ReferenceHub user)
    {
        string[] array = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(user, (int)Id.DebugColorInput).SyncInputText.Split(' ');
        if (array.Length == 0)
            return new Color(0.75f, 0.5f, 0, 0.5f);
        float r = (array.TryGet(0, out string element1) && float.TryParse(element1, out float result1)) ? (result1 / 255f) : 0;
        float g = (array.TryGet(1, out string element2) && float.TryParse(element2, out float result2)) ? (result2 / 255f) : 0;
        float b = (array.TryGet(2, out string element3) && float.TryParse(element3, out float result3)) ? (result3 / 255f) : 0;
        return new Color(r, g, b);
    }

    public enum Id
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
}
