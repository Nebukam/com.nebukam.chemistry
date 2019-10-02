using UnityEngine;
using Nebukam.Cluster;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.Chemistry
{
    [DisallowMultipleComponent]
    public class WFCSetup3DMain : SlotGrid
    {

        public AtomConstraintsManifest manifest;
        public ISlotCluster<ConstrainedSlot, ClusterBrain> cluster;
        public SimpleSolver<ConstrainedSlot, ConstrainedSlotInfos, ClusterBrain> solver;
        public GameObject nullPrefab;
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

        protected virtual void ClearChilds()
        {
            Transform tr = transform;
            int count = tr.childCount;
            Transform[] childs = new Transform[count];

            for (int i = 0; i < count; i++)
                childs[i] = tr.GetChild(i);

            for (int i = 0; i < count; i++)
                Destroy(childs[i].gameObject);
        }

        void Update()
        {
            if (solver.TryComplete())
            {

                ClearChilds();
                cluster.size = gridSize;
                cluster.Fill();

                NativeArray<int> results = solver.constraintSolver.results;
                int index, count = min(results.Length, cluster.Count);
                Quaternion r;
                AtomConstraints atom;
                ISlot slot;
                GameObject go;
                IClusterBrain brain = cluster.brain;
                ByteTrio coord;

                for (int i = 0; i < count; i++)
                {

                    index = results[i];

                    if (index < 0)
                        continue;

                    atom = manifest.infos[index];

                    if (atom.prefab == nullPrefab)
                        continue;

                    slot = cluster[i];
                    coord = slot.coordinates;

                    r = Quaternion.Euler(0f, (float)(UnityEngine.Random.Range(0, 4) * 90), 0f);
                    go = GameObject.Instantiate(atom.prefab, brain.ComputePosition(ref coord), r, transform) as GameObject;

                }

                solver.seed = (uint)UnityEngine.Random.Range(0, 10000);
                solver.Schedule(0f);

            }
        }
        
    }

}