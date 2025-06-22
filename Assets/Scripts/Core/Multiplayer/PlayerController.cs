using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Evolution.Dungeon;

namespace Evolution.Core.Multiplayer
{
    /// <summary>
    /// Networked player controller that syncs transform state and
    /// positions players at the dungeon entry when they join.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private DungeonGenerator dungeonGenerator;

        // Up to six spawn offsets around the entry point
        private static readonly Vector3[] SpawnOffsets =
        {
            Vector3.zero,
            new Vector3(1.5f, 0f, 0f),
            new Vector3(-1.5f, 0f, 0f),
            new Vector3(0f, 0f, 1.5f),
            new Vector3(0f, 0f, -1.5f),
            new Vector3(1.5f, 0f, 1.5f)
        };

        private void Start()
        {
            if (dungeonGenerator == null)
                dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                Vector3 basePos = GetEntryPosition();
                int index = (int)OwnerClientId % SpawnOffsets.Length;
                transform.position = basePos + SpawnOffsets[index];
            }
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 dir = new Vector3(h, 0f, v);
            if (dir.sqrMagnitude > 0f)
            {
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        private Vector3 GetEntryPosition()
        {
            if (dungeonGenerator != null)
                return dungeonGenerator.transform.position;
            return Vector3.zero;
        }
    }
}
