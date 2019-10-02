using Nebukam.Cluster;
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
        public NativeArray<int3> m_headerIndices;
        [ReadOnly]
        public NativeArray<int> m_neighbors;

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
        public NativeArray<int3> headerIndices { set { m_headerIndices = value; } }
        public NativeArray<int> neighbors { set { m_neighbors = value; } }
        public NativeArray<int> results { set { m_results = value; } }

        #endregion

        public void Execute()
        {

            NativeList<int> candidates = new NativeList<int>(m_neighbors.Length, Allocator.Temp);
            NativeList<int> socketIndices = new NativeList<int>(m_socketCount, Allocator.Temp);
            NativeList<int> socketContent = new NativeList<int>(m_socketCount, Allocator.Temp);
            NativeList<int> unsolvables = new NativeList<int>(0, Allocator.Temp);
            Random rr = m_random;
            int
                sCount,
                nCount = m_neighbors.Length,
                result;

            for (int i = 0, iCount = m_inputSlotInfos.Length; i < iCount; i++)
            {

                FindCurrentNeighbors(i, ref socketIndices, ref socketContent, ref candidates);
                FindCandidates(ref socketIndices, ref socketContent, ref candidates);

                result = SlotContent.UNSET;
                sCount = candidates.Length;

                if (sCount > 0)
                {
                    rr = m_random;
                    int r = rr.NextInt(0, sCount);
                    result = candidates[r];
                    m_random = rr;
                }
                else
                {
                    result = SlotContent.NULL;
                    unsolvables.Add(i);
                }

                m_results[i] = result;

            }

            // Second pass if necessary
            if (unsolvables.Length != 0)
            {

                int index = 0;

                for (int i = 0, iCount = unsolvables.Length; i < iCount; i++)
                {
                    index = unsolvables[i];

                    FindCurrentNeighbors(index, ref socketIndices, ref socketContent, ref candidates);
                    FindCandidates(ref socketIndices, ref socketContent, ref candidates);

                    result = SlotContent.UNSET;
                    sCount = candidates.Length;

                    if (sCount > 0)
                    {
                        rr = m_random;
                        int r = rr.NextInt(0, sCount);
                        result = candidates[r];
                        m_random = rr;
                    }
                    else
                    {
                        result = SlotContent.NULL;
                    }

                    m_results[index] = result;

                }
            }

            candidates.Dispose();
            socketIndices.Dispose();
            socketContent.Dispose();
            unsolvables.Dispose();

        }

        /// <summary>
        /// Find the current neighbors for a given slot index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="socketIndices"></param>
        /// <param name="socketContents"></param>
        /// <param name="candidates"></param>
        private void FindCurrentNeighbors(
            int index,
            ref NativeList<int> socketIndices,
            ref NativeList<int> socketContents,
            ref NativeList<int> candidates)
        {

            T_SLOT_INFOS current = m_inputSlotInfos[index];
            ByteTrio
                center = current.coord,
                ncoord;

            int _s;

            candidates.Clear();
            socketIndices.Clear();
            socketContents.Clear();

            // Identify the content of each neighboring socket, if any
            for (int s = 0; s < m_socketCount; s++)
            {
                ncoord = center + m_offsets[s];

                if (m_inputSlotCoordinateMap.TryGetValue(ncoord, out _s))
                {
                    _s = m_results[_s];
                    //socketIndices.Add(s);
                    //socketContent.Add(_s);

                    if (_s >= 0 || _s == SlotContent.NULL)
                    {
                        socketIndices.Add(s);
                        socketContents.Add(_s);
                    }
                }
                else
                {
                    // Slot neighbor is NULL
                    socketIndices.Add(s);
                    socketContents.Add(SlotContent.NULL);
                }

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socketIndices">The index of the socket</param>
        /// <param name="socketContents">The value of the socket at the given index</param>
        /// <param name="candidates">an empty list that will be filled</param>
        private void FindCandidates(
            ref NativeList<int> socketIndices,
            ref NativeList<int> socketContents,
            ref NativeList<int> candidates)
        {

            int3 header;
            bool valid = false;
            int
                nullSocketCount = socketIndices.Length,
                socket,
                content,
                _h;

            for (int h = 0; h < m_headerCount; h++)
            {

                _h = h * m_socketCount; //Base index

                for (int i = 0; i < nullSocketCount; i++)
                {

                    content = socketContents[i];

                    socket = socketIndices[i];

                    header = m_headerIndices[_h + socket];

                    valid = false;

                    // Check if that socket's candidates has matching content
                    for (int n = header.x; n < header.y; n++)
                    {
                        if (m_neighbors[n] == content)
                        {
                            valid = true;
                            break;
                        }
                    }

                    if (!valid)
                        break;

                }

                if (valid)
                {
                    // atom is legit candidate !
                    candidates.Add(h);
                }

            }

        }

    }
}
