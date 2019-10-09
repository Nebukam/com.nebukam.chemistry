using Nebukam.Cluster;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Nebukam.Chemistry
{
    [DisallowMultipleComponent]
    public abstract class ModuleTesterBehaviour<T_CLUSTER, T_SLOT, T_SLOT_INFOS, T_BRAIN> : ModuleClusterBehaviour
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
        where T_CLUSTER : class, ISlotCluster<T_SLOT, T_BRAIN>, new()
    {

        public ModuleConstraintsManifest manifest;

        [Header("Spawn settings")]
        public GameObject nullPrefab;
        public GameObject unsolvablePrefab;
        public bool applyRandomRotation = true;

        public T_CLUSTER cluster;
        public ISolverBundle solver;

        // Start is called before the first frame update
        void Start()
        {
            cluster = new T_CLUSTER();
            cluster.Init(manifest.model, gridSize, true);

            solver = Solver(cluster);

            solver.manifest = manifest;
            solver.Schedule(0f);
        }

        protected abstract ISolverBundle Solver(T_CLUSTER cluster); //solver.slotCluster = cluster;

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

                NativeArray<int> results = solver.results;
                int index, count = min(results.Length, cluster.Count);
                GameObject go, prefab;
                Quaternion r;
                ISlot slot;
                ByteTrio coord;
                float3 origin = transform.position;

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

                    r = applyRandomRotation ? Quaternion.Euler(0f, UnityEngine.Random.Range(0, 4) * 90, 0f) : Quaternion.identity;
                    go = GameObject.Instantiate(prefab, origin + cluster.brain.ComputePosition(ref coord), r, transform) as GameObject;

                }

                solver.seed = (uint)UnityEngine.Random.Range(0, 10000);
                solver.Schedule(0f);

            }
        }

    }

}
