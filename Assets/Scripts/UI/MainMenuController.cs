using OneHitDown.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
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

        // Bảng mapping skin cho từng thẻ (NinjaFrog = 0, VirtualGuy = 1, PinkMan = 2, MaskDude = 3)
        private readonly CharacterSkin[] cardSkins = new CharacterSkin[4]
        {
            CharacterSkin.NinjaFrog,
            CharacterSkin.VirtualGuy,
            CharacterSkin.PinkMan,
            CharacterSkin.MaskDude
        };

        private readonly Outline[] cardOutlines = new Outline[4];
        private readonly Image[] cardBackgrounds = new Image[4];

        // Các mức âm lượng khi bấm nút AUDIO: 100% -> 75% -> 50% -> 25% -> 0% (Tắt) -> 100%
        private static readonly float[] VolumeSteps = { 1.0f, 0.75f, 0.50f, 0.25f, 0.0f };
        private int currentVolumeIndex = 0;
        private TMP_Text audioButtonLabel;

        private TMP_FontAsset cachedFont;

        private void Awake()
        {
            // Khắc phục triệt để lỗi Font Asset bị thiếu Atlas Texture (nguyên nhân gây UnassignedReferenceException làm đơ UI)
            FixMissingFontAtlases();

            // Tự động tìm kiếm, sửa lỗi thiếu Graphic raycast và kết nối toàn bộ UI (Self-Healing Runtime)
            AutoSetupAndFixScene();

            // Khởi tạo mức âm lượng
            InitAudioVolume();

            // Cập nhật giao diện ban đầu
            RefreshSelection();
            RefreshAudioDisplay();

            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(false);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private void FixMissingFontAtlases()
        {
            TMP_FontAsset safeFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (safeFont == null) safeFont = TMP_Settings.defaultFontAsset;

            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var txt in allTexts)
            {
                if (txt == null) continue;
                if (!IsFontValid(txt.font))
                {
                    txt.font = safeFont;
                }
            }

            cachedFont = safeFont;
        }

        private static bool IsFontValid(TMP_FontAsset font)
        {
            if (font == null) return false;
            if (font.atlasTextures == null || font.atlasTextures.Length == 0) return false;
            if (font.atlasTextures[0] == null) return false;
            return true;
        }

        private void AutoSetupAndFixScene()
        {
            Transform innerBoxTr = null;
            if (characterPanel != null)
            {
                innerBoxTr = characterPanel.transform.Find("InnerBox");
            }
            if (innerBoxTr == null)
            {
                GameObject ib = GameObject.Find("InnerBox");
                if (ib != null) innerBoxTr = ib.transform;
            }

            if (!IsFontValid(cachedFont))
            {
                cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (cachedFont == null) cachedFont = TMP_Settings.defaultFontAsset;
            }

            // 1. SETUP CHẾ ĐỘ 1P / 2P
            SetupModeUI(innerBoxTr);

            // 2. SETUP 4 THẺ NHÂN VẬT & NÚT CHỌN SKIN VỚI VIỀN SÁNG VÀ CHỮ SÁNG
            SetupCharacterCardsUI(innerBoxTr);

            // 3. SETUP 4 NÚT HÀNH ĐỘNG (PLAY, LEVELS, SETTINGS, AUDIO) ĐỂ BẤM LÀ ĂN NGAY
            SetupActionButtons(innerBoxTr);

            // 4. SETUP LEVEL SELECT PANEL
            if (levelSelectPanel == null)
            {
                GameObject lsp = GameObject.Find("LevelSelectPanel");
                if (lsp != null) levelSelectPanel = lsp;
            }
        }

        private void SetupModeUI(Transform innerBoxTr)
        {
            if (innerBoxTr == null) return;

            if (singlePlayerButton == null)
            {
                Transform tr1P = innerBoxTr.Find("Button_SinglePlayer");
                if (tr1P != null) singlePlayerButton = tr1P.GetComponent<Button>();
            }

            if (twoPlayerButton == null)
            {
                Transform tr2P = innerBoxTr.Find("Button_TwoPlayer");
                if (tr2P != null) twoPlayerButton = tr2P.GetComponent<Button>();
            }

            // Nếu chưa có 2 nút, tạo ngay tại runtime
            if (singlePlayerButton == null || twoPlayerButton == null)
            {
                if (modeToggleButton != null)
                {
                    modeToggleButton.gameObject.SetActive(false);
                }

                if (singlePlayerButton == null)
                {
                    singlePlayerButton = CreateRuntimeButton(innerBoxTr, "Button_SinglePlayer",
                        new Vector2(-185, 205), new Vector2(340, 50), "1 NGƯỜI CHƠI (1P)", 26);
                }

                if (twoPlayerButton == null)
                {
                    twoPlayerButton = CreateRuntimeButton(innerBoxTr, "Button_TwoPlayer",
                        new Vector2(185, 205), new Vector2(340, 50), "2 NGƯỜI CHƠI (2P CO-OP)", 26);
                }
            }

            if (singlePlayerButton != null)
            {
                singlePlayerButton.onClick.RemoveAllListeners();
                singlePlayerButton.onClick.AddListener(SetSinglePlayerMode);
                FixButtonRaycast(singlePlayerButton);
            }

            if (twoPlayerButton != null)
            {
                twoPlayerButton.onClick.RemoveAllListeners();
                twoPlayerButton.onClick.AddListener(SetTwoPlayerMode);
                FixButtonRaycast(twoPlayerButton);
            }

            if (modeLabel == null)
            {
                Transform mlTr = innerBoxTr.Find("ModeLabel");
                if (mlTr != null)
                {
                    modeLabel = mlTr.GetComponent<TMP_Text>();
                }
                else
                {
                    GameObject mlGo = new GameObject("ModeLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                    mlGo.transform.SetParent(innerBoxTr, false);
                    RectTransform rt = mlGo.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(950, 36);
                    rt.anchoredPosition = new Vector2(0, 155);
                    TextMeshProUGUI tmp = mlGo.GetComponent<TextMeshProUGUI>();
                    if (cachedFont != null) tmp.font = cachedFont;
                    tmp.fontSize = 24;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.white;
                    tmp.raycastTarget = false;
                    modeLabel = tmp;
                }
            }
        }

        private void SetupCharacterCardsUI(Transform innerBoxTr)
        {
            if (cardButtons == null || cardButtons.Length < 4) cardButtons = new Button[4];
            if (player1SkinButtons == null || player1SkinButtons.Length < 4) player1SkinButtons = new Button[4];
            if (player2SkinButtons == null || player2SkinButtons.Length < 4) player2SkinButtons = new Button[4];
            if (player1SelectionMarks == null || player1SelectionMarks.Length < 4) player1SelectionMarks = new GameObject[4];
            if (player2SelectionMarks == null || player2SelectionMarks.Length < 4) player2SelectionMarks = new GameObject[4];
            if (characterLabels == null || characterLabels.Length < 4) characterLabels = new TMP_Text[4];

            for (int i = 0; i < 4; i++)
            {
                Transform cardTr = null;
                if (innerBoxTr != null)
                {
                    cardTr = innerBoxTr.Find($"CharacterCard_{i}");
                }
                if (cardTr == null)
                {
                    GameObject found = GameObject.Find($"CharacterCard_{i}");
                    if (found != null) cardTr = found.transform;
                }

                if (cardTr == null) continue;

                // 1. Xác định skin
                TMP_Text foundLabel = cardTr.GetComponentInChildren<TMP_Text>(true);
                CharacterSkin assignedSkin = (CharacterSkin)i;
                if (foundLabel != null)
                {
                    string txt = foundLabel.text.ToUpperInvariant();
                    if (txt.Contains("FROG")) assignedSkin = CharacterSkin.NinjaFrog;
                    else if (txt.Contains("VIRTUAL")) assignedSkin = CharacterSkin.VirtualGuy;
                    else if (txt.Contains("PINK")) assignedSkin = CharacterSkin.PinkMan;
                    else if (txt.Contains("MASK")) assignedSkin = CharacterSkin.MaskDude;
                }
                cardSkins[i] = assignedSkin;
                characterLabels[i] = foundLabel;

                // 2. Đảm bảo thẻ có Image nền để nhận click chuột
                Image cardImg = cardTr.GetComponent<Image>();
                if (cardImg == null)
                {
                    cardImg = cardTr.gameObject.AddComponent<Image>();
                }
                cardImg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);
                cardImg.raycastTarget = true;
                cardBackgrounds[i] = cardImg;

                // 3. THÊM VIỀN ĐẬM RÕ NÉT XUNG QUANH THẺ (Outline dày 5px)
                Outline outline = cardTr.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = cardTr.gameObject.AddComponent<Outline>();
                }
                outline.effectDistance = new Vector2(5, 5);
                outline.useGraphicAlpha = false;
                outline.enabled = false;
                cardOutlines[i] = outline;

                // 4. Đảm bảo thẻ có Button với targetGraphic
                Button cardBtn = cardTr.GetComponent<Button>();
                if (cardBtn == null)
                {
                    cardBtn = cardTr.gameObject.AddComponent<Button>();
                }
                cardBtn.targetGraphic = cardImg;
                cardButtons[i] = cardBtn;

                int cardIdx = i;
                cardBtn.onClick.RemoveAllListeners();
                cardBtn.onClick.AddListener(() => OnCharacterCardClicked(cardIdx));

                // 5. Tìm P1_Mark & P2_Mark
                Transform p1MarkTr = cardTr.Find("P1_Mark");
                if (p1MarkTr != null)
                {
                    player1SelectionMarks[i] = p1MarkTr.gameObject;
                    Image p1Img = p1MarkTr.GetComponent<Image>();
                    if (p1Img != null)
                    {
                        p1Img.color = new Color(0f, 1f, 1f, 0.5f);
                        p1Img.raycastTarget = false;
                    }
                }

                Transform p2MarkTr = cardTr.Find("P2_Mark");
                if (p2MarkTr != null)
                {
                    player2SelectionMarks[i] = p2MarkTr.gameObject;
                    Image p2Img = p2MarkTr.GetComponent<Image>();
                    if (p2Img != null)
                    {
                        p2Img.color = new Color(1f, 0.6f, 0f, 0.5f);
                        p2Img.raycastTarget = false;
                    }
                }

                // 6. Tắt raycastTarget của Sprite và Label để không chắn click
                Image[] allImgs = cardTr.GetComponentsInChildren<Image>(true);
                foreach (var img in allImgs)
                {
                    if (img != cardImg && !img.name.StartsWith("Btn_"))
                    {
                        img.raycastTarget = false;
                    }
                }

                if (foundLabel != null)
                {
                    foundLabel.raycastTarget = false;
                }

                // 7. Tạo hoặc nối nút [ P1 ] và [ P2 ] ở đáy thẻ
                Transform btnsContainer = cardTr.Find("ButtonsContainer");
                if (btnsContainer == null)
                {
                    GameObject bcGo = new GameObject("ButtonsContainer", typeof(RectTransform));
                    bcGo.transform.SetParent(cardTr, false);
                    RectTransform bcRt = bcGo.GetComponent<RectTransform>();
                    bcRt.sizeDelta = new Vector2(180, 38);
                    bcRt.anchoredPosition = new Vector2(0, -68);
                    btnsContainer = bcGo.transform;
                }

                // Nút P1 (Chữ P1 sáng rực rỡ)
                Transform p1BtnTr = btnsContainer.Find("Btn_P1");
                Button p1Btn = p1BtnTr != null ? p1BtnTr.GetComponent<Button>() : null;
                if (p1Btn == null)
                {
                    p1Btn = CreateRuntimeButton(btnsContainer, "Btn_P1",
                        new Vector2(-44, 0), new Vector2(76, 34), "<b>P1</b>", 24);
                }
                player1SkinButtons[i] = p1Btn;
                p1Btn.onClick.RemoveAllListeners();
                p1Btn.onClick.AddListener(() => SelectSkinDirect(cardIdx, true));
                FixButtonRaycast(p1Btn);

                // Nút P2 (Chữ P2 sáng rực rỡ)
                Transform p2BtnTr = btnsContainer.Find("Btn_P2");
                Button p2Btn = p2BtnTr != null ? p2BtnTr.GetComponent<Button>() : null;
                if (p2Btn == null)
                {
                    p2Btn = CreateRuntimeButton(btnsContainer, "Btn_P2",
                        new Vector2(44, 0), new Vector2(76, 34), "<b>P2</b>", 24);
                }
                player2SkinButtons[i] = p2Btn;
                p2Btn.onClick.RemoveAllListeners();
                p2Btn.onClick.AddListener(() => SelectSkinDirect(cardIdx, false));
                FixButtonRaycast(p2Btn);
            }
        }

        private void SetupActionButtons(Transform innerBoxTr)
        {
            if (innerBoxTr == null) return;

            // 1. PLAY BUTTON
            if (playButton == null)
            {
                Transform tr = innerBoxTr.Find("ActionBtn_PLAY");
                if (tr != null) playButton = tr.GetComponent<Button>();
            }
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(PlayDirectly);
                FixButtonRaycast(playButton);
            }

            // 2. LEVELS BUTTON
            if (levelsButton == null)
            {
                Transform tr = innerBoxTr.Find("ActionBtn_LEVELS");
                if (tr != null) levelsButton = tr.GetComponent<Button>();
            }
            if (levelsButton != null)
            {
                levelsButton.onClick.RemoveAllListeners();
                levelsButton.onClick.AddListener(ShowLevelSelect);
                FixButtonRaycast(levelsButton);
            }

            // 3. SETTINGS BUTTON
            if (settingsButton == null)
            {
                Transform tr = innerBoxTr.Find("ActionBtn_SETTINGS");
                if (tr != null) settingsButton = tr.GetComponent<Button>();
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(ToggleSettings);
                FixButtonRaycast(settingsButton);
            }

            // 4. AUDIO BUTTON
            if (audioButton == null)
            {
                Transform tr = innerBoxTr.Find("ActionBtn_AUDIO");
                if (tr != null) audioButton = tr.GetComponent<Button>();
            }
            if (audioButton != null)
            {
                audioButton.onClick.RemoveAllListeners();
                audioButton.onClick.AddListener(CycleVolume);
                FixButtonRaycast(audioButton);

                // Tìm nhãn text dưới nút audio để hiển thị mức % âm lượng
                audioButtonLabel = audioButton.GetComponentInChildren<TMP_Text>(true);
            }

            if (feedbackLabel == null)
            {
                Transform tr = innerBoxTr.Find("FeedbackLabel");
                if (tr != null)
                {
                    feedbackLabel = tr.GetComponent<TMP_Text>();
                    if (feedbackLabel != null) feedbackLabel.raycastTarget = false;
                }
            }
        }

        private static void FixButtonRaycast(Button btn)
        {
            if (btn == null) return;

            Image rootImg = btn.GetComponent<Image>();
            if (rootImg != null)
            {
                rootImg.raycastTarget = true;
                btn.targetGraphic = rootImg;
            }

            // Tắt raycastTarget của tất cả con (Icon, Text) để không nuốt click
            Image[] childImgs = btn.GetComponentsInChildren<Image>(true);
            foreach (var img in childImgs)
            {
                if (img != rootImg)
                {
                    img.raycastTarget = false;
                }
            }

            TMP_Text[] childTexts = btn.GetComponentsInChildren<TMP_Text>(true);
            foreach (var txt in childTexts)
            {
                txt.raycastTarget = false;
            }
        }

        private Button CreateRuntimeButton(Transform parent, string name, Vector2 pos, Vector2 size, string text, float fontSize)
        {
            GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);

            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            Image img = btnGo.GetComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.25f, 1f);
            img.raycastTarget = true;

            Button btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = img;

            GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnGo.transform, false);

            RectTransform txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
            if (cachedFont != null) tmp.font = cachedFont;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return btn;
        }

        // ==================== QUẢN LÝ CHẾ ĐỘ ====================

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

        // ==================== QUẢN LÝ CHỌN SKIN ====================

        public void OnCharacterCardClicked(int cardIndex)
        {
            PlayClickClip();
            if (cardIndex < 0 || cardIndex >= cardSkins.Length) return;

            CharacterSkin selectedSkin = cardSkins[cardIndex];

            if (!GameSession.IsTwoPlayerMode)
            {
                // Chế độ 1 người: Bấm vào thẻ là chọn ngay cho P1
                GameSession.P1Skin = selectedSkin;
            }
            else
            {
                // Chế độ 2 người:
                // Nếu thẻ này chưa phải P1 thì gán P1
                // Nếu thẻ này đã là P1 thì gán P2
                if (GameSession.P1Skin != selectedSkin)
                {
                    GameSession.P1Skin = selectedSkin;
                }
                else
                {
                    GameSession.P2Skin = selectedSkin;
                }
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
                if (GameSession.IsTwoPlayerMode && skin == GameSession.P2Skin)
                {
                    PlayErrorClip();
                }
                GameSession.P1Skin = skin;
            }
            else
            {
                if (GameSession.IsTwoPlayerMode && skin == GameSession.P1Skin)
                {
                    PlayErrorClip();
                }
                GameSession.P2Skin = skin;
            }

            RefreshSelection();
        }

        // ==================== CẬP NHẬT GIAO DIỆN ====================

        public void RefreshSelection()
        {
            UpdateModeVisuals();
            UpdateSkinVisuals();
            RefreshCharacterLabels();

            bool isConflict = GameSession.IsTwoPlayerMode && GameSession.P1Skin == GameSession.P2Skin;
            if (playButton != null)
            {
                playButton.interactable = !isConflict;

                // Đổi màu nút Play sáng lên khi sẵn sàng chơi
                Image playImg = playButton.GetComponent<Image>();
                if (playImg != null)
                {
                    playImg.color = isConflict ? new Color(0.4f, 0.4f, 0.4f, 1f) : new Color(0.2f, 0.9f, 0.3f, 1f); // Xanh lá sáng khi sẵn sàng
                }
            }

            if (isConflict)
            {
                SetFeedback("<color=#FF4444>LỖI: 2 NGƯỜI CHƠI KHÔNG ĐƯỢC CHỌN TRÙNG NHÂN VẬT!</color>");
            }
            else
            {
                SetFeedback(string.Empty);
            }
        }

        private void UpdateModeVisuals()
        {
            bool is2P = GameSession.IsTwoPlayerMode;

            if (modeLabel != null)
            {
                modeLabel.text = is2P
                    ? "<color=#FF9500>[2 NGƯỜI CHƠI - DÂY XÍCH CO-OP]</color>  P1: A/D/W  |  P2: Phím Mũi Tên"
                    : "<color=#00FFFF>[1 NGƯỜI CHƠI]</color>  Điều khiển: A/D/W hoặc Mũi tên, Space nhảy";
            }

            // Nút 1P
            if (singlePlayerButton != null)
            {
                Image bg = singlePlayerButton.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = !is2P ? new Color(0f, 0.95f, 1f, 1f) : new Color(0.18f, 0.18f, 0.25f, 1f);
                }

                TMP_Text txt = singlePlayerButton.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.color = !is2P ? new Color(0.08f, 0.08f, 0.12f, 1f) : new Color(0.7f, 0.7f, 0.8f, 1f);
                }
            }

            // Nút 2P
            if (twoPlayerButton != null)
            {
                Image bg = twoPlayerButton.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = is2P ? new Color(1f, 0.6f, 0f, 1f) : new Color(0.18f, 0.18f, 0.25f, 1f);
                }

                TMP_Text txt = twoPlayerButton.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.color = is2P ? new Color(0.08f, 0.08f, 0.12f, 1f) : new Color(0.7f, 0.7f, 0.8f, 1f);
                }
            }
        }

        private void UpdateSkinVisuals()
        {
            bool is2P = GameSession.IsTwoPlayerMode;
            CharacterSkin p1Skin = GameSession.P1Skin;
            CharacterSkin p2Skin = GameSession.P2Skin;

            for (int i = 0; i < 4; i++)
            {
                CharacterSkin cardSkin = cardSkins[i];
                bool isP1 = cardSkin == p1Skin;
                bool isP2 = is2P && cardSkin == p2Skin;

                // 1. VIỀN SÁNG RỰC RỠ XUNG QUANH THẺ (OUTLINE DÀY 5PX)
                if (cardOutlines[i] != null)
                {
                    if (isP1 && isP2)
                    {
                        // Trùng màu: Viền đỏ cảnh báo
                        cardOutlines[i].enabled = true;
                        cardOutlines[i].effectColor = new Color(1f, 0.15f, 0.15f, 1f);
                    }
                    else if (isP1)
                    {
                        // P1: Viền Cyan sáng rực rỡ
                        cardOutlines[i].enabled = true;
                        cardOutlines[i].effectColor = new Color(0f, 1f, 1f, 1f);
                    }
                    else if (isP2)
                    {
                        // P2: Viền Cam sáng rực rỡ
                        cardOutlines[i].enabled = true;
                        cardOutlines[i].effectColor = new Color(1f, 0.6f, 0f, 1f);
                    }
                    else
                    {
                        // Không chọn: Tắt viền
                        cardOutlines[i].enabled = false;
                    }
                }

                // 2. MÀU TINT NỀN THẺ NHÂN VẬT
                if (cardBackgrounds[i] != null)
                {
                    if (isP1 && isP2)
                    {
                        cardBackgrounds[i].color = new Color(0.25f, 0.08f, 0.08f, 0.95f);
                    }
                    else if (isP1)
                    {
                        cardBackgrounds[i].color = new Color(0.05f, 0.18f, 0.22f, 0.95f);
                    }
                    else if (isP2)
                    {
                        cardBackgrounds[i].color = new Color(0.22f, 0.14f, 0.05f, 0.95f);
                    }
                    else
                    {
                        cardBackgrounds[i].color = new Color(0.12f, 0.12f, 0.18f, 0.95f);
                    }
                }

                // 3. KHUNG VIỀN DẤU P1 & P2
                if (player1SelectionMarks != null && i < player1SelectionMarks.Length && player1SelectionMarks[i] != null)
                {
                    player1SelectionMarks[i].SetActive(isP1);
                }

                if (player2SelectionMarks != null && i < player2SelectionMarks.Length && player2SelectionMarks[i] != null)
                {
                    player2SelectionMarks[i].SetActive(isP2);
                }

                // 4. NÚT P1 (CHỮ P1 SÁNG RỰC RỠ)
                if (player1SkinButtons != null && i < player1SkinButtons.Length && player1SkinButtons[i] != null)
                {
                    Image bg = player1SkinButtons[i].GetComponent<Image>();
                    TMP_Text txt = player1SkinButtons[i].GetComponentInChildren<TMP_Text>();

                    if (isP1)
                    {
                        if (bg != null) bg.color = new Color(0f, 0.95f, 1f, 1f); // Nền Cyan sáng
                        if (txt != null)
                        {
                            txt.text = "<b>P1</b>";
                            txt.color = Color.white; // Chữ trắng nổi bật
                            txt.fontSize = 26;
                        }
                    }
                    else
                    {
                        if (bg != null) bg.color = new Color(0.15f, 0.15f, 0.22f, 1f); // Nền tối
                        if (txt != null)
                        {
                            txt.text = "<b>P1</b>";
                            txt.color = new Color(0f, 0.95f, 1f, 1f); // Chữ Cyan sáng rực rỡ trên nền tối!
                            txt.fontSize = 24;
                        }
                    }
                }

                // 5. NÚT P2 (CHỮ P2 SÁNG RỰC RỠ - Ẩn khi ở chế độ 1P)
                if (player2SkinButtons != null && i < player2SkinButtons.Length && player2SkinButtons[i] != null)
                {
                    player2SkinButtons[i].gameObject.SetActive(is2P);
                    if (is2P)
                    {
                        Image bg = player2SkinButtons[i].GetComponent<Image>();
                        TMP_Text txt = player2SkinButtons[i].GetComponentInChildren<TMP_Text>();

                        if (isP2)
                        {
                            if (bg != null) bg.color = new Color(1f, 0.6f, 0f, 1f); // Nền Cam sáng
                            if (txt != null)
                            {
                                txt.text = "<b>P2</b>";
                                txt.color = Color.white; // Chữ trắng nổi bật
                                txt.fontSize = 26;
                            }
                        }
                        else
                        {
                            if (bg != null) bg.color = new Color(0.15f, 0.15f, 0.22f, 1f); // Nền tối
                            if (txt != null)
                            {
                                txt.text = "<b>P2</b>";
                                txt.color = new Color(1f, 0.65f, 0.1f, 1f); // Chữ Cam sáng rực rỡ trên nền tối!
                                txt.fontSize = 24;
                            }
                        }
                    }
                }
            }
        }

        private void RefreshCharacterLabels()
        {
            if (characterLabels == null) return;

            bool is2P = GameSession.IsTwoPlayerMode;
            CharacterSkin p1Skin = GameSession.P1Skin;
            CharacterSkin p2Skin = GameSession.P2Skin;

            for (int i = 0; i < characterLabels.Length; i++)
            {
                if (characterLabels[i] == null) continue;

                CharacterSkin skin = cardSkins[i];
                string baseName = GetSkinDisplayName(skin);
                bool isP1 = skin == p1Skin;
                bool isP2 = is2P && skin == p2Skin;

                if (isP1 && isP2)
                {
                    characterLabels[i].text = $"⚠ P1+P2: {baseName}";
                    characterLabels[i].color = new Color(1f, 0.35f, 0.35f);
                }
                else if (isP1)
                {
                    characterLabels[i].text = $"★ P1: {baseName}";
                    characterLabels[i].color = new Color(0f, 1f, 1f);
                }
                else if (isP2)
                {
                    characterLabels[i].text = $"★ P2: {baseName}";
                    characterLabels[i].color = new Color(1f, 0.7f, 0.1f);
                }
                else
                {
                    characterLabels[i].text = baseName;
                    characterLabels[i].color = new Color(0.85f, 0.85f, 0.95f);
                }
            }
        }

        private static string GetSkinDisplayName(CharacterSkin skin)
        {
            switch (skin)
            {
                case CharacterSkin.NinjaFrog: return "NINJA FROG";
                case CharacterSkin.VirtualGuy: return "VIRTUAL GUY";
                case CharacterSkin.PinkMan: return "PINK MAN";
                case CharacterSkin.MaskDude: return "MASK DUDE";
                default: return skin.ToString().ToUpperInvariant();
            }
        }

        // ==================== BẮT ĐẦU CHƠI (PLAY) ====================

        public void PlayDirectly()
        {
            PlayClickClip();
            if (GameSession.IsTwoPlayerMode && GameSession.P1Skin == GameSession.P2Skin)
            {
                SetFeedback("<color=#FF4444>LỖI: 2 người chơi không được chọn trùng nhân vật!</color>");
                PlayErrorClip();
                return;
            }

            int targetLevel = Mathf.Clamp(GameSession.CurrentLevelIndex, 1, 5);
            string sceneName = $"Level_{targetLevel:00}";

            Debug.Log($"[MainMenuController] ĐANG VÀO MÀN: {sceneName} | Chế độ: {(GameSession.IsTwoPlayerMode ? "2P" : "1P")} | P1: {GameSession.P1Skin} | P2: {GameSession.P2Skin}");

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                if (Application.CanStreamedLevelBeLoaded("Level_01"))
                {
                    sceneName = "Level_01";
                }
                else if (Application.CanStreamedLevelBeLoaded("Level_03"))
                {
                    sceneName = "Level_03";
                }
                else
                {
                    sceneName = "Level_05";
                }
            }

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                SetFeedback($"<color=#FF4444>Chưa tìm thấy Scene {sceneName} trong Build Settings.</color>");
            }
        }

        // ==================== CHỌN MÀN CHƠI (LEVELS) ====================

        public void ShowLevelSelect()
        {
            PlayClickClip();
            if (GameSession.IsTwoPlayerMode && GameSession.P1Skin == GameSession.P2Skin)
            {
                SetFeedback("<color=#FF4444>LỖI: 2 người chơi không được chọn trùng nhân vật!</color>");
                PlayErrorClip();
                return;
            }

            SetFeedback(string.Empty);
            if (characterPanel != null)
            {
                characterPanel.SetActive(false);
            }

            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(true);
                LevelSelectMenu lsm = levelSelectPanel.GetComponent<LevelSelectMenu>();
                if (lsm != null)
                {
                    lsm.RefreshLevels();
                }
            }
        }

        public void ShowCharacterSelect()
        {
            PlayClickClip();
            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(false);
            }

            if (characterPanel != null)
            {
                characterPanel.SetActive(true);
            }
        }

        // ==================== ĐIỀU CHỈNH ÂM LƯỢNG (AUDIO) ====================

        private void InitAudioVolume()
        {
            float curVol = AudioListener.volume;
            // Tìm index gần nhất
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

        /// <summary>
        /// Bấm nút AUDIO sẽ giảm dần âm lượng: 100% -> 75% -> 50% -> 25% -> 0% (Tắt) -> 100%
        /// </summary>
        public void CycleVolume()
        {
            PlayClickClip();
            currentVolumeIndex = (currentVolumeIndex + 1) % VolumeSteps.Length;
            float newVolume = VolumeSteps[currentVolumeIndex];

            AudioListener.volume = newVolume;
            AudioListener.pause = (newVolume <= 0.01f);

            RefreshAudioDisplay();

            if (newVolume > 0.01f)
            {
                SetFeedback($"<color=#00FFFF>Âm lượng trò chơi: {(int)(newVolume * 100)}%</color>");
            }
            else
            {
                SetFeedback("<color=#FF8888>Âm thanh: ĐÃ TẮT TIẾNG (MUTE)</color>");
            }
        }

        public void ToggleAudio()
        {
            CycleVolume();
        }

        private void RefreshAudioDisplay()
        {
            float vol = AudioListener.volume;
            bool isMuted = AudioListener.pause || vol <= 0.01f;

            if (audioIcon != null)
            {
                audioIcon.sprite = isMuted ? audioOffSprite : audioOnSprite;
            }

            if (audioButtonLabel != null)
            {
                if (isMuted)
                {
                    audioButtonLabel.text = "AUDIO <color=#FF4444>TẮT</color>";
                }
                else
                {
                    int pct = (int)(vol * 100);
                    audioButtonLabel.text = $"AUDIO <color=#00FFFF>{pct}%</color>";
                }
            }
        }

        public void ToggleSettings()
        {
            PlayClickClip();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
            else
            {
                SetFeedback("Điều khiển: P1: A/D/W/Space | P2: Phím Mũi Tên | R: Reset màn | ESC: Tạm dừng");
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
            if (uiAudioSource != null && errorClip != null)
            {
                uiAudioSource.PlayOneShot(errorClip);
            }
        }

        private void PlayClickClip()
        {
            if (uiAudioSource != null && clickClip != null)
            {
                uiAudioSource.PlayOneShot(clickClip);
            }
        }
    }
}
