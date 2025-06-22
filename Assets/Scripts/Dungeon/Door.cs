using UnityEngine;

namespace Evolution.Dungeon
{
    /// <summary>
    /// Controls a dungeon door, handling locked state, animations,
    /// and collision.
    /// </summary>
    public class Door : MonoBehaviour
    {
        [SerializeField] private bool locked = false;
        [SerializeField] private Animator animator;
        [SerializeField] private Collider doorCollider;

        private static readonly int OpenHash = Animator.StringToHash("Open");
        private static readonly int CloseHash = Animator.StringToHash("Close");

        /// <summary>
        /// True if the door is currently locked.
        /// </summary>
        public bool IsLocked => locked;

        /// <summary>
        /// True if the door has been opened.
        /// </summary>
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (doorCollider == null)
                doorCollider = GetComponent<Collider>();
        }

        /// <summary>
        /// Unlock the door so it can be opened.
        /// </summary>
        public void Unlock()
        {
            locked = false;
        }

        /// <summary>
        /// Open the door if it is unlocked. Disables its collider and
        /// plays an animation if available.
        /// </summary>
        public void Open()
        {
            if (locked || IsOpen)
                return;
            IsOpen = true;
            if (animator != null)
                animator.SetTrigger(OpenHash);
            if (doorCollider != null)
                doorCollider.enabled = false;
        }

        /// <summary>
        /// Close the door and re-enable collision.
        /// </summary>
        public void Close()
        {
            if (!IsOpen)
                return;
            IsOpen = false;
            if (animator != null)
                animator.SetTrigger(CloseHash);
            if (doorCollider != null)
                doorCollider.enabled = true;
        }
    }
}
