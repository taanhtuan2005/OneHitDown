// ====================================================================================================
// SCRIPT UNITY EDITOR: XÂY DỰNG SCENE MÀN 5 'Assets/Scenes/Level_05.unity' (CHAOS NEXUS - ENDGAME)
// Người 5 — Hoàng Thị Giang Uyên (2351060497) — Module 5.
// Tuân thủ 100%: Docs/SCHEMA_CHI_TIET.md §3 Module 5 + §4 Bảng Prefab + §5 Quy tắc Scene.
//                 Docs/Phan_cong.md Mục III (Người 5) + Mục I.1 (Scene nháp Dev_Nguoi5).
//
// MỤC ĐÍCH:
//   1. Dựng trọn vẹn Màn 5 (Endgame) dài 200 units theo đúng đặc tả:
//      - 3 Quả Cổ Vật Chính Pineapple (countsAsRelic = true) để mở Cổng Thần [3/3].
//      - 2 Quả Thưởng Phụ Melon & Strawberry (countsAsRelic = false).
//      - 2 con lắc SpikedBall giao nhau ngược chiều bằng phaseOffset (đặc trưng Màn 5).
//      - FallingPlatform đặt isPermanentInRun = true ("Ác Mộng Quay Đầu").
//      - ArrowBooster lơ lửng phía trên Hố Tử Thần Vĩnh Viễn.
//   2. Gắn TRỌN BỘ RIG MODULE 5 còn thiếu vào Scene:
//      GameManager · CameraController · AudioManager · HUDController · PauseMenu · IrisTransition
//      kèm Canvas (Screen Space Overlay) + EventSystem dùng InputSystemUIInputModule
//      (ProjectSettings.activeInputHandler = 1 → chỉ New Input System).
//   3. Dựng thêm Scene nháp 'Assets/Scenes/Dev/Dev_Nguoi5.unity' để kiểm thử HUD / Pause / Iris.
//
// Menu Editor: Tools -> One Hit Down -> Build Level 05 Scene (Chaos Nexus)
//              Tools -> One Hit Down -> Build Dev_Nguoi5 Scene
// Batch mode : -executeMethod OneHitDown.EditorTools.Module5SceneBuilder.BuildFromCommandLine
// ====================================================================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using OneHitDown.Core;
using OneHitDown.Level;
using OneHitDown.Player;
using OneHitDown.Traps;
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
    /// Công cụ Editor tự động hoá việc dựng Màn 5 (Endgame) và gắn trọn bộ rig Module 5
    /// (GameManager, CameraController, AudioManager, HUD, PauseMenu, IrisTransition, Canvas, EventSystem).
    /// </summary>
    public static class Module5SceneBuilder
    {
        #region Hằng số đường dẫn

        private const string SceneDir = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/Level_05.unity";
        private const string DevSceneDir = "Assets/Scenes/Dev";
        private const string DevScenePath = "Assets/Scenes/Dev/Dev_Nguoi5.unity";

        private const string PrefabsDir = "Assets/Prefabs";
        private const string PrefabsTrapsDir = "Assets/Prefabs/Traps";

        private const string TerrainTexturePath = "Assets/Art/02_MoiTruong/PA1_Terrain/Terrain (16x16).png";
        private const string BackgroundTexturePath = "Assets/Art/02_MoiTruong/PA1_Background/Purple.png";
        private const string PineappleTexturePath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Fruits/Pineapple.png";
        private const string TransitionTexturePath = "Assets/Art/07_HieuUng/PA1_Other/Transition.png";
        private const string FontPath = "Assets/Art/08_Font/VT323/VT323-Regular.ttf";
        private const string RestartButtonTexturePath = "Assets/Art/05_GiaoDien/PA1_Menu/Buttons/Restart.png";
        private const string PlayButtonTexturePath = "Assets/Art/05_GiaoDien/PA1_Menu/Buttons/Play.png";
        private const string SettingsButtonTexturePath = "Assets/Art/05_GiaoDien/PA1_Menu/Buttons/Settings.png";
        private const string PauseIconTexturePath = "Assets/Art/05_GiaoDien/Kenney_GameIcons/UI/PNG/White/2x/pause.png";

        // 7 Sprite 9-Slice đã được khai báo sẵn trong .meta của Terrain (16x16).png (do Level03SceneBuilder cắt).
        private const string StoneBlockSpriteName = "Terrain (16x16)_StoneBlock";
        private const string StoneInnerBrickSpriteName = "Terrain (16x16)_StoneInnerBricks";
        private const string OneWayGoldSpriteName = "Terrain (16x16)_OneWayGold";
        private const string SteelBeamHorizSpriteName = "Terrain (16x16)_SteelBeamHoriz";
        private const string SteelPillarVertSpriteName = "Terrain (16x16)_SteelPillarVert";
        private const string SteelBlock2x2SpriteName = "Terrain (16x16)_SteelBlock2x2";

        private const int GroundLayer = 8;

        #endregion

        #region Sprite cache

        private static Sprite stoneBlockSprite;
        private static Sprite stoneInnerBrickSprite;
        private static Sprite oneWayGoldSprite;
        private static Sprite steelBeamHorizSprite;
        private static Sprite steelPillarVertSprite;
        private static Sprite steelBlock2x2Sprite;
        private static Sprite pineappleSprite;
        private static Sprite transitionSprite;
        private static Font hudFont;
        private static Sprite restartIconSprite;
        private static Sprite playIconSprite;
        private static Sprite settingsIconSprite;
        private static Sprite pauseIconSprite;

        #endregion

        #region Menu Editor

        [MenuItem("Tools/One Hit Down/Build Level 05 Scene (Chaos Nexus)", false, 31)]
        public static void BuildLevel05Menu()
        {
            BuildLevel05Scene(openScene: true);
        }

        [MenuItem("Tools/One Hit Down/Build Dev_Nguoi5 Scene", false, 21)]
        public static void BuildDevNguoi5Menu()
        {
            BuildDevNguoi5Scene(openScene: true);
        }

        [MenuItem("Tools/One Hit Down/Validate Level 05 Scene", false, 32)]
        public static void ValidateLevel05Menu()
        {
            ValidateScene(ScenePath);
        }

        /// <summary>
        /// Điểm vào cho batch mode:
        /// Unity.exe -batchmode -quit -projectPath &lt;path&gt; -executeMethod OneHitDown.EditorTools.Module5SceneBuilder.BuildFromCommandLine
        /// </summary>
        public static void BuildFromCommandLine()
        {
            BuildLevel05Scene(openScene: false);
            BuildDevNguoi5Scene(openScene: false);
        }

        [InitializeOnLoadMethod]
        private static void OnProjectLoaded()
        {
            // Chỉ tự dựng khi mở Editor tương tác; batch mode do BuildFromCommandLine chủ động gọi.
            if (Application.isBatchMode)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (File.Exists(ScenePath))
                {
                    return;
                }

                Debug.Log("<b>[Module5SceneBuilder]</b> Chưa có Scene Level_05.unity. Đang tự động tạo...");
                BuildLevel05Scene(openScene: false);
            };
        }

        #endregion

        #region Dựng Scene Màn 5

        /// <summary>Tạo mới hoàn toàn Scene Level_05.unity với địa hình, bẫy, Cổ Vật và trọn bộ rig Module 5.</summary>
        public static void BuildLevel05Scene(bool openScene = true)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Level 05 - Build Scene", "Nạp Sprite & Prefab chuẩn...", 0.05f);
                EnsureDirectory(SceneDir);
                LoadSharedAssets();

                if (stoneBlockSprite == null)
                {
                    Debug.LogError($"[Module5SceneBuilder] Không tìm thấy sprite '{StoneBlockSpriteName}' trong {TerrainTexturePath}. Hủy dựng Scene.");
                    return;
                }

                if (hudFont == null)
                {
                    Debug.LogError($"[Module5SceneBuilder] Không tìm thấy font HUD tại {FontPath}. Hủy dựng Scene.");
                    return;
                }

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // --- Các nhóm Hierarchy theo quy chuẩn dự án ---
                GameObject envGroup = new GameObject("--- 01. ENVIRONMENT & BACKGROUND ---");
                GameObject terrainGroup = new GameObject("--- 02. TERRAIN & CHAOS ARCHITECTURE ---");
                GameObject trapsGroup = new GameObject("--- 03. TRAPS & HAZARDS ---");
                GameObject relicsGroup = new GameObject("--- 04. RELICS & BONUS FRUITS ---");
                GameObject coreGroup = new GameObject("--- 05. MODULE 5 CORE (GAMEMANAGER / CAMERA / AUDIO) ---");
                GameObject uiGroup = new GameObject("--- 06. MODULE 5 UI (CANVAS / HUD / PAUSE / IRIS) ---");
                GameObject signsGroup = new GameObject("--- 07. SIGNS & LABELS ---");

                EditorUtility.DisplayProgressBar("Level 05 - Build Scene", "Dựng nền & địa hình...", 0.20f);
                CreateBackground(envGroup.transform, LoadFirstSprite(BackgroundTexturePath, "Purple_0"));
                BuildTerrain(terrainGroup.transform);

                EditorUtility.DisplayProgressBar("Level 05 - Build Scene", "Bố trí Bẫy & Hố Tử Thần...", 0.45f);
                BuildTraps(trapsGroup.transform);

                EditorUtility.DisplayProgressBar("Level 05 - Build Scene", "Đặt Cổ Vật & Quả Thưởng...", 0.60f);
                ExitGate exitGate = BuildRelicsAndExitGate(relicsGroup.transform);

                EditorUtility.DisplayProgressBar("Level 05 - Build Scene", "Gắn Camera, AudioManager & GameManager...", 0.72f);
                CameraController cameraController = CreateCamera(coreGroup.transform);
                AudioManager audioManager = CreateAudioManager(coreGroup.transform);
                LevelStartPoint startPoint = CreateLevelStart(coreGroup.transform);

                EditorUtility.DisplayProgressBar("Level 05 - Build Scene", "Dựng Canvas / HUD / PauseMenu / IrisTransition...", 0.85f);
                Module5UiRig ui = BuildModule5Ui(uiGroup.transform);

                GameManager gameManager = CreateGameManager(coreGroup.transform, startPoint, exitGate, cameraController, audioManager, ui, 5);

                EditorUtility.DisplayProgressBar("Level 05 - Build Scene", "Gắn biển hiệu & lưu Scene...", 0.94f);
                CreateSigns(signsGroup.transform);

                // Ghi GameManager vào SerializedObject của HUD/Pause/Iris để liên kết 2 chiều đủ.
                WireUiReferences(ui, gameManager);

                bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
                RegisterSceneInBuildSettings(ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (saved)
                {
                    Debug.Log($"<b>[Module5SceneBuilder]</b> TẠO THÀNH CÔNG SCENE: <color=green>{ScenePath}</color>");
                    if (openScene)
                    {
                        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Module5SceneBuilder] Lỗi tạo Scene Level_05: {ex}");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>Dựng Scene nháp Dev_Nguoi5.unity để kiểm thử nhanh HUD / PauseMenu / IrisTransition.</summary>
        public static void BuildDevNguoi5Scene(bool openScene = true)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Dev_Nguoi5 - Build Scene", "Nạp Sprite & Prefab chuẩn...", 0.05f);
                EnsureDirectory(DevSceneDir);
                LoadSharedAssets();

                if (stoneBlockSprite == null || hudFont == null)
                {
                    Debug.LogError("[Module5SceneBuilder] Thiếu sprite đá hoặc font HUD. Hủy dựng Scene nháp.");
                    return;
                }

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                GameObject envGroup = new GameObject("--- 01. ENVIRONMENT & BACKGROUND ---");
                GameObject terrainGroup = new GameObject("--- 02. TEST TERRAIN ---");
                GameObject coreGroup = new GameObject("--- 03. MODULE 5 CORE ---");
                GameObject uiGroup = new GameObject("--- 04. MODULE 5 UI ---");
                GameObject labelsGroup = new GameObject("--- 05. LABELS ---");

                CreateBackground(envGroup.transform, LoadFirstSprite(BackgroundTexturePath, "Purple_0"));

                // Sàn phẳng dài 60 units để chạy thử R / ESC-P / G / H / T và nhặt thử Cổ Vật.
                CreateStoneRect("Dev_Floor", 0f, 0f, 60f, 4f, terrainGroup.transform, addInnerBricks: true);
                CreateStoneRect("Dev_Ceiling", 0f, 15f, 60f, 3f, terrainGroup.transform, addInnerBricks: false);

                CameraController cameraController = CreateCamera(coreGroup.transform);
                AudioManager audioManager = CreateAudioManager(coreGroup.transform);
                LevelStartPoint startPoint = CreateLevelStart(coreGroup.transform, new Vector3(3f, 4.1f, 0f));
                ExitGate exitGate = CreateExitGate(terrainGroup.transform, new Vector3(52f, 5.4f, 0f));

                // 3 Cổ Vật Pineapple để thử chuỗi [0/3] -> [3/3] -> mở cổng.
                CreateRelic(terrainGroup.transform, "Dev_Relic_1", new Vector3(10f, 5.5f, 0f));
                CreateRelic(terrainGroup.transform, "Dev_Relic_2", new Vector3(20f, 5.5f, 0f));
                CreateRelic(terrainGroup.transform, "Dev_Relic_3", new Vector3(30f, 5.5f, 0f));

                Module5UiRig ui = BuildModule5Ui(uiGroup.transform);
                GameManager gameManager = CreateGameManager(coreGroup.transform, startPoint, exitGate, cameraController, audioManager, ui, 5);
                WireUiReferences(ui, gameManager);

                CreateLabel("Dev_Header", "DEV NGUOI 5 — KIỂM THỬ MODULE 5: HUD / PAUSE / IRIS / RESET", new Vector3(18f, 13.5f, 0f), labelsGroup.transform);

                bool saved = EditorSceneManager.SaveScene(scene, DevScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (saved)
                {
                    Debug.Log($"<b>[Module5SceneBuilder]</b> TẠO THÀNH CÔNG SCENE NHÁP: <color=green>{DevScenePath}</color>");
                    if (openScene)
                    {
                        EditorSceneManager.OpenScene(DevScenePath, OpenSceneMode.Single);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Module5SceneBuilder] Lỗi tạo Scene Dev_Nguoi5: {ex}");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #endregion

        #region Nạp tài nguyên dùng chung

        private static void LoadSharedAssets()
        {
            stoneBlockSprite = LoadFirstSprite(TerrainTexturePath, StoneBlockSpriteName);
            stoneInnerBrickSprite = LoadFirstSprite(TerrainTexturePath, StoneInnerBrickSpriteName);
            oneWayGoldSprite = LoadFirstSprite(TerrainTexturePath, OneWayGoldSpriteName);
            steelBeamHorizSprite = LoadFirstSprite(TerrainTexturePath, SteelBeamHorizSpriteName);
            steelPillarVertSprite = LoadFirstSprite(TerrainTexturePath, SteelPillarVertSpriteName);
            steelBlock2x2Sprite = LoadFirstSprite(TerrainTexturePath, SteelBlock2x2SpriteName);
            pineappleSprite = LoadFirstSprite(PineappleTexturePath, "Pineapple_0");
            transitionSprite = LoadFirstSprite(TransitionTexturePath, "Transition_0");
            restartIconSprite = LoadFirstSprite(RestartButtonTexturePath, "Restart_0");
            playIconSprite = LoadFirstSprite(PlayButtonTexturePath, "Play_0");
            settingsIconSprite = LoadFirstSprite(SettingsButtonTexturePath, "Settings_0");
            pauseIconSprite = LoadFirstSprite(PauseIconTexturePath, "pause_0");
            hudFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);

            if (hudFont == null)
            {
                Debug.LogError($"[Module5SceneBuilder] Không nạp được Font tại {FontPath}.");
            }
        }

        private static Sprite LoadFirstSprite(string texturePath, string preferredName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite sprite && sprite.name == preferredName)
                {
                    return sprite;
                }
            }

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    return sprite;
                }
            }

            Debug.LogWarning($"[Module5SceneBuilder] Không tìm thấy sprite '{preferredName}' tại {texturePath}.");
            return null;
        }

        private static GameObject LoadPrefab(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Module5SceneBuilder] Không tìm thấy Prefab tại: {prefabPath}");
            }

            return prefab;
        }

        private static GameObject InstantiatePrefab(string prefabPath, Transform parent, Vector3 position)
        {
            GameObject prefab = LoadPrefab(prefabPath);
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
            return instance;
        }

        #endregion

        #region Nền & Địa hình Màn 5

        private static void CreateBackground(Transform parent, Sprite backgroundSprite)
        {
            if (backgroundSprite == null)
            {
                return;
            }

            GameObject backgroundGo = new GameObject("Background_Purple");
            backgroundGo.transform.SetParent(parent);
            backgroundGo.transform.position = new Vector3(100f, 10f, 10f);

            SpriteRenderer renderer = backgroundGo.AddComponent<SpriteRenderer>();
            renderer.sprite = backgroundSprite;
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.sortingOrder = -50;

            // Phóng bằng transform thay vì Tiled: sprite nền không cần Mesh Type = Full Rect.
            Vector2 spriteSize = backgroundSprite.bounds.size;
            const float backgroundWidth = 230f;
            const float backgroundHeight = 28f;
            backgroundGo.transform.localScale = new Vector3(
                spriteSize.x > 0.001f ? backgroundWidth / spriteSize.x : 1f,
                spriteSize.y > 0.001f ? backgroundHeight / spriteSize.y : 1f,
                1f);
        }

        /// <summary>
        /// Địa hình Màn 5: 9 thềm đá rải từ X = 0..200, giữa các thềm là 8 vực thẳm
        /// (mỗi vực gắn 1 KillZone ở đáy để rơi xuống là chết ngay, đúng cơ chế Level/KillZone.cs).
        /// </summary>
        private static void BuildTerrain(Transform parent)
        {
            GameObject ceilingGroup = new GameObject("01_Chaos_Ceiling");
            ceilingGroup.transform.SetParent(parent);

            GameObject groundGroup = new GameObject("02_Stone_Ground_Platforms");
            groundGroup.transform.SetParent(parent);

            GameObject platformsGroup = new GameObject("03_OneWay_Platforms_And_Chains");
            platformsGroup.transform.SetParent(parent);

            GameObject hazardGroup = new GameObject("04_Death_Pits");
            hazardGroup.transform.SetParent(parent);

            // --- A. Trần hang Chaos Nexus (đỡ con lắc & tạo cảm giác ngột ngạt) ---
            (string name, float x, float yBottom, float w, float h)[] ceilingSegments =
            {
                ("C_S1_Ceiling",       0f,   15f, 30f, 3f),
                ("C_S2_PendulumVault", 36f,  14f, 22f, 3f),
                ("C_S3_Ceiling",       64f,  15f, 54f, 3f),
                ("C_S4_Ceiling",       124f, 15f, 38f, 3f),
                ("C_S5_AltarVault",    168f, 14f, 32f, 4f)
            };

            foreach (var segment in ceilingSegments)
            {
                CreateStoneRect(segment.name, segment.x, segment.yBottom, segment.w, segment.h, ceilingGroup.transform, addInnerBricks: true);
            }

            // --- B. 9 thềm đá chính (mặt trên Y = 4) ---
            (string name, float xLeft, float width)[] ledges =
            {
                ("L5_S1_StartLedge",   0f,   12f),
                ("L5_S1_LedgeB",       18f,  12f),
                ("L5_S2_PendulumHall", 36f,  22f),
                ("L5_S2_LedgeC",       64f,  12f),
                ("L5_S3_LedgeD",       82f,  14f),
                ("L5_S3_LedgeE",       102f, 16f),
                ("L5_S4_LedgeF",       124f, 16f),
                ("L5_S4_LedgeG",       146f, 16f),
                ("L5_S5_Altar",        168f, 32f)
            };

            foreach (var ledge in ledges)
            {
                CreateStoneRect(ledge.name, ledge.xLeft, 0f, ledge.width, 4f, groundGroup.transform, addInnerBricks: true);
            }

            // --- C. 8 vực thẳm: đáy đá + KillZone phủ kín miệng vực ---
            (string name, float xLeft, float width)[] pits =
            {
                ("Pit_S1_A", 12f,  6f),
                ("Pit_S1_B", 30f,  6f),
                ("Pit_S2_A", 58f,  6f),
                ("Pit_S3_A", 76f,  6f),
                ("Pit_S3_B", 96f,  6f),
                ("Pit_S4_A", 118f, 6f),
                ("Pit_S4_B", 140f, 6f),
                ("Pit_S5_A", 162f, 6f)
            };

            foreach (var pit in pits)
            {
                CreateStoneRect($"{pit.name}_Bed", pit.xLeft, 0f, pit.width, 1f, groundGroup.transform, addInnerBricks: false);
                CreateKillZone($"{pit.name}_KillZone", pit.xLeft, pit.width, hazardGroup.transform);
            }

            // --- D. Bệ treo One-Way vàng kim (9-Slice + PlatformEffector2D) ---
            CreateOneWayBar("L5_Bridge1", 20f, 8.5f, 5f, platformsGroup.transform);
            CreateOneWayBar("L5_Bridge2", 92f, 9.5f, 5f, platformsGroup.transform);
            CreateOneWayBar("L5_Bridge3", 128f, 9.5f, 5f, platformsGroup.transform);
            CreateOneWayBar("L5_Bridge4", 152f, 9.5f, 5f, platformsGroup.transform);

            // --- E. Dầm thép & trụ thép trang trí ở Điện Thờ Cổng Thần ---
            CreateSteelPillar("L5_AltarPillarLeft", 168f, 4f, 10f, platformsGroup.transform, withCollider: false);
            CreateSteelPillar("L5_AltarPillarRight", 199f, 4f, 10f, platformsGroup.transform, withCollider: false);
            CreateSteelBlock2x2("L5_AltarCapLeft", 168f, 12f, platformsGroup.transform, withCollider: false);
            CreateSteelBlock2x2("L5_AltarCapRight", 198f, 12f, platformsGroup.transform, withCollider: false);

            // 2 khối thép 2x2 đặt hai bên Cổng Thần (x 186..188 và 196..198) — thuần trang trí, không collider
            // để không chặn đường vào Cổng (Cổng chiếm x 190.5..193.5).
            CreateSteelBlock2x2("L5_AltarFlankLeft", 186f, 4f, platformsGroup.transform, withCollider: false);
            CreateSteelBlock2x2("L5_AltarFlankRight", 196f, 4f, platformsGroup.transform, withCollider: false);

            // Dầm ngang bắc qua 2 trụ, đặt ở Y = 12 (trên đỉnh Cổng Thần Y = 6.81) — không chặn lối vào.
            CreateSteelBeam("L5_AltarLintel", 168f, 12f, 32f, platformsGroup.transform, withCollider: false, sortingOrder: 0);
        }

        private static GameObject CreateStoneRect(string name, float xLeft, float yBottom, float width, float height, Transform parent, bool addInnerBricks)
        {
            Vector2 center = new Vector2(xLeft + width * 0.5f, yBottom + height * 0.5f);
            Vector2 size = new Vector2(width, height);

            GameObject boxGo = new GameObject(name);
            boxGo.transform.SetParent(parent);
            boxGo.transform.position = new Vector3(center.x, center.y, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                boxGo.layer = groundLayer;
            }

            SpriteRenderer renderer = boxGo.AddComponent<SpriteRenderer>();
            renderer.sprite = stoneBlockSprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = size;
            renderer.sortingOrder = 1;

            BoxCollider2D collider = boxGo.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.offset = Vector2.zero;

            if (addInnerBricks && stoneInnerBrickSprite != null && width >= 3f && height >= 3f)
            {
                int widthInt = Mathf.RoundToInt(width);
                int heightInt = Mathf.RoundToInt(height);

                for (int row = 1; row < heightInt - 1; row += 2)
                {
                    for (int column = 1; column < widthInt - 1; column += 3)
                    {
                        int offsetColumn = column + ((row / 2) % 2);
                        if (offsetColumn + 2 > widthInt - 1 || row + 2 > heightInt - 1)
                        {
                            continue;
                        }

                        GameObject brickChild = new GameObject($"InnerBrick_{row}_{offsetColumn}");
                        brickChild.transform.SetParent(boxGo.transform, false);
                        brickChild.transform.localPosition = new Vector3(
                            -width * 0.5f + offsetColumn + 1.0f,
                            height * 0.5f - row - 1.0f,
                            0f);

                        SpriteRenderer brickRenderer = brickChild.AddComponent<SpriteRenderer>();
                        brickRenderer.sprite = stoneInnerBrickSprite;
                        brickRenderer.sortingOrder = 2;
                    }
                }
            }

            return boxGo;
        }

        private static void CreateOneWayBar(string name, float xLeft, float yTop, float width, Transform parent)
        {
            if (oneWayGoldSprite == null)
            {
                return;
            }

            const float barHeight = 0.5f;
            GameObject barGo = new GameObject(name);
            barGo.transform.SetParent(parent);
            barGo.transform.position = new Vector3(xLeft + width * 0.5f, yTop - barHeight * 0.5f, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                barGo.layer = groundLayer;
            }

            SpriteRenderer renderer = barGo.AddComponent<SpriteRenderer>();
            renderer.sprite = oneWayGoldSprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(width, barHeight);
            renderer.sortingOrder = 2;

            BoxCollider2D collider = barGo.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width, barHeight);
            collider.offset = Vector2.zero;
            collider.usedByEffector = true;

            PlatformEffector2D effector = barGo.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 180f;
        }

        private static void CreateSteelPillar(string name, float xLeft, float yBottom, float height, Transform parent, bool withCollider)
        {
            if (steelPillarVertSprite == null)
            {
                return;
            }

            GameObject pillarGo = new GameObject(name);
            pillarGo.transform.SetParent(parent);
            pillarGo.transform.position = new Vector3(xLeft + 0.5f, yBottom + height * 0.5f, 0f);

            SpriteRenderer renderer = pillarGo.AddComponent<SpriteRenderer>();
            renderer.sprite = steelPillarVertSprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(1f, height);
            renderer.sortingOrder = 0;

            if (!withCollider)
            {
                return;
            }

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                pillarGo.layer = groundLayer;
            }

            BoxCollider2D collider = pillarGo.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, height);
        }

        private static void CreateSteelBeam(string name, float xLeft, float yBottom, float width, Transform parent, bool withCollider, int sortingOrder)
        {
            if (steelBeamHorizSprite == null)
            {
                return;
            }

            GameObject beamGo = new GameObject(name);
            beamGo.transform.SetParent(parent);
            beamGo.transform.position = new Vector3(xLeft + width * 0.5f, yBottom + 0.5f, 0f);

            SpriteRenderer renderer = beamGo.AddComponent<SpriteRenderer>();
            renderer.sprite = steelBeamHorizSprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(width, 1f);
            renderer.sortingOrder = sortingOrder;

            if (!withCollider)
            {
                return;
            }

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                beamGo.layer = groundLayer;
            }

            BoxCollider2D collider = beamGo.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width, 1f);
        }

        private static void CreateSteelBlock2x2(string name, float xLeft, float yBottom, Transform parent, bool withCollider)
        {
            if (steelBlock2x2Sprite == null)
            {
                return;
            }

            GameObject blockGo = new GameObject(name);
            blockGo.transform.SetParent(parent);
            blockGo.transform.position = new Vector3(xLeft + 1f, yBottom + 1f, 0f);

            SpriteRenderer renderer = blockGo.AddComponent<SpriteRenderer>();
            renderer.sprite = steelBlock2x2Sprite;
            renderer.sortingOrder = 1;

            if (!withCollider)
            {
                return;
            }

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                blockGo.layer = groundLayer;
            }

            BoxCollider2D collider = blockGo.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(2f, 2f);
        }

        /// <summary>Tạo vùng chết ở đáy vực bằng script Level/KillZone.cs (tự bật isTrigger trong Awake).</summary>
        private static void CreateKillZone(string name, float xLeft, float width, Transform parent)
        {
            GameObject zoneGo = new GameObject(name);
            zoneGo.transform.SetParent(parent);
            zoneGo.transform.position = new Vector3(xLeft + width * 0.5f, 1.5f, 0f);

            BoxCollider2D collider = zoneGo.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(width, 5f);

            zoneGo.AddComponent<KillZone>();
        }

        #endregion

        #region Bẫy Màn 5

        /// <summary>
        /// Bố trí toàn bộ bẫy theo đúng đặc tả Màn 5:
        /// 2 con lắc SpikedBall giao nhau ngược chiều, FallingPlatform vĩnh viễn,
        /// ArrowBooster trên Hố Tử Thần, cùng dàn bẫy Module 2 & 3.
        /// </summary>
        private static void BuildTraps(Transform parent)
        {
            GameObject pendulumGroup = new GameObject("01_SpikedBall_Pendulums");
            pendulumGroup.transform.SetParent(parent);

            GameObject module2Group = new GameObject("02_Module2_Lethal_Traps");
            module2Group.transform.SetParent(parent);

            GameObject module3Group = new GameObject("03_Module3_Interactive_Traps");
            module3Group.transform.SetParent(parent);

            // --- A. 2 con lắc giao nhau ngược chiều (đặc trưng Màn 5, SCHEMA dòng 319-327) ---
            // Trục treo ở đáy trần Y = 14; armLength 9 → quả cầu hạ xuống Y = 5, quét ngang ±8.16 units.
            // phaseOffset = PI / swingSpeed để 2 con lắc lệch pha đúng nửa chu kỳ (ngược chiều).
            const float pendulumSwingSpeed = 2.5f;
            float oppositePhase = Mathf.PI / pendulumSwingSpeed;

            CreateSpikedBallPendulum("L5_Pendulum_A", new Vector3(44f, 14f, 0f), 9f, 0f, pendulumGroup.transform);
            CreateSpikedBallPendulum("L5_Pendulum_B", new Vector3(50f, 14f, 0f), 9f, oppositePhase, pendulumGroup.transform);

            // --- B. Module 2: bẫy sát thương trực tiếp ---
            // Hàng gai đầu tiên đặt từ x = 8 (cách điểm xuất phát x = 3 đúng 5 units) để người chơi
            // có khoảng đệm quan sát trước khi gặp bẫy đầu tiên.
            CreateSpikeRow("L5_Spikes_S1", 8f, 4.5f, 3, module2Group.transform);
            CreateSpikeRow("L5_Spikes_S2", 68f, 4.5f, 4, module2Group.transform);
            CreateSpikeRow("L5_Spikes_S3", 106f, 4.5f, 5, module2Group.transform);
            CreateSpikeRow("L5_Spikes_S4", 150f, 4.5f, 4, module2Group.transform);

            // Cưa xoay đặt thấp (đáy Y = 4.35) để người chơi nhảy qua được bằng Nhảy Đơn.
            InstantiatePrefab($"{PrefabsDir}/Saw.prefab", module2Group.transform, new Vector3(86f, 5.4f, 0f));

            // Đầu Đá treo dưới trần C_S4 (đáy trần Y = 15) ngay trên Bệ One-Way L5_Bridge3 (mặt Y = 9.5).
            // RockHead chỉ lao xuống tối đa detectionDistance = 6 units nên phải đặt đủ thấp:
            // đáy đầu đá Y = 12.6 → lao 3.1 units là chạm mặt bệ, đủ tầm với người chơi đứng trên bệ (đầu Y = 11.05).
            InstantiatePrefab($"{PrefabsDir}/RockHead.prefab", module2Group.transform, new Vector3(130.5f, 13.8f, 0f));

            // Đầu Gai phóng ngang: nghỉ trên miệng vực S4_A (118..124) rồi lao sang trái 4 units
            // quét qua mép phải thềm L5_S3_LedgeE. Điểm nghỉ không chặn lối đi.
            GameObject spikeHead = InstantiatePrefab($"{PrefabsDir}/SpikeHead.prefab", module2Group.transform, new Vector3(119.5f, 5.6f, 0f));
            if (spikeHead != null)
            {
                SerializedObject spikeSerialized = new SerializedObject(spikeHead.GetComponent<SpikeHeadTrap>());
                SetVector2(spikeSerialized, "dashDirection", Vector2.left);
                SetFloat(spikeSerialized, "dashDistance", 4f);
                spikeSerialized.ApplyModifiedPropertiesWithoutUndo();
            }

            InstantiatePrefab($"{PrefabsDir}/FireTrap.prefab", module2Group.transform, new Vector3(24f, 4.55f, 0f));
            InstantiatePrefab($"{PrefabsDir}/FireTrap.prefab", module2Group.transform, new Vector3(170f, 4.55f, 0f));

            // --- C. Module 3: cơ quan tương tác & môi trường ---
            // Đệm lò xo đặt trên thềm S1_LedgeB (18..30, mặt Y = 4) theo đúng chuẩn Level_03 (Y = mặt đất + 0.25).
            InstantiatePrefab($"{PrefabsTrapsDir}/Trampoline.prefab", module3Group.transform, new Vector3(26f, 4.25f, 0f));

            // Bệ nâng tuần tra dọc giữa vực S1_B (30..36) làm bàn đạp trung gian.
            GameObject movingPlatform = InstantiatePrefab($"{PrefabsDir}/MovingPlatform.prefab", module3Group.transform, new Vector3(33f, 4f, 0f));
            if (movingPlatform != null)
            {
                SerializedObject movingSerialized = new SerializedObject(movingPlatform.GetComponent<MovingPlatform>());
                SetVector2(movingSerialized, "moveOffset", new Vector2(0f, 2.5f));
                SetFloat(movingSerialized, "moveSpeed", 2f);
                movingSerialized.ApplyModifiedPropertiesWithoutUndo();
            }

            // Bệ sập VĨNH VIỄN ("Ác Mộng Quay Đầu" — SCHEMA dòng 368): chỉ hồi khi ResetLevel().
            // Đây là cầu nối duy nhất qua vực S3_A (76..82) và S3_B (96..102): mặt bệ Y = 3.81 nằm
            // ngay dưới mặt thềm Y = 4 nên nhảy xuống là chạm, nhưng sập rồi thì phải chờ ResetLevel().
            CreateFallingPlatform("L5_FallingPlatform_A1", new Vector3(77.5f, 3.5f, 0f), module3Group.transform);
            CreateFallingPlatform("L5_FallingPlatform_A2", new Vector3(80.5f, 3.5f, 0f), module3Group.transform);
            CreateFallingPlatform("L5_FallingPlatform_B1", new Vector3(97.5f, 3.5f, 0f), module3Group.transform);
            CreateFallingPlatform("L5_FallingPlatform_B2", new Vector3(100.5f, 3.5f, 0f), module3Group.transform);

            // Bong bóng Mũi Tên Đẩy Khí lơ lửng trên Hố Tử Thần Vĩnh Viễn (SCHEMA dòng 392).
            // Đặt Y = 7.2 (trên mép vực S4_B 140..146 và dưới bệ One-Way L5_Bridge4 mặt Y = 9.5)
            // để người chơi nhảy từ thềm L5_S4_LedgeG (mặt Y = 4) chạm được bong bóng rồi được đẩy lên bệ.
            GameObject arrowBooster = InstantiatePrefab($"{PrefabsTrapsDir}/ArrowBooster.prefab", module3Group.transform, new Vector3(143f, 7.2f, 0f));
            if (arrowBooster != null)
            {
                SerializedObject arrowSerialized = new SerializedObject(arrowBooster.GetComponent<ArrowBooster>());
                SetVector2(arrowSerialized, "boostDirection", Vector2.up);
                SetFloat(arrowSerialized, "boostForce", 14f);
                SetFloat(arrowSerialized, "reactivateDelay", 2.5f);
                arrowSerialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void CreateSpikedBallPendulum(string name, Vector3 pivotPosition, float armLength, float phaseOffset, Transform parent)
        {
            GameObject pendulumGo = InstantiatePrefab($"{PrefabsDir}/SpikedBall.prefab", parent, pivotPosition);
            if (pendulumGo == null)
            {
                return;
            }

            pendulumGo.name = name;

            SpikedBallPendulum pendulum = pendulumGo.GetComponent<SpikedBallPendulum>();
            if (pendulum == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(pendulum);
            SetFloat(serialized, "armLength", armLength);
            SetFloat(serialized, "maxAngle", 65f);
            SetFloat(serialized, "swingSpeed", 2.5f);
            SetFloat(serialized, "phaseOffset", phaseOffset);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateSpikeRow(string name, float xLeft, float yCenter, int count, Transform parent)
        {
            GameObject rowGroup = new GameObject(name);
            rowGroup.transform.SetParent(parent);
            rowGroup.transform.position = new Vector3(xLeft, yCenter, 0f);

            for (int i = 0; i < count; i++)
            {
                InstantiatePrefab($"{PrefabsDir}/Spikes.prefab", rowGroup.transform, new Vector3(xLeft + i, yCenter, 0f));
            }
        }

        private static void CreateFallingPlatform(string name, Vector3 position, Transform parent)
        {
            GameObject platformGo = InstantiatePrefab($"{PrefabsTrapsDir}/FallingPlatform.prefab", parent, position);
            if (platformGo == null)
            {
                return;
            }

            platformGo.name = name;

            FallingPlatform platform = platformGo.GetComponent<FallingPlatform>();
            if (platform == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(platform);
            SetBool(serialized, "isPermanentInRun", true);
            SetFloat(serialized, "shakeDuration", 0.4f);
            SetFloat(serialized, "fallSpeed", 12f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        #endregion

        #region Cổ Vật, Quả Thưởng & Cổng Thần

        private static ExitGate BuildRelicsAndExitGate(Transform parent)
        {
            // 3 Quả Cổ Vật Chính Pineapple (countsAsRelic = true) — SCHEMA dòng 72.
            CreateRelic(parent, "L5_Relic_1_Pineapple", new Vector3(7f, 5.5f, 0f));
            CreateRelic(parent, "L5_Relic_2_Pineapple", new Vector3(70f, 5.5f, 0f));
            CreateRelic(parent, "L5_Relic_3_Pineapple", new Vector3(155f, 5.5f, 0f));

            // 2 Quả Thưởng Phụ (countsAsRelic = false) — SCHEMA dòng 73: Melon & Strawberry ở Màn 5.
            CreateBonusFruit(parent, "L5_Bonus_Melon", $"{PrefabsDir}/Fruit_Melon.prefab", new Vector3(22f, 9.5f, 0f));
            CreateBonusFruit(parent, "L5_Bonus_Strawberry", $"{PrefabsDir}/Fruit_Strawberry.prefab", new Vector3(94f, 10.5f, 0f));

            return CreateExitGate(parent, new Vector3(192f, 5.4f, 0f));
        }

        private static void CreateRelic(Transform parent, string name, Vector3 position)
        {
            GameObject relicGo = InstantiatePrefab($"{PrefabsDir}/Fruit_Pineapple.prefab", parent, position);
            if (relicGo == null)
            {
                return;
            }

            relicGo.name = name;

            OneHitDown.Items.CollectibleItem item = relicGo.GetComponent<OneHitDown.Items.CollectibleItem>();
            if (item == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(item);
            SetInt(serialized, "fruitType", (int)OneHitDown.Items.FruitType.Pineapple);
            SetBool(serialized, "countsAsRelic", true);
            SetInt(serialized, "scoreValue", 100);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateBonusFruit(Transform parent, string name, string prefabPath, Vector3 position)
        {
            GameObject fruitGo = InstantiatePrefab(prefabPath, parent, position);
            if (fruitGo == null)
            {
                return;
            }

            fruitGo.name = name;

            OneHitDown.Items.CollectibleItem item = fruitGo.GetComponent<OneHitDown.Items.CollectibleItem>();
            if (item == null)
            {
                return;
            }

            // Prefab gốc đang đặt countsAsRelic = true → phải hạ xuống false để không phá bộ đếm [x/3].
            SerializedObject serialized = new SerializedObject(item);
            SetBool(serialized, "countsAsRelic", false);
            SetInt(serialized, "scoreValue", 100);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ExitGate CreateExitGate(Transform parent, Vector3 position)
        {
            GameObject gateGo = InstantiatePrefab($"{PrefabsDir}/ExitGate.prefab", parent, position);
            return gateGo != null ? gateGo.GetComponent<ExitGate>() : null;
        }

        #endregion

        #region Camera, AudioManager, LevelStart & GameManager

        private static CameraController CreateCamera(Transform parent)
        {
            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.transform.SetParent(parent);
            cameraGo.transform.position = new Vector3(6f, 8f, -10f);
            cameraGo.tag = "MainCamera";

            Camera camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.06f, 0.16f, 1f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;

            cameraGo.AddComponent<AudioListener>();

            CameraController controller = cameraGo.AddComponent<CameraController>();
            SerializedObject serialized = new SerializedObject(controller);
            SetFloat(serialized, "targetOrthoSize", 5.5f);
            SetFloat(serialized, "smoothSpeed", 6f);
            SetVector3(serialized, "offset", new Vector3(0f, 1.5f, -10f));
            SetFloat(serialized, "minY", -2f);
            SetFloat(serialized, "maxY", 16f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return controller;
        }

        private static AudioManager CreateAudioManager(Transform parent)
        {
            GameObject audioGo = InstantiatePrefab($"{PrefabsDir}/AudioManager.prefab", parent, Vector3.zero);
            return audioGo != null ? audioGo.GetComponent<AudioManager>() : null;
        }

        private static LevelStartPoint CreateLevelStart(Transform parent)
        {
            return CreateLevelStart(parent, new Vector3(3f, 4.1f, 0f));
        }

        /// <summary>
        /// Đặt Bệ Xuất Phát và tách 2 điểm hồi sinh P1/P2 ra hai bên
        /// (prefab gốc để cả hai trùng at (0,0,0) nên 2P sẽ chồng nhau nếu không tách).
        /// </summary>
        private static LevelStartPoint CreateLevelStart(Transform parent, Vector3 position)
        {
            GameObject startGo = InstantiatePrefab($"{PrefabsDir}/LevelStart.prefab", parent, position);
            if (startGo == null)
            {
                return null;
            }

            Transform p1Spawn = startGo.transform.Find("P1SpawnPoint");
            Transform p2Spawn = startGo.transform.Find("P2SpawnPoint");

            if (p1Spawn != null)
            {
                p1Spawn.localPosition = new Vector3(-0.75f, 0f, 0f);
            }

            if (p2Spawn != null)
            {
                p2Spawn.localPosition = new Vector3(0.75f, 0f, 0f);
            }

            return startGo.GetComponent<LevelStartPoint>();
        }

        private static GameManager CreateGameManager(
            Transform parent,
            LevelStartPoint startPoint,
            ExitGate exitGate,
            CameraController cameraController,
            AudioManager audioManager,
            Module5UiRig ui,
            int levelIndex)
        {
            GameObject managerGo = new GameObject("GameManager");
            managerGo.transform.SetParent(parent);
            managerGo.transform.position = Vector3.zero;

            GameManager manager = managerGo.AddComponent<GameManager>();

            GameObject player1Prefab = LoadPrefab($"{PrefabsDir}/Player1.prefab");
            GameObject player2Prefab = LoadPrefab($"{PrefabsDir}/Player2.prefab");
            GameObject soulTetherPrefab = LoadPrefab($"{PrefabsDir}/SoulTether.prefab");

            SerializedObject serialized = new SerializedObject(manager);
            SetObject(serialized, "startPoint", startPoint);
            SetObject(serialized, "exitGate", exitGate);
            SetObject(serialized, "player1Prefab", player1Prefab != null ? player1Prefab.GetComponent<PlayerController>() : null);
            SetObject(serialized, "player2Prefab", player2Prefab != null ? player2Prefab.GetComponent<PlayerController>() : null);
            SetObject(serialized, "soulTetherPrefab", soulTetherPrefab != null ? soulTetherPrefab.GetComponent<SoulTether>() : null);
            SetObject(serialized, "mainCamera", cameraController);
            SetObject(serialized, "audioManager", audioManager);
            SetObject(serialized, "hudController", ui.Hud);
            SetObject(serialized, "pauseMenu", ui.Pause);
            SetObject(serialized, "irisTransition", ui.Iris);
            SetInt(serialized, "levelIndexOverride", levelIndex);
            SetInt(serialized, "totalRelicsRequired", 3);
            SetFloat(serialized, "respawnDelay", 0.18f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return manager;
        }

        #endregion

        #region Rig UI Module 5 — Canvas / HUD / PauseMenu / IrisTransition

        /// <summary>Bộ tham chiếu UI do builder sinh ra, dùng để nối vào GameManager.</summary>
        private struct Module5UiRig
        {
            public HUDController Hud;
            public PauseMenu Pause;
            public IrisTransition Iris;
            public GameObject PausePanel;
        }

        /// <summary>
        /// Dựng Canvas Screen Space Overlay + EventSystem (InputSystemUIInputModule) + HUD + PauseMenu + Iris.
        /// Đây là lần đầu dự án có UI trong Scene nên toàn bộ rig được tạo tại đây.
        /// </summary>
        private static Module5UiRig BuildModule5Ui(Transform parent)
        {
            Module5UiRig rig = new Module5UiRig();

            // --- Canvas ---
            GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(parent, false);

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // --- EventSystem (chỉ New Input System: activeInputHandler = 1) ---
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(parent, false);
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            // --- HUD ---
            GameObject hudGo = CreateUiRoot("HUD", canvasGo.transform);
            HUDController hud = hudGo.AddComponent<HUDController>();

            Image relicIcon = CreateUiImage("RelicIcon", hudGo.transform, pineappleSprite, true);
            PlaceTopLeft((RectTransform)relicIcon.transform, 40f, 40f, 72f, 72f);

            Text relicCounter = CreateUiText("RelicCounter", hudGo.transform, 48, TextAnchor.MiddleLeft, new Color(1f, 0.95f, 0.8f, 1f), "[0/3]");
            PlaceTopLeft((RectTransform)relicCounter.transform, 124f, 46f, 220f, 60f);

            Text levelText = CreateUiText("LevelText", hudGo.transform, 44, TextAnchor.MiddleRight, new Color(1f, 0.95f, 0.8f, 1f), "LEVEL 05");
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

            // --- PauseMenu (component nằm trên root, panel là con để SetActive không tắt script) ---
            GameObject pauseRootGo = CreateUiRoot("PauseMenu", canvasGo.transform);
            PauseMenu pauseMenu = pauseRootGo.AddComponent<PauseMenu>();

            GameObject pausePanelGo = CreateUiRoot("PausePanel", pauseRootGo.transform);
            RectTransform pausePanelRect = (RectTransform)pausePanelGo.transform;
            StretchFullScreen(pausePanelRect);

            Image pauseBackdrop = pausePanelGo.AddComponent<Image>();
            pauseBackdrop.color = new Color(0.03f, 0.02f, 0.07f, 0.82f);
            pauseBackdrop.raycastTarget = true;

            Text pauseTitle = CreateUiText("PauseTitle", pausePanelGo.transform, 96, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.35f, 1f), "TẠM DỪNG");
            RectTransform pauseTitleRect = (RectTransform)pauseTitle.transform;
            CenterAt(pauseTitleRect, 0f, 250f, 900f, 120f);

            Button resumeButton = CreateSolidButton("ResumeButton", pausePanelGo.transform, playIconSprite, "TIẾP TỤC", 60f);
            Button pauseRestartButton = CreateSolidButton("PauseRestartButton", pausePanelGo.transform, restartIconSprite, "CHƠI LẠI", -40f);
            Button mainMenuButton = CreateSolidButton("MainMenuButton", pausePanelGo.transform, settingsIconSprite, "VỀ MENU CHÍNH", -140f);

            SerializedObject pauseSerialized = new SerializedObject(pauseMenu);
            SetObject(pauseSerialized, "pausePanel", pausePanelGo);
            SetObject(pauseSerialized, "resumeButton", resumeButton);
            SetObject(pauseSerialized, "restartButton", pauseRestartButton);
            SetObject(pauseSerialized, "mainMenuButton", mainMenuButton);
            pauseSerialized.ApplyModifiedPropertiesWithoutUndo();

            // --- IrisTransition (component ở root, Image là con vì HideIris() tắt chính GameObject của Image) ---
            GameObject irisRootGo = CreateUiRoot("IrisTransition", canvasGo.transform);
            IrisTransition iris = irisRootGo.AddComponent<IrisTransition>();

            Image irisImage = CreateUiImage("IrisMask", irisRootGo.transform, transitionSprite, false);
            RectTransform irisRect = (RectTransform)irisImage.transform;
            CenterAt(irisRect, 0f, 0f, 44f, 44f);

            SerializedObject irisSerialized = new SerializedObject(iris);
            SetObject(irisSerialized, "irisImage", irisImage);
            SetFloat(irisSerialized, "closeDuration", 0.35f);
            SetFloat(irisSerialized, "openDuration", 0.35f);
            SetFloat(irisSerialized, "overshoot", 1.6f);
            irisSerialized.ApplyModifiedPropertiesWithoutUndo();

            rig.Hud = hud;
            rig.Pause = pauseMenu;
            rig.Iris = iris;
            rig.PausePanel = pausePanelGo;

            return rig;
        }

        /// <summary>Nối HUD.pauseMenu -> PauseMenu sau khi cả hai đã tồn tại.</summary>
        private static void WireUiReferences(Module5UiRig ui, GameManager gameManager)
        {
            if (ui.Hud == null || ui.Pause == null)
            {
                return;
            }

            SerializedObject hudSerialized = new SerializedObject(ui.Hud);
            SetObject(hudSerialized, "pauseMenu", ui.Pause);
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            if (gameManager != null)
            {
                Debug.Log("<b>[Module5SceneBuilder]</b> Đã nối HUD ↔ PauseMenu ↔ GameManager.");
            }
        }

        #endregion

        #region Helper dựng UI (uGUI thuần — không phụ thuộc TMP Essentials)

        private static GameObject CreateUiRoot(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;

            RectTransform rect = (RectTransform)go.transform;
            StretchFullScreen(rect);

            return go;
        }

        private static void StretchFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static Image CreateUiImage(string name, Transform parent, Sprite sprite, bool preserveAspect)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;

            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = false;

            return image;
        }

        private static Text CreateUiText(string name, Transform parent, int fontSize, TextAnchor alignment, Color color, string initialText)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;

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

        private static Button CreateIconButton(string name, Transform parent, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;

            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = true;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;

            return button;
        }

        private static Button CreateSolidButton(string name, Transform parent, Sprite icon, string label, float y)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;

            Image background = go.AddComponent<Image>();
            background.color = new Color(0.16f, 0.13f, 0.24f, 0.96f);
            background.raycastTarget = true;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = background;

            RectTransform rect = (RectTransform)go.transform;
            CenterAt(rect, 0f, y, 620f, 84f);

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

        private static void PlaceTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void PlaceTopRight(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void CenterAt(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        #endregion

        #region Biển hiệu trong màn

        private static void CreateSigns(Transform parent)
        {
            CreateLabel("L5_Sign_S1", "MÀN 5: CHAOS NEXUS — [KHỞI ĐẦU]", new Vector3(6f, 12.5f, 0f), parent);
            CreateLabel("L5_Sign_S2", "MÀN 5: CHAOS NEXUS — [HÀNH LANG CON LẮC GIAO NHAU]", new Vector3(47f, 11.5f, 0f), parent);
            CreateLabel("L5_Sign_S3", "MÀN 5: CHAOS NEXUS — [BỆ SẬP VĨNH VIỄN]", new Vector3(89f, 12.5f, 0f), parent);
            CreateLabel("L5_Sign_S4", "MÀN 5: CHAOS NEXUS — [HỐ TỬ THẦN & MŨI TÊN ĐẨY KHÍ]", new Vector3(143f, 12.5f, 0f), parent);
            CreateLabel("L5_Sign_S5", "MÀN 5: CHAOS NEXUS — [ĐIỆN THỜ CỔNG THẦN]", new Vector3(184f, 11.5f, 0f), parent);
        }

        private static void CreateLabel(string name, string content, Vector3 position, Transform parent)
        {
            GameObject labelGo = new GameObject(name);
            labelGo.transform.SetParent(parent);
            labelGo.transform.position = position;

            TextMesh mesh = labelGo.AddComponent<TextMesh>();
            mesh.text = content;
            mesh.fontSize = 24;
            mesh.characterSize = 0.08f;
            mesh.color = new Color(1f, 0.95f, 0.8f, 0.95f);
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
        }

        #endregion

        #region SerializedObject helpers

        private static void SetObject(SerializedObject serialized, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[Module5SceneBuilder] Không tìm thấy property '{propertyName}' trên {serialized.targetObject.GetType().Name}!");
                return;
            }

            property.objectReferenceValue = value;
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[Module5SceneBuilder] Không tìm thấy property '{propertyName}' trên {serialized.targetObject.GetType().Name}!");
                return;
            }

            property.floatValue = value;
        }

        private static void SetInt(SerializedObject serialized, string propertyName, int value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[Module5SceneBuilder] Không tìm thấy property '{propertyName}' trên {serialized.targetObject.GetType().Name}!");
                return;
            }

            property.intValue = value;
        }

        private static void SetBool(SerializedObject serialized, string propertyName, bool value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[Module5SceneBuilder] Không tìm thấy property '{propertyName}' trên {serialized.targetObject.GetType().Name}!");
                return;
            }

            property.boolValue = value;
        }

        private static void SetVector2(SerializedObject serialized, string propertyName, Vector2 value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[Module5SceneBuilder] Không tìm thấy property '{propertyName}' trên {serialized.targetObject.GetType().Name}!");
                return;
            }

            property.vector2Value = value;
        }

        private static void SetVector3(SerializedObject serialized, string propertyName, Vector3 value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[Module5SceneBuilder] Không tìm thấy property '{propertyName}' trên {serialized.targetObject.GetType().Name}!");
                return;
            }

            property.vector3Value = value;
        }

        #endregion

        #region Kiểm tra hợp lệ Scene

        /// <summary>
        /// Mở Scene và xác minh toàn bộ rig Module 5 đã được gắn đúng:
        /// GameManager đủ 12 tham chiếu, Canvas + EventSystem tồn tại, HUD/Pause/Iris nối chéo đủ.
        /// Dùng để tự kiểm tra sau khi dựng (menu Tools hoặc -executeMethod ValidateFromCommandLine).
        /// </summary>
        public static void ValidateScene(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogError($"[Module5SceneBuilder] Không có Scene để kiểm tra: {scenePath}");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int errorCount = 0;

            // --- 1. GameManager & 12 tham chiếu ---
            GameManager gameManager = FindInScene<GameManager>(scene);
            if (gameManager == null)
            {
                Debug.LogError("[Validate] Thiếu GameManager trong Scene.");
                errorCount++;
            }
            else
            {
                SerializedObject serialized = new SerializedObject(gameManager);
                string[] requiredReferences =
                {
                    "startPoint", "exitGate", "player1Prefab", "player2Prefab", "soulTetherPrefab",
                    "mainCamera", "hudController", "pauseMenu", "irisTransition", "audioManager"
                };

                foreach (string referenceName in requiredReferences)
                {
                    SerializedProperty property = serialized.FindProperty(referenceName);
                    if (property == null || property.objectReferenceValue == null)
                    {
                        Debug.LogError($"[Validate] GameManager.{referenceName} đang TRỐNG.");
                        errorCount++;
                    }
                }

                // Chỉ số màn phải được ép đúng để HUD hiện LEVEL 05 và BGM phát bgm_level5_chaos_nexus
                // khi mở Scene trực tiếp (không đi qua MainMenu/LevelSelectMenu).
                SerializedProperty levelOverride = serialized.FindProperty("levelIndexOverride");
                if (levelOverride == null)
                {
                    Debug.LogError("[Validate] GameManager thiếu field levelIndexOverride.");
                    errorCount++;
                }
                else if (levelOverride.intValue != 5)
                {
                    Debug.LogError($"[Validate] GameManager.levelIndexOverride = {levelOverride.intValue}, cần 5.");
                    errorCount++;
                }
            }

            // --- 2. Canvas + EventSystem ---
            Canvas canvas = FindInScene<Canvas>(scene);
            if (canvas == null)
            {
                Debug.LogError("[Validate] Thiếu Canvas trong Scene.");
                errorCount++;
            }
            else if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError($"[Validate] Canvas.renderMode = {canvas.renderMode}, cần ScreenSpaceOverlay.");
                errorCount++;
            }

            EventSystem eventSystem = FindInScene<EventSystem>(scene);
            if (eventSystem == null)
            {
                Debug.LogError("[Validate] Thiếu EventSystem trong Scene.");
                errorCount++;
            }
            else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                Debug.LogError("[Validate] EventSystem thiếu InputSystemUIInputModule (activeInputHandler = 1).");
                errorCount++;
            }

            // --- 3. HUD / PauseMenu / IrisTransition nối chéo ---
            HUDController hud = FindInScene<HUDController>(scene);
            PauseMenu pause = FindInScene<PauseMenu>(scene);
            IrisTransition iris = FindInScene<IrisTransition>(scene);

            if (hud == null)
            {
                Debug.LogError("[Validate] Thiếu HUDController.");
                errorCount++;
            }
            else
            {
                SerializedObject hudSerialized = new SerializedObject(hud);
                string[] hudReferences =
                {
                    "relicIcon", "relicCounterText", "levelText", "timeText",
                    "modeText", "deathsText", "restartButton", "pauseButton", "pauseMenu"
                };

                foreach (string referenceName in hudReferences)
                {
                    SerializedProperty property = hudSerialized.FindProperty(referenceName);
                    if (property == null || property.objectReferenceValue == null)
                    {
                        Debug.LogError($"[Validate] HUDController.{referenceName} đang TRỐNG.");
                        errorCount++;
                    }
                }
            }

            if (pause == null)
            {
                Debug.LogError("[Validate] Thiếu PauseMenu.");
                errorCount++;
            }
            else
            {
                SerializedObject pauseSerialized = new SerializedObject(pause);
                string[] pauseReferences = { "pausePanel", "resumeButton", "restartButton", "mainMenuButton" };

                foreach (string referenceName in pauseReferences)
                {
                    SerializedProperty property = pauseSerialized.FindProperty(referenceName);
                    if (property == null || property.objectReferenceValue == null)
                    {
                        Debug.LogError($"[Validate] PauseMenu.{referenceName} đang TRỐNG.");
                        errorCount++;
                    }
                }
            }

            if (iris == null)
            {
                Debug.LogError("[Validate] Thiếu IrisTransition.");
                errorCount++;
            }
            else
            {
                SerializedObject irisSerialized = new SerializedObject(iris);
                SerializedProperty irisImageProperty = irisSerialized.FindProperty("irisImage");
                if (irisImageProperty == null || irisImageProperty.objectReferenceValue == null)
                {
                    Debug.LogError("[Validate] IrisTransition.irisImage đang TRỐNG.");
                    errorCount++;
                }
            }

            // --- 4. Nội dung màn: 3 Cổ Vật + Cổng Thần + Bẫy đặc trưng ---
            int relicCount = 0;
            int bonusCount = 0;
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                foreach (OneHitDown.Items.CollectibleItem item in rootObject.GetComponentsInChildren<OneHitDown.Items.CollectibleItem>(true))
                {
                    SerializedObject itemSerialized = new SerializedObject(item);
                    SerializedProperty countsAsRelicProperty = itemSerialized.FindProperty("countsAsRelic");
                    if (countsAsRelicProperty != null && countsAsRelicProperty.boolValue)
                    {
                        relicCount++;
                    }
                    else
                    {
                        bonusCount++;
                    }
                }
            }

            if (relicCount != 3)
            {
                Debug.LogError($"[Validate] Số Cổ Vật Chính = {relicCount}, đặc tả yêu cầu đúng 3 (SCHEMA dòng 72).");
                errorCount++;
            }

            if (bonusCount != 2)
            {
                Debug.LogError($"[Validate] Số Quả Thưởng Phụ = {bonusCount}, đặc tả Màn 5 yêu cầu đúng 2 (Melon & Strawberry).");
                errorCount++;
            }

            int pendulumCount = CountInScene<SpikedBallPendulum>(scene);
            if (pendulumCount < 2)
            {
                Debug.LogError($"[Validate] Số con lắc SpikedBall = {pendulumCount}, Màn 5 cần 2 con lắc giao nhau.");
                errorCount++;
            }

            // --- 5. Build Settings ---
            bool inBuildSettings = false;
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (buildScene.path == scenePath)
                {
                    inBuildSettings = true;
                    break;
                }
            }

            if (!inBuildSettings)
            {
                Debug.LogError($"[Validate] Scene '{scenePath}' chưa có trong Build Settings → Application.CanStreamedLevelBeLoaded sẽ trả false.");
                errorCount++;
            }

            if (errorCount == 0)
            {
                Debug.Log($"<b>[Validate]</b> <color=green>PASS</color> — {scenePath}: 3 Cổ Vật, {bonusCount} Quả Thưởng, {pendulumCount} con lắc, rig Module 5 đầy đủ.");
            }
            else
            {
                Debug.LogError($"<b>[Validate]</b> <color=red>FAIL</color> — {scenePath}: {errorCount} lỗi.");
            }
        }

        /// <summary>Điểm vào batch mode cho bước kiểm tra Scene.</summary>
        public static void ValidateFromCommandLine()
        {
            ValidateScene(ScenePath);
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                T found = rootObject.GetComponentInChildren<T>(true);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static int CountInScene<T>(Scene scene) where T : Component
        {
            int total = 0;
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                total += rootObject.GetComponentsInChildren<T>(true).Length;
            }

            return total;
        }

        #endregion

        #region Build Settings & thư mục

        /// <summary>Thêm Scene vào Build Settings để Application.CanStreamedLevelBeLoaded trả về true.</summary>
        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (EditorBuildSettingsScene existing in scenes)
            {
                if (existing.path == scenePath)
                {
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[Module5SceneBuilder] Đã thêm '{scenePath}' vào Build Settings.");
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
