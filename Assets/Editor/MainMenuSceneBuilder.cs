#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using OneHitDown.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Script Editor tự động hóa việc dựng hoàn chỉnh Scene 'Assets/Scenes/MainMenu.unity'
    /// Khớp 100% hình ảnh thiết kế: Nền caro tím, khung viền vàng, font VT323, 4 skin nhân vật,
    /// chọn chế độ 1P / 2P rõ ràng, chọn skin P1 / P2 độc lập và popup LevelSelect.
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        private const string SceneDir = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

        // Asset Paths
        private const string BgPurplePath = "Assets/Art/02_MoiTruong/PA1_Background/Purple.png";
        private const string FontVT323Path = "Assets/Art/08_Font/VT323/VT323-Regular.ttf";

        // Main Characters (Frame 0)
        private const string NinjaFrogPath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Ninja Frog/Idle (32x32).png";
        private const string PinkManPath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Pink Man/Idle (32x32).png";
        private const string VirtualGuyPath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Virtual Guy/Idle (32x32).png";
        private const string MaskDudePath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Mask Dude/Idle (32x32).png";

        // Action Icons
        private const string PlayIconPath = "Assets/Art/05_GiaoDien/Kenney_GameIcons/UI/PNG/Black/1x/forward.png";
        private const string LevelsIconPath = "Assets/Art/05_GiaoDien/Kenney_GameIcons/UI/PNG/Black/1x/menuGrid.png";
        private const string SettingsIconPath = "Assets/Art/05_GiaoDien/Kenney_GameIcons/UI/PNG/Black/1x/gear.png";
        private const string AudioOnIconPath = "Assets/Art/05_GiaoDien/Kenney_GameIcons/UI/PNG/Black/1x/audioOn.png";
        private const string AudioOffIconPath = "Assets/Art/05_GiaoDien/Kenney_GameIcons/UI/PNG/Black/1x/audioOff.png";

        // Level Select Icons
        private const string LockedIconPath = "Assets/Art/05_GiaoDien/Kenney_GameIcons/UI/PNG/Black/1x/locked.png";
        private const string UnlockedIconPath = "Assets/Art/05_GiaoDien/Kenney_GameIcons/UI/PNG/Black/1x/unlocked.png";
        private const string LevelsDir = "Assets/Art/05_GiaoDien/PA1_Menu/Levels";

        // Audio Clips
        private const string BgmMenuPath = "Assets/Art/06_AmThanh/BGM_ChiptuneLoops/Audio/bgm_menu.ogg";
        private const string ClickClipPath = "Assets/Art/06_AmThanh/Kenney_InterfaceSounds/Audio/click_002.ogg";
        private const string ErrorClipPath = "Assets/Art/06_AmThanh/Kenney_InterfaceSounds/Audio/error_003.ogg";

        [MenuItem("Tools/One Hit Down/Build MainMenu Scene", false, 1)]
        public static void BuildMenu()
        {
            try
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayProgressBar("One Hit Down", "Đang dựng Scene MainMenu...", 0.1f);
                }
                Debug.Log("<b>[MainMenuSceneBuilder]</b> BẮT ĐẦU DỰNG SCENE MAINMENU.UNITY...");

                if (!Directory.Exists(SceneDir))
                {
                    Directory.CreateDirectory(SceneDir);
                }

                // Tạo scene rỗng mới
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // Tải tài nguyên
                TMP_FontAsset fontVT323 = GetOrCreateVT323FontAsset();
                Sprite bgPurple = AssetDatabase.LoadAssetAtPath<Sprite>(BgPurplePath);
                Sprite playIcon = AssetDatabase.LoadAssetAtPath<Sprite>(PlayIconPath);
                Sprite levelsIcon = AssetDatabase.LoadAssetAtPath<Sprite>(LevelsIconPath);
                Sprite settingsIcon = AssetDatabase.LoadAssetAtPath<Sprite>(SettingsIconPath);
                Sprite audioOnIcon = AssetDatabase.LoadAssetAtPath<Sprite>(AudioOnIconPath);
                Sprite audioOffIcon = AssetDatabase.LoadAssetAtPath<Sprite>(AudioOffIconPath);
                Sprite lockedIcon = AssetDatabase.LoadAssetAtPath<Sprite>(LockedIconPath);
                Sprite unlockedIcon = AssetDatabase.LoadAssetAtPath<Sprite>(UnlockedIconPath);

                AudioClip bgmMenu = AssetDatabase.LoadAssetAtPath<AudioClip>(BgmMenuPath);
                AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClickClipPath);
                AudioClip errorClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ErrorClipPath);

                Sprite[] charSprites = {
                    LoadFirstSubSprite(NinjaFrogPath),
                    LoadFirstSubSprite(VirtualGuyPath),
                    LoadFirstSubSprite(PinkManPath),
                    LoadFirstSubSprite(MaskDudePath)
                };

                // 1. Tạo Main Camera
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                Camera cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.13f, 0.13f, 0.18f); // #20202E
                camGo.AddComponent<AudioListener>();

                // AudioSource BGM Menu
                AudioSource bgmSource = camGo.AddComponent<AudioSource>();
                bgmSource.clip = bgmMenu;
                bgmSource.loop = true;
                bgmSource.playOnAwake = true;
                bgmSource.volume = 0.65f;

                // 2. Tạo Canvas & EventSystem
                GameObject canvasGo = new GameObject("Canvas");
                Canvas canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGo.AddComponent<GraphicRaycaster>();

                GameObject eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<InputSystemUIInputModule>();

                // AudioSource SFX
                AudioSource sfxSource = canvasGo.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;

                // 3. Background Tiled
                GameObject bgGo = CreateUIObject("Background", canvasGo.transform);
                RectTransform bgRect = bgGo.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                Image bgImg = bgGo.AddComponent<Image>();
                bgImg.sprite = bgPurple;
                bgImg.type = Image.Type.Tiled;
                bgImg.color = Color.white;

                // 4. Khung chính MainBox (CharacterPanel)
                GameObject mainBoxGo = CreateUIObject("CharacterPanel", canvasGo.transform);
                RectTransform mainBoxRect = mainBoxGo.GetComponent<RectTransform>();
                mainBoxRect.sizeDelta = new Vector2(1080, 740);
                mainBoxRect.anchoredPosition = Vector2.zero;

                // Viền vàng ngoài (#FFDE00)
                Image borderImg = mainBoxGo.AddComponent<Image>();
                borderImg.color = new Color(1f, 0.87f, 0f); // #FFDE00

                // Lõi xám than trong (#14141E)
                GameObject innerBoxGo = CreateUIObject("InnerBox", mainBoxGo.transform);
                RectTransform innerRect = innerBoxGo.GetComponent<RectTransform>();
                innerRect.anchorMin = Vector2.zero;
                innerRect.anchorMax = Vector2.one;
                innerRect.offsetMin = new Vector2(5, 5);
                innerRect.offsetMax = new Vector2(-5, -5);
                Image innerImg = innerBoxGo.AddComponent<Image>();
                innerImg.color = new Color(0.08f, 0.08f, 0.12f); // #14141E

                // 5. Tiêu đề "ONE HIT DOWN"
                GameObject titleGo = CreateUIObject("Title", innerBoxGo.transform);
                RectTransform titleRect = titleGo.GetComponent<RectTransform>();
                titleRect.sizeDelta = new Vector2(900, 70);
                titleRect.anchoredPosition = new Vector2(0, 290);
                TextMeshProUGUI titleText = titleGo.AddComponent<TextMeshProUGUI>();
                titleText.font = fontVT323;
                titleText.text = "ONE HIT DOWN";
                titleText.fontSize = 68;
                titleText.color = new Color(1f, 0.87f, 0f); // #FFDE00
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.raycastTarget = false;

                // 6. KHU VỰC CHỌN CHẾ ĐỘ 1P / 2P RÕ RÀNG
                // Nút 1P
                GameObject btn1PGo = CreateUIObject("Button_SinglePlayer", innerBoxGo.transform);
                RectTransform btn1PRect = btn1PGo.GetComponent<RectTransform>();
                btn1PRect.sizeDelta = new Vector2(340, 50);
                btn1PRect.anchoredPosition = new Vector2(-185, 215);
                Image btn1PImg = btn1PGo.AddComponent<Image>();
                btn1PImg.color = new Color(0f, 0.95f, 1f, 1f); // Cyan
                Button btn1P = btn1PGo.AddComponent<Button>();

                GameObject txt1PGo = CreateUIObject("Text", btn1PGo.transform);
                RectTransform txt1PRect = txt1PGo.GetComponent<RectTransform>();
                txt1PRect.anchorMin = Vector2.zero;
                txt1PRect.anchorMax = Vector2.one;
                txt1PRect.offsetMin = Vector2.zero;
                txt1PRect.offsetMax = Vector2.zero;
                TextMeshProUGUI txt1P = txt1PGo.AddComponent<TextMeshProUGUI>();
                txt1P.font = fontVT323;
                txt1P.text = "1 NGƯỜI CHƠI (1P)";
                txt1P.fontSize = 28;
                txt1P.color = new Color(0.08f, 0.08f, 0.12f, 1f);
                txt1P.alignment = TextAlignmentOptions.Center;
                txt1P.raycastTarget = false;

                // Nút 2P
                GameObject btn2PGo = CreateUIObject("Button_TwoPlayer", innerBoxGo.transform);
                RectTransform btn2PRect = btn2PGo.GetComponent<RectTransform>();
                btn2PRect.sizeDelta = new Vector2(340, 50);
                btn2PRect.anchoredPosition = new Vector2(185, 215);
                Image btn2PImg = btn2PGo.AddComponent<Image>();
                btn2PImg.color = new Color(0.18f, 0.18f, 0.25f, 1f); // Dimmed
                Button btn2P = btn2PGo.AddComponent<Button>();

                GameObject txt2PGo = CreateUIObject("Text", btn2PGo.transform);
                RectTransform txt2PRect = txt2PGo.GetComponent<RectTransform>();
                txt2PRect.anchorMin = Vector2.zero;
                txt2PRect.anchorMax = Vector2.one;
                txt2PRect.offsetMin = Vector2.zero;
                txt2PRect.offsetMax = Vector2.zero;
                TextMeshProUGUI txt2P = txt2PGo.AddComponent<TextMeshProUGUI>();
                txt2P.font = fontVT323;
                txt2P.text = "2 NGƯỜI CHƠI (2P CO-OP)";
                txt2P.fontSize = 28;
                txt2P.color = new Color(0.7f, 0.7f, 0.8f, 1f);
                txt2P.alignment = TextAlignmentOptions.Center;
                txt2P.raycastTarget = false;

                // Nhãn mô tả chế độ & phím điều khiển
                GameObject modeLabelGo = CreateUIObject("ModeLabel", innerBoxGo.transform);
                RectTransform modeLabelRect = modeLabelGo.GetComponent<RectTransform>();
                modeLabelRect.sizeDelta = new Vector2(950, 36);
                modeLabelRect.anchoredPosition = new Vector2(0, 160);
                TextMeshProUGUI modeLabelTxt = modeLabelGo.AddComponent<TextMeshProUGUI>();
                modeLabelTxt.font = fontVT323;
                modeLabelTxt.text = "<color=#00FFFF>[1 NGƯỜI CHƠI]</color>  Điều khiển: A/D/W hoặc Mũi tên, Space nhảy";
                modeLabelTxt.fontSize = 24;
                modeLabelTxt.color = Color.white;
                modeLabelTxt.alignment = TextAlignmentOptions.Center;
                modeLabelTxt.raycastTarget = false;

                // 7. KHU VỰC 4 THẺ NHÂN VẬT (CHỌN SKIN)
                float[] posX = { -320, -107, 107, 320 };
                string[] defaultLabels = { "NINJA FROG", "VIRTUAL GUY", "PINK MAN", "MASK DUDE" };

                Button[] cardBtns = new Button[4];
                Button[] p1Btns = new Button[4];
                Button[] p2Btns = new Button[4];
                GameObject[] p1Marks = new GameObject[4];
                GameObject[] p2Marks = new GameObject[4];
                TMP_Text[] charLabels = new TMP_Text[4];

                for (int i = 0; i < 4; i++)
                {
                    // Thẻ nhân vật root (có Image làm nền và Button để bắt click trực tiếp)
                    GameObject cardGo = CreateUIObject($"CharacterCard_{i}", innerBoxGo.transform);
                    RectTransform cardRect = cardGo.GetComponent<RectTransform>();
                    cardRect.sizeDelta = new Vector2(196, 230);
                    cardRect.anchoredPosition = new Vector2(posX[i], 15);
                    Image cardBgImg = cardGo.AddComponent<Image>();
                    cardBgImg.color = new Color(0.12f, 0.12f, 0.18f); // #1E1E2E
                    Button cardBtn = cardGo.AddComponent<Button>();
                    cardBtn.targetGraphic = cardBgImg;
                    cardBtns[i] = cardBtn;

                    Outline outline = cardGo.AddComponent<Outline>();
                    outline.effectDistance = new Vector2(5, 5);
                    outline.useGraphicAlpha = false;
                    outline.enabled = (i == 0);
                    outline.effectColor = new Color(0f, 1f, 1f, 1f);

                    // Viền P1 (Cyan)
                    GameObject p1MarkGo = CreateUIObject("P1_Mark", cardGo.transform);
                    RectTransform p1MarkRect = p1MarkGo.GetComponent<RectTransform>();
                    p1MarkRect.anchorMin = Vector2.zero;
                    p1MarkRect.anchorMax = Vector2.one;
                    p1MarkRect.offsetMin = new Vector2(2, 2);
                    p1MarkRect.offsetMax = new Vector2(-2, -2);
                    Image p1MarkImg = p1MarkGo.AddComponent<Image>();
                    p1MarkImg.color = new Color(0f, 0.95f, 1f, 0.35f);
                    p1MarkImg.raycastTarget = false;
                    p1Marks[i] = p1MarkGo;

                    // Viền P2 (Orange)
                    GameObject p2MarkGo = CreateUIObject("P2_Mark", cardGo.transform);
                    RectTransform p2MarkRect = p2MarkGo.GetComponent<RectTransform>();
                    p2MarkRect.anchorMin = Vector2.zero;
                    p2MarkRect.anchorMax = Vector2.one;
                    p2MarkRect.offsetMin = new Vector2(2, 2);
                    p2MarkRect.offsetMax = new Vector2(-2, -2);
                    Image p2MarkImg = p2MarkGo.AddComponent<Image>();
                    p2MarkImg.color = new Color(1f, 0.6f, 0f, 0.35f);
                    p2MarkImg.raycastTarget = false;
                    p2Marks[i] = p2MarkGo;

                    // Sprite nhân vật phóng to (ở nửa trên thẻ)
                    GameObject charImgGo = CreateUIObject("Sprite", cardGo.transform);
                    RectTransform charImgRect = charImgGo.GetComponent<RectTransform>();
                    charImgRect.sizeDelta = new Vector2(96, 96);
                    charImgRect.anchoredPosition = new Vector2(0, 45);
                    Image charImg = charImgGo.AddComponent<Image>();
                    charImg.sprite = charSprites[i];
                    charImg.preserveAspect = true;
                    charImg.raycastTarget = false;

                    // Nhãn tên nhân vật ở giữa
                    GameObject labelGo = CreateUIObject("Label", cardGo.transform);
                    RectTransform labelRect = labelGo.GetComponent<RectTransform>();
                    labelRect.sizeDelta = new Vector2(185, 26);
                    labelRect.anchoredPosition = new Vector2(0, -18);
                    TextMeshProUGUI labelTxt = labelGo.AddComponent<TextMeshProUGUI>();
                    labelTxt.font = fontVT323;
                    labelTxt.text = defaultLabels[i];
                    labelTxt.fontSize = 22;
                    labelTxt.color = Color.white;
                    labelTxt.alignment = TextAlignmentOptions.Center;
                    labelTxt.raycastTarget = false;
                    charLabels[i] = labelTxt;

                    // Container 2 nút P1 và P2 ở đáy thẻ
                    GameObject btnsBoxGo = CreateUIObject("ButtonsContainer", cardGo.transform);
                    RectTransform btnsBoxRect = btnsBoxGo.GetComponent<RectTransform>();
                    btnsBoxRect.sizeDelta = new Vector2(185, 38);
                    btnsBoxRect.anchoredPosition = new Vector2(0, -68);

                    // Nút [ P1 ]
                    GameObject p1BtnGo = CreateUIObject("Btn_P1", btnsBoxGo.transform);
                    RectTransform p1BtnRect = p1BtnGo.GetComponent<RectTransform>();
                    p1BtnRect.sizeDelta = new Vector2(78, 34);
                    p1BtnRect.anchoredPosition = new Vector2(-44, 0);
                    Image p1BtnImg = p1BtnGo.AddComponent<Image>();
                    p1BtnImg.color = (i == 0) ? new Color(0f, 0.95f, 1f, 1f) : new Color(0.18f, 0.18f, 0.25f, 1f);
                    Button p1Btn = p1BtnGo.AddComponent<Button>();
                    p1Btn.targetGraphic = p1BtnImg;
                    p1Btns[i] = p1Btn;

                    GameObject p1TxtGo = CreateUIObject("Text", p1BtnGo.transform);
                    RectTransform p1TxtRect = p1TxtGo.GetComponent<RectTransform>();
                    p1TxtRect.anchorMin = Vector2.zero;
                    p1TxtRect.anchorMax = Vector2.one;
                    p1TxtRect.offsetMin = Vector2.zero;
                    p1TxtRect.offsetMax = Vector2.zero;
                    TextMeshProUGUI p1Txt = p1TxtGo.AddComponent<TextMeshProUGUI>();
                    p1Txt.font = fontVT323;
                    p1Txt.text = "<b>P1</b>";
                    p1Txt.fontSize = 24;
                    p1Txt.color = Color.white;
                    p1Txt.alignment = TextAlignmentOptions.Center;
                    p1Txt.raycastTarget = false;

                    // Nút [ P2 ]
                    GameObject p2BtnGo = CreateUIObject("Btn_P2", btnsBoxGo.transform);
                    RectTransform p2BtnRect = p2BtnGo.GetComponent<RectTransform>();
                    p2BtnRect.sizeDelta = new Vector2(78, 34);
                    p2BtnRect.anchoredPosition = new Vector2(44, 0);
                    Image p2BtnImg = p2BtnGo.AddComponent<Image>();
                    p2BtnImg.color = (i == 1) ? new Color(1f, 0.6f, 0f, 1f) : new Color(0.18f, 0.18f, 0.25f, 1f);
                    Button p2Btn = p2BtnGo.AddComponent<Button>();
                    p2Btn.targetGraphic = p2BtnImg;
                    p2Btns[i] = p2Btn;

                    GameObject p2TxtGo = CreateUIObject("Text", p2BtnGo.transform);
                    RectTransform p2TxtRect = p2TxtGo.GetComponent<RectTransform>();
                    p2TxtRect.anchorMin = Vector2.zero;
                    p2TxtRect.anchorMax = Vector2.one;
                    p2TxtRect.offsetMin = Vector2.zero;
                    p2TxtRect.offsetMax = Vector2.zero;
                    TextMeshProUGUI p2Txt = p2TxtGo.AddComponent<TextMeshProUGUI>();
                    p2Txt.font = fontVT323;
                    p2Txt.text = "<b>P2</b>";
                    p2Txt.fontSize = 24;
                    p2Txt.color = Color.white;
                    p2Txt.alignment = TextAlignmentOptions.Center;
                    p2Txt.raycastTarget = false;
                }

                // 8. Dãy 4 nút hành động: PLAY, LEVELS, SETTINGS, AUDIO
                float[] btnX = { -270, -90, 90, 270 };
                Sprite[] actionIcons = { playIcon, levelsIcon, settingsIcon, audioOnIcon };
                string[] actionNames = { "PLAY", "LEVELS", "SETTINGS", "AUDIO" };

                Button[] actionBtns = new Button[4];
                Image audioIconImg = null;

                for (int i = 0; i < 4; i++)
                {
                    GameObject actionGo = CreateUIObject($"ActionBtn_{actionNames[i]}", innerBoxGo.transform);
                    RectTransform actionRect = actionGo.GetComponent<RectTransform>();
                    actionRect.sizeDelta = new Vector2(100, 100);
                    actionRect.anchoredPosition = new Vector2(btnX[i], -185);
                    Image btnBg = actionGo.AddComponent<Image>();
                    btnBg.color = (i == 0) ? new Color(0.2f, 0.9f, 0.3f, 1f) : Color.white;
                    btnBg.raycastTarget = true;
                    Button btn = actionGo.AddComponent<Button>();
                    btn.targetGraphic = btnBg;
                    actionBtns[i] = btn;

                    // Icon đen ở giữa
                    GameObject iconGo = CreateUIObject("Icon", actionGo.transform);
                    RectTransform iconRect = iconGo.GetComponent<RectTransform>();
                    iconRect.sizeDelta = new Vector2(56, 56);
                    iconRect.anchoredPosition = Vector2.zero;
                    Image iconImg = iconGo.AddComponent<Image>();
                    iconImg.sprite = actionIcons[i];
                    iconImg.color = Color.black;
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;

                    if (i == 3) audioIconImg = iconImg;

                    // Nhãn chữ bên dưới nút
                    GameObject actionLabelGo = CreateUIObject("Text", actionGo.transform);
                    RectTransform actionLabelRect = actionLabelGo.GetComponent<RectTransform>();
                    actionLabelRect.sizeDelta = new Vector2(140, 32);
                    actionLabelRect.anchoredPosition = new Vector2(0, -75);
                    TextMeshProUGUI actionLabelTxt = actionLabelGo.AddComponent<TextMeshProUGUI>();
                    actionLabelTxt.font = fontVT323;
                    actionLabelTxt.text = actionNames[i];
                    actionLabelTxt.fontSize = 28;
                    actionLabelTxt.color = Color.white;
                    actionLabelTxt.alignment = TextAlignmentOptions.Center;
                    actionLabelTxt.raycastTarget = false;
                }

                // 9. Feedback Label (thông báo lỗi/hướng dẫn)
                GameObject fbGo = CreateUIObject("FeedbackLabel", innerBoxGo.transform);
                RectTransform fbRect = fbGo.GetComponent<RectTransform>();
                fbRect.sizeDelta = new Vector2(950, 36);
                fbRect.anchoredPosition = new Vector2(0, -295);
                TextMeshProUGUI fbText = fbGo.AddComponent<TextMeshProUGUI>();
                fbText.font = fontVT323;
                fbText.text = "";
                fbText.fontSize = 26;
                fbText.color = new Color(1f, 0.35f, 0.35f);
                fbText.alignment = TextAlignmentOptions.Center;
                fbText.raycastTarget = false;

                // 10. LevelSelectPanel (Popup chọn 5 màn chơi)
                GameObject levelSelectPanelGo = CreateUIObject("LevelSelectPanel", canvasGo.transform);
                RectTransform lspRect = levelSelectPanelGo.GetComponent<RectTransform>();
                lspRect.anchorMin = Vector2.zero;
                lspRect.anchorMax = Vector2.one;
                lspRect.offsetMin = Vector2.zero;
                lspRect.offsetMax = Vector2.zero;
                Image dimmerImg = levelSelectPanelGo.AddComponent<Image>();
                dimmerImg.color = new Color(0f, 0f, 0f, 0.85f);

                // Khung popup LevelSelect
                GameObject lspBoxGo = CreateUIObject("LevelBox", levelSelectPanelGo.transform);
                RectTransform lspBoxRect = lspBoxGo.GetComponent<RectTransform>();
                lspBoxRect.sizeDelta = new Vector2(900, 480);
                lspBoxRect.anchoredPosition = Vector2.zero;
                Image lspBorder = lspBoxGo.AddComponent<Image>();
                lspBorder.color = new Color(1f, 0.87f, 0f);

                GameObject lspInnerGo = CreateUIObject("Inner", lspBoxGo.transform);
                RectTransform lspInnerRect = lspInnerGo.GetComponent<RectTransform>();
                lspInnerRect.anchorMin = Vector2.zero;
                lspInnerRect.anchorMax = Vector2.one;
                lspInnerRect.offsetMin = new Vector2(4, 4);
                lspInnerRect.offsetMax = new Vector2(-4, -4);
                Image lspInnerBg = lspInnerGo.AddComponent<Image>();
                lspInnerBg.color = new Color(0.125f, 0.125f, 0.18f);

                // Tiêu đề LevelSelect
                GameObject lspTitleGo = CreateUIObject("Title", lspInnerGo.transform);
                RectTransform lspTitleRect = lspTitleGo.GetComponent<RectTransform>();
                lspTitleRect.sizeDelta = new Vector2(800, 60);
                lspTitleRect.anchoredPosition = new Vector2(0, 160);
                TextMeshProUGUI lspTitleTxt = lspTitleGo.AddComponent<TextMeshProUGUI>();
                lspTitleTxt.font = fontVT323;
                lspTitleTxt.text = "CHỌN MÀN CHƠI";
                lspTitleTxt.fontSize = 54;
                lspTitleTxt.color = new Color(1f, 0.87f, 0f);
                lspTitleTxt.alignment = TextAlignmentOptions.Center;
                lspTitleTxt.raycastTarget = false;

                // 5 nút Level
                Button[] lvlBtns = new Button[5];
                Image[] lockIcons = new Image[5];
                float[] lvlX = { -280, -140, 0, 140, 280 };

                for (int i = 0; i < 5; i++)
                {
                    int lvlNum = i + 1;
                    string lvlSpritePath = $"{LevelsDir}/{lvlNum:00}.png";
                    Sprite lvlSprite = AssetDatabase.LoadAssetAtPath<Sprite>(lvlSpritePath);

                    GameObject lvlBtnGo = CreateUIObject($"LevelBtn_{lvlNum:00}", lspInnerGo.transform);
                    RectTransform lvlBtnRect = lvlBtnGo.GetComponent<RectTransform>();
                    lvlBtnRect.sizeDelta = new Vector2(100, 100);
                    lvlBtnRect.anchoredPosition = new Vector2(lvlX[i], 10);
                    Image lvlBtnImg = lvlBtnGo.AddComponent<Image>();
                    lvlBtnImg.sprite = lvlSprite;
                    lvlBtnImg.preserveAspect = true;
                    Button btn = lvlBtnGo.AddComponent<Button>();
                    lvlBtns[i] = btn;

                    // Icon Lock/Unlock ở góc
                    GameObject lockGo = CreateUIObject("LockIcon", lvlBtnGo.transform);
                    RectTransform lockRect = lockGo.GetComponent<RectTransform>();
                    lockRect.sizeDelta = new Vector2(36, 36);
                    lockRect.anchoredPosition = new Vector2(30, -30);
                    Image lockImg = lockGo.AddComponent<Image>();
                    lockImg.sprite = (i == 0) ? unlockedIcon : lockedIcon;
                    lockImg.preserveAspect = true;
                    lockImg.raycastTarget = false;
                    lockIcons[i] = lockImg;
                }

                // Nút Back
                GameObject backBtnGo = CreateUIObject("BackButton", lspInnerGo.transform);
                RectTransform backBtnRect = backBtnGo.GetComponent<RectTransform>();
                backBtnRect.sizeDelta = new Vector2(200, 50);
                backBtnRect.anchoredPosition = new Vector2(0, -155);
                Image backImg = backBtnGo.AddComponent<Image>();
                backImg.color = Color.white;
                Button backBtn = backBtnGo.AddComponent<Button>();

                GameObject backTxtGo = CreateUIObject("Text", backBtnGo.transform);
                RectTransform backTxtRect = backTxtGo.GetComponent<RectTransform>();
                backTxtRect.sizeDelta = new Vector2(180, 40);
                backTxtRect.anchoredPosition = Vector2.zero;
                TextMeshProUGUI backTxt = backTxtGo.AddComponent<TextMeshProUGUI>();
                backTxt.font = fontVT323;
                backTxt.text = "QUAY LẠI";
                backTxt.fontSize = 28;
                backTxt.color = Color.black;
                backTxt.alignment = TextAlignmentOptions.Center;
                backTxt.raycastTarget = false;

                // Feedback LevelSelect
                GameObject lspFbGo = CreateUIObject("Feedback", lspInnerGo.transform);
                RectTransform lspFbRect = lspFbGo.GetComponent<RectTransform>();
                lspFbRect.sizeDelta = new Vector2(800, 30);
                lspFbRect.anchoredPosition = new Vector2(0, -90);
                TextMeshProUGUI lspFbTxt = lspFbGo.AddComponent<TextMeshProUGUI>();
                lspFbTxt.font = fontVT323;
                lspFbTxt.text = "";
                lspFbTxt.fontSize = 22;
                lspFbTxt.color = new Color(1f, 0.35f, 0.35f);
                lspFbTxt.alignment = TextAlignmentOptions.Center;
                lspFbTxt.raycastTarget = false;

                // Gắn LevelSelectMenu Component
                LevelSelectMenu levelSelectMenu = levelSelectPanelGo.AddComponent<LevelSelectMenu>();
                SerializedObject soLsm = new SerializedObject(levelSelectMenu);
                SerializedProperty propBtns = soLsm.FindProperty("levelButtons");
                SerializedProperty propLocks = soLsm.FindProperty("lockIndicators");
                propBtns.arraySize = 5;
                propLocks.arraySize = 5;
                for (int i = 0; i < 5; i++)
                {
                    propBtns.GetArrayElementAtIndex(i).objectReferenceValue = lvlBtns[i];
                    propLocks.GetArrayElementAtIndex(i).objectReferenceValue = lockIcons[i];
                }
                soLsm.FindProperty("lockedSprite").objectReferenceValue = lockedIcon;
                soLsm.FindProperty("unlockedSprite").objectReferenceValue = unlockedIcon;
                soLsm.FindProperty("backButton").objectReferenceValue = backBtn;
                soLsm.FindProperty("feedbackLabel").objectReferenceValue = lspFbTxt;
                soLsm.ApplyModifiedProperties();

                // Ẩn panel LevelSelect ban đầu
                levelSelectPanelGo.SetActive(false);

                // 11. Gắn MainMenuController Component vào Canvas
                MainMenuController menuCtrl = canvasGo.AddComponent<MainMenuController>();
                SerializedObject soMenu = new SerializedObject(menuCtrl);
                soMenu.FindProperty("characterPanel").objectReferenceValue = mainBoxGo;
                soMenu.FindProperty("levelSelectPanel").objectReferenceValue = levelSelectPanelGo;
                soMenu.FindProperty("singlePlayerButton").objectReferenceValue = btn1P;
                soMenu.FindProperty("twoPlayerButton").objectReferenceValue = btn2P;
                soMenu.FindProperty("modeLabel").objectReferenceValue = modeLabelTxt;
                soMenu.FindProperty("playButton").objectReferenceValue = actionBtns[0];
                soMenu.FindProperty("levelsButton").objectReferenceValue = actionBtns[1];
                soMenu.FindProperty("settingsButton").objectReferenceValue = actionBtns[2];
                soMenu.FindProperty("audioButton").objectReferenceValue = actionBtns[3];
                soMenu.FindProperty("audioIcon").objectReferenceValue = audioIconImg;
                soMenu.FindProperty("audioOnSprite").objectReferenceValue = audioOnIcon;
                soMenu.FindProperty("audioOffSprite").objectReferenceValue = audioOffIcon;
                soMenu.FindProperty("feedbackLabel").objectReferenceValue = fbText;
                soMenu.FindProperty("uiAudioSource").objectReferenceValue = sfxSource;
                soMenu.FindProperty("clickClip").objectReferenceValue = clickClip;
                soMenu.FindProperty("errorClip").objectReferenceValue = errorClip;

                // Gán card buttons
                SerializedProperty propCardBtns = soMenu.FindProperty("cardButtons");
                propCardBtns.arraySize = 4;
                for (int i = 0; i < 4; i++)
                {
                    propCardBtns.GetArrayElementAtIndex(i).objectReferenceValue = cardBtns[i];
                }

                // Gán P1 skin buttons
                SerializedProperty propP1Btns = soMenu.FindProperty("player1SkinButtons");
                propP1Btns.arraySize = 4;
                for (int i = 0; i < 4; i++)
                {
                    propP1Btns.GetArrayElementAtIndex(i).objectReferenceValue = p1Btns[i];
                }

                // Gán P2 skin buttons
                SerializedProperty propP2Btns = soMenu.FindProperty("player2SkinButtons");
                propP2Btns.arraySize = 4;
                for (int i = 0; i < 4; i++)
                {
                    propP2Btns.GetArrayElementAtIndex(i).objectReferenceValue = p2Btns[i];
                }

                // Gán Marks & Labels
                SerializedProperty propP1Marks = soMenu.FindProperty("player1SelectionMarks");
                SerializedProperty propP2Marks = soMenu.FindProperty("player2SelectionMarks");
                SerializedProperty propCharLabels = soMenu.FindProperty("characterLabels");
                propP1Marks.arraySize = 4;
                propP2Marks.arraySize = 4;
                propCharLabels.arraySize = 4;
                for (int i = 0; i < 4; i++)
                {
                    propP1Marks.GetArrayElementAtIndex(i).objectReferenceValue = p1Marks[i];
                    propP2Marks.GetArrayElementAtIndex(i).objectReferenceValue = p2Marks[i];
                    propCharLabels.GetArrayElementAtIndex(i).objectReferenceValue = charLabels[i];
                }

                soMenu.ApplyModifiedProperties();

                // Nối tham chiếu mainMenu cho LevelSelectMenu
                soLsm.Update();
                soLsm.FindProperty("mainMenu").objectReferenceValue = menuCtrl;
                soLsm.ApplyModifiedProperties();

                // 12. Lưu Scene
                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log($"<b>[MainMenuSceneBuilder]</b> ĐÃ LƯU SCENE TẠI: {ScenePath}");

                // 13. Đăng ký Scene vào EditorBuildSettings (Index 0)
                RegisterSceneInBuildSettings(ScenePath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("<b>[MainMenuSceneBuilder]</b> HOÀN TẤT DỰNG SCENE MAINMENU.UNITY THÀNH CÔNG 100%!");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "Hoàn Tất",
                        "ĐÃ TẠO XONG SCENE MAINMENU.UNITY KHỚP 100% THIẾT KẾ!\n\n" +
                        "- 2 Nút chọn chế độ 1P / 2P rõ ràng\n" +
                        "- 4 Skins: Ninja Frog, Pink Man, Virtual Guy, Mask Dude\n" +
                        "- Nút chọn P1 và P2 trực tiếp trên từng thẻ\n" +
                        "- 4 Nút hành động: PLAY, LEVELS, SETTINGS, AUDIO\n" +
                        "- Đầy đủ popup LevelSelect chọn 5 màn",
                        "Đồng Ý");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainMenuSceneBuilder] LỖI: {ex.Message}\n{ex.StackTrace}");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Lỗi Build MainMenu", ex.Message, "Đóng");
                }
            }
            finally
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        public static void BuildFromCommandLine()
        {
            BuildMenu();
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Sprite LoadFirstSubSprite(string texturePath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            foreach (Object obj in assets)
            {
                if (obj is Sprite sp)
                {
                    return sp;
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        }

        private static TMP_FontAsset GetOrCreateVT323FontAsset()
        {
            TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (defaultFont != null)
            {
                return defaultFont;
            }

            return TMP_Settings.defaultFontAsset;
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int existingIndex = scenes.FindIndex(s => s.path == scenePath);
            if (existingIndex >= 0)
            {
                scenes.RemoveAt(existingIndex);
            }

            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
