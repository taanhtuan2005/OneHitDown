// Hoàng Thị Giang Uyên (2351060497) — Module 5: Hiệu ứng màn tròn IrisTransition
// Docs/SCHEMA_CHI_TIET.md §3 Module 5.5 — Docs/Phan_cong.md Mục III (Người 5).
//
// Dùng ảnh Assets/Art/07_HieuUng/PA1_Other/Transition.png (44×44) làm mặt nạ tròn:
//   Pha ĐÓNG: phóng to ảnh từ tâm màn hình tới khi phủ kín khung hình.
//   Gọi callback chuyển Scene (GameManager.OnLevelCompleted).
//   Pha MỞ  : thu nhỏ ảnh về 0 để lộ màn chơi mới.
// Toàn bộ dùng unscaled time nên vẫn chạy đúng khi Time.timeScale = 0.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    /// <summary>
    /// Hiệu ứng chuyển màn hình tròn (Iris Transition) từ <c>Transition.png</c> (44×44):
    /// phóng to tâm tròn che kín màn hình, gọi callback chuyển Scene rồi thu nhỏ lại.
    /// </summary>
    public class IrisTransition : MonoBehaviour
    {
        #region Field SerializeField (gán trong Inspector)

        [Header("Ảnh màn tròn")]
        [SerializeField] private Image irisImage;              // Image dùng sprite Transition.png (44×44)

        [Header("Thông số hiệu ứng")]
        [SerializeField] private float closeDuration = 0.35f;  // Thời gian đóng (giây, unscaled)
        [SerializeField] private float openDuration = 0.35f;   // Thời gian mở (giây, unscaled)
        [SerializeField] private float overshoot = 1.6f;       // Hệ số phủ tràn viền khung hình

        #endregion

        #region Trạng thái runtime

        private Coroutine transitionRoutine;

        #endregion

        #region Vòng đời: Awake / OnDisable

        private void Awake()
        {
            HideIris();
        }

        private void OnDisable()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }

            HideIris();
        }

        #endregion

        #region Chạy hiệu ứng — PlayTransition

        /// <summary>
        /// Chạy hiệu ứng: đóng kín màn hình → gọi <paramref name="onComplete"/>
        /// (thường là chuyển Scene) → mở lại màn tròn.
        /// </summary>
        public void PlayTransition(Action onComplete)
        {
            if (irisImage == null)
            {
                Debug.LogWarning("[IrisTransition] Chưa gán irisImage (Transition.png) — bỏ qua hiệu ứng.", this);
                onComplete?.Invoke();
                return;
            }

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(PlayRoutine(onComplete));
        }

        private IEnumerator PlayRoutine(Action onComplete)
        {
            float coveredScale = ResolveCoveredScale();

            // Hiện mặt nạ và chặn click xuyên qua trong suốt hiệu ứng.
            irisImage.gameObject.SetActive(true);
            irisImage.raycastTarget = true;

            yield return AnimateScale(0f, coveredScale, closeDuration);

            // Chuyển Scene khi màn hình đã bị che kín.
            onComplete?.Invoke();

            yield return AnimateScale(coveredScale, 0f, openDuration);

            HideIris();
            transitionRoutine = null;
        }

        #endregion

        #region Nội suy tỉ lệ phóng

        /// <summary>Nội suy tỉ lệ phóng của ảnh màn tròn theo thời gian unscaled.</summary>
        private IEnumerator AnimateScale(float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                // Smoothstep để hiệu ứng không bị giật ở hai đầu.
                float t = Mathf.Clamp01(elapsed / duration);
                SetScale(Mathf.LerpUnclamped(from, to, t * t * (3f - 2f * t)));
                yield return null;
            }

            SetScale(to);
        }

        private void SetScale(float scale)
        {
            Vector3 localScale = irisImage.rectTransform.localScale;
            localScale.x = scale;
            localScale.y = scale;
            irisImage.rectTransform.localScale = localScale;
        }

        #endregion

        #region Ẩn mặt nạ & tính tỉ lệ phủ kín màn hình

        private void HideIris()
        {
            if (irisImage != null)
            {
                irisImage.raycastTarget = false;
                irisImage.gameObject.SetActive(false);
            }
        }

        /// <summary>Tỉ lệ phóng cần thiết để ảnh 44×44 phủ kín khung hình hiện tại.</summary>
        private float ResolveCoveredScale()
        {
            Sprite sprite = irisImage.sprite;

            if (sprite == null || sprite.rect.width <= 0f || sprite.rect.height <= 0f)
            {
                return 1f;
            }

            return Mathf.Max(Screen.width, Screen.height) * overshoot / Mathf.Min(sprite.rect.width, sprite.rect.height);
        }

        #endregion
    }
}
