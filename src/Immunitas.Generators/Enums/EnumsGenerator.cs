using Microsoft.CodeAnalysis;

namespace Immunitas.Generators.Enums;

[Generator]
public sealed class EnumsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //Debugger.Launch();
        IncrementalValueProvider<Compilation> compilationProvider = context.CompilationProvider;

        var allEnums = compilationProvider.SelectMany(static (compilation, token) =>
        {
            var sharedContractAttribute = compilation.GetSharedContractAttributeSymbol()
                ?? throw new InvalidOperationException("SharedContractAttribute not found");
            var flagsAttribute = compilation.GetTypeByMetadataName("System.FlagsAttribute")
                ?? throw new InvalidOperationException("FlagsAttribute not found");
            var descriptionAttribute = compilation.GetTypeByMetadataName("System.ComponentModel.DescriptionAttribute")
                ?? throw new InvalidOperationException("DescriptionAttribute not found");

            var enumsNamespace = "Immunitas.Portal.UI.ApiProxies.Contracts.Enums";

            var enums = GetAllEnums(compilation);
            var enumsFilteredByAttribute = enums.Where(e => e.HasAttribute(sharedContractAttribute));

            return enumsFilteredByAttribute.Select(e =>
            {
                // Проверяем наличие [Flags]
                bool isFlags = e.HasAttribute(flagsAttribute);

                // Получаем базовый тип (int, byte, etc.)
                string underlyingType = e.EnumUnderlyingType?.ToDisplayString() ?? "int";

                // Собираем члены с их явными значениями
                var members = e.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(f => f.HasConstantValue)
                    .Select(f =>
                    {
                        var descriptionAttributeData = f.FindAttribute(descriptionAttribute);
                        var descriptionValue = descriptionAttributeData?.ConstructorArguments.FirstOrDefault().Value as string;
                        return new EnumMemberModel(f.Name, f.ConstantValue, descriptionValue);
                    })
                    .ToList();

                return new EnumModel(
                    e.Name,
                    enumsNamespace,
                    underlyingType,
                    isFlags,
                    members);
            });
        });

        context.RegisterSourceOutput(allEnums, static (spc, model) =>
        {
            var source = EnumsSourceGenerationHelpers.GenerateEnumSource(model);
            spc.AddSource($"{model.Name}.g.cs", source);
        });
    }

    // Метод для получения всех Enum из сборки
    private static IEnumerable<INamedTypeSymbol> GetAllEnums(Compilation compilation)
    {
        foreach (var reference in compilation.References)
        {
            var assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assembly != null && assembly.Name.StartsWith("Immunitas"))
            {
                foreach (var type in SourceGenerationHelper.GetAllTypes(assembly.GlobalNamespace))
                {
                    if (type.TypeKind == TypeKind.Enum)
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
