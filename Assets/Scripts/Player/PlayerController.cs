// Trần Văn Tuyên & Nhóm 505
// Bộ điều khiển nhân vật PlayerController chuẩn Unity 6 (6000.x)
// Tuân thủ 100% tài liệu Docs/SCHEMA_CHI_TIET.md Mục 3.2 và Docs/Phan_cong.md.
// Hỗ trợ cả New Input System (UnityEngine.InputSystem) và phím tắt thử nghiệm.
using System;
using System.Collections;
using UnityEngine;
using OneHitDown.Core;

namespace OneHitDown.Player
{
    /// <summary>
    /// Enum phân biệt Player 1 và Player 2 (chung bàn phím).
    /// </summary>
    public enum PlayerType { Player1, Player2 }

    /// <summary>
    /// Enum 4 bộ skin nhân vật (ánh xạ vào mảng skinControllers).
    /// </summary>
    public enum CharacterSkin { NinjaFrog = 0, VirtualGuy = 1, PinkMan = 2, MaskDude = 3 }

    /// <summary>
    /// Điều khiển vật lý nhân vật: gia tốc đà chạy, nhảy đơn, nhảy đôi,
    /// trượt băng, ngoại lực quạt gió, đệm lò xo và mũi tên gia tốc.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        private static readonly WaitForSeconds DevRespawnWait = new(1.2f);

        // Hash tham số Animator (Zero-GC: không dùng chuỗi trong Update)
        private static readonly int AnimSpeed = Animator.StringToHash("speed");
        private static readonly int AnimIsGrounded = Animator.StringToHash("isGrounded");
        private static readonly int AnimVerticalVelocity = Animator.StringToHash("verticalVelocity");
        private static readonly int AnimDoubleJump = Animator.StringToHash("DoubleJump");

        [Header("Cấu hình Player & Skin")]
        [SerializeField] private PlayerType playerType = PlayerType.Player1; // Phân biệt input Player1 (A/D/Space) vs Player2 (Arrows)
        [SerializeField] private RuntimeAnimatorController[] skinControllers; // Mảng 4 Animator Controllers cho 4 Skins

        [Header("Tốc độ & Gia tốc (Schema §3.2)")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float groundAcceleration = 65f;
        [SerializeField] private float groundDeceleration = 80f;
        [SerializeField] private float airAcceleration = 45f;
        [SerializeField] private float iceFriction = 4f;

        [Header("Lực Nhảy (Schema §3.2)")]
        [SerializeField] private float jumpForce = 15.5f;
        [SerializeField] private float doubleJumpForce = 13.5f;
        [SerializeField] private float defaultGravityScale = 3.5f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Kiểm tra mặt đất")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.55f, 0.12f);
        [SerializeField] private LayerMask groundLayer;

        [Header("Trạng thái nội bộ")]
        [SerializeField] private bool isDead = false;
        [SerializeField] private bool isGrounded = true;
        [SerializeField] private bool isOnIce = false;
        [SerializeField] private bool canDoubleJump = true;

        [Header("Tham chiếu Component")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D col;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator anim; // Animator điều khiển Idle (thở) / Run / Jump / Fall / DoubleJump

        // Biến vận hành thời gian thực (Zero-GC)
        private float horizontalInput;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private Vector2 platformVelocity;
        private Vector2 externalVelocity;
        private float appliedExternalX; // Ngoại lực ngang đã cộng vào rb ở bước vật lý trước
        private Vector2 spawnPoint;
        private Coroutine respawnRoutine;
        private PlayerVFX playerVFX; // Cache trong Awake để phát VFX chết / hồi sinh

        #region Public Properties & Events

        public PlayerType Type => playerType;
        public bool IsDead => isDead;
        public bool IsGrounded => isGrounded;
        public bool IsOnIce => isOnIce;
        public bool CanDoubleJump => canDoubleJump;
        public float HorizontalInput => horizontalInput;
        public Rigidbody2D Rigidbody => rb != null ? rb : (rb = GetComponent<Rigidbody2D>());

        public event Action<PlayerController> OnPlayerDied;
        public event Action OnSingleJumped;
        public event Action OnDoubleJumped;
        public event Action OnLanded;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }

            if (col == null)
            {
                col = GetComponent<Collider2D>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (anim == null)
            {
                anim = GetComponent<Animator>();
            }

            if (rb != null)
            {
                rb.gravityScale = defaultGravityScale;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.freezeRotation = true;
            }

            // Mặc định layer Ground nếu chưa gán trong Inspector (Layer 8)
            if (groundLayer.value == 0)
            {
                int groundLayerIndex = LayerMask.NameToLayer("Ground");
                if (groundLayerIndex >= 0)
                {
                    groundLayer = 1 << groundLayerIndex;
                }
            }

            // Tạo vật liệu ma sát 0 chống dính tường
            if (col != null && col.sharedMaterial == null)
            {
                PhysicsMaterial2D slipperyMat = new PhysicsMaterial2D("PlayerZeroFriction")
                {
                    friction = 0f,
                    bounciness = 0f
                };
                col.sharedMaterial = slipperyMat;
            }

            playerVFX = GetComponent<PlayerVFX>();
            spawnPoint = transform.position;
        }

        /// <summary>
        /// Đổi skin nhân vật bằng cách gán Animator Controller tương ứng.
        /// Gọi từ GameManager sau khi đọc lựa chọn skin từ GameSession.
        /// </summary>
        public void ApplySkin(CharacterSkin skin)
        {
            if (anim == null || skinControllers == null || skinControllers.Length == 0)
            {
                return;
            }

            int skinIndex = (int)skin;
            if (skinIndex >= 0 && skinIndex < skinControllers.Length && skinControllers[skinIndex] != null)
            {
                anim.runtimeAnimatorController = skinControllers[skinIndex];
                // Rebind Animator để khởi tạo lại state machine với controller mới
                if (anim.isActiveAndEnabled)
                {
                    anim.Rebind();
                    anim.Update(0f);
                }
            }
        }

        private void Update()
        {
            if (isDead)
            {
                CheckManualRespawnInput();
                return;
            }

            ReadInput();

            // Đếm lùi bộ đếm Coyote time & Jump buffer
            if (coyoteCounter > 0f)
            {
                coyoteCounter -= Time.deltaTime;
            }

            if (jumpBufferCounter > 0f)
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            HandleJump();
            UpdateVisuals();
            CheckManualRespawnInput();
        }

        private void FixedUpdate()
        {
            if (isDead || rb == null)
            {
                return;
            }

            CheckGround();

            // Tính toán gia tốc phù hợp ngữ cảnh (Băng / Không trung / Mặt đất)
            float accel;
            if (isOnIce)
            {
                // Trên băng: nhả phím hoặc bẻ lái ngược chiều đang trượt -> ma sát thấp (iceFriction, trượt dài);
                // bấm phím cùng chiều (hoặc đang đứng yên) -> tăng tốc bình thường để không bị ì.
                float vxRel = rb.linearVelocity.x - platformVelocity.x;
                bool hasInput = Mathf.Abs(horizontalInput) > 0.01f;
                bool reversing = hasInput && Mathf.Abs(vxRel) > 0.5f && Mathf.Sign(vxRel) != Mathf.Sign(horizontalInput);
                accel = hasInput && !reversing ? groundAcceleration : iceFriction;
            }
            else if (!isGrounded)
            {
                accel = airAcceleration;
            }
            else
            {
                accel = Mathf.Abs(horizontalInput) > 0.01f ? groundAcceleration : groundDeceleration;
            }

            // Vận tốc ngang mục tiêu
            float targetVx = horizontalInput * moveSpeed;
            // Trừ phần ngoại lực ngang đã cộng ở bước trước để nó chỉ là độ lệch tạm thời,
            // không bị "nướng" vào vận tốc nền rồi cộng dồn lại mỗi bước (lỗi quạt ngang đẩy văng nhân vật).
            float currentVx = rb.linearVelocity.x - platformVelocity.x - appliedExternalX;
            float newVx = Mathf.MoveTowards(currentVx, targetVx, accel * Time.fixedDeltaTime) + platformVelocity.x;

            // Trên không và đang ép vào tường theo hướng di chuyển: kẹp lại để không dính tường
            if (!isGrounded && IsTouchingWall(horizontalInput) && Mathf.Sign(newVx - platformVelocity.x) == Mathf.Sign(horizontalInput))
            {
                newVx = platformVelocity.x;
            }

            // Áp dụng ngoại lực (từ quạt gió ngang hoặc dây kéo)
            newVx += externalVelocity.x;
            appliedExternalX = externalVelocity.x;
            float newVy = rb.linearVelocity.y + externalVelocity.y;

            // Giảm dần ngoại lực về 0 mượt mà theo thời gian (25 unit/giây)
            externalVelocity = Vector2.MoveTowards(externalVelocity, Vector2.zero, 25f * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector2(newVx, newVy);
        }

        #endregion

        #region Input & Movement Handling

        /// <summary>
        /// Đọc phím điều khiển hỗ trợ cả New Input System và Legacy Input.
        /// Player1: A/D di chuyển ngang, Space hoặc W để nhảy.
        /// Player2: Arrow keys di chuyển ngang, UpArrow để nhảy.
        /// </summary>
        private void ReadInput()
        {
            float h = 0f;
            bool jumpPressed = false;

            if (playerType == PlayerType.Player1)
            {
#if ENABLE_INPUT_SYSTEM
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.aKey.isPressed)
                    {
                        h -= 1f;
                    }

                    if (keyboard.dKey.isPressed)
                    {
                        h += 1f;
                    }

                    if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                    {
                        jumpPressed = true;
                    }
                }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
                if (h == 0f)
                {
                    if (Input.GetKey(KeyCode.A))
                    {
                        h -= 1f;
                    }

                    if (Input.GetKey(KeyCode.D))
                    {
                        h += 1f;
                    }
                }

                if (!jumpPressed)
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
                    {
                        jumpPressed = true;
                    }
                }
#endif
            }
            else // Player2
            {
#if ENABLE_INPUT_SYSTEM
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.leftArrowKey.isPressed)
                    {
                        h -= 1f;
                    }

                    if (keyboard.rightArrowKey.isPressed)
                    {
                        h += 1f;
                    }

                    if (keyboard.upArrowKey.wasPressedThisFrame)
                    {
                        jumpPressed = true;
                    }
                }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
                if (h == 0f)
                {
                    if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        h -= 1f;
                    }

                    if (Input.GetKey(KeyCode.RightArrow))
                    {
                        h += 1f;
                    }
                }

                if (!jumpPressed)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        jumpPressed = true;
                    }
                }
#endif
            }

            horizontalInput = h;
            if (jumpPressed)
            {
                jumpBufferCounter = jumpBufferTime;
            }
        }

        /// <summary>
        /// Kiểm tra phím R để chủ động hồi sinh lại vị trí xuất phát (tiện lợi cho thử nghiệm Dev Scene).
        /// </summary>
        private void CheckManualRespawnInput()
        {
            bool rPressed = false;
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.rKey.wasPressedThisFrame)
            {
                rPressed = true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.R))
            {
                rPressed = true;
            }
#endif
            if (rPressed)
            {
                Respawn(spawnPoint);
            }
        }

        /// <summary>
        /// Quét va chạm với sàn đất Layer 8.
        /// </summary>
        private void CheckGround()
        {
            Vector2 checkPos;
            if (groundCheck != null)
            {
                checkPos = groundCheck.position;
            }
            else if (col != null)
            {
                checkPos = new Vector2(col.bounds.center.x, col.bounds.min.y);
            }
            else
            {
                checkPos = (Vector2)transform.position + Vector2.down * 0.5f;
            }

            Collider2D hit = Physics2D.OverlapBox(checkPos, groundCheckSize, 0f, groundLayer);
            bool wasGrounded = isGrounded;

            if (hit != null && rb != null && rb.linearVelocity.y <= 0.2f)
            {
                isGrounded = true;
                coyoteCounter = coyoteTime;
                canDoubleJump = true;

                if (!wasGrounded)
                {
                    OnLanded?.Invoke();
                }
            }
            else
            {
                isGrounded = false;
            }
        }

        /// <summary>
        /// Kiểm tra có tường (Layer Ground) ngay phía trước theo hướng di chuyển hay không.
        /// </summary>
        private bool IsTouchingWall(float direction)
        {
            if (col == null || Mathf.Abs(direction) < 0.01f)
            {
                return false;
            }

            Bounds b = col.bounds;
            Vector2 size = new Vector2(0.05f, b.size.y * 0.8f);
            Vector2 dir = direction > 0f ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.BoxCast(b.center, size, 0f, dir, b.extents.x + 0.05f, groundLayer);
            return hit.collider != null;
        }

        /// <summary>
        /// Xử lý logic Nhảy đơn và Nhảy đôi chuẩn Schema §3.2.
        /// </summary>
        private void HandleJump()
        {
            if (jumpBufferCounter <= 0f)
            {
                return;
            }

            // Nhảy Đơn (trong thời gian ân hạn Coyote Time)
            if (coyoteCounter > 0f)
            {
                ExecuteJump(jumpForce);
                coyoteCounter = 0f;
                jumpBufferCounter = 0f;
                OnSingleJumped?.Invoke();
            }
            // Nhảy Đôi (khi đang trên không và còn lượt)
            else if (canDoubleJump)
            {
                ExecuteJump(doubleJumpForce);
                canDoubleJump = false;
                jumpBufferCounter = 0f;
                if (anim != null && anim.runtimeAnimatorController != null)
                {
                    anim.SetTrigger(AnimDoubleJump);
                }

                OnDoubleJumped?.Invoke();
            }
        }

        private void ExecuteJump(float force)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            }
        }

        private void UpdateVisuals()
        {
            if (anim != null && anim.runtimeAnimatorController != null && rb != null)
            {
                Vector2 v = rb.linearVelocity;
                anim.SetFloat(AnimSpeed, Mathf.Abs(v.x - platformVelocity.x));
                anim.SetBool(AnimIsGrounded, isGrounded);
                anim.SetFloat(AnimVerticalVelocity, v.y);
            }

            if (spriteRenderer == null)
            {
                return;
            }

            if (horizontalInput > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
            else if (horizontalInput < -0.01f)
            {
                spriteRenderer.flipX = true;
            }
        }

        #endregion

        #region Public Trap & Interaction API (Schema §3.2)

        /// <summary>
        /// Hồi lượt nhảy đôi và gán vận tốc đẩy (dùng bởi TrampolineTrap và ArrowBooster).
        /// </summary>
        public void GrantDoubleJumpAndBoost(Vector2 boostVelocity)
        {
            canDoubleJump = true;
            coyoteCounter = 0f;
            if (rb != null)
            {
                rb.linearVelocity = boostVelocity;
            }
        }

        /// <summary>
        /// Cộng thêm ngoại lực vào nhân vật (dùng bởi FanTrap thổi ngang và SoulTether).
        /// </summary>
        public void AddExternalVelocity(Vector2 extVel)
        {
            // Quạt gọi hàm này mỗi bước vật lý khi nhân vật còn trong vùng gió: xung lực cùng chiều
            // chỉ giữ mức lớn nhất (không cộng dồn vô hạn), xung lực ngược chiều vẫn triệt tiêu nhau.
            externalVelocity.x = MergeImpulse(externalVelocity.x, extVel.x);
            externalVelocity.y = MergeImpulse(externalVelocity.y, extVel.y);
        }

        private static float MergeImpulse(float current, float incoming)
        {
            if (current * incoming > 0f)
            {
                return Mathf.Abs(incoming) > Mathf.Abs(current) ? incoming : current;
            }

            return current + incoming;
        }

        /// <summary>
        /// Cập nhật cờ trượt băng (dùng bởi IceSurface).
        /// </summary>
        public void SetOnIce(bool onIce)
        {
            isOnIce = onIce;
        }

        /// <summary>
        /// Cập nhật vận tốc bệ di động (dùng bởi MovingPlatform).
        /// </summary>
        public void SetPlatformVelocity(Vector2 platVel)
        {
            platformVelocity = platVel;
        }

        /// <summary>
        /// Kích hoạt chết khi chạm bẫy tử thần hoặc vùng chết.
        /// Guard chống lặp vô hạn. Phát VFX Desappearing và bắn sự kiện OnPlayerDied.
        /// </summary>
        public void Die()
        {
            // Guard chống lặp vô hạn & God Mode (Schema Mục 3.2 dòng 244)
            if (isDead || (GameManager.Instance != null && GameManager.Instance.IsGodMode))
            {
                return;
            }

            isDead = true;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            if (playerVFX != null)
            {
                playerVFX.PlayDeathVFX(transform.position);
            }

            OnPlayerDied?.Invoke(this);

            // Tự động hồi sinh sau 1.2s trong Scene thử nghiệm Dev
            if (gameObject.activeInHierarchy)
            {
                if (respawnRoutine != null)
                {
                    StopCoroutine(respawnRoutine);
                }

                respawnRoutine = StartCoroutine(AutoRespawnDevRoutine());
            }
        }

        /// <summary>
        /// Khôi phục trạng thái nhân vật về điểm xuất phát.
        /// </summary>
        public void Respawn(Vector2 spawnPosition)
        {
            isDead = false;
            isOnIce = false;
            canDoubleJump = true;
            platformVelocity = Vector2.zero;
            externalVelocity = Vector2.zero;
            appliedExternalX = 0f;
            transform.position = spawnPosition;

            if (rb != null)
            {
                rb.simulated = true;
                rb.linearVelocity = Vector2.zero;
            }

            if (playerVFX != null)
            {
                playerVFX.PlaySpawnVFX(spawnPosition);
            }

            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
                respawnRoutine = null;
            }

            // Rebind Animator để đảm bảo state machine khởi tạo đúng sau respawn
            if (anim != null && anim.isActiveAndEnabled)
            {
                anim.Rebind();
                anim.Update(0f);
            }
        }

        private IEnumerator AutoRespawnDevRoutine()
        {
            yield return DevRespawnWait;
            Respawn(spawnPoint);
        }

        #endregion

        #region Collision Handling

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Hazard"))
            {
                Die();
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.collider.CompareTag("Hazard"))
            {
                Die();
            }
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector2 checkPos;
            if (groundCheck != null)
            {
                checkPos = groundCheck.position;
            }
            else if (col != null)
            {
                checkPos = new Vector2(col.bounds.center.x, col.bounds.min.y);
            }
            else
            {
                checkPos = (Vector2)transform.position + Vector2.down * 0.5f;
            }

            Gizmos.DrawWireCube(checkPos, groundCheckSize);
        }
#endif
    }
}
