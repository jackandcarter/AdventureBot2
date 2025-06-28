using UnityEditor;
using UnityEngine;
using Evolution.Data;

namespace Evolution.Editor
{
    public class ShopEditor : EditorWindow
    {
        private ShopDatabase database;

        [MenuItem("Adventure/Shop Editor")]
        public static void Open()
        {
            GetWindow<ShopEditor>("Shop Editor");
        }

        private void OnGUI()
        {
            database = (ShopDatabase)EditorGUILayout.ObjectField("Database", database, typeof(ShopDatabase), false);

            if (database == null)
            {
                if (GUILayout.Button("Create Database"))
                    CreateDatabase();
                return;
            }

            var so = new SerializedObject(database);
            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("Shops"), true);
            so.ApplyModifiedProperties();
        }

        private void CreateDatabase()
        {
            database = ScriptableObject.CreateInstance<ShopDatabase>();
            EnsureDataFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/ShopDatabase.asset");
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
