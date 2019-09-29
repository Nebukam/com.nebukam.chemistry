using Nebukam.Cluster;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Chemistry
{

    [CreateAssetMenu(fileName = "AtomConstraintsModel", menuName = "Nebukam/WFC/Atom Constraints Model", order = 1)]
    public class AtomConstraintsModel : SlotModelData
    {
        [Header("Neighbor config")]
        public int3[] offsets;
        public int3[] mirrors;
        public int[] mirrorsIndices;
    }

}
