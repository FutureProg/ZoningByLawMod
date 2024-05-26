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

namespace Trejak.ZoningByLaw.Prefab
{

    public partial class IndexBuildingsSystem : GameSystemBase
    {
        // to update after BuildingInitializeSystem in the PrefabUpdate phase

        private NativeHashMap<Entity, BuildingByLawProperties> index;

        EntityQuery _buildingsPrefabsQuery;


        protected override void OnUpdate()
        {
            _buildingsPrefabsQuery = GetEntityQuery(
                typeof(Created),
                typeof(PrefabData),
                typeof(BuildingData),
                typeof(SpawnableBuildingData),
                typeof(SubObject)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity">The prefab entity</param>
        public bool TryGetByLawProperty(Entity entity, out BuildingByLawProperties buildingByLawProperty)
        {
            return index.TryGetValue(entity, out buildingByLawProperty);
        }

        public partial struct UpdateByLawProperties : IJobChunk
        {

            public EntityTypeHandle entityHandle;
            public BufferTypeHandle<SubObject> subObjectBufferHandle;

            public BufferLookup<SubLane> subLaneBufferLookup;
            public ComponentLookup<ParkingLaneData> parkingLaneDataLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                throw new NotImplementedException();
            }
        }
    }

    public struct BuildingByLawProperties
    {
        public int parkingCount;
    }
}
