using ConstructionMaterialsManager.Data;
using ConstructionMaterialsManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ConstructionMaterialsManager.Services;

public class MaterialCalculationResult
{
    public Material Material { get; set; } = null!;
    public double RoadLength { get; set; }
    public double RoadWidth { get; set; }
    public double LayerThickness { get; set; }
    public double VolumeCubicMeters { get; set; }
    public double MassTonnes { get; set; }
    public double PlannedQuantity { get; set; }
    public double ActualUsed { get; set; }
    public double Overrun { get; set; }
    public double OverrunPercent { get; set; }
    public decimal EstimatedCost { get; set; }
    public bool IsOverBudget { get; set; }
}

public interface ICalculatorService
{
    MaterialCalculationResult CalculateMaterialUsage(int materialId, double length, double width, double thickness, double lossPercent = 5);
    List<MaterialCalculationResult> CalculatePlanVsFact(int projectId);
    List<MaterialType> GetMaterialTypes();
}

public class CalculatorService : ICalculatorService
{
    private readonly ApplicationDbContext _context;

    public CalculatorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public MaterialCalculationResult CalculateMaterialUsage(int materialId, double length, double width, double thickness, double lossPercent = 5)
    {
        var material = _context.Materials
            .Include(m => m.MaterialType)
            .FirstOrDefault(m => m.MaterialId == materialId);

        if (material == null)
            throw new InvalidOperationException($"Материал с ID {materialId} не найден.");

        var volume = length * width * thickness;

        double massTonnes;
        if (material.Density.HasValue && material.Density.Value > 0)
        {
            massTonnes = volume * (double)material.Density.Value;
        }
        else
        {
            massTonnes = volume * 2.0;
        }

        var withLoss = massTonnes * (1 + lossPercent / 100.0);

        var plannedQuantity = material.Unit.Contains("т", StringComparison.OrdinalIgnoreCase) ||
                              material.Unit.Contains("ton", StringComparison.OrdinalIgnoreCase)
            ? withLoss
            : withLoss * 1000;

        var estimatedCost = (decimal)(plannedQuantity * (double)material.CostPerUnit);

        return new MaterialCalculationResult
        {
            Material = material,
            RoadLength = length,
            RoadWidth = width,
            LayerThickness = thickness,
            VolumeCubicMeters = volume,
            MassTonnes = massTonnes,
            PlannedQuantity = plannedQuantity,
            ActualUsed = 0,
            Overrun = 0,
            OverrunPercent = 0,
            EstimatedCost = estimatedCost,
            IsOverBudget = false
        };
    }

    public List<MaterialCalculationResult> CalculatePlanVsFact(int projectId)
    {
        var projectMaterials = _context.ProjectMaterials
            .Include(pm => pm.Material)
            .ThenInclude(m => m.MaterialType)
            .Where(pm => pm.ProjectId == projectId)
            .ToList();

        var results = new List<MaterialCalculationResult>();

        foreach (var pm in projectMaterials)
        {
            var material = pm.Material;
            var planned = (double)pm.PlannedQuantity;
            var actual = (double)(pm.UsedQuantity ?? 0);

            var overrun = actual - planned;
            var overrunPercent = planned > 0 ? (overrun / planned) * 100 : 0;

            var estimatedCost = pm.PlannedQuantity * material.CostPerUnit;

            results.Add(new MaterialCalculationResult
            {
                Material = material,
                RoadLength = 0,
                RoadWidth = 0,
                LayerThickness = 0,
                VolumeCubicMeters = 0,
                MassTonnes = 0,
                PlannedQuantity = planned,
                ActualUsed = actual,
                Overrun = overrun,
                OverrunPercent = overrunPercent,
                EstimatedCost = estimatedCost,
                IsOverBudget = overrunPercent > 5
            });
        }

        return results;
    }

    public List<MaterialType> GetMaterialTypes()
    {
        return _context.MaterialTypes.AsNoTracking().ToList();
    }
}
