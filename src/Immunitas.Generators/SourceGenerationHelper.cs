using Microsoft.CodeAnalysis;

namespace Immunitas.Generators
{
    internal static class SourceGenerationHelper
    {
        internal static INamedTypeSymbol? GetSharedContractAttributeSymbol(this Compilation compilation) =>
            compilation.GetTypeByMetadataName(
                "Immunitas.Generators.Attributes.SharedContractAttribute");

        public static bool HasAttribute(this ISymbol type, INamedTypeSymbol attributeSymbol) =>
            type.GetAttributes().Any(attr =>
                SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol));

        public static AttributeData? FindAttribute(this ISymbol type, INamedTypeSymbol attributeSymbol) =>
            type.GetAttributes().FirstOrDefault(attr =>
                SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol));

        public static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
        {
            foreach (var type in ns.GetTypeMembers())
                yield return type;

            foreach (var nested in ns.GetNamespaceMembers())
                foreach (var t in GetAllTypes(nested))
                    yield return t;
        }
    }
}
