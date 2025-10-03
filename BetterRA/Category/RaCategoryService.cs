// source: https://github.com/SynapseSL/Synapse/blob/synapse3/Synapse3.SynapseModule/Permissions/RemoteAdmin/RemoteAdminCategoryService.cs
using System.Collections.ObjectModel;
using System.Reflection;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Permissions;

namespace BetterRA.Category;

public static class RaCategoryService
{
    private static bool registered = false;

    internal static void Register()
    {
        if (registered) return;
        registered = true;
        RegisterCategory<OverWatchCategory>();
        RegisterCategory<GodModeCategory>();
        RegisterCategory<NoClipCategory>();
    }

    internal static readonly List<RaCategory> _remoteAdminCategories = new();
    public static ReadOnlyCollection<RaCategory> RemoteAdminCategories => _remoteAdminCategories.AsReadOnly();

    public static RaCategory GetCategory(int id) => _remoteAdminCategories.FirstOrDefault(x => x.Attribute.Id == id);

    public static void AddCategory<TCategory>(TCategory category) where TCategory : RaCategory
    {
        if (!_remoteAdminCategories.Contains(category))
            _remoteAdminCategories.Add(category);
    }

    public static void RegisterCategory<TCategory>() where TCategory : RaCategory
    {
        var info = typeof(TCategory).GetCustomAttribute<RaCategoryAttribute>();
        if (info == null) return;
        info.CategoryType = typeof(TCategory);

        RegisterCategory(info);
    }

    public static void RegisterCategory(RaCategoryAttribute info)
    {
        if (info.CategoryType == null) return;
        if (IsIdRegistered(info.Id)) return;

        var category = (RaCategory)Activator.CreateInstance(info.CategoryType);
        category.Attribute = info;
        category.Load();

        _remoteAdminCategories.Add(category);
    }

    public static bool IsIdRegistered(uint id) => _remoteAdminCategories.Any(x => x.Attribute.Id == id);
}