using Colossal.Entities;
using Colossal.Json;
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
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.Serialization;
using Trejak.ZoningByLaw.Systems;
using Trejak.ZoningByLaw.UISystems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Trejak.ZoningByLaw.UI
{
    public partial class ConfigPanelUISystem : ExtendedUISystemBase
    {

        private PrefabSystem _prefabSystem;
        private ByLawRenderToolSystem _bylawRenderSystem;
        private ToolSystem _toolSystem;
        //private ToolbarUISystem _toolbarUISystem;
        //private RawMapBinding<Entity> _toolBarUIAssetsBinding;
        //private Traverse _toolbarUIClearAssetSelection;
        private EndFrameBarrier _endFrameBarrier;
        private ZoneSystem _zoneSystem;
        private EntityQuery _bylawsQuery;
        private EntityQuery _zoneCellsQuery;        
        private PrefabBase _basePrefab; // the prefab we use to clone the zones        

        private ValueBinding<Entity> _selectedByLaw; // the prefab entity for the selected bylaw
        private ValueBindingHelper<ZoningByLawBinding> _selectedByLawData;
        private ValueBindingHelper<ByLawZoneListItem[]> _byLawZoneList;
        private ValueBinding<bool> _configPanelOpen;
        private ValueBinding<string> _selectedByLawName;
        private ValueBinding<Color[]> _selectedByLawColour;

        private TriggerBinding<Entity> _setActiveByLaw;
        private TriggerBinding<ZoningByLawBinding> _setByLawData;
        private TriggerBinding _createNewByLaw;
        private TriggerBinding _deleteByLaw;
        private TriggerBinding<bool> _setConfigPanelOpen;
        private TriggerBinding<string> _setByLawName;
        private TriggerBinding<Color, Color> _setByLawZoneColour;

        //private TriggerBinding _toggleByLawRenderPreview;

        const string uiGroupName = "Trejak.ZoningByLaw";

        struct ByLawZoneListItem
        {
            public Entity entity;
            public string name;
        }

        protected override void OnCreate()
        {          
            base.OnCreate();
            var ecb = new EntityCommandBuffer();            
            _bylawsQuery = GetEntityQuery(ComponentType.ReadOnly<ByLawZoneData>());
            var eqb = new EntityQueryBuilder(Allocator.Temp);
            _zoneCellsQuery = eqb.WithAll<Cell>()
                .WithAbsent<Temp>()
                .WithAbsent<Deleted>()
                .Build(this.EntityManager);
            eqb.Reset();

            //_toolbarUISystem = this.World.GetOrCreateSystemManaged<ToolbarUISystem>();
            //_toolbarUIClearAssetSelection = Traverse.Create(_toolbarUISystem).Method("ClearAssetSelection", false);
            //_toolBarUIAssetsBinding = Traverse.Create(_toolbarUISystem).Field<RawMapBinding<Entity>>("m_AssetsBinding").Value;
            _prefabSystem = this.World.GetOrCreateSystemManaged<PrefabSystem>();
            _zoneSystem = this.World.GetOrCreateSystemManaged<ZoneSystem>();            
            _endFrameBarrier = this.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            _bylawRenderSystem = this.World.GetOrCreateSystemManaged<ByLawRenderToolSystem>();
            _toolSystem = this.World.GetOrCreateSystemManaged<ToolSystem>();
            GetBasePrefab();

            this.AddBinding(_selectedByLaw = new ValueBinding<Entity>(uiGroupName, "SelectedByLaw", Entity.Null));
            _selectedByLawData = this.CreateBinding<ZoningByLawBinding>("SelectedByLawData", new ZoningByLawBinding());
            this.AddBinding(_configPanelOpen = new ValueBinding<bool>(uiGroupName, "IsConfigPanelOpen", false));
            this.AddBinding(_selectedByLawName = new ValueBinding<string>(uiGroupName, "SelectedByLawName", ""));
            _byLawZoneList = this.CreateBinding<ByLawZoneListItem[]>("ByLawZoneList", GetByLawList());
            this.AddBinding(_selectedByLawColour = new ValueBinding<Color[]>(uiGroupName, "SelectedByLawColour", new Color[] { default, default }, new ArrayWriter<Color>()));

            this.AddBinding(_setActiveByLaw = new TriggerBinding<Entity>(uiGroupName, "SetActiveByLaw", SetActiveByLaw));
            _setByLawData = CreateTrigger<ZoningByLawBinding>("SetByLawData", SetByLawData);
            _createNewByLaw =  CreateTrigger("CreateNewByLaw", CreateNewByLaw);
            _deleteByLaw = CreateTrigger("DeleteByLaw", DeleteByLaw);            
            this.AddBinding(_setConfigPanelOpen = new TriggerBinding<bool>(uiGroupName, "SetConfigPanelOpen", SetConfigPanelOpen));
            this.AddBinding(_setByLawName = new TriggerBinding<string>(uiGroupName, "SetByLawName", SetByLawName));
            this.AddBinding(_setByLawZoneColour = new TriggerBinding<Color, Color>(uiGroupName, "SetByLawZoneColour", SetByLawZoneColour));
            //this.AddBinding(_toggleByLawRenderPreview = new TriggerBinding(uiGroupName, "ToggleByLawRenderPreview", ToggleByLawRenderPreview));

            eqb.Dispose();
        }        

        // TODO: this
        //void ToggleByLawRenderPreview()
        //{
        //    if (_toolSystem.activeTool == _bylawRenderSystem)
        //    {
        //        _bylawRenderSystem.SetToolEnabled(false);
        //    } else
        //    {
        //        _bylawRenderSystem.SetByLaw(_selectedByLawData.value);
        //    }
        //}

        void SetByLawZoneColour(Color zoneColour, Color borderColour)
        {
            if (_selectedByLaw.value == Entity.Null)
            {
                return;
            }
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw.value);
            prefab.m_Color = zoneColour;
            prefab.m_Edge = borderColour;
            Traverse.Create(_zoneSystem).Field<bool>("m_UpdateColors").Value = true;
            _selectedByLawColour.Update(new Color[] { zoneColour, borderColour });
            SaveActiveByLawToDisk();
        }

        public void SetConfigPanelOpen(bool newValue)
        {
            if (newValue)
            {
                UpdateByLawList();
            }
            _configPanelOpen.Update(newValue);
        }

        void SetActiveByLaw(Entity entity)
        {
            if (entity != Entity.Null && !EntityManager.HasComponent<ByLawZoneData>(entity))
            {                 
                Mod.log.Warn("Entity " + entity.Index + ", " + entity.Version + " doesn't have the ByLawZoneData component! Clearing active bylaw.");
                SetActiveByLaw(Entity.Null);
                return;
            }
            Mod.log.Info("Set active by law to " + entity.Index + ", " + entity.Version);            
            _selectedByLaw.Update(entity);
            ZoningByLawBinding data;
            if (_selectedByLaw.value == Entity.Null)
            {
                data = default;
            } else
            {
                data = ZoningByLawBinding.FromEntity(entity, EntityManager);
            }
            bool result = _prefabSystem.TryGetPrefab<ByLawZonePrefab>(entity, out var prefab);            
            this._selectedByLawData.Value = data;
            if (result)
            {                
                this._selectedByLawColour.Update(new Color[] { prefab.m_Color, prefab.m_Edge });
                this._selectedByLawName.Update(prefab.bylawName != null? prefab.bylawName : "");
            } else
            {
                this._selectedByLawColour.Update(new Color[] { default, default });
                this._selectedByLawName.Update("");
            }
        }

        void SetByLawData(ZoningByLawBinding data)
        {
            Mod.log.Info("Set By Law Data: " + data.ToJSONString());
            data.UpdateEntity(_selectedByLaw.value, this.EntityManager);            
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw.value);            
            prefab.Update(data);     
            GameManager.instance.localizationManager.ReloadActiveLocale();
            Utils.SetPrefabText(prefab, data);            
            this._selectedByLawData.Value = data;
            SaveActiveByLawToDisk();
        }

        void SetByLawName(string name)
        {
            if (_selectedByLaw.value == Entity.Null)
            {
                return;
            }            
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw.value);
            prefab.bylawName = name;
            Utils.AddLocaleText($"Assets.NAME[{prefab.name}]", prefab.bylawName);
            Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", _selectedByLawData.Value.CreateDescription());
            _selectedByLawName.Update(prefab.bylawName);
            GameManager.instance.localizationManager.ReloadActiveLocale();
            UpdateByLawList();
            SaveActiveByLawToDisk();
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
        void CreateNewByLaw()
        {
            ZoningByLawBinding data = new ZoningByLawBinding()
            {
                deleted = false,
                blocks = new ByLawBlockBinding[]
                {
                    new()
                    {
                        blockData = new BuildingBlocks.ByLawBlock()
                        {
                            blockType = BuildingBlocks.BlockType.Instruction
                        },
                        itemData = new BuildingBlocks.ByLawItem[]
                        {
                            new BuildingBlocks.ByLawItem()
                            {
                                byLawItemType = BuildingBlocks.ByLawItemType.Uses,
                                constraintType = BuildingBlocks.ByLawConstraintType.MultiSelect,
                                propertyOperator = BuildingBlocks.ByLawPropertyOperator.AtLeastOne,
                                itemCategory = BuildingBlocks.ByLawItemCategory.Lot,
                                valueByteFlag = 0
                            }
                        }
                    }
                }
            };
            int count = _bylawsQuery.CalculateEntityCount() + 1;
            string byLawName = "Zoning ByLaw " + count;
            string idName = byLawName + '_' + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var prefab = Utils.CreateByLawPrefabFromData(data, count, idName, byLawName);
            if (!_prefabSystem.AddPrefab(prefab))
            {
                Mod.log.Error($"Failed to add new zone prefab \"{byLawName}\"!");                
            }
            GameManager.instance.localizationManager.ReloadActiveLocale();
            UpdateByLawList();
            Utils.SaveByLaw(_prefabSystem.GetEntity(prefab), this.EntityManager);            
        }

        void SaveActiveByLawToDisk()
        {
            Utils.SaveByLaw(_selectedByLaw.value, this.EntityManager);            
        }

        void DeleteActiveByLawFromDisk()
        {
            UpdateByLawList();
            Utils.DeleteByLawFromDisk(_selectedByLaw.value, this.EntityManager);
            SetActiveByLaw(Entity.Null); 
        }

        void DeleteByLaw()
        {
            var ecb = _endFrameBarrier.CreateCommandBuffer();

            var entity = _selectedByLaw.value;
            var data = EntityManager.GetComponentData<ByLawZoneData>(entity);            
            data.deleted = true;
            EntityManager.SetComponentData(entity, data);
            DeleteActiveByLawFromDisk();

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
            //// Update the ToolbarUI            
            //_toolbarUIClearAssetSelection.GetValue();
            //_toolBarUIAssetsBinding.UpdateAll();

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
            this._byLawZoneList.Value = GetByLawList();
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
                    name = (counter+1) + ": " + prefab.bylawName
                });
                counter++;
            }
            entityArr.Dispose();
            return list.ToArray();
        }

    }
}
