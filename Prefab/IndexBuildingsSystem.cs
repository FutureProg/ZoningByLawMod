using Colossal.Collections;
using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Prefabs;
using Game.UI.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Trejak.ZoningByLaw.Prefab
{

    public partial class IndexBuildingsSystem : GameSystemBase
    {
        // to update after BuildingInitializeSystem in the PrefabUpdate phase

        private EntityQuery _buildingsPrefabsQuery;
        private EntityQuery _initQuery;
        private NativeList<BuildingByLawProperties> _properties;
        private JobHandle _propertiesReaders;
        private bool _initialized;


        protected override void OnCreate()
        {
            base.OnCreate();
            _buildingsPrefabsQuery = GetEntityQuery(
                new EntityQueryDesc[] 
                {
                    new EntityQueryDesc
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Updated>(),
                            ComponentType.ReadOnly<PrefabData>()
                        },
                        Any = new ComponentType[]
                        {
                            ComponentType.ReadWrite<BuildingData>(),
                            ComponentType.ReadWrite<BuildingExtensionData>(),
                            ComponentType.ReadWrite<ServiceUpgradeData>(),
                            ComponentType.ReadWrite<SpawnableBuildingData>()
                        }
                    }
                }
            );
            _initQuery = GetEntityQuery(
                new EntityQueryDesc[]
                {
                    new EntityQueryDesc
                    {
                        All = new ComponentType[]
                        {                            
                            ComponentType.ReadOnly<PrefabData>(),
                            ComponentType.ReadWrite<BuildingData>(),
                            ComponentType.ReadWrite<SpawnableBuildingData>()
                        }
                    }
                }
            );
            _properties = new NativeList<BuildingByLawProperties>(20, Allocator.Persistent);
            _propertiesReaders = default;
            _initialized = false;
            Mod.log.Info($"Created IndexBuildingsSystem {_buildingsPrefabsQuery.CalculateEntityCount()} entities.");
            RequireForUpdate(_buildingsPrefabsQuery);
        }

        protected override void OnUpdate()
        {
            UpdateIndex(true);
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            if (!_initialized && _initQuery.CalculateEntityCount() > 0)
            {
                UpdateIndex(false);
            }
        }

        private void UpdateIndex(bool onUpdate)
        {
            this._propertiesReaders.Complete();
            this._propertiesReaders = default;
            
            int processedEnts = 0;


            NativeArray<ArchetypeChunk> chunks;
            if (onUpdate)
            {
                Mod.log.Info($"Running IndexBuildingsSystem for {_buildingsPrefabsQuery.CalculateEntityCount()} entities.");
                chunks = _buildingsPrefabsQuery.ToArchetypeChunkArray(Allocator.Temp);
            } else
            {
                Mod.log.Info($"Initializing IndexBuildingsSystem for {_initQuery.CalculateEntityCount()} entities.");
                chunks = _initQuery.ToArchetypeChunkArray(Allocator.Temp);
            }
            
            EntityTypeHandle entityHandle = GetEntityTypeHandle();
            BufferLookup<SubObject> subObjectLookup = GetBufferLookup<SubObject>(true);
            BufferLookup<SubLane> subLaneBufferLookup = GetBufferLookup<SubLane>(true);
            ComponentLookup<ParkingLaneData> parkingLaneDataLookup = GetComponentLookup<ParkingLaneData>(true);
            ComponentLookup<PrefabData> prefabDataLookup = GetComponentLookup<PrefabData>(true);
            foreach (ArchetypeChunk chunk in chunks)
            {
                var buildingEntities = chunk.GetNativeArray(entityHandle);

                for (int i = 0; i < buildingEntities.Length; i++)
                {
                    Entity buildingEntity = buildingEntities[i];
                    if (!subObjectLookup.TryGetBuffer(buildingEntity, out var buildingSubObjects))
                    {
                        continue;
                    }
                    PrefabData prefabData = prefabDataLookup[buildingEntity];
                    while (prefabData.m_Index >= _properties.Length)
                    {
                        _properties.Add(new BuildingByLawProperties() { initialized = false });
                    }
                    int parkingCount = 0;
                    for (int j = 0; j < buildingSubObjects.Length; j++)
                    {
                        SubObject subObj = buildingSubObjects[j];
                        if (subObj.m_Probability < 100 || (subObj.m_Flags & SubObjectFlags.OnGround) == 0)
                        {
                            continue;
                        }
                        else
                        {
                            parkingCount += AssessSubObject(subObj, subLaneBufferLookup, parkingLaneDataLookup);
                        }
                    }
                    processedEnts++;
                    _properties[prefabData.m_Index] = new BuildingByLawProperties()
                    {
                        initialized = true,
                        parkingCount = parkingCount
                    };
                }
            }
            if (processedEnts > 0)
            {
                _initialized = true;
            }
            Mod.log.Info($"IndexBuildingsSystem created properties for {processedEnts} entities.");
        }

        public void AddPropertiesReader(JobHandle jobHandle)
        {
            this._propertiesReaders = JobHandle.CombineDependencies(this._propertiesReaders, jobHandle);
        }

        public bool TryGetProperties(PrefabData prefabData, out BuildingByLawProperties properties)
        {
            if (prefabData.m_Index >= _properties.Length)
            {
                properties = new BuildingByLawProperties() { initialized = false };
                return false;
            }
            properties = _properties[prefabData.m_Index];
            return properties.initialized;
        }

        public BuildingByLawPropertiesLookup GetPropertiesLookup()
        {
            return new BuildingByLawPropertiesLookup(this._properties.AsArray());
        }

        public int AssessSubObject(SubObject subObj, BufferLookup<SubLane> subLaneBufferLookup, ComponentLookup<ParkingLaneData> parkingLaneDataLookup)
        {
            int re = 0;
            if (subLaneBufferLookup.TryGetBuffer(subObj.m_Prefab, out var subLanes))
            {
                foreach (var lane in subLanes)
                {
                    if (parkingLaneDataLookup.HasComponent(lane.m_Prefab))
                    {
                        re++;
                    }
                }
            }
            return re;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _propertiesReaders.Complete();
            _properties.Dispose();            
        }

    }

    public struct BuildingByLawPropertiesLookup
    {

        private NativeArray<BuildingByLawProperties> _properties;

        public BuildingByLawPropertiesLookup(NativeArray<BuildingByLawProperties> properties)
        {
            this._properties = properties;
        }

        public BuildingByLawProperties this[int prefabIndex]
        {
            get
            {
                return this._properties[prefabIndex];
            }
        }

        public BuildingByLawProperties this[PrefabData prefabData]
        {
            get
            {
                return this[prefabData.m_Index];
            }
        }

    }

    public struct BuildingByLawProperties
    {
        public bool initialized;
        public int parkingCount;
    }
}
