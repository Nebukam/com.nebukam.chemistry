using Nebukam.JobAssist;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// Base solver bundle.
    /// </summary>
    /// <typeparam name="S">Slot Type</typeparam>
    /// <typeparam name="T">Slot Infos Type (paired with provided Slot type)</typeparam>
    /// <typeparam name="J">Solver Job struct</typeparam>
    /// <typeparam name="P">Solver processor (paired with provided Job type)</typeparam>
    public class AbstractSolverBundle<S, T, J, P> : ProcessorChain
        where S : ConstrainedSlot, ISlot
        where T : struct, ISlotInfos<S>
        where J : struct, IConstraintSolverJob<S, T>
        where P : AbstractConstraintsSolverProcessor<S, T, J>, new()
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

        public ISlotCluster<S> slotCluster
        {
            get { return m_preparation.slotCluster; }
            set { m_preparation.slotCluster = value; }
        }

        protected SolverPreparation<S, T> m_preparation;

        protected P m_constraintSolver;
        public IConstraintsSolverProcessor<S, T> constraintSolver { get { return m_constraintSolver; } }

        public AbstractSolverBundle()
        {
            Add(ref m_preparation);
            Add(ref m_constraintSolver);
        }

    }
}
