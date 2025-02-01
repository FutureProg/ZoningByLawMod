using Colossal;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Collections;
using Unity.Entities;

namespace Trejak.ZoningByLaw
{
    public class LocaleDictionary : IDictionarySource
    {

        EntityQuery _bylawQuery;
        PrefabUISystem _prefabUISystem;
        PrefabSystem _prefabSystem;

        public LocaleDictionary(EntityManager em, PrefabUISystem prefabUISystem, PrefabSystem prefabSystem)
        {
            _bylawQuery = em.CreateEntityQuery(ComponentType.ReadOnly<ByLawZoneData>(), ComponentType.ReadOnly<PrefabData>());
            _prefabUISystem = prefabUISystem;
            _prefabSystem = prefabSystem;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            var bylawPrefabArr = _bylawQuery.ToComponentDataArray<PrefabData>(Allocator.Temp);
            var bylawZoneDataArr = _bylawQuery.ToComponentDataArray<ByLawZoneData>(Allocator.Temp);
            for(int i = 0; i < bylawPrefabArr.Length; i++)
            {
                var prefabData = bylawPrefabArr[i];
                var zoneData = bylawZoneDataArr[i];
                var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(prefabData);
                var prefabEntity = _prefabSystem.GetEntity(prefab);
                _prefabUISystem.GetTitleAndDescription(prefabEntity, out string titleId, out string descId);

                yield return new(titleId, prefab.bylawName);
                yield return new(descId, prefab.CreateDescription());
            }            
        }

        public void Unload()
        {}
    }
}
