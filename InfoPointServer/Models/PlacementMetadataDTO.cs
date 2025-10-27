namespace InfoPointServer.Models
{
    public class PlacementMetadataDto
    {
        public string PlanogramId { get; set; } = String.Empty;
        public string State { get; set; } = String.Empty;
        public bool MultiplePlacements { get; set; }
    }
}
