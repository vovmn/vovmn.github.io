namespace Immunitas.Generators.Enums;

internal record EnumMemberModel
{
    public string Name { get; set; }
    public object? Value { get; set; }
    public string? Description { get; set; }

    public EnumMemberModel(string name, object? value, string? description)
    {
        Name = name;
        Value = value;
        Description = description;
    }
}

internal record EnumModel
{
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string UnderlyingType { get; set; }
    public bool IsFlags { get; set; }
    public List<EnumMemberModel> Members { get; set; }

    public EnumModel(
        string name,
        string @namespace,
        string underlyingType,
        bool isFlags,
        List<EnumMemberModel> members)
    {
        Name = name; 
        Namespace = @namespace;
        UnderlyingType = underlyingType;
        IsFlags = isFlags;
        Members = members.ToList();
    }
}
