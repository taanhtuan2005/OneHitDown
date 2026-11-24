// Phan Văn Tú (2351260693) & Nhóm 505 — Module 4: Main Menu, Intro Logo Động, Chọn Chế Độ & Skin
// Docs/SCHEMA_CHI_TIET.md §3 Module 4.6 — Docs/Phan_cong.md Mục III (Người 4).
using System.Collections;
using OneHitDown.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OneHitDown.UI
{
    /// <summary>
    /// Điều khiển toàn bộ màn hình Main Menu: Intro Logo động Nhóm 505, banner Tiếp tục màn
    /// đang tạm dừng, chọn 1P/2P, chọn 4 Skins (khóa trùng màu 2P), Cài đặt & Điều khiển âm thanh.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private static readonly float[] VolumeSteps = { 1.0f, 0.75f, 0.50f, 0.25f, 0.0f };
        private static readonly float[] CardPositionsX = { -420f, -140f, 140f, 420f };
        private static bool hasPlayedIntroSession;

        /// <summary>Đặt lại cờ Intro mỗi lần bấm Play (kể cả khi bật Fast Enter Play Mode).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticIntroState() => hasPlayedIntroSession = false;

        [Header("Các Panel UI")]
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Nút chế độ 1P / 2P")]
        [SerializeField] private Button singlePlayerButton;
        [SerializeField] private Button twoPlayerButton;
        [SerializeField] private Button modeToggleButton;
        [SerializeField] private TMP_Text modeLabel;

        [Header("Chọn Skin Nhân Vật (4 Skins)")]
        [SerializeField] private Button[] cardButtons = new Button[4];
        [SerializeField] private Button[] player1SkinButtons = new Button[4];
        [SerializeField] private Button[] player2SkinButtons = new Button[4];
        [SerializeField] private GameObject[] player1SelectionMarks = new GameObject[4];
        [SerializeField] private GameObject[] player2SelectionMarks = new GameObject[4];
        [SerializeField] private TMP_Text[] characterLabels = new TMP_Text[4];

        [Header("Các Nút Hành Động Chính")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Image audioIcon;
        [SerializeField] private Sprite audioOnSprite;
        [SerializeField] private Sprite audioOffSprite;

        [Header("Phản hồi & Âm thanh")]
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip errorClip;
        [SerializeField] private AudioClip clickClip;

        // Danh sách skin theo đúng thứ tự chuẩn enum (0..3)
        private readonly CharacterSkin[] cardSkins =
        {
            CharacterSkin.NinjaFrog,
            CharacterSkin.VirtualGuy,
            CharacterSkin.PinkMan,
            CharacterSkin.MaskDude
        };

        private readonly Sprite[] cachedSkinSprites = new Sprite[4];
        private readonly Outline[] cardOutlines = new Outline[4];
        private readonly Image[] cardBackgrounds = new Image[4];
        private readonly Image[] statusBadgeBgs = new Image[4];
        private readonly TMP_Text[] statusBadgeTexts = new TMP_Text[4];
        private readonly RectTransform[] cardSpriteRects = new RectTransform[4];

        private Sprite cachedPlayIcon;
        private Sprite cachedLevelsIcon;
        private Sprite cachedSettingsIcon;
        private TMP_FontAsset cachedFont;

        private RectTransform bgScrollRect;
        private float bgTilePeriodPx = 64f;
        private RectTransform headerLogoRect;

        private TMP_Text playButtonLabel;
        private TMP_Text audioButtonLabel;
        private TMP_Text settingsVolumeLabel;
        private TMP_Text settingsFeedbackLabel;
        private GameObject resumeBannerObj;
        private TMP_Text resumeBannerLabel;

        // Intro Splash Screen
        private GameObject introOverlay;
        private CanvasGroup introCanvasGroup;
        private RectTransform introContentRect;
        private RectTransform introLeftBeam;
        private RectTransform introRightBeam;
        private RectTransform introLogoRect;
        private Image introFlashImage;
        private TMP_Text introStudioText;
        private RectTransform introTitleRect;
        private TMP_Text introTitleText;
        private TMP_Text introTaglineText;
        private TMP_Text introSkipText;
        private Coroutine introCoroutine;
        private bool isIntroPlaying;
        private bool skipIntroRequested;

        private int currentVolumeIndex;

        #region Vòng đời MonoBehaviour

        private void Awake()
        {
            FixMissingFontAtlases();
            ExtractExistingSceneAssets();
            SetupBackgroundMotion();
            RebuildModernMenuUI();
            BuildSettingsPopup();
            BuildIntroOverlay();
            InitAudioVolume();

            RefreshSelection();
            RefreshAudioDisplay();

            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void Start()
        {
            if (!hasPlayedIntroSession)
            {
                hasPlayedIntroSession = true;
                StartIntroSequence();
            }
            else if (introOverlay != null)
            {
                introOverlay.SetActive(false);
            }
        }

        private void Update()
        {
            float t = Time.unscaledTime;

            // 1. Cuộn nền caro tím vô tận (Zero-GC)
            if (bgScrollRect != null)
            {
                float offset = Mathf.Repeat(t * 24f, bgTilePeriodPx);
                bgScrollRect.anchoredPosition = new Vector2(-offset, -offset);
            }

            // 2. Nhân vật nhấp nhô Idle trên 4 thẻ & Logo nhẹ nhàng thở
            for (int i = 0; i < cardSpriteRects.Length; i++)
            {
                if (cardSpriteRects[i] != null)
                {
                    float bob = Mathf.Sin(t * 3.4f + i * 0.9f) * 4.5f;
                    cardSpriteRects[i].anchoredPosition = new Vector2(0f, 34f + bob);
                }
            }

            if (headerLogoRect != null)
            {
                float s = 1f + Mathf.Sin(t * 2.5f) * 0.035f;
                headerLogoRect.localScale = new Vector3(s, s, 1f);
            }

            // 3. Bỏ qua Intro bằng phím (New Input System)
            if (isIntroPlaying && !skipIntroRequested)
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
                {
                    RequestSkipIntro();
                }
#endif
            }
        }

        #endregion

        #region Trích xuất tài nguyên sẵn có từ Scene & Dựng lại UI 5 tầng sạch đẹp

        private void FixMissingFontAtlases()
        {
            TMP_FontAsset safeFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (safeFont == null) safeFont = TMP_Settings.defaultFontAsset;

            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (TextMeshProUGUI txt in allTexts)
            {
                if (txt != null && !IsFontValid(txt.font))
                {
                    txt.font = safeFont;
                }
            }

            cachedFont = safeFont;
        }

        private static bool IsFontValid(TMP_FontAsset font)
        {
            return font != null && font.atlasTextures != null && font.atlasTextures.Length > 0 && font.atlasTextures[0] != null;
        }

        private void ExtractExistingSceneAssets()
        {
            Transform innerTr = characterPanel != null ? characterPanel.transform.Find("InnerBox") : null;
            if (innerTr == null)
            {
                GameObject ib = GameObject.Find("InnerBox");
                if (ib != null) innerTr = ib.transform;
            }

            // Trích xuất sprite 4 nhân vật theo nhãn gốc trên Scene
            for (int i = 0; i < 4; i++)
            {
                Transform cardTr = innerTr != null ? innerTr.Find($"CharacterCard_{i}") : null;
                if (cardTr == null)
                {
                    GameObject found = GameObject.Find($"CharacterCard_{i}");
                    if (found != null) cardTr = found.transform;
                }

                if (cardTr == null) continue;

                CharacterSkin mappedSkin = (CharacterSkin)i;
                TMP_Text label = cardTr.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    string upper = label.text.ToUpperInvariant();
                    if (upper.Contains("FROG")) mappedSkin = CharacterSkin.NinjaFrog;
                    else if (upper.Contains("VIRTUAL")) mappedSkin = CharacterSkin.VirtualGuy;
                    else if (upper.Contains("PINK")) mappedSkin = CharacterSkin.PinkMan;
                    else if (upper.Contains("MASK")) mappedSkin = CharacterSkin.MaskDude;
                }

                Transform spTr = cardTr.Find("Sprite");
                Image spImg = spTr != null ? spTr.GetComponent<Image>() : null;
                if (spImg != null && spImg.sprite != null)
                {
                    cachedSkinSprites[(int)mappedSkin] = spImg.sprite;
                }
            }

            // Trích xuất icon 4 nút hành động
            cachedPlayIcon = ExtractIcon(playButton, innerTr, "ActionBtn_PLAY");
            cachedLevelsIcon = ExtractIcon(levelsButton, innerTr, "ActionBtn_LEVELS");
            cachedSettingsIcon = ExtractIcon(settingsButton, innerTr, "ActionBtn_SETTINGS");

            if (audioOnSprite == null)
            {
                audioOnSprite = ExtractIcon(audioButton, innerTr, "ActionBtn_AUDIO");
            }

            if (levelSelectPanel == null)
            {
                GameObject lsp = GameObject.Find("LevelSelectPanel");
                if (lsp != null) levelSelectPanel = lsp;
            }
        }

        private static Sprite ExtractIcon(Button btn, Transform parent, string objName)
        {
            Transform tr = btn != null ? btn.transform : (parent != null ? parent.Find(objName) : null);
            if (tr == null) return null;
            Transform iconTr = tr.Find("Icon");
            Image img = iconTr != null ? iconTr.GetComponent<Image>() : tr.GetComponentInChildren<Image>(true);
            return img != null ? img.sprite : null;
        }

        private void SetupBackgroundMotion()
        {
            GameObject bgGo = GameObject.Find("Background");
            if (bgGo == null) return;

            Image bgImg = bgGo.GetComponent<Image>();
            RectTransform rt = bgGo.GetComponent<RectTransform>();
            if (bgImg == null || rt == null) return;

            bgImg.color = new Color(0.86f, 0.84f, 0.96f, 1f);

            if (bgImg.sprite != null && bgImg.sprite.pixelsPerUnit > 0f)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                float refPpu = canvas != null ? canvas.referencePixelsPerUnit : 100f;
                bgTilePeriodPx = bgImg.sprite.rect.width * (refPpu / bgImg.sprite.pixelsPerUnit);
                if (bgTilePeriodPx < 16f) bgTilePeriodPx = 64f;
            }

            // Mở rộng vùng phủ ngoài biên đúng 1 chu kỳ ô để cuộn liên tục không lộ viền
            float pad = Mathf.Max(128f, bgTilePeriodPx * 1.5f);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-pad, -pad);
            rt.offsetMax = new Vector2(pad, pad);
            bgScrollRect = rt;
        }

        private void RebuildModernMenuUI()
        {
            if (characterPanel == null)
            {
                GameObject cp = GameObject.Find("CharacterPanel");
                if (cp != null) characterPanel = cp;
            }

            if (characterPanel == null) return;

            // Khung chính 1220x840 viền vàng nổi bật
            RectTransform cpRect = characterPanel.GetComponent<RectTransform>();
            cpRect.sizeDelta = new Vector2(1220f, 840f);
            cpRect.anchoredPosition = Vector2.zero;
            Image cpBorder = characterPanel.GetComponent<Image>();
            if (cpBorder != null) cpBorder.color = new Color(1f, 0.85f, 0f, 1f);

            Transform innerTr = characterPanel.transform.Find("InnerBox");
            if (innerTr == null)
            {
                GameObject ibGo = new GameObject("InnerBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                ibGo.transform.SetParent(characterPanel.transform, false);
                innerTr = ibGo.transform;
            }

            RectTransform innerRect = innerTr.GetComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(6f, 6f);
            innerRect.offsetMax = new Vector2(-6f, -6f);
            Image innerBg = innerTr.GetComponent<Image>();
            if (innerBg != null) innerBg.color = new Color(0.07f, 0.08f, 0.13f, 0.98f);

            // Xóa sạch UI cũ bên trong InnerBox để sắp xếp 5 tầng hoàn toàn không chồng lấn
            for (int i = innerTr.childCount - 1; i >= 0; i--)
            {
                Transform child = innerTr.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            // ----- TẦNG 1: HEADER + LOGO NHÓM 505 (Bấm vào Huy hiệu để xem lại Intro) -----
            RectTransform logoBox = RuntimeUiHelper.CreateUiRect("HeaderLogo505", innerTr, new Vector2(-505f, 352f), new Vector2(74f, 74f));
            Image logoImg = logoBox.gameObject.AddComponent<Image>();
            logoImg.sprite = RuntimeUiHelper.GetTeam505LogoSprite();
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = true;
            Button logoBtn = logoBox.gameObject.AddComponent<Button>();
            logoBtn.targetGraphic = logoImg;
            logoBtn.onClick.AddListener(ReplayIntro);
            headerLogoRect = logoBox;

            CreateTmpText("TeamTag", innerTr, new Vector2(-395f, 352f), new Vector2(130f, 44f),
                "TEAM 505\nTLU STUDIO", 18f, new Color(0.55f, 0.88f, 1f, 1f), TextAlignmentOptions.Left);

            TMP_Text mainTitle = CreateTmpText("Title", innerTr, new Vector2(0f, 362f), new Vector2(700f, 68f),
                "ONE HIT DOWN", 68f, new Color(1f, 0.87f, 0f, 1f), TextAlignmentOptions.Center);
            mainTitle.fontStyle = FontStyles.Bold;

            CreateTmpText("Subtitle", innerTr, new Vector2(0f, 316f), new Vector2(760f, 28f),
                "CO-OP PIXEL PLATFORMER  |  5 MÀN THỬ THÁCH ĐỒNG ĐỘI", 21f,
                new Color(0.66f, 0.72f, 0.88f, 1f), TextAlignmentOptions.Center);

            // ----- BANNER TẠM DỪNG / DẢI PHÂN CÁCH -----
            BuildResumeOrDividerStrip(innerTr);

            // ----- TẦNG 2: CHỌN CHẾ ĐỘ 1P / 2P & DẢI PHÍM ĐIỀU KHIỂN -----
            singlePlayerButton = CreateModeButton("Button_SinglePlayer", innerTr, new Vector2(-225f, 210f),
                new Vector2(430f, 48f), "1 NGƯỜI CHƠI (1P SOLO)", SetSinglePlayerMode);
            twoPlayerButton = CreateModeButton("Button_TwoPlayer", innerTr, new Vector2(225f, 210f),
                new Vector2(430f, 48f), "2 NGƯỜI CHƠI (2P CO-OP)", SetTwoPlayerMode);

            RectTransform modeBar = RuntimeUiHelper.CreateUiRect("ModeBar", innerTr, new Vector2(0f, 160f), new Vector2(980f, 34f));
            Image modeBarBg = modeBar.gameObject.AddComponent<Image>();
            modeBarBg.color = new Color(0.11f, 0.13f, 0.21f, 1f);
            modeBarBg.raycastTarget = false;
            modeLabel = CreateTmpText("ModeLabel", modeBar, Vector2.zero, new Vector2(960f, 30f),
                string.Empty, 21f, Color.white, TextAlignmentOptions.Center);

            // ----- TẦNG 3: 4 THẺ NHÂN VẬT KHÔNG BAO GIỜ CHỒNG LẤN -----
            BuildCharacterCards(innerTr);

            // ----- TẦNG 4: DẢI THÔNG BÁO RIÊNG BIỆT (KHÔNG ĐÈ NÚT) -----
            RectTransform fbBar = RuntimeUiHelper.CreateUiRect("FeedbackBar", innerTr, new Vector2(0f, -176f), new Vector2(1100f, 40f));
            Image fbBg = fbBar.gameObject.AddComponent<Image>();
            fbBg.color = new Color(0.10f, 0.11f, 0.18f, 1f);
            fbBg.raycastTarget = false;
            feedbackLabel = CreateTmpText("FeedbackLabel", fbBar, Vector2.zero, new Vector2(1070f, 34f),
                string.Empty, 23f, new Color(1f, 0.4f, 0.4f, 1f), TextAlignmentOptions.Center);

            // ----- TẦNG 5: 4 NÚT HÀNH ĐỘNG ARCADE NẰM NGANG -----
            BuildActionButtonsRow(innerTr);

            // ----- CHÂN TRANG -----
            CreateTmpText("FooterHint", innerTr, new Vector2(0f, -364f), new Vector2(1120f, 30f),
                "NHÓM 505 (TLU)",
                19f, new Color(0.50f, 0.56f, 0.72f, 1f), TextAlignmentOptions.Center);
        }

        private void BuildResumeOrDividerStrip(Transform innerTr)
        {
            if (GameSession.HasPausedLevel)
            {
                int lvl = GameSession.PausedLevelIndex;
                int totalSec = Mathf.FloorToInt(GameSession.PausedElapsedTime);
                string timeStr = $"{totalSec / 60:00}:{totalSec % 60:00}";
                string text = $"[TAM DUNG] TIEP TUC MAN {lvl:00}: {GameSession.GetLevelDisplayName(lvl)}  |  TIME {timeStr}  |  CHET {GameSession.PausedDeathCount:00}  >> BAM DE CHOI TIEP";

                Button resumeBtn = CreateArcadePillButton("ResumePausedBanner", innerTr, new Vector2(0f, 268f),
                    new Vector2(1080f, 42f), new Color(0.08f, 0.34f, 0.28f, 1f), new Color(1f, 0.86f, 0.15f, 1f),
                    text, 21f, new Color(1f, 0.96f, 0.72f, 1f), ResumePausedLevel);
                resumeBannerObj = resumeBtn.gameObject;
                resumeBannerLabel = resumeBtn.GetComponentInChildren<TMP_Text>();
            }
            else
            {
                RectTransform div = RuntimeUiHelper.CreateUiRect("HeaderDivider", innerTr, new Vector2(0f, 268f), new Vector2(1080f, 4f));
                Image leftHalf = RuntimeUiHelper.CreateUiRect("LeftCyan", div, new Vector2(-270f, 0f), new Vector2(540f, 4f)).gameObject.AddComponent<Image>();
                leftHalf.color = new Color(0f, 0.82f, 0.95f, 0.65f);
                leftHalf.raycastTarget = false;
                Image rightHalf = RuntimeUiHelper.CreateUiRect("RightOrange", div, new Vector2(270f, 0f), new Vector2(540f, 4f)).gameObject.AddComponent<Image>();
                rightHalf.color = new Color(1f, 0.58f, 0.1f, 0.65f);
                rightHalf.raycastTarget = false;
                resumeBannerObj = null;
            }
        }

        private void BuildCharacterCards(Transform innerTr)
        {
            cardButtons = new Button[4];
            player1SkinButtons = new Button[4];
            player2SkinButtons = new Button[4];
            player1SelectionMarks = new GameObject[4];
            player2SelectionMarks = new GameObject[4];
            characterLabels = new TMP_Text[4];

            for (int i = 0; i < 4; i++)
            {
                int cardIdx = i;
                CharacterSkin skin = cardSkins[i];

                RectTransform cardRt = RuntimeUiHelper.CreateUiRect($"CharacterCard_{i}", innerTr,
                    new Vector2(CardPositionsX[i], -6f), new Vector2(256f, 268f));
                Image cardBg = cardRt.gameObject.AddComponent<Image>();
                cardBg.color = new Color(0.11f, 0.12f, 0.19f, 1f);
                cardBg.raycastTarget = true;
                cardBackgrounds[i] = cardBg;

                Outline outline = cardRt.gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(4f, 4f);
                outline.useGraphicAlpha = false;
                outline.enabled = false;
                cardOutlines[i] = outline;

                Button cardBtn = cardRt.gameObject.AddComponent<Button>();
                cardBtn.targetGraphic = cardBg;
                cardBtn.onClick.AddListener(() => OnCharacterCardClicked(cardIdx));
                cardButtons[i] = cardBtn;

                // 1. Ngăn trên cùng: Huy hiệu trạng thái P1 / P2
                RectTransform badgeRt = RuntimeUiHelper.CreateUiRect("StatusBadge", cardRt, new Vector2(0f, 108f), new Vector2(232f, 30f));
                Image badgeBg = badgeRt.gameObject.AddComponent<Image>();
                badgeBg.color = new Color(0.16f, 0.18f, 0.27f, 1f);
                badgeBg.raycastTarget = false;
                statusBadgeBgs[i] = badgeBg;
                statusBadgeTexts[i] = CreateTmpText("BadgeText", badgeRt, Vector2.zero, new Vector2(224f, 26f),
                    "SẴN SÀNG", 18f, new Color(0.75f, 0.8f, 0.92f, 1f), TextAlignmentOptions.Center);

                // 2. Ngăn thứ 2: Bục & Sprite Nhân Vật
                RectTransform pedestalRt = RuntimeUiHelper.CreateUiRect("Pedestal", cardRt, new Vector2(0f, 34f), new Vector2(128f, 98f));
                Image pedestalBg = pedestalRt.gameObject.AddComponent<Image>();
                pedestalBg.color = new Color(0.07f, 0.08f, 0.14f, 1f);
                pedestalBg.raycastTarget = false;

                RectTransform shadowRt = RuntimeUiHelper.CreateUiRect("FootShadow", cardRt, new Vector2(0f, -3f), new Vector2(54f, 12f));
                Image shadowImg = shadowRt.gameObject.AddComponent<Image>();
                shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
                shadowImg.raycastTarget = false;

                RectTransform spriteRt = RuntimeUiHelper.CreateUiRect("Sprite", cardRt, new Vector2(0f, 34f), new Vector2(84f, 84f));
                Image spriteImg = spriteRt.gameObject.AddComponent<Image>();
                spriteImg.sprite = cachedSkinSprites[(int)skin];
                spriteImg.preserveAspect = true;
                spriteImg.raycastTarget = false;
                cardSpriteRects[i] = spriteRt;

                // 3. Ngăn thứ 3: Bảng tên nhân vật cố định (tách biệt hoàn toàn với nút bên dưới)
                RectTransform plateRt = RuntimeUiHelper.CreateUiRect("NamePlate", cardRt, new Vector2(0f, -46f), new Vector2(232f, 34f));
                Image plateBg = plateRt.gameObject.AddComponent<Image>();
                plateBg.color = new Color(0.05f, 0.06f, 0.10f, 1f);
                plateBg.raycastTarget = false;

                characterLabels[i] = CreateTmpText("Label", plateRt, Vector2.zero, new Vector2(224f, 30f),
                    GameSession.GetSkinDisplayName(skin), 22f, Color.white, TextAlignmentOptions.Center);
                characterLabels[i].textWrappingMode = TextWrappingModes.NoWrap;

                // 4. Ngăn đáy: Nút chọn P1 & P2 riêng biệt
                RectTransform btnsBox = RuntimeUiHelper.CreateUiRect("ButtonsContainer", cardRt, new Vector2(0f, -100f), new Vector2(232f, 42f));

                Button p1Btn = CreateRuntimeButton(btnsBox, "Btn_P1", new Vector2(-55f, 0f), new Vector2(106f, 38f), "<b>P1</b>", 22f);
                p1Btn.onClick.AddListener(() => SelectSkinDirect(cardIdx, true));
                player1SkinButtons[i] = p1Btn;

                Button p2Btn = CreateRuntimeButton(btnsBox, "Btn_P2", new Vector2(55f, 0f), new Vector2(106f, 38f), "<b>P2</b>", 22f);
                p2Btn.onClick.AddListener(() => SelectSkinDirect(cardIdx, false));
                player2SkinButtons[i] = p2Btn;
            }
        }

        private void BuildActionButtonsRow(Transform innerTr)
        {
            playButton = CreateArcadeActionButton("ActionBtn_PLAY", innerTr, CardPositionsX[0], -268f,
                new Color(0.14f, 0.76f, 0.34f, 1f), cachedPlayIcon, "BẮT ĐẦU", PlayDirectly, out playButtonLabel, out _);

            levelsButton = CreateArcadeActionButton("ActionBtn_LEVELS", innerTr, CardPositionsX[1], -268f,
                new Color(0.16f, 0.48f, 0.88f, 1f), cachedLevelsIcon, "CHỌN MÀN", ShowLevelSelect, out _, out _);

            settingsButton = CreateArcadeActionButton("ActionBtn_SETTINGS", innerTr, CardPositionsX[2], -268f,
                new Color(0.42f, 0.32f, 0.86f, 1f), cachedSettingsIcon, "CÀI ĐẶT", ToggleSettings, out _, out _);

            audioButton = CreateArcadeActionButton("ActionBtn_AUDIO", innerTr, CardPositionsX[3], -268f,
                new Color(0.86f, 0.54f, 0.10f, 1f), audioOnSprite, "AUDIO\n100%", CycleVolume, out audioButtonLabel, out audioIcon);
        }

        private Button CreateArcadeActionButton(string name, Transform parent, float posX, float posY, Color bgColor,
            Sprite iconSprite, string labelText, UnityEngine.Events.UnityAction onClick, out TMP_Text outLabel, out Image outIcon)
        {
            RectTransform rt = RuntimeUiHelper.CreateUiRect(name, parent, new Vector2(posX, posY), new Vector2(256f, 84f));
            Image bg = rt.gameObject.AddComponent<Image>();
            bg.color = bgColor;
            bg.raycastTarget = true;

            Outline outline = rt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.55f);
            outline.effectDistance = new Vector2(3f, -3f);

            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            if (onClick != null) btn.onClick.AddListener(onClick);

            // Hộp sáng bên trái giúp icon đen Kenney luôn rõ nét tương phản cao
            RectTransform iconBadge = RuntimeUiHelper.CreateUiRect("IconBadge", rt, new Vector2(-78f, 0f), new Vector2(56f, 56f));
            Image badgeImg = iconBadge.gameObject.AddComponent<Image>();
            badgeImg.color = new Color(0.95f, 0.96f, 1f, 0.95f);
            badgeImg.raycastTarget = false;

            RectTransform iconRt = RuntimeUiHelper.CreateUiRect("Icon", iconBadge, Vector2.zero, new Vector2(38f, 38f));
            Image iconImg = iconRt.gameObject.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.color = Color.black;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            outIcon = iconImg;

            outLabel = CreateTmpText("Text", rt, new Vector2(36f, 0f), new Vector2(154f, 68f),
                labelText, 26f, Color.white, TextAlignmentOptions.Center);
            outLabel.fontStyle = FontStyles.Bold;

            return btn;
        }

        private void BuildSettingsPopup()
        {
            if (settingsPanel != null)
            {
                Destroy(settingsPanel);
                settingsPanel = null;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            Transform rootParent = canvas != null ? canvas.transform : transform;

            GameObject overlay = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlay.transform.SetParent(rootParent, false);
            RectTransform rt = overlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.88f);

            RectTransform inner = RuntimeUiHelper.CreateFramedCard("SettingsCard", overlay.transform,
                Vector2.zero, new Vector2(960f, 610f),
                new Color(1f, 0.86f, 0f, 1f), new Color(0.08f, 0.09f, 0.16f, 1f), 5f);

            CreateTmpText("Title", inner, new Vector2(0f, 240f), new Vector2(880f, 52f),
                "CÀI ĐẶT & HƯỚNG DẪN CHƠI", 46f, new Color(1f, 0.87f, 0f, 1f), TextAlignmentOptions.Center);

            // Khung hướng dẫn chi tiết (mở rộng & cỡ chữ 20 để mỗi mục gọn trên 1 dòng)
            RectTransform guideBox = RuntimeUiHelper.CreateFramedCard("GuideBox", inner,
                new Vector2(0f, 82f), new Vector2(890f, 224f),
                new Color(0.24f, 0.30f, 0.48f, 1f), new Color(0.05f, 0.06f, 0.11f, 1f), 3f);

            string guideText =
                "<color=#00F0FF>[1P SOLO]</color>  Di chuyển: A/D hoặc Mũi tên  |  Nhảy / Nhảy đôi: W, Space, Lên\n" +
                "<color=#FF9500>[2P CO-OP]</color> P1: A / D / W / Space  |  P2: Mũi tên Trái / Phải / Lên (Dây Xích 64px)\n\n" +
                "<color=#FFDE00>[LUẬT CHƠI]</color> Nhặt đủ 3 Cổ Vật mở Cổng Thần. Một người chết -> cả đội hồi sinh đầu màn!\n" +
                "<color=#AAFFAA>[PHÍM TẮT]</color>  R: Chơi lại  |  ESC / P: Tạm dừng  |  G: God Mode  |  1..5: Đổi màn nhanh";

            CreateTmpText("GuideContent", guideBox, Vector2.zero, new Vector2(854f, 198f),
                guideText, 20f, new Color(0.90f, 0.92f, 0.98f, 1f), TextAlignmentOptions.Left);

            // Nút âm lượng & đặt lại tiến độ (bỏ mở khóa tất cả để giữ tiến trình chơi thực tế)
            Button volBtn = CreateArcadePillButton("VolumeBtn", inner, new Vector2(0f, -66f), new Vector2(540f, 50f),
                new Color(0.18f, 0.24f, 0.40f, 1f), new Color(0f, 0.9f, 1f, 1f),
                "ÂM LƯỢNG TỔNG: 100%", 23f, Color.white, CycleVolume);
            settingsVolumeLabel = volBtn.GetComponentInChildren<TMP_Text>();

            CreateArcadePillButton("ResetProgressBtn", inner, new Vector2(0f, -132f), new Vector2(430f, 46f),
                new Color(0.45f, 0.16f, 0.20f, 1f), new Color(1f, 0.45f, 0.45f, 1f),
                "ĐẶT LẠI TIẾN ĐỘ (VỀ MÀN 1)", 22f, Color.white, () =>
                {
                    PlayClickClip();
                    GameSession.ResetAllProgress();
                    RefreshResumeBannerVisibility();
                    const string msg = "<color=#FFAA55>Đã đặt lại tiến độ: Chỉ mở Màn 01 (khóa Màn 02..05).</color>";
                    SetFeedback(msg);
                    if (settingsFeedbackLabel != null) settingsFeedbackLabel.text = msg;
                });

            settingsFeedbackLabel = CreateTmpText("SettingsFeedback", inner, new Vector2(0f, -184f), new Vector2(860f, 32f),
                string.Empty, 21f, new Color(1f, 0.85f, 0.4f, 1f), TextAlignmentOptions.Center);

            CreateArcadePillButton("CloseSettingsBtn", inner, new Vector2(0f, -242f), new Vector2(320f, 52f),
                new Color(0.92f, 0.94f, 0.98f, 1f), new Color(1f, 0.87f, 0f, 1f),
                "ĐÓNG BẢNG CÀI ĐẶT", 25f, new Color(0.08f, 0.08f, 0.12f, 1f), ToggleSettings);

            settingsPanel = overlay;
            settingsPanel.SetActive(false);
        }

        #endregion

        #region Màn hình Intro Logo Động Nhóm 505 × One Hit Down

        private void BuildIntroOverlay()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Transform rootParent = canvas != null ? canvas.transform : transform;

            introOverlay = new GameObject("IntroOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            introOverlay.transform.SetParent(rootParent, false);
            introOverlay.transform.SetAsLastSibling();

            RectTransform rt = introOverlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = introOverlay.GetComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.09f, 1f);
            bg.raycastTarget = true;

            // Cho phép nhấn chuột bất kỳ chỗ nào trên Intro để bỏ qua
            Button skipBtn = introOverlay.AddComponent<Button>();
            skipBtn.transition = Selectable.Transition.None;
            skipBtn.onClick.AddListener(RequestSkipIntro);

            introCanvasGroup = introOverlay.GetComponent<CanvasGroup>();

            introContentRect = RuntimeUiHelper.CreateUiRect("IntroContent", introOverlay.transform, Vector2.zero, new Vector2(1400f, 760f));

            // Dải tia năng lượng Cyan (P1) & Cam (P2) đẩy lên trên (y = 148) để không vướng chữ
            RectTransform leftRt = RuntimeUiHelper.CreateUiRect("LeftBeam", introContentRect, new Vector2(-720f, 148f), new Vector2(460f, 10f));
            Image leftImg = leftRt.gameObject.AddComponent<Image>();
            leftImg.color = new Color(0f, 0.95f, 1f, 1f);
            leftImg.raycastTarget = false;
            introLeftBeam = leftRt;

            RectTransform rightRt = RuntimeUiHelper.CreateUiRect("RightBeam", introContentRect, new Vector2(720f, 148f), new Vector2(460f, 10f));
            Image rightImg = rightRt.gameObject.AddComponent<Image>();
            rightImg.color = new Color(1f, 0.58f, 0.08f, 1f);
            rightImg.raycastTarget = false;
            introRightBeam = rightRt;

            // Huy hiệu Pixel Nhóm 505 ở phía trên trung tâm
            introLogoRect = RuntimeUiHelper.CreateUiRect("IntroShield505", introContentRect, new Vector2(0f, 148f), new Vector2(156f, 156f));
            Image logoImg = introLogoRect.gameObject.AddComponent<Image>();
            logoImg.sprite = RuntimeUiHelper.GetTeam505LogoSprite();
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = false;

            introStudioText = CreateTmpText("StudioPresent", introContentRect, new Vector2(0f, 38f), new Vector2(1000f, 44f),
                "NHÓM 505  x  TLU GAME STUDIO PRESENTS", 28f, new Color(0.65f, 0.90f, 1f, 0f), TextAlignmentOptions.Center);

            // Đại tự "ONE HIT DOWN" dập từ dưới lên không bao giờ lướt ngang mặt Khiên 505
            introTitleRect = RuntimeUiHelper.CreateUiRect("IntroGameTitle", introContentRect, new Vector2(0f, -72f), new Vector2(1100f, 120f));
            introTitleText = introTitleRect.gameObject.AddComponent<TextMeshProUGUI>();
            if (cachedFont != null) introTitleText.font = cachedFont;
            introTitleText.text = "ONE HIT DOWN";
            introTitleText.fontSize = 104f;
            introTitleText.fontStyle = FontStyles.Bold;
            introTitleText.color = new Color(1f, 0.87f, 0f, 0f);
            introTitleText.alignment = TextAlignmentOptions.Center;
            introTitleText.raycastTarget = false;

            introTaglineText = CreateTmpText("IntroTagline", introContentRect, new Vector2(0f, -168f), new Vector2(1080f, 44f),
                "CHỈ MỘT VA CHẠM  -  CẢ ĐỘI HỒI SINH ĐẦU MÀN!", 28f, new Color(1f, 0.65f, 0.25f, 0f), TextAlignmentOptions.Center);

            introSkipText = CreateTmpText("IntroSkipHint", introContentRect, new Vector2(0f, -305f), new Vector2(700f, 32f),
                "Nhấn Space / Click chuột để bỏ qua Intro...", 20f, new Color(0.60f, 0.65f, 0.78f, 0.85f), TextAlignmentOptions.Center);

            // Lớp lóe sáng va chạm phủ màn
            introFlashImage = RuntimeUiHelper.CreateStretchPanel("IntroFlash", introOverlay.transform, new Color(1f, 1f, 1f, 0f), false);

            introOverlay.SetActive(false);
        }

        public void ReplayIntro()
        {
            PlayClickClip();
            StartIntroSequence();
        }

        private void StartIntroSequence()
        {
            if (introOverlay == null) return;
            if (introCoroutine != null) StopCoroutine(introCoroutine);
            introCoroutine = StartCoroutine(PlayIntroRoutine());
        }

        private void RequestSkipIntro()
        {
            skipIntroRequested = true;
        }

        private IEnumerator PlayIntroRoutine()
        {
            isIntroPlaying = true;
            skipIntroRequested = false;

            introOverlay.SetActive(true);
            introOverlay.transform.SetAsLastSibling();
            introCanvasGroup.alpha = 1f;
            introContentRect.anchoredPosition = Vector2.zero;
            introLogoRect.localScale = Vector3.zero;
            introLeftBeam.anchoredPosition = new Vector2(-720f, 148f);
            introRightBeam.anchoredPosition = new Vector2(720f, 148f);
            introFlashImage.color = new Color(1f, 1f, 1f, 0f);
            SetTmpAlpha(introStudioText, 0f);
            SetTmpAlpha(introTitleText, 0f);
            SetTmpAlpha(introTaglineText, 0f);
            introTitleRect.anchoredPosition = new Vector2(0f, -20f);
            introTitleRect.localScale = new Vector3(2.2f, 2.2f, 1f);

            // Pha 1 (0.0s - 0.85s): Hai tia Cyan & Cam lao vào hợp nhất làm bật sáng Huy hiệu 505
            float t = 0f;
            const float phase1Dur = 0.85f;
            while (t < phase1Dur && !skipIntroRequested)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / phase1Dur);
                float ease = 1f - (1f - p) * (1f - p);

                introLeftBeam.anchoredPosition = new Vector2(Mathf.Lerp(-720f, -295f, ease), 148f);
                introRightBeam.anchoredPosition = new Vector2(Mathf.Lerp(720f, 295f, ease), 148f);

                if (p > 0.55f)
                {
                    float lp = (p - 0.55f) / 0.45f;
                    float scale = Mathf.Sin(lp * Mathf.PI * 0.5f) * 1.12f;
                    introLogoRect.localScale = new Vector3(scale, scale, 1f);
                }

                yield return null;
            }

            if (!skipIntroRequested)
            {
                introLogoRect.localScale = Vector3.one;
                PlayClickClip();
                SetTmpAlpha(introStudioText, 1f);
            }

            // Pha 2 (0.85s - 1.3s): Đại tự ONE HIT DOWN phóng đóng dấu từ trên nền xuống vị trí chuẩn (không qua mặt Khiên)
            t = 0f;
            const float dropDur = 0.36f;
            while (t < dropDur && !skipIntroRequested)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / dropDur);
                float easeIn = p * p * p;

                introTitleRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(-20f, -72f, easeIn));
                float sc = Mathf.Lerp(2.2f, 1f, easeIn);
                introTitleRect.localScale = new Vector3(sc, sc, 1f);
                SetTmpAlpha(introTitleText, p);
                yield return null;
            }

            if (!skipIntroRequested)
            {
                introTitleRect.anchoredPosition = new Vector2(0f, -72f);
                introTitleRect.localScale = Vector3.one;
                SetTmpAlpha(introTitleText, 1f);
                PlayClickClip();

                // Rung chấn động & lóe sáng
                float shakeT = 0f;
                const float shakeDur = 0.28f;
                while (shakeT < shakeDur && !skipIntroRequested)
                {
                    shakeT += Time.unscaledDeltaTime;
                    float sp = 1f - Mathf.Clamp01(shakeT / shakeDur);
                    introFlashImage.color = new Color(1f, 0.92f, 0.6f, sp * 0.45f);
                    float dx = Mathf.Sin(shakeT * 75f) * 10f * sp;
                    float dy = Mathf.Cos(shakeT * 90f) * 10f * sp;
                    introContentRect.anchoredPosition = new Vector2(dx, dy);
                    yield return null;
                }

                introContentRect.anchoredPosition = Vector2.zero;
                introFlashImage.color = new Color(1f, 1f, 1f, 0f);
            }

            // Pha 3 (Giữ logo + hiện Tagline mượt)
            t = 0f;
            const float holdDur = 1.25f;
            while (t < holdDur && !skipIntroRequested)
            {
                t += Time.unscaledDeltaTime;
                float tp = Mathf.Clamp01(t / 0.45f);
                SetTmpAlpha(introTaglineText, tp);
                float pulse = 0.45f + 0.45f * Mathf.Sin(t * 5f);
                SetTmpAlpha(introSkipText, pulse);
                yield return null;
            }

            // Pha 4: Mờ dần mở ra Main Menu
            t = 0f;
            float fadeDur = skipIntroRequested ? 0.16f : 0.45f;
            while (t < fadeDur)
            {
                t += Time.unscaledDeltaTime;
                introCanvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDur);
                yield return null;
            }

            introOverlay.SetActive(false);
            isIntroPlaying = false;
            introCoroutine = null;
        }

        private static void SetTmpAlpha(TMP_Text textComp, float alpha)
        {
            if (textComp == null) return;
            Color c = textComp.color;
            c.a = alpha;
            textComp.color = c;
        }

        #endregion

        #region Quản lý Chế độ & Chọn Skin

        public void SetSinglePlayerMode()
        {
            PlayClickClip();
            GameSession.IsTwoPlayerMode = false;
            RefreshSelection();
        }

        public void SetTwoPlayerMode()
        {
            PlayClickClip();
            GameSession.IsTwoPlayerMode = true;
            RefreshSelection();
        }

        public void ToggleMode()
        {
            PlayClickClip();
            GameSession.IsTwoPlayerMode = !GameSession.IsTwoPlayerMode;
            RefreshSelection();
        }

        public void OnCharacterCardClicked(int cardIndex)
        {
            PlayClickClip();
            if (cardIndex < 0 || cardIndex >= cardSkins.Length) return;

            CharacterSkin selectedSkin = cardSkins[cardIndex];
            if (!GameSession.IsTwoPlayerMode)
            {
                GameSession.P1Skin = selectedSkin;
            }
            else if (GameSession.P1Skin != selectedSkin)
            {
                GameSession.P1Skin = selectedSkin;
            }
            else
            {
                GameSession.P2Skin = selectedSkin;
            }

            RefreshSelection();
        }

        private void SelectSkinDirect(int cardIndex, bool isPlayer1)
        {
            PlayClickClip();
            if (cardIndex < 0 || cardIndex >= cardSkins.Length) return;

            CharacterSkin skin = cardSkins[cardIndex];
            if (isPlayer1)
            {
                if (GameSession.IsTwoPlayerMode && skin == GameSession.P2Skin) PlayErrorClip();
                GameSession.P1Skin = skin;
            }
            else
            {
                if (GameSession.IsTwoPlayerMode && skin == GameSession.P1Skin) PlayErrorClip();
                GameSession.P2Skin = skin;
            }

            RefreshSelection();
        }

        public void RefreshSelection()
        {
            UpdateModeVisuals();
            UpdateSkinVisuals();
            RefreshPlayButtonLabel();

            bool isConflict = GameSession.IsTwoPlayerMode && GameSession.P1Skin == GameSession.P2Skin;
            if (playButton != null)
            {
                playButton.interactable = !isConflict;
                Image playImg = playButton.GetComponent<Image>();
                if (playImg != null)
                {
                    playImg.color = isConflict
                        ? new Color(0.38f, 0.38f, 0.42f, 1f)
                        : new Color(0.14f, 0.76f, 0.34f, 1f);
                }
            }

            if (isConflict)
            {
                SetFeedback("<color=#FF5555>LỖI: Ở chế độ 2P Co-op, hai người chơi không được chọn trùng nhân vật!</color>");
            }
            else
            {
                SetFeedback(GameSession.HasPausedLevel
                    ? $"<color=#66FFCC>Đang tạm dừng Màn {GameSession.PausedLevelIndex:00}: {GameSession.GetLevelDisplayName(GameSession.PausedLevelIndex)} - Bấm BẮT ĐẦU hoặc Banner phía trên để chơi tiếp!</color>"
                    : "<color=#A5B4D4>Chọn nhân vật và bấm BẮT ĐẦU để vào cuộc phiêu lưu!</color>");
            }
        }

        private void RefreshPlayButtonLabel()
        {
            if (playButtonLabel == null) return;
            playButtonLabel.text = GameSession.HasPausedLevel
                ? $"TIẾP TỤC\nMÀN {GameSession.PausedLevelIndex:00}"
                : "BẮT ĐẦU";
        }

        private void RefreshResumeBannerVisibility()
        {
            if (resumeBannerObj != null && !GameSession.HasPausedLevel)
            {
                resumeBannerObj.SetActive(false);
            }

            RefreshPlayButtonLabel();
        }

        private void UpdateModeVisuals()
        {
            bool is2P = GameSession.IsTwoPlayerMode;

            if (modeLabel != null)
            {
                modeLabel.text = is2P
                    ? "<color=#FF9500>[2P CO-OP DÂY XÍCH]</color>  P1: A / D / W / Space   ||   P2: Phím Mũi Tên"
                    : "<color=#00F0FF>[1P SOLO]</color>  Di chuyển: A/D hoặc Mũi tên  |  Nhảy / Nhảy đôi: W / Space / Lên";
            }

            StyleModeButton(singlePlayerButton, !is2P, new Color(0f, 0.92f, 1f, 1f));
            StyleModeButton(twoPlayerButton, is2P, new Color(1f, 0.6f, 0.05f, 1f));
        }

        private static void StyleModeButton(Button btn, bool active, Color activeColor)
        {
            if (btn == null) return;
            Image bg = btn.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = active ? activeColor : new Color(0.15f, 0.16f, 0.24f, 1f);
            }

            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                txt.color = active ? new Color(0.06f, 0.07f, 0.12f, 1f) : new Color(0.75f, 0.78f, 0.88f, 1f);
            }
        }

        private void UpdateSkinVisuals()
        {
            bool is2P = GameSession.IsTwoPlayerMode;
            CharacterSkin p1Skin = GameSession.P1Skin;
            CharacterSkin p2Skin = GameSession.P2Skin;

            for (int i = 0; i < 4; i++)
            {
                CharacterSkin skin = cardSkins[i];
                bool isP1 = skin == p1Skin;
                bool isP2 = is2P && skin == p2Skin;

                if (cardOutlines[i] != null)
                {
                    if (isP1 && isP2)
                    {
                        cardOutlines[i].enabled = true;
                        cardOutlines[i].effectColor = new Color(1f, 0.2f, 0.2f, 1f);
                    }
                    else if (isP1)
                    {
                        cardOutlines[i].enabled = true;
                        cardOutlines[i].effectColor = new Color(0f, 0.95f, 1f, 1f);
                    }
                    else if (isP2)
                    {
                        cardOutlines[i].enabled = true;
                        cardOutlines[i].effectColor = new Color(1f, 0.6f, 0.05f, 1f);
                    }
                    else
                    {
                        cardOutlines[i].enabled = false;
                    }
                }

                if (cardBackgrounds[i] != null)
                {
                    if (isP1 && isP2) cardBackgrounds[i].color = new Color(0.24f, 0.09f, 0.10f, 1f);
                    else if (isP1) cardBackgrounds[i].color = new Color(0.06f, 0.17f, 0.22f, 1f);
                    else if (isP2) cardBackgrounds[i].color = new Color(0.22f, 0.14f, 0.06f, 1f);
                    else cardBackgrounds[i].color = new Color(0.11f, 0.12f, 0.19f, 1f);
                }

                if (statusBadgeBgs[i] != null && statusBadgeTexts[i] != null)
                {
                    if (isP1 && isP2)
                    {
                        statusBadgeBgs[i].color = new Color(0.75f, 0.16f, 0.16f, 1f);
                        statusBadgeTexts[i].text = "[!] TRÙNG NHÂN VẬT";
                        statusBadgeTexts[i].color = Color.white;
                    }
                    else if (isP1)
                    {
                        statusBadgeBgs[i].color = new Color(0f, 0.72f, 0.82f, 1f);
                        statusBadgeTexts[i].text = "P1 ĐANG CHỌN";
                        statusBadgeTexts[i].color = Color.white;
                    }
                    else if (isP2)
                    {
                        statusBadgeBgs[i].color = new Color(0.85f, 0.48f, 0.05f, 1f);
                        statusBadgeTexts[i].text = "P2 ĐANG CHỌN";
                        statusBadgeTexts[i].color = Color.white;
                    }
                    else
                    {
                        statusBadgeBgs[i].color = new Color(0.16f, 0.18f, 0.27f, 1f);
                        statusBadgeTexts[i].text = "SẴN SÀNG";
                        statusBadgeTexts[i].color = new Color(0.68f, 0.72f, 0.84f, 1f);
                    }
                }

                if (characterLabels[i] != null)
                {
                    characterLabels[i].text = GameSession.GetSkinDisplayName(skin);
                }

                if (player1SkinButtons[i] != null)
                {
                    RectTransform p1Rt = player1SkinButtons[i].GetComponent<RectTransform>();
                    if (is2P)
                    {
                        p1Rt.anchoredPosition = new Vector2(-55f, 0f);
                        p1Rt.sizeDelta = new Vector2(106f, 38f);
                    }
                    else
                    {
                        p1Rt.anchoredPosition = Vector2.zero;
                        p1Rt.sizeDelta = new Vector2(224f, 38f);
                    }

                    Image bg = player1SkinButtons[i].GetComponent<Image>();
                    TMP_Text txt = player1SkinButtons[i].GetComponentInChildren<TMP_Text>();
                    if (bg != null) bg.color = isP1 ? new Color(0f, 0.92f, 1f, 1f) : new Color(0.16f, 0.17f, 0.26f, 1f);
                    if (txt != null)
                    {
                        txt.textWrappingMode = TextWrappingModes.NoWrap;
                        txt.fontSize = is2P ? 22f : 20f;
                        txt.text = is2P ? "<b>P1</b>" : (isP1 ? "<b>ĐANG CHỌN</b>" : "<b>CHỌN P1</b>");
                        txt.color = isP1 ? new Color(0.05f, 0.06f, 0.1f, 1f) : new Color(0f, 0.92f, 1f, 1f);
                    }
                }

                if (player2SkinButtons[i] != null)
                {
                    player2SkinButtons[i].gameObject.SetActive(is2P);
                    if (is2P)
                    {
                        Image bg = player2SkinButtons[i].GetComponent<Image>();
                        TMP_Text txt = player2SkinButtons[i].GetComponentInChildren<TMP_Text>();
                        if (bg != null) bg.color = isP2 ? new Color(1f, 0.6f, 0.05f, 1f) : new Color(0.16f, 0.17f, 0.26f, 1f);
                        if (txt != null)
                        {
                            txt.textWrappingMode = TextWrappingModes.NoWrap;
                            txt.fontSize = 22f;
                            txt.text = "<b>P2</b>";
                            txt.color = isP2 ? new Color(0.05f, 0.06f, 0.1f, 1f) : new Color(1f, 0.65f, 0.15f, 1f);
                        }
                    }
                }
            }
        }

        #endregion

        #region Bắt đầu chơi, Chuyển panel & Âm lượng

        public void PlayDirectly()
        {
            if (GameSession.HasPausedLevel)
            {
                ResumePausedLevel();
                return;
            }

            PlayClickClip();
            if (HasTwoPlayerConflict()) return;

            int targetLevel = Mathf.Clamp(GameSession.CurrentLevelIndex, 1, 5);
            LoadLevelScene(targetLevel);
        }

        public void ResumePausedLevel()
        {
            PlayClickClip();
            if (HasTwoPlayerConflict()) return;

            int levelToResume = GameSession.HasPausedLevel
                ? GameSession.PausedLevelIndex
                : Mathf.Clamp(GameSession.CurrentLevelIndex, 1, 5);

            GameSession.ClearPausedSession();
            GameSession.CurrentLevelIndex = levelToResume;
            LoadLevelScene(levelToResume);
        }

        private bool HasTwoPlayerConflict()
        {
            if (GameSession.IsTwoPlayerMode && GameSession.P1Skin == GameSession.P2Skin)
            {
                SetFeedback("<color=#FF5555>LỖI: 2 người chơi không được chọn trùng nhân vật!</color>");
                PlayErrorClip();
                return true;
            }

            return false;
        }

        private void LoadLevelScene(int targetLevel)
        {
            string sceneName = $"Level_{targetLevel:00}";
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                if (Application.CanStreamedLevelBeLoaded("Level_01")) sceneName = "Level_01";
                else if (Application.CanStreamedLevelBeLoaded("Level_03")) sceneName = "Level_03";
                else sceneName = "Level_05";
            }

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                SetFeedback($"<color=#FF5555>Chưa tìm thấy Scene {sceneName} trong Build Settings.</color>");
            }
        }

        public void ShowLevelSelect()
        {
            PlayClickClip();
            if (HasTwoPlayerConflict()) return;

            SetFeedback(string.Empty);
            if (characterPanel != null) characterPanel.SetActive(false);

            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(true);
                LevelSelectMenu lsm = levelSelectPanel.GetComponent<LevelSelectMenu>();
                if (lsm != null) lsm.RefreshLevels();
            }
        }

        public void ShowCharacterSelect()
        {
            PlayClickClip();
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (characterPanel != null) characterPanel.SetActive(true);
            RefreshSelection();
        }

        private void InitAudioVolume()
        {
            float curVol = AudioListener.volume;
            int bestIdx = 0;
            float minDiff = float.MaxValue;
            for (int i = 0; i < VolumeSteps.Length; i++)
            {
                float diff = Mathf.Abs(VolumeSteps[i] - curVol);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestIdx = i;
                }
            }

            currentVolumeIndex = bestIdx;
        }

        public void CycleVolume()
        {
            PlayClickClip();
            currentVolumeIndex = (currentVolumeIndex + 1) % VolumeSteps.Length;
            float newVolume = VolumeSteps[currentVolumeIndex];

            AudioListener.volume = newVolume;
            AudioListener.pause = (newVolume <= 0.01f);

            RefreshAudioDisplay();

            string volumeFeedback = newVolume > 0.01f
                ? $"<color=#00F0FF>Âm lượng trò chơi: {(int)(newVolume * 100)}%</color>"
                : "<color=#FF7777>Âm thanh: ĐÃ TẮT TIẾNG (MUTE)</color>";
            SetFeedback(volumeFeedback);
            if (settingsFeedbackLabel != null) settingsFeedbackLabel.text = volumeFeedback;
        }

        public void ToggleAudio() => CycleVolume();

        private void RefreshAudioDisplay()
        {
            float vol = AudioListener.volume;
            bool isMuted = AudioListener.pause || vol <= 0.01f;

            if (audioIcon != null)
            {
                audioIcon.sprite = isMuted && audioOffSprite != null ? audioOffSprite : audioOnSprite;
            }

            int pct = Mathf.RoundToInt(vol * 100f);
            if (audioButtonLabel != null)
            {
                audioButtonLabel.text = isMuted ? "AUDIO\nTẮT" : $"AUDIO\n{pct}%";
            }

            if (audioButton != null)
            {
                Image bg = audioButton.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = isMuted
                        ? new Color(0.55f, 0.22f, 0.22f, 1f)
                        : new Color(0.86f, 0.54f, 0.10f, 1f);
                }
            }

            if (settingsVolumeLabel != null)
            {
                settingsVolumeLabel.text = isMuted
                    ? "ÂM LƯỢNG TỔNG: ĐÃ TẮT (BẤM ĐỂ MỞ)"
                    : $"ÂM LƯỢNG TỔNG: {pct}% (BẤM ĐỂ ĐỔI)";
            }
        }

        public void ToggleSettings()
        {
            PlayClickClip();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
                if (settingsPanel.activeSelf)
                {
                    settingsPanel.transform.SetAsLastSibling();
                    if (settingsFeedbackLabel != null) settingsFeedbackLabel.text = string.Empty;
                    RefreshAudioDisplay();
                }
            }
        }

        public void QuitGame()
        {
            PlayClickClip();
            Application.Quit();
        }

        private void SetFeedback(string message)
        {
            if (feedbackLabel != null)
            {
                feedbackLabel.text = message;
            }
        }

        private void PlayErrorClip()
        {
            if (uiAudioSource != null && errorClip != null) uiAudioSource.PlayOneShot(errorClip);
        }

        private void PlayClickClip()
        {
            if (uiAudioSource != null && clickClip != null) uiAudioSource.PlayOneShot(clickClip);
        }

        #endregion

        #region Tiện ích tạo UI TextMeshPro nội bộ

        private TMP_Text CreateTmpText(string name, Transform parent, Vector2 pos, Vector2 size,
            string text, float fontSize, Color color, TextAlignmentOptions align)
        {
            RectTransform rt = RuntimeUiHelper.CreateUiRect(name, parent, pos, size);
            TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (cachedFont != null) tmp.font = cachedFont;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        private Button CreateModeButton(string name, Transform parent, Vector2 pos, Vector2 size,
            string text, UnityEngine.Events.UnityAction onClick)
        {
            Button btn = CreateRuntimeButton(parent, name, pos, size, text, 25f);
            btn.onClick.AddListener(onClick);
            return btn;
        }

        private Button CreateRuntimeButton(Transform parent, string name, Vector2 pos, Vector2 size,
            string text, float fontSize)
        {
            RectTransform rt = RuntimeUiHelper.CreateUiRect(name, parent, pos, size);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.16f, 0.17f, 0.25f, 1f);
            img.raycastTarget = true;

            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            TMP_Text tmp = CreateTmpText("Text", rt, Vector2.zero, new Vector2(size.x - 12f, size.y - 6f),
                text, fontSize, Color.white, TextAlignmentOptions.Center);
            tmp.fontStyle = FontStyles.Bold;
            return btn;
        }

        private Button CreateArcadePillButton(string name, Transform parent, Vector2 pos, Vector2 size,
            Color bgColor, Color borderColor, string text, float fontSize, Color textColor,
            UnityEngine.Events.UnityAction onClick)
        {
            RectTransform rt = RuntimeUiHelper.CreateUiRect(name, parent, pos, size);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            Outline outline = rt.gameObject.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(2.5f, 2.5f);
            outline.useGraphicAlpha = false;

            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

            TMP_Text tmp = CreateTmpText("Text", rt, Vector2.zero, new Vector2(size.x - 20f, size.y - 6f),
                text, fontSize, textColor, TextAlignmentOptions.Center);
            tmp.fontStyle = FontStyles.Bold;
            return btn;
        }

        #endregion
    }
}
