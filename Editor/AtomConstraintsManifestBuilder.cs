using Nebukam.Collections;
using Nebukam.Cluster;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.Chemistry.Ed
{
    public class AtomConstraintsManifestBuilder : Pooling.PoolItem, Pooling.IRequireCleanUp
    {

        protected internal List<AtomConstraintsBuilder> m_builders = new List<AtomConstraintsBuilder>();
        protected internal Dictionary<Object, AtomConstraintsBuilder> m_builderMap = new Dictionary<Object, AtomConstraintsBuilder>();

        protected internal List<SlotGroup> m_groupSelection = new List<SlotGroup>();
        protected internal List<GameObject> m_childs = new List<GameObject>();
        protected internal Dictionary<GameObject, Object> m_childsMap = new Dictionary<GameObject, Object>();
        protected internal Dictionary<GameObject, ByteTrio> m_coordMap = new Dictionary<GameObject, ByteTrio>();

        protected internal ListDictionary<ByteTrio, GameObject> m_slots = new ListDictionary<ByteTrio, GameObject>();

        protected AtomConstraintsModel slotModel;
        protected internal int
            uid = 0,
            socketCount = 0;
        protected internal int3[] offsets, mirrors;

        public void Process(AtomConstraintsModel slotData, string id)
        {

            Reset();

            slotModel = slotData;
            offsets = slotModel.offsets;
            mirrors = slotModel.mirrors;

            socketCount = offsets.Length;

            #region group processing

            m_groupSelection.Clear();

            SlotGroup[] groupComponents = GameObject.FindObjectsOfType<SlotGroup>();
            SlotGroup group;

            ByteTrio ccoords, ocoords;

            GameObject ggo, cgo, prefab;
            Transform gtr, ctr;
            int3 off, bnd;

            AtomConstraintsBuilder builder;

            int childCount;
            
            for (int g = 0, count = groupComponents.Length; g < count; g++)
            {

                group = groupComponents[g];
                ggo = group.gameObject;

                if (!ggo.activeSelf || !group.enabled || group.manifestID != id)
                    continue;

                bnd = group.gridSize;
                gtr = ggo.transform;

                m_slots.Clear();
                m_coordMap.Clear();

                m_childs.Clear();
                m_childs.Capacity = gtr.childCount;

                m_childsMap.Clear();

                childCount = gtr.childCount;

                // Go through all children and cache necessary data
                // such as each GameObject related prefab, coordinates in grid etc.
                for (int i = 0; i < childCount; i++)
                {

                    ctr = gtr.GetChild(i);
                    cgo = ctr.gameObject;
                    prefab = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(cgo);

                    if (prefab == null)
                        continue;

                    if (!group.brain.TryGetCoordOf(ctr.position, out ccoords))
                    {
                        Debug.LogWarning(cgo.name + " is outside its parent grid.", cgo);
                        continue;
                    }

                    if(!m_builderMap.TryGetValue(prefab, out builder))
                    {

                        builder = Pooling.Pool.Rent<AtomConstraintsBuilder>();

                        builder.m_index = uid++;
                        builder.m_prefab = prefab;

                        m_builderMap[prefab] = builder;
                        m_builders.Add(builder);

                    }

                    m_childs.Add(cgo);
                    m_childsMap[cgo] = prefab;
                    m_coordMap[cgo] = ccoords;

                    m_slots.Add(ccoords, cgo);

                    builder.m_instanceCount += 1;

                }

                childCount = m_childs.Count;

                for (int i = 0; i < childCount; i++)
                {
                    cgo = m_childs[i];
                    ccoords = m_coordMap[cgo];
                    builder = m_builderMap[m_childsMap[cgo]];

                    for(int o = 0; o < socketCount; o++)
                    {
                        off = offsets[o];

                        if(!Contains(bnd, ccoords + off))
                        {
                            // If coordinate falls outside, fill neighbor slot with null reference
                            builder.Add(o, null);
                            continue;
                        }
                        
                        ocoords = ccoords + offsets[o];
                        
                        if (!m_slots.TryGet(ocoords, out List<GameObject> list))
                            continue;

                        for (int b = 0, listCount = list.Count; b < listCount; b++)
                        {
                            builder.Add(o, m_builderMap[m_childsMap[list[b]]]);
                        }
                    }


                }

            }

            #endregion

            WriteData(id);

            #region Clean up

            for(int i = 0, count = m_builders.Count; i < count; i++)
                m_builders[i].Release();

            m_builders.Clear();
            m_builderMap.Clear();

            #endregion

        }

        protected void WriteData(string id)
        {

            Scene activeScene = SceneManager.GetActiveScene();
            int headerCount = m_builders.Count;
            string
                sceneName = activeScene.name,
                destFolderPath, destInfosPath;

            string[] scenePathSplit = activeScene.path.Split('/');

            destInfosPath =
            destFolderPath = string.Join("/", scenePathSplit, 0, scenePathSplit.Length - 1) + "/" + sceneName + ".gen/";
            Directory.CreateDirectory(destFolderPath);

            destInfosPath += id + ".infos/";
            Directory.CreateDirectory(destInfosPath);

            #region Create manifest

            AtomConstraintsManifest manifest = ScriptableObject.CreateInstance<AtomConstraintsManifest>();
            AtomConstraints info;
            AtomConstraints[] infos = new AtomConstraints[headerCount];
            int
                totalNeighborsLength = 0,
                headerLength = socketCount * headerCount;

            for (int i = 0; i < headerCount; i++)
            {

                info = m_builders[i].FixData(slotModel);
                info.manifest = manifest;

                AssetDatabase.CreateAsset(
                    info,
                    destInfosPath + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_builders[i].m_prefab)) + ".infos.asset");

                infos[i] = info;

                totalNeighborsLength += info.neighbors.Length;
            }

            manifest.infos = infos;
            manifest.socketCount = socketCount;
            manifest.headerCount = headerCount;
            manifest.model = slotModel;

            #endregion

            #region Create inlined manifest

            AtomConstraintsManifestInlined manifestInlined = ScriptableObject.CreateInstance<AtomConstraintsManifestInlined>();
            int3[] headerIndices = new int3[headerLength];
            int[]
                neighbors = new int[totalNeighborsLength],
                ns;
            int
                b, l, e, nCount,
                hIndex = 0,
                nIndex = 0;

            for (int i = 0; i < headerCount; i++)
            {
                info = infos[i];
                ns = info.neighbors;
                nCount = ns.Length;

                for (int j = 0; j < socketCount; j++)
                {
                    b = nIndex + info.begin[j];
                    l = info.lengths[j];
                    e = b + l;

                    headerIndices[hIndex++] = int3(b, e, l);
                }

                for (int j = 0; j < nCount; j++)
                    neighbors[nIndex++] = ns[j];
            }

            manifestInlined.headerCount = headerCount;
            manifestInlined.headerLength = headerLength;
            manifestInlined.headerIndices = headerIndices;
            manifestInlined.neighbors = neighbors;

            #endregion

            AssetDatabase.CreateAsset(manifestInlined, destFolderPath + id + ".manifest-inlined.asset");
            manifest.inlinedManifest = manifestInlined;

            AssetDatabase.CreateAsset(manifest, destFolderPath + id + ".manifest.asset");

            manifest.u = UnityEngine.Random.Range(0, 10000);

            EditorUtility.SetDirty(manifestInlined);
            EditorUtility.SetDirty(manifest);
            


        }
        
        private bool Contains(int3 bounds, int3 pt)
        {
            if (pt.x < 0 || pt.x >= bounds.x)
                return false;

            if (pt.y < 0 || pt.y >= bounds.y)
                return false;

            if (pt.z < 0 || pt.z >= bounds.z)
                return false;

            return true;
        }

        public void Reset()
        {
            uid = 0;

            offsets = null;
            mirrors = null;
        }

        public void CleanUp()
        {
            Reset();
        }

    }
}
