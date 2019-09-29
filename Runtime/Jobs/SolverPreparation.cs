using Nebukam.JobAssist;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S">Slot Type</typeparam>
    /// <typeparam name="T">Slot Infos Type (paired with provided Slot type)</typeparam>
    public class SolverPreparation<S, T> : ProcessorGroup
        where S : ConstrainedSlot, ISlot
        where T : struct, ISlotInfos<S>
    {

        public AtomConstraintsManifest manifest
        {
            get { return m_manifestProvider.manifest; }
            set { m_manifestProvider.manifest = value; }
        }

        public ISlotCluster<S> slotCluster
        {
            get { return m_clusterProvider.slotCluster; }
            set { m_clusterProvider.slotCluster = value; }
        }

        protected ClusterProvider<S, T> m_clusterProvider;
        public IClusterProvider<S, T> clusterProvider { get { return m_clusterProvider; } }

        protected ConstraintsManifestProvider m_manifestProvider;
        public IConstraintsManifestProvider manifestProvider { get { return m_manifestProvider; } }

        public SolverPreparation()
        {
            Add(ref m_clusterProvider);
            Add(ref m_manifestProvider);
        }

    }
}
