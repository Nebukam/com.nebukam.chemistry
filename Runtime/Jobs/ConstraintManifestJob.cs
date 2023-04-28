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

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Nebukam.Chemistry
{

    /// <summary>
    /// Precompute a bunch of useful data and lookup table to ease the Solver's job.
    /// </summary>
    [BurstCompile]
    public struct ConstraintManifestJob : IJob
    {

        // Model infos
        [ReadOnly]
        public int m_socketCount;

        // Manifest infos
        [ReadOnly]
        public int m_moduleCount;
        [ReadOnly]
        public NativeArray<int3> m_modulesHeaders;
        [ReadOnly]
        public NativeArray<int> m_modulesNeighbors;

        public NativeParallelHashMap<IntPair, bool> m_nullPairLookup;
        //public NativeList<int> m_nonNullHeaders;

        public void Execute()
        {

            m_nullPairLookup.Clear();

            int3 header;
            int _h;
            bool _n = true;

            // For each header...
            for(int headerIndex = 0; headerIndex < m_moduleCount; headerIndex++)
            {

                _h = headerIndex * m_socketCount;
                _n = false;

                // Check all accepted values, per-socket...
                for (int socketIndex = 0; socketIndex < m_socketCount; socketIndex++)
                {

                    header = m_modulesHeaders[_h + socketIndex];

                    // header : socket @ true
                    if (header.z == 1 && m_modulesNeighbors[header.x] == SlotContent.NULL)
                    {
                        m_nullPairLookup.TryAdd(new IntPair(headerIndex, socketIndex), true);
                        _n = true;
                    }

                }

                if (!_n)
                {
                    //    m_nonNullHeaders.Add(headerIndex);
                }

            }

        }

    }
}
