namespace Immunitas.Generators.Contracts
{
    public record ClassRecord
    {
        public string Name { get; set; }

        public string Namespace { get; set; }

        public List<PropertyRecord> Properties { get; set; }

        public ClassRecord(string name, string @namespace, List<PropertyRecord> properties)
        {
            Name = name;
            Namespace = @namespace;
            Properties = properties;
        }
    }

    public record PropertyRecord
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public PropertyRecord(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
