using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConstructionMaterialsManager.Data;

/// <summary>
/// Обработчик событий для автоматического создания уведомлений
/// </summary>
public class NotificationEventHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventAggregator _eventAggregator;

    public NotificationEventHandler(IServiceProvider serviceProvider, IEventAggregator eventAggregator)
    {
        _serviceProvider = serviceProvider;
        _eventAggregator = eventAggregator;
        Subscribe();
    }

    private void Subscribe()
    {
        _eventAggregator.Subscribe<MaterialChangedMessage>(OnMaterialChanged);
        _eventAggregator.Subscribe<SupplierChangedMessage>(OnSupplierChanged);
        _eventAggregator.Subscribe<DeliveryChangedMessage>(OnDeliveryChanged);
        _eventAggregator.Subscribe<ProjectChangedMessage>(OnProjectChanged);
    }

    private void OnMaterialChanged(MaterialChangedMessage msg)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            if (msg.Change == ChangeType.Added)
            {
                var material = db.GetMaterials().FirstOrDefault(m => m.MaterialId == msg.EntityId);
                if (material != null)
                {
                    notificationService.AddNotification(new Notification
                    {
                        Title = $"Материал добавлен: {material.Name}",
                        Message = $"Новый материал «{material.Name}» ({material.Unit}) добавлен в систему. Стоимость: {material.CostPerUnit:N2} ₽.",
                        Type = "Info",
                        MaterialId = material.MaterialId,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });
                }
            }
        }
        catch
        {
            // Игнорируем ошибки создания уведомлений
        }
    }

    private void OnSupplierChanged(SupplierChangedMessage msg)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            if (msg.Change == ChangeType.Added)
            {
                var suppliers = db.GetSuppliers();
                var supplier = suppliers.FirstOrDefault(s => s.SupplierId == msg.EntityId);
                if (supplier != null)
                {
                    notificationService.AddNotification(new Notification
                    {
                        Title = $"Поставщик добавлен: {supplier.Name}",
                        Message = $"Новый поставщик «{supplier.Name}» добавлен в систему. Контакт: {supplier.ContactPerson}, тел: {supplier.Phone}.",
                        Type = "Info",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });
                }
            }
        }
        catch
        {
            // Игнорируем ошибки создания уведомлений
        }
    }

    private void OnDeliveryChanged(DeliveryChangedMessage msg)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            if (msg.Change == ChangeType.Added)
            {
                var deliveries = db.GetDeliveries();
                var delivery = deliveries.FirstOrDefault(d => d.DeliveryId == msg.EntityId);
                if (delivery != null)
                {
                    notificationService.AddNotification(new Notification
                    {
                        Title = $"Поставка принята: {delivery.Material?.Name}",
                        Message = $"Принято {delivery.Quantity:N2} {delivery.Material?.Unit} материала «{delivery.Material?.Name}» от поставщика «{delivery.Supplier?.Name}».",
                        Type = "Success",
                        MaterialId = delivery.MaterialId,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });
                }
            }
        }
        catch
        {
            // Игнорируем ошибки создания уведомлений
        }
    }

    private void OnProjectChanged(ProjectChangedMessage msg)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            if (msg.Change == ChangeType.Added)
            {
                var projects = db.GetProjects();
                var project = projects.FirstOrDefault(p => p.ProjectId == msg.EntityId);
                if (project != null)
                {
                    notificationService.AddNotification(new Notification
                    {
                        Title = $"Проект создан: {project.Name}",
                        Message = $"Новый проект «{project.Name}» создан. Бюджет: {project.Budget:N2} ₽. Период: {project.StartDate:dd.MM.yyyy} — {project.EndDate:dd.MM.yyyy}.",
                        Type = "Info",
                        ProjectId = project.ProjectId,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });
                }
            }
            else if (msg.Change == ChangeType.Modified)
            {
                var projects = db.GetProjects();
                var project = projects.FirstOrDefault(p => p.ProjectId == msg.EntityId);
                if (project != null && project.Status == "Завершен")
                {
                    notificationService.AddNotification(new Notification
                    {
                        Title = $"Проект завершён: {project.Name}",
                        Message = $"Проект «{project.Name}» успешно завершён. Итоговый бюджет: {project.Budget:N2} ₽.",
                        Type = "Success",
                        ProjectId = project.ProjectId,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });
                }
            }
        }
        catch
        {
            // Игнорируем ошибки создания уведомлений
        }
    }
}
