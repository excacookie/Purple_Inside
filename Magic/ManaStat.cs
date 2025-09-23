using PlayerRoles;
using PlayerStatsSystem;

namespace Magic;

public class ManaStat : StatBase
{
    #region Properties & Variables
    // TODO: display

    // Memento FpcStateProcessor ce charge de la régène via la stopwatch

    // Per s 
    const float ManaRegen = 2f;

    public static Dictionary<RoleTypeId, float> DefaultMaxMana = new Dictionary<RoleTypeId, float>()
    {
        { RoleTypeId.NtfSpecialist, 100 },
        { RoleTypeId.ChaosConscript, 100 }
    };

    public override float CurValue { get; set; }

    public override float MaxValue { get; set; }

    public override float MinValue => 0;
    #endregion

    #region Methods
    public override void ClassChanged()
    {
        if (DefaultMaxMana.TryGetValue(Hub.GetRoleId(), out float max))
            MaxValue = max;
        else
            MaxValue = 0;

        CurValue = MaxValue;
    }

    public override void Update()
    {
        if (CurValue == MaxValue)
            return;

        CurValue = Mathf.Min(CurValue + ManaRegen * Time.deltaTime, MaxValue);
    }

    internal static void Register()
    {
        ref var definedModules = ref AccessTools.StaticFieldRefAccess<PlayerStats, Type[]>(nameof(PlayerStats.DefinedModules));
        definedModules = definedModules.AddItem(typeof(ManaStat)).ToArray();

        Logger.Info($"{definedModules.Length}");

        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (hub.playerStats.StatModules.All(p => p is not ManaStat))
                hub.playerStats._statModules = hub.playerStats.StatModules.AddToArray(new ManaStat());
        }
    }

    internal static void UnRegister()
    {
        ref var definedModules = ref AccessTools.StaticFieldRefAccess<PlayerStats, Type[]>(nameof(PlayerStats.DefinedModules));
        definedModules = definedModules.Where(p => p != typeof(ManaStat)).ToArray();

        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (hub.playerStats.StatModules.Any(p => p is ManaStat))
                hub.playerStats._statModules = hub.playerStats.StatModules.Where(p => p is not ManaStat).ToArray();
        }
    }
    #endregion

}

