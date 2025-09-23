using System.Configuration;
using CustomPlayerEffects;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using Respawning.Objectives;
using TMPro;
using UserSettings.ServerSpecific;

namespace BetterRA;

internal class BetterRAPlugin : Plugin
{
    #region Properties & Variables
    public override string Author => PluginInfo.PLUGIN_AUTHORS;
    public override string Description => PluginInfo.PLUGIN_DESCRIPTION;
    public override string Name => PluginInfo.PLUGIN_NAME;
    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
    public override Version Version { get; } = new Version(PluginInfo.PLUGIN_VERSION);
    #endregion

    #region Methods
    public override void Enable()
    {

    }

    public override void Disable()
    {

    }
    #endregion
}

/* [HarmonyPrefix]
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), typeof(CommandSender), typeof(string))]
    public static bool OnRequestPlayer(CommandSender sender, string data)
    {
        try
        {
            var args = data.Split(' ');
            if (args.Length != 2) return false;
            if (!int.TryParse(args[0], out var number)) return false;

            var arg = args[1].Split('.')[0];
            if (int.TryParse(arg, out var categoryId))
            {
                var category = CategoryService.GetCategory(categoryId);
                if (category != null && category.CanSeeCategory(sender.GetSynapsePlayer()))
                {
                    sender.RaReply("$1 " + category.GetInfo(sender, number == 0), true, true, string.Empty);
                    return false;
                }
            }

            var requestSensitiveData = number == 0;
            var playerSender = sender as PlayerCommandSender;

            if (requestSensitiveData && playerSender != null &&
                !playerSender.ServerRoles.Staff &&
                !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess)) return false;

            var players =
                RAUtils.ProcessPlayerIdOrNamesList(new ArraySegment<string>(args.Skip(1).ToArray()), 0, out _);
            if (players.Count == 0) return false;

            var allowedToSeeUserIds = PermissionsHandler.IsPermitted(sender.Permissions, 18007046UL) ||
                                      playerSender != null &&
                                      (playerSender.ServerRoles.Staff || playerSender.ServerRoles.RaEverywhere);

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
                        userIds += hub.characterClassManager.UserId + ".";
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

            var id = string.IsNullOrWhiteSpace(player.characterClassManager.UserId)
                ? "(none)"
                : player.characterClassManager.UserId + " <color=green><link=CP_USERID></link></color>";


            message += "\nUser ID: " + (allowedToSeeUserIds ? id : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>");

            if (allowedToSeeUserIds)
            {
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, player.characterClassManager.UserId ?? "");
                if (player.characterClassManager.SaltedUserId != null &&
                    player.characterClassManager.SaltedUserId.Contains("$"))
                {
                    message += "\nSalted User ID: " + player.characterClassManager.SaltedUserId;
                }

                if (!string.IsNullOrWhiteSpace(player.characterClassManager.UserId2))
                {
                    message += "\nUser ID 2: " + player.characterClassManager.UserId2;
                }
            }

            message += "\nServer role: " + player.serverRoles.GetColoredRoleString();
            var seeHidden = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
            var seeGlobal = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);

            if (playerSender != null && playerSender.ServerRoles.Staff)
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

                if (player.serverRoles.RaEverywhere)
                {
                    message +=
                        "\nStudio Status: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>";
                }
                else if (player.serverRoles.Staff)
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

            if (player.serverRoles.DoNotTrack)
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
                var sPlayer = player.GetSynapsePlayer();
                switch (sPlayer.RoleType)
                {
                    case RoleTypeId.None:
                        message += "None";
                        break;

                    case RoleTypeId.Spectator:
                        message += "Spectator";
                        break;

                    default:
                        message += sPlayer.RoleName;
                        message += " <color=#fcff99>[HP: " + CommandProcessor.GetRoundedStat<HealthStat>(player) +
                                   "]</color>";
                        message += " <color=green>[AHP: " + CommandProcessor.GetRoundedStat<AhpStat>(player) +
                                   "]</color>";
                        message += " <color=#977dff>[HS: " + CommandProcessor.GetRoundedStat<HumeShieldStat>(player) +
                                   "]</color>";
                        message += "\nTeam: " + TeamService.GetTeamName(sPlayer.TeamID);
                        message += "\nPosition: " + player.transform.position;
                        message += "\nRoom: " + sPlayer.Room.Name;
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
                string.IsNullOrWhiteSpace(player.characterClassManager.UserId)
                    ? "(no User ID)"
                    : player.characterClassManager.UserId);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: RemoteAdmin Receive List failed\n" + ex);
        }

        return false;
    }
}*/
