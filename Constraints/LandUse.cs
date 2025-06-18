using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.BuildingBlocks;

namespace Trejak.ZoningByLaw.Constraints
{
    public struct LandUseConstraint
    {
        public ByLawItemType constraintType { get => ByLawItemType.Uses; }
        public ByLawConstraintType constraint { get => ByLawConstraintType.MultiSelect; }
        public ByLawItemCategory itemCategory { get => ByLawItemCategory.Lot; }
        public ByLawPropertyOperator[] propertyOperators { get => new ByLawPropertyOperator[] { ByLawPropertyOperator.IsNot, ByLawPropertyOperator.AtLeastOne, ByLawPropertyOperator.OnlyOneOf}; }
        
        
    }
}
