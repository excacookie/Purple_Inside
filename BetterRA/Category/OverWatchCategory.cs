using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public override string GetInfo(CommandSender sender, bool secondPage)
    {
        var text = "<color=white>Selects all Players that are currently in OverWatch:\n";

        foreach (var player in GetPlayers())
        {
            text += player.nicknameSync.MyNick + "\n";
        }

        return text + "</color>";
    }

    public override List<ReferenceHub> GetPlayers() => ReferenceHub.AllHubs.Where(p => p.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip)).ToList();

    public override bool DisplayOnTop => false;

    public override bool CanSeeCategory(ReferenceHub player) => ReferenceHub.AllHubs.Any(p => p.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip));
}