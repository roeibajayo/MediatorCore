using System.Reflection;

namespace MediatorCore.Infrastructure;

internal static class AssemblyExtentions
{
    internal static IEnumerable<Type> GetAllInheritsFromMarker(Type type, Type marker,
            bool ignoreAbstract = true,
            bool directBaseTypeOnly = false)
    {
        return GetAllInherits([marker.Assembly], type, ignoreAbstract, directBaseTypeOnly);
    }

    internal static IEnumerable<Type> GetAllInherits(IEnumerable<Assembly> assemblies, Type type, 
        bool ignoreAbstract = true, 
        bool directBaseTypeOnly = false)
    {
        return assemblies
            .SelectMany(x => x.DefinedTypes)
            .Where(t => (!ignoreAbstract || !t.IsAbstract) &&
                !t.IsInterface &&
                type.IsInterface ?
                    t.GetInterfaces().Select(x => type.IsGenericType && x.IsGenericType ? x.GetGenericTypeDefinition() : x).Contains(type) :
                    directBaseTypeOnly ? t.BaseType == type : t.GetBaseTypes().Contains(type));
    }

    internal static IEnumerable<Type> GetBaseTypes(this Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        while (type.BaseType != null)
        {
            yield return type.BaseType;
            type = type.BaseType;
        }
    }
}
