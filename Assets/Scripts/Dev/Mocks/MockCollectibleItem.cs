// ====================================================================================================
// BẢN MOCK TẠM THỜI CHO SCENE LEVEL_03 — DO TRẦN VĂN TUYÊN (NGƯỜI 3) TẠO ĐỂ TEST MÀN 3.
// CẢNH BÁO CHO TEAM: ĐÂY LÀ MOCK ĐỂ TEST GAMEPLAY, HOÀN TOÀN KHÔNG THAY THẾ SCRIPT CHÍNH THỨC!
// NGƯỜI 4 (PHAN VĂN TÚ) VẪN BẮT BUỘC PHẢI TỰ VIẾT 'Assets/Scripts/Items/CollectibleItem.cs'
// THEO ĐÚNG SCHEMA §3 MODULE 4.1!
// ====================================================================================================

using OneHitDown.Core;
using UnityEngine;

namespace OneHitDown.Dev.Mocks
{
    /// <summary>
    /// Mock Vật phẩm Thu thập (Kiwi Cổ Vật & Dâu Tây Thưởng): nhấp nhô lơ lửng, nhặt được khi chạm.
    /// </summary>
    public class MockCollectibleItem : MonoBehaviour, IResettable
    {
        [SerializeField] private string itemName = "Kiwi";
        [SerializeField] private bool isRelic = true; // true: Cổ vật Kiwi bắt buộc mở cổng; false: Dâu tây thưởng phụ
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobSpeed = 3.0f;

        private Vector3 startPos;
        private SpriteRenderer sr;
        private Collider2D col;
        private bool isCollected;

        public bool IsCollected => isCollected;
        public bool IsRelic => isRelic;

        private void Awake()
        {
            startPos = transform.position;
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (isCollected) return;
            float offsetY = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = startPos + new Vector3(0f, offsetY, 0f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected || !other.CompareTag("Player")) return;

            isCollected = true;
            if (sr != null) sr.enabled = false;
            if (col != null) col.enabled = false;

            Debug.Log($"<b>[Level 03]</b> Thu thập thành công: <color={(isRelic ? "green" : "yellow")}>{itemName}</color>!");
        }

        public void ResetState()
        {
            isCollected = false;
            transform.position = startPos;
            if (sr != null) sr.enabled = true;
            if (col != null) col.enabled = true;
        }
    }
}
