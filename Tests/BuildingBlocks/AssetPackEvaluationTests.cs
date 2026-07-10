using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Assertions;
using Unity.Collections;
using ZoningByLaw.BuildingBlocks;

namespace Trejak.ZoningByLaw.Tests.BuildingBlocks
{
    public class AssetPackEvaluationTests
    {
        [Test]
        public void TestEvalAssetPack_MatchesWhenBuildingBelongsToASelectedPack()
        {
            var selectedHash = AssetPackHashUtils.NameToHash("European Pack");
            var otherHash = AssetPackHashUtils.NameToHash("North American Pack");

            var item = new ByLawItem
            {
                byLawItemType = ByLawItemType.AssetPack,
                constraintType = ByLawConstraintType.MultiSelect,
                propertyOperator = ByLawPropertyOperator.AtLeastOne,
                valueNumberArray = new NativeArray<int>(new[] { selectedHash }, Allocator.Persistent)
            };
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { otherHash, selectedHash }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(BuildingBlockSystem.EvalAssetPack(item, properties),
                    "Building belonging to a selected pack should match");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetPack_DoesNotMatchWhenBuildingBelongsToNoSelectedPack()
        {
            var selectedHash = AssetPackHashUtils.NameToHash("European Pack");
            var buildingHash = AssetPackHashUtils.NameToHash("North American Pack");

            var item = new ByLawItem
            {
                byLawItemType = ByLawItemType.AssetPack,
                constraintType = ByLawConstraintType.MultiSelect,
                propertyOperator = ByLawPropertyOperator.AtLeastOne,
                valueNumberArray = new NativeArray<int>(new[] { selectedHash }, Allocator.Persistent)
            };
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { buildingHash }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(!BuildingBlockSystem.EvalAssetPack(item, properties),
                    "Building not belonging to any selected pack should not match");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetPack_EmptySelection_NeverMatches()
        {
            var item = new ByLawItem
            {
                byLawItemType = ByLawItemType.AssetPack,
                constraintType = ByLawConstraintType.MultiSelect,
                propertyOperator = ByLawPropertyOperator.AtLeastOne,
                valueNumberArray = new NativeArray<int>(0, Allocator.Persistent)
            };
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { AssetPackHashUtils.NameToHash("European Pack") }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(!BuildingBlockSystem.EvalAssetPack(item, properties),
                    "An empty (or legacy, unresolved) pack selection should never match a building");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
            }
        }
    }
}
