namespace ConstructionMaterialsManager.Services;

/// <summary>
/// Базовый класс для сообщений об изменении сущностей.
/// </summary>
public class EntityChangedMessage
{
    public string EntityType { get; }
    public int? EntityId { get; }
    public ChangeType Change { get; }

    public EntityChangedMessage(string entityType, int? entityId = null, ChangeType change = ChangeType.Modified)
    {
        EntityType = entityType;
        EntityId = entityId;
        Change = change;
    }
}

public enum ChangeType
{
    Added,
    Modified,
    Deleted
}

// Конкретные сообщения для типобезопасной подписки
public class MaterialChangedMessage : EntityChangedMessage
{
    public MaterialChangedMessage(int? id = null, ChangeType change = ChangeType.Modified)
        : base("Material", id, change) { }
}

public class SupplierChangedMessage : EntityChangedMessage
{
    public SupplierChangedMessage(int? id = null, ChangeType change = ChangeType.Modified)
        : base("Supplier", id, change) { }
}

public class DeliveryChangedMessage : EntityChangedMessage
{
    public DeliveryChangedMessage(int? id = null, ChangeType change = ChangeType.Modified)
        : base("Delivery", id, change) { }
}

public class ProjectChangedMessage : EntityChangedMessage
{
    public ProjectChangedMessage(int? id = null, ChangeType change = ChangeType.Modified)
        : base("Project", id, change) { }
}

public class UserChangedMessage : EntityChangedMessage
{
    public UserChangedMessage(int? id = null, ChangeType change = ChangeType.Modified)
        : base("User", id, change) { }
}

public class ProjectMaterialChangedMessage : EntityChangedMessage
{
    public int ProjectId { get; }

    public ProjectMaterialChangedMessage(int projectId, int? id = null, ChangeType change = ChangeType.Modified)
        : base("ProjectMaterial", id, change)
    {
        ProjectId = projectId;
    }
}

public class QualityCheckChangedMessage : EntityChangedMessage
{
    public QualityCheckChangedMessage(int? id = null, ChangeType change = ChangeType.Modified)
        : base("QualityCheck", id, change) { }
}
