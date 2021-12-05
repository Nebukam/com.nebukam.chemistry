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

using Nebukam.Cluster;
using Nebukam.JobAssist;
using Unity.Collections;

namespace Nebukam.Chemistry
{

    public interface ISolverBundle : IProcessor
    {
        uint seed { get; set; }
        ModuleConstraintsManifest manifest { get; set; }
        NativeArray<int> results { get; }
    }

    /// <summary>
    /// Base solver bundle.
    /// </summary>
    /// <typeparam name="T_SLOT">Slot Type</typeparam>
    /// <typeparam name="T_SLOT_INFOS">Slot Infos Type (paired with provided Slot type)</typeparam>
    /// <typeparam name="T_JOB">Solver Job struct</typeparam>
    /// <typeparam name="T_BRAIN">Cluster brain</typeparam>
    /// <typeparam name="T_SOLVER">Solver processor (paired with provided Job type)</typeparam>
    public class AbstractSolverBundle<T_SLOT, T_SLOT_INFOS, T_JOB, T_BRAIN, T_SOLVER> : ProcessorChain, ISolverBundle
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_JOB : struct, ISolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN>
        where T_BRAIN : struct, IClusterBrain
        where T_SOLVER : AbstractSolverProcessor<T_SLOT, T_SLOT_INFOS, T_JOB, T_BRAIN>, new()
    {

        public uint seed
        {
            get { return m_constraintSolver.seed; }
            set { m_constraintSolver.seed = value; }
        }

        public ModuleConstraintsManifest manifest
        {
            get { return m_preparation.manifest; }
            set { m_preparation.manifest = value; }
        }

        public ISlotCluster<T_SLOT, T_BRAIN> slotCluster
        {
            get { return m_preparation.slotCluster; }
            set { m_preparation.slotCluster = value; }
        }

        public NativeArray<int> results {
            get { return m_constraintSolver.results; }
        }

        protected SolverPreparation<T_SLOT, T_SLOT_INFOS, T_BRAIN> m_preparation;

        protected T_SOLVER m_constraintSolver;
        public ISolverProcessor<T_SLOT, T_SLOT_INFOS, T_BRAIN> constraintSolver { get { return m_constraintSolver; } }

        public AbstractSolverBundle()
        {
            Add(ref m_preparation);
            Add(ref m_constraintSolver);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }
}
