using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Serialization;
using Unity.Assertions;

namespace Trejak.ZoningByLaw.Tests.Serialization
{
    public class ParkingSerializationTests
    {
        [Test]
        public void TestSerializableByLawItem_LegacyParkingRecord_SelfHealsConstraintTypeToCount()
        {
            // Saves written before the Parking mapping fix persisted constraintType="Length" for
            // Parking items, which routed evaluation to EvaluateLength (no Parking case, always false).
            // ToByLawItem() must recompute constraintType from byLawItemType instead of trusting the
            // stored value, so loading such a save repairs the item instead of staying broken.
            var legacyItem = new SerializableByLawItem
            {
                byLawItemType = ByLawItemType.Parking.ToString(),
                constraintType = ByLawConstraintType.Length.ToString(),
                itemCategory = ByLawItemCategory.Lot.ToString(),
                propertyOperator = ByLawPropertyOperator.Is.ToString(),
                valueNumberArray = new int[0]
            };

            var byLawItem = legacyItem.ToByLawItem();
            try
            {
                Assert.IsTrue(byLawItem.constraintType == ByLawConstraintType.Count,
                    $"Legacy Parking records with a stale constraintType must self-heal to Count on load, got {byLawItem.constraintType}");
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
