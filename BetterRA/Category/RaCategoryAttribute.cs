namespace BetterRA.Category;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RaCategoryAttribute : Attribute
{
#pragma warning disable CS8618 // Un champ non-nullable doit contenir une valeur autre que Null lors de la fermeture du constructeur. Envisagez d’ajouter le modificateur « required » ou de déclarer le champ comme pouvant accepter la valeur Null.
    public RaCategoryAttribute() { }
#pragma warning restore CS8618 // Un champ non-nullable doit contenir une valeur autre que Null lors de la fermeture du constructeur. Envisagez d’ajouter le modificateur « required » ou de déclarer le champ comme pouvant accepter la valeur Null.

    public RaCategoryAttribute(string name, uint id, Type categoryType)
    {
        Name = name;
        Id = id;
        CategoryType = categoryType;
    }

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = "white";

    public int Size { get; set; } = 20;

    public string? RemoteAdminIdentifier { get; set; }

    public uint Id { get; set; }

    public Type CategoryType { get; internal set; }
}