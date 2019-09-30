using Nebukam.JobAssist;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T_SLOT">Slot Type</typeparam>
    /// <typeparam name="T_SLOT_INFOS">Slot Infos Type (paired with provided Slot type)</typeparam>
    public class SolverPreparation<T_SLOT, T_SLOT_INFOS, T_BRAIN> : ProcessorGroup
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
    {

        public AtomConstraintsManifest manifest
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
