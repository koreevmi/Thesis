namespace ConstructionMaterialsManager.Models;

public partial class Material
{
    public int MaterialId { get; set; }

    public string Name { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public decimal CostPerUnit { get; set; }

    public int SupplierId { get; set; }

    public decimal? CurrentStock { get; set; }

    public int? MaterialTypeId { get; set; }

    public decimal? Density { get; set; }

    public string? Fraction { get; set; }

    public string? Gost { get; set; }

    public string? StrengthGrade { get; set; }

    public string? FrostResistance { get; set; }

    public string? WaterResistance { get; set; }

    public string? RadioactivityClass { get; set; }

    public string? Leshchadness { get; set; }

    public string? FinenessModule { get; set; }

    public string StorageType { get; set; } = "Открытый";

    public int? ShelfLifeDays { get; set; }

    public string? Notes { get; set; }

    public virtual MaterialType? MaterialType { get; set; }

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual ICollection<MaterialMovement> MaterialMovements { get; set; } = new List<MaterialMovement>();

    public virtual ICollection<ProjectMaterial> ProjectMaterials { get; set; } = new List<ProjectMaterial>();

    public virtual ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
}
