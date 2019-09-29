#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEditor;
using Nebukam.JobAssist;
using Unity.Jobs;
using Unity.Collections;
using Nebukam.Cluster;

namespace Nebukam.Chemistry.Ed
{
    [CustomEditor(typeof(SimpleGridTester))]
    public class SimpleGridTesterEditor : Editor
    {

        //void OnEnable() { EditorApplication.update += Update; }
        //void OnDisable() { EditorApplication.update -= Update; }


        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();

            SimpleGridTester grid = (SimpleGridTester)target;

            if(grid.slotModel == null)
                EditorGUILayout.HelpBox("Slot constraints cannot be null.", MessageType.Error);

            
            if (GUILayout.Button("Clear Grid")) { ClearGrid(grid); }
            if (GUILayout.Button("Fill Grid")) { FillGrid(grid); }

        }

        protected virtual void ClearGrid(SimpleGridTester grid)
        {
            Transform tr = grid.transform;
            int count = tr.childCount;
            Transform[] childs = new Transform[count];

            for (int i = 0; i < count; i++)
                childs[i] = tr.GetChild(i);

            for (int i = 0; i < count; i++)
                DestroyImmediate(childs[i].gameObject);

        }

        protected virtual void FillGrid(SimpleGridTester grid)
        {

            if(grid.manifest == null)
            {
                AssetDatabase.Refresh();
                Debug.LogError("Cannot fill grid : no manifest !", grid);
                return;
            }

            AssetDatabase.Refresh();

            grid.cluster.pos = grid.transform.position;
            grid.cluster.Init(grid.gridSize, grid.slotModel.model, true);

            if (grid.solver == null)
                grid.solver = new SimpleSolver<ConstrainedSlot, ConstrainedSlotInfos>();

            if (grid.solver.scheduled)
                grid.solver.Complete();

            ClearGrid(grid);

            grid.solver.manifest = grid.manifest;
            grid.solver.slotCluster = grid.cluster;
            grid.solver.seed = (uint)UnityEngine.Random.Range(1, 10000);
            grid.solver.Schedule(0f);

            Update();
        }

        private void Update()
        {

            SimpleGridTester grid = (SimpleGridTester)target;

            if (grid == null || 
                grid.solver == null)
                return;

            grid.solver.Complete();
            IClusterBrain brain = grid.cluster.brain;

            int index;
                NativeArray<int> results = grid.solver.constraintSolver.results;
                AtomConstraints atom;
                ISlot slot;
                GameObject go;
                ByteTrio coords;

                for(int i = 0, count = results.Length; i < count; i++)
                {

                    index = results[i];

                    if (index < 0)
                        continue;

                    atom = grid.manifest.infos[index];
                    slot = grid.cluster[i];

                    if (atom.prefab == grid.nullPrefab)
                        continue;

                    coords = slot.coordinates;

                    go = PrefabUtility.InstantiatePrefab(atom.prefab, grid.transform) as GameObject;
                    go.name = atom.prefab.name + " #" + i;
                    go.transform.position = brain.ComputePosition(ref coords);// slot.pos + grid.slotModel.model.size * 0.5f;

                }

            //}

        }


    }
}
#endif