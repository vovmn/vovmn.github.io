namespace Immunitas.Generators.Enums;

internal static class EnumsSourceGenerationHelpers
{
    public static string GenerateEnumSource(EnumModel model)
    {
        var sourceBuilder = new System.Text.StringBuilder();
        sourceBuilder.AppendLine("/* auto-generated, do not modify */");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("using System;");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine($"namespace {model.Namespace}");
        sourceBuilder.AppendLine("{");
        if (model.IsFlags)
        {
            sourceBuilder.AppendLine("    [System.Flags]");
        }
        sourceBuilder.AppendLine($"    public enum {model.Name} : {model.UnderlyingType}");
        sourceBuilder.AppendLine("    {");
        foreach (var member in model.Members)
        {
            if (member.Description != null)
            {
                sourceBuilder.AppendLine($"        [System.ComponentModel.Description(\"{member.Description}\")]");
            }
            sourceBuilder.AppendLine($"        {member.Name} = {member.Value},");
        }
        sourceBuilder.AppendLine("    }");

        if (model.Members.Any(member => member.Description != null)) {
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"    public static class {model.Name}Extensions");
            sourceBuilder.AppendLine("    {");
            sourceBuilder.AppendLine($"        public static string ToFriendlyString(this {model.Name} value)");
            sourceBuilder.AppendLine("        {");
            sourceBuilder.AppendLine("            switch (value)");
            sourceBuilder.AppendLine("            {");
            foreach (var member in model.Members)
            {
                if (member.Description != null)
                {
                    sourceBuilder.AppendLine($"                case {model.Name}.{member.Name}: return \"{member.Description}\";");
                }
            }
            sourceBuilder.AppendLine("                default: return value.ToString();");
            sourceBuilder.AppendLine("            }");
            sourceBuilder.AppendLine("        }");
            sourceBuilder.AppendLine("    }");
        }
        sourceBuilder.AppendLine("}");
        return sourceBuilder.ToString();
    }
}
