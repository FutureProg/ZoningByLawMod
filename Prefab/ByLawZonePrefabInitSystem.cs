using Game;
using Game.Common;
using Game.Prefabs;
using Trejak.ZoningByLaw;
using Trejak.ZoningByLaw.Prefab;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace Trejak.ZoningByLaw.Prefab
{
    /// <summary>
    /// Responsible for transferring the data from the prefab object to the prefab entity
    /// </summary>
    public partial class ByLawZonePrefabInitSystem : GameSystemBase
    {

        EntityQuery _createdZonesQuery;
        private PrefabSystem _prefabSystem;


        protected override void OnCreate()
        {
            base.OnCreate();
            _createdZonesQuery = this.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<Created>(),
                ComponentType.ReadWrite<ByLawZoneData>()
            });
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
        }

        protected override void OnUpdate()
        {
            if (this._createdZonesQuery.IsEmptyIgnoreFilter)
            {
                return;
            }
            var entities = this._createdZonesQuery.ToEntityArray(Allocator.Temp);
            for(int i = 0; i < entities.Length; i++)
            {                
                var entity = entities[i];
                var bylawData = SystemAPI.GetComponentRW<ByLawZoneData>(entity);
                var prefabData = SystemAPI.GetComponentRO<PrefabData>(entity);
                ByLawZonePrefab prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(prefabData.ValueRO);
                Mod.log.Info("Initializing Zone Prefab: " + prefab.name);
                bylawData.ValueRW.frontage = prefab.frontage;
                bylawData.ValueRW.height = prefab.height;
                bylawData.ValueRW.lotSize = prefab.lotSize;
                bylawData.ValueRW.parking = prefab.parking;
                bylawData.ValueRW.zoneType = prefab.zoneType;                
                SystemAPI.SetComponent(entity, bylawData.ValueRW);
                Utils.SetPrefabText(prefab, bylawData.ValueRO);
            }
            entities.Dispose();
        }

    }
}
