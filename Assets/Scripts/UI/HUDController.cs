// Hoàng Thị Giang Uyên (2351060497) — Module 5: HUD trong màn chơi
// Docs/SCHEMA_CHI_TIET.md §3 Module 5.5 — Docs/Phan_cong.md Mục III (Người 5).
//
// Bố cục: góc trái icon Cổ Vật + bộ đếm [x/3] (font Assets/Art/08_Font/VT323/VT323-Regular.ttf);
// góc phải LEVEL 01..05, TIME, [1P]/[2P], DEATHS, nút Restart.png và pause.png.
//
// Zero-GC (CLAUDE.md Mục 5): chuỗi chỉ dựng lại khi giá trị số thực sự đổi.
using OneHitDown.Core;
using UnityEngine;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    /// <summary>
    /// HUD trong màn chơi: bộ đếm Cổ Vật <c>[x/3]</c>, <c>TIME</c>, <c>LEVEL 0x</c>,
    /// chế độ <c>[1P]/[2P]</c>, <c>DEATHS</c> và 2 nút Restart / Pause.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        #region Hằng số & Field SerializeField

        private const string ClickSfx = "click_002";

        [Header("Góc trái — Cổ Vật")]
        [SerializeField] private Image relicIcon;          // Icon Cổ Vật riêng của từng màn
        [SerializeField] private Text relicCounterText;    // Bộ đếm dạng [x/3]

        [Header("Góc phải — Thông tin màn chơi")]
        [SerializeField] private Text levelText;           // LEVEL 01..05
        [SerializeField] private Text timeText;            // TIME mm:ss
        [SerializeField] private Text modeText;            // [1P] hoặc [2P]
        [SerializeField] private Text deathsText;          // DEATHS xx

        [Header("Nút điều khiển")]
        [SerializeField] private Button restartButton;     // Restart.png (21×22)
        [SerializeField] private Button pauseButton;       // pause.png
        [SerializeField] private PauseMenu pauseMenu;      // Bảng tạm dừng do nút pause mở

        #endregion

        #region Trạng thái runtime

        // Giá trị lần trước — chỉ dựng lại chuỗi khi số liệu thực sự đổi.
        private int lastRelic = -1, lastRequired = -1, lastSecond = -1;
        private int lastDeath = -1, lastLevel = -1, lastMode = -1;

        private GameManager gameManager;

        #endregion

        #region Vòng đời MonoBehaviour

        private void Awake()
        {
            // Ẩn icon nếu màn chưa gán sprite Cổ Vật.
            if (relicIcon != null)
            {
                relicIcon.enabled = relicIcon.sprite != null;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }
        }

        private void Start()
        {
            // GameManager có DefaultExecutionOrder(-100) nên Instance đã sẵn sàng ở Start.
            gameManager = GameManager.Instance;
            RefreshHud();
        }

        private void Update()
        {
            RefreshHud();
        }

        #endregion

        #region Làm mới HUD — [x/3], TIME, DEATHS, LEVEL, [1P]/[2P]

        /// <summary>
        /// Cập nhật toàn bộ HUD theo số liệu hiện tại của <see cref="GameManager"/>.
        /// GameManager cũng gọi hàm này sau khi reset màn hoặc nhặt Cổ Vật.
        /// </summary>
        public void RefreshHud()
        {
            if (gameManager == null && (gameManager = GameManager.Instance) == null)
            {
                return;
            }

            int collected = gameManager.CollectedRelicsCount;
            int required = gameManager.TotalRelicsRequired;
            int deaths = gameManager.DeathCount;
            int elapsed = Mathf.FloorToInt(gameManager.ElapsedTime);
            int levelIndex = gameManager.LevelIndex;
            int mode = GameSession.IsTwoPlayerMode ? 1 : 0;

            // 1. Bộ đếm Cổ Vật [x/3].
            if (collected != lastRelic || required != lastRequired)
            {
                lastRelic = collected;
                lastRequired = required;
                ApplyText(relicCounterText, "[" + collected + "/" + required + "]");
            }

            // 2. Thời gian — chỉ dựng lại chuỗi khi sang giây mới.
            if (elapsed != lastSecond)
            {
                lastSecond = elapsed;
                ApplyText(timeText, "TIME " + (elapsed / 60).ToString("00") + ":" + (elapsed % 60).ToString("00"));
            }

            // 3. Số lần chết.
            if (deaths != lastDeath)
            {
                lastDeath = deaths;
                ApplyText(deathsText, "DEATHS " + deaths.ToString("00"));
            }

            // 4. Tên màn LEVEL 01..05.
            if (levelIndex != lastLevel)
            {
                lastLevel = levelIndex;
                ApplyText(levelText, "LEVEL " + levelIndex.ToString("00"));
            }

            // 5. Chế độ chơi [1P] / [2P].
            if (mode != lastMode)
            {
                lastMode = mode;
                ApplyText(modeText, mode == 1 ? "[2P]" : "[1P]");
            }
        }

        /// <summary>Gán chữ cho một Text nếu tham chiếu còn hợp lệ và nội dung thực sự khác.</summary>
        private static void ApplyText(Text target, string value)
        {
            if (target != null && target.text != value)
            {
                target.text = value;
            }
        }

        #endregion

        #region Nút Restart.png / pause.png

        /// <summary>Nút Restart.png: khởi động lại màn chơi về Bệ Xuất Phát.</summary>
        private void OnRestartClicked()
        {
            PlayClick();

            if (gameManager != null)
            {
                gameManager.ResetLevel();
            }
        }

        /// <summary>Nút pause.png: bật/tắt bảng tạm dừng.</summary>
        private void OnPauseClicked()
        {
            PlayClick();

            if (pauseMenu != null)
            {
                pauseMenu.TogglePause();
            }
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
