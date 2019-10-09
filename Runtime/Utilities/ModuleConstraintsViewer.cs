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

using Nebukam.Cluster;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Chemistry
{
    public class ModuleConstraintsViewer : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool draw = true;
        public ModuleConstraintsModel model;
        public Color color = Color.white;
        public Color color2 = Color.white;
        public bool wire = false;
        
        private void OnDrawGizmos()
        {

            SlotModel baseModel = model?.model;

            if (!draw || model == null)
                return;

            int3[] sockets = model.socketsOffsets;

            if(sockets == null)
            {
                Debug.LogError("Input model has no sockets.");
                return;
            }

            float3
                pos,
                tr = transform.position,
                s = baseModel.size;

            Gizmos.color = color;

            if (wire)
                Gizmos.DrawWireCube(baseModel.offset + (float3)transform.position, s);
            else
                Gizmos.DrawCube(baseModel.offset + (float3)transform.position, s);

            Gizmos.color = color2;

            // Draw neighbors

            for (int i = 0, count = sockets.Length; i < count; i++)
            {
                pos = tr + (s * sockets[i]);

                Gizmos.color = model.socketColors[i];

                if (wire)
                    Gizmos.DrawWireCube(pos, s * .5f);
                else
                    Gizmos.DrawCube(pos, s * .5f);
            }

        }
#endif
    }
}
