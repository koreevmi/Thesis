using ConstructionMaterialsManager.Data;
using ConstructionMaterialsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConstructionMaterialsManager.Services;

public interface INotificationService
{
    List<Notification> GetNotifications(bool onlyUnread = false);
    int GetUnreadCount();
    void AddNotification(Notification notification);
    void MarkAsRead(int notificationId);
    void MarkAllAsRead();
    void DeleteNotification(int notificationId);
    void CheckMaterialShortages();
}

public class NotificationService : INotificationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private const decimal MinStockThreshold = 10;

    public NotificationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public List<Notification> GetNotifications(bool onlyUnread = false)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            IQueryable<Notification> query = context.Notifications
                .AsNoTracking()
                .Include(n => n.Material)
                .Include(n => n.Project);

            if (onlyUnread)
                query = query.Where(n => !n.IsRead);

            return query.OrderByDescending(n => n.CreatedAt).ToList();
        }
        catch (Exception)
        {
            return new List<Notification>();
        }
    }

    public int GetUnreadCount()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            return context.Notifications.Count(n => !n.IsRead);
        }
        catch
        {
            return 0;
        }
    }

    public void AddNotification(Notification notification)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        notification.CreatedAt = DateTime.Now;
        notification.IsRead = false;
        context.Notifications.Add(notification);
        context.SaveChanges();
    }

    public void MarkAsRead(int notificationId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var notification = context.Notifications.Find(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            context.SaveChanges();
        }
    }

    public void MarkAllAsRead()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var unread = context.Notifications.Where(n => !n.IsRead).ToList();
        foreach (var n in unread)
            n.IsRead = true;
        context.SaveChanges();
    }

    public void DeleteNotification(int notificationId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var notification = context.Notifications.Find(notificationId);
        if (notification != null)
        {
            context.Notifications.Remove(notification);
            context.SaveChanges();
        }
    }

    public void CheckMaterialShortages()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            var existing = context.Notifications
                .Where(n => n.Type == "Warning" && n.Title.Contains("Нехватка"))
                .ToList();

            foreach (var e in existing)
                context.Notifications.Remove(e);
            context.SaveChanges();

            var lowStockMaterials = context.Materials
                .Where(m => m.CurrentStock == null || m.CurrentStock < MinStockThreshold)
                .ToList();

            foreach (var material in lowStockMaterials)
            {
                var stock = material.CurrentStock ?? 0;
                context.Notifications.Add(new Notification
                {
                    Title = $"Нехватка: {material.Name}",
                    Message = $"Остаток материала «{material.Name}» составляет {stock:N2} {material.Unit}. Рекомендуется заказать поставку.",
                    Type = "Warning",
                    MaterialId = material.MaterialId,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            var activeProjects = context.Projects
                .Where(p => p.Status == "В процессе" || p.Status == "Запланирован")
                .ToList();

            foreach (var project in activeProjects)
            {
                var projectMaterials = context.ProjectMaterials
                    .Where(pm => pm.ProjectId == project.ProjectId)
                    .ToList();

                foreach (var pm in projectMaterials)
                {
                    var material = context.Materials.Find(pm.MaterialId);
                    if (material != null)
                    {
                        var needed = pm.PlannedQuantity - (pm.UsedQuantity ?? 0);
                        var available = material.CurrentStock ?? 0;

                        if (needed > available)
                        {
                            context.Notifications.Add(new Notification
                            {
                                Title = $"Дефицит для проекта «{project.Name}»",
                                Message = $"Материал «{material.Name}»: требуется {needed:N2} {material.Unit}, доступно {available:N2} {material.Unit}. Не хватает {needed - available:N2} {material.Unit}.",
                                Type = "Warning",
                                MaterialId = material.MaterialId,
                                ProjectId = project.ProjectId,
                                CreatedAt = DateTime.Now,
                                IsRead = false
                            });
                        }
                    }
                }
            }

            context.SaveChanges();
        }
        catch (Exception)
        {
        }
    }
}
