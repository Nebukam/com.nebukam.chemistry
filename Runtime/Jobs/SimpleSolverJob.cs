// Copyright (c) 2019 Timothé Lapetite - nebukam@gmail.com.
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

using Nebukam.Cluster;
using Nebukam.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Nebukam.Chemistry
{

    [BurstCompile]
    public struct SimpleSolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN> : ISolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN>
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
    {

        #region IConstraintSolverJob

        [ReadOnly]
        public Random m_random;

        // Cluster infos
        [ReadOnly]
        public T_BRAIN m_brain;
        [ReadOnly]
        public NativeArray<T_SLOT_INFOS> m_inputSlotInfos;
        [ReadOnly]
        public NativeHashMap<ByteTrio, int> m_inputSlotCoordinateMap;

        // Model infos
        [ReadOnly]
        public int m_socketCount;
        [ReadOnly]
        public NativeArray<int3> m_offsets;
        [ReadOnly]
        public NativeArray<int3> m_mirrors;
        [ReadOnly]
        public NativeArray<int> m_mirrorsIndices;

        // Manifest infos
        [ReadOnly]
        public int m_headerCount;
        [ReadOnly]
        public NativeArray<float> m_headerWeights;
        [ReadOnly]
        public NativeArray<int3> m_headerIndices;
        [ReadOnly]
        public NativeArray<int> m_neighbors;

        // Lookups
        [ReadOnly]
        public NativeHashMap<IntPair, bool> m_nullPairLookup;

        public NativeArray<int> m_results;

        // Cluster infos
        public Random random { set { m_random = value; } }
        public T_BRAIN brain { set { m_brain = value; } }
        public NativeArray<T_SLOT_INFOS> inputSlotInfos { set { m_inputSlotInfos = value; } }
        public NativeHashMap<ByteTrio, int> inputSlotCoordinateMap { set { m_inputSlotCoordinateMap = value; } }

        // Model infos
        public int socketCount { set { m_socketCount = value; } }
        public NativeArray<int3> offsets { set { m_offsets = value; } }
        public NativeArray<int3> mirrors { set { m_mirrors = value; } }
        public NativeArray<int> mirrorsIndices { set { m_mirrorsIndices = value; } }

        // Manifest infos
        public int headerCount { set { m_headerCount = value; } }
        public NativeArray<float> headerWeights { set { m_headerWeights = value; } }
        public NativeArray<int3> headerIndices { set { m_headerIndices = value; } }
        public NativeArray<int> neighbors { set { m_neighbors = value; } }
        public NativeArray<int> results { set { m_results = value; } }

        // Lookups
        public NativeHashMap<IntPair, bool> nullPairLookup { set { m_nullPairLookup = value; } }

        public NativeArray<float> m_debug;
        public NativeArray<float> debug { set { m_debug = value; } }

        #endregion

        public void Execute()
        {

            #region QueryHandle

            QueryHandle<T_SLOT, T_SLOT_INFOS, T_BRAIN> Q =
                new QueryHandle<T_SLOT, T_SLOT_INFOS, T_BRAIN>()
                {
                    m_brain = m_brain,
                    m_inputSlotInfos = m_inputSlotInfos,
                    m_inputSlotCoordinateMap = m_inputSlotCoordinateMap,
                    m_socketCount = m_socketCount,
                    m_offsets = m_offsets,
                    m_mirrors = m_mirrors,
                    m_mirrorsIndices = m_mirrorsIndices,
                    m_headerCount = m_headerCount,
                    m_headerWeights = m_headerWeights,
                    m_headerIndices = m_headerIndices,
                    m_neighbors = m_neighbors,
                    m_results = m_results,
                    m_nullPairLookup = m_nullPairLookup
                };

            #endregion

            NativeList<int>
                socketIndices = new NativeList<int>(m_socketCount, Allocator.Temp),
                socketContent = new NativeList<int>(m_socketCount, Allocator.Temp),
                candidates = new NativeList<int>(m_neighbors.Length, Allocator.Temp),
                unsolvables = new NativeList<int>(10, Allocator.Temp);

            NativeList<float>
                weights = new NativeList<float>(m_headerCount, Allocator.Temp);

            int
                sCount = m_inputSlotInfos.Length,
                nCount = m_neighbors.Length,
                cCount,
                result;

            for (int slotIndex = 0; slotIndex < sCount; slotIndex++)
            {

                if (m_results[slotIndex] >= 0)
                    continue;

                if (Q.TryGetCandidates(
                    slotIndex, 
                    ref socketIndices, 
                    ref socketContent, 
                    ref candidates, 
                    ref weights, 
                    out cCount))
                {
                    //result = candidates[NextInt(ref cCount)];
                    result = candidates[NRandom.GetRandomWeightedIndex(ref weights, NextFloat())];
                }
                else
                {
                    result = SlotContent.UNSOLVABLE;
                    unsolvables.Add(slotIndex);
                }

                m_results[slotIndex] = result;

            }

            // Second pass if necessary
            if (unsolvables.Length != 0)
            {
                int index = 0;

                for (int u = 0, uCount = unsolvables.Length; u < uCount; u++)
                {
                    index = unsolvables[u];

                    if (Q.TryGetCandidates(
                        index, 
                        ref socketIndices, 
                        ref socketContent, 
                        ref candidates, 
                        ref weights, 
                        out cCount))
                        result = candidates[NextInt(ref cCount)];
                    else
                        result = SlotContent.UNSOLVABLE;

                    m_results[index] = result;
                }
            }

            socketIndices.Dispose();
            socketContent.Dispose();
            candidates.Dispose();
            unsolvables.Dispose();
            weights.Dispose();

        }

        #region utils

        /// <summary>
        /// Return a random int from the random object.
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        private int NextInt(ref int max)
        {
            Random randomCopy = m_random;
            int result = randomCopy.NextInt(0, max);
            m_random = randomCopy;
            return result;
        }

        /// <summary>
        /// Return a random float from the random object.
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        private float NextFloat(ref float max)
        {
            Random randomCopy = m_random;
            float result = randomCopy.NextFloat(0f, max);
            m_random = randomCopy;
            return result;
        }


        /// <summary>
        /// Return a random float from the random object.
        /// </summary>
        /// <returns></returns>
        private float NextFloat()
        {
            Random randomCopy = m_random;
            float result = randomCopy.NextFloat();
            m_random = randomCopy;
            return result;
        }


        #endregion

    }
}
