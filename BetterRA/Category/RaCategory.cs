using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterRA.Category;

public abstract class RaCategory
{
#pragma warning disable CS8618 // Un champ non-nullable doit contenir une valeur autre que Null lors de la fermeture du constructeur. Envisagez d’ajouter le modificateur « required » ou de déclarer le champ comme pouvant accepter la valeur Null.
    public RaCategoryAttribute Attribute { get; set; }
#pragma warning restore CS8618 // Un champ non-nullable doit contenir une valeur autre que Null lors de la fermeture du constructeur. Envisagez d’ajouter le modificateur « required » ou de déclarer le champ comme pouvant accepter la valeur Null.

    public abstract string GetInfo(CommandSender sender, bool secondPage);

    public abstract List<ReferenceHub> GetPlayers();

    public abstract bool DisplayOnTop { get; }

    public abstract bool CanSeeCategory(ReferenceHub player);

    public virtual string ExternalURL => "";

    public virtual void Load() { }
}