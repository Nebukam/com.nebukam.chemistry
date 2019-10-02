using Nebukam.Cluster;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Chemistry
{

    [CreateAssetMenu(fileName = "AtomConstraintsModel", menuName = "Nebukam/WFC/Atom Constraints Model", order = 1)]
    public class AtomConstraintsModel : SlotModelData
    {
        [Header("Neighbor config")]
        public int3[] sockets;
        public int3[] socketMirrors;
        public int[] socketMirrorIndices;
    }

}
