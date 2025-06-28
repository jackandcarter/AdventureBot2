using UnityEditor;
using UnityEngine;
using Evolution.Data;

namespace Evolution.Editor
{
    public class ClassEditor : EditorWindow
    {
        private ClassDatabase database;

        [MenuItem("Adventure/Class Editor")]
        public static void Open()
        {
            GetWindow<ClassEditor>("Class Editor");
        }

        private void OnGUI()
        {
            database = (ClassDatabase)EditorGUILayout.ObjectField("Database", database, typeof(ClassDatabase), false);

            if (database == null)
            {
                if (GUILayout.Button("Create Database"))
                    CreateDatabase();
                return;
            }

            var so = new SerializedObject(database);
            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("Classes"), true);
            so.ApplyModifiedProperties();
        }

        private void CreateDatabase()
        {
            database = ScriptableObject.CreateInstance<ClassDatabase>();
            EnsureDataFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/ClassDatabase.asset");
            AssetDatabase.CreateAsset(database, path);
            AssetDatabase.SaveAssets();
        }

        private void EnsureDataFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
        }
    }
}
