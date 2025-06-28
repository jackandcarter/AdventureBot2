using UnityEditor;
using UnityEngine;
using Evolution.Data;

namespace Evolution.Editor
{
    public class EnemyEditor : EditorWindow
    {
        private EnemyDatabase database;

        [MenuItem("Adventure/Enemy Editor")]
        public static void Open()
        {
            GetWindow<EnemyEditor>("Enemy Editor");
        }

        private void OnGUI()
        {
            database = (EnemyDatabase)EditorGUILayout.ObjectField("Database", database, typeof(EnemyDatabase), false);

            if (database == null)
            {
                if (GUILayout.Button("Create Database"))
                    CreateDatabase();
                return;
            }

            var so = new SerializedObject(database);
            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("Enemies"), true);
            so.ApplyModifiedProperties();
        }

        private void CreateDatabase()
        {
            database = ScriptableObject.CreateInstance<EnemyDatabase>();
            EnsureDataFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/EnemyDatabase.asset");
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
