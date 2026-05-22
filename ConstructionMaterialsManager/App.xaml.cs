using ConstructionMaterialsManager.Data;
using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using ConstructionMaterialsManager.Views.Pages;
using ConstructionMaterialsManager.Views.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace ConstructionMaterialsManager
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }

        public IServiceProvider ServiceProvider { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            ServiceProvider.GetRequiredService<NotificationEventHandler>();
            SeedMaterialTypes(ServiceProvider);

            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer("Server=DESKTOP-JTTSOIJ;Database=RoadConstructionDB;User Id=koreev;Password=123;TrustServerCertificate=True;"));

            services.AddScoped<IDatabaseService, DatabaseService>();

            services.AddSingleton<IEventAggregator, EventAggregator>();

            services.AddSingleton<INotificationService, NotificationService>();

            services.AddSingleton<NotificationEventHandler>();

            services.AddSingleton<IExcelService, ExcelService>();
            services.AddScoped<ICalculatorService, CalculatorService>();
            services.AddLogging(configure => configure.AddDebug().AddConsole());

            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<MaterialsPage>();
            services.AddTransient<SuppliersPage>();
            services.AddTransient<ProjectsPage>();
            services.AddTransient<DeliveriesPage>();
            services.AddTransient<ReportsPage>();
            services.AddTransient<UsersPage>();
            services.AddTransient<NotificationsPage>();
            services.AddTransient<MaterialWindow>();
            services.AddTransient<SupplierWindow>();
            services.AddTransient<ProjectWindow>();
            services.AddTransient<DeliveryWindow>();
            services.AddTransient<UserWindow>();
            services.AddTransient<ProjectMaterialsWindow>();
            services.AddTransient<MaterialSelectionWindow>();
            services.AddTransient<EditProjectMaterialWindow>();
            services.AddTransient<MaterialCalculatorWindow>();
            services.AddTransient<QualityChecksPage>();
            services.AddTransient<QualityCheckWindow>();
        }

        private static void SeedMaterialTypes(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (context.MaterialTypes.Any()) return;

            context.MaterialTypes.AddRange(
                new MaterialType { Name = "Щебень", Category = "Нерудные материалы", DefaultUnit = "т", Description = "Гранитный, гравийный, известняковый щебень различных фракций" },
                new MaterialType { Name = "Песок", Category = "Нерудные материалы", DefaultUnit = "м³", Description = "Песок строительный, карьерный, речной" },
                new MaterialType { Name = "Асфальтобетон", Category = "Дорожные покрытия", DefaultUnit = "т", Description = "Горячий и холодный асфальтобетон" },
                new MaterialType { Name = "Битум", Category = "Вяжущие материалы", DefaultUnit = "т", Description = "Дорожный битум, битумные эмульсии" },
                new MaterialType { Name = "Геотекстиль", Category = "Геосинтетика", DefaultUnit = "м²", Description = "Геотекстиль, георешетки, геомембраны" },
                new MaterialType { Name = "Грунт", Category = "Нерудные материалы", DefaultUnit = "м³", Description = "Грунт для устройства земляного полотна" },
                new MaterialType { Name = "Цемент", Category = "Вяжущие материалы", DefaultUnit = "т", Description = "Портландцемент различных марок" },
                new MaterialType { Name = "Бетон", Category = "Дорожные покрытия", DefaultUnit = "м³", Description = "Товарный бетон для дорожных конструкций" }
            );

            context.SaveChanges();
        }

    }
}
