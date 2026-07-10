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
using System.Timers;
using Colossal.Mathematics;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.Serialization;
using Trejak.ZoningByLaw.Systems;
using Trejak.ZoningByLaw.UISystems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using ZoningByLaw.BuildingBlocks;
using ZoningByLaw.UISystems;
using Colossal.IO.AssetDatabase.Internal;
using System.Collections;

namespace Trejak.ZoningByLaw.UI
{
    public partial class ConfigPanelUISystem : ExtendedUISystemBase
    {
        private PrefabSystem _prefabSystem;
        private ZoningByLawToolSystem _byLawToolSystem;
        private ToolSystem _toolSystem;
        private CountElligiblePropertiesSystem _elligibleBuildingsSystem;
        private IndexBuildingsSystem _indexBuildingsSystem;

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
        private GetterValueBinding<int> _elligibleBuildings;
        private TriggerBinding<bool> _setConfigPanelOpen;
        private TriggerBinding<string> _setByLawName;
        private TriggerBinding<Color, Color> _setByLawZoneColour;
        private int _lastElligibleBuildingCount;
        private GetterValueBinding<Dictionary<string, FieldDataBase>> _byLawFieldsBinding;

        private Timer _writeToFileTimer;

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
            _byLawToolSystem = this.World.GetOrCreateSystemManaged<ZoningByLawToolSystem>();
            _toolSystem = this.World.GetOrCreateSystemManaged<ToolSystem>();
            _elligibleBuildingsSystem = this.World.GetOrCreateSystemManaged<CountElligiblePropertiesSystem>();
            _indexBuildingsSystem = this.World.GetOrCreateSystemManaged<IndexBuildingsSystem>();
            GetBasePrefab();

            this.AddBinding(_selectedByLaw = new ValueBinding<Entity>(uiGroupName, "SelectedByLaw", Entity.Null));
            _selectedByLawData = this.CreateBinding<ZoningByLawBinding>("SelectedByLawData", new ZoningByLawBinding());
            this.AddBinding(_configPanelOpen = new ValueBinding<bool>(uiGroupName, "IsConfigPanelOpen", false));
            this.AddBinding(_selectedByLawName = new ValueBinding<string>(uiGroupName, "SelectedByLawName", ""));
            _byLawZoneList = this.CreateBinding<ByLawZoneListItem[]>("ByLawZoneList", GetByLawList());
            this.AddBinding(_selectedByLawColour = new ValueBinding<Color[]>(uiGroupName, "SelectedByLawColour",
                new Color[] { default, default }, new ArrayWriter<Color>()));

            this.AddBinding(_setActiveByLaw =
                new TriggerBinding<Entity>(uiGroupName, "SetActiveByLaw", SetActiveByLaw));
            _setByLawData = CreateTrigger<ZoningByLawBinding>("SetByLawData", SetByLawData);
            _createNewByLaw = CreateTrigger("CreateNewByLaw", CreateNewByLaw);
            _deleteByLaw = CreateTrigger("DeleteByLaw", OpenDeleteDialogue);

            _elligibleBuildings = CreateBinding<int>("ElligibleBuildings", GetElligibleBuildingCount);
            this.AddBinding(_setConfigPanelOpen =
                new TriggerBinding<bool>(uiGroupName, "SetConfigPanelOpen", SetConfigPanelOpen));
            this.CreateTrigger("ToggleTool", this.ToggleTool);
            this.AddBinding(_setByLawName = new TriggerBinding<string>(uiGroupName, "SetByLawName", SetByLawName));
            this.AddBinding(_setByLawZoneColour =
                new TriggerBinding<Color, Color>(uiGroupName, "SetByLawZoneColour", SetByLawZoneColour));
            this.AddBinding(new GetterMapBinding<string, int>(uiGroupName, "assetPackNameToHash",
                (string key) => key.GetHashCode()));
            //this.AddBinding(_toggleByLawRenderPreview = new TriggerBinding(uiGroupName, "ToggleByLawRenderPreview", ToggleByLawRenderPreview));

            _byLawFieldsBinding = this.CreateBinding("ByLawFields", GetFields);            
            this.CreateTrigger<string, SetByLawItemPayload<string>>("SetByLawItemValueString", SetByLawItemValueString);
            this.CreateTrigger<string, SetByLawItemPayload<int>>("SetByLawItemValueInt", SetByLawItemValueInt);
            this.CreateTrigger<string, SetByLawItemPayload<int[]>>("SetByLawItemValueIntArr", SetByLawItemValueIntArr);
            this.CreateTrigger<string, int>("SetByLawItemPropertyOperator", SetByLawItemPropertyOperator);
            this.CreateTrigger<ByLawItemType>("ToggleItemEnabled", ToggleItemEnabled);

            _writeToFileTimer = new Timer(4500);
            _writeToFileTimer.AutoReset = true;
            _writeToFileTimer.Elapsed += (sender, e) => SaveActiveByLawToDisk();

            eqb.Dispose();
        }

        int GetElligibleBuildingCount()
        {
            if (_selectedByLaw.value == Entity.Null ||
                !EntityManager.TryGetComponent<ByLawZoneData>(_selectedByLaw.value, out var bylawData))
            {
                _lastElligibleBuildingCount = -1;
            }
            else
            {
                _lastElligibleBuildingCount = bylawData.elligibleBuildings;
            }

            return _lastElligibleBuildingCount;
        }

        struct SetByLawItemPayload<T>
        {
            public string itemType;
            public T value;
        }

        void SetByLawItemValueInt(string itemType, SetByLawItemPayload<int> payload)
        {
            SetFieldValue(itemType, payload.value);
        }

        void SetByLawItemValueIntArr(string itemType, SetByLawItemPayload<int[]> payload)
        {
            SetFieldValue(itemType, payload.value);
        }

        void SetByLawItemValueString(string itemType, SetByLawItemPayload<string> payload)
        {
            SetFieldValue(itemType, payload.value);
        }

        void SetByLawItemPropertyOperator(string itemType, int propertyOperator)
        {
            SetFieldValue(itemType, propertyOperator, true);
        }

        public Dictionary<string, FieldDataBase> GetFields()
        {
            if (_selectedByLaw.value == Entity.Null)
            {
                return new Dictionary<string, FieldDataBase>();
            }

            Dictionary<string, FieldDataBase> fieldDict = new();
            var blockRefBuffer = EntityManager.GetBuffer<ByLawBlockReference>(_selectedByLaw.value);
            for (int i = 0; i < blockRefBuffer.Length; i++)
            {
                var blockEntity = blockRefBuffer[i].block;
                if (!EntityManager.HasComponent<ByLawBlock>(blockEntity) ||
                    !EntityManager.TryGetBuffer<ByLawItem>(blockEntity, false, out var bylawItemBuffer)) continue;

                // Set the default values for the ByLawItemType
                var itemTypesArr = Enum.GetValues(typeof(ByLawItemType));
                for (int j = 0; j < itemTypesArr.Length; j++)
                {
                    ByLawItemType itemType = (ByLawItemType)itemTypesArr.GetValue(j);
                    // Omit Asset Pack Item Type
                    if(itemType == ByLawItemType.AssetPack)
                    {
                        continue;
                    }
                    string itemTypeId = Enum.GetName(typeof(ByLawItemType), itemType);
                    var constraintType = BuildingBlockSystem.GetConstraintTypes(itemType);
                    var operators = BuildingBlockSystem.GetPropertyOperators(itemType).Select(op =>
                        new FieldDataOption<ByLawPropertyOperator>()
                        {
                            label = "ZBL.PropertyOperator[" + Enum.GetName(typeof(ByLawPropertyOperator), op) + "]",
                            value = op
                        }).ToList();
                    switch (constraintType)
                    {
                        case ByLawConstraintType.Count:
                        case ByLawConstraintType.Length:                            
                            fieldDict[itemTypeId] = new RangeFieldData()
                            {
                                id = itemTypeId,
                                label = "ZBL.ByLawItemType[" + itemTypeId + "]",
                                value = new double[] { 0, 0 },
                                operatorOptions = operators,
                                min = 0,
                                step = 1
                            };
                            break;
                        case ByLawConstraintType.MultiSelect:
                        case ByLawConstraintType.SingleSelect:
                            List<FieldDataOption<object>> mappedValues = new List<FieldDataOption<object>>();
                            if (itemType == ByLawItemType.AssetPack)
                            {
                                mappedValues = _indexBuildingsSystem.GetAssetPacks()
                                    .Select(ap => new FieldDataOption<object>()
                                    {
                                        label = ap.name,
                                        image = ap.thumbnailUrl,
                                        value = _indexBuildingsSystem.GetAssetPackHash(ap)
                                    }).ToList();
                            } else
                            {
                                var enumType = BuildingBlockSystem.GetConstraintEnumType(itemType);
                                var enumValues = Enum.GetValues(enumType);
                                foreach(var val in enumValues)
                                {
                                    // The 0/None value is never a meaningful flag in a multi-select
                                    // bitmask (it contributes nothing), so hide it. It stays available
                                    // for single-select constraints where None is a real category
                                    // (e.g. pollution "None" = reject any measurable pollution).
                                    if (constraintType == ByLawConstraintType.MultiSelect && Convert.ToInt32(val) == 0)
                                    {
                                        continue;
                                    }
                                    mappedValues.Add(new FieldDataOption<object>()
                                    {
                                        label = "ZBL.FlagValues[" + Enum.GetName(enumType, val) + "]",
                                        value = val
                                    });
                                }
                            }                            

                            if (constraintType == ByLawConstraintType.MultiSelect)
                            {
                                fieldDict[itemTypeId] = new CheckboxFieldData()
                                {
                                    id = itemTypeId,
                                    label = "ZBL.ByLawItemType[" + itemTypeId + "]",
                                    options = mappedValues,
                                    operatorOptions = operators,
                                    value = new int[] { 0 }
                                };
                            }
                            else
                            {
                                fieldDict[itemTypeId] = new RadioFieldData()
                                {
                                    id = itemTypeId,
                                    label = "ZBL.ByLawItemType[" + itemTypeId + "]",
                                    options = mappedValues,
                                    operatorOptions = operators,
                                    value = 0
                                };
                            }
                            break;
                        case ByLawConstraintType.None:
                        default:
                            break;
                    }
                }

                // If the ByLawItem exists in the buffer, override the default values with the saved ones
                for (int j = 0; j < bylawItemBuffer.Length; j++)
                {
                    var item = bylawItemBuffer[j];
                    if (item.byLawItemType == ByLawItemType.None) continue;
                    // Temporarily Skip Asset Pack here
                    if (item.byLawItemType == ByLawItemType.AssetPack)
                    {
                        continue;
                    }
                    var itemTypeKey = Enum.GetName(typeof(ByLawItemType), item.byLawItemType);                    
                    var constraintType = BuildingBlockSystem.GetConstraintTypes(item.byLawItemType);
                    switch (constraintType)
                    {                                                    
                        case ByLawConstraintType.Length:
                        case ByLawConstraintType.Count:
                            ((RangeFieldData)fieldDict[itemTypeKey]).value = new double[]
                            {
                                item.valueBounds1.min,
                                item.valueBounds1.max
                            };
                            break;
                        case ByLawConstraintType.SingleSelect:
                            ((RadioFieldData)fieldDict[itemTypeKey]).value = item.valueByteFlag;
                            break;
                        case ByLawConstraintType.MultiSelect:
                            if (item.byLawItemType == ByLawItemType.AssetPack)
                            {
                                ((CheckboxFieldData)fieldDict[itemTypeKey]).value = item.valueNumberArray.ToArray();
                            } else
                            {
                                var enumValues= BuildingBlockSystem.GetConstarintEnumValues(item.byLawItemType);
                                if (enumValues == null)
                                {
                                    throw new Exception($"Enum values not found for ByLawItemType {item.byLawItemType}!");
                                }
                                List<int> selectedValues = new List<int>();
                                foreach (int val in enumValues)
                                {                                    
                                    if ((item.valueByteFlag & val) != 0)
                                    {                                 
                                        selectedValues.Add(val);                                        
                                    }                                    
                                }
                                ((CheckboxFieldData)fieldDict[itemTypeKey]).value = selectedValues.ToArray();
                            }
                            break;
                        case ByLawConstraintType.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    fieldDict[itemTypeKey].selectedOperator = item.propertyOperator;
                }
            }
            return fieldDict;
        }

        private void SetFieldValue(string field, object value, bool propertyOperator = false)
        {
            if (_selectedByLaw.value == Entity.Null)
            {
                return;
            }

            var found = false;
            // TODO: this is temporary, will need to refactor and rename the type to LandUse on the C# side later
            if (field == "LandUse")
            {
                field = "Uses";
            }
            ByLawZoneData data = new()
            {
                deleted = false,
                elligibleBuildings = GetElligibleBuildingCount()
            };
            EntityManager.SetComponentData(_selectedByLaw.value, data);
            Mod.log.Info(
                $"Set field {field} to {value} on entity {_selectedByLaw.value.Index}, {_selectedByLaw.value.Version}");

            var blockRefBuffer = EntityManager.GetBuffer<ByLawBlockReference>(_selectedByLaw.value);
            for (int blockIdx = 0; blockIdx < blockRefBuffer.Length; blockIdx++)
            {
                var blockEntity = blockRefBuffer[blockIdx].block;
                if (EntityManager.HasComponent<ByLawBlock>(blockEntity) &&
                    EntityManager.TryGetBuffer<ByLawItem>(blockEntity, false, out var bylawItemBuffer))
                {
                    for (var i = 0; i < bylawItemBuffer.Length; i++)
                    {
                        var item = bylawItemBuffer[i];                        
                        string bylawItemTypeStr = Enum.GetName(typeof(ByLawItemType), item.byLawItemType);
                        if (bylawItemTypeStr != field) continue;
                        Mod.log.Info(
                            $"Found matching ByLawItem of type {bylawItemTypeStr} in block entity {blockEntity.Index}, {blockEntity.Version}");
                        Mod.log.Info("Is value an array? " + (value.GetType().IsArray ? "Yes" : "No"));
                        if (propertyOperator && value is int opIntValue)
                        {
                            if (Enum.IsDefined(typeof(ByLawPropertyOperator), opIntValue))
                            {
                                var op = (ByLawPropertyOperator)opIntValue;
                                item.propertyOperator = op;
                                bylawItemBuffer[i] = item;
                                Mod.log.Info($"Set propertyOperator to {op}");
                            }
                            else
                            {
                                Mod.log.Warn(
                                    $"Failed to parse int value '{opIntValue}' to ByLawPropertyOperator for field '{field}'");
                            }
                        }
                        else if (item.byLawItemType == ByLawItemType.AssetPack && value.GetType().IsArray)
                        {
                            var valArr = value as Array;
                            var assetHashList = new List<int>();
                            for (int idx = 0; idx < valArr.Length; idx++)
                            {
                                assetHashList.Add(int.Parse(valArr.GetValue(idx).ToString()));
                            }
                            var assetHashes = assetHashList.ToArray();
                            if (item.valueNumberArray.IsCreated)
                            {
                                item.valueNumberArray.Dispose();
                            }
                            item.valueNumberArray = new NativeArray<int>(assetHashes, Allocator.Persistent);
                            bylawItemBuffer[i] = item;
                            Mod.log.Info(
                                $"Set valueNumberArray to [{string.Join(", ", assetHashes)}]");
                        }
                        else switch (value)
                            {
                                case int intValue:
                                    item.valueNumber = intValue;
                                    bylawItemBuffer[i] = item;
                                    Mod.log.Info($"Set valueNumber to {intValue}");
                                    break;
                                case double doubleValue:
                                    item.valueNumber = (int)doubleValue;
                                    bylawItemBuffer[i] = item;
                                    Mod.log.Info($"Set valueNumber to {doubleValue}");
                                    break;
                                case float floatValue:
                                    item.valueNumber = (int)floatValue;
                                    bylawItemBuffer[i] = item;
                                    Mod.log.Info($"Set valueNumber to {floatValue}");
                                    break;
                                case string strValue when int.TryParse(strValue, out int parsedInt):
                                    item.valueNumber = parsedInt;
                                    bylawItemBuffer[i] = item;
                                    Mod.log.Info($"Set valueNumber to {parsedInt}");
                                    break;
                                case string strValue:
                                    Mod.log.Warn(
                                        $"Failed to parse string value '{strValue}' to int for field '{field}'");
                                    break;
                                case bool boolValue:
                                    item.valueByteFlag = boolValue ? 1 : 0;
                                    bylawItemBuffer[i] = item;
                                    Mod.log.Info($"Set valueByteFlag to {(boolValue ? 1 : 0)}");
                                    break;
                                case Array arr:                               
                                    switch (item.constraintType)
                                    {
                                        case ByLawConstraintType.Length or ByLawConstraintType.Count when arr.Length == 2 &&
                                                                              double.TryParse(arr.GetValue(0).ToString(), out var minVal) &&
                                                                              double.TryParse(arr.GetValue(1).ToString(), out var maxVal):
                                            item.valueBounds1 = new Bounds1() { min = (float)minVal, max = (float)maxVal };
                                            bylawItemBuffer[i] = item;
                                            Mod.log.Info($"Set valueBounds1 to [{minVal}, {maxVal}]");
                                            break;
                                        case ByLawConstraintType.MultiSelect or ByLawConstraintType.SingleSelect when
                                            arr.Length > 0:
                                            {
                                                int flag = 0;
                                                for (int j = 0; j < arr.Length; j++)
                                                {
                                                    if (arr.GetValue(j) is int enumInt)
                                                    {
                                                        flag |= enumInt;
                                                    }
                                                    else if (arr.GetValue(j) is string enumStr &&
                                                             int.TryParse(enumStr, out int parsedEnumInt))
                                                    {
                                                        flag |= parsedEnumInt;
                                                    }
                                                }

                                                item.valueByteFlag = flag;
                                                bylawItemBuffer[i] = item;
                                                Mod.log.Info($"Set valueByteFlag to {flag} from array of integers");
                                                break;
                                            }
                                    }

                                    break;
                                default:
                                    Mod.log.Warn($"Unhandled value type {value.GetType()} for field '{field}'");
                                    break;
                            }

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Mod.log.Warn($"No matching ByLawItem found for field '{field}'");
                }

                _elligibleBuildingsSystem.EnqueueUpdate(_selectedByLaw.value);
                _writeToFileTimer.Stop();
                this._selectedByLawData.Value = ZoningByLawBinding.FromEntity(_selectedByLaw.value, EntityManager);
                SaveActiveByLawToDisk();
                _writeToFileTimer.Start();
            }            
            this._byLawFieldsBinding.Update();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            this._elligibleBuildings.Update();            
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

        public void ToggleTool()
        {
            if (_toolSystem.activeTool == _byLawToolSystem)
            {
                _byLawToolSystem.SetToolEnabled(false);
            }
            else
            {
                _byLawToolSystem.SetToolEnabled(true);
            }
        }

        public void ToggleItemEnabled(ByLawItemType itemType)
        {
            if (_selectedByLaw.value == Entity.Null)
            {
                return;
            }
            var blockRefBuffer = EntityManager.GetBuffer<ByLawBlockReference>(_selectedByLaw.value);
            for(int blockIdx = 0; blockIdx < blockRefBuffer.Length; blockIdx++)
            {
                var blockEntity = blockRefBuffer[blockIdx].block;
                if (EntityManager.HasComponent<ByLawBlock>(blockEntity) &&
                    EntityManager.TryGetBuffer<ByLawItem>(blockEntity, false, out var bylawItemBuffer))
                {
                    bool foundByLawItem = false;
                    for (var i = 0; i < bylawItemBuffer.Length; i++)
                    {
                        var item = bylawItemBuffer[i];
                        if (item.byLawItemType != itemType) continue;
                        // remove the bylaw constraint, meaning we toggle it to disabled
                        foundByLawItem = true;
                        bylawItemBuffer.RemoveAt(i);
                        Mod.log.Info(
                            $"ByLawItem of type {item.byLawItemType} disabled in block entity {blockEntity.Index}, {blockEntity.Version}");
                        break;
                    }
                    if (!foundByLawItem)
                    {
                        // add a new bylaw constraint with default values, meaning we toggle it to enabled
                        ByLawItem newItem = new ByLawItem()
                        {
                            byLawItemType = itemType,
                            constraintType = BuildingBlockSystem.GetConstraintTypes(itemType),
                            propertyOperator = BuildingBlockSystem.GetPropertyOperators(itemType).First(),
                            valueBounds1 = new Bounds1() { min = 0, max = 0 },
                            valueByteFlag = 0,
                            valueNumber = 0,
                            valueNumberArray = new NativeArray<int>(0, Allocator.Persistent)
                        };
                        bylawItemBuffer.Add(newItem);
                        Mod.log.Info(
                            $"ByLawItem of type {newItem.byLawItemType} enabled in block entity {blockEntity.Index}, {blockEntity.Version}");
                    }
                }
            }
            _writeToFileTimer.Stop();
            _elligibleBuildingsSystem.EnqueueUpdate(_selectedByLaw.value);
            this._selectedByLawData.Value = ZoningByLawBinding.FromEntity(_selectedByLaw.value, EntityManager);
            this.SaveActiveByLawToDisk();
            _writeToFileTimer.Start();
            this._byLawFieldsBinding.Update();
        }

        public void SetConfigPanelOpen(bool newValue)
        {
            if (newValue)
            {
                UpdateByLawList();
                _writeToFileTimer.Start();
            }
            else
            {
                _writeToFileTimer.Stop();
                SaveActiveByLawToDisk();
            }

            _configPanelOpen.Update(newValue);
        }

        void SetActiveByLaw(Entity entity)
        {
            if (_selectedByLaw.value != Entity.Null && _selectedByLaw.value != entity)
            {
                _writeToFileTimer.Stop();
                SaveActiveByLawToDisk();
                _writeToFileTimer.Start();
            }

            if (entity != Entity.Null && !EntityManager.HasComponent<ByLawZoneData>(entity))
            {
                Mod.log.Warn("Entity " + entity.Index + ", " + entity.Version +
                             " doesn't have the ByLawZoneData component! Clearing active bylaw.");
                SetActiveByLaw(Entity.Null);
                return;
            }

            Mod.log.Info("Set active by law to " + entity.Index + ", " + entity.Version);
            _selectedByLaw.Update(entity);
            ZoningByLawBinding data;
            if (_selectedByLaw.value == Entity.Null)
            {
                data = default;
            }
            else
            {
                data = ZoningByLawBinding.FromEntity(entity, EntityManager);
                _elligibleBuildingsSystem.EnqueueUpdate(entity);
            }

            bool result = _prefabSystem.TryGetPrefab<ByLawZonePrefab>(entity, out var prefab);
            this._selectedByLawData.Value = data;
            if (result)
            {
                this._selectedByLawColour.Update(new Color[] { prefab.m_Color, prefab.m_Edge });
                this._selectedByLawName.Update(prefab.bylawName != null ? prefab.bylawName : "");
            }
            else
            {
                this._selectedByLawColour.Update(new Color[] { default, default });
                this._selectedByLawName.Update("");
            }
            _byLawFieldsBinding.Update();
        }

        void SetByLawData(ZoningByLawBinding data)
        {
            if (data.blocks.Length == 0 || data.blocks == null)
            {
                Mod.log.Warn("ByLawData has no blocks! Filling with one empty block.");
                data.blocks = new ByLawBlockBinding[]
                {
                    new()
                    {
                        blockData = new ByLawBlock()
                        {
                            blockType = BuildingBlocks.BlockType.Instruction
                        },
                        itemData = new BuildingBlocks.ByLawItem[0]
                    }
                };
            }

            Mod.log.Info("Set By Law Data: " + data.ToJSONString());
            data.UpdateEntity(_selectedByLaw.value, this.EntityManager, GetElligibleBuildingCount());
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw.value);
            prefab.UpdateByLawData(data);
            _elligibleBuildingsSystem.EnqueueUpdate(_selectedByLaw.value);
            GameManager.instance.localizationManager.ReloadActiveLocale();

            _writeToFileTimer.Stop();
            _writeToFileTimer.Start();
            this._selectedByLawData.Value = data;
            SaveActiveByLawToDisk();
            this._byLawFieldsBinding.Update();
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
            this._byLawFieldsBinding.Update();
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
                                valueByteFlag = 0,
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
            if (_selectedByLaw.value == Entity.Null)
            {
                return;
            }

            Utils.SaveByLaw(_selectedByLaw.value, this.EntityManager);
        }

        void OpenDeleteDialogue()
        {
            GameManager.instance.userInterface.appBindings.ShowConfirmationDialog(
                new ConfirmationDialog(
                    "Zoning ByLaw Mod",
                    "Are you sure you want to delete this bylaw? This cannot be undone!",
                    "Common.DIALOG_ACTION[Yes]",
                    "Common.DIALOG_ACTION[No]"
                ),
                option =>
                {
                    if (option == 0)
                    {
                        DeleteByLaw();
                    }
                }
            );
        }

        void DeleteActiveByLawFromDisk()
        {
            _writeToFileTimer.Stop();
            UpdateByLawList();
            Utils.DeleteByLawFromDisk(_selectedByLaw.value, this.EntityManager);
            SetActiveByLaw(Entity.Null);
            _writeToFileTimer.Start();
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
            for (int i = 0; i < uiGroupElements.Length; i++)
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
                    name = (counter + 1) + ": " + prefab.bylawName
                });
                counter++;
            }

            entityArr.Dispose();
            return list.ToArray();
        }
    }
}