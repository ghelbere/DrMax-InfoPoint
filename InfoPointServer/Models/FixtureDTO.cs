namespace InfoPointServer.Models
{
    public class FixtureDto
    {
        public string Name { get; set; } = String.Empty;
        public string Type { get; set; } = String.Empty;
        public string Block { get; set; } = String.Empty;
        public string PositionHint { get; set; } = String.Empty;
        public CoordinatesDto Coordinates { get; set; } = new();
    }
}
