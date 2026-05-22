namespace ConstructionMaterialsManager.Models;

public partial class MaterialType
{
    public int MaterialTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Description { get; set; }

    public string DefaultUnit { get; set; } = null!;

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
