using UnityEngine;
using Nebukam.Cluster;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.Chemistry
{
    [DisallowMultipleComponent]
    public class SimpleSolverCylinderBehaviour2D : ModuleTesterBehaviour<SlotClusterFixed<ConstrainedSlot, CylinderBrain>, ConstrainedSlot, ConstrainedSlotInfos, CylinderBrain>
    {

        SimpleSolver<ConstrainedSlot, ConstrainedSlotInfos, CylinderBrain> typedSolver;

        protected override ISolverBundle Solver(SlotClusterFixed<ConstrainedSlot, CylinderBrain> cluster)
        {
            typedSolver = new SimpleSolver<ConstrainedSlot, ConstrainedSlotInfos, CylinderBrain>();
            typedSolver.slotCluster = cluster;
            solver = typedSolver;

            return solver;
        }

        protected override void Spawn()
        {
            NativeArray<int> results = solver.results;
            int index, count = min(results.Length, cluster.Count);
            GameObject go, prefab;
            Quaternion r;
            ISlot slot;
            ByteTrio coord;
            float3 origin = transform.position;
            float inc = 57.2958f * ((PI * 2f) / (float)gridSize.x);

            for (int i = 0; i < count; i++)
            {

                index = results[i];

                if (index < 0)
                    prefab = unsolvablePrefab;
                else
                    prefab = manifest.infos[index].prefab;

                if (prefab == nullPrefab)
                    continue;

                slot = cluster[i];
                coord = slot.coordinates;

                r = Quaternion.Euler(0f, inc * i, 0f);
                go = GameObject.Instantiate(prefab, origin + cluster.brain.ComputePosition(ref coord), r, transform) as GameObject;

                go.name = i + ":" + prefab.name;

            }
        }


    }

}