using ConstructionMaterialsManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ConstructionMaterialsManager.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Delivery> Deliveries { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<MaterialMovement> MaterialMovements { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectMaterial> ProjectMaterials { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<MaterialType> MaterialTypes { get; set; }

    public virtual DbSet<QualityCheck> QualityChecks { get; set; }

    // Connection string configured in App.xaml.cs via AddDbContext
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is provided via DI in App.xaml.cs
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.DeliveryId).HasName("PK__Deliveri__626D8FCEEC599B0A");

            entity.Property(e => e.DeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BatchNumber).HasMaxLength(100);
            entity.Property(e => e.CertificateNumber).HasMaxLength(100);
            entity.Property(e => e.CertificateDate).HasColumnType("datetime");
            entity.Property(e => e.Manufacturer).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Material).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Deliverie__Mater__47DBAE45");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Deliverie__Suppl__48CFD27E");
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("PK__Material__C50610F7D11276D9");

            entity.Property(e => e.CostPerUnit).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CurrentStock)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.Density).HasColumnType("decimal(8, 3)");
            entity.Property(e => e.Fraction).HasMaxLength(50);
            entity.Property(e => e.Gost).HasMaxLength(100);
            entity.Property(e => e.StrengthGrade).HasMaxLength(50);
            entity.Property(e => e.FrostResistance).HasMaxLength(50);
            entity.Property(e => e.WaterResistance).HasMaxLength(50);
            entity.Property(e => e.RadioactivityClass).HasMaxLength(20);
            entity.Property(e => e.Leshchadness).HasMaxLength(50);
            entity.Property(e => e.FinenessModule).HasMaxLength(50);
            entity.Property(e => e.StorageType).HasMaxLength(50).HasDefaultValue("Открытый");
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Supplier).WithMany(p => p.Materials)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Materials__Suppl__403A8C7D");

            entity.HasOne(d => d.MaterialType).WithMany(p => p.Materials)
                .HasForeignKey(d => d.MaterialTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Materials__MaterialType");
        });

        modelBuilder.Entity<MaterialMovement>(entity =>
        {
            entity.HasKey(e => e.MovementId).HasName("PK__Material__D18224460B0AAAE5");

            entity.Property(e => e.MovementDate).HasColumnType("datetime");
            entity.Property(e => e.MovementType).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Material).WithMany(p => p.MaterialMovements)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MaterialM__Mater__5535A963");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Projects__761ABEF050B6DF95");

            entity.Property(e => e.Budget).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<ProjectMaterial>(entity =>
        {
            entity.HasKey(e => e.ProjectMaterialId).HasName("PK__ProjectM__4796558050D0AC06");

            entity.Property(e => e.PlannedQuantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UsedQuantity)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Material).WithMany(p => p.ProjectMaterials)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProjectMa__Mater__4F7CD00D");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectMaterials)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProjectMa__Proje__4E88ABD4");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE666B4468EA149");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C148AA2E5");

            entity.HasIndex(e => e.Login, "UQ__Users__5E55825B6FF0D7AF").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);

            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(e => e.Material)
                .WithMany()
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Notifications__Material");

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Notifications__Project");
        });

        modelBuilder.Entity<MaterialType>(entity =>
        {
            entity.HasKey(e => e.MaterialTypeId).HasName("PK__MaterialTypes");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DefaultUnit).HasMaxLength(20);
        });

        modelBuilder.Entity<QualityCheck>(entity =>
        {
            entity.HasKey(e => e.QualityCheckId).HasName("PK__QualityChecks");

            entity.Property(e => e.CheckDate).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.BatchNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("На проверке");
            entity.Property(e => e.InspectorName).HasMaxLength(100);
            entity.Property(e => e.TestResults).HasMaxLength(1000);
            entity.Property(e => e.Comments).HasMaxLength(500);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.QualityChecks)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK__QualityChecks__Material");

            entity.HasOne(e => e.Delivery)
                .WithMany(d => d.QualityChecks)
                .HasForeignKey(e => e.DeliveryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__QualityChecks__Delivery");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
