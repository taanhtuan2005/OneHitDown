// Hoàng Thị Giang Uyên (2351060497) — Module 5: Camera bám mượt 1P / Trung điểm 2P
// Tuân thủ Docs/SCHEMA_CHI_TIET.md §3 Module 5.2 và Docs/Phan_cong.md Mục III (Người 5).
using UnityEngine;

namespace OneHitDown.Player
{
    /// <summary>
    /// Camera 2D bám mượt theo Player1 ở chế độ 1P, hoặc bám theo trung điểm của
    /// Player1 và Player2 ở chế độ 2P Co-op. Có giới hạn biên dọc <c>minY</c>/<c>maxY</c>
    /// để camera không trôi ra khỏi khung màn chơi.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        #region Field SerializeField (gán trong Inspector)

        [Header("Mục tiêu theo dõi")]
        [SerializeField] private Transform target1;                              // Player1
        [SerializeField] private Transform target2;                              // Player2 (chỉ dùng ở chế độ 2P)

        [Header("Thông số bám")]
        [SerializeField] private float targetOrthoSize = 5.5f;                   // Kích thước trực quan chuẩn của màn chơi
        [SerializeField] private float smoothSpeed = 6f;                         // Tốc độ nội suy Vector3.Lerp
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);   // Lệch tâm nhìn lên trên

        [Header("Giới hạn biên dọc")]
        [SerializeField] private float minY = -2f;
        [SerializeField] private float maxY = 16f;

        [Header("Tự tìm mục tiêu khi chưa được gán")]
        [Tooltip("Chu kỳ dò lại Player (giây) khi target1 còn null. 0 = tắt tự tìm.")]
        [SerializeField] private float autoFindInterval = 0.25f;

        #endregion

        #region Trạng thái nội bộ

        // Coroutine dò Player khi target1 còn null (chỉ chạy ở trạng thái lỗi cấu hình).
        private Coroutine autoFindRoutine;

        // Đã bám được mục tiêu lần đầu chưa: lần đầu thì đặt thẳng, các lần sau mới nội suy.
        private bool hasSnappedToTarget;

        #endregion

        #region Vòng đời: Awake / Start / OnDisable

        private void Awake()
        {
            Camera cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = targetOrthoSize;
        }

        /// <summary>
        /// Dự phòng khi <c>GameManager</c> không gán được <c>target1</c> (ví dụ mở thẳng Scene
        /// để test, hoặc Inspector thiếu tham chiếu). Không đặt việc dò trong <c>LateUpdate</c>
        /// để giữ Zero GC Alloc theo quy tắc dự án.
        /// </summary>
        private void Start()
        {
            if (target1 == null)
            {
                TryFindTargetOnce();
            }

            if (target1 == null && autoFindInterval > 0f)
            {
                autoFindRoutine = StartCoroutine(AutoFindRoutine());
            }
        }

        private void OnDisable()
        {
            if (autoFindRoutine != null)
            {
                StopCoroutine(autoFindRoutine);
                autoFindRoutine = null;
            }
        }

        #endregion

        #region Gán mục tiêu — SetTargets

        /// <summary>
        /// Gán cặp mục tiêu do <see cref="OneHitDown.Core.GameManager"/> sinh ra.
        /// Truyền <paramref name="p2"/> bằng <c>null</c> để chạy chế độ 1P.
        /// </summary>
        public void SetTargets(Transform p1, Transform p2)
        {
            target1 = p1;
            target2 = p2;

            // Mục tiêu mới (vừa hồi sinh / vừa đổi chế độ) → bám lại tức thì, không trượt từ vị trí cũ.
            hasSnappedToTarget = false;

            // Đã có mục tiêu thật thì dừng dò dự phòng.
            if (target1 != null && autoFindRoutine != null)
            {
                StopCoroutine(autoFindRoutine);
                autoFindRoutine = null;
            }
        }

        #endregion

        #region Tự tìm mục tiêu (fallback)

        /// <summary>
        /// Dò Player đang hoạt động trong Scene. Ưu tiên Player1, nếu không có thì lấy
        /// Player bất kỳ còn sống.
        /// </summary>
        private void TryFindTargetOnce()
        {
            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            if (players == null || players.Length == 0)
            {
                return;
            }

            PlayerController picked = null;
            for (int i = 0; i < players.Length; i++)
            {
                PlayerController candidate = players[i];
                if (candidate == null || !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (candidate.Type == PlayerType.Player1)
                {
                    picked = candidate;
                    break;
                }

                if (picked == null)
                {
                    picked = candidate;
                }
            }

            if (picked != null)
            {
                target1 = picked.transform;
                hasSnappedToTarget = false;
            }
        }

        /// <summary>Dò lại theo chu kỳ <c>autoFindInterval</c> cho tới khi tìm được Player.</summary>
        private System.Collections.IEnumerator AutoFindRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(autoFindInterval);

            while (target1 == null)
            {
                yield return wait;

                if (target1 == null)
                {
                    TryFindTargetOnce();
                }
            }

            autoFindRoutine = null;
        }

        #endregion

        #region Bám mục tiêu (LateUpdate)

        private void LateUpdate()
        {
            if (target1 == null)
            {
                return;
            }

            // 1P bám Player1; 2P bám trung điểm để cả hai luôn trong khung hình.
            Vector3 focusPoint = target2 != null
                ? (target1.position + target2.position) * 0.5f
                : target1.position;

            Vector3 desiredPosition = focusPoint + offset;
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

            // Frame đầu tiên bám mục tiêu: đặt thẳng để camera không trượt từ vị trí khởi tạo.
            if (!hasSnappedToTarget)
            {
                hasSnappedToTarget = true;
                transform.position = desiredPosition;
                return;
            }

            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        #endregion
    }
}
