using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterRA.Category;

public abstract class RaCategory
{
    public RaCategoryAttribute Attribute { get; set; }

    public abstract string GetInfo(CommandSender sender, bool secondPage);

    public abstract List<ReferenceHub> GetPlayers();

    public abstract bool DisplayOnTop { get; }

    public abstract bool CanSeeCategory(ReferenceHub player);

    public virtual string ExternalURL => "";

    public virtual void Load() { }
}