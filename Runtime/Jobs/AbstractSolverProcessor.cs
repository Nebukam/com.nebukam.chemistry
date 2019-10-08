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
        protected NativeArray<int> m_results = new NativeArray<int>(0, Allocator.Persistent);
        protected NativeArray<float> m_debug = new NativeArray<float>(0, Allocator.Persistent);

        public uint seed { get; set; } = 1;
        public IClusterProvider<T_SLOT, T_SLOT_INFOS, T_BRAIN> clusterProvider { get { return m_clusterProvider; } }
        public IConstraintsManifestProvider manifestProvider { get { return m_manifestProvider; } }
        public NativeArray<int> results { get { return m_results; } }
        public NativeArray<float> debug { get { return m_debug; } }

        protected override void InternalLock()
        {

        }

        protected override void Prepare(ref T_JOB job, float delta)
        {

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
            job.headerWeights = m_manifestProvider.headerWeights;
            job.headerIndices = m_manifestProvider.headerIndices;
            job.neighbors = m_manifestProvider.neighbors;

            job.nullPairLookup = m_manifestProvider.nullPairLookup;
            
            int count = m_clusterProvider.outputSlotInfos.Length;
            if (m_results.Length != count)
            {
                m_results.Dispose();
                m_results = new NativeArray<int>(count, Allocator.Persistent);

                m_debug.Dispose();
                m_debug = new NativeArray<float>(count, Allocator.Persistent);
            }

            // This is where pre-defined constraints should be set.
            for (int i = 0; i < count; i++)
                m_results[i] = SlotContent.UNSET;

            job.results = m_results;
            job.debug = m_debug;
        }

        protected override void Apply(ref T_JOB job)
        {

        }

        protected override void InternalUnlock() { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) { return; }

            m_clusterProvider = null;
            m_manifestProvider = null;
            m_results.Dispose();

        }

    }
    
}
