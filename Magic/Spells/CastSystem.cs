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
    
    public Spell? Canalise{ get; set; }
    public readonly List<Spell> Casted = new List<Spell>();
    public readonly ReferenceHub Hub;
    #endregion

    #region Constructor & Destructor
    static CastSystem()
    {
        PlayerRoleManager.OnRoleChanged += OnClassChanged;
        // Register<>();
        Register<FireBallSpell>(1, 3, 0.2f);
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


    public bool TryCast(SpellId id,out Spell? spell)
    {
        spell = null;

        if (Canalise != null)
        {
            Logger.Info("in canalise");
            return false;
        }
            

        if (InCooldown(id))
        {
            Logger.Info("in Cooldown");
            return false;
        }         

        if (!spells.TryGetValue(id, out var info))
#if !DEBUG
            return false;
#else
            throw new Exception($"{id} is not regired.");
#endif
        if (!Hub.playerStats.TryGetModule<ManaStat>(out var mana))
#if !DEBUG
            return false;
#else
            throw new Exception($"No mana state module.");
#endif

        if (mana.CurValue < info.cost)
        {
            Logger.Info($"pas de mana {mana.CurValue}");
            return false;
        }
        

        mana.CurValue -= info.cost;
        var cooldown = new CoolDown(info.cooldownSecond);
        Logger.Info($"{cooldown.Duration}");
        cooldown.Start();
        Logger.Info($"active: {cooldown.IsActive}");
        SetCooldown(id, cooldown);
        Logger.Info($"Cooldown: {InCooldown(id)}");
        Canalise = spell = info.ctor();
        spell.Casted += OnCasted;
        spell.Destroy += OnDestroy;
        Canalise.StartCast(new Footprint(Hub));
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

    private void OnDestroy(Spell spell)
    {
        if (Canalise == spell)
        {
            Canalise = null;
        }
        Casted.Remove(spell);
        spell.Destroy -= OnDestroy;
        spell.Casted -= OnCasted;
    }

    private void OnCasted(Spell spell)
    {
        if (Canalise == spell)
        {
            Canalise = null;
        }
        Casted.Add(spell);
        spell.Casted -= OnCasted;
    }

#endregion

    

    public struct SpellInfo
    {
        public float cost;
        public float cooldownSecond;
        public Func<Spell> ctor;
    }
}
