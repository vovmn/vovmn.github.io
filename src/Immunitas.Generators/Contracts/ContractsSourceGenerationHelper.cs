using System.Text;

namespace Immunitas.Generators.Contracts
{
    public static class ContractsSourceGenerationHelper
    {
        internal static string GenerateContract(ClassRecord value)
        {
            var sb = new StringBuilder();

            sb.AppendLine("/* auto-generated, do not modify */");
            sb.AppendLine();
            sb.AppendLine("#nullable enable\n");
            sb.AppendLine($$"""
namespace {{value.Namespace}}
{
    public class {{value.Name}}
    {
""");
            foreach (var property in value.Properties)
            {
                sb.AppendLine($$"""
        public required {{property.Type}} {{property.Name}} { get; set; }
""");
            }

            sb.AppendLine("""
    }
""");
            sb.AppendLine("""
}
""");
            return sb.ToString();
        }
    }
}
