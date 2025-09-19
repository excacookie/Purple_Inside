using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;

namespace Magic;

public class EventsHandler : CustomEventsHandler
{
    #region Methods
    public override void OnPlayerDeath(PlayerDeathEventArgs ev)
    {
        MagicPlugin.BusyCasting.Remove(ev.Player.ReferenceHub);
        MagicPlugin.CastingIceSpike.Remove(ev.Player.ReferenceHub);
    }

    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        Logger.Info($"joueure {ev.Player.DisplayName} à rejoint le serveur!");
        ev.Player.SendBroadcast($"<color=#FF36F9><b>{ev.Player.DisplayName}</b> bienvenue sur <b>Purple Inside</b>!</color>", 10);
    }
    #endregion
}