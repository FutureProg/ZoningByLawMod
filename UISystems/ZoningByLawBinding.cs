using Colossal.IO.AssetDatabase.Internal;
using Colossal.UI.Binding;
using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Collections;
using Unity.Entities;

namespace Trejak.ZoningByLaw.UISystems
{
    public struct ZoningByLawBinding //: IJsonWritable, IJsonReadable
    {
        public ByLawBlockBinding[] blocks;
        public bool deleted;


        public static ZoningByLawBinding FromEntity(Entity byLawEntity, EntityManager em)
        {
            var buffer = new ZoningByLawBinding.Buffer();
            ByLawZoneData byLawData = em.GetComponentData<ByLawZoneData>(byLawEntity);
            var blockRefBuffer = em.GetBuffer<ByLawBlockReference>(byLawEntity);

            buffer.SetDeleted(byLawData.deleted);            

            foreach(var blockRef in blockRefBuffer)
            {
                buffer.AddBlock(ByLawBlockBinding.FromEntity(blockRef.block, em));
            }

            return buffer.Build();
        }

        public void UpdateEntity(Entity byLawEntity, EntityManager em)
        {
            ByLawZoneData data = new()
            {
                deleted = this.deleted
            };
            em.SetComponentData(byLawEntity, data);

            var blockRefBuffer = em.GetBuffer<ByLawBlockReference>(byLawEntity);
            foreach(var bufferRef in blockRefBuffer)
            {
                em.AddComponent(bufferRef.block, typeof(Deleted));
            }
            blockRefBuffer.Clear();
            foreach(ByLawBlockBinding binding in blocks)
            {
                Entity e = em.CreateEntity(typeof(ByLawBlock), typeof(ByLawItem));
                em.SetComponentData(e, binding.blockData);
                var itemBuffer = em.GetBuffer<ByLawItem>(e);
                foreach(ByLawItem item in binding.itemData)
                {
                    itemBuffer.Add(item);
                }
                blockRefBuffer.Add(new() {  block = e });
            }
        }    
        
        public string CreateDescription()
        {
            return "Test Description";
        }

        //public void Read(IJsonReader reader)
        //{
        //    reader.ReadMapBegin();
        //    ArrayReader<ByLawBlockBinding> arrReader = new ArrayReader<ByLawBlockBinding>();
        //    reader.ReadProperty(nameof(blocks));            
        //    arrReader.Read(reader, out this.blocks);
        //    reader.ReadProperty(nameof(deleted));            
        //    reader.Read(out this.deleted);
        //    reader.ReadMapEnd();
        //}

        //public void Write(IJsonWriter writer)
        //{
        //    writer.TypeBegin(GetType().FullName);
        //    writer.PropertyName(nameof(blocks));
        //    writer.Write(blocks);
        //    writer.PropertyName(nameof(deleted));
        //    writer.Write(deleted);
        //    writer.TypeEnd();
        //}        

        public struct Buffer
        {
            List<ByLawBlockBinding> blocks;
            bool deleted;

            public Buffer()
            {
                this.blocks = new List<ByLawBlockBinding>();
                deleted = false;
            }

            public Buffer(bool deleted = false)
            {
                this.deleted = deleted;
                this.blocks = new List<ByLawBlockBinding>();
            }

            public void SetDeleted(bool deleted)
            {
                this.deleted = deleted;
            }

            public void AddBlock(ByLawBlock block, ByLawItem[] items)
            {
                this.blocks.Add(new ByLawBlockBinding()
                {
                    blockData = block,
                    itemData = items
                });
            }

            public void AddBlock(ByLawBlockBinding block)
            {
                this.blocks.Add(block);
            }

            public ZoningByLawBinding Build()
            {
                return new ZoningByLawBinding()
                {
                    blocks = this.blocks.ToArray(),
                    deleted = this.deleted
                };
            }
        }
    }

    public struct ByLawBlockBinding// : IJsonWritable, IJsonReadable
    {
        
        public ByLawBlock blockData;
        public ByLawItem[] itemData;

        public static ByLawBlockBinding FromEntity(Entity blockEntity, EntityManager em)
        {
            return new ByLawBlockBinding()
            {
                blockData = em.GetComponentData<ByLawBlock>(blockEntity),
                itemData = em.GetBuffer<ByLawItem>(blockEntity).ToNativeArray(Allocator.Persistent).ToArray()
            };            
        }

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty(nameof(blockData));
            this.blockData.Read(reader);
            reader.ReadProperty(nameof(itemData));
            ArrayReader<ByLawItem> arrReader = new ArrayReader<ByLawItem>();
            arrReader.Read(reader, out this.itemData);
            reader.ReadMapEnd();
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(GetType().FullName);
            writer.PropertyName(nameof(blockData));
            writer.Write(blockData);
            writer.PropertyName(nameof(itemData));
            GenericUIWriter<ByLawItem[]> genericWriter = new GenericUIWriter<ByLawItem[]>();
            genericWriter.Write(writer, itemData);
            writer.TypeEnd();
        }
    }
}
