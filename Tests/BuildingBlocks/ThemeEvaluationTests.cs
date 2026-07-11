using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Assertions;
using Unity.Collections;
using ZoningByLaw.BuildingBlocks;

namespace Trejak.ZoningByLaw.Tests.BuildingBlocks
{
    public class ThemeEvaluationTests
    {
        [Test]
        public void TestEvalTheme_MatchesWhenBuildingHasASelectedTheme()
        {
            var selectedHash = ThemeHashUtils.NameToHash("European");
            var otherHash = ThemeHashUtils.NameToHash("North American");

            var item = new ByLawItem
            {
                byLawItemType = ByLawItemType.Theme,
                constraintType = ByLawConstraintType.MultiSelect,
                propertyOperator = ByLawPropertyOperator.AtLeastOne,
                valueNumberArray = new NativeArray<int>(new[] { selectedHash, otherHash }, Allocator.Persistent)
            };
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                themes = new NativeArray<int>(new[] { selectedHash }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(BuildingBlockSystem.EvalTheme(item, properties),
                    "Building with a selected theme should match");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalTheme_DoesNotMatchWhenBuildingHasNoSelectedTheme()
        {
            var selectedHash = ThemeHashUtils.NameToHash("European");
            var buildingHash = ThemeHashUtils.NameToHash("North American");

            var item = new ByLawItem
            {
                byLawItemType = ByLawItemType.Theme,
                constraintType = ByLawConstraintType.MultiSelect,
                propertyOperator = ByLawPropertyOperator.AtLeastOne,
                valueNumberArray = new NativeArray<int>(new[] { selectedHash }, Allocator.Persistent)
            };
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                themes = new NativeArray<int>(new[] { buildingHash }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(!BuildingBlockSystem.EvalTheme(item, properties),
                    "Building without a selected theme should not match");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalTheme_EmptySelection_NeverMatches()
        {
            var item = new ByLawItem
            {
                byLawItemType = ByLawItemType.Theme,
                constraintType = ByLawConstraintType.MultiSelect,
                propertyOperator = ByLawPropertyOperator.AtLeastOne,
                valueNumberArray = new NativeArray<int>(0, Allocator.Persistent)
            };
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                themes = new NativeArray<int>(new[] { ThemeHashUtils.NameToHash("European") }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(!BuildingBlockSystem.EvalTheme(item, properties),
                    "An empty (or legacy, unresolved) theme selection should never match a building");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalTheme_BuildingWithNoTheme_NeverMatches()
        {
            var selectedHash = ThemeHashUtils.NameToHash("European");

            var item = new ByLawItem
            {
                byLawItemType = ByLawItemType.Theme,
                constraintType = ByLawConstraintType.MultiSelect,
                propertyOperator = ByLawPropertyOperator.AtLeastOne,
                valueNumberArray = new NativeArray<int>(new[] { selectedHash }, Allocator.Persistent)
            };
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                themes = new NativeArray<int>(0, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(!BuildingBlockSystem.EvalTheme(item, properties),
                    "A building with no theme (e.g. not zone-spawned) should not match any theme constraint");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.themes.Dispose();
            }
        }
    }
}
