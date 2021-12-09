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
#if UNITY_EDITOR
using Nebukam.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Mathematics.math;

namespace Nebukam.Chemistry.Editor
{
    public class ModuleConstraintsManifestBuilder : PoolItem, IRequireCleanUp
    {

        protected internal List<ModuleConstraintsBuilder> m_builders = new List<ModuleConstraintsBuilder>();
        protected internal Dictionary<Object, ModuleConstraintsBuilder> m_builderMap = new Dictionary<Object, ModuleConstraintsBuilder>();

        protected internal List<GameObject> m_items = new List<GameObject>();
        protected internal Dictionary<GameObject, Object> m_itemsMap = new Dictionary<GameObject, Object>();
        protected internal Dictionary<GameObject, ByteTrio> m_moduleBuildersMap = new Dictionary<GameObject, ByteTrio>();

        protected internal ListDictionary<ByteTrio, GameObject> m_moduleBuilders = new ListDictionary<ByteTrio, GameObject>();

        protected internal bool useGUID = false;

        protected ModuleConstraintsModel model;
        protected internal int
            uid = 0,
            m_socketCount = 0;

        protected internal int3[] 
            m_socketsOffsets, 
            m_socketsMirrors;

        public void Process(ModuleConstraintsModel inputModel, string id)
        {

            Reset();

            model = inputModel;

            m_socketsOffsets = model.socketsOffsets;
            m_socketsMirrors = model.socketsMirrors;
            m_socketCount = m_socketsOffsets.Length;

            #region group processing
            
            ModuleSampler[] samplerComponent = GameObject.FindObjectsOfType<ModuleSampler>();
            ModuleSampler sampler;

            ByteTrio 
                itemCoords, 
                socketCoord;

            GameObject 
                samplerGo, 
                itemGo, 
                modulePrefab;

            Transform 
                samplerTr, 
                itemTr;

            int3 
                samplerSize;

            ModuleConstraintsBuilder moduleBuilder;
            ModuleInfos moduleInfos;
            
            int itemCount;

            for (int samplerIndex = 0, count = samplerComponent.Length; samplerIndex < count; samplerIndex++)
            {

                sampler = samplerComponent[samplerIndex];
                samplerGo = sampler.gameObject;

                if (!samplerGo.activeSelf || 
                    !sampler.enabled || 
                    sampler.manifestID != id)
                    continue;

                samplerSize = sampler.gridSize;
                samplerTr = samplerGo.transform;

                m_moduleBuilders.Clear();
                m_moduleBuildersMap.Clear();

                m_items.Clear();
                m_items.Capacity = samplerTr.childCount;

                m_itemsMap.Clear();

                itemCount = samplerTr.childCount;

                // Go through all children and cache necessary data
                // such as each GameObject related prefab, coordinates in grid etc.
                for (int i = 0; i < itemCount; i++)
                {

                    itemTr = samplerTr.GetChild(i);
                    itemGo = itemTr.gameObject;

                    moduleInfos = itemGo.GetComponent<ModuleInfos>();
                    modulePrefab = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(itemGo);

                    if (modulePrefab == null)
                        continue;

                    if (!sampler.brain.TryGetCoordOf(itemTr.position, out itemCoords))
                    {
                        Debug.LogWarning(itemGo.name + " is outside its parent grid.", itemGo);
                        continue;
                    }

                    if (!m_builderMap.TryGetValue(modulePrefab, out moduleBuilder))
                    {

                        moduleBuilder = Pool.Rent<ModuleConstraintsBuilder>();

                        moduleBuilder.m_index = uid++;
                        moduleBuilder.m_prefab = modulePrefab;

                        m_builderMap[modulePrefab] = moduleBuilder;
                        m_builders.Add(moduleBuilder);

                    }

                    if (moduleInfos != null)
                        moduleBuilder.m_weight = moduleInfos.weight;

                    m_items.Add(itemGo);
                    m_itemsMap[itemGo] = modulePrefab;
                    m_moduleBuildersMap[itemGo] = itemCoords;

                    m_moduleBuilders.Add(itemCoords, itemGo);

                }

                itemCount = m_items.Count;

                for (int i = 0; i < itemCount; i++)
                {
                    itemGo = m_items[i];
                    itemCoords = m_moduleBuildersMap[itemGo];
                    moduleBuilder = m_builderMap[m_itemsMap[itemGo]];

                    for (int s = 0; s < m_socketCount; s++)
                    {
                        socketCoord = itemCoords + m_socketsOffsets[s];

                        if (socketCoord.x < 0 || socketCoord.x >= samplerSize.x ||
                            socketCoord.y < 0 || socketCoord.y >= samplerSize.y ||
                            socketCoord.z < 0 || socketCoord.z >= samplerSize.z)
                        {
                            // If coordinate falls outside, fill neighbor slot with null reference
                            if (!sampler.ignoreNullSockets)
                                moduleBuilder.Add(s, null);
                        }
                        else if (m_moduleBuilders.TryGet(socketCoord, out List<GameObject> list))
                        {
                            // Feed module builder with current socket's contents
                            for (int b = 0, listCount = list.Count; b < listCount; b++)
                                moduleBuilder.Add(s, m_builderMap[m_itemsMap[list[b]]]);
                        }
                    }

                }

            }

            #endregion

            WriteData(id);

            #region Clean up

            for (int i = 0, count = m_builders.Count; i < count; i++)
                m_builders[i].Release();

            m_builders.Clear();
            m_builderMap.Clear();

            #endregion

        }

        protected void WriteData(string id)
        {

            Scene activeScene = SceneManager.GetActiveScene();
            int moduleCount = m_builders.Count;
            string
                sceneName = activeScene.name,
                destFolderPath, destInfosPath, assetName, num;

            string[] scenePathSplit = activeScene.path.Split('/');

            destInfosPath =
            destFolderPath = string.Join("/", scenePathSplit, 0, scenePathSplit.Length - 1) + "/" + sceneName + ".gen/";
            Directory.CreateDirectory(destFolderPath);

            destInfosPath += id + ".infos/";
            Directory.CreateDirectory(destInfosPath);

            #region Create manifest

            ModuleConstraintsManifest manifest = ScriptableObject.CreateInstance<ModuleConstraintsManifest>();
            ModuleConstraintsBuilder moduleBuilder;
            ModuleConstraints module;
            ModuleConstraints[] modulesConstraints = new ModuleConstraints[moduleCount];
            int
                totalNeighborsLength = 0,
                headerCount = m_socketCount * moduleCount;

            string tpl = "{0:0}";

            if (moduleCount >= 10) tpl = "{0:00}";
            if (moduleCount >= 100) tpl = "{0:000}";
            if (moduleCount >= 1000) tpl = "{0:0000}";
            if (moduleCount >= 10000) tpl = "{0:00000}";

            for (int i = 0; i < moduleCount; i++)
            {

                moduleBuilder = m_builders[i];

                module = moduleBuilder.FixData(model);
                module.manifest = manifest;

                if(useGUID)
                {
                    assetName = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(moduleBuilder.m_prefab));
                }
                else
                {
                    num = string.Format(tpl, i);
                    assetName = num + "." + moduleBuilder.m_prefab.name;                    
                }

                AssetDatabase.CreateAsset(
                    module,
                    destInfosPath + assetName + ".asset");

                modulesConstraints[i] = module;

                totalNeighborsLength += module.neighbors.Length;
            }

            manifest.infos = modulesConstraints;
            manifest.socketCount = m_socketCount;
            manifest.moduleCount = moduleCount;
            manifest.model = model;

            #endregion

            #region Create inlined manifest

            ModuleConstraintsManifestInlined manifestInlined = ScriptableObject.CreateInstance<ModuleConstraintsManifestInlined>();
            int3[] modulesHeaders = new int3[headerCount];
            float[] modulesWeights = new float[moduleCount];
            int[]
                neighbors = new int[totalNeighborsLength],
                ns;
            int
                b, l, e, nCount,
                hIndex = 0,
                nIndex = 0;

            for (int i = 0; i < moduleCount; i++)
            {
                module = modulesConstraints[i];
                ns = module.neighbors;
                nCount = ns.Length;

                modulesWeights[i] = module.weight;

                for (int j = 0; j < m_socketCount; j++)
                {
                    b = nIndex + module.begin[j];
                    l = module.lengths[j];
                    e = b + l;

                    modulesHeaders[hIndex++] = int3(b, e, l);
                }

                for (int j = 0; j < nCount; j++)
                    neighbors[nIndex++] = ns[j];
            }

            manifestInlined.moduleCount = moduleCount;
            manifestInlined.modulesWeights = modulesWeights;
            manifestInlined.modulesLength = headerCount;
            manifestInlined.modulesHeaders = modulesHeaders;
            manifestInlined.modulesNeighbors = neighbors;

            #endregion

            AssetDatabase.CreateAsset(manifestInlined, destFolderPath + id + ".manifest-inlined.asset");
            manifest.inlinedManifest = manifestInlined;

            AssetDatabase.CreateAsset(manifest, destFolderPath + id + ".manifest.asset");

            manifest.u = UnityEngine.Random.Range(0, 10000);

            EditorUtility.SetDirty(manifestInlined);
            EditorUtility.SetDirty(manifest);

        }
        
        public void Reset()
        {
            uid = 0;

            m_socketsOffsets = null;
            m_socketsMirrors = null;
            useGUID = false;
        }

        public void CleanUp()
        {
            Reset();
        }

    }
}
#endif