﻿using Colossal.Mathematics;
using Game.Buildings;
using Game.Prefabs;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Entities;

namespace ZoningByLaw.BuildingBlocks
{
    public static class BuildingBlockSystem
    {

        public static bool Evaluate(Entity building, BuildingData buildingData, BuildingPropertyData propertyData, BuildingByLawProperties properties, ByLawItem item, ByLawZoneSpawnSystem.EvaluateSpawnAreas job)
        {
            if (!properties.initialized)
            {
                return false;
            }
            switch(item.constraintType)
            {
                case ByLawConstraintType.Count:
                    return EvaluateCount(building, properties, item, job);
                case ByLawConstraintType.Length:
                    return EvaluateLength(building, buildingData, properties, item, job);
                case ByLawConstraintType.MultiSelect:
                    return EvaluateMultiSelect(building, propertyData, properties, item, job);
                case ByLawConstraintType.SingleSelect:
                    return EvaluateSingleSelect(building, properties, item, job);
                case ByLawConstraintType.None:
                default:
                    return false;
            }
        }

        private static bool EvaluateSingleSelect(Entity building, BuildingByLawProperties properties, ByLawItem item, ByLawZoneSpawnSystem.EvaluateSpawnAreas job)
        {
            return false;
        }

        private static bool EvaluateMultiSelect(Entity building, BuildingPropertyData propertyData, BuildingByLawProperties properties, ByLawItem item, ByLawZoneSpawnSystem.EvaluateSpawnAreas job)
        {
            switch(item.byLawItemType)
            {
                case ByLawItemType.Uses:
                    return EvalLandUse(building, properties, item, job);
                default:
                    return false;
            }
        }

        public static bool EvalLandUse(Entity building, BuildingByLawProperties properties, ByLawItem item, ByLawZoneSpawnSystem.EvaluateSpawnAreas job)
        {
            var objectData = job.objectdataLookup[building];

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
            matchCount += (ByLawZoneType.Residential & flag) != 0 && properties.isResidential ? 1 : 0;

            switch (item.propertyOperator)
            {
                case ByLawPropertyOperator.AtLeastOne:
                    return matchCount >= 1;
                case ByLawPropertyOperator.OnlyOneOf:
                    return matchCount == 1;
                case ByLawPropertyOperator.IsNot:
                    return matchCount == 0;
                case ByLawPropertyOperator.Is:
                    return matchCount > 0 && missCount == 0;
            }
            return true;
        }

        public static bool EvalFlags(int flag, int value)
        {
            return (flag & value) != 0;
        }

        private static bool EvaluateLength(Entity building, BuildingData buildingData, BuildingByLawProperties properties, ByLawItem item, ByLawZoneSpawnSystem.EvaluateSpawnAreas job)
        {            
            switch (item.byLawItemType)
            {
                case ByLawItemType.Height:
                    return EvalBounds(item.valueBounds1, properties.buildingHeight);
                case ByLawItemType.LotSize:
                    var lotSize = (buildingData.m_LotSize.x * 8) * (buildingData.m_LotSize.y * 8);
                    return EvalBounds(item.valueBounds1, lotSize);
                case ByLawItemType.LotWidth:
                    return EvalBounds(item.valueBounds1, buildingData.m_LotSize.x * 8);
                    
                default:
                    return false;
            }
        }

        public static bool EvaluateCount(Entity building, BuildingByLawProperties properties, ByLawItem item, ByLawZoneSpawnSystem.EvaluateSpawnAreas job)
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

        public static ByLawConstraintType GetConstraintTypes(ByLawItemType itemType)
        {
            switch(itemType)
            {
                case ByLawItemType.Uses:
                    return ByLawConstraintType.MultiSelect;

                case ByLawItemType.Height:
                case ByLawItemType.LotWidth:
                case ByLawItemType.LotSize:
                case ByLawItemType.Parking:
                case ByLawItemType.FrontSetback:
                case ByLawItemType.LeftSetback:
                case ByLawItemType.RightSetback:
                case ByLawItemType.RearSetback:                
                    return ByLawConstraintType.Length;                                   

                case ByLawItemType.NoisePollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.AirPollutionLevel:
                    return ByLawConstraintType.MultiSelect;

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
                case ByLawItemType.Parking:
                    return ByLawItemCategory.Lot;

                case ByLawItemType.Height:               
                case ByLawItemType.FrontSetback:
                case ByLawItemType.LeftSetback:
                case ByLawItemType.RightSetback:
                case ByLawItemType.RearSetback:
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

    }
}
