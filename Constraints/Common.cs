using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.BuildingBlocks;

public struct BaseConstraintData
{
    public ByLawItemType constraintType; 
    public ByLawConstraintType constraint;
    public ByLawItemCategory itemCategory;
    public ByLawPropertyOperator[] propertyOperators;
}