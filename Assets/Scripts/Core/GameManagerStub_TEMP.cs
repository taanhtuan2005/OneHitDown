using UnityEngine;

namespace OneHitDown.Core
{
    /// <summary>
    /// [TẠM THỜI - XÓA SAU KHI TEST XONG]
    /// Lớp giả tối thiểu để ExitGate.cs và CollectibleItem.cs biên dịch được.
    /// KHÔNG có logic thật: không đếm Cổ Vật, không hồi sinh, không chuyển màn.
    /// Chỉ ghi log để biết code của Module 4 đã gọi tới đúng chỗ hay chưa.
    ///
    /// Khi Module 5 viết xong GameManager.cs thật, xóa file này ngay,
    /// nếu không sẽ báo lỗi trùng tên class.
    /// </summary>
    public sealed class GameManager
    {
        private static readonly GameManager instance = new GameManager();

        public static GameManager Instance => instance;

        /// <summary>Số Cổ Vật cần thu thập mỗi màn (chuẩn 3/3).</summary>
        public int TotalRelicsRequired { get; set; } = 3;

        /// <summary>
        /// Chế độ bất tử (Schema dòng 486, bật/tắt bằng phím G trong GameManager thật).
        /// Stub chỉ giữ đúng chữ ký để PlayerController.Die() biên dịch được guard God Mode.
        /// </summary>
        public bool IsGodMode { get; set; } = false;

        public void OnRelicCollected(int score, bool countsAsRelic)
        {
            Debug.Log($"[GameManager STUB] OnRelicCollected(score={score}, countsAsRelic={countsAsRelic}) - chưa có logic thật.");
        }

        public void OnLevelCompleted()
        {
            Debug.Log("[GameManager STUB] OnLevelCompleted() - chưa có logic thật.");
        }
    }
}
