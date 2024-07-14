using Game.Debug;
using Game.Debug.Tests;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.Serialization;
using Trejak.ZoningByLaw.UISystems;
using UnityEngine;

namespace Trejak.ZoningByLaw.Tests.Serialization
{    
    public class FileUtilsTests
    {        

        private ByLawRecord TestRecord()
        {
            var binding = new ZoningByLawBinding()
            {
                blocks = new ByLawBlockBinding[0],
                deleted = false
            };
            ByLawRecord record = new ByLawRecord("TestByLaw", "Test Description", Color.white, Color.white, binding, new PrefabID(nameof(ByLawZonePrefab), "TestByLaw"));
            return record;
        }

        [Test]
        public void TestLoadFromFile()
        {
            var record = TestRecord();
            FileUtils.SaveToFile(record);
            var other = FileUtils.LoadFromFile(record.idName);

            Assert.Equals(record.idName, other.idName);
            Assert.Equals(record.bylawName, other.bylawName);
            Assert.Equals(record.zoningByLawBinding, other.zoningByLawBinding);
            Assert.Equals(record.zoneColor, other.zoneColor);
        }

        [Test]        
        public void TestSaveToFile()
        {
            var record = TestRecord();
            FileUtils.SaveToFile(record);

            bool result = Directory.Exists(Path.Combine(FileUtils.ContentFolder, "ByLaws"));
            string filePath = Path.Combine(FileUtils.ContentFolder, "ByLaws", record.idName + ".json");
            result = result && File.Exists(filePath);

            string fileContent = "";
            if (result)
            {
                fileContent = File.ReadAllText(filePath);
                File.Delete(filePath);
            }
            
            Assert.IsTrue(result, "Did not find expected file");

            Mod.log.Info($"Test Save File Result:\n{fileContent}");
        }

        [Test]
        public void TestSaveToFile_Null() => Assert.Throws<NullReferenceException>(() => FileUtils.SaveToFile(null));        
    }
}
