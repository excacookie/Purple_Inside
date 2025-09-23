using CommandSystem;
using LabApi.Features.Wrappers;

namespace BetterRA;

public static class CommandSenderExtensions
{
    public static bool TryGetHub(this ICommandSender sender, out ReferenceHub? hub)
    {
        if (sender is not CommandSender commandSender)
        {
            hub = null;
            return false;
        }

        return ReferenceHub.TryGetHub(1, out hub);
    }

}
