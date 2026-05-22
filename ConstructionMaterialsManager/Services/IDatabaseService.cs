using ConstructionMaterialsManager.Models;

namespace ConstructionMaterialsManager.Services
{
    public interface IDatabaseService
    {
        User GetUserByLogin(string login);
        List<User> GetUsers();
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(int userId);

        List<Material> GetMaterials();
        List<MaterialType> GetMaterialTypes();
        List<Supplier> GetSuppliers();
        List<Project> GetProjects();
        List<Delivery> GetDeliveries();
        List<MaterialMovement> GetMaterialMovements();
        List<ProjectMaterial> GetProjectMaterials(int projectId);
        void AddMaterial(Material material);
        void UpdateMaterial(Material material);
        void DeleteMaterial(int materialId);
        void AddSupplier(Supplier supplier);
        void UpdateSupplier(Supplier supplier);
        void DeleteSupplier(int supplierId);
        void AddProject(Project project);
        void UpdateProject(Project project);
        void DeleteProject(int projectId);
        void AddDelivery(Delivery delivery);
        void UpdateDelivery(Delivery delivery);
        void DeleteDelivery(int deliveryId);
        void AddMaterialMovement(MaterialMovement movement);
        void AddProjectMaterial(ProjectMaterial projectMaterial);
        void UpdateProjectMaterial(ProjectMaterial projectMaterial);
        void RemoveProjectMaterial(int projectMaterialId);

        List<QualityCheck> GetQualityChecks(int? materialId = null);
        void AddQualityCheck(QualityCheck qualityCheck);
        void UpdateQualityCheck(QualityCheck qualityCheck);
        void DeleteQualityCheck(int qualityCheckId);

    }
}
