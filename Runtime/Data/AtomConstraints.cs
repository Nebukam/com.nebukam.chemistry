using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace Nebukam.Chemistry
{
    
    [CreateAssetMenu(fileName = "AtomConstraints", menuName = "Nebukam/WFC/Atom Constraints", order = 1)]
    public class AtomConstraints : ScriptableObject
    {

        [System.Serializable]
        public class Relationship
        {
            public int count = 0;
        }

        public int index;
        public int instanceCount;
        public GameObject prefab;
        public AtomConstraintsManifest manifest;

        public int[] begin;
        public int[] lengths;
        public int[] neighbors;

        public int Count(int side){ return lengths[side]; }

        /// <summary>
        /// x = start, y = end, z = length
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public IntTrio Bounds(int side)
        {
            int start = begin[side], length = lengths[side];
            return new IntTrio(start, start + length, length);
        }
        
        public void WriteIndices(int side, List<int> indices)
        {
            IntTrio bounds = Bounds(side);
            for (int i = bounds.x, count = bounds.y; i < count; i++)
                indices.Add(neighbors[i]);
        }

        public int[] Neighbors(int side)
        {
            IntTrio bounds = Bounds(side);

            int[] indices = new int[bounds.z];
            for (int i = 0, count = bounds.z; i < count; i++)
                indices[i] = neighbors[bounds.x + i];

            return indices;
        }

    }

}
