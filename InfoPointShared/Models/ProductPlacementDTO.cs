namespace InfoPoint.Models
{
    public class ProductPlacementDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public FixtureDto Fixture { get; set; } = new();
        public PlacementMetadataDto Placement { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string PlanogramImageBase64 { get; set; } = string.Empty;
    }
}
