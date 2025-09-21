using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Footprinting;

namespace Magic.Spells;

public abstract class Spell
{
    public static Dictionary<ReferenceHub, Spell> InCasting { get; } = new Dictionary<ReferenceHub, Spell>();

    public Footprint Caster;

    /// <summary>
    /// When the player start the invokation of the spell.
    /// </summary>
    public abstract void StartCast();

    /// <summary>
    /// When the invokation is stoped other than the player stoped it.
    /// Ex: The player is dead.
    /// </summary>
    public abstract void StopCast();

    /// <summary>
    /// When the player stop the invokation.
    /// </summary>
    public abstract void PlayerStopCast();

    /// <summary>
    /// The player ended the invokation the spell is casted.
    /// </summary>
    public abstract void Cast();

}
