using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T_SLOT">Slot Type</typeparam>
    /// <typeparam name="T_SLOT_INFOS">Slot Infos Type (paired with provided Slot type)</typeparam>
    public class BaseSolver<T_SLOT, T_SLOT_INFOS, T_JOB, T_BRAIN> : AbstractSolverBundle<T_SLOT, T_SLOT_INFOS, T_JOB, T_BRAIN, BaseSolverProcessor<T_SLOT, T_SLOT_INFOS, T_JOB, T_BRAIN>>
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_JOB : struct, ISolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN>
        where T_BRAIN : struct, IClusterBrain
    {

    }
}
