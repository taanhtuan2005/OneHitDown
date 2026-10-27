// Trần Văn Tuyên, phiên bản 1.0
// Module 3: Interactive Gadgets & Environmental Mechanics
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Traps
{
    /// <summary>
    /// Đệm nhún lò xo đa hướng: bật nhân vật theo <c>bounceDirection</c> và hồi ngay 1 lượt Nhảy Đôi.
    /// Cơ quan không giữ trạng thái riêng nên không cần triển khai <c>IResettable</c>.
    /// </summary>
    public class TrampolineTrap : MonoBehaviour
    {
        private const string JumpStateName = "Jump";
        private const float DefaultBounceForce = 20f;

        [SerializeField] private float bounceForce = 20f;              // Lực bật lò xo (unit/giây)
        [SerializeField] private Vector2 bounceDirection = Vector2.up; // Hướng đẩy, tự chuẩn hóa về độ dài 1
        [SerializeField] private Animator animator;                    // Animator chạy clip Idle / Jump

        /// <summary>Lực bật hiện tại (đọc-only cho test và công cụ debug).</summary>
        public float BounceForce => bounceForce;

        /// <summary>Hướng đẩy đã chuẩn hóa (đọc-only cho test và công cụ debug).</summary>
        public Vector2 BounceDirection => bounceDirection;

        /// <summary>Lấy tham chiếu Animator và chuẩn hóa dữ liệu Inspector trước khi có sự kiện vật lý nào.</summary>
        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            ValidateBounceDirection();
            ValidateBounceForce();

            if (animator == null)
            {
                Debug.LogWarning($"[TrampolineTrap] animator chưa được gán trên {name}.", this);
            }
        }

        /// <summary>Cảnh báo và tự sửa dữ liệu sai ngay khi chỉnh trong Inspector.</summary>
        private void OnValidate()
        {
            ValidateBounceDirection();
            ValidateBounceForce();
        }

        /// <summary>Chỉ nhận collider của nhân vật còn sống; collider khác (quả, mảnh vỡ, bẫy) bỏ qua im lặng.</summary>
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player"))
            {
                return;
            }

            if (!col.TryGetComponent(out PlayerController player))
            {
                return;
            }

            if (player.IsDead)
            {
                return;
            }

            Bounce(player);
        }

        /// <summary>Đẩy nhân vật theo hướng lò xo và hồi 1 lượt Nhảy Đôi, sau đó phát animation Jump.</summary>
        private void Bounce(PlayerController player)
        {
            player.GrantDoubleJumpAndBoost(bounceDirection * bounceForce);

            if (animator != null)
            {
                animator.Play(JumpStateName, 0, 0f);
            }
        }

        /// <summary>Chuẩn hóa hướng đẩy; vector zero được thay bằng Vector2.up để nhân vật không kẹt tại chỗ.</summary>
        private void ValidateBounceDirection()
        {
            if (bounceDirection.sqrMagnitude < 0.001f)
            {
                Debug.LogWarning("[TrampolineTrap] bounceDirection không được là Vector2.zero. Tự gán Vector2.up.", this);
                bounceDirection = Vector2.up;
                return;
            }

            bounceDirection.Normalize();
        }

        /// <summary>Lực bật phải dương; giá trị sai được đưa về mức chuẩn 20f của toàn dự án.</summary>
        private void ValidateBounceForce()
        {
            if (bounceForce > 0f)
            {
                return;
            }

            Debug.LogWarning($"[TrampolineTrap] bounceForce phải > 0, đang là {bounceForce}. Tự reset về 20f.", this);
            bounceForce = DefaultBounceForce;
        }
    }
}