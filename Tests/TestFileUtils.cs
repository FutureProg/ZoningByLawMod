using Game.Debug.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.Serialization;
using Trejak.ZoningByLaw.UISystems;
using UnityEngine;
using UnityEngine.Assertions;

namespace ZoningByLaw.Tests
{
    public class TestFileUtils
    {

        public void TestSaveToFile()
        {
            var binding = new ZoningByLawBinding()
            {
                blocks = new ByLawBlockBinding[0],
                deleted = false
            };
            ByLawRecord record = new ByLawRecord("TestByLaw", "Test Description", Color.white, Color.white, binding, new Game.Prefabs.PrefabID(nameof(ByLawZonePrefab), "TestByLaw"));
            Utils.SaveToFile(record);

            bool result = Directory.Exists(Path.Combine(Utils.ContentFolder, "ByLaws"));
            result = result && File.Exists(Path.Combine(Utils.ContentFolder, "ByLaws", record.bylawName + ".json"));

            Assert.IsTrue(result);
        }
    }
}
