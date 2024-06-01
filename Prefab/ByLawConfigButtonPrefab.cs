using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Trejak.ZoningByLaw.Prefab
{
    internal class ByLawConfigButtonPrefab : PrefabBase
    {

        public static ByLawConfigButtonPrefab Create(UIAssetCategoryPrefab assetCategory, PrefabSystem prefabSystem)
        {
            ByLawConfigButtonPrefab prefab = new ByLawConfigButtonPrefab();
            prefab.name = "ByLawConfigButtonPrefab";
            prefab.active = true;
            prefab.isDirty = true;

            UIObject uiObject = ScriptableObject.CreateInstance<UIObject>();
            uiObject.m_Group = assetCategory;            
            uiObject.m_Icon = "coui://trejak_zbl/config-icon.svg";
            uiObject.m_LargeIcon = "coui://trejak_zbl/config-icon.svg";
            uiObject.active = true;
            uiObject.m_Priority = 0;
            uiObject.name = prefab.name;
            prefab.AddComponentFrom(uiObject);

            Utils.AddLocaleText($"Assets.NAME[{prefab.name}]", "ByLaw Configuration Panel");
            Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", "Opens the panel that allows you to create and edit zoning bylaws");
            return prefab;
        }

    }
}
