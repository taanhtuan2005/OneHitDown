// ====================================================================================================
// SCRIPT UNITY EDITOR: ĐÓNG GÓI RIG MODULE 5 THÀNH 'Assets/Prefabs/CoreRig.prefab'
// Người 5 — Hoàng Thị Giang Uyên (2351060497) — Module 5.
// Tuân thủ 100%: Docs/SCHEMA_CHI_TIET.md §3 Module 5 + §4 Bảng Prefab + §5 Quy tắc Scene.
//                 Docs/Phan_cong.md Mục III (Người 5) + CLAUDE.md Mục 7 (Prefab-First).
//
// MỤC ĐÍCH:
//   Đóng gói toàn bộ hạ tầng dùng chung của Module 5 vào MỘT prefab duy nhất để các thành viên
//   khác (Người 1/2/4) chỉ cần thả prefab vào Scene của mình thay vì phải sửa Scene người khác:
//     Canvas (Screen Space Overlay) + EventSystem (InputSystemUIInputModule)
//     + HUDController + PauseMenu + IrisTransition + GameManager + AudioManager
//
//   Ba tham chiếu Scene (startPoint · exitGate · mainCamera) KHÔNG thể nằm trong prefab vì
//   chúng thuộc từng màn. Chúng được nối tự động bởi InstantiateCoreRig(...) hoặc bởi menu
//   'Tools -> One Hit Down -> Attach CoreRig to Current Scene'.
//   Nếu quên nối: GameManager báo lỗi rõ ràng, còn CameraController tự tìm Player (fallback).
//
// Menu Editor: Tools -> One Hit Down -> Build CoreRig Prefab
//              Tools -> One Hit Down -> Attach CoreRig to Current Scene
//              Tools -> One Hit Down -> Validate CoreRig Prefab
// Batch mode : -executeMethod OneHitDown.EditorTools.CoreRigBuilder.BuildFromCommandLine
// ====================================================================================================

#if UNITY_EDITOR
using System;
using System.IO;
using OneHitDown.Core;
using OneHitDown.Level;
using OneHitDown.Player;
using OneHitDown.UI;
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
    /// Sinh và quản lý <c>Assets/Prefabs/CoreRig.prefab</c> — prefab tự chứa toàn bộ hạ tầng
    /// Module 5 (Canvas, EventSystem, HUD, PauseMenu, IrisTransition, GameManager, AudioManager).
    /// </summary>
    public static class CoreRigBuilder
    {
        #region Hằng số đường dẫn

        public const string CoreRigPrefabPath = "Assets/Prefabs/CoreRig.prefab";
        public const string CoreRigPrefabFolder = "Assets/Prefabs";

        private const string AudioManagerPrefabPath = "Assets/Prefabs/AudioManager.prefab";
        private const string Player1PrefabPath = "Assets/Prefabs/Player1.prefab";
        private const string Player2PrefabPath = "Assets/Prefabs/Player2.prefab";
        private const string SoulTetherPrefabPath = "Assets/Prefabs/SoulTether.prefab";

        private const string PineappleTexturePath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Fruits/Pineapple.png";
        private const string TransitionTexturePath = "Assets/Art/07_HieuUng/PA1_Other/Transition.png";
        private const string FontPath = "Assets/Art/08_Font/VT323/VT323-Regular.ttf";
        private const string RestartButtonTexturePath = "Assets/Art/05_GiaoDien/PA1_Menu/Buttons/Restart.png";
        private const string PlayButtonTexturePath = "Assets/Art/05_GiaoDien/PA1_Menu/Buttons/Play.png";
        private const string SettingsButtonTexturePath = "Assets/Art/05_GiaoDien/PA1_Menu/Buttons/Settings.png";
        private const string PauseIconTexturePath = "Assets/Art/05_GiaoDien/Kenney_GameIcons/UI/PNG/White/2x/pause.png";

        private const string HudRootName = "HUD";
        private const string PauseRootName = "PauseMenu";
        private const string IrisRootName = "IrisTransition";
        private const string CanvasName = "Canvas";
        private const string EventSystemName = "EventSystem";
        private const string GameManagerName = "GameManager";
        private const string AudioManagerName = "AudioManager";

        #endregion

        #region Sprite & Font cache

        private static Sprite pineappleSprite;
        private static Sprite transitionSprite;
        private static Sprite restartIconSprite;
        private static Sprite playIconSprite;
        private static Sprite settingsIconSprite;
        private static Sprite pauseIconSprite;
        private static Font hudFont;

        #endregion

        #region Bộ tham chiếu trả về

        /// <summary>
        /// Các component gốc của CoreRig sau khi thả vào Scene, dùng để nối tiếp
        /// ba tham chiếu Scene (startPoint · exitGate · mainCamera).
        /// </summary>
        public struct CoreRigRefs
        {
            public GameObject Root;
            public GameManager GameManager;
            public AudioManager AudioManager;
            public HUDController Hud;
            public PauseMenu Pause;
            public IrisTransition Iris;
            public Canvas Canvas;
            public EventSystem EventSystem;
        }

        #endregion

        #region Menu Editor

        [MenuItem("Tools/One Hit Down/Build CoreRig Prefab", false, 34)]
        public static void BuildCoreRigMenu()
        {
            BuildCoreRigPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/One Hit Down/Attach CoreRig to Current Scene", false, 35)]
        public static void AttachToCurrentSceneMenu()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                Debug.LogError("[CoreRigBuilder] Không có Scene nào đang mở.");
                return;
            }

            if (!File.Exists(CoreRigPrefabPath))
            {
                Debug.Log("<b>[CoreRigBuilder]</b> Chưa có CoreRig.prefab — đang sinh mới...");
                BuildCoreRigPrefab();
            }

            LevelStartPoint startPoint = FindInScene<LevelStartPoint>(activeScene);
            ExitGate exitGate = FindInScene<ExitGate>(activeScene);
            CameraController cameraController = FindInScene<CameraController>(activeScene);

            if (startPoint == null)
            {
                Debug.LogWarning("[CoreRigBuilder] Scene chưa có LevelStartPoint — GameManager sẽ không sinh được Player. Hãy thả 'Assets/Prefabs/LevelStart.prefab' trước.");
            }

            if (exitGate == null)
            {
                Debug.LogWarning("[CoreRigBuilder] Scene chưa có ExitGate — không có Cổng Thần để kết thúc màn.");
            }

            if (cameraController == null)
            {
                Debug.LogWarning("[CoreRigBuilder] Scene chưa có CameraController — HUD vẫn chạy nhưng camera sẽ không bám Player.");
            }

            GameObject existing = GameObject.Find("CoreRig");
            if (existing != null)
            {
                Debug.LogWarning("[CoreRigBuilder] Scene đã có 'CoreRig' — bỏ qua để tránh trùng GameManager.");
                return;
            }

            CoreRigRefs refs = InstantiateCoreRig(null, startPoint, exitGate, cameraController, 0);

            if (refs.Root != null)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                Debug.Log($"<b>[CoreRigBuilder]</b> Đã thả CoreRig vào Scene '{activeScene.name}'. Nhớ lưu Scene (Ctrl+S).");
            }
        }

        [MenuItem("Tools/One Hit Down/Validate CoreRig Prefab", false, 36)]
        public static void ValidateCoreRigMenu()
        {
            ValidateCoreRig();
        }

        /// <summary>
        /// Tự sinh prefab khi mở dự án mà chưa có — cùng quy ước với Level03SceneBuilder /
        /// Module5SceneBuilder. Bỏ qua ở batchmode để không dựng chồng khi chạy CI.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnProjectLoaded()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            if (File.Exists(CoreRigPrefabPath))
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (File.Exists(CoreRigPrefabPath))
                {
                    return;
                }

                Debug.Log("<b>[CoreRigBuilder]</b> Chưa có CoreRig.prefab — tự dựng lần đầu...");
                BuildCoreRigPrefab();
                ValidateCoreRig();
                AssetDatabase.SaveAssets();
            };
        }

        /// <summary>Entry point cho <c>-executeMethod</c> khi chạy batchmode.</summary>
        public static void BuildFromCommandLine()
        {
            try
            {
                BuildCoreRigPrefab();
                ValidateCoreRig();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"<b>[CoreRigBuilder]</b> HOÀN TẤT: <color=green>{CoreRigPrefabPath}</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CoreRigBuilder] Lỗi sinh CoreRig.prefab: {ex}");
                throw;
            }
        }

        #endregion

        #region Sinh CoreRig.prefab

        /// <summary>
        /// Dựng cây CoreRig trong một Scene tạm rồi lưu thành prefab.
        /// Cây: CoreRig → [GameManager, AudioManager, Canvas(HUD/PauseMenu/IrisTransition), EventSystem].
        /// </summary>
        public static void BuildCoreRigPrefab()
        {
            LoadSharedAssets();

            if (hudFont == null)
            {
                Debug.LogError($"[CoreRigBuilder] Không tìm thấy font HUD tại {FontPath}. Hủy sinh prefab.");
                return;
            }

            // Dựng trong Scene tạm để PrefabUtility lưu được mà KHÔNG đụng Scene người dùng đang mở.
            // NewScene(Additive) không đổi active scene, nên phải MoveGameObjectToScene để
            // CoreRig không rơi vào Scene của người dùng (tránh làm bẩn Scene chưa lưu).
            Scene tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            GameObject root = new GameObject("CoreRig");
            SceneManager.MoveGameObjectToScene(root, tempScene);

            try
            {
                EditorUtility.DisplayProgressBar("CoreRig - Build Prefab", "Dựng Canvas / EventSystem...", 0.15f);
                CoreRigRefs refs = BuildCoreRigHierarchy(root.transform);

                EditorUtility.DisplayProgressBar("CoreRig - Build Prefab", "Nối tham chiếu nội bộ...", 0.75f);
                WireInternalReferences(refs);

                EditorUtility.DisplayProgressBar("CoreRig - Build Prefab", "Lưu prefab...", 0.9f);
                EnsureDirectory(CoreRigPrefabFolder);

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, CoreRigPrefabPath);
                if (saved == null)
                {
                    Debug.LogError($"[CoreRigBuilder] Không lưu được prefab tại {CoreRigPrefabPath}.");
                    return;
                }

                Debug.Log($"<b>[CoreRigBuilder]</b> ĐÃ SINH PREFAB: <color=green>{CoreRigPrefabPath}</color>");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                UnityEngine.Object.DestroyImmediate(root);

                if (tempScene.IsValid())
                {
                    EditorSceneManager.CloseScene(tempScene, true);
                }
            }
        }

        /// <summary>Dựng cây con của CoreRig dưới <paramref name="root"/>.</summary>
        private static CoreRigRefs BuildCoreRigHierarchy(Transform root)
        {
            CoreRigRefs refs = new CoreRigRefs();
            refs.Root = root.gameObject;

            // --- AudioManager (prefab có sẵn của Module 5) ---
            GameObject audioGo = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(AudioManagerPrefabPath), root) as GameObject;
            if (audioGo != null)
            {
                audioGo.name = AudioManagerName;
                audioGo.transform.localPosition = Vector3.zero;
                refs.AudioManager = audioGo.GetComponent<AudioManager>();
            }
            else
            {
                Debug.LogError($"[CoreRigBuilder] Không nạp được {AudioManagerPrefabPath}.");
            }

            // --- Canvas ---
            GameObject canvasGo = new GameObject(CanvasName, typeof(RectTransform));
            canvasGo.transform.SetParent(root, false);
            canvasGo.layer = UiLayer;

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            refs.Canvas = canvas;

            // --- HUD ---
            GameObject hudGo = CreateUiRoot(HudRootName, canvasGo.transform);
            HUDController hud = hudGo.AddComponent<HUDController>();

            Image relicIcon = CreateUiImage("RelicIcon", hudGo.transform, pineappleSprite, true);
            PlaceTopLeft((RectTransform)relicIcon.transform, 40f, 40f, 72f, 72f);

            Text relicCounter = CreateUiText("RelicCounter", hudGo.transform, 48, TextAnchor.MiddleLeft, new Color(1f, 0.95f, 0.8f, 1f), "[0/3]");
            PlaceTopLeft((RectTransform)relicCounter.transform, 124f, 46f, 220f, 60f);

            Text levelText = CreateUiText("LevelText", hudGo.transform, 44, TextAnchor.MiddleRight, new Color(1f, 0.95f, 0.8f, 1f), "LEVEL 00");
            PlaceTopRight((RectTransform)levelText.transform, 40f, 40f, 320f, 52f);

            Text timeText = CreateUiText("TimeText", hudGo.transform, 44, TextAnchor.MiddleRight, new Color(1f, 0.95f, 0.8f, 1f), "TIME 00:00");
            PlaceTopRight((RectTransform)timeText.transform, 40f, 96f, 320f, 52f);

            Text modeText = CreateUiText("ModeText", hudGo.transform, 44, TextAnchor.MiddleRight, new Color(0.6f, 0.95f, 1f, 1f), "[1P]");
            PlaceTopRight((RectTransform)modeText.transform, 40f, 152f, 320f, 52f);

            Text deathsText = CreateUiText("DeathsText", hudGo.transform, 44, TextAnchor.MiddleRight, new Color(1f, 0.55f, 0.55f, 1f), "DEATHS 00");
            PlaceTopRight((RectTransform)deathsText.transform, 40f, 208f, 320f, 52f);

            Button restartButton = CreateIconButton("RestartButton", hudGo.transform, restartIconSprite);
            PlaceTopRight((RectTransform)restartButton.transform, 40f, 268f, 56f, 56f);

            Button pauseButton = CreateIconButton("PauseButton", hudGo.transform, pauseIconSprite);
            PlaceTopRight((RectTransform)pauseButton.transform, 108f, 268f, 56f, 56f);

            SerializedObject hudSerialized = new SerializedObject(hud);
            SetObject(hudSerialized, "relicIcon", relicIcon);
            SetObject(hudSerialized, "relicCounterText", relicCounter);
            SetObject(hudSerialized, "levelText", levelText);
            SetObject(hudSerialized, "timeText", timeText);
            SetObject(hudSerialized, "modeText", modeText);
            SetObject(hudSerialized, "deathsText", deathsText);
            SetObject(hudSerialized, "restartButton", restartButton);
            SetObject(hudSerialized, "pauseButton", pauseButton);
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();
            refs.Hud = hud;

            // --- PauseMenu (component ở root, panel là CON để SetActive(false) không tắt script) ---
            GameObject pauseRootGo = CreateUiRoot(PauseRootName, canvasGo.transform);
            PauseMenu pauseMenu = pauseRootGo.AddComponent<PauseMenu>();

            GameObject pausePanelGo = CreateUiRoot("PausePanel", pauseRootGo.transform);
            Image pauseBackdrop = pausePanelGo.AddComponent<Image>();
            pauseBackdrop.color = new Color(0.03f, 0.02f, 0.07f, 0.82f);
            pauseBackdrop.raycastTarget = true;

            Text pauseTitle = CreateUiText("PauseTitle", pausePanelGo.transform, 96, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.35f, 1f), "TẠM DỪNG");
            CenterAt((RectTransform)pauseTitle.transform, 0f, 250f, 900f, 120f);

            Button resumeButton = CreateSolidButton("ResumeButton", pausePanelGo.transform, playIconSprite, "TIẾP TỤC", 60f);
            Button pauseRestartButton = CreateSolidButton("PauseRestartButton", pausePanelGo.transform, restartIconSprite, "CHƠI LẠI", -40f);
            Button mainMenuButton = CreateSolidButton("MainMenuButton", pausePanelGo.transform, settingsIconSprite, "VỀ MENU CHÍNH", -140f);

            SerializedObject pauseSerialized = new SerializedObject(pauseMenu);
            SetObject(pauseSerialized, "pausePanel", pausePanelGo);
            SetObject(pauseSerialized, "resumeButton", resumeButton);
            SetObject(pauseSerialized, "restartButton", pauseRestartButton);
            SetObject(pauseSerialized, "mainMenuButton", mainMenuButton);
            pauseSerialized.ApplyModifiedPropertiesWithoutUndo();
            // Khớp trạng thái lúc chơi: Awake() của PauseMenu ẩn panel ngay, nên prefab cũng để ẩn.
            pausePanelGo.SetActive(false);
            refs.Pause = pauseMenu;

            // --- IrisTransition (component ở root, Image là CON vì HideIris() tắt chính GameObject của Image) ---
            GameObject irisRootGo = CreateUiRoot(IrisRootName, canvasGo.transform);
            IrisTransition iris = irisRootGo.AddComponent<IrisTransition>();

            Image irisImage = CreateUiImage("IrisMask", irisRootGo.transform, transitionSprite, false);
            CenterAt((RectTransform)irisImage.transform, 0f, 0f, 44f, 44f);

            SerializedObject irisSerialized = new SerializedObject(iris);
            SetObject(irisSerialized, "irisImage", irisImage);
            SetFloat(irisSerialized, "closeDuration", 0.35f);
            SetFloat(irisSerialized, "openDuration", 0.35f);
            SetFloat(irisSerialized, "overshoot", 1.6f);
            irisSerialized.ApplyModifiedPropertiesWithoutUndo();
            // Khớp trạng thái lúc chơi: Awake()/OnDisable() của IrisTransition gọi HideIris().
            irisImage.gameObject.SetActive(false);
            refs.Iris = iris;

            // --- EventSystem (chỉ New Input System: ProjectSettings.activeInputHandler = 1) ---
            GameObject eventSystemGo = new GameObject(EventSystemName);
            eventSystemGo.transform.SetParent(root, false);
            EventSystem eventSystem = eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
            refs.EventSystem = eventSystem;

            // --- GameManager ---
            GameObject managerGo = new GameObject(GameManagerName);
            managerGo.transform.SetParent(root, false);
            GameManager manager = managerGo.AddComponent<GameManager>();
            refs.GameManager = manager;

            return refs;
        }

        /// <summary>
        /// Nối các tham chiếu NỘI BỘ prefab (UI ↔ GameManager ↔ AudioManager).
        /// Ba tham chiếu Scene (startPoint · exitGate · mainCamera) để trống, nối sau khi thả vào Scene.
        /// </summary>
        private static void WireInternalReferences(CoreRigRefs refs)
        {
            if (refs.Hud != null && refs.Pause != null)
            {
                SerializedObject hudSerialized = new SerializedObject(refs.Hud);
                SetObject(hudSerialized, "pauseMenu", refs.Pause);
                hudSerialized.ApplyModifiedPropertiesWithoutUndo();
            }

            if (refs.GameManager == null)
            {
                Debug.LogError("[CoreRigBuilder] Thiếu GameManager — không nối được tham chiếu.");
                return;
            }

            GameObject player1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Player1PrefabPath);
            GameObject player2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Player2PrefabPath);
            GameObject soulTetherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SoulTetherPrefabPath);

            SerializedObject serialized = new SerializedObject(refs.GameManager);
            SetObject(serialized, "hudController", refs.Hud);
            SetObject(serialized, "pauseMenu", refs.Pause);
            SetObject(serialized, "irisTransition", refs.Iris);
            SetObject(serialized, "audioManager", refs.AudioManager);
            SetObject(serialized, "player1Prefab", player1Prefab != null ? player1Prefab.GetComponent<PlayerController>() : null);
            SetObject(serialized, "player2Prefab", player2Prefab != null ? player2Prefab.GetComponent<PlayerController>() : null);
            SetObject(serialized, "soulTetherPrefab", soulTetherPrefab != null ? soulTetherPrefab.GetComponent<SoulTether>() : null);
            SetInt(serialized, "totalRelicsRequired", 3);
            SetFloat(serialized, "respawnDelay", 0.18f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("<b>[CoreRigBuilder]</b> Đã nối nội bộ: HUD ↔ PauseMenu ↔ IrisTransition ↔ GameManager ↔ AudioManager.");
        }

        #endregion

        #region Thả CoreRig vào Scene

        /// <summary>
        /// Thả <c>CoreRig.prefab</c> vào Scene và nối ba tham chiếu Scene còn thiếu.
        /// Trả về bộ tham chiếu để nơi gọi dùng tiếp (ví dụ đặt <c>levelIndexOverride</c>).
        /// </summary>
        /// <param name="parent">Nhóm Hierarchy cha; truyền <c>null</c> để đặt ở gốc Scene.</param>
        /// <param name="startPoint">Bệ Xuất Phát của màn (bắt buộc để sinh Player).</param>
        /// <param name="exitGate">Cổng Thần của màn.</param>
        /// <param name="cameraController">Camera bám Player của màn.</param>
        /// <param name="levelIndex">Chỉ số màn 1..5; truyền 0 để lấy từ GameSession.</param>
        public static CoreRigRefs InstantiateCoreRig(
            Transform parent,
            LevelStartPoint startPoint,
            ExitGate exitGate,
            CameraController cameraController,
            int levelIndex)
        {
            CoreRigRefs refs = new CoreRigRefs();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoreRigPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[CoreRigBuilder] Không tìm thấy {CoreRigPrefabPath}. Hãy chạy 'Tools -> One Hit Down -> Build CoreRig Prefab' trước.");
                return refs;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[CoreRigBuilder] Không thả được CoreRig.prefab vào Scene.");
                return refs;
            }

            instance.name = "CoreRig";
            instance.transform.localPosition = Vector3.zero;

            refs.Root = instance;
            refs.GameManager = instance.GetComponentInChildren<GameManager>(true);
            refs.AudioManager = instance.GetComponentInChildren<AudioManager>(true);
            refs.Hud = instance.GetComponentInChildren<HUDController>(true);
            refs.Pause = instance.GetComponentInChildren<PauseMenu>(true);
            refs.Iris = instance.GetComponentInChildren<IrisTransition>(true);
            refs.Canvas = instance.GetComponentInChildren<Canvas>(true);
            refs.EventSystem = instance.GetComponentInChildren<EventSystem>(true);

            // Nối ba tham chiếu Scene — ghi thành override của prefab instance.
            if (refs.GameManager != null)
            {
                SerializedObject serialized = new SerializedObject(refs.GameManager);
                SetObject(serialized, "startPoint", startPoint);
                SetObject(serialized, "exitGate", exitGate);
                SetObject(serialized, "mainCamera", cameraController);
                SetInt(serialized, "levelIndexOverride", levelIndex);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.RecordPrefabInstancePropertyModifications(refs.GameManager);
            }

            if (refs.GameManager == null || refs.Hud == null || refs.Pause == null || refs.Iris == null)
            {
                Debug.LogError("[CoreRigBuilder] CoreRig thiếu component gốc — prefab có thể đã hỏng. Chạy lại 'Build CoreRig Prefab'.");
            }

            return refs;
        }

        #endregion

        #region Kiểm tra CoreRig.prefab

        /// <summary>Kiểm tra prefab có đủ 7 thành phần và đã nối đúng tham chiếu nội bộ.</summary>
        public static void ValidateCoreRig()
        {
            if (!File.Exists(CoreRigPrefabPath))
            {
                Debug.LogError($"[Validate CoreRig] Không có prefab: {CoreRigPrefabPath}");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoreRigPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Validate CoreRig] Không nạp được {CoreRigPrefabPath}.");
                return;
            }

            int errorCount = 0;

            GameManager manager = prefab.GetComponentInChildren<GameManager>(true);
            AudioManager audio = prefab.GetComponentInChildren<AudioManager>(true);
            Canvas canvas = prefab.GetComponentInChildren<Canvas>(true);
            EventSystem eventSystem = prefab.GetComponentInChildren<EventSystem>(true);
            HUDController hud = prefab.GetComponentInChildren<HUDController>(true);
            PauseMenu pause = prefab.GetComponentInChildren<PauseMenu>(true);
            IrisTransition iris = prefab.GetComponentInChildren<IrisTransition>(true);

            errorCount += CheckRequired(manager, "GameManager");
            errorCount += CheckRequired(audio, "AudioManager");
            errorCount += CheckRequired(canvas, "Canvas");
            errorCount += CheckRequired(eventSystem, "EventSystem");
            errorCount += CheckRequired(hud, "HUDController");
            errorCount += CheckRequired(pause, "PauseMenu");
            errorCount += CheckRequired(iris, "IrisTransition");

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError($"[Validate CoreRig] Canvas.renderMode = {canvas.renderMode}, cần ScreenSpaceOverlay.");
                errorCount++;
            }

            if (eventSystem != null && eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                Debug.LogError("[Validate CoreRig] EventSystem thiếu InputSystemUIInputModule (activeInputHandler = 1).");
                errorCount++;
            }

            // Tham chiếu NỘI BỘ phải có sẵn trong prefab.
            if (hud != null)
            {
                SerializedObject hudSerialized = new SerializedObject(hud);
                string[] hudReferences =
                {
                    "relicIcon", "relicCounterText", "levelText", "timeText",
                    "modeText", "deathsText", "restartButton", "pauseButton", "pauseMenu"
                };
                errorCount += CheckReferences(hudSerialized, "HUDController", hudReferences);
            }

            if (pause != null)
            {
                SerializedObject pauseSerialized = new SerializedObject(pause);
                string[] pauseReferences = { "pausePanel", "resumeButton", "restartButton", "mainMenuButton" };
                errorCount += CheckReferences(pauseSerialized, "PauseMenu", pauseReferences);
            }

            if (iris != null)
            {
                SerializedObject irisSerialized = new SerializedObject(iris);
                errorCount += CheckReferences(irisSerialized, "IrisTransition", new[] { "irisImage" });
            }

            if (manager != null)
            {
                SerializedObject managerSerialized = new SerializedObject(manager);
                string[] managerInternalReferences =
                {
                    "hudController", "pauseMenu", "irisTransition", "audioManager",
                    "player1Prefab", "player2Prefab", "soulTetherPrefab"
                };
                errorCount += CheckReferences(managerSerialized, "GameManager", managerInternalReferences);

                // Ba tham chiếu Scene PHẢI để trống trong prefab — chúng được nối khi thả vào Scene.
                SerializedProperty levelOverride = managerSerialized.FindProperty("levelIndexOverride");
                if (levelOverride != null && levelOverride.intValue != 0)
                {
                    Debug.LogWarning($"[Validate CoreRig] GameManager.levelIndexOverride = {levelOverride.intValue} trong prefab; nên để 0 và đặt theo từng màn.");
                }
            }

            if (errorCount == 0)
            {
                Debug.Log($"<b>[Validate CoreRig]</b> <color=green>PASS</color> — {CoreRigPrefabPath}: đủ 7 thành phần, tham chiếu nội bộ đầy đủ.");
            }
            else
            {
                Debug.LogError($"[Validate CoreRig] FAIL — {errorCount} lỗi trong {CoreRigPrefabPath}.");
            }
        }

        private static int CheckRequired(UnityEngine.Object value, string label)
        {
            if (value == null)
            {
                Debug.LogError($"[Validate CoreRig] Thiếu {label} trong prefab.");
                return 1;
            }

            return 0;
        }

        private static int CheckReferences(SerializedObject serialized, string ownerLabel, string[] propertyNames)
        {
            int missing = 0;

            foreach (string propertyName in propertyNames)
            {
                SerializedProperty property = serialized.FindProperty(propertyName);
                if (property == null)
                {
                    Debug.LogError($"[Validate CoreRig] {ownerLabel} không có field '{propertyName}'.");
                    missing++;
                    continue;
                }

                if (property.objectReferenceValue == null)
                {
                    Debug.LogError($"[Validate CoreRig] {ownerLabel}.{propertyName} đang TRỐNG.");
                    missing++;
                }
            }

            return missing;
        }

        #endregion

        #region Helper dựng UI (uGUI thuần — không phụ thuộc TMP Essentials)

        private static int UiLayer => LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;

        internal static GameObject CreateUiRoot(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = UiLayer;

            StretchFullScreen((RectTransform)go.transform);
            return go;
        }

        internal static void StretchFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        internal static Image CreateUiImage(string name, Transform parent, Sprite sprite, bool preserveAspect)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = UiLayer;

            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = false;

            return image;
        }

        internal static Text CreateUiText(string name, Transform parent, int fontSize, TextAnchor alignment, Color color, string initialText)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = UiLayer;

            Text text = go.AddComponent<Text>();
            text.font = hudFont;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.text = initialText;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        internal static Button CreateIconButton(string name, Transform parent, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = UiLayer;

            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = true;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;

            return button;
        }

        internal static Button CreateSolidButton(string name, Transform parent, Sprite icon, string label, float y)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = UiLayer;

            Image background = go.AddComponent<Image>();
            background.color = new Color(0.16f, 0.13f, 0.24f, 0.96f);
            background.raycastTarget = true;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = background;

            CenterAt((RectTransform)go.transform, 0f, y, 620f, 84f);

            Image iconImage = CreateUiImage("Icon", go.transform, icon, true);
            RectTransform iconRect = (RectTransform)iconImage.transform;
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(24f, 0f);
            iconRect.sizeDelta = new Vector2(56f, 56f);

            Text labelText = CreateUiText("Label", go.transform, 46, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.85f, 1f), label);
            RectTransform labelRect = (RectTransform)labelText.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.offsetMin = new Vector2(90f, 0f);
            labelRect.offsetMax = new Vector2(-24f, 0f);

            return button;
        }

        internal static void PlaceTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        internal static void PlaceTopRight(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        internal static void CenterAt(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        #endregion

        #region Helper SerializedObject & tài nguyên

        private static void LoadSharedAssets()
        {
            pineappleSprite = LoadFirstSprite(PineappleTexturePath, "Pineapple_0");
            transitionSprite = LoadFirstSprite(TransitionTexturePath, "Transition_0");
            restartIconSprite = LoadFirstSprite(RestartButtonTexturePath, "Restart_0");
            playIconSprite = LoadFirstSprite(PlayButtonTexturePath, "Play_0");
            settingsIconSprite = LoadFirstSprite(SettingsButtonTexturePath, "Settings_0");
            pauseIconSprite = LoadFirstSprite(PauseIconTexturePath, "pause_0");
            hudFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);

            if (pauseIconSprite == null)
            {
                Debug.LogWarning($"[CoreRigBuilder] Không tìm thấy sprite nút Pause tại {PauseIconTexturePath}.");
            }
        }

        private static Sprite LoadFirstSprite(string texturePath, string preferredName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning($"[CoreRigBuilder] Không nạp được asset tại {texturePath}.");
                return null;
            }

            Sprite firstSprite = null;

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    if (sprite.name == preferredName)
                    {
                        return sprite;
                    }

                    if (firstSprite == null)
                    {
                        firstSprite = sprite;
                    }
                }
            }

            return firstSprite;
        }

        internal static void SetObject(SerializedObject serialized, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[CoreRigBuilder] Không tìm thấy property '{propertyName}' trên {serialized.targetObject.GetType().Name}!");
                return;
            }

            property.objectReferenceValue = value;
        }

        internal static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[CoreRigBuilder] Không tìm thấy property '{propertyName}' trên {serialized.targetObject.GetType().Name}!");
                return;
            }

            property.floatValue = value;
        }

        internal static void SetInt(SerializedObject serialized, string propertyName, int value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[CoreRigBuilder] Không tìm thấy property '{propertyName}' trên {serialized.targetObject.GetType().Name}!");
                return;
            }

            property.intValue = value;
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            if (!scene.IsValid())
            {
                return null;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T found = roots[i].GetComponentInChildren<T>(true);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void EnsureDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        #endregion
    }
}
#endif
