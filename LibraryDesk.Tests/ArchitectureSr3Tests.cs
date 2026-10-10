using System.Reflection;
using LibraryDesk.Core.Domain;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class ArchitectureSr3Tests
{
    private const string InfrastructureNamespace = "LibraryDesk.Core.Storage";

    [Fact]
    public void CoreAssembly_DoesNotReferenceConsoleApplication()
    {
        Assembly core = typeof(Loan).Assembly;

        IEnumerable<string> references = core.GetReferencedAssemblies().Select(reference => reference.Name!);

        Assert.DoesNotContain("LibraryDesk.App", references);
    }

    [Fact]
    public void Domain_PublicApi_DoesNotExposeInfrastructureTypes()
    {
        Assembly core = typeof(Loan).Assembly;
        IEnumerable<Type> domainTypes = core.GetExportedTypes()
            .Where(type => type.Namespace?.StartsWith("LibraryDesk.Core.Domain", StringComparison.Ordinal) == true);

        IEnumerable<Type> exposedTypes = domainTypes.SelectMany(PublicSignatureTypes);

        Assert.DoesNotContain(exposedTypes, IsInfrastructureType);
    }

    private static IEnumerable<Type> PublicSignatureTypes(Type type)
    {
        const BindingFlags PublicMembers = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        IEnumerable<Type> properties = type.GetProperties(PublicMembers).Select(property => property.PropertyType);
        IEnumerable<Type> methods = type.GetMethods(PublicMembers)
            .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType));
        IEnumerable<Type> constructors = type.GetConstructors(PublicMembers)
            .SelectMany(constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType));
        return properties.Concat(methods).Concat(constructors);
    }

    private static bool IsInfrastructureType(Type type)
    {
        Type inspected = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        return inspected.Namespace?.StartsWith(InfrastructureNamespace, StringComparison.Ordinal) == true;
    }
}
