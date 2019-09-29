#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEditor;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Nebukam.Cluster;

namespace Nebukam.Chemistry.Ed
{
    [CustomEditor(typeof(AtomConstraintsModel))]
    public class AtomConstraintsModelEditor : Editor
    {

        public override void OnInspectorGUI()
        {

            bool dirty = false;
            AtomConstraintsModel data = (AtomConstraintsModel)target;

            if (data.offsets != null && data.mirrors != null
                && data.offsets.Length != data.mirrors.Length)
            {
                EditorGUILayout.HelpBox("Offsets & Mirrors must have the same length.", MessageType.Error);
            }

            DrawDefaultInspector();

            if (GUILayout.Button("Set defaults indices"))
            {
                SetDefaultOffsets(data);
                dirty = true;
            }

            GUILayout.Space(10.0f);

            if (GUILayout.Button("Compute mirror offsets"))
            {
                MirrorBaseOffset(data);
                dirty = true;
            }

            if (GUILayout.Button("Compute mirror indices"))
            {
                ComputeMirrorIndices(data);
                dirty = true;
            }

            if (dirty)
                EditorUtility.SetDirty(data);

        }

        protected void SetDefaultOffsets(AtomConstraintsModel data)
        {

            int3[] offsets = new int3[6];

            for (int i = 0; i < 6; i++)
                offsets[i] = Sockets.OFFSETS[i];

            data.offsets = offsets;
            MirrorBaseOffset(data);

        }

        protected void MirrorBaseOffset(AtomConstraintsModel data)
        {
            int3[] offsets = data.offsets;
            int3[] mirrors = new int3[data.offsets.Length];
            
            for (int i = 0; i < mirrors.Length; i++)
                mirrors[i] = offsets[i] * -1;

            data.mirrors = mirrors;
            ComputeMirrorIndices(data);
        }

        protected void ComputeMirrorIndices(AtomConstraintsModel data)
        {
            int3[] offsets = data.offsets;
            int3[] mirrors = data.mirrors;

            if(offsets.Length != mirrors.Length)
            {
                Debug.LogError("Offset & Mirrors don't have the same length.", data);
                return;
            }

            int
                count = offsets.Length,
                index = -1;

            int[] indices = new int[count];
            
            for(int i = 0; i < count; i++)
            {
                index = System.Array.IndexOf(offsets, mirrors[i]);
                indices[i] = index;
            }

            data.mirrorsIndices = indices;

        }
        
    }
}
#endif