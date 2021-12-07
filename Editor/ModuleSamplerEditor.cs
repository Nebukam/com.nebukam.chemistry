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
using UnityEditor;
using UnityEngine;

namespace Nebukam.Chemistry.Editor
{
    [CustomEditor(typeof(ModuleSampler))]
    [CanEditMultipleObjects]
    public class ModuleSamplerEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {

            ModuleSampler group = (ModuleSampler)target;

            if (group.slotModel == null)
            {
                EditorGUILayout.HelpBox("Slot constraints cannot be null.", MessageType.Error);
            }
            else if (!(group.slotModel is ModuleConstraintsModel))
            {
                EditorGUILayout.HelpBox("Slot constraints must be of type ModuleConstraintsModel.", MessageType.Error);
            }

            DrawDefaultInspector();

            //EditorGUILayout.HelpBox("Manifest : "+SceneManager.GetActiveScene().path +"/WFC/"+ group.clusterName + "", MessageType.Info);
            if (GUILayout.Button("Generate manifest files")) { GenerateManifest(group); }

        }

        /// <summary>
        /// Commit the current template configuration
        /// </summary>
        protected virtual void GenerateManifest(ModuleSampler group)
        {
            ModuleConstraintsManifestBuilder builder = Pooling.Pool.Rent<ModuleConstraintsManifestBuilder>();

            builder.Process(group.slotModel, group.manifestID);

            builder.Release();

            //WFCUtilities.GenerateManifest((target as SlotGroup).manifestID);
        }

    }
}
#endif