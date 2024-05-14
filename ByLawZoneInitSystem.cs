using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Trejak.ZoningByLaw
{
    public partial class ByLawZoneInitSystem : GameSystemBase
    {

        private EntityQuery _createdEntities;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            
        }
    }
}
