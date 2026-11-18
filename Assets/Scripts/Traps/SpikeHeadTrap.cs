using System.Collections;
using UnityEngine;

namespace OneHitDown.Traps
{
    public class SpikeHeadTrap : TrapBase
    {
        [SerializeField] private Vector2 dashDirection = Vector2.right;
        [SerializeField] private float dashDistance = 4f;
        [SerializeField] private float dashSpeed = 16f;
        [SerializeField] private float returnSpeed = 5f;
        [SerializeField] private float cycleDelay = 1.2f;
        private Animator animator;
        private Collider2D hitbox;
        private ContactFilter2D groundFilter;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[4];

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
            hitbox = GetComponent<Collider2D>();
            dashDirection = dashDirection.sqrMagnitude > 0f ? dashDirection.normalized : Vector2.right;
            groundFilter.SetLayerMask(1 << 8);
            groundFilter.useTriggers = false;
        }

        private void OnEnable() => ResetState();
        private void OnDisable() => StopAllCoroutines();
        protected override void Update() { } // Chu kỳ riêng, không chạy tuần tra của lớp cha.

        private void Play(string state)
        {
            if (animator != null && animator.runtimeAnimatorController != null) animator.Play(state, 0, 0f);
        }

        private IEnumerator DashCycleRoutine()
        {
            while (true)
            {
                Play("Blink");
                yield return new WaitForSeconds(cycleDelay);
                float travelled = 0f;
                while (travelled < dashDistance)
                {
                    float step = Mathf.Min(dashSpeed * Time.deltaTime, dashDistance - travelled);
                    int count = hitbox != null ? hitbox.Cast(dashDirection, groundFilter, hits, step) : 0;
                    float allowed = step;
                    for (int i = 0; i < count; i++) allowed = Mathf.Min(allowed, hits[i].distance);
                    transform.position += (Vector3)(dashDirection * allowed);
                    travelled += allowed;
                    if (count > 0) break;
                    yield return null;
                }
                Play(Mathf.Abs(dashDirection.x) >= Mathf.Abs(dashDirection.y)
                    ? (dashDirection.x > 0 ? "Right Hit" : "Left Hit")
                    : (dashDirection.y > 0 ? "Top Hit" : "Bottom Hit"));
                while ((transform.position - startPosition).sqrMagnitude > 0.0001f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, startPosition, returnSpeed * Time.deltaTime);
                    yield return null;
                }
                transform.position = startPosition;
            }
        }

        public override void ResetState()
        {
            StopAllCoroutines();
            base.ResetState();
            Play("Idle");
            if (isActiveAndEnabled) StartCoroutine(DashCycleRoutine());
        }
    }
}
