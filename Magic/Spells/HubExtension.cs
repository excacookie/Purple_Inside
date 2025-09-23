using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic.Spells;
public static class HubExtension
{
    public static CastSystem GetCastSystem(this ReferenceHub hub)
    {
        if (!CastSystem.playerSystem.TryGetValue(hub, out var castSystem))
            castSystem = CastSystem.playerSystem[hub] = new CastSystem(hub);
        return castSystem;

    }

}
