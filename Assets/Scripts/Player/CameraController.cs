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

        #endregion

        #region Vòng đời: Awake

        private void Awake()
        {
            Camera cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = targetOrthoSize;
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

            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        #endregion
    }
}
