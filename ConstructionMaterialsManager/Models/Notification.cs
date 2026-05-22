namespace ConstructionMaterialsManager.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Type { get; set; } = null!; // Warning, Info, Success

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? MaterialId { get; set; }

    public int? ProjectId { get; set; }

    public virtual Material? Material { get; set; }

    public virtual Project? Project { get; set; }
}
