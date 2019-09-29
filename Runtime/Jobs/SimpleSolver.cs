using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S">Slot Type</typeparam>
    /// <typeparam name="T">Slot Infos Type (paired with provided Slot type)</typeparam>
    public class SimpleSolver<S, T> : AbstractSolverBundle<S, T, SimpleConstraintsSolverJob<S, T>, SimpleConstraintsSolverProcessor<S, T>>
        where S : ConstrainedSlot, ISlot
        where T : struct, ISlotInfos<S>
    {

    }
}
