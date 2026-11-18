// ====================================================================================================
// SCRIPT UNITY EDITOR: XÂY DỰNG SCENE MÀN 4 'Assets/Scenes/Level_04.unity' (SANCTUARY OF CRUSHING IDOLS)
// Người 4 — Phan Văn Tú (2351260693) — Module 4.
// Tuân thủ 100%: Docs/SCHEMA_CHI_TIET.md §3 Module 4 + §4 Bảng Prefab + §5 Quy tắc Scene.
//                 Docs/Phan_cong.md Mục III (Người 4) & CLAUDE.md.
//
// MỤC ĐÍCH:
//   1. Dựng trọn vẹn Màn 4 (Điện Thờ Thần Tượng / Tượng Đá Nghiền - Sanctuary of Crushing Idols) dài ~105 units:
//      - 3 Quả Cổ Vật Chính Orange (countsAsRelic = true) để mở Cổng Thần [3/3].
//      - 1 Quả Thưởng Phụ Cherries (countsAsRelic = false).
//      - Bẫy đặc trưng Màn 4: Tượng đá dập RockHead (cảm biến 0.25s), Khối đá gai SpikeHead (phóng ngang hẹp),
//        Cối xay quạt gió FanTrap thổi ngang ngược chiều (windDirection = (-1, 0), windForce = 15f),
//        Bệ nâng di động MovingPlatform, chông gai Spikes và hố sập DeathPit.
//   2. Gắn TRỌN BỘ HẠ TẦNG CỐT LÕI (CoreRig):
//      GameManager (levelIndexOverride = 4, totalRelicsRequired = 3) · CameraController · AudioManager ·
//      HUDController · PauseMenu · IrisTransition kèm Canvas + EventSystem.
//   3. Tự động thêm 'Assets/Scenes/Level_04.unity' vào EditorBuildSettings đúng vị trí giữa Level_03 và Level_05.
//
// Menu Editor: Tools -> One Hit Down -> Build Level 04 Scene (Sanctuary of Crushing Idols)
// Batch mode : -executeMethod OneHitDown.EditorTools.Level04SceneBuilder.BuildFromCommandLine
// ====================================================================================================

#if UNITY_EDITOR
using System;
using System.IO;
using OneHitDown.Core;
using OneHitDown.Items;
using OneHitDown.Level;
using OneHitDown.Player;
using OneHitDown.Traps;
using OneHitDown.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Script Editor tự động hóa việc dựng hoàn chỉnh Scene 'Assets/Scenes/Level_04.unity'
    /// theo thiết kế Điện Thờ Tượng Đá Nghiền (Sanctuary of Crushing Idols).
    /// </summary>
    public static class Level04SceneBuilder
    {
        private const string SceneDir = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/Level_04.unity";
        private const string DevSceneDir = "Assets/Scenes/Dev";
        private const string DevScenePath = "Assets/Scenes/Dev/Dev_Nguoi4.unity";
        private const string LogTag = "<b>[Level04SceneBuilder]</b>";

        // Asset Paths
        private const string BgGrayPath = "Assets/Art/02_MoiTruong/PA1_Background/Gray.png";
        private const string TerrainPath = "Assets/Art/02_MoiTruong/PA1_Terrain/Terrain (16x16).png";

        // Khối đá xám 9-slice từ Terrain (16x16).png
        private const string StoneBlockSpriteName = "Terrain (16x16)_StoneBlock";
        private const string StoneInnerBricksName = "Terrain (16x16)_StoneInnerBricks";

        // Prefab Paths
        private const string CoreRigPrefabPath = "Assets/Prefabs/CoreRig.prefab";
        private const string AudioManagerPrefabPath = "Assets/Prefabs/AudioManager.prefab";
        private const string LevelStartPrefabPath = "Assets/Prefabs/LevelStart.prefab";
        private const string ExitGatePrefabPath = "Assets/Prefabs/ExitGate.prefab";

        private const string Player1PrefabPath = "Assets/Prefabs/Player1.prefab";
        private const string Player2PrefabPath = "Assets/Prefabs/Player2.prefab";
        private const string SoulTetherPrefabPath = "Assets/Prefabs/SoulTether.prefab";

        private const string OrangePrefabPath = "Assets/Prefabs/Fruit_Orange.prefab";
        private const string CherriesPrefabPath = "Assets/Prefabs/Fruit_Cherries.prefab";

        private const string RockHeadPrefabPath = "Assets/Prefabs/RockHead.prefab";
        private const string SpikeHeadPrefabPath = "Assets/Prefabs/SpikeHead.prefab";
        private const string MovingPlatformPrefabPath = "Assets/Prefabs/MovingPlatform.prefab";
        private const string FanTrapPrefabPath = "Assets/Prefabs/Traps/FanTrap.prefab";
        private const string SpikesPrefabPath = "Assets/Prefabs/Spikes.prefab";
        private const string DeathPitPrefabPath = "Assets/Prefabs/Traps/DeathPit.prefab";

        private const float FloorBottomY = -6f;

        [MenuItem("Tools/One Hit Down/Build Level 04 Scene", false, 4)]
        public static void BuildScene()
        {
            BuildSceneInternal();
        }

        [MenuItem("Tools/One Hit Down/Build Dev_Nguoi4 Scene", false, 20)]
        public static void BuildDevScene()
        {
            BuildDevSceneInternal();
        }

        /// <summary>Dựng Scene nháp Dev_Nguoi4.unity để test riêng các bẫy, cơ quan và quả Module 4.</summary>
        public static bool BuildDevSceneInternal()
        {
            try
            {
                if (!Directory.Exists(DevSceneDir))
                {
                    Directory.CreateDirectory(DevSceneDir);
                }

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgGrayPath);
                Sprite stoneSprite = LoadSprite(TerrainPath, StoneBlockSpriteName);
                if (stoneSprite == null) stoneSprite = LoadSprite(TerrainPath, "Tile_16x16_X00_Y00");

                GameObject envGroup = new GameObject("--- 01. ENVIRONMENT ---");
                GameObject bgGo = new GameObject("Background");
                bgGo.transform.SetParent(envGroup.transform);
                bgGo.transform.position = new Vector3(25f, 5f, 5f);
                SpriteRenderer bgSr = bgGo.AddComponent<SpriteRenderer>();
                bgSr.sprite = bgSprite;
                bgSr.drawMode = SpriteDrawMode.Tiled;
                bgSr.size = new Vector2(70f, 25f);
                bgSr.sortingOrder = -100;

                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.transform.SetParent(envGroup.transform);
                camGo.transform.position = new Vector3(3.5f, 2.0f, -10f);
                Camera cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.backgroundColor = new Color(0.18f, 0.18f, 0.22f, 1f);
                camGo.AddComponent<AudioListener>();
                CameraController camCtrl = camGo.AddComponent<CameraController>();

                GameObject tg = new GameObject("--- 02. TERRAIN ---");
                BuildStoneBlock(tg.transform, "Floor", 0, 50, 0f, -4f, stoneSprite);
                BuildStoneBlock(tg.transform, "Wall_Left", -2, -1, 10f, -4f, stoneSprite);
                BuildStoneBlock(tg.transform, "Wall_Right", 51, 52, 10f, -4f, stoneSprite);
                BuildStoneBlock(tg.transform, "Ledge", 20, 26, 4f, 3f, stoneSprite);
                BuildStoneBlock(tg.transform, "Ceiling", 12, 16, 10f, 8f, stoneSprite);

                GameObject trg = new GameObject("--- 03. TRAPS ---");
                GameObject rockHeadPrefab = LoadPrefab(RockHeadPrefabPath);
                SpawnRockHead(rockHeadPrefab, trg.transform, "Dev_RockHead", new Vector3(14f, 6.5f, 0f));

                GameObject spikeHeadPrefab = LoadPrefab(SpikeHeadPrefabPath);
                SpawnSpikeHead(spikeHeadPrefab, trg.transform, "Dev_SpikeHead", new Vector3(35f, 1.5f, 0f), Vector2.left, 4f);

                GameObject fanPrefab = LoadPrefab(FanTrapPrefabPath);
                SpawnFanTrap(fanPrefab, trg.transform, "Dev_FanTrap", new Vector3(45f, 1.5f, 0f), Vector2.left, 15f);

                GameObject platPrefab = LoadPrefab(MovingPlatformPrefabPath);
                SpawnMovingPlatform(platPrefab, trg.transform, "Dev_MovingPlatform", new Vector3(22f, 1.5f, 0f), Vector2.zero, new Vector2(5f, 0f), 2.5f);

                GameObject ig = new GameObject("--- 04. ITEMS ---");
                LevelStartPoint startPoint = null;
                GameObject startPrefab = LoadPrefab(LevelStartPrefabPath);
                if (startPrefab != null)
                {
                    GameObject startGo = (GameObject)PrefabUtility.InstantiatePrefab(startPrefab);
                    startGo.transform.SetParent(ig.transform);
                    startGo.transform.position = new Vector3(2.5f, 0f, 0f);
                    startPoint = startGo.GetComponentInChildren<LevelStartPoint>(true);
                }

                ExitGate exitGate = null;
                GameObject exitPrefab = LoadPrefab(ExitGatePrefabPath);
                if (exitPrefab != null)
                {
                    GameObject exitGo = (GameObject)PrefabUtility.InstantiatePrefab(exitPrefab);
                    exitGo.transform.SetParent(ig.transform);
                    exitGo.transform.position = new Vector3(48f, 0f, 0f);
                    exitGate = exitGo.GetComponentInChildren<ExitGate>(true);
                }

                GameObject orangePrefab = LoadPrefab(OrangePrefabPath);
                SpawnFruit(orangePrefab, ig.transform, "Fruit_Orange_1", new Vector3(14f, 1f, 0f), true);
                SpawnFruit(orangePrefab, ig.transform, "Fruit_Orange_2", new Vector3(23f, 5f, 0f), true);
                SpawnFruit(orangePrefab, ig.transform, "Fruit_Orange_3", new Vector3(42f, 1f, 0f), true);

                GameObject coreGroup = new GameObject("--- 00. CORE ---");
                GameObject coreRigPrefab = LoadPrefab(CoreRigPrefabPath);
                if (coreRigPrefab != null)
                {
                    GameObject coreRigGo = (GameObject)PrefabUtility.InstantiatePrefab(coreRigPrefab);
                    coreRigGo.transform.SetParent(coreGroup.transform);
                    GameManager gm = coreRigGo.GetComponentInChildren<GameManager>(true);
                    if (gm != null)
                    {
                        SerializedObject gmSo = new SerializedObject(gm);
                        gmSo.FindProperty("startPoint").objectReferenceValue = startPoint;
                        gmSo.FindProperty("exitGate").objectReferenceValue = exitGate;
                        GameObject p1Prefab = LoadPrefab(Player1PrefabPath);
                        GameObject p2Prefab = LoadPrefab(Player2PrefabPath);
                        GameObject tetherPrefab = LoadPrefab(SoulTetherPrefabPath);
                        if (p1Prefab != null) gmSo.FindProperty("player1Prefab").objectReferenceValue = p1Prefab.GetComponent<PlayerController>();
                        if (p2Prefab != null) gmSo.FindProperty("player2Prefab").objectReferenceValue = p2Prefab.GetComponent<PlayerController>();
                        if (tetherPrefab != null) gmSo.FindProperty("soulTetherPrefab").objectReferenceValue = tetherPrefab.GetComponent<SoulTether>();
                        gmSo.FindProperty("levelIndexOverride").intValue = 4;
                        gmSo.FindProperty("totalRelicsRequired").intValue = 3;
                        gmSo.FindProperty("mainCamera").objectReferenceValue = camCtrl;
                        gmSo.ApplyModifiedProperties();
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                bool saved = EditorSceneManager.SaveScene(scene, DevScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"{LogTag} ĐÃ DỰNG THÀNH CÔNG SCENE NHÁP TẠI: {DevScenePath}");
                return saved;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogTag} Lỗi dựng Dev_Nguoi4: {ex.Message}");
                return false;
            }
        }

        /// <summary>Dựng toàn bộ Scene; trả về true nếu lưu thành công.</summary>
        public static bool BuildSceneInternal()
        {
            try
            {
                if (File.Exists(ScenePath) && !Application.isBatchMode)
                {
                    if (!EditorUtility.DisplayDialog(
                        "Xác nhận ghi đè Level 04",
                        "Màn chơi 'Level_04.unity' đã tồn tại.\nBạn có chắc chắn muốn DỰNG LẠI và GHI ĐÈ toàn bộ không?",
                        "Ghi đè",
                        "Hủy bỏ (Bảo vệ Scene)"))
                    {
                        Debug.Log($"{LogTag} Đã hủy lệnh dựng lại để bảo vệ Level 04 hiện tại.");
                        return false;
                    }
                }

                EditorUtility.DisplayProgressBar("One Hit Down", "Đang dựng Scene Level_04 (Sanctuary of Crushing Idols)...", 0.1f);
                Debug.Log($"{LogTag} BẮT ĐẦU DỰNG SCENE LEVEL_04.UNITY...");

                if (!Directory.Exists(SceneDir))
                {
                    Directory.CreateDirectory(SceneDir);
                }

                // 1. Scene trống
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // 2. Sprite nền và địa hình đá điện thờ
                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgGrayPath);
                Sprite stoneSprite = LoadSprite(TerrainPath, StoneBlockSpriteName);
                if (stoneSprite == null) stoneSprite = LoadSprite(TerrainPath, "Tile_16x16_X00_Y00");

                // 3. --- 01. ENVIRONMENT & BACKGROUND ---
                GameObject envGroup = new GameObject("--- 01. ENVIRONMENT & BACKGROUND ---");

                GameObject bgGo = new GameObject("Background_Gray");
                bgGo.transform.SetParent(envGroup.transform);
                bgGo.transform.position = new Vector3(50f, 6f, 5f);
                SpriteRenderer bgSr = bgGo.AddComponent<SpriteRenderer>();
                bgSr.sprite = bgSprite;
                bgSr.drawMode = SpriteDrawMode.Tiled;
                bgSr.size = new Vector2(130f, 35f);
                bgSr.sortingOrder = -100;

                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.transform.SetParent(envGroup.transform);
                camGo.transform.position = new Vector3(3.5f, 2.0f, -10f);
                Camera cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.backgroundColor = new Color(0.18f, 0.18f, 0.22f, 1f); // Xám đá điện thờ
                camGo.AddComponent<AudioListener>();

                CameraController camCtrl = camGo.AddComponent<CameraController>();
                SerializedObject camSo = new SerializedObject(camCtrl);
                camSo.FindProperty("targetOrthoSize").floatValue = 5.5f;
                camSo.FindProperty("smoothSpeed").floatValue = 6f;
                camSo.FindProperty("offset").vector3Value = new Vector3(0f, 1.5f, -10f);
                camSo.FindProperty("minY").floatValue = -3f;
                camSo.FindProperty("maxY").floatValue = 16f;
                camSo.FindProperty("autoFindInterval").floatValue = 0.25f;
                camSo.ApplyModifiedProperties();

                EditorUtility.DisplayProgressBar("One Hit Down", "Dựng địa hình điện thờ đá...", 0.3f);

                // 4. --- 02. TERRAIN & ARCHITECTURE ---
                GameObject terrainGroup = new GameObject("--- 02. TERRAIN & ARCHITECTURE ---");
                Transform tg = terrainGroup.transform;

                // Vách chắn hai đầu màn (chống rơi khỏi map)
                BuildStoneBlock(tg, "Wall_Left", -2, -1, 14f, FloorBottomY, stoneSprite);
                BuildStoneBlock(tg, "Wall_Right", 102, 103, 14f, FloorBottomY, stoneSprite);

                // SÀN ĐIỆN THỜ:
                // Chặng A: Sàn xuất phát & khu Tượng đá 1 (x: 0..26, y: 0)
                BuildStoneBlock(tg, "Floor_A_Start", 0, 26, 0f, FloorBottomY, stoneSprite);

                // Trần treo tượng đá Chặng A
                BuildStoneBlock(tg, "Ceiling_A_RockHead", 10, 16, 12f, 8f, stoneSprite);

                // Chặng B: Bậc gờ bước đệm (x: 29..32, y: 1.5)
                BuildStoneBlock(tg, "Floor_B_Plinth", 29, 32, 1.5f, FloorBottomY, stoneSprite);

                // Bờ vực bên kia hố sâu Chặng B (x: 44..56, y: 0)
                BuildStoneBlock(tg, "Floor_B_PlatformLanding", 44, 56, 0f, FloorBottomY, stoneSprite);

                // Bệ cao chứa Cổ vật 2 (x: 48..52, y: 5.5, dày 1.5)
                BuildStoneBlock(tg, "Ledge_Relic_2", 48, 52, 5.5f, 4f, stoneSprite);

                // Trần treo tượng đá Chặng B
                BuildStoneBlock(tg, "Ceiling_B_RockHead", 47, 53, 13f, 9.5f, stoneSprite);

                // Chặng C: Hành lang quạt gió & chông gai (x: 60..82, y: 0)
                BuildStoneBlock(tg, "Floor_C_WindAlley", 60, 82, 0f, FloorBottomY, stoneSprite);

                // Trần thấp ép người chơi chạy dưới quạt gió Chặng C
                BuildStoneBlock(tg, "Ceiling_C_LowTunnel", 64, 76, 7.5f, 6.0f, stoneSprite);

                // Chặng D: Sàn dẫn vào Cổng Thần (x: 85..101, y: 0)
                BuildStoneBlock(tg, "Floor_D_ExitAltar", 85, 101, 0f, FloorBottomY, stoneSprite);

                // Trần treo tượng đá phòng thủ trước cổng
                BuildStoneBlock(tg, "Ceiling_D_RockHead", 88, 93, 12f, 8.5f, stoneSprite);

                EditorUtility.DisplayProgressBar("One Hit Down", "Đặt bẫy Tượng Đá, Khối Gai và Quạt Gió...", 0.5f);

                // 5. --- 03. TRAPS & HAZARDS ---
                GameObject trapGroup = new GameObject("--- 03. TRAPS & HAZARDS ---");
                Transform trg = trapGroup.transform;

                GameObject rockHeadPrefab = LoadPrefab(RockHeadPrefabPath);
                GameObject spikeHeadPrefab = LoadPrefab(SpikeHeadPrefabPath);
                GameObject fanPrefab = LoadPrefab(FanTrapPrefabPath);
                GameObject platformPrefab = LoadPrefab(MovingPlatformPrefabPath);
                GameObject spikesPrefab = LoadPrefab(SpikesPrefabPath);
                GameObject deathPitPrefab = LoadPrefab(DeathPitPrefabPath);

                // [CHẶNG A] Tượng Đá Nghiền #1 (RockHead)
                // Cảm biến tiệm cận 0.25s: người chơi chạy qua dưới gầm nháy mắt rồi dập xuống sàn y = 0
                SpawnRockHead(rockHeadPrefab, trg, "RockHead_1", new Vector3(13f, 6.5f, 0f), 2.5f, 6.5f, 0.25f, 18f, 4f);

                // [CHẶNG B] Hố sập ngụy trang, Bệ di động, Tượng đá #2 & Khối đá gai #1
                // Hố sập DeathPit ngụy trang tại x = 27..28
                SpawnDeathPit(deathPitPrefab, trg, "DeathPit_B_1", new Vector3(27.25f, -0.6875f, 0f));
                SpawnDeathPit(deathPitPrefab, trg, "DeathPit_B_2", new Vector3(28.75f, -0.6875f, 0f));

                // Bệ nâng di động tuần tra ngang nối qua hố sâu từ x = 33 đến x = 43 (moveOffset = 9f, 0f)
                SpawnMovingPlatform(platformPrefab, trg, "MovingPlatform_Abyss", new Vector3(33.5f, 1.25f, 0f), Vector2.zero, new Vector2(9.5f, 0f), 3.0f);

                // Tượng đá #2 treo bảo vệ bệ Cam 2
                SpawnRockHead(rockHeadPrefab, trg, "RockHead_2", new Vector3(50f, 8f, 0f), 3f, 7f, 0.25f, 18f, 4f);

                // Khối đá gai #1 (SpikeHead) phóng ngang qua hành lang Chặng B
                SpawnSpikeHead(spikeHeadPrefab, trg, "SpikeHead_1", new Vector3(55f, 1.5f, 0f), Vector2.left, 5.0f, 16f, 5f, 1.4f);

                // [CHẶNG C] Hành lang Quạt Gió Ngược (FanTrap) & Chông Gai (Spikes)
                // Dãy chông gai sàn x = 65..69
                SpawnSpikes(spikesPrefab, trg, "Spikes_C_1", new Vector3(65.5f, 0.25f, 0f));
                SpawnSpikes(spikesPrefab, trg, "Spikes_C_2", new Vector3(67.0f, 0.25f, 0f));
                SpawnSpikes(spikesPrefab, trg, "Spikes_C_3", new Vector3(68.5f, 0.25f, 0f));

                // Quạt gió thổi ngang ngược chiều từ phải sang trái (windDirection = (-1, 0), windForce = 15f)
                SpawnFanTrap(fanPrefab, trg, "FanTrap_HorizontalAlley", new Vector3(77.5f, 1.5f, 0f), Vector2.left, 15f);

                // Khối đá gai #2 phóng dọc từ trần xuống chặn bước tiến ngược gió
                SpawnSpikeHead(spikeHeadPrefab, trg, "SpikeHead_Vertical", new Vector3(71.5f, 5.5f, 0f), Vector2.down, 4.0f, 15f, 5f, 1.8f);

                // [CHẶNG D] Tượng đá #3 phòng thủ trước Cổng Thần
                SpawnRockHead(rockHeadPrefab, trg, "RockHead_FinalGuard", new Vector3(90f, 7.0f, 0f), 2.8f, 7f, 0.25f, 18f, 4f);

                // Đáy vực sâu KillZone
                GameObject killZone = new GameObject("KillZone_Abyss");
                killZone.transform.SetParent(trg);
                killZone.transform.position = new Vector3(50f, -8f, 0f);
                killZone.tag = "Hazard";
                int trapsLayer = LayerMask.NameToLayer("Traps");
                killZone.layer = trapsLayer >= 0 ? trapsLayer : 10;
                BoxCollider2D killCol = killZone.AddComponent<BoxCollider2D>();
                killCol.isTrigger = true;
                killCol.size = new Vector2(120f, 4f);
                killZone.AddComponent<KillZone>();

                EditorUtility.DisplayProgressBar("One Hit Down", "Đặt Cam Cổ Vật [3/3], Anh đào và Cổng Thần...", 0.7f);

                // 6. --- 04. ITEMS & OBJECTIVES ---
                GameObject itemGroup = new GameObject("--- 04. ITEMS & OBJECTIVES ---");
                Transform ig = itemGroup.transform;

                LevelStartPoint startPoint = null;
                GameObject startPrefab = LoadPrefab(LevelStartPrefabPath);
                if (startPrefab != null)
                {
                    GameObject startGo = (GameObject)PrefabUtility.InstantiatePrefab(startPrefab);
                    startGo.transform.SetParent(ig);
                    startGo.transform.position = new Vector3(2.5f, 0f, 0f);
                    startPoint = startGo.GetComponentInChildren<LevelStartPoint>(true);
                }

                ExitGate exitGate = null;
                GameObject exitPrefab = LoadPrefab(ExitGatePrefabPath);
                if (exitPrefab != null)
                {
                    GameObject exitGo = (GameObject)PrefabUtility.InstantiatePrefab(exitPrefab);
                    exitGo.transform.SetParent(ig);
                    exitGo.transform.position = new Vector3(97.5f, 0f, 0f);
                    exitGate = exitGo.GetComponentInChildren<ExitGate>(true);
                }

                // 3 Quả Cam Cổ Vật Chính (bắt buộc nhặt [3/3] để mở Cổng Thần Màn 4)
                GameObject orangePrefab = LoadPrefab(OrangePrefabPath);
                // Quả 1: Đặt sau tượng đá RockHead 1 (người chơi luồn qua nhặt khi tượng đang rút lên)
                SpawnFruit(orangePrefab, ig, "Fruit_Orange_Relic_1", new Vector3(16.5f, 1.0f, 0f), true);
                // Quả 2: Đặt trên bệ cao Chặng B (yêu cầu nhảy đà và né tượng đá 2)
                SpawnFruit(orangePrefab, ig, "Fruit_Orange_Relic_2", new Vector3(50.0f, 6.5f, 0f), true);
                // Quả 3: Đặt ở cuối hành lang gió ngược Chặng C (vượt qua chông gai và luồng gió quạt)
                SpawnFruit(orangePrefab, ig, "Fruit_Orange_Relic_3", new Vector3(74.5f, 1.2f, 0f), true);

                // Quả Anh đào thưởng phụ (countsAsRelic = false): mồi nhử lơ lửng phía trên hố sập gạch Chặng B
                GameObject cherriesPrefab = LoadPrefab(CherriesPrefabPath);
                SpawnFruit(cherriesPrefab, ig, "Fruit_Cherries_Bonus", new Vector3(28.0f, 2.2f, 0f), false);

                EditorUtility.DisplayProgressBar("One Hit Down", "Liên kết Core Managers...", 0.85f);

                // 7. --- 00. CORE MANAGERS ---
                GameObject coreGroup = new GameObject("--- 00. CORE MANAGERS ---");

                AudioManager audioMgr = null;
                GameObject audioPrefab = LoadPrefab(AudioManagerPrefabPath);
                if (audioPrefab != null)
                {
                    GameObject audioGo = (GameObject)PrefabUtility.InstantiatePrefab(audioPrefab);
                    audioGo.transform.SetParent(coreGroup.transform);
                    audioMgr = audioGo.GetComponent<AudioManager>();
                }

                GameManager gm = null;
                HUDController hud = null;
                PauseMenu pauseMenu = null;
                IrisTransition iris = null;
                GameObject coreRigPrefab = LoadPrefab(CoreRigPrefabPath);
                if (coreRigPrefab != null)
                {
                    GameObject coreRigGo = (GameObject)PrefabUtility.InstantiatePrefab(coreRigPrefab);
                    coreRigGo.transform.SetParent(coreGroup.transform);
                    gm = coreRigGo.GetComponentInChildren<GameManager>(true);
                    hud = coreRigGo.GetComponentInChildren<HUDController>(true);
                    pauseMenu = coreRigGo.GetComponentInChildren<PauseMenu>(true);
                    iris = coreRigGo.GetComponentInChildren<IrisTransition>(true);
                }

                if (gm != null)
                {
                    SerializedObject gmSo = new SerializedObject(gm);
                    gmSo.FindProperty("startPoint").objectReferenceValue = startPoint;
                    gmSo.FindProperty("exitGate").objectReferenceValue = exitGate;

                    GameObject p1Prefab = LoadPrefab(Player1PrefabPath);
                    GameObject p2Prefab = LoadPrefab(Player2PrefabPath);
                    GameObject tetherPrefab = LoadPrefab(SoulTetherPrefabPath);
                    if (p1Prefab != null) gmSo.FindProperty("player1Prefab").objectReferenceValue = p1Prefab.GetComponent<PlayerController>();
                    if (p2Prefab != null) gmSo.FindProperty("player2Prefab").objectReferenceValue = p2Prefab.GetComponent<PlayerController>();
                    if (tetherPrefab != null) gmSo.FindProperty("soulTetherPrefab").objectReferenceValue = tetherPrefab.GetComponent<SoulTether>();

                    gmSo.FindProperty("levelIndexOverride").intValue = 4; // Màn 4
                    gmSo.FindProperty("totalRelicsRequired").intValue = 3; // Đúng 3 Cam cổ vật
                    gmSo.FindProperty("respawnDelay").floatValue = 0.18f;  // Hồi sinh tức thì < 0.2s

                    gmSo.FindProperty("mainCamera").objectReferenceValue = camCtrl;
                    gmSo.FindProperty("hudController").objectReferenceValue = hud;
                    gmSo.FindProperty("pauseMenu").objectReferenceValue = pauseMenu;
                    gmSo.FindProperty("irisTransition").objectReferenceValue = iris;
                    gmSo.FindProperty("audioManager").objectReferenceValue = audioMgr;
                    gmSo.ApplyModifiedProperties();
                }
                else
                {
                    Debug.LogWarning($"{LogTag} Không tìm thấy GameManager trong CoreRig!");
                }

                // 8. Lưu Scene và đăng ký Build Settings (chèn đúng vị trí sau Level_03)
                EditorSceneManager.MarkSceneDirty(scene);
                bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
                RegisterInBuildSettings(ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                Debug.Log(saved
                    ? $"{LogTag} HOÀN TẤT DỰNG SCENE TẠI: {ScenePath}"
                    : $"{LogTag} LỖI: không lưu được Scene {ScenePath}");
                return saved;
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[Level04SceneBuilder] LỖI KHI DỰNG SCENE LEVEL_04: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        #region Spawn helpers

        private static void SpawnRockHead(GameObject prefab, Transform parent, string name, Vector3 pos,
            float detectionWidth = 2.5f, float detectionDistance = 6.5f, float reactionDelay = 0.25f, float slamSpeed = 18f, float retractSpeed = 4f)
        {
            if (prefab == null) return;
            GameObject rh = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            rh.name = name;
            rh.transform.SetParent(parent);
            rh.transform.position = pos;

            RockHeadTrap trap = rh.GetComponent<RockHeadTrap>();
            if (trap != null)
            {
                SerializedObject so = new SerializedObject(trap);
                so.FindProperty("detectionWidth").floatValue = detectionWidth;
                so.FindProperty("detectionDistance").floatValue = detectionDistance;
                so.FindProperty("reactionDelay").floatValue = reactionDelay;
                so.FindProperty("slamSpeed").floatValue = slamSpeed;
                so.FindProperty("retractSpeed").floatValue = retractSpeed;
                so.ApplyModifiedProperties();
            }
        }

        private static void SpawnSpikeHead(GameObject prefab, Transform parent, string name, Vector3 pos,
            Vector2 dashDirection, float dashDistance = 4f, float dashSpeed = 16f, float returnSpeed = 5f, float cycleDelay = 1.2f)
        {
            if (prefab == null) return;
            GameObject sh = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            sh.name = name;
            sh.transform.SetParent(parent);
            sh.transform.position = pos;

            SpikeHeadTrap trap = sh.GetComponent<SpikeHeadTrap>();
            if (trap != null)
            {
                SerializedObject so = new SerializedObject(trap);
                so.FindProperty("dashDirection").vector2Value = dashDirection.normalized;
                so.FindProperty("dashDistance").floatValue = dashDistance;
                so.FindProperty("dashSpeed").floatValue = dashSpeed;
                so.FindProperty("returnSpeed").floatValue = returnSpeed;
                so.FindProperty("cycleDelay").floatValue = cycleDelay;
                so.ApplyModifiedProperties();
            }
        }

        private static void SpawnFanTrap(GameObject prefab, Transform parent, string name, Vector3 pos, Vector2 windDir, float windForce = 15f)
        {
            if (prefab == null) return;
            GameObject fan = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            fan.name = name;
            fan.transform.SetParent(parent);
            fan.transform.position = pos;

            FanTrap trap = fan.GetComponent<FanTrap>();
            if (trap != null)
            {
                SerializedObject so = new SerializedObject(trap);
                so.FindProperty("windDirection").vector2Value = windDir.normalized;
                so.FindProperty("windForce").floatValue = windForce;
                so.ApplyModifiedProperties();
            }

            // Xoay hình ảnh và vùng gió khớp hướng thổi ngang
            if (Mathf.Abs(windDir.x) > 0.5f)
            {
                float rotZ = windDir.x < 0 ? 90f : -90f;
                fan.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
            }
        }

        private static void SpawnMovingPlatform(GameObject prefab, Transform parent, string name, Vector3 pos,
            Vector2 startOffset, Vector2 moveOffset, float speed = 3.0f)
        {
            if (prefab == null) return;
            GameObject mp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            mp.name = name;
            mp.transform.SetParent(parent);
            mp.transform.position = pos;

            MovingPlatform platform = mp.GetComponent<MovingPlatform>();
            if (platform != null)
            {
                SerializedObject so = new SerializedObject(platform);
                so.FindProperty("startOffset").vector2Value = startOffset;
                so.FindProperty("moveOffset").vector2Value = moveOffset;
                so.FindProperty("speed").floatValue = speed;
                so.ApplyModifiedProperties();
            }
        }

        private static void SpawnSpikes(GameObject prefab, Transform parent, string name, Vector3 pos)
        {
            if (prefab == null) return;
            GameObject sp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            sp.name = name;
            sp.transform.SetParent(parent);
            sp.transform.position = pos;
        }

        private static void SpawnDeathPit(GameObject prefab, Transform parent, string name, Vector3 pos)
        {
            if (prefab == null) return;
            GameObject dp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            dp.name = name;
            dp.transform.SetParent(parent);
            dp.transform.position = pos;
        }

        private static void SpawnFruit(GameObject prefab, Transform parent, string name, Vector3 pos, bool countsAsRelic)
        {
            if (prefab == null) return;
            GameObject fruit = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            fruit.name = name;
            fruit.transform.SetParent(parent);
            fruit.transform.position = pos;

            CollectibleItem item = fruit.GetComponent<CollectibleItem>();
            if (item != null)
            {
                SerializedObject so = new SerializedObject(item);
                so.FindProperty("countsAsRelic").boolValue = countsAsRelic;
                so.ApplyModifiedProperties();
            }
        }

        private static GameObject LoadPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) Debug.LogWarning($"{LogTag} Không tìm thấy prefab: {path}");
            return prefab;
        }

        private static Sprite LoadSprite(string texturePath, string spriteName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            if (assets == null) return null;
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite sprite && sprite.name == spriteName) return sprite;
            }
            return null;
        }

        #endregion

        #region Terrain Builder

        /// <summary>
        /// Dựng khối đá điện thờ vững chắc có collider Layer Ground (8) và SpriteRenderer Tiled.
        /// </summary>
        private static void BuildStoneBlock(Transform parent, string name, int startX, int endX, float surfaceY, float bottomY, Sprite stoneSprite)
        {
            float width = endX - startX + 1;
            float height = surfaceY - bottomY;

            GameObject block = new GameObject(name);
            block.transform.SetParent(parent);
            block.transform.position = new Vector3(startX + width / 2f, surfaceY - height / 2f, 0f);
            block.layer = 8; // Layer 8: Ground

            BoxCollider2D col = block.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, height);

            if (stoneSprite != null)
            {
                SpriteRenderer sr = block.AddComponent<SpriteRenderer>();
                sr.sprite = stoneSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.tileMode = SpriteTileMode.Continuous;
                sr.size = new Vector2(width, height);
                sr.sortingOrder = 1;
            }
        }

        #endregion

        /// <summary>Thêm Level_04 vào Build Settings, chèn ngay sau Level_03 và trước Level_05.</summary>
        private static void RegisterInBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene existing in scenes)
            {
                if (existing.path == scenePath)
                {
                    existing.enabled = true;
                    EditorBuildSettings.scenes = scenes;
                    return;
                }
            }

            int insertAt = scenes.Length;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path.EndsWith("Level_03.unity"))
                {
                    insertAt = i + 1;
                    break;
                }
            }

            EditorBuildSettingsScene[] result = new EditorBuildSettingsScene[scenes.Length + 1];
            Array.Copy(scenes, 0, result, 0, insertAt);
            result[insertAt] = new EditorBuildSettingsScene(scenePath, true);
            Array.Copy(scenes, insertAt, result, insertAt + 1, scenes.Length - insertAt);
            EditorBuildSettings.scenes = result;
        }

        /// <summary>Điểm vào batchmode: -executeMethod OneHitDown.EditorTools.Level04SceneBuilder.BuildFromCommandLine</summary>
        public static void BuildFromCommandLine()
        {
            bool ok = BuildSceneInternal();
            EditorApplication.Exit(ok ? 0 : 1);
        }
    }
}
#endif
