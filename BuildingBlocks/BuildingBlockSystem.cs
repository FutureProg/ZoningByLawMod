using Colossal.Mathematics;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using Trejak.ZoningByLaw;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.UISystems;
using Unity.Entities;

namespace ZoningByLaw.BuildingBlocks
{
    public static class BuildingBlockSystem
    {

        public struct EvaluationParams
        {
            public PollutionThresholdsSet pollutionsThresholds;
            public ComponentLookup<ObjectData> objectdataLookup;
        }

        public static bool Evaluate(Entity building, BuildingData buildingData, BuildingPropertyData propertyData, BuildingByLawProperties properties, ByLawItem item, EvaluationParams evalParams)
        {
            if (!properties.initialized)
            {
                return false;
            }
            // An empty/None item is not a real constraint. It must pass, otherwise a single
            // stray item (e.g. legacy data) would AND every building down to zero matches.
            if (item.byLawItemType == ByLawItemType.None)
            {
                return true;
            }
            switch(item.constraintType)
            {
                case ByLawConstraintType.Count:
                    return EvaluateCount(building, properties, item, evalParams);
                case ByLawConstraintType.Length:
                    return EvaluateLength(building, buildingData, properties, item, evalParams);
                case ByLawConstraintType.MultiSelect:
                    return EvaluateMultiSelect(building, propertyData, properties, item, evalParams);
                case ByLawConstraintType.SingleSelect:
                    return EvaluateSingleSelect(building, properties, item, evalParams);
                case ByLawConstraintType.None:
                default:
                    return false;
            }
        }

        private static bool EvaluateSingleSelect(Entity building, BuildingByLawProperties properties, ByLawItem item, EvaluationParams evalParams)
        {
            switch (item.byLawItemType)
            {
                case ByLawItemType.AirPollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.NoisePollutionLevel:
                    return EvalPollution(building, properties, item, evalParams);
                default:
                    return false;
            }
        }

        private static bool EvaluateMultiSelect(Entity building, BuildingPropertyData propertyData, BuildingByLawProperties properties, ByLawItem item, EvaluationParams evalParams)
        {
            switch(item.byLawItemType)
            {
                case ByLawItemType.Uses:
                    return EvalLandUse(building, properties, item, evalParams);
                case ByLawItemType.AssetPack:
                    return EvalAssetPack(item, properties);
                case ByLawItemType.Density:
                    return EvalDensity(item, properties);
                default:
                    return false;
            }
        }

        public static bool EvalAssetPack(ByLawItem item, BuildingByLawProperties properties)
        {
            for(int i = 0; i < item.valueNumberArray.Length; i++)
            {
                for (int j = 0; j < properties.assetPacks.Length; j++)
                {
                    if (properties.assetPacks[j] == item.valueNumberArray[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static float PollutionLevelValue(ByLawItemType itemType, BuildingByLawProperties properties) => itemType switch
        {
            ByLawItemType.AirPollutionLevel => properties.pollutionData.m_AirPollution,
            ByLawItemType.GroundPollutionLevel => properties.pollutionData.m_GroundPollution,
            ByLawItemType.NoisePollutionLevel => properties.pollutionData.m_NoisePollution,
            _ => 0.0f
        };

        public static bool EvalDensity(ByLawItem item, BuildingByLawProperties properties)
        {
            var density = properties.buildingDensity;
            var densityMask = (BuildingDensity) item.valueByteFlag;
            // BuildingDensity.None means the building has no density classification (e.g. it isn't a
            // zone-spawned building), so it participates in no density constraint regardless of operator.
            // Every branch below excludes it so Is, IsNot, and AtLeastOne stay consistent.
            switch (item.propertyOperator)
            {
                // Is is no longer offered in GetPropertyOperators (identical to AtLeastOne for a
                // single-value density), but is still evaluated here for any by-law saved before that change.
                case ByLawPropertyOperator.Is:
                    return (densityMask & density) == density && density != BuildingDensity.None;
                case ByLawPropertyOperator.IsNot:
                    return (densityMask & density) == 0 && density != BuildingDensity.None;
                case ByLawPropertyOperator.AtLeastOne:
                    return (densityMask & density) != 0;
                default:
                    return false;
            }
        }

        public static bool EvalPollution(Entity building, BuildingByLawProperties properties, ByLawItem item, EvaluationParams evalParams)
        {
            float basePollutionValue = PollutionLevelValue(item.byLawItemType, properties);
            var pollutionLimit = (ByLawPollutionThreshold) item.valueByteFlag;
            var thresholdData = evalParams.pollutionsThresholds.ground;
            if (item.byLawItemType == ByLawItemType.AirPollutionLevel)
            {
                thresholdData = evalParams.pollutionsThresholds.air;
            } 
            else if (item.byLawItemType == ByLawItemType.NoisePollutionLevel)
            {
                thresholdData = evalParams.pollutionsThresholds.noise;
            }

            if (basePollutionValue > thresholdData.low && pollutionLimit == ByLawPollutionThreshold.None)
            {
                return false;
            }
            if (basePollutionValue > thresholdData.medium && pollutionLimit <= ByLawPollutionThreshold.Low)
            {
                return false;
            }
            if (basePollutionValue > thresholdData.high && pollutionLimit <= ByLawPollutionThreshold.Medium)
            {
                return false;
            }
            return true;
        }

        public static bool EvalLandUse(Entity building, BuildingByLawProperties properties, ByLawItem item, EvaluationParams evalParams)
        {
            var objectData = evalParams.objectdataLookup[building];

            int matchCount = 0;
            int missCount = 0;
            var flag = (ByLawZoneType) item.valueByteFlag;            
            if (properties.isExtractor) // extractors only function when plopped down, so won't be spawning them
            {
                return false;
            }
            if ((ByLawZoneType.Residential & flag) == 0 && properties.isResidential)
            {
                missCount++;
            }
            if ((ByLawZoneType.Office & flag) == 0 && properties.isOffice)
            {
                missCount++;
            }
            if ((ByLawZoneType.Commercial & flag) == 0 && properties.isCommercial)
            {
                missCount++;
            }
            if ((ByLawZoneType.Industrial & flag) == 0 && properties.isIndustry)
            {
                missCount++;
            }

            matchCount += (ByLawZoneType.Residential & flag) != 0 && properties.isResidential ? 1 : 0;
            matchCount += (ByLawZoneType.Office & flag) != 0 && properties.isOffice ? 1 : 0;
            matchCount += (ByLawZoneType.Industrial & flag) != 0 && properties.isIndustry ? 1 : 0;
            matchCount += (ByLawZoneType.Commercial & flag) != 0 && properties.isCommercial ? 1 : 0;

            int numberOfFlags = 0;
            numberOfFlags += (ByLawZoneType.Residential & flag) != 0 ? 1 : 0;
            numberOfFlags += (ByLawZoneType.Office & flag) != 0 ? 1 : 0;
            numberOfFlags += (ByLawZoneType.Industrial & flag) != 0 ? 1 : 0;
            numberOfFlags += (ByLawZoneType.Commercial & flag) != 0 ? 1 : 0;

            switch (item.propertyOperator)
            {
                case ByLawPropertyOperator.AtLeastOne:
                    return matchCount >= 1;
                case ByLawPropertyOperator.OnlyOneOf:
                    return matchCount == 1 && missCount == 0;
                case ByLawPropertyOperator.IsNot:
                    return matchCount == 0;
                case ByLawPropertyOperator.Is:
                    return matchCount == numberOfFlags && missCount == 0;
            }
            return true;
        }

        public static bool EvalFlags(int flag, int value)
        {
            return (flag & value) != 0;
        }

        private static bool EvaluateLength(Entity building, BuildingData buildingData, BuildingByLawProperties properties, ByLawItem item, EvaluationParams evalParams)
        {            
            switch (item.byLawItemType)
            {
                case ByLawItemType.Height:
                    return EvalBounds(item.valueBounds1, properties.buildingHeight);
                case ByLawItemType.LotSize:
                    var lotSize = buildingData.m_LotSize.x * buildingData.m_LotSize.y * 8;
                    return EvalBounds(item.valueBounds1, lotSize);
                case ByLawItemType.LotWidth:
                    return EvalBounds(item.valueBounds1, buildingData.m_LotSize.x * 8);
                case ByLawItemType.LotDepth:
                    return EvalBounds(item.valueBounds1, buildingData.m_LotSize.y * 8);
                case ByLawItemType.FrontSetback:
                    return properties.checkedBuildingSetBack && EvalBounds(item.valueBounds1, properties.buildingSetbackFront);
                case ByLawItemType.RearSetback:
                    return properties.checkedBuildingSetBack && EvalBounds(item.valueBounds1, properties.buildingSetBackRear);                    
                case ByLawItemType.LeftSetback:
                    return properties.checkedBuildingSetBack && EvalBounds(item.valueBounds1, properties.buildingSetBackLeft);
                case ByLawItemType.RightSetback:
                    return properties.checkedBuildingSetBack && EvalBounds(item.valueBounds1, properties.buildingSetBackRight);                    
                default:
                    return false;
            }
        }

        public static bool EvaluateCount(Entity building, BuildingByLawProperties properties, ByLawItem item, EvaluationParams evalParams)
        {
            switch(item.byLawItemType)
            {
                case ByLawItemType.Parking:                    
                    return EvalBounds(item.valueBounds1, properties.parkingCount);
                default:
                    return false;
            }            
        }

        public static bool EvalBounds(Bounds1 bounds, float value)
        {
            bool re = true;
            if (bounds.min > 0)
            {
                re = re && value >= bounds.min;
            }
            if (bounds.max >= 0)
            {
                re = re && value <= bounds.max;
            }
            return re;
        }

        public static Type GetConstraintEnumType(ByLawItemType itemType)
        {
            switch (itemType)
            {
                case ByLawItemType.Uses:
                    return typeof(ByLawZoneType);
                case ByLawItemType.AirPollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.NoisePollutionLevel:
                    return typeof(ByLawPollutionThreshold);
                case ByLawItemType.Density:
                    return typeof(BuildingDensity);
                default:
                    return null;
            }            
        }

        public static int[] GetConstarintEnumValues(ByLawItemType itemType)
        {
            switch (itemType)
            {
                case ByLawItemType.Uses:
                    return Array.ConvertAll((ByLawZoneType[])Enum.GetValues(typeof(ByLawZoneType)), e => (int) e);
                case ByLawItemType.AirPollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.NoisePollutionLevel:
                    return Array.ConvertAll((ByLawPollutionThreshold[])Enum.GetValues(typeof(ByLawPollutionThreshold)), e => (int) e);
                case ByLawItemType.Density:
                    return Array.ConvertAll((BuildingDensity[])Enum.GetValues(typeof(BuildingDensity)), e => (int)e);
                default:
                    return null;
            }
        }

        public static ByLawConstraintType GetConstraintTypes(ByLawItemType itemType)
        {
            switch(itemType)
            {
                case ByLawItemType.Uses:
                case ByLawItemType.AssetPack:
                case ByLawItemType.Density:
                    return ByLawConstraintType.MultiSelect;
                case ByLawItemType.Height:
                case ByLawItemType.LotWidth:
                case ByLawItemType.LotSize:
                case ByLawItemType.LotDepth:
                case ByLawItemType.Parking:
                case ByLawItemType.FrontSetback:
                case ByLawItemType.LeftSetback:
                case ByLawItemType.RightSetback:
                case ByLawItemType.RearSetback:
                    return ByLawConstraintType.Length;
                case ByLawItemType.NoisePollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.AirPollutionLevel:
                    return ByLawConstraintType.SingleSelect;
                case ByLawItemType.None:
                default:
                    return ByLawConstraintType.None;
                
            }
        }

        public static ByLawItemCategory GetItemCategories(ByLawItemType itemType)
        {
            switch (itemType)
            {
                case ByLawItemType.Uses:
                case ByLawItemType.LotWidth:
                case ByLawItemType.LotSize:
                case ByLawItemType.LotDepth:
                case ByLawItemType.Parking:
                case ByLawItemType.AssetPack:
                    return ByLawItemCategory.Lot;

                case ByLawItemType.Height:               
                case ByLawItemType.FrontSetback:
                case ByLawItemType.LeftSetback:
                case ByLawItemType.RightSetback:
                case ByLawItemType.RearSetback:
                case ByLawItemType.Density:
                    return ByLawItemCategory.Building;

                case ByLawItemType.NoisePollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.AirPollutionLevel:
                    return ByLawItemCategory.Pollution;

                case ByLawItemType.None:
                default:
                    return ByLawItemCategory.None;
            }
        }

        public static List<ByLawPropertyOperator> GetPropertyOperators(ByLawItemType itemType)
        {
            var re = new List<ByLawPropertyOperator>();
            switch (itemType)
            {
                case ByLawItemType.Uses:
                    re.Add(ByLawPropertyOperator.Is);
                    re.Add(ByLawPropertyOperator.IsNot);
                    re.Add(ByLawPropertyOperator.AtLeastOne);
                    re.Add(ByLawPropertyOperator.OnlyOneOf);
                    break;
                case ByLawItemType.Density:
                    // Is and AtLeastOne are equivalent here since a building only ever has one density
                    // value, so Is is omitted as redundant. AtLeastOne is listed first (and is thus the
                    // default) since it reads more naturally for a single-value match.
                    re.Add(ByLawPropertyOperator.AtLeastOne);
                    re.Add(ByLawPropertyOperator.IsNot);
                    break;
                case ByLawItemType.Height:
                case ByLawItemType.LotWidth:
                case ByLawItemType.LotSize:
                case ByLawItemType.LotDepth:
                case ByLawItemType.Parking:
                case ByLawItemType.FrontSetback:
                case ByLawItemType.LeftSetback:
                case ByLawItemType.RightSetback:
                case ByLawItemType.RearSetback:
                    re.Add(ByLawPropertyOperator.Is);
                    break;
                case ByLawItemType.AirPollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.NoisePollutionLevel:
                    re.Add(ByLawPropertyOperator.AtMost);
                    break;
                case ByLawItemType.AssetPack:
                    // EvalAssetPack matches if the building belongs to ANY selected pack, i.e. "at least
                    // one", not "exactly one" - AtLeastOne is the operator whose label actually matches
                    // that behavior.
                    re.Add(ByLawPropertyOperator.AtLeastOne);
                    break;
                case ByLawItemType.None:
                default:
                    re.Add(ByLawPropertyOperator.None);
                    break;
            }

            return re;
        }
    }
}
