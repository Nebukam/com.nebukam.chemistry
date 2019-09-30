using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Nebukam.JobAssist;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{

    [BurstCompile]
    public struct SimpleConstraintsSolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN> : IConstraintSolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN>
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
    {

        [ReadOnly]
        public Random random;

        #region IConstraintSolverJob

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
            Random rr = random;
            ByteTrio 
                center, 
                ncoord;
            T_SLOT_INFOS current;
            int 
                _s,
                sCount,
                nCount = m_neighbors.Length,
                result;

            for (int i = 0, iCount = m_inputSlotInfos.Length; i < iCount; i++ )
            {

                current = m_inputSlotInfos[i];
                center = current.coord;

                candidates.Clear();
                socketIndices.Clear();
                socketContent.Clear();

                result = Content.UNSET;
                
                // Identify the content of each neighboring socket, if any
                for (int s = 0; s < m_socketCount; s++)
                {
                    ncoord = center + m_offsets[s];

                    if (m_inputSlotCoordinateMap.TryGetValue(ncoord, out _s))
                    {
                        _s = m_results[_s];
                        //socketIndices.Add(s);
                        //socketContent.Add(_s);
                        
                        if (_s >= 0 || _s == Content.NULL)
                        {
                            socketIndices.Add(s);
                            socketContent.Add(_s);
                        }
                    }
                    else
                    {
                        // Slot neighbor is NULL
                        socketIndices.Add(s);
                        socketContent.Add(Content.NULL);
                    }

                }

                FindCandidates(ref socketIndices, ref socketContent, ref candidates);

                sCount = candidates.Length;

                if (sCount > 0)
                {
                    rr = random;
                    int r = rr.NextInt(0, sCount);
                    result = candidates[r];// (int)(r* sCount)];
                    random = rr;
                }
                else
                {
                    result = Content.NULL;
                }

                m_results[i] = result;

            }

            candidates.Dispose();
            socketIndices.Dispose();
            socketContent.Dispose();

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
