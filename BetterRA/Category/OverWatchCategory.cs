using PlayerStatsSystem;

namespace BetterRA.Category;

[RaCategory(
    Name = "[ OverWatch]",
    Color = "#03f8fc",
    Size = 20,
    Id = 101
    )]
public class OverWatchCategory : RaCategory
{
    public override bool DisplayOnTop => false;

    public override string GetInfo(CommandSender sender, bool secondPage)
    {
        var text = "<color=white>Selects all Players that are currently in OverWatch:\n";

        foreach (var player in GetPlayers())
        {
            text += player.nicknameSync.MyNick + "\n";
        }

        return text + "</color>";
    }

    public override List<ReferenceHub> GetPlayers() => ReferenceHub.AllHubs.Where(IsOvewatch).ToList();
    public override bool CanSeeCategory(ReferenceHub player) => ReferenceHub.AllHubs.Any(IsOvewatch);
    private bool IsOvewatch(ReferenceHub player) => player.serverRoles.IsInOverwatch;
}