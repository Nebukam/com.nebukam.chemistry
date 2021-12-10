// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
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
using static Nebukam.JobAssist.CollectionsUtils;
using Unity.Collections;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T_SLOT">Slot Type</typeparam>
    /// <typeparam name="T_SLOT_INFOS">Slot Infos Type (paired with provided Slot type)</typeparam>
    public interface ISolverProcessor<T_SLOT, T_SLOT_INFOS, T_BRAIN> : IProcessor
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
    {
        uint seed { get; set; }
        IClusterProvider<T_SLOT, T_SLOT_INFOS, T_BRAIN> clusterProvider { get; }
        IConstraintsManifestProvider manifestProvider { get; }
        NativeArray<int> results { get; }
        NativeArray<float> debug { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T_SLOT">Slot Type</typeparam>
    /// <typeparam name="T_SLOT_INFOS">Slot Infos Type (paired with provided Slot type)</typeparam>
    /// <typeparam name="T_JOB">Solver Job struct</typeparam>
    /// <typeparam name="T_JOB">Cluster brain</typeparam>
    public abstract class AbstractSolverProcessor<T_SLOT, T_SLOT_INFOS, T_JOB, T_BRAIN> : Processor<T_JOB>, ISolverProcessor<T_SLOT, T_SLOT_INFOS, T_BRAIN>
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_JOB : struct, ISolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN>
        where T_BRAIN : struct, IClusterBrain
    {

        protected IClusterProvider<T_SLOT, T_SLOT_INFOS, T_BRAIN> m_clusterProvider = null;
        protected IConstraintsManifestProvider m_manifestProvider = null;
        protected NativeArray<int> m_results = default;
        protected NativeArray<float> m_debug = default;

        public uint seed { get; set; } = 1;
        public IClusterProvider<T_SLOT, T_SLOT_INFOS, T_BRAIN> clusterProvider { get { return m_clusterProvider; } }
        public IConstraintsManifestProvider manifestProvider { get { return m_manifestProvider; } }
        public NativeArray<int> results { get { return m_results; } }
        public NativeArray<float> debug { get { return m_debug; } }

        protected override void Prepare(ref T_JOB job, float delta)
        {

            if (!TryGetFirstInCompound(out m_clusterProvider, true) ||
                !TryGetFirstInCompound(out m_manifestProvider, true))
            {
                throw new System.Exception("No Cluster Provider or Manifest Provider in chain !");
            }

            job.random = new Unity.Mathematics.Random(seed);
            job.brain = m_clusterProvider.slotCluster.brain;

            job.inputSlotInfos = m_clusterProvider.outputSlotInfos;
            job.inputSlotCoordinateMap = m_clusterProvider.outputSlotCoordinateMap;

            job.socketCount = m_manifestProvider.manifest.socketCount;
            job.socketsOffsets = m_manifestProvider.socketsOffsets;
            job.socketsMirrors = m_manifestProvider.socketsMirrors;
            job.socketsMirrorsIndices = m_manifestProvider.socketsMirrorsIndices;

            job.moduleCount = m_manifestProvider.manifest.moduleCount;
            job.modulesWeights = m_manifestProvider.modulesWeights;
            job.modulesHeaders = m_manifestProvider.modulesHeaders;
            job.modulesNeighbors = m_manifestProvider.modulesNeighbors;

            job.nullPairLookup = m_manifestProvider.nullPairLookup;
            
            int count = m_clusterProvider.outputSlotInfos.Length;

            MakeLength(ref m_results, count);
            MakeLength(ref m_debug, count);

            // This is where pre-defined constraints should be set.
            FloodArray(m_results, SlotContent.UNSET);

            job.results = m_results;
            job.debug = m_debug;
        }

        protected override void InternalDispose()
        {
            m_clusterProvider = null;
            m_manifestProvider = null;
            m_results.Dispose();
        }

    }
    
}
