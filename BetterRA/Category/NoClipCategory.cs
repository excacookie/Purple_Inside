using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;

namespace BetterRA.Category;

[RaCategory(
    Name = "[ NoClip]",
    Color = "#03f8fc",
    Size = 20,
    Id = 104
)]
public class NoClipCategory : RaCategory
{
    public override bool DisplayOnTop => false;

    public override string GetInfo(CommandSender sender, bool secondPage)
    {
        var text = "<color=white>Selects all Players that are currently NoClipping:\n";

        foreach (var player in GetPlayers())
        {
            text += player.nicknameSync.MyNick + "\n";
        }
        return text + "</color>";
    }

    public override List<ReferenceHub> GetPlayers() => ReferenceHub.AllHubs.Where(IsNoclingEnable).ToList();
    public override bool CanSeeCategory(ReferenceHub player) => ReferenceHub.AllHubs.Any(IsNoclingEnable);

    public bool IsNoclingEnable(ReferenceHub player) => player.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip);
}