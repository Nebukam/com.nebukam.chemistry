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

using Nebukam.Cluster;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Nebukam.Chemistry
{

    [BurstCompile]
    public struct QueryHandler<T_SLOT, T_SLOT_INFOS, T_BRAIN>
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
        [ReadOnly]
        public NativeArray<int> m_results;

        // Lookups
        [ReadOnly]
        public NativeHashMap<IntPair, bool> m_nullPairLookup;

        #endregion

        #region candidates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="contents"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public bool TryGetCandidates(
            int index,
            ref NativeList<Neighbor> contents,
            ref NativeList<int> candidates,
            ref NativeList<float> weights,
            out int length)
        {
            if (!TryGetSlotNeighbors(index, ref contents, out length))
                return false;

            return TryGetCandidates(ref contents, ref candidates, ref weights, out length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="contents"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public bool TryGetCandidates(
            ByteTrio coord,
            ref NativeList<Neighbor> contents,
            ref NativeList<int> candidates,
            ref NativeList<float> weights,
            out int length)
        {
            if (!TryGetSlotNeighbors(coord, ref contents, out length))
                return false;

            return TryGetCandidates(ref contents, ref candidates, ref weights, out length);
        }

        /// <summary>
        /// Attempts to build a list of all satisfactory candidates given a finite set of constraints.
        /// </summary>
        /// <param name="contents">The value of the socket at the given index</param>
        /// <param name="candidates">an empty list that will be filled</param>
        public bool TryGetCandidates(
            ref NativeList<Neighbor> contents,
            ref NativeList<int> candidates,
            ref NativeList<float> weights,
            out int length)
        {

            candidates.Clear();
            weights.Clear();

            if (contents.Length == 0)
            {
                length = 0;
                return false;
            }

            for (int m = 0; m < m_moduleCount; m++)
            {
                if (ModuleMatches(ref contents, ref m))
                {
                    candidates.Add(m);
                    weights.Add(m_modulesWeights[m]);
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
        /// <param name="contents"></param>
        public bool TryGetSlotNeighbors(
            int index,
            ref NativeList<Neighbor> contents,
            out int length)
        {

            contents.Clear();

            int3
                center = m_inputSlotInfos[index].coord,
                ccoord;

            int _s;

            // Identify the content of each neighboring socket, if any
            for (int s = 0; s < m_socketCount; s++)
            {
                ccoord = center + m_socketOffsets[s];
                m_brain.Clamp(ref ccoord);

                if (m_inputSlotCoordinateMap.TryGetValue(ccoord, out _s))
                    _s = m_results[_s];
                else
                    _s = SlotContent.NULL;

                contents.Add(new Neighbor() { socket = s, value = _s });

            }

            length = contents.Length;
            return length > 0;

        }

        /// <summary>
        /// Find the current neighbors for a given slot coordinate.
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="contents"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool TryGetSlotNeighbors(
            ByteTrio coord,
            ref NativeList<Neighbor> contents,
            out int length)
        {

            contents.Clear();
            m_brain.Clamp(ref coord);

            int3 center = coord, ccoord;
            int _s;

            if (!m_inputSlotCoordinateMap.TryGetValue(coord, out _s))
            {
                length = 0;
                return false;
            }

            // Identify the content of each neighboring socket, if any

            for (int s = 0; s < m_socketCount; s++)
            {

                ccoord = center + m_socketOffsets[s];
                m_brain.Clamp(ref ccoord);

                if (m_inputSlotCoordinateMap.TryGetValue(ccoord, out _s))
                    _s = m_results[_s];
                else
                    _s = SlotContent.NULL;

                contents.Add(new Neighbor() { socket = s, value = _s });

            }

            length = contents.Length;
            return length > 0;

        }

        public bool TryGetMatchingNeighbors(
            int index,
            int match,
            ref NativeList<int> socketIndices,
            ref NativeList<int> slotIndices,
            out int length)
        {

            socketIndices.Clear();
            slotIndices.Clear();

            int3
                center = m_inputSlotInfos[index].coord,
                ccoord;

            int _s, _c;

            // Identify the content of each neighboring socket, if any
            for (int s = 0; s < m_socketCount; s++)
            {

                ccoord = center + m_socketOffsets[s];
                m_brain.Clamp(ref ccoord);

                if (m_inputSlotCoordinateMap.TryGetValue(ccoord, out _s))
                {
                    _c = m_results[_s];
                    if (_c == match)
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

            int3
                center = m_inputSlotInfos[index].coord,
                ccoord;

            int count = 0;

            // Identify the content of each neighboring socket, if any
            for (int s = 0; s < m_socketCount; s++)
            {

                ccoord = center + m_socketOffsets[s];
                m_brain.Clamp(ref ccoord);

                if (m_inputSlotCoordinateMap.TryGetValue(ccoord, out int _s))
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
        /// <param name="moduleIndex"></param>
        /// <returns></returns>
        public bool ModuleMatches(
            ref ByteTrio coord,
            ref int moduleIndex)
        {

            Neighbor content = default;
            int3
                center = coord,
                socket;

            for (int s = 0; s < m_socketCount; s++)
            {

                content.socket = s;

                socket = center + m_socketOffsets[s];
                m_brain.Clamp(ref socket);

                if (m_inputSlotCoordinateMap.TryGetValue(socket, out int _s))
                {

                    content.value = m_results[_s];

                    if (content.IsUndefined)
                    {
                        if (RequiresNull(ref moduleIndex, ref content.socket))
                            return false;
                        else
                            continue;
                    }

                }
                else
                {
                    content.value = SlotContent.NULL;
                }

                if (!SocketContains(ref moduleIndex, ref content))
                    return false;

            }

            return true;

        }

        /// <summary>
        /// Checks if a given set of socketIndices & their respective content satisfies the constraints of a given header.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="moduleIndex"></param>
        /// <returns></returns>
        public bool ModuleMatches(
            ref NativeList<Neighbor> contents,
            ref int moduleIndex)
        {

            Neighbor content;

            for (int s = 0, count = contents.Length; s < count; s++)
            {

                content = contents[s];

                if (content.IsUndefined)
                {
                    if (RequiresNull(ref moduleIndex, ref content.socket))
                        return false;
                    else
                        continue;
                }

                if (!SocketContains(ref moduleIndex, ref content))
                    return false;

            }

            return true;

        }

        /// <summary>
        /// Return whether a module requires a NULL at a given socket
        /// </summary>
        /// <param name="moduleIndex"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public bool RequiresNull(ref int moduleIndex, ref int socket)
        {
            return m_nullPairLookup.TryGetValue(new IntPair(moduleIndex, socket), out bool b);
        }

        /// <summary>
        /// Return whether an header's socket options contains the given key
        /// </summary>
        /// <param name="socketHeader"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool SocketContains(ref int moduleIndex, ref Neighbor content)
        {

            int3 socketHeader = m_modulesHeaders[moduleIndex * m_socketCount + content.socket];

            for (int i = socketHeader.x; i < socketHeader.y; i++)
                if (m_modulesNeighbors[i] == content.value)
                    return true;

            return false;
        }

    }
}
