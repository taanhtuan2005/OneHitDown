using OneHitDown.Core;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Traps
{
    public enum PatrolMode { None, PingPong, Loop }
    public enum PatrolAxis { Horizontal, Vertical }

    public class TrapBase : MonoBehaviour, IResettable
    {
        [SerializeField] protected bool isLethal = true;
        [SerializeField] protected PatrolMode patrolMode = PatrolMode.None;
        [SerializeField] protected PatrolAxis patrolAxis = PatrolAxis.Horizontal;
        [SerializeField, Min(0f)] protected float patrolSpeed = 2.5f; // Tiles/giây.
        [SerializeField, Min(0f)] protected float patrolRange = 3f; // Tiles từ điểm xuất phát.
        protected Vector3 startPosition;
        protected float patrolTimer;

        protected virtual void Awake()
        {
            startPosition = transform.position;
            gameObject.layer = 10;
            gameObject.tag = isLethal ? "Hazard" : "Untagged";
        }

        protected virtual void Update()
        {
            patrolTimer += Time.deltaTime;
            if (patrolMode == PatrolMode.None || patrolRange <= 0f) return;
            float distance = patrolMode == PatrolMode.PingPong
                ? Mathf.PingPong(patrolTimer * patrolSpeed, patrolRange)
                : Mathf.Repeat(patrolTimer * patrolSpeed, patrolRange);
            transform.position = startPosition + PatrolDirection * distance;
        }

        protected Vector3 PatrolDirection => patrolAxis == PatrolAxis.Horizontal ? Vector3.right : Vector3.up;

        protected virtual void OnTriggerEnter2D(Collider2D col) => KillPlayer(col);
        protected virtual void OnCollisionEnter2D(Collision2D col) => KillPlayer(col.collider);
        // Stay bảo đảm nhân vật đang đứng trong vùng sát thương cũng bị xử lý.
        protected virtual void OnTriggerStay2D(Collider2D col) => KillPlayer(col);
        protected virtual void OnCollisionStay2D(Collision2D col) => KillPlayer(col.collider);

        protected void KillPlayer(Collider2D col)
        {
            if (!isLethal) return;
            var player = col.GetComponentInParent<PlayerController>();
            if (player != null && !player.IsDead) player.Die();
        }

        public virtual void ResetState()
        {
            transform.position = startPosition;
            patrolTimer = 0f;
        }
    }
}
