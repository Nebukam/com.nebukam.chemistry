using System.Collections.Generic;
using Nebukam;
using Nebukam.Collections;
using UnityEngine;
using UnityEditor;

namespace Nebukam.Chemistry.Ed
{
    public class AtomConstraintsBuilder : Pooling.PoolItem, Pooling.IRequireCleanUp, Pooling.IRequireInit
    {

        protected internal AtomConstraints m_data;

        protected internal int m_index;
        protected internal GameObject m_prefab = null;
        protected internal int m_instanceCount = 0;
        protected internal ListDictionary<int, AtomConstraintsBuilder> m_neighbors = new ListDictionary<int, AtomConstraintsBuilder>();
        
        public void Add(int side, AtomConstraintsBuilder infos)
        {
            if(m_neighbors.Contains(side, infos))
            {
                //TODO : Should we keep track of neighboring count of a given prefab, 
                //it should be there.
            }
            else
            {
                m_neighbors.Add(side, infos);
            }
        }

        public AtomConstraints FixData( AtomConstraintsModel model )
        {

            m_data.index = m_index;
            m_data.prefab = m_prefab;
            m_data.instanceCount = m_instanceCount;

            AtomConstraintsBuilder builder;
            List<int> 
                positions = m_neighbors.keyList,
                indices = new List<int>();
            int iCount = model.sockets.Length;

            int[] 
                begin = new int[iCount],
                lengths = new int[iCount];
            int count, index = 0;

            for(int i = 0; i < iCount; i++)
            {

                if (m_neighbors.TryGet(i, out List<AtomConstraintsBuilder> builders))
                    count = builders.Count;
                else
                    count = 0;

                begin[i] = index;
                lengths[i] = count;

                for (int b = 0; b < count; b++)
                {
                    builder = builders[b];
                    if(builder == null)
                        indices.Add(SlotContent.NULL);
                    else
                        indices.Add(builder.m_index);

                    index++;
                }

            }

            m_data.begin = begin;
            m_data.lengths = lengths;
            m_data.neighbors = indices.ToArray();

            return m_data;
        }

        public void Init()
        {
            m_data = ScriptableObject.CreateInstance<AtomConstraints>();
        }

        public void CleanUp()
        {
            m_data = null;
            m_prefab = null;
            m_instanceCount = 0;
            m_neighbors.Clear();
        }

    }
}
