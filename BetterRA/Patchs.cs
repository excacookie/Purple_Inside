using BetterRA.Category;
using CentralAuth;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using RemoteAdmin;
using RemoteAdmin.Communication;
using Utils;
using VoiceChat;

namespace BetterRA;

[HarmonyPatch]
public static class Patchs
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), typeof(CommandSender), typeof(string))]
    public static bool OnRequestPlayer(CommandSender sender, string data)
    {
        try
        {
            var args = data.Split(' ');
            if (args.Length != 2) return false;
            if (!int.TryParse(args[0], out var number)) return false;

            var playerSender = sender as PlayerCommandSender;

            if (playerSender != null)
            {
                var arg = args[1].Split('.')[0];
                if (int.TryParse(arg, out var categoryId))
                {
                    // We store category id in the RA in negative to not mess with players
                    // Old game can't handle string only int
                    var category = RaCategoryService.GetCategory(-categoryId);
                    if (category != null && category.CanSeeCategory(playerSender.ReferenceHub))
                    {
                        sender.RaReply("$1 " + category.GetInfo(sender, number == 0), true, true, string.Empty);
                        return false;
                    }
                }
            }    

            var requestSensitiveData = number == 0;

            if (requestSensitiveData && playerSender != null)
            {
                var hasSensitiveInfoPerms =
                    playerSender.ReferenceHub.authManager.RemoteAdminGlobalAccess
                    || playerSender.ReferenceHub.authManager.BypassBansFlagSet
                    || CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess);

                if (!hasSensitiveInfoPerms)
                    return false;
            }

            var players =
                RAUtils.ProcessPlayerIdOrNamesList(new ArraySegment<string>(args.Skip(1).ToArray()), 0, out _);
            if (players.Count == 0) return false;

            var allowedToSeeUserIds = PermissionsHandler.IsPermitted(sender.Permissions, 18007046UL) ||
                                      playerSender != null &&
                                      (playerSender.ReferenceHub.authManager.RemoteAdminGlobalAccess 
                                        || playerSender.ReferenceHub.authManager.BypassBansFlagSet);

            if (players.Count > 1)
            {
                var text = "<color=white>";
                text += "Selecting multiple players:";
                text += "\nPlayer ID: <color=green><link=CP_ID></link></color>";
                text += "\nIP Address: " + (requestSensitiveData
                    ? "<color=green><link=CP_IP></link></color>"
                    : "[REDACTED]");
                text += "\nUser ID: " +
                        (allowedToSeeUserIds ? "<color=green><link=CP_USERID></link></color>" : "[REDACTED]");
                text += "</color>";

                var playerIds = "";
                var playerIps = "";
                var userIds = "";

                foreach (var hub in players)
                {
                    playerIds += hub.PlayerId + ".";

                    if (requestSensitiveData)
                    {
                        playerIps += (hub.networkIdentity.connectionToClient.IpOverride != null
                            ? hub.networkIdentity.connectionToClient.OriginalIpAddress
                            : hub.networkIdentity.connectionToClient.address) + ",";
                    }

                    if (allowedToSeeUserIds)
                    {
                        userIds += hub.authManager.UserId + ".";
                    }
                }

                if (playerIds.Length > 0)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, playerIds);
                }

                if (playerIps.Length > 0)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, playerIps);
                }

                if (userIds.Length > 0)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, userIds);
                }

                sender.RaReply("$1 " + text, true, true, string.Empty);
                return false;
            }

            var seeGamePlayData = PermissionsHandler.IsPermitted(sender.Permissions, PlayerPermissions.GameplayData);
            var player = players[0];
            var connection = player.networkIdentity.connectionToClient;

            if (playerSender != null)
                playerSender.ReferenceHub.queryProcessor.GameplayData = seeGamePlayData;

            var message = "<color=white>";
            message += "Nickname: " + player.nicknameSync.CombinedName;
            message += $"\nPlayer ID: {player.PlayerId} <color=green><link=CP_ID></link></color>";
            RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, player.PlayerId.ToString());

            if (connection == null)
            {
                message += "\nIP Address: null";
            }
            else if (requestSensitiveData)
            {
                message += "\nIP Address: " + connection.address + " ";
                if (connection.IpOverride != null)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, connection.OriginalIpAddress ?? "");
                    message += " [routed via " + connection.OriginalIpAddress + "]";
                }
                else
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, connection.address ?? "");
                }

                message += " <color=green><link=CP_IP></link></color>";
            }
            else
            {
                message += "\nIP Address: [REDACTED]";
            }

            var id = string.IsNullOrWhiteSpace(player.authManager.UserId)
                ? "(none)"
                : player.authManager.UserId + " <color=green><link=CP_USERID></link></color>";


            message += "\nUser ID: " + (allowedToSeeUserIds ? id : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>");

            if (allowedToSeeUserIds)
            {
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, player.authManager.UserId ?? "");
                if (player.authManager.SaltedUserId != null &&
                    player.authManager.SaltedUserId.Contains("$"))
                {
                    message += "\nSalted User ID: " + player.authManager.SaltedUserId;
                }
            }

            message += "\nServer role: " + player.serverRoles.GetColoredRoleString();
            var seeHidden = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
            var seeGlobal = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);

            if (playerSender != null && playerSender.ReferenceHub.authManager.NorthwoodStaff)
            {
                seeHidden = true;
                seeGlobal = true;
            }

            var hasHiddenBadge = !string.IsNullOrWhiteSpace(player.serverRoles.HiddenBadge);
            var isAllowedToSee = !hasHiddenBadge || (player.serverRoles.GlobalHidden && seeGlobal) ||
                                 (!player.serverRoles.GlobalHidden && seeHidden);

            if (isAllowedToSee)
            {
                if (hasHiddenBadge)
                {
                    message += "\n<color=#DC143C>Hidden role: </color>" + player.serverRoles.HiddenBadge;
                    message += "\n<color=#DC143C>Hidden role type: </color>" +
                               (player.serverRoles.GlobalHidden ? "GLOBAL" : "LOCAL");
                }

                if (player.authManager.RemoteAdminGlobalAccess)
                {
                    message +=
                        "\nStudio Status: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>";
                }
                else if (player.authManager.BypassBansFlagSet)
                {
                    message += "\nStudio Status: <color=#94B9CF>Studio Staff</color>";
                }
            }

            var flags = (int)VoiceChatMutes.GetFlags(players[0]);
            if (flags != 0)
            {
                message += "\nMUTE STATUS:";

                foreach (var mute in Enum.GetValues(typeof(VcMuteFlags)))
                {
                    var muteValue = (int)mute;
                    if (muteValue != 0 & (flags & muteValue) == muteValue)
                    {
                        message += " <color=#F70D1A>";
                        message += (VcMuteFlags)muteValue;
                        message += "</color>";
                    }
                }
            }

            message += "\nActive flag(s):";

            if (player.characterClassManager.GodMode)
            {
                message += " <color=#659EC7>[GOD MODE]</color>";
            }

            if (player.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip))
            {
                message += " <color=#DC143C>[NOCLIP ENABLED]</color>";
            }
            else if (FpcNoclip.IsPermitted(player))
            {
                message += " <color=#E52B50>[NOCLIP UNLOCKED]</color>";
            }

            if (player.authManager.DoNotTrack)
            {
                message += " <color=#BFFF00>[DO NOT TRACK]</color>";
            }

            if (player.serverRoles.BypassMode)
            {
                message += " <color=#BFFF00>[BYPASS MODE]</color>";
            }

            if (isAllowedToSee && player.serverRoles.RemoteAdmin)
            {
                message += " <color=#43C6DB>[RA AUTHENTICATED]</color>";
            }

            if (player.serverRoles.IsInOverwatch)
            {
                message += " <color=#008080>[OVERWATCH MODE]</color>";
            }
            else if (seeGamePlayData)
            {
                message += "\nClass: ";
                switch (player.GetRoleId())
                {
                    case RoleTypeId.None:
                        message += "None";
                        break;

                    case RoleTypeId.Filmmaker:
                        message += "Filmmaker";
                        break;

                    case RoleTypeId.Spectator:
                        message += "Spectator";
                        break;

                    default:
                        message += player.roleManager.CurrentRole.RoleName;
                        message += " <color=#fcff99>[HP: " + CommandProcessor.GetRoundedStat<HealthStat>(player) +
                                   "]</color>";
                        message += " <color=green>[AHP: " + CommandProcessor.GetRoundedStat<AhpStat>(player) +
                                   "]</color>";
                        message += " <color=#977dff>[HS: " + CommandProcessor.GetRoundedStat<HumeShieldStat>(player) +
                                   "]</color>";
                        foreach (var additionalStats in player.playerStats.StatModules)
                        {
                            if (additionalStats is not IRaDisplayedStat displayedStat) continue;
                            message += $" <color={displayedStat.Color}>[{displayedStat.Prefix}: {GetRoundedStat(additionalStats).ToString()}]</color>";
                        }

                        message += "\nTeam: " + player.GetTeam();
                        message += "\nPosition: " + player.transform.position;
                        
                        if (player.GetPosition().TryGetRoom(out var room))
                            message += "\nRoom: " + room.Name;
                        else
                            message += "\nRoom: " + "OUT OF BOUND";
                        break;
                }
            }
            else
            {
                message += "\n<color=#D4AF37>Some fields were hidden. GameplayData permission required.</color>";
            }

            message += "</color>";
            sender.RaReply("$1 " + message, true, true, string.Empty);
            RaPlayerQR.Send(sender, false,
                string.IsNullOrWhiteSpace(player.authManager.UserId)
                    ? "(no User ID)"
                    : player.authManager.UserId);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error("BetterRA: RemoteAdmin Receive List failed\n" + ex);
        }

        return false;
    }

    private static float GetRoundedStat(StatBase stat)
    {
        return Mathf.Round(stat.CurValue * 100f) / 100f;
    }

    // TODO display category

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExternalLookupCommand), nameof(ExternalLookupCommand.Execute))]
    public static bool Execute(ExternalLookupCommand __instance, ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.BanningUpToDay | PlayerPermissions.LongTermBanning | PlayerPermissions.SetGroup | PlayerPermissions.PlayersManagement | PlayerPermissions.PermissionsManagement | PlayerPermissions.ViewHiddenBadges | PlayerPermissions.PlayerSensitiveDataAccess | PlayerPermissions.ViewHiddenGlobalBadges, out response))
        {
            return false;
        }

        if (sender is not PlayerCommandSender playerCommandSender)
        {
            response = "This command can only be executed by players!";
            return false;
        }

        string text = string.Empty;
        if (arguments.Count >= 1)
        {
            var id = arguments.At(0);
            foreach (var category in RaCategoryService._remoteAdminCategories)
            {
                if (id != category.Attribute.Id.ToString()) continue;
                playerCommandSender.RaReply("% none % " + category.ExternalURL, true, false, "");
                response = "Lookup success!";
                return true;
            }

            if (!int.TryParse(id, out var result))
            {
                response = "Invalid player id!";
                return false;
            }

            if (!ReferenceHub.TryGetHub(result, out var hub))
            {
                response = "Invalid player id!";
                return false;
            }

            text = hub.authManager.UserId;
        }

        string remoteAdminExternalPlayerLookupMode = ServerConfigSynchronizer.Singleton.RemoteAdminExternalPlayerLookupMode;
        if (!(remoteAdminExternalPlayerLookupMode == "fullauth"))
        {
            if (remoteAdminExternalPlayerLookupMode == "urlonly")
            {
                playerCommandSender.RaReply("%" + text + "%" + ServerConfigSynchronizer.Singleton.RemoteAdminExternalPlayerLookupURL, success: true, logToConsole: false, "");
                response = "Lookup success!";
                return true;
            }

            response = "Invalid mode or command disabled via config.";
            return false;
        }

        Timing.RunCoroutine(__instance.AuthenticateWithExternalServer(playerCommandSender, text));
        response = "Initiated communication with external server.";
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RAUtils), nameof(RAUtils.ProcessPlayerIdOrNamesList))]
    public static bool OnGettingPlayers(ArraySegment<string> args, int startindex, out string[]? newargs,
        bool keepEmptyEntries, out List<ReferenceHub>? __result)
    {
        try
        {
            newargs = null;
            __result = new List<ReferenceHub>();
            try
            {
                newargs = args.Count > 1
                    ? RAUtils.FormatArguments(args, startindex + 1).Split(new[] { ' ' },
                        keepEmptyEntries ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries)
                    : null;

                if (args.Count <= startindex) return false;

                var info = args.At(startindex);

                if (info.Length == 0) return false;

                if (PlayerService.TryGetPlayers(info, out var players))
                {
                    __result = players.ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Better RA: RemoteAdmin GetPlayers failed\n" + ex);
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.Error("Better RA: Select Player Patch failed\n" + ex);
            newargs = null;
            __result = null;
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RaPlayerList), nameof(RaPlayerList.ReceiveData), typeof(CommandSender), typeof(string))]
    public static bool OnReceiveData(RaPlayerList __instance, CommandSender sender, string data)
    {
        try
        {
            var args = data.Split(' ');
            if (args.Length != 3) return false;
            if (!int.TryParse(args[0], out var number) || !int.TryParse(args[1], out var sortingNumber)) return false;
            if (!Enum.IsDefined(typeof(RaPlayerList.PlayerSorting), sortingNumber)) return false;

            var logRequest = number != 1;
            var sortingType = (RaPlayerList.PlayerSorting)sortingNumber;
            var sortListDescending = args[2] == "1";

            var viewHiddenBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
            var viewGlobalBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);
            var playerSender = sender as PlayerCommandSender;

            if (playerSender != null && playerSender.ReferenceHub.authManager.NorthwoodStaff)
            {
                viewHiddenBadges = true;
                viewGlobalBadges = true;
            }

            var players = new List<RemoteAdminPlayer>();
            var dummies = new List<RemoteAdminPlayer>();

            foreach (var hub in sortListDescending
                         ? __instance.SortPlayersDescending(sortingType)
                         : __instance.SortPlayers(sortingType))
            {
                if (hub.Mode 
                    is not ClientInstanceMode.ReadyClient 
                    and not ClientInstanceMode.Host
                    and not ClientInstanceMode.Dummy) continue;

                var element = new RemoteAdminPlayer
                {
                    Player = hub
                };

                if (hub.IsDummy)
                    dummies.Add(element);
                else
                    players.Add(element);

                var badgeText = RaPlayerList.GetPrefix(hub, viewHiddenBadges, viewGlobalBadges);
                var overWatchText = hub.serverRoles.IsInOverwatch ? RaPlayerList.OverwatchBadge : string.Empty;

                element.Text = badgeText + overWatchText + "<color={RA_ClassColor}>(" +
                               hub.PlayerId + ") " +
                               hub.nicknameSync.CombinedName.Replace("\n", "").Replace("RA_", string.Empty) +
                               "</color>";

                // if (!string.IsNullOrWhiteSpace(hub.CustomRemoteAdminBadge))
                //     element.Text = hub.CustomRemoteAdminBadge + " " + element.Text;
            }

            sender.RaReply("$0 " + GenerateList(players, dummies, sender), true, logRequest, string.Empty);
        }
        catch (Exception ex)
        {
            Logger.Error("BetterRA: RemoteAdmin Receive List failed\n" + ex);
        }

        return false;
    }

    private static string GenerateList(List<RemoteAdminPlayer> players, List<RemoteAdminPlayer> dummies, CommandSender sender)
    {
        var remoteAdminGroups = ServerStatic.PermissionsHandler.Groups.Select(x => new RemoteAdminGroup
        {
            Name = x.Key,
            GroupId = x.Key,
            Color = string.IsNullOrWhiteSpace(x.Value.BadgeColor) ||
                    string.Equals(x.Value.BadgeColor, "none", StringComparison.OrdinalIgnoreCase)
                ? "white"
                : x.Value.BadgeColor
        }).ToList();

        var text = "\n";
        var categories = RaCategoryService.RemoteAdminCategories;
        var playerSender = sender as PlayerCommandSender;

        var groupPlayers = players.ToList();

        foreach (var entry in players)
        {
            var group = remoteAdminGroups.FirstOrDefault(x => x.GroupId == entry.Player?.serverRoles.Group.Name);
            if (group == null) continue;

            group.Members.Add(entry);
            groupPlayers.Remove(entry);
        }

        if (playerSender != null)
        {
            foreach (var category in categories)
            {
                if (!category.DisplayOnTop || !category.CanSeeCategory(playerSender.ReferenceHub)) continue;

                text += CategoryText(players, category);
            }
        }

        foreach (var group in remoteAdminGroups)
        {
            if (group.Members.Count == 0) continue;

            var color = group.Color;
            // if (string.Equals(color, "rainbow", StringComparison.OrdinalIgnoreCase))
            // {
            //     var colors = ServerService.Colors;
            //     color = colors.ElementAt(Random.Range(0, colors.Count)).Value;
            // }
            // else
            {
                color = GetColorHexCode(color);
            }

            text += "<align=center><size=0>(-" + group.GroupId + ")</size> <size=20><color=" + color + ">[" +
                    group.Name +
                    "]</color></size>\n</align>";

            foreach (var player in group.Members)
            {
                text += player.Text + "\n";
            }
        }

        if (groupPlayers.Any())
            text += "<align=center><size=0>(default)</size> <size=20>[Default Player]</size></align>\n";

        foreach (var player in groupPlayers)
        {
            text += player.Text + "\n";
        }

        if (dummies.Any())
            text += "<align=center><size=0>(dummy)</size> <size=20>[Dummy]</size></align>\n";

        foreach (var dummy in dummies)
        {
            text += dummy.Text + "\n";
        }

        if (playerSender != null)
        {
            foreach (var category in categories)
            {
                if (category.DisplayOnTop || !category.CanSeeCategory(playerSender.ReferenceHub)) continue;

                text += CategoryText(players, category);
            }
        }

        return text;
    }

    private static string CategoryText(List<RemoteAdminPlayer> players, RaCategory category)
    {
        var color = category.Attribute.Color;

        // if (string.Equals(color, "rainbow", StringComparison.OrdinalIgnoreCase))
        // {
        //     var colors = ServerService.Colors;
        //     color = colors.ElementAt(Random.Range(0, colors.Count)).Value;
        // }

        var text = $"<align=center><size=0>(-{category.Attribute.Id})</size> " +
            $"<size={category.Attribute.Size}></color>" +
            $"<color={color}>{category.Attribute.Name}</color></size>\n</align>";

        bool displayPlayerMultipleTimes = true;
        if (displayPlayerMultipleTimes)
        {
            var categoryPlayers = category.GetPlayers();
            if (categoryPlayers != null)
            {
                foreach (var player in categoryPlayers)
                {
                    var raPlayer = players.FirstOrDefault(x => x.Player == player);
                    if (raPlayer != null)
                        text += raPlayer.Text + "\n";
                }
            }
        }

        return text;
    }

    private class RemoteAdminPlayer
    {
        public ReferenceHub? Player { get; set; }

        public string? Text { get; set; }
    }

    private class RemoteAdminGroup
    {
        public string? Name { get; set; }

        public List<RemoteAdminPlayer> Members { get; } = new();

        public string? GroupId { get; set; }

        public string? Color { get; set; }
    }

    // help

    public static string GetColorHexCode(string color) => !Enum.TryParse(color, true, out Misc.PlayerInfoColorTypes colorEnum)
         ? Misc.AllowedColors[Misc.PlayerInfoColorTypes.White]
         : Misc.AllowedColors[colorEnum];
}

