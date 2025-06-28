using UnityEditor;
using UnityEngine;
using Evolution.Data;

namespace Evolution.Editor
{
    public class RoomTypeEditor : EditorWindow
    {
        private RoomDatabase database;

        [MenuItem("Adventure/Room Editor")]
        public static void Open()
        {
            GetWindow<RoomTypeEditor>("Room Editor");
        }

        private void OnGUI()
        {
            database = (RoomDatabase)EditorGUILayout.ObjectField("Database", database, typeof(RoomDatabase), false);

            if (database == null)
            {
                if (GUILayout.Button("Create Database"))
                    CreateDatabase();
                return;
            }

            var so = new SerializedObject(database);
            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("Rooms"), true);
            so.ApplyModifiedProperties();
        }

        private void CreateDatabase()
        {
            database = ScriptableObject.CreateInstance<RoomDatabase>();
            EnsureDataFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/RoomDatabase.asset");
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
