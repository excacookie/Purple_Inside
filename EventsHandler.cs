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

    #endregion
}