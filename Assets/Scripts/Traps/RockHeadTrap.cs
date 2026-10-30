using System.Collections;
using UnityEngine;

namespace OneHitDown.Traps
{
    public class RockHeadTrap : TrapBase
    {
        [SerializeField] private float detectionWidth = 2.5f;
        [SerializeField] private float detectionDistance = 6f;
        [SerializeField] private float reactionDelay = 0.25f;
        [SerializeField] private float slamSpeed = 18f;
        [SerializeField] private float retractSpeed = 4f;
        [SerializeField] private float bottomStayTime = 0.5f;
        [SerializeField] private AudioClip impactClip;
        private Collider2D hitbox;
        private Animator animator;
        private AudioSource audioSource;
        private bool busy;
        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
        private ContactFilter2D groundFilter;

        protected override void Awake()
        {
            base.Awake();
            hitbox = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            groundFilter.SetLayerMask(1 << 8);
            groundFilter.useTriggers = false;
        }

        protected override void Update()
        {
            if (busy || hitbox == null) return;
            Bounds bounds = hitbox.bounds;
            var hit = Physics2D.BoxCast(new Vector2(bounds.center.x, bounds.min.y),
                new Vector2(detectionWidth, 0.05f), 0f, Vector2.down, detectionDistance, 1 << 9);
            if (hit.collider != null) StartCoroutine(SlamRoutine());
        }

        private void OnEnable() => ResetState();
        private void OnDisable()
        {
            StopAllCoroutines();
            busy = false;
        }

        private void Play(string state)
        {
            if (animator != null && animator.runtimeAnimatorController != null) animator.Play(state, 0, 0f);
        }

        private IEnumerator SlamRoutine()
        {
            busy = true;
            Play("Blink");
            yield return new WaitForSeconds(reactionDelay);
            float travelled = 0f;
            // Giới hạn hành trình nếu scene chưa có sàn, tránh rơi vô hạn.
            while (travelled < detectionDistance)
            {
                float step = Mathf.Min(slamSpeed * Time.deltaTime, detectionDistance - travelled);
                int count = hitbox.Cast(Vector2.down, groundFilter, groundHits, step);
                float allowed = step;
                for (int i = 0; i < count; i++) allowed = Mathf.Min(allowed, groundHits[i].distance);
                transform.position += Vector3.down * allowed;
                travelled += allowed;
                if (count > 0) break;
                yield return null;
            }
            Play("Bottom Hit");
            if (audioSource != null && impactClip != null) audioSource.PlayOneShot(impactClip);
            yield return new WaitForSeconds(bottomStayTime);
            while ((transform.position - startPosition).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, startPosition, retractSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = startPosition;
            Play("Idle");
            busy = false;
        }

        public override void ResetState()
        {
            StopAllCoroutines();
            base.ResetState();
            busy = false;
            if (audioSource != null) audioSource.Stop();
            Play("Idle");
        }
    }
}
