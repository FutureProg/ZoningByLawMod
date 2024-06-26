using Colossal.Collections;
using Colossal.Serialization.Entities;
using Game;
using Game.Areas;
using Game.Buildings;
using Game.Common;
using Game.Prefabs;
using Game.Rendering;
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
using Unity.Mathematics;

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
            BufferLookup<SubMesh> subMeshBufferLookup = GetBufferLookup<SubMesh>(true);
            ComponentLookup<BuildingData> buildingDataLookup = GetComponentLookup<BuildingData>(true);
            ComponentLookup<ObjectGeometryData> objectGeomDataLookup = GetComponentLookup<ObjectGeometryData>(true);
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
                    bool hasParkingGarage = false;
                    for (int j = 0; j < buildingSubObjects.Length; j++)
                    {
                        SubObject subObj = buildingSubObjects[j];
                        if (subObj.m_Probability < 100 || (subObj.m_Flags & SubObjectFlags.OnGround) == 0)
                        {
                            continue;
                        }
                        else
                        {
                            parkingCount += AssessSubObject(subObj, subLaneBufferLookup, parkingLaneDataLookup, out hasParkingGarage);
                        }
                    }
                    ObjectGeometryData objGeom = objectGeomDataLookup[buildingEntity];
                    ObjectData objData = SystemAPI.GetComponent<ObjectData>(buildingEntity);                    
                    var archetypeComponents = objData.m_Archetype.Valid? objData.m_Archetype.GetComponentTypes(Allocator.Temp) : new NativeArray<ComponentType>(0, Allocator.Temp);
                    if (archetypeComponents.Length == 0)
                    {
                        Mod.log.Warn($"Object Archetype is empty for prefab with index {prefabData.m_Index}");
                    }
                    var propertyData = SystemAPI.GetComponent<BuildingPropertyData>(buildingEntity);
                    var props = new BuildingByLawProperties()
                    {
                        initialized = true,
                        parkingCount = parkingCount,
                        hasParkingGarage = hasParkingGarage,
                        buildingHeight = objGeom.m_Size.y,
                        isOffice = archetypeComponents.Contains(ComponentType.ReadOnly<OfficeProperty>()),
                        isIndustry = archetypeComponents.Contains(ComponentType.ReadOnly<IndustrialProperty>()),
                        isExtractor = archetypeComponents.Contains(ComponentType.ReadOnly<ExtractorProperty>()),
                        isResidential = propertyData.m_ResidentialProperties > 0,
                        isCommercial = archetypeComponents.Contains(ComponentType.ReadOnly<CommercialProperty>())
                    };

                    if (subMeshBufferLookup.TryGetBuffer(buildingEntity, out var buildingSubMeshes))
                    {
                        foreach(SubMesh subMesh in buildingSubMeshes)
                        {
                            AssessSubMesh(subMesh, buildingDataLookup[buildingEntity], ref props);
                        }
                    }

                    processedEnts++;
                    _properties[prefabData.m_Index] = props;
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

        public int AssessSubObject(SubObject subObj, BufferLookup<SubLane> subLaneBufferLookup, ComponentLookup<ParkingLaneData> parkingLaneDataLookup, out bool hasParkingGarage)
        {
            int re = 0;
            hasParkingGarage = false;
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
            if (SystemAPI.HasComponent<SpawnLocationData>(subObj.m_Prefab)) // seeing parking spawn location, seen on garages
            {
                var spawnLocData = SystemAPI.GetComponent<SpawnLocationData>(subObj.m_Prefab);
                if (spawnLocData.m_ConnectionType == RouteConnectionType.Parking && spawnLocData.m_RoadTypes == Game.Net.RoadTypes.Car)
                {
                    re++; // garages have no capacity (that I know of)
                    hasParkingGarage = true;
                }
            }
            return re;
        }

        public void AssessSubMesh(SubMesh subMesh, BuildingData buildingData, ref BuildingByLawProperties properties)
        {            
            if ((subMesh.m_Flags & SubMeshFlags.HasTransform) != 0 && SystemAPI.HasComponent<MeshData>(subMesh.m_SubMesh))
            {
                MeshData meshData = SystemAPI.GetComponent<MeshData>(subMesh.m_SubMesh);
                if (meshData.m_DecalLayer == Game.Rendering.DecalLayers.Buildings)
                {
                    var originOffset = subMesh.m_Position; // relative from lot cetnre, positive Z => towards the front, positve X => towards the left
                    var meshRotation = subMesh.m_Rotation;
                    var rigidTransform = new RigidTransform(meshRotation, originOffset);
                    var meshBounds = meshData.m_Bounds;
                    var meshDimensions = new float3(
                        math.abs(meshBounds.max.x - meshBounds.min.x),
                        math.abs(meshBounds.max.z - meshBounds.max.z),
                        math.abs(meshBounds.max.y) // we're just taking the height above ground
                    );                    
                    var dimensionsHalved = new float3(meshDimensions.x / 2f, 0, meshDimensions.z / 2f);
                    var frontLeft = new float3(dimensionsHalved.x, dimensionsHalved.y, dimensionsHalved.z);
                    var frontRight = new float3(-1 * dimensionsHalved.x, dimensionsHalved.y, dimensionsHalved.z);
                    var backLeft = new float3(dimensionsHalved.x, dimensionsHalved.y, -1 * dimensionsHalved.z);
                    var backRight = new float3(-1 * dimensionsHalved.x, dimensionsHalved.y, -1 * dimensionsHalved.z);
                    frontLeft = math.transform(rigidTransform, frontLeft);
                    frontRight = math.transform(rigidTransform, frontRight);
                    backLeft = math.transform(rigidTransform, backLeft);
                    backRight = math.transform(rigidTransform, backRight);

                    const int FrontLeft = 0, FrontRight = 1, BackLeft = 2, BackRight = 3;
                    var positions = new float3[] { frontLeft, frontRight, backLeft, backRight };
                    float3 maxX = positions[0], maxZ = positions[0], minX = positions[0], minZ = positions[0];
                    for(int i = 0; i < 4; i++)
                    {
                        var pos = positions[i];
                        if (pos.x > maxX.x)
                        {
                            maxX = pos;
                        }
                        if (pos.x < minX.x)
                        {
                            minX = pos;
                        }
                        if (pos.z < minZ.z)
                        {
                            minZ = pos;
                        }
                        if (pos.z > maxZ.z)
                        {
                            maxZ = pos;
                        }
                    }
                    var lotSizeMetres = new float3(buildingData.m_LotSize.x * 8f, 0, buildingData.m_LotSize.y * 8f); // just for my own sanity, putting depth on the z axis
                    float frontSetback = math.abs((lotSizeMetres.z / 2f) - maxZ.z);                    
                    float rearSetback = math.abs((-lotSizeMetres.z / 2f) - minZ.z);
                    float leftSetback = math.abs((lotSizeMetres.x / 2f) - maxX.x);
                    float rightSetback = math.abs((-lotSizeMetres.x / 2f) - minX.x);

                    if (properties.checkedBuildingSetBack)
                    {
                        properties.buildingSetbackFront = math.min(properties.buildingSetbackFront, frontSetback);
                        properties.buildingSetBackRear = math.min(properties.buildingSetBackRear, rearSetback);
                        properties.buildingSetBackLeft = math.min(properties.buildingSetBackLeft, leftSetback);
                        properties.buildingSetBackRight = math.min(properties.buildingSetBackRight, rightSetback);
                    } else
                    {
                        properties.buildingSetbackFront = frontSetback;
                        properties.buildingSetBackRear = rearSetback;
                        properties.buildingSetBackLeft = leftSetback;
                        properties.buildingSetBackRight = rightSetback;
                        properties.checkedBuildingSetBack = true;
                    }
                }                
            }            
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
        public bool hasParkingGarage;
        public bool checkedBuildingSetBack;
        public bool isResidential;
        public bool isCommercial;
        public bool isOffice;
        public bool isIndustry;
        public bool isExtractor;

        public float buildingSetbackFront;
        public float buildingSetBackLeft;
        public float buildingSetBackRight;
        public float buildingSetBackRear;
        public float buildingHeight;
    }
}
