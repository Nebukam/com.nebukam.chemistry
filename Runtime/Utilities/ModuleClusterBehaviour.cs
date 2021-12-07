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

using Nebukam.Cluster;
using Nebukam.Utils;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Nebukam.Chemistry
{

    /// <summary>
    /// A module template store core configuration used to solve & collapse a WFC grid.
    /// It is also used to generate and extract module grid models.
    /// </summary>
    public abstract class ModuleClusterBehaviour : MonoBehaviour
    {

        [Header("Grid Settings")]
        public ModuleConstraintsModel slotModel;
        protected SlotModel model;
        public int3 gridSize = int3(3);

        [Header("Display Settings")]
        public Color color = Color.blue;
        public float radius = 0.2f;
        public bool wireOnly = false;
        public bool noDebug = false;

        public ClusterBrain brain;

#if UNITY_EDITOR

        #region Gizmos

        private void OnDrawGizmos()
        {

            if (noDebug)
                return;

            model = slotModel?.model;

            if (model == null)
                return;

            brain = new ClusterBrain()
            {
                m_pos = transform.position,
                m_clusterSize = gridSize
            };

            brain.slotModel = model;


            float3 from = transform.position, pos;
            Gizmos.color = color.A(0.05f);

            // Draw volume handle
            if (!wireOnly)
            {
                float3 b = gridSize * model.size;
                Gizmos.DrawCube((float3)transform.position + b * 0.5f, b);
            }

            Gizmos.color = color;

            // Draw grid

            ByteTrio coords;
            float3 off = model.size * 0.5f;
            for (int z = 0; z < gridSize.z; z++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int x = 0; x < gridSize.x; x++)
                    {
                        coords = new ByteTrio(x, y, z);
                        pos = brain.ComputePosition(ref coords);
                        Gizmos.DrawWireCube(pos + off, model.size);
                    }
                }
            }

            // Highlight cluster corresponding to active selection

            GameObject[] selection = Selection.gameObjects;
            GameObject go;
            for (int i = 0, count = selection.Length; i < count; i++)
            {
                go = selection[i];

                if (go == Selection.activeGameObject)
                    Gizmos.color = Color.green.A(0.5f);
                else
                    Gizmos.color = color.A(0.25f);

                if (go == gameObject || go.transform.parent != transform)
                    continue;

                pos = go.transform.position;
                if (brain.TryGetCoordOf(pos, out coords))
                {
                    pos = brain.ComputePosition(ref coords);
                    Gizmos.DrawCube(pos + off, model.size);
                }
            }

        }

        private void OnValidate()
        {
            model = slotModel?.model;
        }

        #endregion

#endif

        #region Positioning
        /*
        public Bounds bounds
        {
            get
            {
                if (model == null)
                    return new Bounds();

                float3 s = model.size * gridSize;
                return new Bounds((float3)transform.position + s * 0.5f, s);
            }
        }

        /// <summary>
        /// Retrieve the coordinates that contain the given location,
        /// based on cluster location & slot's size
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool TryGetCoordOf(float3 location, out ByteTrio coord)
        {

            if (!bounds.Contains(location) || model == null)
            {
                coord = ByteTrio.zero;
                return false;
            }

            float3
                pos = transform.position,
                s = model.size,
                loc = location - pos;

            float
                modX = loc.x % s.x,
                modY = loc.y % s.y,
                modZ = loc.z % s.z;

            int
                posX = (int)((loc.x - modX) / s.x),
                posY = (int)((loc.y - modY) / s.y),
                posZ = (int)((loc.z - modZ) / s.z);

            coord = new ByteTrio(posX, posY, posZ);
            return true;
        }

        /// <summary>
        /// Return the world-space projection of the given coordinates, as projected by a default cluster.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float3 ComputePosition(ref ByteTrio coords)
        {
            return (float3)transform.position + coords * model.size + model.offset;
        }
        */
        #endregion

    }

}
