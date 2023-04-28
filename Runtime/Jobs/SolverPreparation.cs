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

using Nebukam.Cluster;
using Nebukam.JobAssist;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T_SLOT">Slot Type</typeparam>
    /// <typeparam name="T_SLOT_INFOS">Slot Infos Type (paired with provided Slot type)</typeparam>
    public class SolverPreparation<T_SLOT, T_SLOT_INFOS, T_BRAIN> : ProcessorGroup
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : unmanaged, ISlotInfos<T_SLOT>
        where T_BRAIN : unmanaged, IClusterBrain
    {

        public ModuleConstraintsManifest manifest
        {
            get { return m_manifestProvider.manifest; }
            set { m_manifestProvider.manifest = value; }
        }

        public ISlotCluster<T_SLOT, T_BRAIN> slotCluster
        {
            get { return m_clusterProvider.slotCluster; }
            set { m_clusterProvider.slotCluster = value; }
        }

        protected ClusterProvider<T_SLOT, T_SLOT_INFOS, T_BRAIN> m_clusterProvider;
        public IClusterProvider<T_SLOT, T_SLOT_INFOS, T_BRAIN> clusterProvider { get { return m_clusterProvider; } }

        protected ConstraintsManifestProvider m_manifestProvider;
        public IConstraintsManifestProvider manifestProvider { get { return m_manifestProvider; } }

        public SolverPreparation()
        {
            Add(ref m_clusterProvider);
            Add(ref m_manifestProvider);
        }

    }
}
