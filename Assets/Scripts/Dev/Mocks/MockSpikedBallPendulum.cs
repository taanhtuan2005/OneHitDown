// ====================================================================================================
// BẢN MOCK TẠM THỜI CHO SCENE LEVEL_03 — DO TRẦN VĂN TUYÊN (NGƯỜI 3) TẠO ĐỂ TEST MÀN 3.
// CẢNH BÁO CHO TEAM: ĐÂY LÀ MOCK ĐỂ TEST GAMEPLAY, HOÀN TOÀN KHÔNG THAY THẾ SCRIPT CHÍNH THỨC!
// NGƯỜI 2 (TRẦN ĐỨC TRUNG) VẪN BẮT BUỘC PHẢI TỰ VIẾT 'Assets/Scripts/Traps/SpikedBallPendulum.cs'
// VÀ ĐÓNG GÓI 'Assets/Prefabs/Traps/SpikedBall.prefab' THEO ĐÚNG SCHEMA §3 MODULE 2.6!
// ====================================================================================================

using OneHitDown.Core;
using UnityEngine;

namespace OneHitDown.Dev.Mocks
{
    /// <summary>
    /// Mock Cầu Gai Con Lắc Xích Sắt: đu đưa qua lại quanh trục treo theo hàm Sin.
    /// </summary>
    public class MockSpikedBallPendulum : MonoBehaviour, IResettable
    {
        [Header("Thông số Con Lắc (Schema §3.2 Module 2.6)")]
        [SerializeField] private float swingAngle = 60f;     // Góc lệch cực đại (độ)
        [SerializeField] private float swingSpeed = 2.0f;     // Tốc độ đu đưa (rad/s)
        [SerializeField] private float armLength = 5.0f;      // Chiều dài tay xích (unit)
        [SerializeField] private Transform ballTransform;     // Transform của quả cầu gai

        private float timer;

        private void Update()
        {
            timer += Time.deltaTime;
            float currentAngle = Mathf.Sin(timer * swingSpeed) * swingAngle;
            transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
        }

        public void ResetState()
        {
            timer = 0f;
            transform.localRotation = Quaternion.identity;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ballTransform != null ? ballTransform.position : transform.position, 0.8f);
        }
#endif
    }
}
