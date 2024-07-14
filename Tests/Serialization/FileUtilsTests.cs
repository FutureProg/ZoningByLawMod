using Game.Debug;
using Game.Debug.Tests;
using Game.Prefabs;
using System;
using System.IO;
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
            var other = FileUtils.LoadFromFile(record.idName + ".json");
            string filePath = Path.Combine(FileUtils.ByLawsFolder, record.idName + ".json");
            File.Delete(filePath);

            Assert.IsTrue(record.idName == other.idName, "ID Names don't match");
            Assert.IsTrue(record.bylawName == other.bylawName, "ByLaw Names don't match");
            Assert.IsTrue(record.zoningByLawBinding.blocks.Length == other.zoningByLawBinding.blocks.Length, "Block array lengths don't match");
            Assert.IsTrue(record.zoneColor == other.zoneColor, "Colours don't match");            
        }        

        [Test]        
        public void TestSaveToFile()
        {
            var record = TestRecord();
            FileUtils.SaveToFile(record);

            bool result = Directory.Exists(FileUtils.ByLawsFolder);
            string filePath = Path.Combine(FileUtils.ByLawsFolder, record.idName + ".json");
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
