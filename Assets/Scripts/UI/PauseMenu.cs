// Hoàng Thị Giang Uyên (2351060497) — Module 5: Bảng tạm dừng trong màn chơi
// Docs/SCHEMA_CHI_TIET.md §3 Module 5.5 — Docs/Phan_cong.md Mục III (Người 5).
//
// Bảng gồm 3 nút: Resume (tiếp tục), Restart (chơi lại màn), Main Menu (về Menu chính).
// Khi tạm dừng đặt Time.timeScale = 0f và ghi nhớ nấc tốc độ đang dùng (phím T có thể
// đang ở 0.5x / 1.5x) để khôi phục đúng khi tiếp tục.
using OneHitDown.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    /// <summary>
    /// Bảng tạm dừng của màn chơi: bật/tắt bằng phím ESC/P (qua <see cref="GameManager"/>)
    /// hoặc nút <c>pause.png</c> trên HUD. Có nút Tiếp tục / Chơi lại / Về Menu chính.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        #region Hằng số & Field SerializeField

        private const string MainMenuSceneName = "MainMenu";
        private const string ClickSfx = "click_002";

        [Header("Bảng tạm dừng")]
        [SerializeField] private GameObject pausePanel;    // Root panel chứa toàn bộ nội dung bảng

        [Header("Nút điều khiển")]
        [SerializeField] private Button resumeButton;      // Tiếp tục chơi
        [SerializeField] private Button restartButton;     // Chơi lại màn (Restart.png)
        [SerializeField] private Button mainMenuButton;    // Về Menu chính (Settings.png)

        #endregion

        #region Trạng thái runtime

        private bool isPaused;
        private float timeScaleBeforePause = 1f;

        /// <summary>Đang tạm dừng hay không (GameManager đọc để chặn phím T).</summary>
        public bool IsPaused => isPaused;

        #endregion

        #region Vòng đời: Awake / OnDestroy

        private void Awake()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnDestroy()
        {
            // Rời Scene khi đang tạm dừng: trả tốc độ chuẩn cho Scene kế tiếp.
            if (isPaused)
            {
                Time.timeScale = 1f;
            }
        }

        #endregion

        #region Bật / tắt tạm dừng — Time.timeScale

        /// <summary>Bật/tắt bảng tạm dừng (phím ESC/P và nút pause.png đều gọi hàm này).</summary>
        public void TogglePause()
        {
            SetPaused(!isPaused);
        }

        /// <summary>Đặt trạng thái tạm dừng tường minh.</summary>
        public void SetPaused(bool paused)
        {
            if (isPaused == paused)
            {
                return;
            }

            isPaused = paused;

            if (isPaused)
            {
                timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = timeScaleBeforePause;
            }

            if (pausePanel != null)
            {
                pausePanel.SetActive(isPaused);
            }

            PlayClick();
        }

        #endregion

        #region Nút Tiếp tục / Chơi lại / Về Menu chính

        private void OnResumeClicked()
        {
            SetPaused(false);
        }

        /// <summary>Đóng bảng, trả tốc độ chuẩn rồi reset màn về Bệ Xuất Phát.</summary>
        private void OnRestartClicked()
        {
            CloseAndResume();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetLevel();
            }
        }

        /// <summary>Đóng bảng, trả tốc độ chuẩn rồi tải Scene MainMenu.</summary>
        private void OnMainMenuClicked()
        {
            CloseAndResume();

            if (Application.CanStreamedLevelBeLoaded(MainMenuSceneName))
            {
                SceneManager.LoadScene(MainMenuSceneName);
                return;
            }

            Debug.LogWarning($"[PauseMenu] Scene '{MainMenuSceneName}' chưa có trong Build Settings — bỏ qua.", this);
        }

        /// <summary>Đóng bảng, trả tốc độ chuẩn và phát tiếng bấm nút (dùng chung cho 2 nút rời màn).</summary>
        private void CloseAndResume()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            PlayClick();
        }

        /// <summary>Phát âm phản hồi khi bấm nút (click_002.ogg).</summary>
        private static void PlayClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(ClickSfx);
            }
        }

        #endregion
    }
}
