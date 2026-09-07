using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LockUp.EditorTools
{
    [CustomEditor(typeof(LevelBuilder))]
    public class LevelBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelBuilder builder = (LevelBuilder)target;

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Build Level", GUILayout.Height(28)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Build Level");
                    builder.Rebuild();
                    EditorSceneManager.MarkSceneDirty(builder.gameObject.scene);
                }

                if (GUILayout.Button("Clear", GUILayout.Height(28)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Clear Level");
                    builder.Clear();
                    EditorSceneManager.MarkSceneDirty(builder.gameObject.scene);
                }
            }

            EditorGUILayout.HelpBox(
                "Geometry is generated under the 'Level' child object. Rebuilding replaces it, " +
                "so hand edits inside 'Level' are lost — put custom props outside it.",
                MessageType.Info);
        }
    }
}
