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
using Nebukam.Cluster;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Nebukam.Chemistry.Editor
{
    [CustomEditor(typeof(ModuleConstraintsModel))]
    public class ModuleConstraintsModelEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {

            bool dirty = false;
            ModuleConstraintsModel data = (ModuleConstraintsModel)target;

            if (data.socketsOffsets != null && data.socketsMirrors != null
                && data.socketsOffsets.Length != data.socketsMirrors.Length)
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
                MirrorBaseSockets(data);
                dirty = true;
            }

            if (GUILayout.Button("Compute mirror indices"))
            {
                ComputeMirrorIndices(data);
                dirty = true;
            }

            if (GUILayout.Button("Assign socket colors"))
            {
                AssignSocketColors(data);
                dirty = true;
            }

            if (dirty)
                EditorUtility.SetDirty(data);

            if (CheckColors(data))
                dirty = true;

        }

        #region Socket definitions

        protected void SetCrossSockets(ModuleConstraintsModel data)
        {
            int3[] sockets = new int3[6];

            for (int i = 0; i < 6; i++)
                sockets[i] = Sockets.OFFSETS[i];

            data.socketsOffsets = sockets;
            MirrorBaseSockets(data);
        }

        protected void SetN1Offsets(ModuleConstraintsModel data)
        {

            int3[] sockets = new int3[26];
            int i = 0;
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        if (x == 0 && y == 0 && z == 0) { continue; }
                        sockets[i++] = int3(x, y, z);
                    }
                }
            }

            data.socketsOffsets = sockets;
            MirrorBaseSockets(data);

        }

        protected void SetN2Offsets(ModuleConstraintsModel data)
        {
            SetN1Offsets(data);
        }

        #endregion

        protected void MirrorBaseSockets(ModuleConstraintsModel data)
        {
            int3[] sockets = data.socketsOffsets;
            int3[] mirrors = new int3[data.socketsOffsets.Length];

            for (int i = 0; i < mirrors.Length; i++)
                mirrors[i] = sockets[i] * -1;

            data.socketsMirrors = mirrors;
            ComputeMirrorIndices(data);
        }

        protected void ComputeMirrorIndices(ModuleConstraintsModel data)
        {
            int3[] offsets = data.socketsOffsets;
            int3[] mirrors = data.socketsMirrors;

            if (offsets.Length != mirrors.Length)
            {
                Debug.LogError("Offset & Mirrors don't have the same length.", data);
                return;
            }

            int
                count = offsets.Length,
                index = -1;

            int[] indices = new int[count];

            for (int i = 0; i < count; i++)
            {
                index = System.Array.IndexOf(offsets, mirrors[i]);
                indices[i] = index;
            }

            data.socketMirrorIndices = indices;

        }

        protected bool CheckColors(ModuleConstraintsModel data)
        {
            if (data.socketsOffsets == null)
                return false;

            if (data.socketColors == null || data.socketColors.Length != data.socketsOffsets.Length)
            {
                AssignSocketColors(data);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void AssignSocketColors(ModuleConstraintsModel data)
        {
            int count = data.socketsOffsets.Length;
            data.socketColors = new Color[count];

            for (int i = 0; i < count; i++)
                data.socketColors[i] = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f);
        }

    }
}
#endif