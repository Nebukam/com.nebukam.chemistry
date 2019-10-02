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

            if (data.sockets != null && data.socketMirrors != null
                && data.sockets.Length != data.socketMirrors.Length)
            {
                EditorGUILayout.HelpBox("Offsets & Mirrors must have the same length.", MessageType.Error);
            }

            DrawDefaultInspector();

            if (GUILayout.Button("N x 0.5"))
            {
                SetCrossSockets(data);
                dirty = true;
            }

            if (GUILayout.Button("N x 1"))
            {
                SetN1Offsets(data);
                dirty = true;
            }

            if (GUILayout.Button("N x 2"))
            {
                SetN2Offsets(data);
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

        #region Socket definitions
        
        protected void SetCrossSockets(AtomConstraintsModel data)
        {
            int3[] offsets = new int3[6];

            for (int i = 0; i < 6; i++)
                offsets[i] = Sockets.OFFSETS[i];

            data.sockets = offsets;
            MirrorBaseOffset(data);
        }

        protected void SetN1Offsets(AtomConstraintsModel data)
        {

            int3[] offsets = new int3[26];
            int i = 0;
            for(int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        if(x == 0 && y == 0 && z == 0) { continue; }
                        offsets[i++] = int3(x, y, z);
                    }
                }
            }

            data.sockets = offsets;
            MirrorBaseOffset(data);

        }

        protected void SetN2Offsets(AtomConstraintsModel data)
        {
            SetN1Offsets(data);
        }

        #endregion

        protected void MirrorBaseOffset(AtomConstraintsModel data)
        {
            int3[] offsets = data.sockets;
            int3[] mirrors = new int3[data.sockets.Length];
            
            for (int i = 0; i < mirrors.Length; i++)
                mirrors[i] = offsets[i] * -1;

            data.socketMirrors = mirrors;
            ComputeMirrorIndices(data);
        }

        protected void ComputeMirrorIndices(AtomConstraintsModel data)
        {
            int3[] offsets = data.sockets;
            int3[] mirrors = data.socketMirrors;

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

            data.socketMirrorIndices = indices;

        }
        
    }
}
#endif