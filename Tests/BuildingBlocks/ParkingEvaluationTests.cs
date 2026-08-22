using Colossal.Mathematics;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Assertions;
using Unity.Entities;
using ZoningByLaw.BuildingBlocks;

namespace Trejak.ZoningByLaw.Tests.BuildingBlocks
{
    public class ParkingEvaluationTests
    {
        [Test]
        public void TestGetConstraintTypes_Parking_ReturnsCount()
        {
            // Parking was previously misclassified as Length, which routed it to EvaluateLength (no
            // Parking case, always false) instead of EvaluateCount (which has the real Parking logic).
            Assert.IsTrue(BuildingBlockSystem.GetConstraintTypes(ByLawItemType.Parking) == ByLawConstraintType.Count,
                "Parking must be classified as Count so evaluation dispatch reaches EvaluateCount");
        }

        [Test]
        public void TestEvaluateCount_Parking_MatchesWhenWithinBounds()
        {
            var item = new ByLawItem
            {
                byLawItemType = ByLawItemType.Parking,
                constraintType = ByLawConstraintType.Count,
                propertyOperator = ByLawPropertyOperator.Is,
                valueBounds1 = new Bounds1(1, 3)
            };
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                parkingCount = 2
            };

            Assert.IsTrue(BuildingBlockSystem.EvaluateCount(Entity.Null, properties, item, default),
                "A building whose parking count falls within the bounds should match");
        }

        [Test]
        public void TestEvaluateCount_Parking_DoesNotMatchWhenOutsideBounds()
        {
            var item = new ByLawItem
            {
                byLawItemType = ByLawItemType.Parking,
                constraintType = ByLawConstraintType.Count,
                propertyOperator = ByLawPropertyOperator.Is,
                valueBounds1 = new Bounds1(1, 3)
            };
            var properties = new BuildingByLawProperties
            {
                initialized = true,
                parkingCount = 0
            };

            Assert.IsTrue(!BuildingBlockSystem.EvaluateCount(Entity.Null, properties, item, default),
                "A building whose parking count falls outside the bounds should not match");
        }
    }
}
