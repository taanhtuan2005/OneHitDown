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
    /// 4 nút hành động (PLAY, LEVELS, SETTINGS, AUDIO) và popup LevelSelect.
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
                EditorUtility.DisplayProgressBar("One Hit Down", "Đang dựng Scene MainMenu...", 0.1f);
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
                    LoadFirstSubSprite(PinkManPath),
                    LoadFirstSubSprite(VirtualGuyPath),
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
                mainBoxRect.sizeDelta = new Vector2(1040, 700);
                mainBoxRect.anchoredPosition = Vector2.zero;

                // Viền vàng ngoài (#FFDE00)
                Image borderImg = mainBoxGo.AddComponent<Image>();
                borderImg.color = new Color(1f, 0.87f, 0f); // #FFDE00

                // Lõi xám than trong (#20202E)
                GameObject innerBoxGo = CreateUIObject("InnerBox", mainBoxGo.transform);
                RectTransform innerRect = innerBoxGo.GetComponent<RectTransform>();
                innerRect.anchorMin = Vector2.zero;
                innerRect.anchorMax = Vector2.one;
                innerRect.offsetMin = new Vector2(4, 4);
                innerRect.offsetMax = new Vector2(-4, -4);
                Image innerImg = innerBoxGo.AddComponent<Image>();
                innerImg.color = new Color(0.125f, 0.125f, 0.18f); // #20202E

                // 5. Tiêu đề "ONE HIT DOWN"
                GameObject titleGo = CreateUIObject("Title", innerBoxGo.transform);
                RectTransform titleRect = titleGo.GetComponent<RectTransform>();
                titleRect.sizeDelta = new Vector2(900, 80);
                titleRect.anchoredPosition = new Vector2(0, 260);
                TextMeshProUGUI titleText = titleGo.AddComponent<TextMeshProUGUI>();
                titleText.font = fontVT323;
                titleText.text = "ONE HIT DOWN";
                titleText.fontSize = 72;
                titleText.color = new Color(1f, 0.87f, 0f); // #FFDE00
                titleText.alignment = TextAlignmentOptions.Center;

                // 6. Nút chọn chế độ: "CHỌN CHẾ ĐỘ: [1 PLAYER] / [2 PLAYERS - CO-OP TETHER]"
                GameObject modeBtnGo = CreateUIObject("ModeToggleButton", innerBoxGo.transform);
                RectTransform modeBtnRect = modeBtnGo.GetComponent<RectTransform>();
                modeBtnRect.sizeDelta = new Vector2(850, 44);
                modeBtnRect.anchoredPosition = new Vector2(0, 185);
                Button modeBtn = modeBtnGo.AddComponent<Button>();
                TextMeshProUGUI modeText = modeBtnGo.AddComponent<TextMeshProUGUI>();
                modeText.font = fontVT323;
                modeText.text = "CHỌN CHẾ ĐỘ: <color=#00FFFF>[1 PLAYER]</color> / 2 PLAYERS - CO-OP TETHER";
                modeText.fontSize = 32;
                modeText.color = new Color(0f, 0.95f, 1f); // #00FFFF
                modeText.alignment = TextAlignmentOptions.Center;

                // 7. Dãy 4 nhân vật
                float[] posX = { -300, -100, 100, 300 };
                string[] defaultLabels = { "P1: NINJA FROG", "P2: PINK MAN", "VIRTUAL GUY", "MASK DUDE" };

                Button[] p1Buttons = new Button[4];
                Button[] p2Buttons = new Button[4];
                GameObject[] p1Marks = new GameObject[4];
                GameObject[] p2Marks = new GameObject[4];
                TMP_Text[] charLabels = new TMP_Text[4];

                for (int i = 0; i < 4; i++)
                {
                    GameObject slotGo = CreateUIObject($"CharacterCard_{i}", innerBoxGo.transform);
                    RectTransform slotRect = slotGo.GetComponent<RectTransform>();
                    slotRect.sizeDelta = new Vector2(160, 180);
                    slotRect.anchoredPosition = new Vector2(posX[i], 35);
                    Button cardBtn = slotGo.AddComponent<Button>();

                    // Viền P1 (Cyan)
                    GameObject p1MarkGo = CreateUIObject("P1_Mark", slotGo.transform);
                    RectTransform p1MarkRect = p1MarkGo.GetComponent<RectTransform>();
                    p1MarkRect.sizeDelta = new Vector2(110, 110);
                    p1MarkRect.anchoredPosition = new Vector2(0, 20);
                    Image p1MarkImg = p1MarkGo.AddComponent<Image>();
                    p1MarkImg.color = new Color(0f, 1f, 1f, 0.35f);
                    p1Marks[i] = p1MarkGo;

                    // Viền P2 (Orange)
                    GameObject p2MarkGo = CreateUIObject("P2_Mark", slotGo.transform);
                    RectTransform p2MarkRect = p2MarkGo.GetComponent<RectTransform>();
                    p2MarkRect.sizeDelta = new Vector2(110, 110);
                    p2MarkRect.anchoredPosition = new Vector2(0, 20);
                    Image p2MarkImg = p2MarkGo.AddComponent<Image>();
                    p2MarkImg.color = new Color(1f, 0.6f, 0f, 0.35f);
                    p2Marks[i] = p2MarkGo;

                    // Sprite nhân vật phóng to
                    GameObject charImgGo = CreateUIObject("Sprite", slotGo.transform);
                    RectTransform charImgRect = charImgGo.GetComponent<RectTransform>();
                    charImgRect.sizeDelta = new Vector2(96, 96);
                    charImgRect.anchoredPosition = new Vector2(0, 20);
                    Image charImg = charImgGo.AddComponent<Image>();
                    charImg.sprite = charSprites[i];
                    charImg.preserveAspect = true;

                    // Nhãn text bên dưới
                    GameObject labelGo = CreateUIObject("Label", slotGo.transform);
                    RectTransform labelRect = labelGo.GetComponent<RectTransform>();
                    labelRect.sizeDelta = new Vector2(180, 36);
                    labelRect.anchoredPosition = new Vector2(0, -60);
                    TextMeshProUGUI labelTxt = labelGo.AddComponent<TextMeshProUGUI>();
                    labelTxt.font = fontVT323;
                    labelTxt.text = defaultLabels[i];
                    labelTxt.fontSize = 24;
                    labelTxt.color = (i == 0 || i == 1) ? Color.white : new Color(0.7f, 0.7f, 0.8f);
                    labelTxt.alignment = TextAlignmentOptions.Center;
                    charLabels[i] = labelTxt;

                    p1Buttons[i] = cardBtn;
                    p2Buttons[i] = cardBtn;
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
                    actionRect.anchoredPosition = new Vector2(btnX[i], -175);
                    Image btnBg = actionGo.AddComponent<Image>();
                    btnBg.color = Color.white;
                    Button btn = actionGo.AddComponent<Button>();
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
                }

                // 9. Feedback Label (thông báo lỗi/hướng dẫn)
                GameObject fbGo = CreateUIObject("FeedbackLabel", innerBoxGo.transform);
                RectTransform fbRect = fbGo.GetComponent<RectTransform>();
                fbRect.sizeDelta = new Vector2(900, 36);
                fbRect.anchoredPosition = new Vector2(0, -280);
                TextMeshProUGUI fbText = fbGo.AddComponent<TextMeshProUGUI>();
                fbText.font = fontVT323;
                fbText.text = "";
                fbText.fontSize = 26;
                fbText.color = new Color(1f, 0.35f, 0.35f);
                fbText.alignment = TextAlignmentOptions.Center;

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
                soMenu.FindProperty("modeToggleButton").objectReferenceValue = modeBtn;
                soMenu.FindProperty("modeLabel").objectReferenceValue = modeText;
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

                // Nối hàm click chọn card nhân vật
                for (int i = 0; i < 4; i++)
                {
                    int skinIdx = i;
                    p1Buttons[i].onClick.AddListener(() => menuCtrl.OnCharacterCardClicked(skinIdx));
                }

                // 12. Lưu Scene
                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log($"<b>[MainMenuSceneBuilder]</b> ĐÃ LƯU SCENE TẠI: {ScenePath}");

                // 13. Đăng ký Scene vào EditorBuildSettings (Index 0)
                RegisterSceneInBuildSettings(ScenePath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("<b>[MainMenuSceneBuilder]</b> HOÀN TẤT DỰNG SCENE MAINMENU.UNITY THÀNH CÔNG 100%!");
                EditorUtility.DisplayDialog(
                    "Hoàn Tất",
                    "ĐÃ TẠO XONG SCENE MAINMENU.UNITY KHỚP 100% THIẾT KẾ!\n\n" +
                    "- Nền caro tím Purple.png\n" +
                    "- Khung viền vàng #FFDE00\n" +
                    "- 4 Skins: Ninja Frog, Pink Man, Virtual Guy, Mask Dude\n" +
                    "- 4 Nút: PLAY, LEVELS, SETTINGS, AUDIO\n" +
                    "- Đầy đủ popup LevelSelect chọn 5 màn",
                    "Đồng Ý");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainMenuSceneBuilder] LỖI: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("Lỗi Build MainMenu", ex.Message, "Đóng");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
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
            const string vt323AssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/VT323-Regular SDF.asset";
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(vt323AssetPath);
            if (fontAsset != null)
            {
                return fontAsset;
            }

            Font ttf = AssetDatabase.LoadAssetAtPath<Font>(FontVT323Path);
            if (ttf != null)
            {
                fontAsset = TMP_FontAsset.CreateFontAsset(ttf);
                if (fontAsset != null)
                {
                    AssetDatabase.CreateAsset(fontAsset, vt323AssetPath);
                    AssetDatabase.SaveAssets();
                    return fontAsset;
                }
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

