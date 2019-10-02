using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Nebukam.JobAssist;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{

    public interface IConstraintsManifestProvider : IProcessor
    {

        AtomConstraintsManifest manifest { get; set; }

        NativeArray<int3> offsets { get; }
        NativeArray<int3> mirrors { get; }
        NativeArray<int> mirrorsIndices { get; }

        NativeArray<int3> headerIndices { get; }
        NativeArray<int> neighbors { get; }

    }

    public class ConstraintsManifestProvider : Processor<Unemployed>, IConstraintsManifestProvider
    {

        protected AtomConstraintsManifest m_manifest = null;

        protected NativeArray<int3> m_offsets = new NativeArray<int3>(0, Allocator.Persistent);
        protected NativeArray<int3> m_mirrors = new NativeArray<int3>(0, Allocator.Persistent);
        protected NativeArray<int> m_mirrorsIndices = new NativeArray<int>(0, Allocator.Persistent);

        protected NativeArray<int3> m_headerIndices = new NativeArray<int3>(0, Allocator.Persistent);
        protected NativeArray<int> m_neighbors = new NativeArray<int>(0, Allocator.Persistent);


        public AtomConstraintsManifest manifest{ get { return m_manifest; } set { m_manifest = value; } }

        public NativeArray<int3> offsets { get { return m_offsets; } }
        public NativeArray<int3> mirrors { get { return m_mirrors; } }
        public NativeArray<int> mirrorsIndices { get { return m_mirrorsIndices; } }

        public NativeArray<int3> headerIndices { get { return m_headerIndices; } }
        public NativeArray<int> neighbors { get { return m_neighbors; } }

        protected override void InternalLock() { }

        protected override void Prepare(ref Unemployed job, float delta)
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

            for(int i = 0; i < modelLength; i++)
            {
                m_offsets[i] = model.sockets[i];
                m_mirrors[i] = model.socketMirrors[i];
                m_mirrorsIndices[i] = model.socketMirrorIndices[i];
            }

            #endregion

            #region manifest

            AtomConstraintsManifestInlined manifestInlined = manifest.inlinedManifest;
            
            int3[] hIndices = manifestInlined.headerIndices;
            int[] nIndices = manifestInlined.neighbors;
            int
                hCount = hIndices.Length,
                nCount = nIndices.Length;

            if (m_headerIndices.Length != hCount)
            {
                m_headerIndices.Dispose();
                m_neighbors.Dispose();

                m_headerIndices = new NativeArray<int3>(hCount, Allocator.Persistent);
                m_neighbors = new NativeArray<int>(nCount, Allocator.Persistent);
            }

            for (int i = 0; i < hCount; i++)
                m_headerIndices[i] = hIndices[i];

            for (int i = 0; i < nCount; i++)
                m_neighbors[i] = nIndices[i];

            #endregion


        }

        protected override void Apply(ref Unemployed job) { }

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

        }

    }
}
