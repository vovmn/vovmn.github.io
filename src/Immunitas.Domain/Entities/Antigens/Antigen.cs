namespace Immunitas.Domain.Entities.Antigens
{
    /// <summary>
    /// Сущность, представляющая антиген
    /// </summary>
    public class Antigen : EntityBase
    {
        /// <summary>
        /// Название антигена
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Псевдоним антигена
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Id органа, к которому относится антиген
        /// </summary>
        public required int OrganId { get; init; }

        /// <summary>
        /// Орган, к которому относится антиген
        /// </summary>
        public Organ Organ { get; init; } = null!;
    }
}
