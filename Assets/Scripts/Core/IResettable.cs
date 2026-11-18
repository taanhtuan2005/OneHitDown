// Trần Văn Tuyên, phiên bản 1.0
// Interface dùng chung cho Instant Reset — Trần Văn Tuyên, Module 3 (Người 3).
namespace OneHitDown.Core
{
    /// <summary>Khôi phục trạng thái đầu lượt chơi, dùng chung với GameManager.</summary>
    public interface IResettable
    {
        void ResetState();
    }
}