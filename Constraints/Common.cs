using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.BuildingBlocks;

public struct BaseConstraintData
{
    public ByLawItemType constraintType; 
    public ByLawConstraintType constraint;
    public ByLawItemCategory itemCategory;
    public ByLawPropertyOperator[] propertyOperators;
}

public enum BuildingDensity : byte
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 4
}

public static class ConstraintMapper
{
    public static ZoneDensity ToZoneDensity(BuildingDensity density)
    {
        return density switch
        {
            BuildingDensity.Low => ZoneDensity.Low,
            BuildingDensity.Medium => ZoneDensity.Medium,
            BuildingDensity.High => ZoneDensity.High,
            _ => throw new ArgumentOutOfRangeException(nameof(density), $"Unsupported BuildingDensity value: {density}"),
        };
    }
    public static BuildingDensity ToBuildingDensityConstraint(ZoneDensity density)
    {
        return density switch
        {
            ZoneDensity.Low => BuildingDensity.Low,
            ZoneDensity.Medium => BuildingDensity.Medium,
            ZoneDensity.High => BuildingDensity.High,
            _ => throw new ArgumentOutOfRangeException(nameof(density), $"Unsupported ZoneDensity value: {density}"),
        };
    }
} 