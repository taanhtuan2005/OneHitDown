// Hoàng Thị Giang Uyên (2351060497) — Module 5: Dây Xích Linh Hồn Co-op 2P
// Docs/SCHEMA_CHI_TIET.md §3 Module 5.3 — Docs/Phan_cong.md Mục III (Người 5).
//
// DistanceJoint2D (maxDistanceOnly) giữ 2 nhân vật trong 4f = 64px; dây đổi 3 màu theo độ
// căng; bật tia lửa spark_01.png + âm cảnh báo khi căng cực đại; truyền lực kéo đàn hồi để
// hỗ trợ 2 kỹ thuật Co-op: Mỏ Neo Sàn (Ground Anchor) và Đu Dây Con Lắc (Aerial Swing).
using OneHitDown.Core;
using UnityEngine;

namespace OneHitDown.Player
{
    /// <summary>
    /// Dây xích vật lý nối Player1 và Player2: giới hạn 4f (64px), đổi màu
    /// Cyan → Orange → Red theo độ căng và truyền lực kéo đàn hồi giữa hai người.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class SoulTether : MonoBehaviour, IResettable
    {
        #region Hằng số & Field SerializeField

        private const string TensionWarningSfx = "zapThreeToneUp";

        [Header("Giới hạn & Màu sắc")]
        [SerializeField] private float maxDistance = 4f;                          // 4 tiles = 64 pixels
        [SerializeField] private Color safeColor = Color.cyan;                    // < 3.0f (< 48px)
        [SerializeField] private Color tensionColor = new Color(1f, 0.6f, 0.1f);  // 3.0–3.75f (48–60px)
        [SerializeField] private Color criticalColor = Color.red;                 // >= 3.75f (60–64px)

        [Header("Tham chiếu")]
        [SerializeField] private ParticleSystem tensionSparkVfx;  // Tia lửa spark_01.png ở giữa dây
        [SerializeField] private LineRenderer lineRenderer;       // Dây xích nối 2 đầu

        [Header("Lực kéo đàn hồi")]
        [SerializeField] private float pullStiffness = 12f;       // Độ cứng lò xo kéo 2 người lại gần
        [SerializeField] private float anchorResistance = 0.25f;  // Hệ số giảm lực cho người bám đất
        [SerializeField] private float warningInterval = 0.5f;    // Giãn cách âm cảnh báo (giây)

        #endregion

        #region Trạng thái runtime

        private PlayerController player1, player2;
        private Rigidbody2D body1, body2;
        private DistanceJoint2D tetherJoint;
        private float warningCooldown;

        /// <summary>Khoảng cách hiện tại giữa 2 người chơi (đơn vị world).</summary>
        public float CurrentDistance { get; private set; }

        #endregion

        #region Vòng đời: Awake

        private void Awake()
        {
            if (lineRenderer == null) { lineRenderer = GetComponent<LineRenderer>(); }

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true;
            }

            ApplyColor(safeColor);
            StopSpark();
        }

        #endregion

        #region Gắn DistanceJoint2D — SetupPhysicsJoint

        /// <summary>Gắn DistanceJoint2D (maxDistanceOnly, distance = maxDistance) giữa 2 người chơi.</summary>
        public void SetupPhysicsJoint(PlayerController p1, PlayerController p2)
        {
            player1 = p1;
            player2 = p2;

            if (p1 == null || p2 == null || p1.Rigidbody == null || p2.Rigidbody == null)
            {
                Debug.LogError("[SoulTether] Thiếu người chơi hoặc Rigidbody2D — không gắn được dây xích.", this);
                return;
            }

            // Cache Rigidbody2D một lần để Update/FixedUpdate không truy vấn lại (Zero-GC).
            body1 = p1.Rigidbody;
            body2 = p2.Rigidbody;

            // Joint phải nằm trên GameObject có Rigidbody2D nên gắn vào Player2, nối tới Player1.
            if (!p2.TryGetComponent(out tetherJoint)) { tetherJoint = p2.gameObject.AddComponent<DistanceJoint2D>(); }

            tetherJoint.connectedBody = body1;
            tetherJoint.autoConfigureDistance = false;
            tetherJoint.maxDistanceOnly = true;
            tetherJoint.distance = maxDistance;
            tetherJoint.enabled = true;

            UpdateVisuals();
        }

        #endregion

        #region Truyền lực kéo — Mỏ Neo Sàn & Đu Dây (FixedUpdate)

        /// <summary>
        /// Khi dây sắp chạm giới hạn (>= 95% maxDistance), truyền lực kéo đàn hồi qua
        /// <c>AddExternalVelocity</c> để hỗ trợ Mỏ Neo Sàn và Đu Dây Con Lắc.
        /// </summary>
        private void FixedUpdate()
        {
            if (body1 == null || body2 == null) { return; }

            Vector2 delta = body1.position - body2.position;
            float distance = delta.magnitude;

            if (distance < maxDistance * 0.95f || distance <= 0.0001f) { return; }

            // Độ vượt ngưỡng: 0 tại 95% chiều dài, tăng dần tới 1 tại 100%.
            float overStretch = Mathf.InverseLerp(maxDistance * 0.95f, maxDistance, distance);
            float pull = pullStiffness * overStretch * Time.fixedDeltaTime;
            Vector2 direction = delta / distance;

            // Mỏ Neo Sàn: người đang bám đất ghì lại nên chỉ nhận phần nhỏ lực.
            player1.AddExternalVelocity(-direction * (pull * (player1.IsGrounded ? anchorResistance : 1f)));
            player2.AddExternalVelocity(direction * (pull * (player2.IsGrounded ? anchorResistance : 1f)));
        }

        #endregion

        #region Vẽ dây, đổi 3 màu & cảnh báo (Update)

        /// <summary>Cập nhật 2 đầu dây, đổi màu 3 cấp, bật tia lửa và phát cảnh báo khi căng cực đại.</summary>
        private void Update()
        {
            if (warningCooldown > 0f) { warningCooldown -= Time.deltaTime; }
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (body1 == null || body2 == null) { return; }

            Vector2 position1 = body1.position;
            Vector2 position2 = body2.position;
            CurrentDistance = Vector2.Distance(position1, position2);

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, position1);
                lineRenderer.SetPosition(1, position2);
            }

            // 3 cấp độ căng: < 3.0f (48px) an toàn, 3.0–3.75f (48–60px) căng, >= 3.75f (60px) cực đại.
            bool isCritical = CurrentDistance >= maxDistance * 0.9375f;
            ApplyColor(CurrentDistance >= maxDistance * 0.75f
                ? (isCritical ? criticalColor : tensionColor)
                : safeColor);

            if (tensionSparkVfx == null) { return; }

            tensionSparkVfx.transform.position = (position1 + position2) * 0.5f;

            if (!isCritical)
            {
                StopSpark();
                return;
            }

            if (!tensionSparkVfx.isPlaying) { tensionSparkVfx.Play(); }

            // Âm cảnh báo lặp mỗi warningInterval giây chừng nào dây còn căng cực đại.
            if (warningCooldown > 0f) { return; }

            warningCooldown = warningInterval;

            if (AudioManager.Instance != null) { AudioManager.Instance.PlaySFX(TensionWarningSfx); }
        }

        #endregion

        #region IResettable — ResetState

        /// <summary>Khôi phục dây về trạng thái gốc khi cả đội chết và hồi sinh đầu màn.</summary>
        public void ResetState()
        {
            if (tetherJoint != null && body1 != null)
            {
                tetherJoint.connectedBody = body1;
                tetherJoint.distance = maxDistance;
                tetherJoint.enabled = true;
            }

            warningCooldown = 0f;
            StopSpark();
            ApplyColor(safeColor);
            UpdateVisuals();
        }

        #endregion

        #region Hàm phụ trợ hiển thị

        private void ApplyColor(Color color)
        {
            if (lineRenderer != null)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }

        private void StopSpark()
        {
            if (tensionSparkVfx != null)
            {
                tensionSparkVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        #endregion
    }
}
