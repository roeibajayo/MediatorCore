using System.Reflection;

namespace MediatorCore.Infrastructure;

internal static class AssemblyExtentions
{
    internal static Assembly[] GetAllReferencedAssemblies(this Assembly assembly)
    {
        var firstReferences = assembly.GetReferencedAssemblies()
                .Select(Assembly.Load);

        var result = new List<Assembly>(firstReferences);

        foreach (var reference in firstReferences)
            result.AddRange(reference.GetAllReferencedAssemblies());

        return result.Distinct().ToArray();
    }
    internal static IEnumerable<Type> GetAllInheritsFromMarker(Type type, Type marker,
            bool ignoreAbstract = true,
            bool directBaseTypeOnly = false)
    {
        return GetAllInherits(type, ignoreAbstract, directBaseTypeOnly, assemblies: marker.Assembly);
    }

    internal static IEnumerable<Type> GetAllInherits(Type type, bool ignoreAbstract = true,
            bool directBaseTypeOnly = false,
            params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = Assembly
                .GetExecutingAssembly()
                .GetAllReferencedAssemblies()
                .Concat(Assembly
                .GetEntryAssembly()
                .GetAllReferencedAssemblies())
                .Concat(new[] { Assembly.GetEntryAssembly() })
                .ToArray();
        }

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
