using Colossal.Mono.CompilerServices.SymbolWriter;
using Game;
using Game.Common;
using Game.Prefabs;
using Trejak.ZoningByLaw;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.UISystems;
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
                var blocks = SystemAPI.GetBuffer<ByLawBlockReference>(entity);
                ByLawZonePrefab prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(prefabData.ValueRO);
                Mod.log.Info("Initializing Zone Prefab: " + prefab.name);
                bylawData.ValueRW.deleted = prefab.deleted;
                SystemAPI.SetComponent(entity, bylawData.ValueRW);
                // create each individual block
                SetupByLawBlocks(blocks, prefab);
                var binding = ZoningByLawBinding.FromEntity(entity, this.EntityManager);
                UpdatePrefabFromBinding(binding, prefab);
            }
            entities.Dispose();
        }

        void UpdatePrefabFromBinding(ZoningByLawBinding binding, ByLawZonePrefab prefab)
        {
            prefab.Update(binding);
            Utils.SetPrefabText(prefab, binding);
        }

        void SetupByLawBlocks(DynamicBuffer<ByLawBlockReference> blockRefs, ByLawZonePrefab prefab)
        {
            foreach (var block in prefab.blocks)
            {
                var blockEntity = CreateByLawBlockEntity(block);
                blockRefs.Add(new() { block = blockEntity });
            }
        }

        Entity CreateByLawBlockEntity(ByLawBlockBinding block)
        {
            var blockEntity = EntityManager.CreateEntity(typeof(ByLawBlock), typeof(ByLawItem));

            SystemAPI.SetComponent(blockEntity, block.blockData);
            DynamicBuffer<ByLawItem> itemsBuffer = SystemAPI.GetBuffer<ByLawItem>(blockEntity);
            foreach (var bylawItem in block.itemData)
            {
                itemsBuffer.Add(bylawItem);
            }            
            return blockEntity;
        }

    }
}
