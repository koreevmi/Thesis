using ConstructionMaterialsManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConstructionMaterialsManager
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-JTTSOIJ;Database=RoadConstructionDB;User Id=koreev;Password=123;TrustServerCertificate=True;");
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
