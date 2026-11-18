// ====================================================================================================
// BẢN MOCK TẠM THỜI CHO SCENE LEVEL_03 — DO TRẦN VĂN TUYÊN (NGƯỜI 3) TẠO ĐỂ TEST MÀN 3.
// CẢNH BÁO CHO TEAM: ĐÂY LÀ MOCK ĐỂ TEST GAMEPLAY, HOÀN TOÀN KHÔNG THAY THẾ SCRIPT CHÍNH THỨC!
// NGƯỜI 2 (TRẦN ĐỨC TRUNG) VẪN BẮT BUỘC PHẢI TỰ VIẾT 'Assets/Scripts/Traps/FireTrap.cs'
// VÀ ĐÓNG GÓI 'Assets/Prefabs/Traps/FireTrap.prefab' THEO ĐÚNG SCHEMA §3 MODULE 2.3!
// ====================================================================================================

using System.Collections;
using OneHitDown.Core;
using UnityEngine;

namespace OneHitDown.Dev.Mocks
{
    /// <summary>
    /// Mock Vạc Lửa chu kỳ: 2s tắt (Off) -> 0.35s cảnh báo (Hit) -> 1.0s bùng lửa (On, kích hoạt Hazard).
    /// </summary>
    public class MockFireTrap : MonoBehaviour, IResettable
    {
        [Header("Chu kỳ Lửa Màn 3 (Schema §3.2 Module 2.3)")]
        [SerializeField] private float offTime = 2.0f;
        [SerializeField] private float warningTime = 0.35f;
        [SerializeField] private float activeTime = 1.0f;

        [Header("Tham chiếu Component")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject fireHitbox;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Sprite warningSprite;
        [SerializeField] private Sprite[] activeFrames;

        private Coroutine cycleRoutine;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (fireHitbox != null) fireHitbox.SetActive(false);
        }

        private void OnEnable()
        {
            StartCycle();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            cycleRoutine = null;
        }

        public void StartCycle()
        {
            if (cycleRoutine != null) StopCoroutine(cycleRoutine);
            cycleRoutine = StartCoroutine(FireCycleRoutine());
        }

        public void ResetState()
        {
            StartCycle();
        }

        private IEnumerator FireCycleRoutine()
        {
            while (true)
            {
                // Pha 1: TẮT (2.0s)
                if (spriteRenderer != null && offSprite != null) spriteRenderer.sprite = offSprite;
                if (fireHitbox != null) fireHitbox.SetActive(false);
                yield return new WaitForSeconds(offTime);

                // Pha 2: CẢNH BÁO TIA LỬA NHỎ (0.35s)
                if (spriteRenderer != null && warningSprite != null) spriteRenderer.sprite = warningSprite;
                yield return new WaitForSeconds(warningTime);

                // Pha 3: BÙNG LỬA SÁT THƯƠNG (1.0s)
                if (fireHitbox != null) fireHitbox.SetActive(true);
                float timer = 0f;
                int frameIdx = 0;
                while (timer < activeTime)
                {
                    if (activeFrames != null && activeFrames.Length > 0 && spriteRenderer != null)
                    {
                        spriteRenderer.sprite = activeFrames[frameIdx % activeFrames.Length];
                        frameIdx++;
                    }
                    yield return new WaitForSeconds(0.08f);
                    timer += 0.08f;
                }
            }
        }
    }
}
