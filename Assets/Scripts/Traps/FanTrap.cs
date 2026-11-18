// Trần Văn Tuyên, phiên bản 1.0
// Module 3: Interactive Gadgets & Environmental Mechanics
using OneHitDown.Core;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Traps
{
    /// <summary>
    /// Cối xay quạt gió: thổi nhân vật theo phương đứng hoặc ngang.
    /// Có thể bật/tắt động trong màn chơi và khôi phục trạng thái ban đầu qua <see cref="IResettable"/>.
    /// </summary>
    public class FanTrap : MonoBehaviour, IResettable
    {
        private const float DefaultWindForce = 15f;
        private const float HorizontalForceMultiplier = 0.35f;
        private const float VerticalAxisThreshold = 0.5f;
        private const float AnimationFrameDuration = 0.05f; // 20 FPS cho 4 frames cánh quạt

        [SerializeField] private Vector2 windDirection = Vector2.up; // Hướng gió thổi (dọc (0,1) hoặc ngang (1,0)/(-1,0))
        [SerializeField] private float windForce = 15f;              // Cường độ luồng gió
        [SerializeField] private bool isActive = true;               // Trạng thái quạt đang chạy hay dừng
        [SerializeField] private Sprite[] animationFrames;           // 4 frames quay cánh quạt (Fan/On (24x8).png)
        [SerializeField] private Sprite offSprite;                   // Sprite quạt tắt (Fan/Off.png)
        [SerializeField] private SpriteRenderer spriteRenderer;      // Renderer hiển thị cánh quạt

        private bool initialActiveState = true;
        private float frameTimer;
        private int currentFrameIndex;

        /// <summary>Hướng gió đã chuẩn hóa (đọc-only cho test và các cơ chế phối hợp).</summary>
        public Vector2 WindDirection => windDirection;

        /// <summary>Cường độ lực gió hiện tại.</summary>
        public float WindForce => windForce;

        /// <summary>Trạng thái luồng gió đang thổi hay tắt.</summary>
        public bool IsActive => isActive;

        /// <summary>Lưu trạng thái ban đầu để khôi phục qua <see cref="ResetState"/>.</summary>
        private void Awake()
        {
            initialActiveState = isActive;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // Collider gió nằm ở child WindZone: Unity 2D chỉ chuyển sự kiện trigger của child lên script ở gốc
            // khi gốc có Rigidbody2D. Bổ sung Rigidbody2D Kinematic nếu prefab chưa có để quạt luôn thổi được.
            if (!TryGetComponent(out Rigidbody2D body))
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;

            ValidateWindDirection();
            ValidateWindForce();
            ValidateAnimationFrames();
            UpdateVisualState();
        }

        /// <summary>Kiểm tra dữ liệu Inspector khi thiết kế màn chơi.</summary>
        private void OnValidate()
        {
            ValidateWindDirection();
            ValidateWindForce();
            ValidateAnimationFrames();
        }

        /// <summary>Tự lặp 4 frames cánh quạt khi đang hoạt động, không cấp phát rác.</summary>
        private void Update()
        {
            if (!isActive || animationFrames == null || animationFrames.Length == 0 || spriteRenderer == null)
            {
                return;
            }

            frameTimer += Time.deltaTime;
            if (frameTimer < AnimationFrameDuration)
            {
                return;
            }

            frameTimer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[currentFrameIndex];
        }

        /// <summary>
        /// Áp dụng lực gió liên tục mỗi FixedUpdate lên nhân vật đứng trong vùng gió (WindZone).
        /// Tuân thủ quy chuẩn Zero-GC: so tag trước khi GetComponent, không LINQ, không new object.
        /// </summary>
        private void OnTriggerStay2D(Collider2D col)
        {
            if (!isActive)
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

            // Thổi dọc: nâng hoặc giữ vận tốc Y không nhỏ hơn sức gió
            if (Mathf.Abs(windDirection.y) > VerticalAxisThreshold)
            {
                Rigidbody2D rb = player.Rigidbody;
                if (rb != null)
                {
                    Vector2 vel = rb.linearVelocity;
                    vel.y = Mathf.Max(vel.y, windDirection.y * windForce);
                    rb.linearVelocity = vel;
                }
            }
            // Thổi ngang: truyền xung lực phụ thuộc hướng gió
            else
            {
                player.AddExternalVelocity(new Vector2(windDirection.x * windForce * HorizontalForceMultiplier, 0f));
            }
        }

        /// <summary>Bật hoặc tắt quạt từ công tắc cơ quan trong màn chơi.</summary>
        public void SetActive(bool active)
        {
            isActive = active;
            UpdateVisualState();
        }

        /// <summary>Khôi phục trạng thái quạt khi nhân vật chết hoặc chơi lại màn chơi.</summary>
        public void ResetState()
        {
            isActive = initialActiveState;
            currentFrameIndex = 0;
            frameTimer = 0f;
            UpdateVisualState();
        }

        /// <summary>Cập nhật ngay sprite quạt sang bật/tắt mà không có độ trễ cảm nhận.</summary>
        private void UpdateVisualState()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (isActive)
            {
                if (animationFrames != null && animationFrames.Length > 0)
                {
                    spriteRenderer.sprite = animationFrames[currentFrameIndex];
                }
            }
            else
            {
                if (offSprite != null)
                {
                    spriteRenderer.sprite = offSprite;
                }
                else if (animationFrames != null && animationFrames.Length > 0)
                {
                    spriteRenderer.sprite = animationFrames[0];
                }
            }
        }

        /// <summary>Chuẩn hóa hướng gió; vector zero được tự chuyển về hướng lên trên.</summary>
        private void ValidateWindDirection()
        {
            if (windDirection.sqrMagnitude < 0.001f)
            {
                Debug.LogWarning("[FanTrap] windDirection không được là zero. Tự gán Vector2.up.", this);
                windDirection = Vector2.up;
                return;
            }

            windDirection.Normalize();
        }

        /// <summary>Cảnh báo nếu sức gió không dương.</summary>
        private void ValidateWindForce()
        {
            if (windForce > 0f)
            {
                return;
            }

            Debug.LogWarning($"[FanTrap] windForce phải > 0, đang là {windForce}.", this);
            windForce = DefaultWindForce;
        }

        /// <summary>Kiểm tra mảng sprite hoạt ảnh cánh quạt.</summary>
        private void ValidateAnimationFrames()
        {
            if (animationFrames == null || animationFrames.Length == 0)
            {
                Debug.LogWarning($"[FanTrap] animationFrames trống trên {name}.", this);
            }
        }
    }
}