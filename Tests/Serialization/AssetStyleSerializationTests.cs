using Newtonsoft.Json;
using System.Linq;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.Serialization;
using Unity.Assertions;
using Unity.Collections;

namespace Trejak.ZoningByLaw.Tests.Serialization
{
    // Asset Pack and Asset Theme are combined into a single AssetStyle constraint (issue #25). Pre-existing
    // bylaw data saved as the old, separate "AssetPack"/"Theme" item types must keep loading correctly -
    // these tests cover both the new combined format and migration of the old one.
    public class AssetStyleSerializationTests
    {
        [Test]
        public void TestSerializableByLawItem_JsonRoundTrip_PreservesBothPackAndThemeNames()
        {
            var item = new SerializableByLawItem
            {
                byLawItemType = ByLawItemType.AssetStyle.ToString(),
                constraintType = ByLawConstraintType.MultiSelect.ToString(),
                itemCategory = ByLawItemCategory.Lot.ToString(),
                propertyOperator = ByLawPropertyOperator.AtLeastOne.ToString(),
                assetPackNames = new[] { "European Pack" },
                themeNames = new[] { "European" }
            };

            string json = JsonConvert.SerializeObject(item);
            Assert.IsTrue(json.Contains("assetPackNames") && json.Contains("themeNames"),
                "JSON should persist both the pack and theme selections by name, not by raw hash");

            var deserialized = JsonConvert.DeserializeObject<SerializableByLawItem>(json);
            var byLawItem = deserialized.ToByLawItem();
            try
            {
                Assert.IsTrue(byLawItem.byLawItemType == ByLawItemType.AssetStyle,
                    "A saved AssetStyle item should load back as AssetStyle");
                Assert.IsTrue(byLawItem.valueNumberArray.Length == 2,
                    "Resolved item should have one hash for the pack and one for the theme");
                Assert.IsTrue(byLawItem.valueNumberArray.Contains(AssetPackHashUtils.NameToHash("European Pack")),
                    "Resolved hashes should contain the hash for the saved pack name");
                Assert.IsTrue(byLawItem.valueNumberArray.Contains(ThemeHashUtils.NameToHash("European")),
                    "Resolved hashes should contain the hash for the saved theme name");
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
        public void TestSerializableByLawItem_LegacyAssetPackRecord_MigratesToAssetStyle()
        {
            // A by-law saved before issue #25 as a standalone "AssetPack" item should keep working: it
            // loads as AssetStyle carrying only the pack half of the (now combined) selection.
            var legacyItem = new SerializableByLawItem
            {
                byLawItemType = ByLawItemType.AssetPack.ToString(),
                constraintType = ByLawConstraintType.MultiSelect.ToString(),
                itemCategory = ByLawItemCategory.Lot.ToString(),
                propertyOperator = ByLawPropertyOperator.AtLeastOne.ToString(),
                assetPackNames = new[] { "European Pack", "North American Pack" }
            };

            var byLawItem = legacyItem.ToByLawItem();
            try
            {
                Assert.IsTrue(byLawItem.byLawItemType == ByLawItemType.AssetStyle,
                    "A legacy AssetPack item should migrate to AssetStyle on load");
                Assert.IsTrue(byLawItem.constraintType == ByLawConstraintType.MultiSelect,
                    "The migrated item's constraintType should be recomputed for AssetStyle");
                Assert.IsTrue(byLawItem.valueNumberArray.Length == 2, "Both legacy pack names should resolve");
                Assert.IsTrue(byLawItem.valueNumberArray.Contains(AssetPackHashUtils.NameToHash("European Pack")));
                Assert.IsTrue(byLawItem.valueNumberArray.Contains(AssetPackHashUtils.NameToHash("North American Pack")));
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
        public void TestSerializableByLawItem_LegacyAssetPackRecord_NormalizesOnlyOneOfToAtLeastOne()
        {
            // The old AssetPack UI only ever offered the OnlyOneOf operator (a pre-existing mismatch
            // with the backend, which ignored propertyOperator entirely and always matched on simple
            // union), so real saved by-laws almost certainly have propertyOperator = OnlyOneOf. The new
            // EvalAssetStyle only handles AtLeastOne/IsNot, so migration must normalize this to
            // AtLeastOne or these existing by-laws would silently stop matching any building.
            var legacyItem = new SerializableByLawItem
            {
                byLawItemType = ByLawItemType.AssetPack.ToString(),
                constraintType = ByLawConstraintType.MultiSelect.ToString(),
                itemCategory = ByLawItemCategory.Lot.ToString(),
                propertyOperator = ByLawPropertyOperator.OnlyOneOf.ToString(),
                assetPackNames = new[] { "European Pack" }
            };

            var byLawItem = legacyItem.ToByLawItem();
            try
            {
                Assert.IsTrue(byLawItem.propertyOperator == ByLawPropertyOperator.AtLeastOne,
                    "A legacy OnlyOneOf AssetPack record should normalize to AtLeastOne on migration");
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
        public void TestSerializableByLawItem_LegacyThemeRecord_MigratesToAssetStyle()
        {
            var legacyItem = new SerializableByLawItem
            {
                byLawItemType = ByLawItemType.Theme.ToString(),
                constraintType = ByLawConstraintType.MultiSelect.ToString(),
                itemCategory = ByLawItemCategory.Lot.ToString(),
                propertyOperator = ByLawPropertyOperator.AtLeastOne.ToString(),
                themeNames = new[] { "European" }
            };

            var byLawItem = legacyItem.ToByLawItem();
            try
            {
                Assert.IsTrue(byLawItem.byLawItemType == ByLawItemType.AssetStyle,
                    "A legacy Theme item should migrate to AssetStyle on load");
                Assert.IsTrue(byLawItem.valueNumberArray.Length == 1, "The legacy theme name should resolve");
                Assert.IsTrue(byLawItem.valueNumberArray.Contains(ThemeHashUtils.NameToHash("European")));
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
            // A record saved before assetPackNames/themeNames existed only has the old raw-hash
            // valueNumberArray, which is no longer trustworthy (string.GetHashCode() isn't stable across
            // process restarts). Such records should be treated as an empty selection.
            var legacyItem = new SerializableByLawItem
            {
                byLawItemType = ByLawItemType.AssetPack.ToString(),
                constraintType = ByLawConstraintType.MultiSelect.ToString(),
                itemCategory = ByLawItemCategory.Lot.ToString(),
                propertyOperator = ByLawPropertyOperator.AtLeastOne.ToString(),
                valueNumberArray = new[] { 12345, 67890 },
                assetPackNames = null,
                themeNames = null
            };

            var byLawItem = legacyItem.ToByLawItem();
            try
            {
                Assert.IsTrue(byLawItem.byLawItemType == ByLawItemType.AssetStyle,
                    "Even a hash-only legacy record should still migrate its type to AssetStyle");
                Assert.IsTrue(byLawItem.valueNumberArray.Length == 0,
                    "Legacy hash-only records must resolve to an empty selection, not the stale raw hashes");
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
