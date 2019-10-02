using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T_SLOT">Slot Type</typeparam>
    /// <typeparam name="T_SLOT_INFOS">Slot Infos Type (paired with provided Slot type)</typeparam>
    public class SimpleSolver<T_SLOT, T_SLOT_INFOS, T_BRAIN> : AbstractSolverBundle<T_SLOT, T_SLOT_INFOS, SimpleSolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN>, T_BRAIN, SimpleSolverProcessor<T_SLOT, T_SLOT_INFOS, T_BRAIN>>
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
    {

    }
}
