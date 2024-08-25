using Colossal.Entities;
using Game.Buildings;
using Game.Common;
using Game.Prefabs;
using Game.Tools;
using Game.UI.Tooltip;
using Trejak.ZoningByLaw.Prefab;

namespace Trejak.ZoningByLaw.Systems
{
    public partial class BuildingPropertiesToolTipSystem : TooltipSystemBase
    {
        private ToolSystem _toolSystem;
        private ToolRaycastSystem _toolRaycastSystem;
        private IndexBuildingsSystem _indexBuildingsSystem;
        private ZoningByLawToolSystem _bylawToolSystem;

        private FloatTooltip _heightTip;
        private FloatTooltip _lotSizeTip;
        private FloatTooltip _frontSetbackTip;
        private FloatTooltip _rearSetbackTip;
        private FloatTooltip _rightSetbackTip;
        private FloatTooltip _leftSetbackTip;


        protected override void OnCreate()
        {
            base.OnCreate();
            _toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            _toolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();
            _bylawToolSystem = World.GetOrCreateSystemManaged<ZoningByLawToolSystem>();
            _indexBuildingsSystem = World.GetOrCreateSystemManaged<IndexBuildingsSystem>();

            _heightTip = new FloatTooltip()
            {
                path = "BuildingHeight",
                icon = "Media/Glyphs/Length.svg",
                label = "Height",//LocalizedString.Id("ZBL.Constraints[HEIGHT]"),
                unit = "length"
            };
            _lotSizeTip = new FloatTooltip()
            {
                path = "BuildingLotSize",
                icon = "Media/Glyphs/Length.svg",
                label = "Lot Size",
                unit = "length"
            };
            _frontSetbackTip = new FloatTooltip()
            {
                path = "BuildingFrontSetback",
                icon = "Media/Glyphs/Length.svg",
                label = "Front Setback",
                unit = "length"
            };
            _rearSetbackTip = new FloatTooltip()
            {
                path = "BuildingRearSetback",
                icon = "Media/Glyphs/Length.svg",
                label = "Rear Setback",
                unit = "length"
            };
            _leftSetbackTip = new FloatTooltip()
            {
                path = "BuildingLeftSetback",
                icon = "Media/Glyphs/Length.svg",
                label = "Rear Setback",
                unit = "length"
            };
            _rightSetbackTip = new FloatTooltip()
            {
                path = "BuildingRightSetback",
                icon = "Media/Glyphs/Length.svg",
                label = "Right Setback",
                unit = "length"
            };
        }

        protected override void OnUpdate()
        {
            if (_toolSystem.activeTool.toolID == _bylawToolSystem.toolID && GetRaycastResult(out var raycastResult, out var prefabRef))
            {
                var entity = raycastResult.m_Owner;
                var prefab = prefabRef.m_Prefab;
                if (_indexBuildingsSystem.TryGetProperties(prefabRef, out BuildingByLawProperties properties))
                {
                    _heightTip.value = properties.buildingHeight;
                    _frontSetbackTip.value = properties.buildingSetbackFront;
                    _rearSetbackTip.value = properties.buildingSetBackRear;
                    _leftSetbackTip.value = properties.buildingSetBackLeft;
                    _rightSetbackTip.value = properties.buildingSetBackRight;
                    base.AddMouseTooltip(_heightTip);
                    base.AddMouseTooltip(_frontSetbackTip);
                    base.AddMouseTooltip(_rearSetbackTip);
                    base.AddMouseTooltip(_leftSetbackTip);
                    base.AddMouseTooltip(_rightSetbackTip);
                }
            }            
        }

        private bool GetRaycastResult (out RaycastResult raycastResult, out PrefabRef prefabRef)
        {
            prefabRef = default;
            return _toolRaycastSystem.GetRaycastResult(out raycastResult)
                && EntityManager.HasComponent<Building>(raycastResult.m_Owner)
                && EntityManager.TryGetComponent(raycastResult.m_Owner, out prefabRef);
        }
    }
}
