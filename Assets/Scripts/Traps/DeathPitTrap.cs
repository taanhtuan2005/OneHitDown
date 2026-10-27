// Trần Văn Tuyên, phiên bản 1.0
// Module 3: Interactive Gadgets & Environmental Mechanics
using System.Collections;
using OneHitDown.Core;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Traps
{
    /// <summary>
    /// Bẫy Hố Tử Thần Vĩnh Viễn ngụy trang bằng sàn gạch Blocks.
    /// Khi nhân vật giẫm lên mặt trên, gạch nứt rồi vỡ, mở toang vùng bẫy chết người (Tag Hazard).
    /// Hố giữ nguyên trạng thái mở suốt lượt chơi; chỉ khôi phục qua <see cref="IResettable.ResetState"/>.
    /// </summary>
    public class DeathPitTrap : MonoBehaviour, IResettable
    {
        private const float ShakeAmplitude = 0.08f;  // Biên độ rung nhẹ lúc nứt gạch
        private const float ShakeFrequency = 30f;    // Tần số rung nứt (Hz)
        private const float BottomFaceThreshold = -0.5f;
        private const float DebrisGravityScale = 2.5f;
        private const float DebrisSpreadVelocity = 2.5f;   // Vận tốc ngang của mảnh vỡ (unit/giây)
        private const float DebrisUpwardVelocity = 3.5f;   // Vận tốc nảy lên của mảnh vỡ (unit/giây)
        private const float DebrisLifetime = 1.0f;         // Thời gian tồn tại của mảnh vỡ VFX (giây)

        [SerializeField] private float shakeDuration = 0.2f;        // Thời gian rung và nứt gạch (giây)
        [SerializeField] private BoxCollider2D solidCollider;       // Collider sàn lúc chưa sập (isTrigger = false)
        [SerializeField] private BoxCollider2D deathTrigger;        // Child trigger mang Tag Hazard (isTrigger = true)
        [SerializeField] private SpriteRenderer blockRenderer;      // Renderer hiển thị khối gạch
        [SerializeField] private Sprite idleSprite;                 // Sprite gạch nguyên vẹn (Blocks/Idle.png)
        [SerializeField] private Sprite crackSprite;                // Sprite nứt mặt trên (Blocks/HitTop (22x22).png)
        [SerializeField] private Sprite part1Sprite;                // Mảnh vỡ 1 (Blocks/Part 1 (22x22).png)
        [SerializeField] private Sprite part2Sprite;                // Mảnh vỡ 2 (Blocks/Part 2 (22x22).png)

        private Vector3 startPosition;
        private bool isTriggered;
        private Coroutine collapseRoutine;
        private GameObject debrisLeft;
        private GameObject debrisRight;

        /// <summary>Trạng thái hố đã sập hay chưa (đọc-only cho test).</summary>
        public bool IsTriggered => isTriggered;

        /// <summary>Thời gian rung cảnh báo trước khi gạch vỡ hoàn toàn.</summary>
        public float ShakeDuration => shakeDuration;

        /// <summary>Khởi tạo trạng thái ban đầu: sàn đóng, trigger chết người tắt.</summary>
        private void Awake()
        {
            startPosition = transform.position;

            if (solidCollider == null)
            {
                solidCollider = GetComponent<BoxCollider2D>();
            }

            if (blockRenderer == null)
            {
                blockRenderer = GetComponent<SpriteRenderer>();
            }

            ValidateDeathTrigger();
            ValidateSolidCollider();
            ValidateBlockRenderer();
            ValidateShakeDuration();

            // Đảm bảo trạng thái đầu trận: sàn solid bật, trigger Hazard tắt
            if (solidCollider != null)
            {
                solidCollider.enabled = true;
            }

            if (deathTrigger != null)
            {
                deathTrigger.gameObject.SetActive(false);
            }

            if (blockRenderer != null && idleSprite != null)
            {
                blockRenderer.sprite = idleSprite;
            }
        }

        /// <summary>Kiểm tra dữ liệu Inspector khi thiết kế màn chơi.</summary>
        private void OnValidate()
        {
            ValidateShakeDuration();
        }

        /// <summary>Dọn coroutine khi bẫy bị tắt để chống rò rỉ logic.</summary>
        private void OnDisable()
        {
            StopAllCoroutines();
            collapseRoutine = null;
            ClearDebris();
        }

        /// <summary>Chỉ kích hoạt sập khi nhân vật còn sống giẫm lên mặt trên của khối gạch.</summary>
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (isTriggered)
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

        /// <summary>Bắt đầu chuỗi sập hố; guard <c>isTriggered</c> chống chạy đúp khi cả 2 người cùng giẫm.</summary>
        public void TriggerCollapse()
        {
            if (isTriggered)
            {
                return;
            }

            isTriggered = true;
            collapseRoutine = StartCoroutine(CollapseRoutine());
        }

        /// <summary>Hiển thị sprite nứt, rung trong 0.2s, sau đó mở toang hố tử thần suốt lượt chơi.</summary>
        private IEnumerator CollapseRoutine()
        {
            // Pha 1: đổi sprite nứt mặt trên
            if (blockRenderer != null && crackSprite != null)
            {
                blockRenderer.sprite = crackSprite;
            }

            // Pha 2: rung nứt
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                float offsetX = Mathf.Sin(elapsed * ShakeFrequency * Mathf.PI * 2f) * ShakeAmplitude;
                transform.position = startPosition + new Vector3(offsetX, 0f, 0f);

                yield return null;
                elapsed += Time.deltaTime;
            }

            transform.position = startPosition;

            // Pha 3: mở toang hố tử thần và tung 2 mảnh vỡ
            if (solidCollider != null)
            {
                solidCollider.enabled = false;
            }

            if (blockRenderer != null)
            {
                blockRenderer.enabled = false;
            }

            SpawnDebris();

            if (deathTrigger != null)
            {
                deathTrigger.gameObject.SetActive(true);
            }

            collapseRoutine = null;
        }

        /// <summary>
        /// Tung 2 mảnh vỡ sang hai bên rồi tự hủy sau <c>DebrisLifetime</c> giây.
        /// Mảnh vỡ được tạo dưới dạng GameObject tạm, không cấp phát trong vòng lặp vật lý nào.
        /// </summary>
        private void SpawnDebris()
        {
            if (part1Sprite == null && part2Sprite == null)
            {
                // Thiếu asset VFX không được phép chặn logic mở hố (spec §9.3)
                return;
            }

            debrisLeft = CreateDebrisPiece("DebrisLeft", part1Sprite, -1f);
            debrisRight = CreateDebrisPiece("DebrisRight", part2Sprite, 1f);
        }

        /// <summary>Tạo một mảnh vỡ có Rigidbody2D để nảy ra ngoài, tự hủy sau thời gian quy định.</summary>
        private GameObject CreateDebrisPiece(string pieceName, Sprite sprite, float horizontalSign)
        {
            if (sprite == null)
            {
                return null;
            }

            GameObject debris = new GameObject(pieceName);
            debris.layer = gameObject.layer;
            debris.transform.position = transform.position + new Vector3(0.25f * horizontalSign, 0.15f, 0f);

            SpriteRenderer renderer = debris.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerID = blockRenderer != null ? blockRenderer.sortingLayerID : 0;
            renderer.sortingOrder = blockRenderer != null ? blockRenderer.sortingOrder + 1 : 1;

            Rigidbody2D body = debris.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = DebrisGravityScale;
            body.freezeRotation = false;
            body.linearVelocity = new Vector2(DebrisSpreadVelocity * horizontalSign, DebrisUpwardVelocity);

            Destroy(debris, DebrisLifetime);
            return debris;
        }

        /// <summary>Hủy ngay các mảnh vỡ còn sót lại khi reset hoặc khi bẫy bị tắt.</summary>
        private void ClearDebris()
        {
            if (debrisLeft != null)
            {
                Destroy(debrisLeft);
                debrisLeft = null;
            }

            if (debrisRight != null)
            {
                Destroy(debrisRight);
                debrisRight = null;
            }
        }

        /// <summary>Khôi phục lại khối gạch nguyên vẹn khi nhân vật chết và hồi sinh.</summary>
        public void ResetState()
        {
            StopAllCoroutines();
            collapseRoutine = null;
            isTriggered = false;
            ClearDebris();

            transform.position = startPosition;

            if (solidCollider != null)
            {
                solidCollider.enabled = true;
            }

            if (blockRenderer != null)
            {
                blockRenderer.enabled = true;
                if (idleSprite != null)
                {
                    blockRenderer.sprite = idleSprite;
                }
            }

            if (deathTrigger != null)
            {
                deathTrigger.gameObject.SetActive(false);
            }
        }

        /// <summary>Kiểm tra va chạm diễn ra từ trên mặt khối gạch xuống.</summary>
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

        /// <summary>Trigger chết người là bắt buộc; nếu thiếu bẫy không thể hoạt động đúng thiết kế.</summary>
        private void ValidateDeathTrigger()
        {
            if (deathTrigger != null)
            {
                return;
            }

            Debug.LogError("[DeathPitTrap] deathTrigger chưa gán. Bẫy sẽ không giết được player!", this);
            enabled = false;
        }

        /// <summary>Cảnh báo nếu thiếu collider sàn.</summary>
        private void ValidateSolidCollider()
        {
            if (solidCollider == null)
            {
                Debug.LogWarning("[DeathPitTrap] solidCollider chưa gán.", this);
            }
        }

        /// <summary>Cảnh báo nếu thiếu sprite renderer hiển thị.</summary>
        private void ValidateBlockRenderer()
        {
            if (blockRenderer == null)
            {
                Debug.LogWarning("[DeathPitTrap] blockRenderer chưa gán.", this);
            }
        }

        /// <summary>Thời gian rung nứt phải hợp lệ (0.05s .. 1.0s).</summary>
        private void ValidateShakeDuration()
        {
            if (shakeDuration >= 0.05f && shakeDuration <= 1.0f)
            {
                return;
            }

            Debug.LogWarning("[DeathPitTrap] shakeDuration không hợp lệ.", this);
            shakeDuration = 0.2f;
        }
    }
}