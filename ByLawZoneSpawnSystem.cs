using Colossal.Entities;
using Game;
using Game.Areas;
using Game.City;
using Game.Common;
using Game.Economy;
using Game.Events;
using Game.Net;
using Game.Prefabs;
using Game.Rendering;
using Game.Simulation;
using Game.Tools;
using Game.Zones;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Trejak.ZoningByLaw.Prefab;
using static Game.Simulation.ServiceCoverageSystem;
using BuildingData = Game.Prefabs.BuildingData;
using Game.Buildings;
using System.Security.AccessControl;
using Trejak.ZoningByLaw.BuildingBlocks;
using ZoningByLaw.BuildingBlocks;

namespace Trejak.ZoningByLaw
{

    //TODO: get all vacant lots
    // -  get the Zone prefab entity from the ZoneSystem.GetPrefab(ZoneType) method
    // - if the zone entity has the ByLawZoneFlag component, execute our spawning logic
    public partial class ByLawZoneSpawnSystem : GameSystemBase
    {

        EntityQuery _vacantLotsQuery;
        EntityQuery _buildingsQuery;
        EntityQuery _processQuery;
        EntityQuery _buildingConfigQuery;
        EntityArchetype _definitionArchetype;

        ZoneSystem _zoneSystem;
        ZoneSpawnSystem _zoneSpawnSystem;
        ResidentialDemandSystem _residentialDemandSystem;
        CommercialDemandSystem _commercialDemandSystem;
        IndustrialDemandSystem _industrialDemandSystem;
        GroundPollutionSystem _pollutionSystem;
        TerrainSystem _terrainSystem;
        Game.Zones.SearchSystem _searchSystem;
        ResourceSystem _resourceSystem;
        CityConfigurationSystem _cityConfigurationSystem;
        EndFrameBarrier _endFrameBarrier;

        IndexBuildingsSystem _indexBuildingsSystem;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 16;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            _vacantLotsQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Block>(),
                        ComponentType.ReadOnly<Owner>(),
                        ComponentType.ReadOnly<CurvePosition>(),
                        ComponentType.ReadOnly<VacantLot>()
                    },
                    Any = new ComponentType[0],
                    None = new ComponentType[]
                    {
                        ComponentType.ReadWrite<Temp>(),
                        ComponentType.ReadWrite<Deleted>()
                    }
                }
            });
            _buildingsQuery = this.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<BuildingData>(),
                ComponentType.ReadOnly<SpawnableBuildingData>(),
                ComponentType.ReadOnly<BuildingSpawnGroupData>(),
                ComponentType.ReadOnly<PrefabData>()
            });
            _definitionArchetype = EntityManager.CreateArchetype(new ComponentType[]
            {
                ComponentType.ReadWrite<CreationDefinition>(),
                ComponentType.ReadWrite<ObjectDefinition>(),
                ComponentType.ReadWrite<Updated>(),
                ComponentType.ReadWrite<Deleted>()
            });
            _processQuery = this.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<IndustrialProcessData>()
            });
            _buildingConfigQuery = this.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<BuildingConfigurationData>()
            });

            _zoneSystem = World.GetOrCreateSystemManaged<ZoneSystem>();
            _residentialDemandSystem = base.World.GetOrCreateSystemManaged<ResidentialDemandSystem>();
            _commercialDemandSystem = base.World.GetOrCreateSystemManaged<CommercialDemandSystem>();
            _industrialDemandSystem = base.World.GetOrCreateSystemManaged<IndustrialDemandSystem>();
            _pollutionSystem = base.World.GetOrCreateSystemManaged<GroundPollutionSystem>();
            _terrainSystem = base.World.GetOrCreateSystemManaged<TerrainSystem>();
            _searchSystem = base.World.GetOrCreateSystemManaged<Game.Zones.SearchSystem>();
            _resourceSystem = base.World.GetOrCreateSystemManaged<ResourceSystem>();
            _cityConfigurationSystem = base.World.GetOrCreateSystemManaged<CityConfigurationSystem>();
            _endFrameBarrier = base.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            _zoneSpawnSystem = World.GetOrCreateSystemManaged<ZoneSpawnSystem>();

            _indexBuildingsSystem = World.GetOrCreateSystemManaged<IndexBuildingsSystem>();

            this.RequireForUpdate(this._vacantLotsQuery);
            this.RequireForUpdate(_buildingsQuery);
        }

        private bool CheckDemand(ref Unity.Mathematics.Random random, int demand)
        {
            return random.NextInt(5000) < demand * demand;
        }

        // Token: 0x06006811 RID: 26641 RVA: 0x0040B3FB File Offset: 0x004095FB
        private bool CheckStorageDemand(ref Unity.Mathematics.Random random, int demand)
        {
            return demand > 0;
        }

        protected override void OnUpdate()
        {            
            Unity.Mathematics.Random random = RandomSeed.Next().GetRandom(0);
            bool spawnResidential = _zoneSpawnSystem.debugFastSpawn || this.CheckDemand(ref random, this._residentialDemandSystem.buildingDemand.x + this._residentialDemandSystem.buildingDemand.y + this._residentialDemandSystem.buildingDemand.z);
            bool spawnCommercial = _zoneSpawnSystem.debugFastSpawn || this.CheckDemand(ref random, this._commercialDemandSystem.buildingDemand);
            bool spawnEmployment = _zoneSpawnSystem.debugFastSpawn || this.CheckDemand(ref random, this._industrialDemandSystem.industrialBuildingDemand + this._industrialDemandSystem.officeBuildingDemand);
            bool spawnStorage = _zoneSpawnSystem.debugFastSpawn || this.CheckStorageDemand(ref random, this._industrialDemandSystem.storageBuildingDemand);
            NativeQueue<ZoneSpawnSystem.SpawnLocation> residentialQ = new NativeQueue<ZoneSpawnSystem.SpawnLocation>(Allocator.TempJob);
            NativeQueue<ZoneSpawnSystem.SpawnLocation> commercialQ = new NativeQueue<ZoneSpawnSystem.SpawnLocation>(Allocator.TempJob);
            NativeQueue<ZoneSpawnSystem.SpawnLocation> industrialQ = new NativeQueue<ZoneSpawnSystem.SpawnLocation>(Allocator.TempJob);

            EvaluateSpawnAreas evaluateSpawnAreas = new()
            {
                blockHandle = GetComponentTypeHandle<Block>(),
                blockLookup = GetComponentLookup<Block>(),
                buildingChunks = _buildingsQuery.ToArchetypeChunkListAsync(base.World.UpdateAllocator.ToAllocator, out var buildingsQueryJob),
                buildingDataHandle = GetComponentTypeHandle<BuildingData>(true),
                buildingPropertyDataHandle = GetComponentTypeHandle<BuildingPropertyData>(true),
                byLawZoneDataLookup = SystemAPI.GetComponentLookup<ByLawZoneData>(true),
                curvePositionHandle = GetComponentTypeHandle<CurvePosition>(true),
                entityHandle = GetEntityTypeHandle(),
                groundPollutionMap = _pollutionSystem.GetMap(true, out var groundPollutionMapJob),
                industrialDemands = _industrialDemandSystem.GetBuildingDemands(out var industrialDemandJob),
                industrialProcesses = _processQuery.ToComponentDataListAsync<IndustrialProcessData>(World.UpdateAllocator.ToAllocator, out var industrialProcessJob),
                landValueLookup = GetComponentLookup<LandValue>(true),
                minDemand = _zoneSpawnSystem.debugFastSpawn ? 0 : 1,
                objGeomDataHandle = GetComponentTypeHandle<ObjectGeometryData>(true),
                ownerHandle = GetComponentTypeHandle<Owner>(true),
                processEstimatesLookup = GetBufferLookup<ProcessEstimate>(true),
                randomSeed = RandomSeed.Next(),
                resourceAvailabilityLookup = GetBufferLookup<ResourceAvailability>(true),
                resourceDataLookup = GetComponentLookup<ResourceData>(true),
                resourcePrefabs = _resourceSystem.GetPrefabs(),
                spawnableBuildingDataHandle = GetComponentTypeHandle<SpawnableBuildingData>(true),
                storageDemands = _industrialDemandSystem.GetStorageBuildingDemands(out var storageBuildingDemandJob),
                vacantLotHandle = GetBufferTypeHandle<VacantLot>(true),
                warehouseHandle = GetComponentTypeHandle<WarehouseData>(true),
                zoneDataLookup = GetComponentLookup<ZoneData>(true),
                zonePrefabs = _zoneSystem.GetPrefabs(),
                zonePreferenceData = SystemAPI.GetSingleton<ZonePreferenceData>(),
                commercialQ = commercialQ.AsParallelWriter(),
                residentialQ = residentialQ.AsParallelWriter(),
                industrialQ = industrialQ.AsParallelWriter(),
                entityStorageInfoLookup = GetEntityStorageInfoLookup(),
                objectdataLookup = SystemAPI.GetComponentLookup<ObjectData>(true),
                commercialDemands = _commercialDemandSystem.GetBuildingDemands(out var commercialDemandJob),
                residentialDemands = _residentialDemandSystem.buildingDemand,
                buildingByLawPropertiesLookup = _indexBuildingsSystem.GetPropertiesLookup(),
                prefabDataLookup = GetComponentLookup<PrefabData>(true),
                bylawBlockLookup = GetComponentLookup<ByLawBlock>(true),
                bylawBlockRefLookup = GetBufferLookup<ByLawBlockReference>(true),
                bylawItemBufferLookup = GetBufferLookup<ByLawItem>(true)
            };            
            ZoneSpawnSystem.SpawnBuildingJob spawnBuildingJob = new()
            {
                m_BlockData = GetComponentLookup<Block>(true),
                m_BuildingConfigurationData = _buildingConfigQuery.GetSingleton<BuildingConfigurationData>(),
                m_Cells = GetBufferLookup<Cell>(true),
                m_CommandBuffer = _endFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_Commercial = commercialQ,
                m_DefinitionArchetype = _definitionArchetype,
                m_Industrial = industrialQ,
                m_LefthandTraffic = _cityConfigurationSystem.leftHandTraffic,
                m_PrefabAreaGeometryData = GetComponentLookup<AreaGeometryData>(true),
                m_PrefabBuildingData = GetComponentLookup<BuildingData>(true),
                m_PrefabNetGeometryData = GetComponentLookup<NetGeometryData>(true),
                m_PrefabObjectGeometryData = GetComponentLookup<ObjectGeometryData>(true),
                m_PrefabPlaceableObjectData = GetComponentLookup<PlaceableObjectData>(true),
                m_PrefabPlaceholderElements = GetBufferLookup<PlaceholderObjectElement>(true),
                m_PrefabRefData = GetComponentLookup<PrefabRef>(true),
                m_PrefabSpawnableObjectData = GetComponentLookup<SpawnableObjectData>(true),
                m_PrefabSubAreaNodes = GetBufferLookup<Game.Prefabs.SubAreaNode>(true),
                m_PrefabSubAreas = GetBufferLookup<Game.Prefabs.SubArea>(true),
                m_PrefabSubNets = GetBufferLookup<Game.Prefabs.SubNet>(true),
                m_RandomSeed = RandomSeed.Next(),
                m_Residential = residentialQ,
                m_TerrainHeightData = _terrainSystem.GetHeightData(false),
                m_TransformData = GetComponentLookup<Game.Objects.Transform>(true),
                m_ValidAreaData = GetComponentLookup<ValidArea>(true),
                m_ZoneSearchTree = _searchSystem.GetSearchTree(true, out var searchTreeJob),               
            };

            var evaluationHandle = evaluateSpawnAreas.ScheduleParallel(this._vacantLotsQuery, JobUtils.CombineDependencies(buildingsQueryJob, groundPollutionMapJob,
                groundPollutionMapJob, industrialDemandJob, industrialProcessJob, this.Dependency, storageBuildingDemandJob, commercialDemandJob));
            var spawnBuildingHandle = spawnBuildingJob.Schedule(3, 1, JobHandle.CombineDependencies(evaluationHandle, searchTreeJob));
            
            _resourceSystem.AddPrefabsReader(evaluationHandle);
            _pollutionSystem.AddReader(evaluationHandle);
            _commercialDemandSystem.AddReader(evaluationHandle);
            _industrialDemandSystem.AddReader(evaluationHandle);
            _indexBuildingsSystem.AddPropertiesReader(evaluationHandle);

            residentialQ.Dispose(spawnBuildingHandle);
            commercialQ.Dispose(spawnBuildingHandle);
            industrialQ.Dispose(spawnBuildingHandle);

            _zoneSystem.AddPrefabsReader(evaluationHandle);
            _terrainSystem.AddCPUHeightReader(spawnBuildingHandle);
            _endFrameBarrier.AddJobHandleForProducer(spawnBuildingHandle);
            _searchSystem.AddSearchTreeReader(spawnBuildingHandle);            

            this.Dependency = spawnBuildingHandle;
        }

        [BurstCompile]
        public struct EvaluateSpawnAreas : IJobChunk
        {
            public RandomSeed randomSeed;
            public ZonePrefabs zonePrefabs;
            public NativeList<IndustrialProcessData> industrialProcesses;
            public NativeArray<GroundPollution> groundPollutionMap;
            public NativeList<ArchetypeChunk> buildingChunks;
            public ZonePreferenceData zonePreferenceData;
            public int3 residentialDemands;
            public NativeArray<int> commercialDemands;
            public NativeArray<int> storageDemands;
            public NativeArray<int> industrialDemands;
            public ResourcePrefabs resourcePrefabs;
            public NativeQueue<ZoneSpawnSystem.SpawnLocation>.ParallelWriter residentialQ;
            public NativeQueue<ZoneSpawnSystem.SpawnLocation>.ParallelWriter commercialQ;
            public NativeQueue<ZoneSpawnSystem.SpawnLocation>.ParallelWriter industrialQ;
            public EntityStorageInfoLookup entityStorageInfoLookup;
            public int minDemand;                        

            public EntityTypeHandle entityHandle;
            public ComponentTypeHandle<Owner> ownerHandle;
            public ComponentTypeHandle<CurvePosition> curvePositionHandle;
            public ComponentTypeHandle<Block> blockHandle;
            public BufferTypeHandle<VacantLot> vacantLotHandle;
            public ComponentTypeHandle<BuildingData> buildingDataHandle;
            public ComponentTypeHandle<SpawnableBuildingData> spawnableBuildingDataHandle;
            public ComponentTypeHandle<BuildingPropertyData> buildingPropertyDataHandle;
            public ComponentTypeHandle<ObjectGeometryData> objGeomDataHandle;
            public ComponentTypeHandle<WarehouseData> warehouseHandle;

            public BufferLookup<ProcessEstimate> processEstimatesLookup;
            public BufferLookup<ResourceAvailability> resourceAvailabilityLookup;
            public ComponentLookup<ZoneData> zoneDataLookup;
            public ComponentLookup<ByLawZoneData> byLawZoneDataLookup;
            public ComponentLookup<Block> blockLookup;
            public ComponentLookup<LandValue> landValueLookup;
            public ComponentLookup<ResourceData> resourceDataLookup;
            public ComponentLookup<ObjectData> objectdataLookup;
            public ComponentLookup<PrefabData> prefabDataLookup;
            public BuildingByLawPropertiesLookup buildingByLawPropertiesLookup;

            public ComponentLookup<ByLawBlock> bylawBlockLookup;
            public BufferLookup<ByLawBlockReference> bylawBlockRefLookup;
            public BufferLookup<ByLawItem> bylawItemBufferLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                Unity.Mathematics.Random random = this.randomSeed.GetRandom(unfilteredChunkIndex);
                ZoneSpawnSystem.SpawnLocation spawnLocation = default(ZoneSpawnSystem.SpawnLocation);

                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityHandle);
                BufferAccessor<VacantLot> vacantLotsAccessor = chunk.GetBufferAccessor<VacantLot>(ref this.vacantLotHandle);

                if (vacantLotsAccessor.Length > 0)
                {
                    //Mod.log.Info("Bylaw Zone Flag Lookup below: ");
                    //Mod.log.Info(byLawZoneFlagLookup);
                    NativeArray<Owner> owners = chunk.GetNativeArray<Owner>(ref this.ownerHandle);
                    NativeArray<CurvePosition> curvePositions = chunk.GetNativeArray<CurvePosition>(ref this.curvePositionHandle);
                    NativeArray<Block> blocks = chunk.GetNativeArray<Block>(ref this.blockHandle);
                    for (int i = 0; i < entities.Length; i++)
                    {                        
                        Entity entity = entities[i];
                        var vacantLots = vacantLotsAccessor[i];
                        var owner = owners[i];
                        var curvePos = curvePositions[i];
                        var block = blocks[i];
                        for (int j = 0; j < vacantLots.Length; j++)
                        {
                            var vacantLot = vacantLots[j];
                            var zonePrefab = this.zonePrefabs[vacantLot.m_Type];                                    
                            if (!byLawZoneDataLookup.TryGetComponent(zonePrefab, out var byLawData) || byLawData.deleted || !bylawBlockRefLookup.TryGetBuffer(zonePrefab, out var byLawBlockRef))
                            {
                                continue;
                            }                            
                            ZoneData zoneData = this.zoneDataLookup[zonePrefab];
                            DynamicBuffer<ProcessEstimate> estimates = this.processEstimatesLookup[zonePrefab];

                            // normally is a switch here for the type of area (residential, industry, commercial)
                            float curvePosScalar = this.CalculateCurvePos(curvePos, vacantLot, block);
                            //TODO: Allow storage to be spawned
                            this.TryAddLot(byLawData, byLawBlockRef, ref spawnLocation, ref random, owner.m_Owner, curvePosScalar, entity, vacantLot.m_Area, vacantLot.m_Flags, (int)vacantLot.m_Height, zoneData, estimates, this.industrialProcesses, true, false);
                        }
                    }
                }
                if (spawnLocation.m_Priority != 0f)
                {
                    switch (spawnLocation.m_AreaType)
                    {
                        case Game.Zones.AreaType.Commercial:
                            this.commercialQ.Enqueue(spawnLocation);
                            break;
                        case Game.Zones.AreaType.Industrial:
                            this.industrialQ.Enqueue(spawnLocation);
                            break;
                        case Game.Zones.AreaType.Residential:
                            this.residentialQ.Enqueue(spawnLocation);
                            break;
                        case Game.Zones.AreaType.None:
                        default:
                            break;
                    }
                }
            }

            private float CalculateCurvePos(CurvePosition curvePosition, VacantLot lot, Block block)
            {
                float s = math.saturate((float)(lot.m_Area.x + lot.m_Area.y) * 0.5f / (float)block.m_Size.x);
                return math.lerp(curvePosition.m_CurvePosition.x, curvePosition.m_CurvePosition.y, s);
            }

            public void TryAddLot(ByLawZoneData byLawData, DynamicBuffer<ByLawBlockReference> byLawBlockRef, ref ZoneSpawnSystem.SpawnLocation bestLocation, ref Unity.Mathematics.Random random, Entity road, float curvePos,
                Entity entity, int4 area, LotFlags flags, int height, ZoneData zoneData, DynamicBuffer<ProcessEstimate> estimates,
                NativeList<IndustrialProcessData> processes, bool normal = true, bool storage = false)
            {
                if (this.resourceAvailabilityLookup.HasBuffer(road))
                {
                    if ((zoneData.m_ZoneFlags & ZoneFlags.SupportLeftCorner) == (ZoneFlags)0)
                    {
                        flags &= ~LotFlags.CornerLeft;
                    }
                    if ((zoneData.m_ZoneFlags & ZoneFlags.SupportRightCorner) == (ZoneFlags)0)
                    {
                        flags &= ~LotFlags.CornerRight;
                    }

                    ZoneSpawnSystem.SpawnLocation spawnLocation = default(ZoneSpawnSystem.SpawnLocation);
                    spawnLocation.m_Entity = entity;
                    spawnLocation.m_LotArea = area;
                    spawnLocation.m_ZoneType = zoneData.m_ZoneType;
                    spawnLocation.m_AreaType = zoneData.m_AreaType;
                    spawnLocation.m_LotFlags = flags;
                    bool office = zoneData.m_AreaType == Game.Zones.AreaType.Industrial && estimates.Length == 0;
                    DynamicBuffer<ResourceAvailability> availabilities = this.resourceAvailabilityLookup[road];
                    if (!this.blockLookup.HasComponent(spawnLocation.m_Entity))
                    {
                        return;
                    }
                    float3 position = ZoneUtils.GetPosition(this.blockLookup[spawnLocation.m_Entity], spawnLocation.m_LotArea.xz, spawnLocation.m_LotArea.yw);
                    bool extractor = false;
                    GroundPollution pollution = GroundPollutionSystem.GetPollution(position, this.groundPollutionMap);
                    float2 pollution2 = new float2((float)pollution.m_Pollution, (float)(pollution.m_Pollution - pollution.m_Previous));
                    float landValue = this.landValueLookup[road].m_LandValue;
                    float maxHeight = (float)height - position.y;
                    if (this.SelectBuilding(byLawData, byLawBlockRef, ref spawnLocation, ref random, availabilities, zoneData, curvePos, pollution2, landValue, maxHeight, estimates, processes, normal, storage, extractor, office)
                        && spawnLocation.m_Priority > bestLocation.m_Priority)
                    {
                        bestLocation = spawnLocation;
                    }
                }                
            }

            private bool CompliesWithByLaw(ByLawZoneData byLaw, DynamicBuffer<ByLawBlockReference> blockRefBuffer, Entity buildingEntity, ObjectGeometryData objGeomData, 
                ZoneData zoneData, BuildingData buildingData, BuildingPropertyData propertyData, ObjectData objectData)
            {
                if (byLaw.deleted) return false;
                bool result = true;
                PrefabData prefabData = prefabDataLookup[buildingEntity];
                BuildingByLawProperties byLawProperties = buildingByLawPropertiesLookup[prefabData];
                foreach (var blockRef in blockRefBuffer)
                {
                    var blockData = bylawBlockLookup[blockRef.block];
                    var itemData = bylawItemBufferLookup[blockRef.block];
                    foreach(var item in itemData)
                    {
                        result = result && BuildingBlockSystem.Evaluate(buildingEntity, buildingData, propertyData, byLawProperties, item, this);
                        if (!result)
                        {
                            return false;
                        }
                    }                    
                }

                //TODO: finish this tomorrow
                return true;
            }

            [Obsolete]
            private bool CompliesWithByLaw(ByLawZoneData byLaw, Entity buildingEntity, ObjectGeometryData objGeomData, ZoneData zoneData, BuildingData buildingData, BuildingPropertyData propertyData, ObjectData objectData)
            {                
                bool re = true;
                PrefabData prefabData = prefabDataLookup[buildingEntity];
                BuildingByLawProperties byLawProperties = buildingByLawPropertiesLookup[prefabData];
                bool hasByLawProperties = byLawProperties.initialized;

                //// Height
                re = re && (byLaw.height.min >= 0 ? objGeomData.m_Size.y >= byLaw.height.min : true);
                re = re && (byLaw.height.max >= 0 ? objGeomData.m_Size.y <= byLaw.height.max : true);
                //// Lot Frontage
                re = re && (byLaw.frontage.min >= 0 ? buildingData.m_LotSize.x * 8 >= byLaw.frontage.min : true);
                re = re && (byLaw.frontage.max >= 0 ? buildingData.m_LotSize.x * 8 <= byLaw.frontage.max : true);
                //// Lot Size
                var lotSize = (buildingData.m_LotSize.x * 8) * (buildingData.m_LotSize.y * 8);
                re = re && (byLaw.lotSize.min >= 0 ? lotSize >= byLaw.lotSize.min : true);
                re = re && (byLaw.lotSize.max >= 0 ? lotSize <= byLaw.lotSize.max : true);
                //// Parking
                if (byLaw.parking.min > 0)
                {
                    re = re && hasByLawProperties && byLawProperties.parkingCount >= byLaw.parking.min;
                }
                if (byLaw.parking.max >= 0)
                {
                    re = re && hasByLawProperties && byLawProperties.parkingCount <= byLaw.parking.max;
                }
                if (!re) return re;

                // Permitted Uses
                var archetypeComponents = objectData.m_Archetype.GetComponentTypes();
                bool isOffice = archetypeComponents.Contains(ComponentType.ReadOnly<OfficeProperty>());//(zoneData.m_ZoneFlags & ZoneFlags.Office) != 0;
                bool isIndustry = archetypeComponents.Contains(ComponentType.ReadOnly<IndustrialProperty>()); //(zoneData.m_AreaType & Game.Zones.AreaType.Industrial) != 0;                
                bool isExtractor = archetypeComponents.Contains(ComponentType.ReadOnly<ExtractorProperty>());
                bool isResidential = propertyData.m_ResidentialProperties > 0;
                bool isCommercial = archetypeComponents.Contains(ComponentType.ReadOnly<CommercialProperty>());
                if (isExtractor) // extractors only function when plopped down, so won't be spawning them
                {
                    return false;
                }
                if ((ByLawZoneType.Residential & byLaw.zoneType) == 0 && isResidential)
                {
                    return false;
                }
                if ((ByLawZoneType.Office & byLaw.zoneType) == 0 && isOffice) {
                    return false;
                }
                if ((ByLawZoneType.Commercial & byLaw.zoneType) == 0 && isCommercial)
                {
                    return false;
                }
                if ((ByLawZoneType.Industrial & byLaw.zoneType) == 0 && isIndustry)
                {
                    return false;
                }
                //re = re && (ByLawZoneType.Office & byLaw.zoneType) == 0 ? !isOffice : true;
                //re = re && (ByLawZoneType.Residential & byLaw.zoneType) == 0? !isResidential : true;
                //re = re && (ByLawZoneType.Commercial & byLaw.zoneType) == 0 ? !isCommercial : true;
                //re = re && (ByLawZoneType.Industrial & byLaw.zoneType) == 0 ? !isIndustry : true;
                return re;
            }

            private bool SelectBuilding(ByLawZoneData byLawData, DynamicBuffer<ByLawBlockReference> byLawBlockRef, ref ZoneSpawnSystem.SpawnLocation location, ref Unity.Mathematics.Random random, DynamicBuffer<ResourceAvailability> availabilities,
                ZoneData zoneData, float curvePos, float2 pollution, float landValue, float maxHeight, DynamicBuffer<ProcessEstimate> estimates,
                NativeList<IndustrialProcessData> processes, bool normal, bool storage, bool extractor, bool office = false)
            {
                int2 maxLotSize = location.m_LotArea.yw - location.m_LotArea.xz;
                BuildingData buildingData = default(BuildingData);
                bool2 lhs = new bool2((location.m_LotFlags & LotFlags.CornerLeft) > (LotFlags)0, (location.m_LotFlags & LotFlags.CornerRight) > (LotFlags)0);
                bool supportsNarrow = (zoneData.m_ZoneFlags & ZoneFlags.SupportNarrow) == (ZoneFlags)0;

                var chunkIdx = random.NextInt(0, buildingChunks.Length);
                ArchetypeChunk buildingChunk = this.buildingChunks[chunkIdx];
                bool isStorage = buildingChunk.Has<WarehouseData>(ref this.warehouseHandle);
                var buildingEntities = buildingChunk.GetNativeArray(this.entityHandle);
                var buildingDataArr = buildingChunk.GetNativeArray<BuildingData>(ref this.buildingDataHandle);
                var spawnableDataArr = buildingChunk.GetNativeArray<SpawnableBuildingData>(ref this.spawnableBuildingDataHandle);
                var buildingPropertyDataArr = buildingChunk.GetNativeArray<BuildingPropertyData>(ref this.buildingPropertyDataHandle);
                var objGeomDataArr = buildingChunk.GetNativeArray<ObjectGeometryData>(ref this.objGeomDataHandle);
                for (int i = 0; i < spawnableDataArr.Length; i++)
                {
                    var spawnableData = spawnableDataArr[i];
                    if (spawnableData.m_Level == 1)
                    {
                        BuildingData subjBuildingData = buildingDataArr[i];
                        int2 lotSize = subjBuildingData.m_LotSize;
                        bool2 rhs = new bool2((subjBuildingData.m_Flags & Game.Prefabs.BuildingFlags.LeftAccess) > (Game.Prefabs.BuildingFlags)0U, (subjBuildingData.m_Flags & Game.Prefabs.BuildingFlags.RightAccess) > (Game.Prefabs.BuildingFlags)0U);
                        float bldgHeight = objGeomDataArr[i].m_Size.y;
                        if (math.all(lotSize <= maxLotSize) && bldgHeight <= maxHeight 
                            && CompliesWithByLaw(byLawData, byLawBlockRef, buildingEntities[i], objGeomDataArr[i], zoneData, subjBuildingData, buildingPropertyDataArr[i], objectdataLookup[buildingEntities[i]]))
                        {
                            BuildingPropertyData buildingPropertyData = buildingPropertyDataArr[i];
                            Game.Zones.AreaType evalAreaType = zoneDataLookup[spawnableData.m_ZonePrefab].m_AreaType; 
                            
                            int num = this.EvaluateDemandAndAvailability(evalAreaType, buildingPropertyData, lotSize.x * lotSize.y, isStorage);
                            if (num >= this.minDemand || extractor)
                            {
                                int2 int2 = math.select(maxLotSize - lotSize, 0, lotSize == maxLotSize - 1);
                                float num2 = (float)(lotSize.x * lotSize.y) * random.NextFloat(1f, 1.05f);
                                num2 += (float)(int2.x * lotSize.y) * random.NextFloat(0.95f, 1f);
                                num2 += (float)(maxLotSize.x * int2.y) * random.NextFloat(0.55f, 0.6f);
                                num2 /= (float)(maxLotSize.x * maxLotSize.y);
                                num2 *= (float)(num + 1);
                                num2 *= math.csum(math.select(0.01f, 0.5f, lhs == rhs));
                                if (!extractor)
                                {
                                    float num3 = landValue;
                                    float num4;
                                    if (evalAreaType == Game.Zones.AreaType.Residential)
                                    {
                                        num4 = ((buildingPropertyData.m_ResidentialProperties == 1) ? 2f : ((float)buildingPropertyData.CountProperties()));
                                        lotSize.x = math.select(lotSize.x, maxLotSize.x, lotSize.x == maxLotSize.x - 1 && supportsNarrow);
                                        num3 *= (float)(lotSize.x * maxLotSize.y);
                                    }
                                    else
                                    {
                                        num4 = buildingPropertyData.m_SpaceMultiplier;
                                    }
                                    float num5 = ZoneEvaluationUtils.GetScore(evalAreaType, office, availabilities, curvePos, ref this.zonePreferenceData,
                                        isStorage, isStorage ? this.storageDemands : this.industrialDemands,
                                        buildingPropertyData, pollution, num3 / num4, estimates, processes, this.resourcePrefabs, ref this.resourceDataLookup);
                                    num5 = math.select(num5, math.max(0f, num5) + 1f, this.minDemand == 0);
                                    num2 *= num5;
                                }
                                if (num2 > location.m_Priority)
                                {
                                    location.m_Building = buildingEntities[i];
                                    location.m_AreaType = evalAreaType;
                                    buildingData = subjBuildingData;
                                    location.m_Priority = num2;
                                }
                            }
                        }
                    }
                }
                
                if (location.m_Building != Entity.Null)
                {
                    bool shouldBeRightAccessOnly = (buildingData.m_Flags & Game.Prefabs.BuildingFlags.LeftAccess) == (Game.Prefabs.BuildingFlags)0U && ((buildingData.m_Flags & Game.Prefabs.BuildingFlags.RightAccess) != (Game.Prefabs.BuildingFlags)0U || random.NextBool());
                    if (shouldBeRightAccessOnly)
                    {
                        location.m_LotArea.x = location.m_LotArea.y - buildingData.m_LotSize.x;
                        location.m_LotArea.w = location.m_LotArea.z + buildingData.m_LotSize.y;
                    }
                    else
                    {
                        location.m_LotArea.yw = location.m_LotArea.xz + buildingData.m_LotSize;
                    }
                    return true;
                }
                return false;
            }
            private int EvaluateDemandAndAvailability(Game.Zones.AreaType areaType, BuildingPropertyData propertyData, int lotSize, bool storage = false)
            {
                switch (areaType)
                {
                    case Game.Zones.AreaType.Residential:
                        if (propertyData.m_ResidentialProperties == 1)
                        {
                            return this.residentialDemands.z;
                        }
                        if ((float)propertyData.m_ResidentialProperties / (propertyData.m_SpaceMultiplier * (float)lotSize) < 1f)
                        {
                            return this.residentialDemands.y;
                        }
                        return this.residentialDemands.x;
                    case Game.Zones.AreaType.Commercial:
                        {
                            int num = 0;
                            ResourceIterator iterator = ResourceIterator.GetIterator();
                            while (iterator.Next())
                            {
                                if ((propertyData.m_AllowedSold & iterator.resource) != Resource.NoResource)
                                {
                                    num += this.commercialDemands[EconomyUtils.GetResourceIndex(iterator.resource)];
                                }
                            }
                            return num;
                        }
                    case Game.Zones.AreaType.Industrial:
                        {
                            int num2 = 0;
                            ResourceIterator iterator2 = ResourceIterator.GetIterator();
                            while (iterator2.Next())
                            {
                                if (storage)
                                {
                                    if ((propertyData.m_AllowedStored & iterator2.resource) != Resource.NoResource)
                                    {
                                        num2 += this.storageDemands[EconomyUtils.GetResourceIndex(iterator2.resource)];
                                    }
                                }
                                else if ((propertyData.m_AllowedManufactured & iterator2.resource) != Resource.NoResource)
                                {
                                    num2 += this.industrialDemands[EconomyUtils.GetResourceIndex(iterator2.resource)];
                                }
                            }
                            return num2;
                        }
                    default:
                        return 0;
                }                
            }
        }
    }
}
