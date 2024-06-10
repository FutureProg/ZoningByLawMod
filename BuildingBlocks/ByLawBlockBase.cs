using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace Trejak.ZoningByLaw.BuildingBlocks
{
    /// <summary>
    /// Base component for the components of a prefab
    /// </summary>
    public abstract class ByLawBlockBase
    {

        public string name;
        public ByLawBlockBase next;

        public abstract IByLawBlockData GetBlockData();
    }

    public interface IByLawBlockData 
    {}

}
