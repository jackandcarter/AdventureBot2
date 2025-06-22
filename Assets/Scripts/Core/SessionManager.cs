using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Evolution.Core
{
    /// <summary>
    /// Tracks active game sessions and provides simple local persistence.
    /// Originally mirrored the Discord bot's SessionManager but now stores data
    /// on disk so no network connection is required.
    /// </summary>
    public class SessionManager : MonoBehaviour
    {
        private readonly Dictionary<int, SessionData> sessions = new();

        private const string SaveFolder = "Sessions";
        private string FolderPath => Path.Combine(Application.persistentDataPath, SaveFolder);

        private void Awake()
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
        }

        public SessionData GetSession(int id)
        {
            sessions.TryGetValue(id, out var sess);
            return sess;
        }

        public void RegisterSession(SessionData data)
        {
            if (data == null) return;
            sessions[data.SessionId] = data;
        }

        public void SaveSession(SessionData data)
        {
            if (data == null) return;
            sessions[data.SessionId] = data;
            string json = JsonUtility.ToJson(data);
            string path = Path.Combine(FolderPath, $"{data.SessionId}.json");
            File.WriteAllText(path, json);
        }

        public SessionData LoadSession(int sessionId)
        {
            string path = Path.Combine(FolderPath, $"{sessionId}.json");
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SessionData>(json);
            sessions[sessionId] = data;
            return data;
        }
    }

    [System.Serializable]
    public class SessionData
    {
        public int SessionId;
        public int OwnerId;
        public string Difficulty;
        public int CurrentFloor;
        public Vector2Int CurrentPosition;
        public Dungeon.DungeonData Dungeon;
        public List<int> Players = new();
    }
}
