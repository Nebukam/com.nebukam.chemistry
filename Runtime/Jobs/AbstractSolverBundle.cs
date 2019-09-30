using Nebukam.JobAssist;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// Base solver bundle.
    /// </summary>
    /// <typeparam name="T_SLOT">Slot Type</typeparam>
    /// <typeparam name="T_SLOT_INFOS">Slot Infos Type (paired with provided Slot type)</typeparam>
    /// <typeparam name="T_JOB">Solver Job struct</typeparam>
    /// <typeparam name="T_BRAIN">Cluster brain</typeparam>
    /// <typeparam name="T_SOLVER">Solver processor (paired with provided Job type)</typeparam>
    public class AbstractSolverBundle<T_SLOT, T_SLOT_INFOS, T_JOB, T_BRAIN, T_SOLVER> : ProcessorChain
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_JOB : struct, IConstraintSolverJob<T_SLOT, T_SLOT_INFOS>
        where T_BRAIN : struct, IClusterBrain
        where T_SOLVER : AbstractConstraintsSolverProcessor<T_SLOT, T_SLOT_INFOS, T_JOB, T_BRAIN>, new()
    {

        public uint seed {
            get { return m_constraintSolver.seed; }
            set { m_constraintSolver.seed = value; }
        }

        public AtomConstraintsManifest manifest
        {
            get { return m_preparation.manifest; }
            set { m_preparation.manifest = value; }
        }

        public ISlotCluster<T_SLOT, T_BRAIN> slotCluster
        {
            get { return m_preparation.slotCluster; }
            set { m_preparation.slotCluster = value; }
        }

        protected SolverPreparation<T_SLOT, T_SLOT_INFOS, T_BRAIN> m_preparation;

        protected T_SOLVER m_constraintSolver;
        public IConstraintsSolverProcessor<T_SLOT, T_SLOT_INFOS, T_BRAIN> constraintSolver { get { return m_constraintSolver; } }

        public AbstractSolverBundle()
        {
            Add(ref m_preparation);
            Add(ref m_constraintSolver);
        }

    }
}
