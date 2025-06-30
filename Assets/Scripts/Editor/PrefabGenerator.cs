using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Evolution.Dungeon;

namespace Evolution.Editor
{
    /// <summary>
    /// Utility editor window that creates room prefabs for every RoomType.
    /// </summary>
    public class PrefabGenerator : EditorWindow
    {
        private string tilesetFolder = "Assets/Resources/Models/Tiles/Base";
        private GameObject doorPrefab;
        private bool overwrite = true;

        [MenuItem("Adventure/Prefab Generator")]
        public static void Open()
        {
            GetWindow<PrefabGenerator>("Prefab Generator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Tileset Folder", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            tilesetFolder = EditorGUILayout.TextField(tilesetFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Tileset Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    if (selected.StartsWith(Application.dataPath))
                        tilesetFolder = "Assets" + selected.Substring(Application.dataPath.Length);
                    else
                        Debug.LogWarning("Selected folder must be inside the project Assets directory.");
                }
            }
            EditorGUILayout.EndHorizontal();

            doorPrefab = (GameObject)EditorGUILayout.ObjectField("Door Prefab", doorPrefab, typeof(GameObject), false);
            overwrite = EditorGUILayout.Toggle("Overwrite Existing", overwrite);

            GUILayout.Space(10);
            if (GUILayout.Button("Generate Prefabs"))
            {
                GeneratePrefabs();
            }
        }

        private void GeneratePrefabs()
        {
            if (doorPrefab == null)
            {
                EditorUtility.DisplayDialog("Prefab Generator", "Please assign a door prefab first.", "OK");
                return;
            }

            EnsureFolders();
            string relativePath = GetResourcesRelativePath(tilesetFolder);

            foreach (RoomType type in Enum.GetValues(typeof(RoomType)))
            {
                string path = $"Assets/Prefabs/Rooms/{type}.prefab";
                if (!overwrite && File.Exists(path))
                    continue;

                CreateRoomPrefab(type, relativePath, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CreateRoomPrefab(RoomType type, string tilePath, string savePath)
        {
            var root = new GameObject(type + "Room");
            var builder = root.AddComponent<RoomBuilder>();
            var room = root.AddComponent<RoomPrefab>();

            // assign tileset path on the serialized property
            var so = new SerializedObject(builder);
            so.FindProperty("tileSetPath").stringValue = tilePath;
            so.ApplyModifiedPropertiesWithoutUndo();

            float halfW = builder.RoomSize.x * builder.TileSize * 0.5f;
            float halfH = builder.RoomSize.y * builder.TileSize * 0.5f;

            GameObject north = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            north.name = "NorthDoor";
            north.transform.SetParent(root.transform, false);
            north.transform.localPosition = new Vector3(0f, 0f, halfH + builder.TileSize * 0.5f);
            north.transform.localRotation = Quaternion.identity;

            GameObject east = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            east.name = "EastDoor";
            east.transform.SetParent(root.transform, false);
            east.transform.localPosition = new Vector3(halfW + builder.TileSize * 0.5f, 0f, 0f);
            east.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            GameObject south = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            south.name = "SouthDoor";
            south.transform.SetParent(root.transform, false);
            south.transform.localPosition = new Vector3(0f, 0f, -halfH - builder.TileSize * 0.5f);
            south.transform.localRotation = Quaternion.identity;

            GameObject west = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            west.name = "WestDoor";
            west.transform.SetParent(root.transform, false);
            west.transform.localPosition = new Vector3(-halfW - builder.TileSize * 0.5f, 0f, 0f);
            west.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            room.NorthDoor = north.GetComponent<Door>();
            room.EastDoor = east.GetComponent<Door>();
            room.SouthDoor = south.GetComponent<Door>();
            room.WestDoor = west.GetComponent<Door>();

            PrefabUtility.SaveAsPrefabAsset(root, savePath);
            GameObject.DestroyImmediate(root);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Rooms"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Rooms");
        }

        private static string GetResourcesRelativePath(string assetPath)
        {
            const string resourcesRoot = "Assets/Resources/";
            if (assetPath.StartsWith(resourcesRoot))
                return assetPath.Substring(resourcesRoot.Length);
            return assetPath.StartsWith("Assets/") ? assetPath.Substring("Assets/".Length) : assetPath;
        }
    }
}
