namespace ConstructionMaterialsManager.Models;

public partial class QualityCheck
{
    public int QualityCheckId { get; set; }

    public int MaterialId { get; set; }

    public int? DeliveryId { get; set; }

    public DateTime CheckDate { get; set; }

    public string BatchNumber { get; set; } = null!;

    public string Status { get; set; } = "На проверке";

    public string? InspectorName { get; set; }

    public string? TestResults { get; set; }

    public string? Comments { get; set; }

    public virtual Material Material { get; set; } = null!;

    public virtual Delivery? Delivery { get; set; }
}
