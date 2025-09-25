using BetterRA;
using GameCore;
using LabApi.Features.Wrappers;
using Magic.Spells;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using System.Collections.Generic;

namespace Magic;

internal class ManaShow
{
    public static void Register()
    {
        //Logger.Info("Enregistrement de ManaShow...");
        Timing.RunCoroutine(CheckManaCoroutine());
    }

    private static IEnumerator<float> CheckManaCoroutine()
    {
        //Logger.Info("Démarrage de CheckManaCoroutine...");
        while (true)
        {
            try
            {
                if (!Player.List.Any())
                {
                    //Logger.Info("Aucun joueur connecté.");
                }

                foreach (var player in Player.List)
                {
                    // Vérifier si le joueur est valide et non local (headless server)
                    if (player == null || player.ReferenceHub == null || player.IsHost)
                    {
                        //Logger.Info($"Joueur ignoré : {player?.Nickname ?? "null"} (invalide ou serveur local).");
                        continue;
                    }

                    if (player.ReferenceHub.playerStats.TryGetModule<ManaStat>(out var mana))
                    {
                        string message = $"Mana restant : {mana.CurValue}/{mana.MaxValue}";
                        player.SendHint(message, 3f);
                        //Logger.Info($"Hint envoyé à {player.Nickname}: {message}");
                    }
                    else
                    {
                        player.SendHint("Aucun mana disponible.", 3f);
                        Logger.Info($"ManaStat non trouvé pour {player.Nickname}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Erreur dans CheckManaCoroutine : {ex}");
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }
}