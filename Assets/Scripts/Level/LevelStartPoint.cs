using UnityEngine;

namespace OneHitDown.Level
{
    [RequireComponent(typeof(Animator))]
    public sealed class LevelStartPoint : MonoBehaviour
    {
        [SerializeField] private Transform p1SpawnPoint;
        [SerializeField] private Transform p2SpawnPoint;

        private Animator animator;

        public Transform P1SpawnPoint => p1SpawnPoint != null ? p1SpawnPoint : transform;
        public Transform P2SpawnPoint => p2SpawnPoint != null ? p2SpawnPoint : transform;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            PlayLaunchAnimation();
        }

        public void PlayLaunchAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Launch");
            }
        }
    }
}
