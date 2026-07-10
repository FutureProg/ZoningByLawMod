using Newtonsoft.Json;
using System.Linq;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.Serialization;
using Unity.Assertions;
using Unity.Collections;

namespace Trejak.ZoningByLaw.Tests.Serialization
{
    public class AssetPackSerializationTests
    {
        [Test]
        public void TestSerializableByLawItem_JsonRoundTrip_PreservesNames()
        {
            var item = new SerializableByLawItem
            {
                byLawItemType = ByLawItemType.AssetPack.ToString(),
                constraintType = ByLawConstraintType.MultiSelect.ToString(),
                itemCategory = ByLawItemCategory.Lot.ToString(),
                propertyOperator = ByLawPropertyOperator.AtLeastOne.ToString(),
                assetPackNames = new[] { "European Pack", "North American Pack" }
            };

            string json = JsonConvert.SerializeObject(item);
            Assert.IsTrue(json.Contains("assetPackNames"), "JSON should persist asset packs by name, not by raw hash");
            Assert.IsTrue(json.Contains("European Pack") && json.Contains("North American Pack"),
                "JSON should contain the stable pack names");

            var deserialized = JsonConvert.DeserializeObject<SerializableByLawItem>(json);
            Assert.IsTrue(deserialized.assetPackNames.Length == 2, "Deserialized record should keep both pack names");

            var byLawItem = deserialized.ToByLawItem();
            try
            {
                var expectedHashes = new[]
                {
                    AssetPackHashUtils.NameToHash("European Pack"),
                    AssetPackHashUtils.NameToHash("North American Pack")
                };
                Assert.IsTrue(byLawItem.valueNumberArray.Length == 2, "Resolved item should have one hash per name");
                foreach (var expected in expectedHashes)
                {
                    Assert.IsTrue(byLawItem.valueNumberArray.Contains(expected),
                        $"Resolved hashes should contain the hash for a saved pack name (expected {expected})");
                }
            }
            finally
            {
                if (byLawItem.valueNumberArray.IsCreated)
                {
                    byLawItem.valueNumberArray.Dispose();
                }
            }
        }

        [Test]
        public void TestSerializableByLawItem_LegacyHashOnlyRecord_ResolvesToEmpty()
        {
            // A record saved before assetPackNames existed only has the old raw-hash valueNumberArray,
            // which is no longer trustworthy (string.GetHashCode() isn't stable across process restarts).
            // Such records should be treated as an empty selection rather than matching on stale hashes.
            var legacyItem = new SerializableByLawItem
            {
                byLawItemType = ByLawItemType.AssetPack.ToString(),
                constraintType = ByLawConstraintType.MultiSelect.ToString(),
                itemCategory = ByLawItemCategory.Lot.ToString(),
                propertyOperator = ByLawPropertyOperator.AtLeastOne.ToString(),
                valueNumberArray = new[] { 12345, 67890 },
                assetPackNames = null
            };

            var byLawItem = legacyItem.ToByLawItem();
            try
            {
                Assert.IsTrue(byLawItem.valueNumberArray.Length == 0,
                    "Legacy hash-only AssetPack records must resolve to an empty selection");
            }
            finally
            {
                if (byLawItem.valueNumberArray.IsCreated)
                {
                    byLawItem.valueNumberArray.Dispose();
                }
            }
        }
    }
}
