using UnityEngine;
using Nebukam.Cluster;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.Chemistry
{
    [DisallowMultipleComponent]
    public class LowestAnthropySolverBehaviour2D : ModuleTesterBehaviour<SlotClusterFixed<ConstrainedSlot, ClusterBrain>, ConstrainedSlot, ConstrainedSlotInfos, ClusterBrain>
    {

        protected override ISolverBundle Solver(SlotClusterFixed<ConstrainedSlot, ClusterBrain> cluster)
        {
            LowestAntropySolver<ConstrainedSlot, ConstrainedSlotInfos, ClusterBrain> solver =
                new LowestAntropySolver<ConstrainedSlot, ConstrainedSlotInfos, ClusterBrain>();

            solver.slotCluster = cluster;

            return solver;
        }

    }

}