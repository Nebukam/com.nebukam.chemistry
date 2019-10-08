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
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Nebukam.Chemistry
{

    [BurstCompile]
    public struct QueryHandle<T_SLOT, T_SLOT_INFOS, T_BRAIN>
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
    {

        #region Properties

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

        public NativeArray<int> m_results;

        // Lookups
        [ReadOnly]
        public NativeHashMap<IntPair, bool> m_nullPairLookup;

        #endregion

        #region compute constants

        /// <summary>
        /// Isolate all headers which any socket have at least more than one constraint
        /// </summary>
        /// <param name="candidates"></param>
        public void ComputeMinConstraintCandidates(
            ref NativeList<int> candidates)
        {

        }

        #endregion

        #region candidates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="socketIndices"></param>
        /// <param name="socketContents"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public bool TryGetCandidates(
            int index,
            ref NativeList<int> socketIndices,
            ref NativeList<int> socketContents,
            ref NativeList<int> candidates,
            ref NativeList<float> weights,
            out int length)
        {
            if (!TryGetSlotNeighbors(index, ref socketIndices, ref socketContents, out length))
            {
                return false;
            }

            return TryGetCandidates(ref socketIndices, ref socketContents, ref candidates, ref weights, out length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="socketIndices"></param>
        /// <param name="socketContents"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public bool TryGetCandidates(
            ByteTrio coord,
            ref NativeList<int> socketIndices,
            ref NativeList<int> socketContents,
            ref NativeList<int> candidates,
            ref NativeList<float> weights,
            out int length)
        {
            if (!TryGetSlotNeighbors(coord, ref socketIndices, ref socketContents, out length))
                return false;

            return TryGetCandidates(ref socketIndices, ref socketContents, ref candidates, ref weights, out length);
        }

        /// <summary>
        /// Attempts to build a list of all satisfactory candidates given a finite set of constraints.
        /// </summary>
        /// <param name="socketIndices">The index of the socket</param>
        /// <param name="socketContents">The value of the socket at the given index</param>
        /// <param name="candidates">an empty list that will be filled</param>
        public bool TryGetCandidates(
            ref NativeList<int> socketIndices,
            ref NativeList<int> socketContents,
            ref NativeList<int> candidates,
            ref NativeList<float> weights,
            out int length)
        {

            candidates.Clear();
            weights.Clear();

            if (socketIndices.Length == 0)
            {
                length = 0;
                return false;
            }

            for (int h = 0; h < m_headerCount; h++)
            {
                if (IsSatisfactory(ref socketIndices, ref socketContents, ref h))
                {
                    candidates.Add(h);
                    weights.Add(m_headerWeights[h]);
                }
            }

            length = candidates.Length;
            return length > 0;
        }

        #endregion

        #region neighbors

        /// <summary>
        /// Find the current neighbors for a given slot index.
        /// Ignore UNSET & EMPTY contents.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="socketIndices"></param>
        /// <param name="socketContents"></param>
        public bool TryGetSlotNeighbors(
            int index,
            ref NativeList<int> socketIndices,
            ref NativeList<int> socketContents,
            out int length)
        {

            socketIndices.Clear();
            socketContents.Clear();

            ByteTrio center = m_inputSlotInfos[index].coord;
            int _s;

            // Identify the content of each neighboring socket, if any
            for (int s = 0; s < m_socketCount; s++)
            {

                if (m_inputSlotCoordinateMap.TryGetValue(center + m_offsets[s], out _s))
                    _s = m_results[_s];
                else
                    _s = SlotContent.NULL;

                socketIndices.Add(s);
                socketContents.Add(_s);

            }

            length = socketIndices.Length;
            return length > 0;

        }

        /// <summary>
        /// Find the current neighbors for a given slot coordinate.
        /// Ignore UNSET & EMPTY contents.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="socketIndices"></param>
        /// <param name="socketContents"></param>
        public bool TryGetSlotNeighbors(
            ByteTrio coord,
            ref NativeList<int> socketIndices,
            ref NativeList<int> socketContents,
            out int length)
        {

            socketIndices.Clear();
            socketContents.Clear();

            int _s;

            // Identify the content of each neighboring socket, if any
            for (int s = 0; s < m_socketCount; s++)
            {

                if (m_inputSlotCoordinateMap.TryGetValue(coord + m_offsets[s], out _s))
                    _s = m_results[_s];
                else
                    _s = SlotContent.NULL;

                socketIndices.Add(s);
                socketContents.Add(_s);

            }

            length = socketIndices.Length;
            return length > 0;

        }

        public bool TryGetUnsetNeighbors(
            int index,
            ref NativeList<int> socketIndices,
            ref NativeList<int> slotIndices,
            out int length)
        {

            socketIndices.Clear();
            slotIndices.Clear();

            // Compute the list of currently UNSET neighbors for the given slot index
            // This should just be a list of socket indices.
            // i.e if all slots are empty, result == m_socketIndices

            ByteTrio center = m_inputSlotInfos[index].coord;
            int _s, _c;

            // Identify the content of each neighboring socket, if any
            for (int s = 0; s < m_socketCount; s++)
            {
                if (m_inputSlotCoordinateMap.TryGetValue(center + m_offsets[s], out _s))
                {
                    _c = m_results[_s];
                    if (_c == SlotContent.UNSET)
                    {
                        socketIndices.Add(s);
                        slotIndices.Add(_s);
                    }
                }
            }

            length = socketIndices.Length;
            return length > 0;
        }

        public float GetNeighborsResolutionRatio(int index)
        {

            ByteTrio center = m_inputSlotInfos[index].coord;
            int _s, count = 0;

            // Identify the content of each neighboring socket, if any
            for (int s = 0; s < m_socketCount; s++)
            {
                if (m_inputSlotCoordinateMap.TryGetValue(center + m_offsets[s], out _s))
                {
                    if (m_results[_s] != SlotContent.UNSET)
                        count++;
                }
                else
                {
                    count++;
                }
            }

            return count / (float)m_socketCount;
        }


        #endregion

        /// <summary>
        /// Checks if a given coordinate satisfies a given header.
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="headerIndex"></param>
        /// <returns></returns>
        public bool IsSatisfactory(
            ref ByteTrio coord,
            ref int headerIndex)
        {

            ByteTrio socket;
            int3 headerSocket;
            int
                _h = headerIndex * m_socketCount,
                _s;

            for (int s = 0; s < m_socketCount; s++)
            {

                socket = coord + m_offsets[s];

                if (m_inputSlotCoordinateMap.TryGetValue(socket, out _s))
                {
                    _s = m_results[_s];

                    // Consider UNSET & EMPTY as satisfactory.
                    if (_s == SlotContent.UNSET)
                    {
                        // socket only accepts NULL ?
                        if (m_nullPairLookup.TryGetValue(new IntPair(headerIndex, s), out bool b))
                            return false;

                        continue;
                    }

                }
                else
                {
                    _s = SlotContent.NULL;
                }

                headerSocket = m_headerIndices[_h + s];

                if (!SocketContains(ref headerSocket, ref _s))
                    return false;

            }

            return true;

        }

        /// <summary>
        /// Checks if a given set of socketIndices & their respective content satisfies the constraints of a given header.
        /// </summary>
        /// <param name="headerIndex"></param>
        /// <param name="socketIndices"></param>
        /// <param name="socketContents"></param>
        /// <returns></returns>
        public bool IsSatisfactory(
            ref NativeList<int> socketIndices,
            ref NativeList<int> socketContents,
            ref int headerIndex)
        {

            int3 headerSocket;
            int
                sCount = socketIndices.Length,
                _h = headerIndex * m_socketCount,
                _content, _s;

            for (int s = 0; s < sCount; s++)
            {
                _s = socketIndices[s];
                _content = socketContents[s];

                if (_content == SlotContent.UNSET)
                {
                    // socket only accepts NULL ?
                    if (m_nullPairLookup.TryGetValue(new IntPair(headerIndex, _s), out bool b))
                        return false;

                    continue;
                }

                headerSocket = m_headerIndices[_h + _s];

                if (!SocketContains(ref headerSocket, ref _content))
                    return false;

            }

            return true;

        }

        /// <summary>
        /// Return whether an header's socket options contains the given key
        /// </summary>
        /// <param name="headerSocket"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool SocketContains(ref int3 headerSocket, ref int key)
        {

            for (int i = headerSocket.x; i < headerSocket.y; i++)
                if (m_neighbors[i] == key)
                    return true;

            return false;
        }

    }
}
