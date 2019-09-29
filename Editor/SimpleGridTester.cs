#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nebukam.Cluster;

namespace Nebukam.Chemistry.Ed
{

    [AddComponentMenu("Nebukam/WFC/WFC Grid Tester")]
    [DisallowMultipleComponent]
    public class SimpleGridTester : SlotGrid
    {

        public AtomConstraintsManifest manifest;
        public ISlotCluster<ConstrainedSlot> cluster = new SlotClusterFixed<ConstrainedSlot, ClusterBrain>();
        public SimpleSolver<ConstrainedSlot, ConstrainedSlotInfos> solver;
        public GameObject nullPrefab;

    }
}
#endif