using Colossal.UI.Binding;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace Trejak.ZoningByLaw.BuildingBlocks
{
    // attached to the same entity as ByLawZoneData
    public struct ByLawBlockReference: IBufferElementData
    {
        public Entity block;
    }

    public struct ByLawBlock : IComponentData, IJsonReadable, IJsonWritable
    {
        public BlockType blockType;
        public LogicOperation logicOperation;        
        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty("blockType");            
            reader.Read(out int blockTypePre);
            blockType = (BlockType)blockTypePre;
            reader.ReadProperty("logicOperation");
            reader.Read(out int logicOpPre);            
            logicOperation = (LogicOperation)logicOpPre;
            reader.ReadMapEnd();
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(GetType().FullName);
            writer.PropertyName(nameof(blockType));
            writer.Write((int) blockType);
            writer.PropertyName(nameof(logicOperation));
            writer.Write((int) logicOperation);
            writer.TypeEnd();
        }
    }    

    public enum BlockType : int
    {
        None = 0,
        Instruction = 1,
        Logic = 2
    }

    public enum LogicOperation : int
    {
        None = 0
    }

}
