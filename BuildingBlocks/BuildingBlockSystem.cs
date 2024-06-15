using Colossal.Mathematics;
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
                    return EvalLandUse(building, propertyData, item, job);
                default:
                    return false;
            }
        }

        public static bool EvalLandUse(Entity building, BuildingPropertyData propertyData, ByLawItem item, ByLawZoneSpawnSystem.EvaluateSpawnAreas job)
        {
            var objectData = job.objectdataLookup[building];            
            var archetypeComponents = objectData.m_Archetype.GetComponentTypes();
            bool isOffice = archetypeComponents.Contains(ComponentType.ReadOnly<OfficeProperty>());//(zoneData.m_ZoneFlags & ZoneFlags.Office) != 0;
            bool isIndustry = archetypeComponents.Contains(ComponentType.ReadOnly<IndustrialProperty>()); //(zoneData.m_AreaType & Game.Zones.AreaType.Industrial) != 0;                
            bool isExtractor = archetypeComponents.Contains(ComponentType.ReadOnly<ExtractorProperty>());
            bool isResidential = propertyData.m_ResidentialProperties > 0;
            bool isCommercial = archetypeComponents.Contains(ComponentType.ReadOnly<CommercialProperty>());

            var flag = (ByLawZoneType) item.valueByteFlag;
            if (isExtractor) // extractors only function when plopped down, so won't be spawning them
            {
                return false;
            }
            if ((ByLawZoneType.Residential & flag) == 0 && isResidential)
            {
                return false;
            }
            if ((ByLawZoneType.Office & flag) == 0 && isOffice)
            {
                return false;
            }
            if ((ByLawZoneType.Commercial & flag) == 0 && isCommercial)
            {
                return false;
            }
            if ((ByLawZoneType.Industrial & flag) == 0 && isIndustry)
            {
                return false;
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
