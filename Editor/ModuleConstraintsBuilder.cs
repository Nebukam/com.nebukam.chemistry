// Copyright (c) 2019 Timothé Lapetite - nebukam@gmail.com.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Nebukam.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nebukam.Chemistry.Ed
{
    public class ModuleConstraintsBuilder : Pooling.PoolItem, Pooling.IRequireCleanUp, Pooling.IRequireInit
    {

        protected internal ModuleConstraints m_data;

        protected internal int m_index;
        protected internal float m_weight = 1f;
        protected internal GameObject m_prefab = null;
        protected internal ListDictionary<int, ModuleConstraintsBuilder> m_neighbors = new ListDictionary<int, ModuleConstraintsBuilder>();

        public void Add(int side, ModuleConstraintsBuilder infos)
        {
            if (m_neighbors.Contains(side, infos))
            {
                //TODO : Should we keep track of neighboring count of a given prefab, 
                //it should be there.
            }
            else
            {
                m_neighbors.Add(side, infos);
            }
        }

        public ModuleConstraints FixData(ModuleConstraintsModel model)
        {

            m_data.index = m_index;
            m_data.prefab = m_prefab;
            m_data.weight = m_weight;

            ModuleConstraintsBuilder builder;
            List<int>
                positions = m_neighbors.keyList,
                indices = new List<int>();

            int iCount = model.socketsOffsets.Length;

            int[]
                begin = new int[iCount],
                lengths = new int[iCount];

            int count, index = 0;

            for (int i = 0; i < iCount; i++)
            {

                if (m_neighbors.TryGet(i, out List<ModuleConstraintsBuilder> builders))
                    count = builders.Count;
                else
                    count = 0;

                begin[i] = index;
                lengths[i] = count;

                for (int b = 0; b < count; b++)
                {
                    builder = builders[b];
                    if (builder == null)
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
            m_data = ScriptableObject.CreateInstance<ModuleConstraints>();
        }

        public void CleanUp()
        {
            m_data = null;
            m_prefab = null;
            m_weight = 1f;
            m_neighbors.Clear();
        }

    }
}
