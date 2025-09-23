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

    #endregion
}