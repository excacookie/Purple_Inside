using System.Collections.Generic;
using BetterRA.Category;
using LabApi.Features.Wrappers;
using Mirror;

namespace BetterRA;

public static class PlayerService
{
    /// <summary>
    /// Returns multiple Player that are parsed from a string.
    /// Use . between each player
    /// </summary>
    /// <param name="arg">The Argument that should be parsed to a player list</param>
    /// <param name="players">The Player List that will be returned</param>
    /// <param name="me">The Player which should be returned for Me and Self</param>
    /// <param name="playerTypes">The Player Types which can be returned</param>
    public static bool TryGetPlayers(string arg, out HashSet<ReferenceHub> players, ReferenceHub me = null)
    {
        players = new HashSet<ReferenceHub>();
        var all = ReferenceHub.AllHubs;
        var args = arg.Split('.');

        foreach (var parameter in args)
        {
            if (string.IsNullOrWhiteSpace(parameter)) continue;
            switch (parameter.ToUpper())
            {
                case "SELF":
                case "ME":
                    if (me == null) continue;
                    players.Add(me);
                    continue;

                case "RA":
                case "REMOTEADMIN":
                case "ADMIN":
                case "STAFF":
                    foreach (var player in all)
                    {
                        if (player.serverRoles.RemoteAdmin)
                            players.Add(player);
                    }
                    continue;

                case "NW":
                case "NORTHWOOD":
                case "GLOBALSTAFF":
                    foreach (var player in all)
                    {
                        if (player.serverRoles.BypassMode)
                            players.Add(player);
                    }
                    break;

                case "NPC":
                case "DUMMY":
                    foreach (var player in all)
                    {
                        if (player.IsDummy/* is DummyPlayer { RaVisible: true }*/)
                            players.Add(player);
                    }
                    break;

                case "DEFAULT":
                    foreach (var player in all)
                    {
                        if (!player.serverRoles.RemoteAdmin)
                            players.Add(player);
                    }
                    break;

                case "*":
                case "ALL":
                case "EVERYONE":
                    foreach (var player in all)
                    {
                        if (!player.IsHost)
                            players.Add(player);
                    }
                    continue;

                default:
                    var player3 = GetPlayer(parameter);

                    if (player3 != null)
                    {
                        players.Add(player3);
                        continue;
                    }

                    if (parameter.Length > 1 && parameter[0] == '-')
                    {
                        var sub_parameter = parameter.Substring(1);

                        players.UnionWith(GetPlayersByServerRole(sub_parameter));

                        if (int.TryParse(sub_parameter, out var id))
                        {
                            //Check for RemoteAdmin Category
                            var category = RaCategoryService.GetCategory(id);
                            var categoryPlayers = category?.GetPlayers();
                            if (categoryPlayers != null)
                                players.UnionWith(categoryPlayers);
                        }
                    }
                    continue;
            }
        }

        return players.Count > 0;
    }

    public static HashSet<ReferenceHub> GetPlayersByServerRole(string serverRole)
    {
        HashSet<ReferenceHub> players = new HashSet<ReferenceHub>();
        if (!ServerStatic.PermissionsHandler.Groups.TryGetValue(serverRole, out var group))
            return players;
        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (hub.serverRoles.Group?.Name == serverRole)
                players.Add(hub);
        }
        return players;
    }

    public static ReferenceHub GetPlayer(string argument)
    {
        if (argument.Contains("@"))
        {
            var player = GetPlayerByUserId(argument);
            if (player != null)
                return player;
        }

        if (int.TryParse(argument, out var playerId))
        {
            var player = GetPlayer(playerId);
            if (player != null)
                return player;
        }

        if (uint.TryParse(argument, out var netId))
        {
            var player = GetPlayer(netId);
            if (player != null)
                return player;
        }

        return GetPlayerByName(argument);
    }

    /// <summary>
    /// Returns the player with that playerID
    /// </summary>
    public static ReferenceHub GetPlayer(int playerId)
        => GetPlayer(x => x.PlayerId == playerId);

    /// <summary>
    /// Returns the player with that NetworkID
    /// </summary>
    public static ReferenceHub GetPlayer(uint netId)
        => GetPlayer(x => x.networkIdentity.netId == netId);

    /// <summary>
    /// Returns the player with that UserID
    /// </summary>
    public static ReferenceHub GetPlayerByUserId(string userid)
        => GetPlayer(x => x.authManager.UserId == userid);

    public static ReferenceHub GetPlayer(Func<ReferenceHub, bool> func)
        => ReferenceHub.AllHubs.FirstOrDefault(func);

    /// <summary>
    /// Returns the player with that Name
    /// </summary>
    public static ReferenceHub GetPlayerByName(string name)
        => GetPlayer(x =>
            string.Equals(x.nicknameSync.DisplayName, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.nicknameSync.MyNick, name, StringComparison.OrdinalIgnoreCase));
}
