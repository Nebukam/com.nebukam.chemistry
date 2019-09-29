#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEditor;

namespace Nebukam.Chemistry.Ed
{
    [CustomEditor(typeof(SlotGroup))]
    [CanEditMultipleObjects]
    public class SlotGroupEditor : Editor
    {

        public override void OnInspectorGUI()
        {

            SlotGroup group = (SlotGroup)target;

            if (group.slotModel == null)
            {
                EditorGUILayout.HelpBox("Slot constraints cannot be null.", MessageType.Error);
            }
            else if (!(group.slotModel is AtomConstraintsModel))
            {
                EditorGUILayout.HelpBox("Slot constraints must be of type WFCSlotData.", MessageType.Error);
            }

            DrawDefaultInspector();

            //EditorGUILayout.HelpBox("Manifest : "+SceneManager.GetActiveScene().path +"/WFC/"+ group.clusterName + "", MessageType.Info);
            if (GUILayout.Button("Generate manifest files")) { GenerateManifest(group); }
            
        }

        /// <summary>
        /// Commit the current template configuration
        /// </summary>
        protected virtual void GenerateManifest(SlotGroup group)
        {
            AtomConstraintsManifestBuilder builder = Pooling.Pool.Rent<AtomConstraintsManifestBuilder>();

            builder.Process(group.slotModel, group.manifestID);

            builder.Release();

            //WFCUtilities.GenerateManifest((target as SlotGroup).manifestID);
        }

    }
}
#endif