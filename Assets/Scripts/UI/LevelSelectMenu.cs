// Phan Văn Tú (2351260693) & Nhóm 505 — Module 4: Bảng chọn 5 màn chơi
// Docs/SCHEMA_CHI_TIET.md §3 Module 4.6 — Docs/Phan_cong.md Mục III (Người 4).
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    /// <summary>
    /// Quản lý bảng chọn 5 màn chơi: hiển thị trạng thái Mở khóa / Đã khóa / Đang tạm dừng,
    /// tên chủ đề từng màn và nút mở khóa nhanh để kiểm thử toàn bộ 5 màn.
    /// </summary>
    public sealed class LevelSelectMenu : MonoBehaviour
    {
        [Header("Tham chiếu Nút & Icon (5 màn)")]
        [SerializeField] private Button[] levelButtons = new Button[5];
        [SerializeField] private Image[] lockIndicators = new Image[5];
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private MainMenuController mainMenu;

        private readonly Sprite[] cachedLevelSprites = new Sprite[5];
        private readonly Image[] cardBorders = new Image[5];
        private readonly Image[] cardBodies = new Image[5];
        private readonly Image[] lockBadgeBgs = new Image[5];
        private readonly TMP_Text[] statusLabels = new TMP_Text[5];
        private TMP_Text modeSummaryLabel;
        private TMP_FontAsset cachedFont;
        private bool hasBuiltModernLayout;

        private void Awake()
        {
            AutoBindLegacyRefs();
            CacheSpritesAndFont();
            RebuildModernLevelSelectUI();
            RefreshLevels();
        }

        private void OnEnable()
        {
            if (hasBuiltModernLayout)
            {
                RefreshLevels();
            }
        }

        private void AutoBindLegacyRefs()
        {
            if (levelButtons == null || levelButtons.Length < 5 || levelButtons[0] == null)
            {
                levelButtons = new Button[5];
                for (int i = 0; i < 5; i++)
                {
                    int lvlNum = i + 1;
                    Transform tr = transform.Find($"LevelBox/Inner/LevelBtn_{lvlNum:00}");
                    if (tr == null)
                    {
                        GameObject found = GameObject.Find($"LevelBtn_{lvlNum:00}");
                        if (found != null) tr = found.transform;
                    }

                    if (tr != null) levelButtons[i] = tr.GetComponent<Button>();
                }
            }

            if (backButton == null)
            {
                Transform btr = transform.Find("LevelBox/Inner/BackButton");
                if (btr == null)
                {
                    GameObject found = GameObject.Find("BackButton");
                    if (found != null) btr = found.transform;
                }

                if (btr != null) backButton = btr.GetComponent<Button>();
            }

            if (feedbackLabel == null)
            {
                Transform ftr = transform.Find("LevelBox/Inner/Feedback");
                if (ftr != null) feedbackLabel = ftr.GetComponent<TMP_Text>();
            }

            if (mainMenu == null)
            {
                mainMenu = FindFirstObjectByType<MainMenuController>();
            }
        }

        private void CacheSpritesAndFont()
        {
            cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (cachedFont == null) cachedFont = TMP_Settings.defaultFontAsset;

            for (int i = 0; i < 5; i++)
            {
                if (levelButtons != null && i < levelButtons.Length && levelButtons[i] != null)
                {
                    Image img = levelButtons[i].GetComponent<Image>();
                    if (img != null && img.sprite != null)
                    {
                        cachedLevelSprites[i] = img.sprite;
                    }
                }
            }
        }

        private void RebuildModernLevelSelectUI()
        {
            // Bảo đảm nền dimmer tối phủ toàn màn hình
            Image backdrop = GetComponent<Image>();
            if (backdrop == null) backdrop = gameObject.AddComponent<Image>();
            backdrop.color = new Color(0.02f, 0.02f, 0.05f, 0.88f);
            backdrop.raycastTarget = true;

            Transform oldBox = transform.Find("LevelBox");
            if (oldBox != null)
            {
                oldBox.SetParent(null);
                Destroy(oldBox.gameObject);
            }

            // Tạo khung chính 1140x580 với viền vàng #FFDE00
            RectTransform inner = RuntimeUiHelper.CreateFramedCard(
                "LevelBox", transform, Vector2.zero, new Vector2(1140f, 580f),
                new Color(1f, 0.87f, 0f, 1f), new Color(0.08f, 0.09f, 0.15f, 1f), 5f);

            // Tiêu đề & Tóm tắt chế độ đang chọn
            CreateTmpText("Title", inner, new Vector2(0f, 224f), new Vector2(960f, 56f),
                "CHỌN MÀN CHƠI (1 - 5)", 52f, new Color(1f, 0.87f, 0f, 1f));

            modeSummaryLabel = CreateTmpText("ModeSummary", inner, new Vector2(0f, 174f), new Vector2(980f, 34f),
                string.Empty, 24f, new Color(0.68f, 0.78f, 0.96f, 1f));

            // 5 Thẻ Màn Chơi
            levelButtons = new Button[5];
            lockIndicators = new Image[5];
            float[] cardX = { -420f, -210f, 0f, 210f, 420f };

            for (int i = 0; i < 5; i++)
            {
                int levelIndex = i + 1;
                RectTransform cardOuter = RuntimeUiHelper.CreateUiRect($"LevelBtn_{levelIndex:00}", inner,
                    new Vector2(cardX[i], 14f), new Vector2(192f, 246f));
                Image borderImg = cardOuter.gameObject.AddComponent<Image>();
                borderImg.color = new Color(0.28f, 0.32f, 0.48f, 1f);
                borderImg.raycastTarget = true;
                cardBorders[i] = borderImg;

                Button btn = cardOuter.gameObject.AddComponent<Button>();
                btn.targetGraphic = borderImg;
                btn.onClick.AddListener(() => SelectLevel(levelIndex));
                levelButtons[i] = btn;

                Image bodyImg = RuntimeUiHelper.CreateStretchPanel("CardBody", cardOuter,
                    new Color(0.11f, 0.13f, 0.21f, 1f), false);
                bodyImg.rectTransform.offsetMin = new Vector2(4f, 4f);
                bodyImg.rectTransform.offsetMax = new Vector2(-4f, -4f);
                cardBodies[i] = bodyImg;

                // Nhãn số màn ở đỉnh thẻ
                CreateTmpText("StageCode", bodyImg.rectTransform, new Vector2(0f, 94f), new Vector2(170f, 28f),
                    $"MÀN {levelIndex:00}", 24f, Color.white);

                // Khung icon số màn 01..05
                RectTransform iconBg = RuntimeUiHelper.CreateUiRect("IconBg", bodyImg.rectTransform,
                    new Vector2(0f, 26f), new Vector2(92f, 82f));
                Image iconBgImg = iconBg.gameObject.AddComponent<Image>();
                iconBgImg.color = new Color(0.06f, 0.07f, 0.12f, 1f);
                iconBgImg.raycastTarget = false;

                RectTransform iconRt = RuntimeUiHelper.CreateUiRect("LevelSprite", iconBg, Vector2.zero, new Vector2(64f, 64f));
                Image iconImg = iconRt.gameObject.AddComponent<Image>();
                iconImg.sprite = cachedLevelSprites[i];
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
                if (cachedLevelSprites[i] == null) iconImg.enabled = false;

                // Nền sáng cho icon ổ khóa (ảnh gốc Kenney màu đen nên cần nền sáng mới nổi bật)
                RectTransform lockBadgeRt = RuntimeUiHelper.CreateUiRect("LockBadge", iconBg,
                    new Vector2(36f, -29f), new Vector2(34f, 34f));
                Image lockBadgeBg = lockBadgeRt.gameObject.AddComponent<Image>();
                lockBadgeBg.color = new Color(1f, 0.42f, 0.42f, 1f);
                lockBadgeBg.raycastTarget = false;
                lockBadgeBgs[i] = lockBadgeBg;

                RectTransform lockRt = RuntimeUiHelper.CreateUiRect("LockIcon", lockBadgeRt, Vector2.zero, new Vector2(24f, 24f));
                Image lockImg = lockRt.gameObject.AddComponent<Image>();
                lockImg.sprite = lockedSprite;
                lockImg.color = Color.black;
                lockImg.preserveAspect = true;
                lockImg.raycastTarget = false;
                lockIndicators[i] = lockImg;

                // Tên chủ đề màn chơi
                CreateTmpText("StageTitle", bodyImg.rectTransform, new Vector2(0f, -44f), new Vector2(176f, 44f),
                    GameSession.GetLevelDisplayName(levelIndex), 22f, new Color(0.92f, 0.94f, 1f, 1f));

                // Thanh trạng thái ở đáy thẻ
                statusLabels[i] = CreateTmpText("StatusText", bodyImg.rectTransform,
                    new Vector2(0f, -94f), new Vector2(172f, 30f), "ĐÃ MỞ", 20f, new Color(0.25f, 0.95f, 0.5f, 1f));
            }

            // Dòng thông báo lỗi / hướng dẫn
            feedbackLabel = CreateTmpText("Feedback", inner, new Vector2(0f, -144f), new Vector2(980f, 34f),
                string.Empty, 24f, new Color(1f, 0.38f, 0.38f, 1f));

            // Căn giữa nút Quay Lại Menu (khóa cứng màn 2..5 theo tiến độ chơi thực tế)
            backButton = CreateTmpButton("BackButton", inner, new Vector2(0f, -210f), new Vector2(340f, 54f),
                new Color(0.92f, 0.94f, 0.98f, 1f), "<< QUAY LẠI MENU", 26f, new Color(0.08f, 0.08f, 0.12f, 1f), GoBack);

            hasBuiltModernLayout = true;
        }

        public void RefreshLevels()
        {
            if (modeSummaryLabel != null)
            {
                modeSummaryLabel.text = GameSession.IsTwoPlayerMode
                    ? $"[CHẾ ĐỘ 2P CO-OP]  P1: {GameSession.GetSkinDisplayName(GameSession.P1Skin)}  |  P2: {GameSession.GetSkinDisplayName(GameSession.P2Skin)}"
                    : $"[CHẾ ĐỘ 1P SOLO]  Nhân vật: {GameSession.GetSkinDisplayName(GameSession.P1Skin)}";
            }

            int buttonCount = levelButtons != null ? levelButtons.Length : 0;
            int iconCount = lockIndicators != null ? lockIndicators.Length : 0;
            int count = Mathf.Min(5, Mathf.Max(buttonCount, iconCount));

            for (int i = 0; i < count; i++)
            {
                int levelIndex = i + 1;
                bool unlocked = GameSession.IsLevelUnlocked(levelIndex);
                bool isPausedHere = GameSession.HasPausedLevel && GameSession.PausedLevelIndex == levelIndex;

                if (i < buttonCount && levelButtons[i] != null)
                {
                    levelButtons[i].interactable = true; // Cho phép bấm để hiện thông báo rõ nếu còn khóa
                }

                if (i < iconCount && lockIndicators[i] != null)
                {
                    lockIndicators[i].sprite = unlocked ? unlockedSprite : lockedSprite;
                    lockIndicators[i].color = Color.black;
                    lockIndicators[i].enabled = lockIndicators[i].sprite != null;
                }

                if (lockBadgeBgs[i] != null)
                {
                    lockBadgeBgs[i].color = unlocked
                        ? new Color(0.24f, 0.92f, 0.48f, 1f)
                        : new Color(1f, 0.42f, 0.42f, 1f);
                    lockBadgeBgs[i].enabled = lockIndicators[i] != null && lockIndicators[i].enabled;
                }

                if (cardBorders[i] != null)
                {
                    cardBorders[i].color = isPausedHere
                        ? new Color(1f, 0.84f, 0f, 1f)
                        : (unlocked ? new Color(0f, 0.88f, 0.96f, 1f) : new Color(0.28f, 0.30f, 0.40f, 1f));
                }

                if (cardBodies[i] != null)
                {
                    cardBodies[i].color = isPausedHere
                        ? new Color(0.10f, 0.22f, 0.20f, 1f)
                        : (unlocked ? new Color(0.11f, 0.15f, 0.25f, 1f) : new Color(0.09f, 0.09f, 0.13f, 1f));
                }

                if (statusLabels[i] != null)
                {
                    if (isPausedHere)
                    {
                        statusLabels[i].text = "[TAM DUNG - CHOI TIEP]";
                        statusLabels[i].color = new Color(1f, 0.87f, 0.2f, 1f);
                    }
                    else if (unlocked)
                    {
                        statusLabels[i].text = "[DA MO KHOA]";
                        statusLabels[i].color = new Color(0.28f, 0.95f, 0.52f, 1f);
                    }
                    else
                    {
                        statusLabels[i].text = "[DANG KHOA]";
                        statusLabels[i].color = new Color(0.65f, 0.45f, 0.45f, 1f);
                    }
                }
            }
        }

        public void SelectLevel(int levelIndex)
        {
            if (!GameSession.IsLevelUnlocked(levelIndex))
            {
                SetFeedback($"<color=#FF5555>Màn {levelIndex:00} đang khóa! Hãy hoàn thành Màn {levelIndex - 1:00} để mở khóa.</color>");
                return;
            }

            string sceneName = $"Level_{levelIndex:00}";
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SetFeedback($"<color=#FF5555>Chưa có scene {sceneName} trong Build Settings.</color>");
                return;
            }

            GameSession.ClearPausedSession();
            GameSession.CurrentLevelIndex = levelIndex;
            SceneManager.LoadScene(sceneName);
        }

        public void UnlockAllAndRefresh()
        {
            GameSession.UnlockAllLevels();
            RefreshLevels();
            SetFeedback("<color=#33FF88>Đã mở khóa toàn bộ 5 màn chơi! Chọn bất kỳ màn nào để trải nghiệm.</color>");
        }

        public void GoBack()
        {
            SetFeedback(string.Empty);
            if (mainMenu != null)
            {
                mainMenu.ShowCharacterSelect();
            }
            else
            {
                gameObject.SetActive(false);
                GameObject cp = GameObject.Find("CharacterPanel");
                if (cp != null) cp.SetActive(true);
            }
        }

        private void SetFeedback(string message)
        {
            if (feedbackLabel != null)
            {
                feedbackLabel.text = message;
            }
        }

        private TMP_Text CreateTmpText(string name, Transform parent, Vector2 pos, Vector2 size,
            string content, float fontSize, Color color)
        {
            RectTransform rt = RuntimeUiHelper.CreateUiRect(name, parent, pos, size);
            TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (cachedFont != null) tmp.font = cachedFont;
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        private Button CreateTmpButton(string name, Transform parent, Vector2 pos, Vector2 size,
            Color bgColor, string label, float fontSize, Color textColor, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform rt = RuntimeUiHelper.CreateUiRect(name, parent, pos, size);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

            CreateTmpText("Text", rt, Vector2.zero, new Vector2(size.x - 16f, size.y - 8f),
                label, fontSize, textColor);
            return btn;
        }
    }
}
