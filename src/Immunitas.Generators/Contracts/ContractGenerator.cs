using Microsoft.CodeAnalysis;

namespace Immunitas.Generators.Contracts;

[Generator]
public sealed class ContractGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
        {
            var sharedContractAttr = compilation.GetSharedContractAttributeSymbol();

            if (sharedContractAttr is null)
                return;

            var contractsToGenerate = new List<ClassRecord>();

            foreach (var reference in compilation.References)
            {
                var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                if (assemblySymbol is null) continue;

                foreach (var type in SourceGenerationHelper.GetAllTypes(assemblySymbol.GlobalNamespace).Where(t => t.TypeKind != TypeKind.Enum))
                {
                    if (type.HasAttribute(sharedContractAttr))
                    {
                        var contractName =
                              type.Name.EndsWith("Query") ? type.Name.Replace("Query", "Request")
                            : type.Name.EndsWith("Command") ? type.Name.Replace("Command", "Request")
                            : type.Name.EndsWith("QueryResult") ? type.Name.Replace("QueryResult", "Response")
                            : type.Name.EndsWith("CommandResult") ? type.Name.Replace("CommandResult", "Response")
                            : type.Name;

                        var contractNamespace = "Immunitas.Portal.UI.ApiProxies.Contracts";

                        var contract = new ClassRecord(
                            contractName,
                            contractNamespace,
                            type.GetMembers()
                                .OfType<IPropertySymbol>()
                                .Where(p => p.SetMethod is not null || p.GetMethod is not null)
                                .Select(p =>
                                {
                                    var propertyType = GetContractFriendlyTypeName(
                                        p.Type,
                                        sharedContractAttr,
                                        compilation);

                                    return new PropertyRecord(propertyType, p.Name);
                                })
                                .ToList()
                        );

                        contractsToGenerate.Add(contract);
                    }
                }
            }

            foreach (var contractToGenerate in contractsToGenerate)
            {
                var code = ContractsSourceGenerationHelper.GenerateContract(contractToGenerate);
                spc.AddSource($"{contractToGenerate.Name}.g.cs", code);
            }
        });
    }

    private static string GetContractFriendlyTypeName(
        ITypeSymbol type,
        INamedTypeSymbol sharedContractAttr,
        Compilation compilation)
    {
        // Встроенные типы (int, string и т.п.)
        if (type.SpecialType != SpecialType.None)
            return type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        // Nullable<T>
        if (type is INamedTypeSymbol { IsGenericType: true } named
            && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return GetContractFriendlyTypeName(named.TypeArguments[0], sharedContractAttr, compilation) + "?";
        }

        // Generic-коллекции
        if (type is INamedTypeSymbol { IsGenericType: true } genType)
        {
            var baseName = genType.Name; // например, IReadOnlyList
            var args = string.Join(", ", genType.TypeArguments.Select(a => GetContractFriendlyTypeName(a, sharedContractAttr, compilation)));

            // если интерфейс/тип находится в System.*, используем namespace
            var ns = genType.ContainingNamespace?.ToDisplayString();
            if (!string.IsNullOrEmpty(ns) && ns!.StartsWith("System"))
                return $"{ns}.{baseName}<{args}>";

            return $"{baseName}<{args}>";
        }

        // Тип в System.* (DateTime, Guid и т.д.)
        if (type.ContainingNamespace?.ToDisplayString().StartsWith("System") == true)
            return type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        // Контракт?
        if (type is INamedTypeSymbol nts
            && SourceGenerationHelper.HasAttribute(nts, sharedContractAttr))
        {
            // контрактные enum'ы — в специальном namespace
            if (nts.TypeKind is TypeKind.Enum)
            {
                return "Immunitas.Portal.UI.ApiProxies.Contracts.Enums." + nts.Name;
            }
            return nts.Name;
        }

        // fallback — печатаем короткое имя
        return type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }
}
