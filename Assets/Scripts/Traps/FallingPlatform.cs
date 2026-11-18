// Trần Văn Tuyên, phiên bản 1.0
// Module 3: Interactive Gadgets & Environmental Mechanics
using System.Collections;
using OneHitDown.Core;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Traps
{
    /// <summary>
    /// Bệ đá treo sụp lún: rung cảnh báo rồi rơi khỏi màn hình.
    /// Ở chế độ vĩnh viễn bệ không tự hồi trong lượt chơi; chỉ khôi phục khi
    /// <c>ResetLevel()</c> gọi <see cref="ResetState"/>.
    /// </summary>
    public class FallingPlatform : MonoBehaviour, IResettable
    {
        private const float ShakeAmplitude = 0.125f; // Biên độ rung ±2 px tại PPU = 16
        private const float ShakeFrequency = 25f;    // Tần số rung (Hz)
        private const float BottomFaceThreshold = -0.5f;
        private const float FallOutOfViewDistance = 20f; // Khoảng rơi trước khi ẩn hẳn khỏi khung hình

        [SerializeField] private float shakeDuration = 0.4f;         // Thời gian rung cảnh báo (giây)
        [SerializeField] private bool isPermanentInRun = true;       // Sập vĩnh viễn suốt lượt chơi
        [SerializeField] private float respawnDuration = 3.0f;       // Thời gian tự hồi ở chế độ luyện tập
        [SerializeField] private float fallSpeed = 12f;              // Vận tốc rơi (unit/giây)
        [SerializeField] private Sprite activeSprite;                // Sprite bệ nguyên vẹn (On)
        [SerializeField] private Sprite offSprite;                   // Sprite bệ đã sập (Off)
        [SerializeField] private BoxCollider2D solidCollider;        // Collider đỡ nhân vật
        [SerializeField] private SpriteRenderer spriteRenderer;      // Renderer hiển thị bệ đá

        private Vector3 startPosition;
        private bool isCollapsing;
        private Coroutine collapseRoutine;

        /// <summary>Bệ đang sập hay còn nguyên vẹn (đọc-only cho test và công cụ debug).</summary>
        public bool IsCollapsing => isCollapsing;

        /// <summary>Cấu hình chế độ sập vĩnh viễn hay tự hồi.</summary>
        public bool IsPermanentInRun => isPermanentInRun;

        /// <summary>Cache tham chiếu và vị trí gốc trước khi có bất kỳ va chạm nào.</summary>
        private void Awake()
        {
            startPosition = transform.position;

            if (solidCollider == null)
            {
                solidCollider = GetComponent<BoxCollider2D>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            ValidateShakeDuration();
            ValidateFallSpeed();
            ValidateSprites();
            ValidateRespawnDuration();
        }

        /// <summary>Kiểm tra dữ liệu Inspector khi thiết kế màn chơi.</summary>
        private void OnValidate()
        {
            ValidateShakeDuration();
            ValidateFallSpeed();
            ValidateSprites();
            ValidateRespawnDuration();
        }

        /// <summary>Dừng coroutine khi bệ bị tắt để không có logic chạy trên object đã ngủ.</summary>
        private void OnDisable()
        {
            StopAllCoroutines();
            collapseRoutine = null;
        }

        /// <summary>Chỉ sập khi nhân vật còn sống giẫm lên mặt trên của bệ.</summary>
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (isCollapsing)
            {
                return;
            }

            if (!col.collider.CompareTag("Player"))
            {
                return;
            }

            if (!col.collider.TryGetComponent(out PlayerController player))
            {
                return;
            }

            if (player.IsDead)
            {
                return;
            }

            if (!IsHitFromAbove(col))
            {
                return;
            }

            TriggerCollapse();
        }

        /// <summary>Khởi động chuỗi rung rồi rơi; guard <c>isCollapsing</c> chống kích hoạt đúp.</summary>
        public void TriggerCollapse()
        {
            if (isCollapsing)
            {
                return;
            }

            isCollapsing = true;
            collapseRoutine = StartCoroutine(CollapseSequence());
        }

        /// <summary>Rung cảnh báo trong <c>shakeDuration</c>, sau đó tắt collider, đổi sprite và cho bệ rơi.</summary>
        private IEnumerator CollapseSequence()
        {
            // Pha 1: rung cảnh báo tại chỗ, bệ vẫn đỡ được nhân vật
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                float offsetX = Mathf.Sin(elapsed * ShakeFrequency * Mathf.PI * 2f) * ShakeAmplitude;
                transform.position = startPosition + new Vector3(offsetX, 0f, 0f);

                yield return null;
                elapsed += Time.deltaTime;
            }

            transform.position = startPosition;

            // Pha 2: sập — tắt va chạm và đổi sprite ngay trong frame này
            if (solidCollider != null)
            {
                solidCollider.enabled = false;
            }

            if (spriteRenderer != null && offSprite != null)
            {
                spriteRenderer.sprite = offSprite;
            }

            // Pha 3: rơi khỏi khung hình
            float fallDistance = 0f;
            while (fallDistance < FallOutOfViewDistance)
            {
                float step = fallSpeed * Time.deltaTime;
                transform.position += new Vector3(0f, -step, 0f);
                fallDistance += step;

                yield return null;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            collapseRoutine = null;

            // Pha 4: chế độ luyện tập tự hồi sau respawnDuration; chế độ vĩnh viễn dừng tại đây
            if (!isPermanentInRun)
            {
                yield return new WaitForSeconds(respawnDuration);
                ResetState();
            }
        }

        /// <summary>Khôi phục bệ về trạng thái nguyên vẹn tức thì; gọi nhiều lần vẫn cho cùng kết quả.</summary>
        public void ResetState()
        {
            StopAllCoroutines();
            collapseRoutine = null;
            isCollapsing = false;

            transform.position = startPosition;

            if (solidCollider != null)
            {
                solidCollider.enabled = true;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                if (activeSprite != null)
                {
                    spriteRenderer.sprite = activeSprite;
                }
            }
        }

        /// <summary>Kiểm tra pháp tuyến va chạm để chỉ nhận cú giẫm từ trên xuống.</summary>
        private static bool IsHitFromAbove(Collision2D col)
        {
            int contactCount = col.contactCount;
            for (int i = 0; i < contactCount; i++)
            {
                if (col.GetContact(i).normal.y < BottomFaceThreshold)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Thời gian rung phải nằm trong khoảng (0, 2].</summary>
        private void ValidateShakeDuration()
        {
            if (shakeDuration > 0f && shakeDuration <= 2f)
            {
                return;
            }

            Debug.LogWarning($"[FallingPlatform] shakeDuration phải trong khoảng (0, 2], đang là {shakeDuration}.", this);
            shakeDuration = 0.4f;
        }

        /// <summary>Vận tốc rơi phải dương.</summary>
        private void ValidateFallSpeed()
        {
            if (fallSpeed > 0f)
            {
                return;
            }

            Debug.LogWarning("[FallingPlatform] fallSpeed phải > 0.", this);
            fallSpeed = 12f;
        }

        /// <summary>Cảnh báo khi thiếu sprite trạng thái.</summary>
        private void ValidateSprites()
        {
            if (activeSprite == null)
            {
                Debug.LogWarning("[FallingPlatform] activeSprite chưa được gán.", this);
            }

            if (offSprite == null)
            {
                Debug.LogWarning("[FallingPlatform] offSprite chưa được gán.", this);
            }
        }

        /// <summary>Chế độ luyện tập bắt buộc có thời gian hồi dương, nếu không sẽ coi như vĩnh viễn.</summary>
        private void ValidateRespawnDuration()
        {
            if (isPermanentInRun || respawnDuration > 0f)
            {
                return;
            }

            Debug.LogWarning("[FallingPlatform] respawnDuration phải > 0 khi isPermanentInRun = false.", this);
            respawnDuration = 3f;
            isPermanentInRun = true;
        }
    }
}