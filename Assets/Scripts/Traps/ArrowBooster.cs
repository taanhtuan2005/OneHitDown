// Trần Văn Tuyên, phiên bản 1.0
// Module 3: Interactive Gadgets & Environmental Mechanics
using System.Collections;
using OneHitDown.Core;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Traps
{
    /// <summary>
    /// Bong Bóng Mũi Tên Đẩy Khí & Hồi Nhảy Đôi (Bẫy/Cơ quan thứ 13):
    /// Đặt lơ lửng trên các vực thẳm rộng hoặc hố tử thần. Khi chạm vào, nhân vật được đẩy bật
    /// theo hướng mũi tên (<c>boostForce = 14f</c>) và được hồi ngay 1 lượt Nhảy Đôi.
    /// Sau khi kích hoạt, mũi tên nổ bung và ẩn trong <c>reactivateDelay = 2.5s</c>.
    /// </summary>
    public class ArrowBooster : MonoBehaviour, IResettable
    {
        private const string IdleStateName = "Idle";
        private const string HitStateName = "Hit";
        private const float DefaultBoostForce = 14f;
        private const float DefaultReactivateDelay = 2.5f;
        private const float HitAnimationDuration = 0.2f; // Thời gian chạy clip nổ Hit (4 frames)

        [SerializeField] private float boostForce = 14f;              // Lực đẩy mũi tên (unit/giây)
        [SerializeField] private Vector2 boostDirection = Vector2.up; // Hướng đẩy (mặc định hướng lên, tự normalize)
        [SerializeField] private float reactivateDelay = 2.5f;        // Thời gian ẩn trước khi tự hồi (giây)
        [SerializeField] private Animator animator;                   // Animator điều khiển clip Idle và Hit
        [SerializeField] private CircleCollider2D boosterCollider;    // Collider trigger nhận va chạm
        [SerializeField] private SpriteRenderer spriteRenderer;       // Renderer hiển thị mũi tên

        private bool isReady = true;
        private Coroutine cooldownRoutine;

        /// <summary>Lực đẩy mũi tên (đọc-only cho test).</summary>
        public float BoostForce => boostForce;

        /// <summary>Hướng đẩy đã chuẩn hóa.</summary>
        public Vector2 BoostDirection => boostDirection;

        /// <summary>Thời gian hồi chiêu.</summary>
        public float ReactivateDelay => reactivateDelay;

        /// <summary>Trạng thái mũi tên đã sẵn sàng kích hoạt hay đang hồi chiêu.</summary>
        public bool IsReady => isReady;

        /// <summary>Cache component và chuẩn hóa vector hướng trước khi chạy.</summary>
        private void Awake()
        {
            if (boosterCollider == null)
            {
                boosterCollider = GetComponent<CircleCollider2D>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            ValidateBoostDirection();
            ValidateBoostForce();
            ValidateReactivateDelay();

            if (animator == null)
            {
                Debug.LogWarning("[ArrowBooster] animator chưa gán.", this);
            }
        }

        /// <summary>Kiểm tra dữ liệu Inspector khi thiết kế màn chơi.</summary>
        private void OnValidate()
        {
            ValidateBoostDirection();
            ValidateBoostForce();
            ValidateReactivateDelay();
        }

        /// <summary>Dọn coroutine khi object bị tắt để chống rò rỉ logic.</summary>
        private void OnDisable()
        {
            StopAllCoroutines();
            cooldownRoutine = null;
        }

        /// <summary>Chỉ kích hoạt khi mũi tên đang sẵn sàng và nhân vật còn sống chạm vào.</summary>
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!isReady)
            {
                return;
            }

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

            TriggerBoost(player);
        }

        /// <summary>
        /// Kích hoạt lực đẩy, hồi Nhảy Đôi cho nhân vật và chuyển mũi tên sang trạng thái hồi chiêu.
        /// Sử dụng hai lớp bảo vệ chống kích hoạt đúp: cờ logic <c>isReady</c> và tắt collider vật lý ngay lập tức.
        /// </summary>
        public void TriggerBoost(PlayerController player)
        {
            if (!isReady)
            {
                return;
            }

            isReady = false;

            if (boosterCollider != null)
            {
                boosterCollider.enabled = false;
            }

            player.GrantDoubleJumpAndBoost(boostDirection * boostForce);

            cooldownRoutine = StartCoroutine(CooldownRoutine());
        }

        /// <summary>Chạy clip Hit nổ bung, ẩn sprite trong 2.5s rồi khôi phục lại trạng thái Ready.</summary>
        private IEnumerator CooldownRoutine()
        {
            if (animator != null)
            {
                animator.Play(HitStateName, 0, 0f);
            }

            yield return new WaitForSeconds(HitAnimationDuration);

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            float remainingDelay = Mathf.Max(0f, reactivateDelay - HitAnimationDuration);
            yield return new WaitForSeconds(remainingDelay);

            RestoreVisualAndPhysics();
            cooldownRoutine = null;
        }

        /// <summary>Khôi phục mũi tên về trạng thái sẵn sàng tức thì (idempotent).</summary>
        public void ResetState()
        {
            StopAllCoroutines();
            cooldownRoutine = null;

            RestoreVisualAndPhysics();
        }

        /// <summary>Khôi phục collider, renderer và đưa animator về clip Idle ban đầu.</summary>
        private void RestoreVisualAndPhysics()
        {
            isReady = true;

            if (boosterCollider != null)
            {
                boosterCollider.enabled = true;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            if (animator != null)
            {
                animator.Play(IdleStateName, 0, 0f);
            }
        }

        /// <summary>Chuẩn hóa hướng đẩy; vector zero được tự chuyển về hướng lên trên.</summary>
        private void ValidateBoostDirection()
        {
            if (boostDirection.sqrMagnitude < 0.001f)
            {
                Debug.LogWarning("[ArrowBooster] boostDirection zero. Tự gán Vector2.up.", this);
                boostDirection = Vector2.up;
                return;
            }

            boostDirection.Normalize();
        }

        /// <summary>Cảnh báo nếu lực đẩy không dương.</summary>
        private void ValidateBoostForce()
        {
            if (boostForce > 0f)
            {
                return;
            }

            Debug.LogWarning("[ArrowBooster] boostForce phải > 0.", this);
            boostForce = DefaultBoostForce;
        }

        /// <summary>Cảnh báo nếu thời gian hồi không dương.</summary>
        private void ValidateReactivateDelay()
        {
            if (reactivateDelay > 0f)
            {
                return;
            }

            Debug.LogWarning("[ArrowBooster] reactivateDelay phải > 0.", this);
            reactivateDelay = DefaultReactivateDelay;
        }
    }
}