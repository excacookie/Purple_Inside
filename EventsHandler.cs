using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;

namespace Magic;

public class EventsHandler : CustomEventsHandler
{
    #region Methods
    // TODO
    public override void OnPlayerDeath(PlayerDeathEventArgs ev)
    {
        MagicUserSettings.BusyCasting.Remove(ev.Player.ReferenceHub);
        MagicUserSettings.CastingIceSpike.Remove(ev.Player.ReferenceHub);
    }

    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        Logger.Info($"joueure {ev.Player.DisplayName} à rejoint le serveur!");
        ev.Player.SendBroadcast($"<color=#FF36F9><b>{ev.Player.DisplayName}</b> bienvenue sur <b>Purple Inside</b>!</color>", 10);
    }
    #endregion
}