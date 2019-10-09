using UnityEngine;
using Nebukam.Cluster;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.Chemistry
{
    [DisallowMultipleComponent]
    public class SimpleSolverBehaviour3D : ModuleTesterBehaviour<SlotClusterFixed<ConstrainedSlot, ClusterBrain>, ConstrainedSlot, ConstrainedSlotInfos, ClusterBrain>
    {

        protected override ISolverBundle Solver(SlotClusterFixed<ConstrainedSlot, ClusterBrain> cluster)
        {
            SimpleSolver<ConstrainedSlot, ConstrainedSlotInfos, ClusterBrain> solver = 
                new SimpleSolver<ConstrainedSlot, ConstrainedSlotInfos, ClusterBrain>();

            solver.slotCluster = cluster;

            return solver;
        }

    }

}