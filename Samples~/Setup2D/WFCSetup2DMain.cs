using Nebukam.Cluster;
using UnityEngine;

namespace Nebukam.Chemistry
{
    [DisallowMultipleComponent]
    public class WFCSetup2DMain : SlotGrid
    {

        public AtomConstraintsManifest manifest;
        public ISlotCluster<ConstrainedSlot, ClusterBrain> cluster;
        public SimpleSolver<ConstrainedSlot, ConstrainedSlotInfos, ClusterBrain> solver;

        // Start is called before the first frame update
        void Start()
        {
            cluster = new SlotClusterFixed<ConstrainedSlot, ClusterBrain>();
            cluster.Init(manifest.model, gridSize, true);

            solver = new SimpleSolver<ConstrainedSlot, ConstrainedSlotInfos, ClusterBrain>();
            
            solver.slotCluster = cluster;
            solver.manifest = manifest;

            solver.Schedule(0f);
        }

        void Update()
        {
            if (solver.TryComplete())
            {
                // Spawn the stuff !
            }
        }
    }

}