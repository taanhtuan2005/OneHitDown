// Hoàng Thị Giang Uyên (2351060497) — Module 5: Bảng tạm dừng trong màn chơi
// Docs/SCHEMA_CHI_TIET.md §3 Module 5.5 — Docs/Phan_cong.md Mục III (Người 5).
//
// Bảng gồm 3 nút: Resume (tiếp tục), Restart (chơi lại màn), Main Menu (lưu tạm dừng & về Menu chính).
// Khi tạm dừng đặt Time.timeScale = 0f và ghi nhớ nấc tốc độ đang dùng (phím T có thể
// đang ở 0.5x / 1.5x) để khôi phục đúng khi tiếp tục.
using OneHitDown.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    /// <summary>
    /// Bảng tạm dừng của màn chơi: bật/tắt bằng phím ESC/P (qua <see cref="GameManager"/>)
    /// hoặc nút Tạm dừng trên HUD. Có nút Tiếp tục / Chơi lại / Về Menu chính (kèm lưu tạm dừng).
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
        [SerializeField] private Button mainMenuButton;    // Về Menu chính (lưu trạng thái tạm dừng)

        #endregion

        #region Trạng thái runtime

        private bool isPaused;
        private float timeScaleBeforePause = 1f;
        private TMP_Text pauseStatsTmp;
        private TMP_FontAsset cachedTmpFont;

        /// <summary>Đang tạm dừng hay không (GameManager đọc để chặn phím T).</summary>
        public bool IsPaused => isPaused;

        #endregion

        #region Vòng đời: Awake / OnDestroy

        private void Awake()
        {
            EnhancePausePanelVisuals();

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

        #region Nâng cấp giao diện bảng Tạm Dừng sắc nét (TextMeshPro đồng bộ Main Menu)

        private void EnhancePausePanelVisuals()
        {
            if (pausePanel == null)
            {
                return;
            }

            cachedTmpFont = RuntimeUiHelper.GetSafeTmpFont();

            Image backdrop = pausePanel.GetComponent<Image>();
            if (backdrop != null)
            {
                backdrop.color = new Color(0.02f, 0.02f, 0.05f, 0.88f);
            }

            // Ẩn tiêu đề VT323 mảnh cũ để thay bằng tiêu đề TextMeshPro sắc nét
            Transform oldTitleTr = pausePanel.transform.Find("PauseTitle");
            if (oldTitleTr != null)
            {
                oldTitleTr.gameObject.SetActive(false);
            }

            if (pausePanel.transform.Find("PauseCard") == null)
            {
                RectTransform cardInner = RuntimeUiHelper.CreateFramedCard(
                    "PauseCard", pausePanel.transform, Vector2.zero, new Vector2(840f, 560f),
                    new Color(1f, 0.85f, 0f, 1f), new Color(0.07f, 0.08f, 0.14f, 0.99f), 6f);
                cardInner.parent.SetAsFirstSibling();

                // Huy hiệu 505 & Tiêu đề lớn rõ nét
                RectTransform logoRt = RuntimeUiHelper.CreateUiRect("Logo505", cardInner, new Vector2(-325f, 206f), new Vector2(68f, 68f));
                Image logoImg = logoRt.gameObject.AddComponent<Image>();
                logoImg.sprite = RuntimeUiHelper.GetTeam505LogoSprite();
                logoImg.preserveAspect = true;
                logoImg.raycastTarget = false;

                TMP_Text titleTmp = CreatePauseTmpText("TmpPauseTitle", cardInner, new Vector2(24f, 206f),
                    new Vector2(620f, 64f), "TẠM DỪNG TRÒ CHƠI", 50f, new Color(1f, 0.87f, 0f, 1f));
                titleTmp.fontStyle = FontStyles.Bold;

                // Khung thống kê màn chơi có nền & viền riêng, tự co chữ vừa khít không bao giờ tràn khung
                RectTransform statsInner = RuntimeUiHelper.CreateFramedCard(
                    "PauseStatsBox", cardInner, new Vector2(0f, 128f), new Vector2(780f, 54f),
                    new Color(0f, 0.78f, 0.90f, 0.85f), new Color(0.10f, 0.13f, 0.22f, 1f), 2.5f);

                pauseStatsTmp = CreatePauseTmpText("PauseStatsTmp", statsInner, Vector2.zero,
                    new Vector2(744f, 42f), string.Empty, 21f, Color.white);
                pauseStatsTmp.fontStyle = FontStyles.Bold;
                pauseStatsTmp.textWrappingMode = TextWrappingModes.NoWrap;
                pauseStatsTmp.enableAutoSizing = true;
                pauseStatsTmp.fontSizeMin = 15f;
                pauseStatsTmp.fontSizeMax = 21f;

                // Dòng gợi ý đáy bảng
                CreatePauseTmpText("PauseHintTmp", cardInner, new Vector2(0f, -218f), new Vector2(760f, 36f),
                    "Nhấn ESC hoặc P để tiếp tục  |  Về Menu chính sẽ giữ lại màn đang chơi",
                    20f, new Color(0.66f, 0.74f, 0.90f, 1f));
            }

            RestylePauseButton(resumeButton, new Vector2(0f, 38f), new Vector2(540f, 68f),
                new Color(0.14f, 0.74f, 0.34f, 1f), "TIẾP TỤC CHƠI", Color.white);
            RestylePauseButton(restartButton, new Vector2(0f, -46f), new Vector2(540f, 68f),
                new Color(0.16f, 0.48f, 0.88f, 1f), "CHƠI LẠI MÀN NÀY", Color.white);
            RestylePauseButton(mainMenuButton, new Vector2(0f, -130f), new Vector2(540f, 68f),
                new Color(0.84f, 0.50f, 0.12f, 1f), "VỀ MENU CHÍNH (TẠM DỪNG)", Color.white);
        }

        private void RestylePauseButton(Button btn, Vector2 pos, Vector2 size, Color bgColor, string labelText, Color textColor)
        {
            if (btn == null) return;

            RectTransform rt = btn.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = pos;
                rt.sizeDelta = size;
            }

            Image bg = btn.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = bgColor;
            }

            Outline outline = btn.GetComponent<Outline>();
            if (outline == null) outline = btn.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.03f, 0.04f, 0.08f, 0.85f);
            outline.effectDistance = new Vector2(3f, -3f);

            // Cân lại kích thước icon bên trái nút để không sát mép
            Transform iconTr = btn.transform.Find("Icon");
            if (iconTr != null)
            {
                RectTransform iconRt = iconTr as RectTransform;
                if (iconRt != null)
                {
                    iconRt.anchorMin = new Vector2(0f, 0.5f);
                    iconRt.anchorMax = new Vector2(0f, 0.5f);
                    iconRt.pivot = new Vector2(0f, 0.5f);
                    iconRt.anchoredPosition = new Vector2(18f, 0f);
                    iconRt.sizeDelta = new Vector2(44f, 44f);
                }
            }

            // Ẩn nhãn Text cũ bị mảnh/nhòe và thay bằng TextMeshPro sắc nét
            Text oldTxt = btn.GetComponentInChildren<Text>(true);
            if (oldTxt != null)
            {
                oldTxt.gameObject.SetActive(false);
            }

            Transform tmpTr = btn.transform.Find("TmpLabel");
            TextMeshProUGUI tmpLabel = tmpTr != null ? tmpTr.GetComponent<TextMeshProUGUI>() : null;
            if (tmpLabel == null)
            {
                GameObject go = new GameObject("TmpLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.transform.SetParent(btn.transform, false);
                RectTransform lrt = go.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(76f, 4f);
                lrt.offsetMax = new Vector2(-20f, -4f);
                tmpLabel = go.GetComponent<TextMeshProUGUI>();
            }

            if (cachedTmpFont != null) tmpLabel.font = cachedTmpFont;
            tmpLabel.text = labelText;
            tmpLabel.fontSize = 26f;
            tmpLabel.fontStyle = FontStyles.Bold;
            tmpLabel.color = textColor;
            tmpLabel.alignment = TextAlignmentOptions.Center;
            tmpLabel.textWrappingMode = TextWrappingModes.NoWrap;
            tmpLabel.raycastTarget = false;
        }

        private TMP_Text CreatePauseTmpText(string name, Transform parent, Vector2 pos, Vector2 size,
            string content, float fontSize, Color color)
        {
            RectTransform rt = RuntimeUiHelper.CreateUiRect(name, parent, pos, size);
            TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (cachedTmpFont != null) tmp.font = cachedTmpFont;
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        private void RefreshPauseStats()
        {
            if (pauseStatsTmp == null) return;

            GameManager gm = GameManager.Instance;
            if (gm == null)
            {
                pauseStatsTmp.text = "TRẠNG THÁI: ĐANG TẠM DỪNG";
                return;
            }

            int sec = Mathf.FloorToInt(gm.ElapsedTime);
            string timeStr = $"{sec / 60:00}:{sec % 60:00}";
            pauseStatsTmp.text =
                $"<color=#66F2FF>MÀN {gm.LevelIndex:00}: {GameSession.GetLevelDisplayName(gm.LevelIndex)}</color>  |  CỔ VẬT {gm.CollectedRelicsCount}/{gm.TotalRelicsRequired}  |  TIME {timeStr}  |  <color=#FF8888>CHẾT {gm.DeathCount:00}</color>";
        }

        #endregion

        #region Bật / tắt tạm dừng — Time.timeScale

        /// <summary>Bật/tắt bảng tạm dừng (phím ESC/P và nút Tạm dừng đều gọi hàm này).</summary>
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
                RefreshPauseStats();
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

        #region Nút Continue / Restart / Main Menu

        /// <summary>Ẩn bảng tạm dừng và trả tốc độ trước khi bấm Pause.</summary>
        private void OnResumeClicked()
        {
            SetPaused(false);
        }

        /// <summary>Đóng bảng, trả tốc độ chuẩn rồi khôi phục toàn màn chơi về Bệ Xuất Phát.</summary>
        private void OnRestartClicked()
        {
            GameSession.ClearPausedSession();
            CloseAndResume();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetLevel();
            }
        }

        /// <summary>Lưu màn đang tạm dừng, đóng bảng, trả tốc độ chuẩn rồi tải Scene MainMenu.</summary>
        private void OnMainMenuClicked()
        {
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                GameSession.SetPausedSession(gm.LevelIndex, gm.ElapsedTime, gm.DeathCount);
            }

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
