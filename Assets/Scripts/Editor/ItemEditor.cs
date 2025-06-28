using UnityEditor;
using UnityEngine;
using Evolution.Data;

namespace Evolution.Editor
{
    public class ItemEditor : EditorWindow
    {
        private ItemDatabase database;

        [MenuItem("Adventure/Item Editor")]
        public static void Open()
        {
            GetWindow<ItemEditor>("Item Editor");
        }

        private void OnGUI()
        {
            database = (ItemDatabase)EditorGUILayout.ObjectField("Database", database, typeof(ItemDatabase), false);

            if (database == null)
            {
                if (GUILayout.Button("Create Database"))
                    CreateDatabase();
                return;
            }

            var so = new SerializedObject(database);
            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("Items"), true);
            so.ApplyModifiedProperties();
        }

        private void CreateDatabase()
        {
            database = ScriptableObject.CreateInstance<ItemDatabase>();
            EnsureDataFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/ItemDatabase.asset");
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
