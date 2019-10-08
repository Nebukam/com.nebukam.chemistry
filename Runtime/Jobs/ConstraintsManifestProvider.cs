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


using Nebukam.JobAssist;
using Unity.Collections;
using Unity.Mathematics;

namespace Nebukam.Chemistry
{

    public interface IConstraintsManifestProvider : IProcessor
    {

        AtomConstraintsManifest manifest { get; set; }

        NativeArray<int3> offsets { get; }
        NativeArray<int3> mirrors { get; }
        NativeArray<int> mirrorsIndices { get; }

        NativeArray<float> headerWeights { get; }
        NativeArray<int3> headerIndices { get; }
        NativeArray<int> neighbors { get; }

        NativeHashMap<IntPair, bool> nullPairLookup { get; }

    }

    public class ConstraintsManifestProvider : Processor<ConstraintManifestJob>, IConstraintsManifestProvider
    {

        protected AtomConstraintsManifest m_manifest = null;

        protected NativeArray<int3> m_offsets = new NativeArray<int3>(0, Allocator.Persistent);
        protected NativeArray<int3> m_mirrors = new NativeArray<int3>(0, Allocator.Persistent);
        protected NativeArray<int> m_mirrorsIndices = new NativeArray<int>(0, Allocator.Persistent);

        protected NativeArray<float> m_headerWeights = new NativeArray<float>(0, Allocator.Persistent);
        protected NativeArray<int3> m_headerIndices = new NativeArray<int3>(0, Allocator.Persistent);
        protected NativeArray<int> m_neighbors = new NativeArray<int>(0, Allocator.Persistent);

        protected NativeHashMap<IntPair, bool> m_nullPairLookup = new NativeHashMap<IntPair, bool>(0, Allocator.Persistent);



        public AtomConstraintsManifest manifest { get { return m_manifest; } set { m_manifest = value; } }

        public NativeArray<int3> offsets { get { return m_offsets; } }
        public NativeArray<int3> mirrors { get { return m_mirrors; } }
        public NativeArray<int> mirrorsIndices { get { return m_mirrorsIndices; } }

        public NativeArray<float> headerWeights { get { return m_headerWeights; } }
        public NativeArray<int3> headerIndices { get { return m_headerIndices; } }
        public NativeArray<int> neighbors { get { return m_neighbors; } }

        // key = header index : socket index
        public NativeHashMap<IntPair, bool> nullPairLookup { get { return m_nullPairLookup; } }

        protected override void InternalLock() { }

        protected override void Prepare(ref ConstraintManifestJob job, float delta)
        {

            #region model infos

            AtomConstraintsModel model = m_manifest.model;
            int modelLength = model.sockets.Length;

            if (m_offsets.Length != modelLength)
            {
                m_offsets.Dispose();
                m_mirrors.Dispose();
                m_mirrorsIndices.Dispose();

                m_offsets = new NativeArray<int3>(modelLength, Allocator.Persistent);
                m_mirrors = new NativeArray<int3>(modelLength, Allocator.Persistent);
                m_mirrorsIndices = new NativeArray<int>(modelLength, Allocator.Persistent);
            }

            for (int i = 0; i < modelLength; i++)
            {
                m_offsets[i] = model.sockets[i];
                m_mirrors[i] = model.socketMirrors[i];
                m_mirrorsIndices[i] = model.socketMirrorIndices[i];
            }

            #endregion

            #region manifest

            AtomConstraintsManifestInlined manifestInlined = manifest.inlinedManifest;
            AtomConstraints[] infos = manifest.infos;

            int3[] hIndices = manifestInlined.headerIndices;
            int[] nIndices = manifestInlined.neighbors;
            int
                headerCount = manifest.headerCount,
                hCount = hIndices.Length,
                nCount = nIndices.Length;

            if (m_headerIndices.Length != hCount)
            {
                m_headerWeights.Dispose();
                m_headerIndices.Dispose();
                m_neighbors.Dispose();
                m_nullPairLookup.Dispose();

                m_headerWeights = new NativeArray<float>(headerCount, Allocator.Persistent);
                m_headerIndices = new NativeArray<int3>(hCount, Allocator.Persistent);
                m_neighbors = new NativeArray<int>(nCount, Allocator.Persistent);
                m_nullPairLookup = new NativeHashMap<IntPair, bool>(headerCount * modelLength, Allocator.Persistent);
            }

            for (int i = 0; i < headerCount; i++)
                m_headerWeights[i] = infos[i].weight;

            for (int i = 0; i < hCount; i++)
                m_headerIndices[i] = hIndices[i];

            for (int i = 0; i < nCount; i++)
                m_neighbors[i] = nIndices[i];

            job.m_socketCount = m_offsets.Length;

            job.m_headerCount = manifest.headerCount;
            job.m_headerIndices = m_headerIndices;
            job.m_neighbors = m_neighbors;

            job.m_nullPairLookup = m_nullPairLookup;

            #endregion

        }

        protected override void Apply(ref ConstraintManifestJob job) { }

        protected override void InternalUnlock() { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) { return; }

            m_manifest = null;

            m_offsets.Dispose();
            m_mirrors.Dispose();
            m_mirrorsIndices.Dispose();

            m_headerIndices.Dispose();
            m_neighbors.Dispose();

            m_nullPairLookup.Dispose();

        }

    }
}
