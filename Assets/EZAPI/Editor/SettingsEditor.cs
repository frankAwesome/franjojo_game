using EzAPI.RunTime;
using UnityEditor;
namespace EzAPI
{
    namespace Editor
    {
        [CustomEditor(typeof(Settings))]
        public class SettingsEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                // Display the fixed message at the top
                EditorGUILayout.HelpBox("If you are not seeing your request payload or request response classes in the dropdown, then please Save the data first by clicking on Tools/EzAPI/Save.\n If you still face same issue please check if you inherited the classes from proper class.\n After Putting all your data also save data or you will not be able to see enums in EndPoints.", MessageType.Info);

                // Then, draw the default inspector
                base.OnInspectorGUI();
            }
        }
    }
}