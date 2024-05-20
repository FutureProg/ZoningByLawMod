using Colossal.UI.Binding;
using Game;
using Game.Common;
using Game.UI.InGame;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Entities;

namespace Trejak.ZoningByLaw.Systems
{
    public partial class ResetGameToolbarUISystem : GameSystemBase
    {
        private RawMapBinding<Entity> _toolBarUIAssetsBinding;        
        
        protected override void OnCreate()
        {
            base.OnCreate();
            var toolbarUISystem = this.World.GetOrCreateSystemManaged<ToolbarUISystem>();
            _toolBarUIAssetsBinding = Traverse.Create(toolbarUISystem).Field<RawMapBinding<Entity>>("m_AssetsBinding").Value;

            var query = GetEntityQuery(ComponentType.ReadOnly<ByLawZoneData>(), ComponentType.ReadOnly<Deleted>());
            this.RequireForUpdate(query);
        }
        protected override void OnUpdate()
        {
            _toolBarUIAssetsBinding.UpdateAll();
        }
    }
}
