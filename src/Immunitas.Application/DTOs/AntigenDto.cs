using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs
{
    [SharedContract]
    public class AntigenDto
    {
        public required int Id { get; set; }

        public required string Name { get; set; }

        public required string SystemName { get; set; }
    }
}
