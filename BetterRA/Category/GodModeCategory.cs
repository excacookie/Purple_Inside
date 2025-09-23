namespace BetterRA.Category;

[RaCategory(
    Name = "[ GodMode]",
    Color = "#03f8fc",
    Size = 20,
    Id = 103
    )]
public class GodModeCategory : RaCategory
{
    public override string GetInfo(CommandSender sender, bool secondPage)
    {
        var text = "<color=white>Selects all Players that are currently in GodMode:\n";

        foreach (var player in GetPlayers())
        {
            text += player.nicknameSync.MyNick + "\n";
        }

        return text + "</color>";
    }

    public override List<ReferenceHub> GetPlayers() => ReferenceHub.AllHubs.Where(x => x.characterClassManager.GodMode).ToList();

    public override bool DisplayOnTop => false;

    public override bool CanSeeCategory(ReferenceHub player) => ReferenceHub.AllHubs.Any(x => x.characterClassManager.GodMode);
}