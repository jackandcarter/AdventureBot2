using UnityEditor;
using UnityEngine;
using Evolution.Data;

namespace Evolution.Editor
{
    public class StatsEditor : EditorWindow
    {
        private StatsDatabase database;
        private Vector2 scroll;

        [MenuItem("Adventure/Stats Editor")]
        public static void Open()
        {
            GetWindow<StatsEditor>("Stats Editor");
        }

        private void OnGUI()
        {
            database = (StatsDatabase)EditorGUILayout.ObjectField("Database", database, typeof(StatsDatabase), false);

            if (database == null)
            {
                if (GUILayout.Button("Create Database"))
                    CreateDatabase();
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            for (int i = 0; i < database.Stats.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                database.Stats[i] = (StatDefinition)EditorGUILayout.ObjectField(database.Stats[i], typeof(StatDefinition), false);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    database.Stats.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Stat Reference"))
                database.Stats.Add(null);

            if (GUILayout.Button("Create New Stat"))
                CreateStat();

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
                EditorUtility.SetDirty(database);
        }

        private void CreateDatabase()
        {
            database = ScriptableObject.CreateInstance<StatsDatabase>();
            EnsureDataFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/StatsDatabase.asset");
            AssetDatabase.CreateAsset(database, path);
            AssetDatabase.SaveAssets();
        }

        private void CreateStat()
        {
            EnsureDataFolder();
            if (!AssetDatabase.IsValidFolder("Assets/Data/Stats"))
                AssetDatabase.CreateFolder("Assets/Data", "Stats");

            var stat = ScriptableObject.CreateInstance<StatDefinition>();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/Stats/NewStat.asset");
            AssetDatabase.CreateAsset(stat, path);
            AssetDatabase.SaveAssets();
            database.Stats.Add(stat);
        }

        private void EnsureDataFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
        }
    }
}
