using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Assertions;
using Unity.Collections;
using ZoningByLaw.BuildingBlocks;

namespace Trejak.ZoningByLaw.Tests.BuildingBlocks
{
    // Asset Pack and Asset Theme are combined into a single AssetStyle constraint (issue #25) so that
    // selecting both a pack and a theme spawns buildings matching EITHER selection (union), not just
    // buildings matching both at once, and so a negative ("none of") condition is available for both.
    public class AssetStyleEvaluationTests
    {
        private static ByLawItem MakeItem(ByLawPropertyOperator op, int[] selectedHashes)
        {
            return new ByLawItem
            {
                byLawItemType = ByLawItemType.AssetStyle,
                constraintType = ByLawConstraintType.MultiSelect,
                propertyOperator = op,
                valueNumberArray = new NativeArray<int>(selectedHashes, Allocator.Persistent)
            };
        }

        [Test]
        public void TestEvalAssetStyle_AtLeastOne_MatchesWhenBuildingBelongsToASelectedPackOnly()
        {
            var selectedPackHash = AssetPackHashUtils.NameToHash("European Pack");
            var item = MakeItem(ByLawPropertyOperator.AtLeastOne, new[] { selectedPackHash });
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { selectedPackHash }, Allocator.Persistent),
                themes = new NativeArray<int>(0, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "Building belonging to a selected pack should match, even with no themes selected");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetStyle_AtLeastOne_MatchesWhenBuildingHasASelectedThemeOnly()
        {
            var selectedThemeHash = ThemeHashUtils.NameToHash("European");
            var item = MakeItem(ByLawPropertyOperator.AtLeastOne, new[] { selectedThemeHash });
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(0, Allocator.Persistent),
                themes = new NativeArray<int>(new[] { selectedThemeHash }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "Building with a selected theme should match, even with no packs selected");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetStyle_AtLeastOne_BothSelected_MatchesViaPackAloneUnion()
        {
            // A building belonging to the selected pack but not the selected theme should still match:
            // combining packs and themes into one constraint spawns the union of both selections, not
            // their intersection.
            var selectedPackHash = AssetPackHashUtils.NameToHash("European Pack");
            var selectedThemeHash = ThemeHashUtils.NameToHash("European");
            var item = MakeItem(ByLawPropertyOperator.AtLeastOne, new[] { selectedPackHash, selectedThemeHash });
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { selectedPackHash }, Allocator.Persistent),
                themes = new NativeArray<int>(new[] { ThemeHashUtils.NameToHash("North American") }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "Building matching the pack half of a combined pack+theme selection should match");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetStyle_AtLeastOne_BothSelected_MatchesViaThemeAloneUnion()
        {
            var selectedPackHash = AssetPackHashUtils.NameToHash("European Pack");
            var selectedThemeHash = ThemeHashUtils.NameToHash("European");
            var item = MakeItem(ByLawPropertyOperator.AtLeastOne, new[] { selectedPackHash, selectedThemeHash });
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { AssetPackHashUtils.NameToHash("North American Pack") }, Allocator.Persistent),
                themes = new NativeArray<int>(new[] { selectedThemeHash }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "Building matching the theme half of a combined pack+theme selection should match");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetStyle_AtLeastOne_BothSelected_DoesNotMatchWhenNeitherMatches()
        {
            var item = MakeItem(ByLawPropertyOperator.AtLeastOne, new[]
            {
                AssetPackHashUtils.NameToHash("European Pack"),
                ThemeHashUtils.NameToHash("European")
            });
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { AssetPackHashUtils.NameToHash("North American Pack") }, Allocator.Persistent),
                themes = new NativeArray<int>(new[] { ThemeHashUtils.NameToHash("North American") }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(!BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "Building matching neither the selected packs nor themes should not match");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetStyle_IsNot_BothSelected_MatchesWhenBuildingHasNeither()
        {
            // The negative condition should exclude a building only if it belongs to a selected pack OR
            // has a selected theme - a building with neither should pass ("none of" is satisfied).
            var item = MakeItem(ByLawPropertyOperator.IsNot, new[]
            {
                AssetPackHashUtils.NameToHash("European Pack"),
                ThemeHashUtils.NameToHash("European")
            });
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { AssetPackHashUtils.NameToHash("North American Pack") }, Allocator.Persistent),
                themes = new NativeArray<int>(new[] { ThemeHashUtils.NameToHash("North American") }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "A building belonging to none of the selected packs/themes should pass a 'none of' constraint");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetStyle_IsNot_FailsWhenBuildingMatchesSelectedPackOnly()
        {
            var selectedPackHash = AssetPackHashUtils.NameToHash("European Pack");
            var item = MakeItem(ByLawPropertyOperator.IsNot, new[] { selectedPackHash, ThemeHashUtils.NameToHash("European") });
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { selectedPackHash }, Allocator.Persistent),
                themes = new NativeArray<int>(new[] { ThemeHashUtils.NameToHash("North American") }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(!BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "A building belonging to a selected pack should fail a 'none of' constraint even if its theme isn't selected");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetStyle_IsNot_FailsWhenBuildingMatchesSelectedThemeOnly()
        {
            var selectedThemeHash = ThemeHashUtils.NameToHash("European");
            var item = MakeItem(ByLawPropertyOperator.IsNot, new[] { AssetPackHashUtils.NameToHash("European Pack"), selectedThemeHash });
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { AssetPackHashUtils.NameToHash("North American Pack") }, Allocator.Persistent),
                themes = new NativeArray<int>(new[] { selectedThemeHash }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(!BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "A building with a selected theme should fail a 'none of' constraint even if its pack isn't selected");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetStyle_EmptySelection_AtLeastOneNeverMatches()
        {
            var item = MakeItem(ByLawPropertyOperator.AtLeastOne, new int[0]);
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { AssetPackHashUtils.NameToHash("European Pack") }, Allocator.Persistent),
                themes = new NativeArray<int>(new[] { ThemeHashUtils.NameToHash("European") }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(!BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "An empty (or legacy, unresolved) selection should never match a building under AtLeastOne");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }

        [Test]
        public void TestEvalAssetStyle_EmptySelection_IsNotAlwaysMatches()
        {
            // Vacuous truth, matching the same convention as EvalDensity/EvalLandUse: with nothing
            // selected, "none of the selection" is trivially satisfied by every building.
            var item = MakeItem(ByLawPropertyOperator.IsNot, new int[0]);
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                assetPacks = new NativeArray<int>(new[] { AssetPackHashUtils.NameToHash("European Pack") }, Allocator.Persistent),
                themes = new NativeArray<int>(new[] { ThemeHashUtils.NameToHash("European") }, Allocator.Persistent)
            };

            try
            {
                Assert.IsTrue(BuildingBlockSystem.EvalAssetStyle(item, properties),
                    "An empty selection should always match a building under IsNot (nothing to exclude)");
            }
            finally
            {
                item.valueNumberArray.Dispose();
                properties.assetPacks.Dispose();
                properties.themes.Dispose();
            }
        }
    }
}
