using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;

namespace HelloWorldPlugin;

public class MyCustomEventsHandler : CustomEventsHandler
{
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        Logger.Info($"joueure {ev.Player.DisplayName} à rejoint le serveur!");
        ev.Player.SendBroadcast($"<color=#FF36F9><b>{ev.Player.DisplayName}</b> bienvenue sur <b>Purple Inside</b>!</color>", 10);
    }
}