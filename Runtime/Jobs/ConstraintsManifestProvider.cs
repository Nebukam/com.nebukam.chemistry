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


using Nebukam.JobAssist;
using Unity.Collections;
using Unity.Mathematics;

namespace Nebukam.Chemistry
{

    public interface IConstraintsManifestProvider : IProcessor
    {

        ModuleConstraintsManifest manifest { get; set; }

        NativeArray<int3> socketsOffsets { get; }
        NativeArray<int3> socketsMirrors { get; }
        NativeArray<int> socketsMirrorsIndices { get; }

        NativeArray<float> modulesWeights { get; }
        NativeArray<int3> modulesHeaders { get; }
        NativeArray<int> modulesNeighbors { get; }

        NativeHashMap<IntPair, bool> nullPairLookup { get; }

    }

    public class ConstraintsManifestProvider : Processor<ConstraintManifestJob>, IConstraintsManifestProvider
    {

        protected ModuleConstraintsManifest m_manifest = null;

        protected NativeArray<int3> m_socketsOffsets = new NativeArray<int3>(0, Allocator.Persistent);
        protected NativeArray<int3> m_socketsMirrors = new NativeArray<int3>(0, Allocator.Persistent);
        protected NativeArray<int> m_socketsMirrorsIndices = new NativeArray<int>(0, Allocator.Persistent);

        protected NativeArray<float> m_modulesWeights = new NativeArray<float>(0, Allocator.Persistent);
        protected NativeArray<int3> m_modulesHeaders = new NativeArray<int3>(0, Allocator.Persistent);
        protected NativeArray<int> m_modulesNeighbors = new NativeArray<int>(0, Allocator.Persistent);

        protected NativeHashMap<IntPair, bool> m_nullPairLookup = new NativeHashMap<IntPair, bool>(0, Allocator.Persistent);



        public ModuleConstraintsManifest manifest { get { return m_manifest; } set { m_manifest = value; } }

        public NativeArray<int3> socketsOffsets { get { return m_socketsOffsets; } }
        public NativeArray<int3> socketsMirrors { get { return m_socketsMirrors; } }
        public NativeArray<int> socketsMirrorsIndices { get { return m_socketsMirrorsIndices; } }

        public NativeArray<float> modulesWeights { get { return m_modulesWeights; } }
        public NativeArray<int3> modulesHeaders { get { return m_modulesHeaders; } }
        public NativeArray<int> modulesNeighbors { get { return m_modulesNeighbors; } }

        // key = header index : socket index
        public NativeHashMap<IntPair, bool> nullPairLookup { get { return m_nullPairLookup; } }

        protected override void Prepare(ref ConstraintManifestJob job, float delta)
        {

            #region model infos

            ModuleConstraintsModel model = m_manifest.model;
            int socketCount = model.socketsOffsets.Length;

            if (m_socketsOffsets.Length != socketCount)
            {
                m_socketsOffsets.Dispose();
                m_socketsMirrors.Dispose();
                m_socketsMirrorsIndices.Dispose();

                m_socketsOffsets = new NativeArray<int3>(socketCount, Allocator.Persistent);
                m_socketsMirrors = new NativeArray<int3>(socketCount, Allocator.Persistent);
                m_socketsMirrorsIndices = new NativeArray<int>(socketCount, Allocator.Persistent);
            }

            for (int i = 0; i < socketCount; i++)
            {
                m_socketsOffsets[i] = model.socketsOffsets[i];
                m_socketsMirrors[i] = model.socketsMirrors[i];
                m_socketsMirrorsIndices[i] = model.socketMirrorIndices[i];
            }

            #endregion

            #region manifest

            ModuleConstraintsManifestInlined manifestInlined = manifest.inlinedManifest;
            ModuleConstraints[] infos = manifest.infos;

            int3[] headerList = manifestInlined.modulesHeaders;
            int[] neighborsList = manifestInlined.modulesNeighbors;
            int
                moduleCount = manifest.moduleCount,
                headerCount = headerList.Length,
                neighborsCount = neighborsList.Length;

            if (m_modulesHeaders.Length != headerCount)
            {
                m_modulesWeights.Dispose();
                m_modulesHeaders.Dispose();
                m_modulesNeighbors.Dispose();
                m_nullPairLookup.Dispose();

                m_modulesWeights = new NativeArray<float>(moduleCount, Allocator.Persistent);
                m_modulesHeaders = new NativeArray<int3>(headerCount, Allocator.Persistent);
                m_modulesNeighbors = new NativeArray<int>(neighborsCount, Allocator.Persistent);
                m_nullPairLookup = new NativeHashMap<IntPair, bool>(moduleCount * socketCount, Allocator.Persistent);
            }

            for (int i = 0; i < moduleCount; i++)
                m_modulesWeights[i] = infos[i].weight;

            for (int i = 0; i < headerCount; i++)
                m_modulesHeaders[i] = headerList[i];

            for (int i = 0; i < neighborsCount; i++)
                m_modulesNeighbors[i] = neighborsList[i];

            job.m_socketCount = socketCount;
            job.m_moduleCount = manifest.moduleCount;

            job.m_modulesHeaders = m_modulesHeaders;
            job.m_modulesNeighbors = m_modulesNeighbors;

            job.m_nullPairLookup = m_nullPairLookup;

            #endregion

        }

        protected override void InternalDispose()
        {
            m_manifest = null;

            m_socketsOffsets.Dispose();
            m_socketsMirrors.Dispose();
            m_socketsMirrorsIndices.Dispose();

            m_modulesHeaders.Dispose();
            m_modulesNeighbors.Dispose();

            m_nullPairLookup.Dispose();
        }

    }
}
