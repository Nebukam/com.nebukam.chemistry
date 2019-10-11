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
using Nebukam.JobAssist;
using Nebukam.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Nebukam.Chemistry
{

    [BurstCompile]
    public struct LowestAntropySolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN> : ISolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN>
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
        public NativeArray<int3> m_socketOffsets;
        [ReadOnly]
        public NativeArray<int3> m_socketsMirrors;
        [ReadOnly]
        public NativeArray<int> m_socketsMirrorsIndices;

        // Manifest infos
        [ReadOnly]
        public int m_moduleCount;
        [ReadOnly]
        public NativeArray<float> m_modulesWeights;
        [ReadOnly]
        public NativeArray<int3> m_modulesHeaders;
        [ReadOnly]
        public NativeArray<int> m_modulesNeighbors;

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
        public NativeArray<int3> socketsOffsets { set { m_socketOffsets = value; } }
        public NativeArray<int3> socketsMirrors { set { m_socketsMirrors = value; } }
        public NativeArray<int> socketsMirrorsIndices { set { m_socketsMirrorsIndices = value; } }

        // Manifest infos
        public int moduleCount { set { m_moduleCount = value; } }
        public NativeArray<float> modulesWeights { set { m_modulesWeights = value; } }
        public NativeArray<int3> modulesHeaders { set { m_modulesHeaders = value; } }
        public NativeArray<int> modulesNeighbors { set { m_modulesNeighbors = value; } }
        public NativeArray<int> results { set { m_results = value; } }

        // Lookups
        public NativeHashMap<IntPair, bool> nullPairLookup { set { m_nullPairLookup = value; } }

        public NativeArray<float> m_debug;
        public NativeArray<float> debug { set { m_debug = value; } }

        #endregion

        [BurstCompile]
        public struct Antropy
        {
            public ByteTrio coord;
            public int index;
            public int candidates;
            public float progress;
        }

        [BurstCompile]
        public struct SortAntropy : System.Collections.Generic.IComparer<Antropy>
        {
            public int Compare(Antropy a, Antropy b)
            {

                if (a.coord.y < b.coord.y)
                    return 1;

                if (a.candidates - b.candidates < 0)
                    return 1;

                float progress = a.progress - b.progress;
                return progress > 0f ? 1 : -1;
            }
        }

        public void Execute()
        {

            #region QueryHandle

            QueryHandler<T_SLOT, T_SLOT_INFOS, T_BRAIN> Q =
                new QueryHandler<T_SLOT, T_SLOT_INFOS, T_BRAIN>()
                {
                    m_brain = m_brain,
                    m_inputSlotInfos = m_inputSlotInfos,
                    m_inputSlotCoordinateMap = m_inputSlotCoordinateMap,
                    m_socketCount = m_socketCount,
                    m_socketOffsets = m_socketOffsets,
                    m_socketsMirrors = m_socketsMirrors,
                    m_socketsMirrorsIndices = m_socketsMirrorsIndices,
                    m_moduleCount = m_moduleCount,
                    m_modulesWeights = m_modulesWeights,
                    m_modulesHeaders = m_modulesHeaders,
                    m_modulesNeighbors = m_modulesNeighbors,
                    m_results = m_results,
                    m_nullPairLookup = m_nullPairLookup
                };

            #endregion

            int
                sCount = m_inputSlotInfos.Length,
                nCount = m_modulesNeighbors.Length,
                count,
                index,
                result;

            Antropy _a;
            SortAntropy antropyComparer = default;

            NativeList<Neighbor>
                contents = new NativeList<Neighbor>(m_socketCount, Allocator.Temp);

            NativeList<int>
                candidates = new NativeList<int>(m_modulesNeighbors.Length, Allocator.Temp),
                slotSocketIndices = new NativeList<int>(m_socketCount, Allocator.Temp),
                slotIndices = new NativeList<int>(m_socketCount, Allocator.Temp);

            NativeList<float>
                weights = new NativeList<float>(m_moduleCount, Allocator.Temp);

            NativeList<Antropy> 
                open = new NativeList<Antropy>(sCount, Allocator.Temp);

            #region compute initial antropy

            for (int slotIndex = 0; slotIndex < sCount; slotIndex++)
            {
                if (m_results[slotIndex] < 0 && 
                    Q.TryGetCandidates(
                        slotIndex, 
                        ref contents, 
                        ref candidates, 
                        ref weights, 
                        out count))
                {
                    _a = new Antropy()
                    {
                        coord = m_inputSlotInfos[slotIndex].coord,
                        index = slotIndex,
                        candidates = count,
                        progress = Q.GetNeighborsResolutionRatio(slotIndex)
                    };

                    if (open.Length != 0)
                    {
                        if (antropyComparer.Compare(_a, open[0]) >= 0)
                            open[0] = _a;                            
                    }
                    else
                    {
                        open.Add(_a);
                    }

                }
            }

            if (open.Length != 0)
                open.Sort(antropyComparer);
            else
                open.Add(new Antropy() { index = 0, candidates = m_moduleCount });

            // ISSUE : When resolving candidates, some header gets selected because a neighboring socket is UNSET, and thus ignored
            // while the header only accepts NULL for that socket.

            #endregion

            #region collapse 

            int increment = 0;
            float ratio;

            while (open.Length > 0)
            {
                _a = open.Pop();
                index = _a.index;
                m_debug[index] = increment / (float)sCount;

                // Collapse

                if (Q.TryGetCandidates(
                    index, 
                    ref contents, 
                    ref candidates, 
                    ref weights, 
                    out count))
                {
                    result = candidates[ NRandom.GetRandomWeightedIndex(ref weights, NextFloat()) ];
                }
                else 
                {
                    // Uh oh, no candidate found !
                    result = SlotContent.UNSOLVABLE;
                }

                m_results[index] = result;

                // Recalculate antropy of neighboring slots that are currently unset

                if (Q.TryGetMatchingNeighbors(
                    index, SlotContent.UNSET,
                    ref slotSocketIndices, 
                    ref slotIndices, 
                    out count))
                {
                    for (int s = 0; s < count; s++)
                    {
                        index = slotIndices[s];
                        if (Q.TryGetCandidates(
                            index, 
                            ref contents, 
                            ref candidates, 
                            ref weights, 
                            out int antropy))
                        {
                            ratio = Q.GetNeighborsResolutionRatio(index);
                            UpdateAntropy(ref index, ref antropy, ref ratio, ref open);
                        }
                    }
                }

                // Sort open slots which antropy is computed

                open.Sort(antropyComparer);
                increment++;
            }

            #endregion

            open.Dispose();
            contents.Dispose();
            candidates.Dispose();
            slotSocketIndices.Dispose();
            slotIndices.Dispose();
            weights.Dispose();
        }

        private void UpdateAntropy(
            ref int index,
            ref int antropy,
            ref float ratio,
            ref NativeList<Antropy> open)
        {

            Antropy a;
            for (int i = 0, count = open.Length; i < count; i++)
            {
                a = open[i];
                if (a.index == index)
                {
                    a.candidates = antropy;
                    a.progress = ratio;
                    open[i] = a;
                    return;
                }
            }

            open.Add(new Antropy()
            {
                coord = m_inputSlotInfos[index].coord,
                index = index,
                candidates = antropy,
                progress = ratio
            });

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
