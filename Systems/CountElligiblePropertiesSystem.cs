using Game;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Collections;
using Unity.Entities;
using ZoningByLaw.BuildingBlocks;

namespace Trejak.ZoningByLaw.Systems
{
    public partial class CountElligiblePropertiesSystem : GameSystemBase
    {
        EntityQuery _bylawsQuery;
        EntityQuery _buildingsQuery;
        private IndexBuildingsSystem _indexBuildingsSystem;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 8;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            _bylawsQuery = GetEntityQuery(
                ComponentType.ReadWrite<ByLawZoneData>()
            );

            _buildingsQuery = GetEntityQuery(
                ComponentType.ReadOnly<BuildingData>(),
                ComponentType.ReadOnly<SpawnableBuildingData>(),
                ComponentType.ReadOnly<BuildingSpawnGroupData>(),
                ComponentType.ReadOnly<PrefabData>()
            );

            _indexBuildingsSystem = World.GetOrCreateSystemManaged<IndexBuildingsSystem>();
            this.RequireForUpdate(_bylawsQuery);
            this.RequireForUpdate(_buildingsQuery);
        }
        protected override void OnUpdate()
        {
            var evalParams = new BuildingBlockSystem.EvaluationParams()
            {
                objectdataLookup = SystemAPI.GetComponentLookup<ObjectData>(true),
                pollutionsThresholds = new()
                {
                    air = IndexBuildingsSystem.airThresholds,
                    ground = IndexBuildingsSystem.groundThresholds,
                    noise = IndexBuildingsSystem.noiseThresholds
                }
            };

            CountElligiblePropertiesJob elligiblePropertiesJob = new CountElligiblePropertiesJob()
            {
                evaluationParams = evalParams,
                buildingByLawPropertiesLookup = _indexBuildingsSystem.GetPropertiesLookup(),
                buildingEntities = _buildingsQuery.ToEntityArray(Allocator.TempJob),
                bylawBlockLookup = SystemAPI.GetComponentLookup<ByLawBlock>(true),
                bylawItemBufferLookup = SystemAPI.GetBufferLookup<ByLawItem>(true),
                prefabDataLookup = SystemAPI.GetComponentLookup<PrefabData>(true),
                buildingDataLookup = GetComponentLookup<BuildingData>(true),
                buildingPropertyDataLookup = GetComponentLookup<BuildingPropertyData>(true),
                objectGeometryLookup = GetComponentLookup<ObjectGeometryData>(true)
            };
            this.Dependency = elligiblePropertiesJob.Schedule(_bylawsQuery, this.Dependency);
        }


        public partial struct CountElligiblePropertiesJob : IJobEntity
        {
            public NativeArray<Entity> buildingEntities;
            public BuildingByLawPropertiesLookup buildingByLawPropertiesLookup;
            public ComponentLookup<BuildingData> buildingDataLookup;
            public ComponentLookup<BuildingPropertyData> buildingPropertyDataLookup;
            public ComponentLookup<ObjectGeometryData> objectGeometryLookup;


            public ComponentLookup<PrefabData> prefabDataLookup;

            public ComponentLookup<ByLawBlock> bylawBlockLookup;            
            public BufferLookup<ByLawItem> bylawItemBufferLookup;

            public BuildingBlockSystem.EvaluationParams evaluationParams;            

            public void Execute(ref ByLawZoneData bylaw, DynamicBuffer<ByLawBlockReference> blockReferences)
            {

                int elligibleCount = 0;
                foreach(Entity buildingEntity in buildingEntities)
                {
                    if (CompliesWithByLaw(bylaw, blockReferences, buildingEntity, objectGeometryLookup[buildingEntity],
                        buildingDataLookup[buildingEntity], buildingPropertyDataLookup[buildingEntity]))
                    {
                        elligibleCount += 1;
                    }
                }
                bylaw.elligibleBuildings = elligibleCount;
            }

            private bool CompliesWithByLaw(ByLawZoneData byLaw, DynamicBuffer<ByLawBlockReference> blockRefBuffer, Entity buildingEntity, ObjectGeometryData objGeomData, 
                BuildingData buildingData, BuildingPropertyData propertyData)
            {
                if (byLaw.deleted) return false;
                bool result = true;
                PrefabData prefabData = prefabDataLookup[buildingEntity];
                BuildingByLawProperties byLawProperties = buildingByLawPropertiesLookup[prefabData];
                foreach (var blockRef in blockRefBuffer)
                {
                    var blockData = bylawBlockLookup[blockRef.block];
                    var itemData = bylawItemBufferLookup[blockRef.block];
                    foreach (var item in itemData)
                    {
                        result = result && BuildingBlockSystem.Evaluate(buildingEntity, buildingData, propertyData, byLawProperties, item, evaluationParams);
                        if (!result)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

        }
    }
}
