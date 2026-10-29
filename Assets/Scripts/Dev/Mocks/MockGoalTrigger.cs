// ====================================================================================================
// BẢN MOCK TẠM THỜI CHO SCENE LEVEL_03 — DO TRẦN VĂN TUYÊN (NGƯỜI 3) TẠO ĐỂ TEST MÀN 3.
// CẢNH BÁO CHO TEAM: ĐÂY LÀ MOCK ĐỂ TEST GAMEPLAY, HOÀN TOÀN KHÔNG THAY THẾ SCRIPT CHÍNH THỨC!
// NGƯỜI 4 (PHAN VĂN TÚ) VẪN BẮT BUỘC PHẢI TỰ VIẾT 'Assets/Scripts/Level/ExitGate.cs'
// VÀ 'Assets/Scripts/Level/LevelStartPoint.cs' THEO ĐÚNG SCHEMA §3 MODULE 4.3 & 4.4!
// ====================================================================================================

using UnityEngine;

namespace OneHitDown.Dev.Mocks
{
    /// <summary>
    /// Mock Cúp Chiến Thắng / Cổng Thần Cổ cuối Màn 3.
    /// </summary>
    public class MockGoalTrigger : MonoBehaviour
    {
        [SerializeField] private ParticleSystem confettiVfx;
        private bool isTriggered;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (isTriggered || !col.CompareTag("Player")) return;

            isTriggered = true;
            Debug.Log("<b>[Level 03]</b> <color=cyan>★ VICTORY! BẠN ĐÃ CHINH PHỤC THÀNH CÔNG MÀN 3 (HẦM MỘ BĂNG LÃNH & VẠC LỬA)! ★</color>");

            if (confettiVfx != null && !confettiVfx.isPlaying)
            {
                confettiVfx.Play();
            }
        }
    }
}
