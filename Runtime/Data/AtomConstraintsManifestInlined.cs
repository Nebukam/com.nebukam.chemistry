using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace Nebukam.Chemistry
{

    [CreateAssetMenu(fileName = "AtomConstraintsManifestInlined", menuName = "Nebukam/WFC/Atom Constraints Manifest Inlined", order = 1)]
    public class AtomConstraintsManifestInlined : ScriptableObject
    {
        public int headerCount;
        public int headerLength;
        public int3[] headerIndices; // x = begin, y = end, z = length
        public int[] neighbors; // All the data.
    }

}
