using ConstructionMaterialsManager.Data;
using ConstructionMaterialsManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ConstructionMaterialsManager.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly ApplicationDbContext _context;

        public DatabaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public User GetUserByLogin(string login)
        {
            return _context.Users.AsNoTracking().FirstOrDefault(u => u.Login == login);
        }

        public List<User> GetUsers()
        {
            return _context.Users.AsNoTracking().ToList();
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            var existing = _context.Users.Find(user.UserId);
            if (existing != null)
            {
                existing.Login = user.Login;
                existing.Password = user.Password;
                existing.FullName = user.FullName;
                existing.Email = user.Email;
                existing.Role = user.Role;
                _context.SaveChanges();
            }
        }

        public void DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public List<Material> GetMaterials()
        {
            return _context.Materials.AsNoTracking().Include(m => m.Supplier).Include(m => m.MaterialType).ToList();
        }

        public List<MaterialType> GetMaterialTypes()
        {
            return _context.MaterialTypes.AsNoTracking().ToList();
        }

        public List<Supplier> GetSuppliers()
        {
            return _context.Suppliers.AsNoTracking().ToList();
        }

        public List<Project> GetProjects()
        {
            return _context.Projects.AsNoTracking().ToList();
        }

        public List<Delivery> GetDeliveries()
        {
            return _context.Deliveries.AsNoTracking().Include(d => d.Material).Include(d => d.Supplier).ToList();
        }

        public List<MaterialMovement> GetMaterialMovements()
        {
            return _context.MaterialMovements.AsNoTracking().Include(mm => mm.Material).ToList();
        }

        public List<ProjectMaterial> GetProjectMaterials(int projectId)
        {
            return _context.ProjectMaterials
                .AsNoTracking()
                .Include(pm => pm.Material)
                .Where(pm => pm.ProjectId == projectId)
                .ToList();
        }

        public void AddMaterial(Material material)
        {
            _context.Materials.Add(material);
            _context.SaveChanges();
        }

        public void UpdateMaterial(Material material)
        {
            var existing = _context.Materials.Find(material.MaterialId);
            if (existing != null)
            {
                existing.Name = material.Name;
                existing.Unit = material.Unit;
                existing.CostPerUnit = material.CostPerUnit;
                existing.SupplierId = material.SupplierId;
                existing.CurrentStock = material.CurrentStock;
                existing.MaterialTypeId = material.MaterialTypeId;
                existing.Density = material.Density;
                existing.Fraction = material.Fraction;
                existing.Gost = material.Gost;
                existing.StrengthGrade = material.StrengthGrade;
                existing.FrostResistance = material.FrostResistance;
                existing.WaterResistance = material.WaterResistance;
                existing.RadioactivityClass = material.RadioactivityClass;
                existing.Leshchadness = material.Leshchadness;
                existing.FinenessModule = material.FinenessModule;
                existing.StorageType = material.StorageType;
                existing.ShelfLifeDays = material.ShelfLifeDays;
                existing.Notes = material.Notes;
                _context.SaveChanges();
            }
        }

        public void DeleteMaterial(int materialId)
        {
            var material = _context.Materials
                .Include(m => m.Deliveries)
                .Include(m => m.MaterialMovements)
                .Include(m => m.ProjectMaterials)
                .Include(m => m.QualityChecks)
                .FirstOrDefault(m => m.MaterialId == materialId);

            if (material == null)
                return;

            if (material.Deliveries.Any())
                throw new InvalidOperationException($"Невозможно удалить материал «{material.Name}»: к нему привязано {material.Deliveries.Count} поставок. Сначала удалите поставки.");

            if (material.ProjectMaterials.Any())
                throw new InvalidOperationException($"Невозможно удалить материал «{material.Name}»: он используется в {material.ProjectMaterials.Count} проектах. Сначала удалите материалы из проектов.");

            if (material.QualityChecks.Any())
                throw new InvalidOperationException($"Невозможно удалить материал «{material.Name}»: к нему привязано {material.QualityChecks.Count} проверок качества. Сначала удалите проверки.");

            // Удаляем связанные движения
            _context.MaterialMovements.RemoveRange(material.MaterialMovements);

            _context.Materials.Remove(material);
            _context.SaveChanges();
        }

        public void AddSupplier(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            _context.SaveChanges();
        }

        public void UpdateSupplier(Supplier supplier)
        {
            var existing = _context.Suppliers.Find(supplier.SupplierId);
            if (existing != null)
            {
                existing.Name = supplier.Name;
                existing.ContactPerson = supplier.ContactPerson;
                existing.Phone = supplier.Phone;
                existing.Email = supplier.Email;
                existing.Address = supplier.Address;
                _context.SaveChanges();
            }
        }

        public void DeleteSupplier(int supplierId)
        {
            var supplier = _context.Suppliers
                .Include(s => s.Materials)
                .Include(s => s.Deliveries)
                .FirstOrDefault(s => s.SupplierId == supplierId);

            if (supplier == null)
                return;

            if (supplier.Materials.Any())
                throw new InvalidOperationException($"Невозможно удалить поставщика «{supplier.Name}»: к нему привязано {supplier.Materials.Count} материалов. Сначала перепривяжите или удалите материалы.");

            if (supplier.Deliveries.Any())
                throw new InvalidOperationException($"Невозможно удалить поставщика «{supplier.Name}»: к нему привязано {supplier.Deliveries.Count} поставок. Сначала удалите поставки.");

            _context.Suppliers.Remove(supplier);
            _context.SaveChanges();
        }

        public void AddProject(Project project)
        {
            _context.Projects.Add(project);
            _context.SaveChanges();
        }

        public void UpdateProjectMaterial(ProjectMaterial projectMaterial)
        {
            var existing = _context.ProjectMaterials.Find(projectMaterial.ProjectMaterialId);
            if (existing != null)
            {
                existing.PlannedQuantity = projectMaterial.PlannedQuantity;
                existing.UsedQuantity = projectMaterial.UsedQuantity;
                _context.SaveChanges();
            }
        }


        public void DeleteProject(int projectId)
        {
            var project = _context.Projects
                .Include(p => p.ProjectMaterials)
                .FirstOrDefault(p => p.ProjectId == projectId);
            if (project != null)
            {
                var relatedNotifications = _context.Notifications.Where(n => n.ProjectId == projectId).ToList();
                foreach (var notification in relatedNotifications)
                {
                    notification.ProjectId = null;
                }

                _context.ProjectMaterials.RemoveRange(project.ProjectMaterials);
                _context.Projects.Remove(project);
                _context.SaveChanges();
            }
        }


        public void AddDelivery(Delivery delivery)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var material = _context.Materials.Find(delivery.MaterialId);
                    if (material == null)
                    {
                        throw new InvalidOperationException($"Материал с ID {delivery.MaterialId} не найден.");
                    }

                    var supplierExists = _context.Suppliers.Any(s => s.SupplierId == delivery.SupplierId);
                    if (!supplierExists)
                    {
                        throw new InvalidOperationException($"Поставщик с ID {delivery.SupplierId} не найден.");
                    }

                    if (delivery.Quantity <= 0)
                    {
                        throw new InvalidOperationException("Количество должно быть больше нуля.");
                    }

                    _context.Deliveries.Add(delivery);

                    // Обновляем остаток материала на складе
                    material.CurrentStock = (material.CurrentStock ?? 0) + delivery.Quantity;
                    _context.Entry(material).State = EntityState.Modified;

                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateDelivery(Delivery delivery)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var oldDelivery = _context.Deliveries
                        .FirstOrDefault(d => d.DeliveryId == delivery.DeliveryId);

                    if (oldDelivery == null)
                    {
                        throw new InvalidOperationException($"Поставка с ID {delivery.DeliveryId} не найдена.");
                    }

                    int oldMaterialId = oldDelivery.MaterialId;
                    decimal oldQuantity = oldDelivery.Quantity;

                    var oldMaterial = _context.Materials.Find(oldMaterialId);
                    if (oldMaterial != null)
                    {
                        var newOldStock = (oldMaterial.CurrentStock ?? 0) - oldQuantity;
                        if (newOldStock < 0)
                            throw new InvalidOperationException(
                                $"Невозможно изменить поставку: остаток материала «{oldMaterial.Name}» станет отрицательным ({newOldStock:N2}).");
                        oldMaterial.CurrentStock = newOldStock;
                    }

                    var newMaterial = _context.Materials.Find(delivery.MaterialId);
                    if (newMaterial == null)
                    {
                        throw new InvalidOperationException($"Материал с ID {delivery.MaterialId} не найден.");
                    }

                    var supplierExists = _context.Suppliers.Any(s => s.SupplierId == delivery.SupplierId);
                    if (!supplierExists)
                    {
                        throw new InvalidOperationException($"Поставщик с ID {delivery.SupplierId} не найден.");
                    }

                    if (delivery.Quantity <= 0)
                    {
                        throw new InvalidOperationException("Количество должно быть больше нуля.");
                    }

                    newMaterial.CurrentStock = (newMaterial.CurrentStock ?? 0) + delivery.Quantity;

                    oldDelivery.MaterialId = delivery.MaterialId;
                    oldDelivery.Quantity = delivery.Quantity;
                    oldDelivery.DeliveryDate = delivery.DeliveryDate;
                    oldDelivery.SupplierId = delivery.SupplierId;

                    // Записываем движение: списание старой поставки
                    _context.MaterialMovements.Add(new MaterialMovement
                    {
                        MaterialId = oldMaterialId,
                        Quantity = oldQuantity,
                        MovementType = "Расход",
                        MovementDate = DateTime.Now
                    });

                    // Записываем движение: поступление новой поставки
                    _context.MaterialMovements.Add(new MaterialMovement
                    {
                        MaterialId = delivery.MaterialId,
                        Quantity = delivery.Quantity,
                        MovementType = "Поступление",
                        MovementDate = DateTime.Now
                    });

                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void DeleteDelivery(int deliveryId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var delivery = _context.Deliveries
                        .Include(d => d.QualityChecks)
                        .FirstOrDefault(d => d.DeliveryId == deliveryId);
                    if (delivery == null)
                        return;

                    // Обновляем остаток материала
                    var material = _context.Materials.Find(delivery.MaterialId);
                    if (material != null)
                    {
                        var newStock = (material.CurrentStock ?? 0) - delivery.Quantity;
                        if (newStock < 0)
                            throw new InvalidOperationException(
                                $"Невозможно удалить поставку: остаток материала «{material.Name}» станет отрицательным ({newStock:N2}).");
                        material.CurrentStock = newStock;
                    }

                    // Записываем движение: списание
                    _context.MaterialMovements.Add(new MaterialMovement
                    {
                        MaterialId = delivery.MaterialId,
                        Quantity = delivery.Quantity,
                        MovementType = "Расход",
                        MovementDate = DateTime.Now
                    });

                    _context.Deliveries.Remove(delivery);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void AddMaterialMovement(MaterialMovement movement)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var materialExists = _context.Materials.Any(m => m.MaterialId == movement.MaterialId);
                    if (!materialExists)
                    {
                        throw new InvalidOperationException($"Материал с ID {movement.MaterialId} не найден.");
                    }

                    if (movement.Quantity <= 0)
                    {
                        throw new InvalidOperationException("Количество должно быть больше нуля.");
                    }

                    _context.MaterialMovements.Add(movement);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void AddProjectMaterial(ProjectMaterial projectMaterial)
        {
            var projectExists = _context.Projects.Any(p => p.ProjectId == projectMaterial.ProjectId);
            if (!projectExists)
            {
                throw new InvalidOperationException($"Проект с ID {projectMaterial.ProjectId} не найден.");
            }

            var materialExists = _context.Materials.Any(m => m.MaterialId == projectMaterial.MaterialId);
            if (!materialExists)
            {
                throw new InvalidOperationException($"Материал с ID {projectMaterial.MaterialId} не найден.");
            }

            _context.ProjectMaterials.Add(projectMaterial);
            _context.SaveChanges();
        }


        public void RemoveProjectMaterial(int projectMaterialId)
        {
            var projectMaterial = _context.ProjectMaterials.Find(projectMaterialId);
            if (projectMaterial != null)
            {
                _context.ProjectMaterials.Remove(projectMaterial);
                _context.SaveChanges();
            }
        }

        public void UpdateProject(Project project)
        {
            var existing = _context.Projects.Find(project.ProjectId);
            if (existing != null)
            {
                existing.Name = project.Name;
                existing.Description = project.Description;
                existing.StartDate = project.StartDate;
                existing.EndDate = project.EndDate;
                existing.Budget = project.Budget;
                existing.Status = project.Status;
                _context.SaveChanges();
            }
        }

        public List<QualityCheck> GetQualityChecks(int? materialId = null)
        {
            var query = _context.QualityChecks
                .AsNoTracking()
                .Include(qc => qc.Material)
                .ThenInclude(m => m.MaterialType)
                .Include(qc => qc.Delivery)
                .AsQueryable();

            if (materialId.HasValue)
            {
                query = query.Where(qc => qc.MaterialId == materialId.Value);
            }

            return query.OrderByDescending(qc => qc.CheckDate).ToList();
        }

        public void AddQualityCheck(QualityCheck qualityCheck)
        {
            _context.QualityChecks.Add(qualityCheck);
            _context.SaveChanges();
        }

        public void UpdateQualityCheck(QualityCheck qualityCheck)
        {
            var existing = _context.QualityChecks.Find(qualityCheck.QualityCheckId);
            if (existing != null)
            {
                existing.MaterialId = qualityCheck.MaterialId;
                existing.DeliveryId = qualityCheck.DeliveryId;
                existing.CheckDate = qualityCheck.CheckDate;
                existing.BatchNumber = qualityCheck.BatchNumber;
                existing.Status = qualityCheck.Status;
                existing.InspectorName = qualityCheck.InspectorName;
                existing.TestResults = qualityCheck.TestResults;
                existing.Comments = qualityCheck.Comments;
                _context.SaveChanges();
            }
        }

        public void DeleteQualityCheck(int qualityCheckId)
        {
            var qualityCheck = _context.QualityChecks.Find(qualityCheckId);
            if (qualityCheck != null)
            {
                _context.QualityChecks.Remove(qualityCheck);
                _context.SaveChanges();
            }
        }
    }
}
