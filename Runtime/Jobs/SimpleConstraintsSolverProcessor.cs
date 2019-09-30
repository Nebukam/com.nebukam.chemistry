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

using System;
using System.Collections.Generic;
using Nebukam.JobAssist;
using Unity.Collections;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{

    public class SimpleConstraintsSolverProcessor<T_SLOT, T_SLOT_INFOS, T_BRAIN> : AbstractConstraintsSolverProcessor<T_SLOT, T_SLOT_INFOS, SimpleConstraintsSolverJob<T_SLOT, T_SLOT_INFOS>, T_BRAIN>, IConstraintsSolverProcessor<T_SLOT, T_SLOT_INFOS, T_BRAIN>
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
    {

        protected override void InternalLock()
        {

        }

        protected override void Prepare(ref SimpleConstraintsSolverJob<T_SLOT, T_SLOT_INFOS> job, float delta)
        {

            base.Prepare(ref job, delta);

            job.random = new Unity.Mathematics.Random(seed);

            /*

            if (!TryGetFirstInGroup(out m_clusterProvider, true) ||
                !TryGetFirstInGroup(out m_manifestProvider, true))
            {
                throw new System.Exception("No Cluster Provider or Manifest Provider in chain !");
            }

            job.random = new Unity.Mathematics.Random(seed);

            job.inputSlotInfos = m_clusterProvider.outputSlotInfos;
            job.inputSlotCoordinateMap = m_clusterProvider.outputSlotCoordinateMap;

            job.socketCount = m_manifestProvider.manifest.socketCount;
            job.offsets = m_manifestProvider.offsets;
            job.mirrors = m_manifestProvider.mirrors;
            job.mirrorsIndices = m_manifestProvider.mirrorsIndices;

            job.headerCount = m_manifestProvider.manifest.headerCount;
            job.headerIndices = m_manifestProvider.headerIndices;
            job.neighbors = m_manifestProvider.neighbors;

            int count = m_clusterProvider.outputSlotInfos.Length;
            if (m_results.Length != count)
            {
                m_results.Dispose();
                m_results = new NativeArray<int>(count, Allocator.Persistent);
            }

            // This is where pre-defined constraints should be set.
            for (int i = 0; i < count; i++)
                m_results[i] = Constraint.UNSET;

            job.results = m_results;
            */
        }

        protected override void Apply(ref SimpleConstraintsSolverJob<T_SLOT, T_SLOT_INFOS> job)
        {

        }

        protected override void InternalUnlock() { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) { return; }

        }

    }
    
}
