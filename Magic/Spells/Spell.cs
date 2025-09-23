using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Footprinting;

namespace Magic.Spells;

public abstract class Spell
{
    public Footprint Caster { get; private set; }

    /// <summary>
    /// When the player start the invokation of the spell.
    /// </summary>
    public virtual void StartCast(Footprint caster)
    {
        Caster = caster;
        Cast();
    }

    /// <summary>
    /// When the invokation is stoped other than the player stoped it.
    /// Ex: The player is dead.
    /// </summary>
    public virtual void StopCast()
    {
        InvokeDestroy();
    }

    /// <summary>
    /// When the player stop the invokation.
    /// </summary>
    public virtual void PlayerStopCast()
    {
        InvokeDestroy();
    }

    /// <summary>
    /// The player ended the invokation the spell is casted.
    /// </summary>
    public virtual void Cast()
    {
        Casted?.Invoke(this);
    }

    protected void InvokeDestroy() => Destroy?.Invoke(this);

    public event Action<Spell>? Casted;
    public event Action<Spell>? Destroy;
}
