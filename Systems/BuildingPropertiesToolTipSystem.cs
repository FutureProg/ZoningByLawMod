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

        private FloatTooltip _heightTooltip;

        protected override void OnCreate()
        {
            base.OnCreate();
            _toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            _toolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();
            _bylawToolSystem = World.GetOrCreateSystemManaged<ZoningByLawToolSystem>();
            _indexBuildingsSystem = World.GetOrCreateSystemManaged<IndexBuildingsSystem>();

            _heightTooltip = new FloatTooltip()
            {
                path = "BuildingHeight",
                icon = "Media/Glyphs/Length.svg",
                label = "Height",//LocalizedString.Id("ZBL.Constraints[HEIGHT]"),
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
                    _heightTooltip.value = properties.buildingHeight;                    
                    base.AddMouseTooltip(_heightTooltip);                    
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
