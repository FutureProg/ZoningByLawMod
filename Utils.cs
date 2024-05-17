using Game.SceneFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trejak.ZoningByLaw
{
    public static class Utils
    {

        public static void AddLocaleText(string textId, string text)
        {
            GameManager.instance.localizationManager.activeDictionary.Add(textId, text);
        }

    }
}
