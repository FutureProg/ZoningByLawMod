using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;

namespace Trejak.ZoningByLaw.Tools
{
    public partial class ByLawConfigPanelOpenTool : ToolBaseSystem
    {
        public override string toolID => "Trejak.ByLawConfigPanelOpenTool";

        protected override void OnCreate()
        {
            base.OnCreate();            
        }

        public override PrefabBase GetPrefab()
        {
            return null;
        }

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            if (prefab is ByLawConfigButtonPrefab)
            {
                return true;
            }
            return false;
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

        }
    }
}
