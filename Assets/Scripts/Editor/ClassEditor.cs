using UnityEditor;
using UnityEngine;
using Evolution.Data;

namespace Evolution.Editor
{
    public class ClassEditor : EditorWindow
    {
        private ClassDatabase database;
        private StatsDatabase statsDatabase;
        private Vector2 scroll;

        [MenuItem("Adventure/Class Editor")]
        public static void Open()
        {
            GetWindow<ClassEditor>("Class Editor");
        }

        private void OnGUI()
        {
            database = (ClassDatabase)EditorGUILayout.ObjectField("Database", database, typeof(ClassDatabase), false);
            statsDatabase = (StatsDatabase)EditorGUILayout.ObjectField("Stats Database", statsDatabase, typeof(StatsDatabase), false);

            if (database == null)
            {
                if (GUILayout.Button("Create Database"))
                    CreateDatabase();
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            var so = new SerializedObject(database);
            so.Update();
            SerializedProperty classesProp = so.FindProperty("Classes");

            for (int i = 0; i < classesProp.arraySize; i++)
            {
                SerializedProperty classProp = classesProp.GetArrayElementAtIndex(i);
                SerializedProperty className = classProp.FindPropertyRelative("ClassName");
                SerializedProperty statsProp = classProp.FindPropertyRelative("Stats");
                SerializedProperty abilitiesProp = classProp.FindPropertyRelative("Abilities");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(className);

                EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
                for (int s = 0; s < statsProp.arraySize; s++)
                {
                    SerializedProperty statProp = statsProp.GetArrayElementAtIndex(s);
                    SerializedProperty defProp = statProp.FindPropertyRelative("Stat");
                    SerializedProperty valueProp = statProp.FindPropertyRelative("Value");

                    EditorGUILayout.BeginHorizontal();
                    defProp.objectReferenceValue = EditorGUILayout.ObjectField(defProp.objectReferenceValue, typeof(StatDefinition), false);
                    valueProp.floatValue = EditorGUILayout.FloatField(valueProp.floatValue);
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        statsProp.DeleteArrayElementAtIndex(s);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Stat"))
                {
                    statsProp.InsertArrayElementAtIndex(statsProp.arraySize);
                    SerializedProperty newStat = statsProp.GetArrayElementAtIndex(statsProp.arraySize - 1);
                    newStat.FindPropertyRelative("Value").floatValue = 0f;
                }

                EditorGUILayout.PropertyField(abilitiesProp, true);

                if (GUILayout.Button("Remove Class"))
                {
                    classesProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Class"))
            {
                classesProp.InsertArrayElementAtIndex(classesProp.arraySize);
            }

            so.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
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
