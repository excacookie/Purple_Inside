global using SpellId = uint;
using Footprinting;
using PlayerRoles;
using UnityEngine;

namespace Magic.Spells;

public class CastSystem
{
    #region Properties & Variables
    internal static Dictionary<ReferenceHub, CastSystem> playerSystem = new ();
    private static Dictionary<SpellId, SpellInfo> spells = new ();

    private Dictionary<SpellId, CoolDown> _coolDown = new ();
    
    public Spell? Casting { get; set; }
    public readonly List<Spell> Casted = new List<Spell>();
    public readonly ReferenceHub Hub;
    #endregion

    #region Constructor & Destructor
    static CastSystem()
    {
        PlayerRoleManager.OnRoleChanged += OnClassChanged;
        // Register<>();
        Register<FireBallSpell>(1, 10, 2);
    }

    public CastSystem(ReferenceHub hub)
    {
        Hub = hub;
    }
    #endregion

    #region Methods
    public bool InCooldown(SpellId id)
    {
        if (!_coolDown.TryGetValue(id, out var cooldown))
            return false;

        return cooldown.IsActive;
    }

    public void ResetAllCouldown()
    {
        _coolDown.Clear();
    }


    public bool TryCast(SpellId id)
    {
        if (Casting != null)
            return false;

        if (InCooldown(id))
            return false;

        if (!spells.TryGetValue(id, out var info))
#if !DEBUG
            return false;
#else
            throw new Exception($"{id} is not regired.");
#endif
        if (Hub.playerStats.TryGetModule<ManaStat>(out var mana))
#if !DEBUG
            return false;
#else
            throw new Exception($"new mana state module.");
#endif

        if (mana.CurValue < info.cost)
            return false;

        mana.CurValue -= info.cost;
        SetCooldown(id, new CoolDown(info.cooldownSecond));
        Casting = info.ctor();
        Casting.Caster = new Footprinting.Footprint(Hub);
        return true;
    }

    public void SetCooldown(SpellId id, CoolDown cooldown)
    {
        _coolDown[id] = cooldown;
    }

    public static Spell? GetSpell(SpellId id)
    {
        if (!spells.TryGetValue(id, out var info))
            return null;
        return info.ctor();
    }

    public static void Register<TSpell>(SpellId id, float cost, float cooldownSecond)
        where TSpell : Spell, new()
    {
        if (!spells.ContainsKey(id))
        {
            spells.Add(id, new SpellInfo()
            {
                ctor = () => new TSpell(),
                cooldownSecond = cooldownSecond,
                cost = cost
            });
        }
    }

    private static void OnClassChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {
        if (!playerSystem.TryGetValue(userHub, out var system))
            return;

        system.ResetAllCouldown();
    }
#endregion

    

    public struct SpellInfo
    {
        public float cost;
        public float cooldownSecond;
        public Func<Spell> ctor;
    }
}
