namespace Immunitas.Domain.Entities.Measurements
{
    /// <summary>
    /// Сущность, представляющая количество клеток крови определенного размера
    /// </summary>
    public record Point
    {
        /// <summary>
        /// Размер клетки крови
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Количество клеток определенного размера
        /// </summary>
        public double Y { get; set; }

        public Point() { }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X},{Y})";
    }
}
