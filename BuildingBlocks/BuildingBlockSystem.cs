using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.BuildingBlocks;

namespace ZoningByLaw.BuildingBlocks
{
    public static class BuildingBlockSystem
    {

        public static ByLawConstraintType GetConstraintTypes(ByLawItemType itemType)
        {
            switch(itemType)
            {
                case ByLawItemType.Uses:
                    return ByLawConstraintType.MultiSelect;

                case ByLawItemType.Height:
                case ByLawItemType.LotWidth:
                case ByLawItemType.LotSize:
                case ByLawItemType.FrontSetback:
                case ByLawItemType.LeftSetback:
                case ByLawItemType.RightSetback:
                case ByLawItemType.RearSetback:                
                    return ByLawConstraintType.Length;

                case ByLawItemType.NoisePollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.AirPollutionLevel:
                    return ByLawConstraintType.MultiSelect;

                case ByLawItemType.None:
                default:
                    return ByLawConstraintType.None;
                
            }
        }

        public static ByLawItemCategory GetItemCategories(ByLawItemType itemType)
        {
            switch (itemType)
            {
                case ByLawItemType.Uses:
                case ByLawItemType.LotWidth:
                case ByLawItemType.LotSize:
                    return ByLawItemCategory.Lot;

                case ByLawItemType.Height:               
                case ByLawItemType.FrontSetback:
                case ByLawItemType.LeftSetback:
                case ByLawItemType.RightSetback:
                case ByLawItemType.RearSetback:
                    return ByLawItemCategory.Building;

                case ByLawItemType.NoisePollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.AirPollutionLevel:
                    return ByLawItemCategory.Pollution;

                case ByLawItemType.None:
                default:
                    return ByLawItemCategory.None;
            }
        }

    }
}
