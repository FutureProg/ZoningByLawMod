using Colossal.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Common;
using Game.Input;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;
using Game.Zones;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Trejak.ZoningByLaw.UI
{
    public partial class ConfigPanelUISystem : UISystemBase
    {

        private PrefabSystem _prefabSystem;
        private EndFrameBarrier _endFrameBarrier;
        private ZoneSystem _zoneSystem;
        private EntityQuery _bylawsQuery;
        private EntityQuery _zoneCellsQuery;
        private Entity _selectedByLaw; // the prefab entity for the selected bylaw
        private PrefabBase _basePrefab; // the prefab we use to clone the zones        

        private ValueBinding<ByLawZoneData> _selectedByLawData;
        private ValueBinding<ByLawZoneListItem[]> _byLawZoneList;
        private ValueBinding<bool> _configPanelOpen;
        private ValueBinding<string> _selectedByLawName;
        private ValueBinding<Color[]> _selectedByLawColour;

        private TriggerBinding<Entity> _setActiveByLaw;
        private TriggerBinding<ByLawZoneData> _setByLawData;
        private TriggerBinding<ByLawZoneData> _createNewByLaw;
        private TriggerBinding _deleteByLaw;
        private TriggerBinding<bool> _setConfigPanelOpen;
        private TriggerBinding<string> _setByLawName;
        private TriggerBinding<Color, Color> _setByLawZoneColour;        

        const string uiGroupName = "Trejak.ZoningByLaw";

        struct ByLawZoneListItem : IJsonWritable
        {
            public Entity entity;
            public string name;

            public void Write(IJsonWriter writer)
            {
                writer.TypeBegin(GetType().FullName);
                writer.PropertyName("entity");
                writer.Write(entity);
                writer.PropertyName("name");
                writer.Write(name);
                writer.TypeEnd();
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _bylawsQuery = GetEntityQuery(ComponentType.ReadOnly<ByLawZoneData>());
            var eqb = new EntityQueryBuilder(Allocator.Temp);
            _zoneCellsQuery = eqb.WithAll<Cell>()
                .WithAbsent<Temp>()
                .WithAbsent<Deleted>()
                .Build(this.EntityManager);
            eqb.Reset();

            _selectedByLaw = Entity.Null;
            _prefabSystem = this.World.GetOrCreateSystemManaged<PrefabSystem>();
            _zoneSystem = this.World.GetOrCreateSystemManaged<ZoneSystem>();            
            _endFrameBarrier = this.World.GetOrCreateSystemManaged<EndFrameBarrier>();            
            GetBasePrefab();

            this.AddBinding(_selectedByLawData = new ValueBinding<ByLawZoneData>(uiGroupName, "SelectedByLawData", default));
            this.AddBinding(_configPanelOpen = new ValueBinding<bool>(uiGroupName, "IsConfigPanelOpen", false));
            this.AddBinding(_selectedByLawName = new ValueBinding<string>(uiGroupName, "SelectedByLawName", ""));
            this.AddBinding(_byLawZoneList = new ValueBinding<ByLawZoneListItem[]>(uiGroupName, "ByLawZoneList", GetByLawList(), new ArrayWriter<ByLawZoneListItem>()));
            this.AddBinding(_selectedByLawColour = new ValueBinding<Color[]>(uiGroupName, "SelectedByLawColour", new Color[] { default, default }, new ArrayWriter<Color>()));

            this.AddBinding(_setActiveByLaw = new TriggerBinding<Entity>(uiGroupName, "SetActiveByLaw", SetActiveByLaw));
            this.AddBinding(_setByLawData = new TriggerBinding<ByLawZoneData>(uiGroupName, "SetByLawData", SetByLawData));
            this.AddBinding(_createNewByLaw = new TriggerBinding<ByLawZoneData>(uiGroupName, "CreateNewByLaw", CreateNewByLaw));
            this.AddBinding(_deleteByLaw = new TriggerBinding(uiGroupName, "DeleteByLaw", DeleteByLaw));
            this.AddBinding(_setConfigPanelOpen = new TriggerBinding<bool>(uiGroupName, "SetConfigPanelOpen", SetConfigPanelOpen));
            this.AddBinding(_setByLawName = new TriggerBinding<string>(uiGroupName, "SetByLawName", SetByLawName));
            this.AddBinding(_setByLawZoneColour = new TriggerBinding<Color, Color>(uiGroupName, "SetByLawZoneColour", SetByLawZoneColour));

            eqb.Dispose();
        }

        void SetByLawZoneColour(Color zoneColour, Color borderColour)
        {
            if (_selectedByLaw == Entity.Null)
            {
                return;
            }
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw);
            prefab.m_Color = zoneColour;
            prefab.m_Edge = borderColour;
            Traverse.Create(_zoneSystem).Field<bool>("m_UpdateColors").Value = true;
            _selectedByLawColour.Update(new Color[] { zoneColour, borderColour });
            SaveByLawsToDisk();
        }

        void SetConfigPanelOpen(bool newValue)
        {
            if (newValue)
            {
                UpdateByLawList();
            }
            _configPanelOpen.Update(newValue);
        }

        void SetActiveByLaw(Entity entity)
        {
            Mod.log.Info("Set active by law to " + entity.Index + ", " + entity.Version);
            _selectedByLaw = entity;
            ByLawZoneData data;
            if (_selectedByLaw == Entity.Null)
            {
                data = default;
            } else
            {
                data = EntityManager.GetComponentData<ByLawZoneData>(entity);
            }
            bool result = _prefabSystem.TryGetPrefab<ByLawZonePrefab>(entity, out var prefab);            
            this._selectedByLawData.Update(data);
            if (result)
            {                
                this._selectedByLawColour.Update(new Color[] { prefab.m_Color, prefab.m_Edge });
                this._selectedByLawName.Update(prefab.name != null? prefab.name : "");
            } else
            {
                this._selectedByLawColour.Update(new Color[] { default, default });
                this._selectedByLawName.Update("");
            }
        }

        void SetByLawData(ByLawZoneData data)
        {
            Mod.log.Info("Set By Law Data: " + data);
            EntityManager.SetComponentData<ByLawZoneData>(_selectedByLaw, data);
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw);
            Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", data.CreateDescription());
            this._selectedByLawData.Update(data);
            SaveByLawsToDisk();
        }

        void SetByLawName(string name)
        {
            if (_selectedByLaw == Entity.Null)
            {
                return;
            }            
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw);
            prefab.name = name;
            Utils.AddLocaleText($"Assets.NAME[{prefab.name}]", prefab.name);
            Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", _selectedByLawData.value.CreateDescription());
            _selectedByLawName.Update(name);
            UpdateByLawList();
            SaveByLawsToDisk();
        }

        void GetBasePrefab()
        {
            var prefabs = Traverse.Create(_prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
            _basePrefab = prefabs.FirstOrDefault(p => p.name == "NA Residential Medium");
        }

        /// <summary>
        /// Called from the UI after the user hits save on a new bylaw
        /// </summary>
        /// <param name="data"></param>
        void CreateNewByLaw(ByLawZoneData data)
        {            
            if (_basePrefab == null)
            {
                GetBasePrefab();
            }
            ComponentBase[] baseComponents = new ComponentBase[_basePrefab.components.Count];
            _basePrefab.components.CopyTo(baseComponents);
            
            int count = _bylawsQuery.CalculateEntityCount();
            string byLawName = "Zoning ByLaw #" + count;
            var prefab = Utils.CreateByLawPrefabFromData(data, count, byLawName);
            if (!_prefabSystem.AddPrefab(prefab))
            {
                Mod.log.Error($"Failed to add new zone prefab \"{byLawName}\"!");                
            }
            UpdateByLawList();
            SaveByLawsToDisk();
        }

        void SaveByLawsToDisk()
        {
            var entities = _bylawsQuery.ToEntityArray(Allocator.Temp);
            Utils.SaveByLaws(entities.ToArray(), this.EntityManager);
            entities.Dispose();
        }

        void DeleteByLaw()
        {
            var ecb = _endFrameBarrier.CreateCommandBuffer();

            var entity = _selectedByLaw;
            var data = EntityManager.GetComponentData<ByLawZoneData>(entity);            
            data.deleted = true;
            EntityManager.SetComponentData(entity, data);
            var zoneData = EntityManager.GetComponentData<ZoneData>(entity);
            zoneData.m_AreaType = AreaType.None;            
            ecb.SetComponent(entity, zoneData);
            var uiObjData = EntityManager.GetComponentData<UIObjectData>(entity);            
            var uiGroupEntity = uiObjData.m_Group;
            uiObjData.m_Group = Entity.Null;
            EntityManager.SetComponentData(entity, uiObjData);
            var uiGroupElements = EntityManager.GetBuffer<UIGroupElement>(uiGroupEntity);
            for(int i = 0; i < uiGroupElements.Length; i++)
            {
                if (uiGroupElements[i].m_Prefab == entity)
                {
                    uiGroupElements.RemoveAt(i);
                    break;
                }
            }
            ecb.AddComponent<Updated>(entity);
            
            // Clear this zone from all of the existing cells
            var cellEntityArr = _zoneCellsQuery.ToEntityArray(Allocator.TempJob);
            DeleteByLawJob job = new()
            {
                cellBufferLookup = SystemAPI.GetBufferLookup<Cell>(false),
                cellEntityArr = cellEntityArr,
                ecb = ecb.AsParallelWriter(),
                zonePrefabs = _zoneSystem.GetPrefabs(),
                zoneToDelete = entity
            };
            this.Dependency = job.Schedule(cellEntityArr.Length, 32, this.Dependency);

            // "delete" the bylaw (really just hides it)
            //ecb.AddComponent<Deleted>(entity);
            _endFrameBarrier.AddJobHandleForProducer(this.Dependency);            

            UpdateByLawList();
            SetActiveByLaw(Entity.Null);
            SaveByLawsToDisk();
        }

        public partial struct DeleteByLawJob : IJobParallelFor
        {
            public BufferLookup<Cell> cellBufferLookup;
            public NativeArray<Entity> cellEntityArr;
            public ZonePrefabs zonePrefabs;
            public EntityCommandBuffer.ParallelWriter ecb;
            public Entity zoneToDelete;
            public void Execute(int index)
            {
                var cellEntity = cellEntityArr[index];
                var cellArr = cellBufferLookup[cellEntity];
                bool updated = false;
                for (int i = 0; i < cellArr.Length; i++)
                {
                    var cell = cellArr[i];
                    if (zonePrefabs[cell.m_Zone] == zoneToDelete)
                    {
                        cell.m_Zone = ZoneType.None;
                        cellArr[i] = cell;
                        updated = true;
                    }
                }
                if (updated)
                {
                    ecb.AddComponent<Updated>(index, cellEntity);
                }
            }
        }

        void UpdateByLawList()
        {            
            this._byLawZoneList.Update(GetByLawList());
        }

        ByLawZoneListItem[] GetByLawList()
        {
            List<ByLawZoneListItem> list = new();
            var entityArr = _bylawsQuery.ToEntityArray(Allocator.Temp);
            int counter = 0;
            for (int i = 0; i < entityArr.Length; i++)
            {                
                var entity = entityArr[i];
                var data = EntityManager.GetComponentData<ByLawZoneData>(entity);
                if (data.deleted)
                {
                    continue;
                }
                var prefabData = EntityManager.GetComponentData<PrefabData>(entity);
                var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(prefabData);                
                list.Add(new ByLawZoneListItem()
                {
                    entity = entity,
                    name = (counter+1) + ": " + prefab.name
                });
                counter++;
            }
            entityArr.Dispose();
            return list.ToArray();
        }

    }
}
