// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Nebukam.Chemistry
{

    [CreateAssetMenu(fileName = "ModuleConstraints", menuName = "N:Toolkit/Chemistry/Module Constraints", order = 1)]
    public class ModuleConstraints : ScriptableObject
    {

        public int index;
        public float weight = 1f;
        public GameObject prefab;
        public ModuleConstraintsManifest manifest;

        public int[] begin;
        public int[] lengths;
        public int[] neighbors;

        public int Count(int side) { return lengths[side]; }

        /// <summary>
        /// x = start, y = end, z = length
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public int3 Header(int side)
        {
            int start = begin[side], length = lengths[side];
            return new int3(start, start + length, length);
        }

        public void WriteIndices(int side, List<int> indices)
        {
            int3 bounds = Header(side);
            for (int i = bounds.x, count = bounds.y; i < count; i++)
                indices.Add(neighbors[i]);
        }

        public int[] Neighbors(int side)
        {
            int3 bounds = Header(side);
            int[] indices = new int[bounds.z];
            for (int i = 0, count = bounds.z; i < count; i++)
                indices[i] = neighbors[bounds.x + i];

            return indices;
        }

    }

}
