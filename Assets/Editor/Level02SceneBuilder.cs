// Trần Đức Trung (2351060493) — Thiết kế Màn 2 (Level_02.unity)
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
    /// Dựng tự động Scene 'Assets/Scenes/Level_02.unity' theo kịch bản Báo cáo Phần 5 — Màn 2:
    /// "Hẻm Núi Sa Thạch &amp; Cối Xay Gió (Buzzsaw Sandstone)" — Độ khó 2/5 — Căn nhịp Cưa Xích Cổ.
    /// - Phông nền: Background/Yellow.png, địa hình sa thạch (khối đất mặt cam của Terrain 16x16).
    /// - Bẫy: Saw on Chain (SawSpinner tuần tra PingPong trên ray xích), Trampoline (đệm da thú bật thẳng đứng,
    ///   không đẩy trượt ngang), Blocks (DeathPit gạch vỡ ngụy trang hố tử thần).
    /// - Cổ vật: 3 Chuối (Fruit_Bananas, countsAsRelic = true) đặt trong hốc đá trên cao; 1 Dưa hấu thưởng phụ
    ///   (Fruit_Melon, countsAsRelic = false) làm mồi nhử phía trên gạch vỡ.
    /// - Ác Mộng Quay Đầu: lối ngầm dưới đất né cưa dẫn thẳng tới Cổng Thần (khóa nếu thiếu Chuối). Khi quay đầu
    ///   từ phải sang trái (đang giữ phím trái) mà giẫm lên đệm, người chơi nảy lên rồi trôi về trái,
    ///   lao vào lưỡi cưa ray dọc phía trái-trên đệm; phải dùng Nhảy Đôi để bẻ lái. Ở đoạn cuối, đệm sát mép
    ///   vực là lối duy nhất để bật ngược lên bậc thang đá khi quay đầu (nảy lên + giữ trái + Nhảy Đôi).
    ///
    /// Chiều dài màn: LevelStart x = 1.5 → Cổng Thần x = 166 (≈ 164.5 tiles, chuẩn Màn 2: 150 – 180 tiles).
    /// 5 đoạn: Cửa hẻm (0..39) · Hành lang gạch vỡ (43..62) · Đỉnh hẻm (66..99) ·
    /// Cầu gạch vỡ &amp; bậc thang zíc-zắc (100..127) · Vực cuối &amp; Cổng Thần (128..171).
    ///
    /// Âm thanh: dùng AudioManager lồng sẵn trong CoreRig (đã gán đủ 6 BGM); GameManager.levelIndexOverride = 2
    /// phát bgm_level2_grotto.ogg. Không đặt thêm AudioManager.prefab riêng vì AudioManager là singleton —
    /// bản thứ hai bị Destroy làm nhạc nền không phát.
    ///
    /// Quy ước tọa độ: 1 unit = 1 tile (PPU 16). Mặt sàn lối ngầm ở y = 0.
    /// Cân chỉnh theo vật lý chuẩn: gravity 9.81 × 3.5 (khi rơi × fallGravityMultiplier 1.35), jumpForce 15.5
    /// (≈ 3.5 tiles), doubleJumpForce 13.5 (≈ +2.65 tiles), Trampoline bounceForce 20 bật thẳng đứng
    /// (≈ 5.8 tiles; + Nhảy Đôi ≈ 8.5 tiles).
    /// </summary>
    public static class Level02SceneBuilder
    {
        private const string SceneDir = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/Level_02.unity";
        private const string LogTag = "<b>[Level02SceneBuilder]</b>";

        // Asset Paths
        private const string BgYellowPath = "Assets/Art/02_MoiTruong/PA1_Background/Yellow.png";
        private const string TerrainPath = "Assets/Art/02_MoiTruong/PA1_Terrain/Terrain (16x16).png";

        // Khối sa thạch mặt cam 3x3 tại Rect(96, 64, 48, 48) của Terrain (16x16).png (cột X06..X08, hàng Y04..Y06).
        private const string SandTopLeft = "Tile_16x16_X06_Y06";
        private const string SandTopMid = "Tile_16x16_X07_Y06";
        private const string SandTopRight = "Tile_16x16_X08_Y06";
        private const string SandMidLeft = "Tile_16x16_X06_Y05";
        private const string SandCenter = "Tile_16x16_X07_Y05";
        private const string SandMidRight = "Tile_16x16_X08_Y05";
        private const string SandBotLeft = "Tile_16x16_X06_Y04";
        private const string SandBotMid = "Tile_16x16_X07_Y04";
        private const string SandBotRight = "Tile_16x16_X08_Y04";

        // Prefab Paths
        private const string CoreRigPrefabPath = "Assets/Prefabs/CoreRig.prefab";
        private const string LevelStartPrefabPath = "Assets/Prefabs/LevelStart.prefab";
        private const string ExitGatePrefabPath = "Assets/Prefabs/ExitGate.prefab";

        private const string Player1PrefabPath = "Assets/Prefabs/Player1.prefab";
        private const string Player2PrefabPath = "Assets/Prefabs/Player2.prefab";
        private const string SoulTetherPrefabPath = "Assets/Prefabs/SoulTether.prefab";

        private const string BananasPrefabPath = "Assets/Prefabs/Fruit_Bananas.prefab";
        private const string BananasTexturePath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Fruits/Bananas.png";
        private const string MelonPrefabPath = "Assets/Prefabs/Fruit_Melon.prefab";

        private const string SawPrefabPath = "Assets/Prefabs/Saw.prefab";
        private const string DeathPitPrefabPath = "Assets/Prefabs/Traps/DeathPit.prefab";
        private const string TrampolinePrefabPath = "Assets/Prefabs/Traps/Trampoline.prefab";

        // Thông số bố cục dùng chung
        private const float FloorBottomY = -5f;          // Đáy khối sàn lối ngầm (ngoài khung hình)
        private const float SawFloorY = 1.05f;           // Tâm cưa sát mặt sàn (bán kính collider 1.05)
        private const float TrampolineY = 0f;            // Pivot sprite đệm ở đáy → đặt sát mặt sàn y = 0 (khớp Level_01)
        private const float BacktrackSawY = 3f;          // Đáy cưa ray dọc 1.95 > đỉnh đầu nhân vật 1.55: đi bộ dưới sàn an toàn
        private const float BacktrackSawRange = 3.5f;    // Quét y 3 → 6.5, đúng vùng bị hất ngược khi quay đầu
        private const float DeathPitBlockY = -0.6875f;   // Khối gạch 22px (1.375 unit) có mặt trên phẳng y = 0
        // Mọi đệm da thú Màn 2 bật THẲNG ĐỨNG (0, 1): đứng trên đệm chỉ nảy lên, không bị đẩy trượt ngang.
        private static readonly Vector2 BounceUp = Vector2.up;
        private const float LevelMinX = -2f;             // Mép trái vách chắn đầu màn
        private const float LevelMaxX = 174f;            // Mép phải vách chắn cuối màn

        [MenuItem("Tools/One Hit Down/Build Level 02 Scene", false, 3)]
        public static void BuildScene()
        {
            BuildSceneInternal();
        }

        /// <summary>Dựng toàn bộ Scene; trả về true nếu lưu thành công.</summary>
        private static bool BuildSceneInternal()
        {
            try
            {
                if (File.Exists(ScenePath) && !Application.isBatchMode)
                {
                    if (!EditorUtility.DisplayDialog(
                        "Xác nhận ghi đè Level 02",
                        "Màn chơi 'Level_02.unity' đã tồn tại.\nBạn có chắc chắn muốn DỰNG LẠI và GHI ĐÈ toàn bộ không?",
                        "Ghi đè",
                        "Hủy bỏ (Bảo vệ Scene)"))
                    {
                        Debug.Log($"{LogTag} Đã hủy lệnh dựng lại để bảo vệ Level 02 hiện tại.");
                        return false;
                    }
                }

                EditorUtility.DisplayProgressBar("One Hit Down", "Đang dựng Scene Level_02 (Hẻm Núi Sa Thạch)...", 0.1f);
                Debug.Log($"{LogTag} BẮT ĐẦU DỰNG SCENE LEVEL_02.UNITY...");

                if (!Directory.Exists(SceneDir))
                {
                    Directory.CreateDirectory(SceneDir);
                }

                // 1. Scene trống
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // 2. Sprite nền và địa hình sa thạch
                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgYellowPath);
                TerrainSprites sand = new TerrainSprites
                {
                    topLeft = LoadSprite(TerrainPath, SandTopLeft),
                    topMid = LoadSprite(TerrainPath, SandTopMid),
                    topRight = LoadSprite(TerrainPath, SandTopRight),
                    midLeft = LoadSprite(TerrainPath, SandMidLeft),
                    center = LoadSprite(TerrainPath, SandCenter),
                    midRight = LoadSprite(TerrainPath, SandMidRight),
                    botLeft = LoadSprite(TerrainPath, SandBotLeft),
                    botMid = LoadSprite(TerrainPath, SandBotMid),
                    botRight = LoadSprite(TerrainPath, SandBotRight)
                };

                if (bgSprite == null) Debug.LogWarning($"{LogTag} Không tìm thấy Background/Yellow.png.");
                if (sand.topMid == null || sand.center == null) Debug.LogWarning($"{LogTag} Không tìm thấy slice sa thạch trong Terrain (16x16).png.");

                // 3. 01. ENVIRONMENT & BACKGROUND
                GameObject envGroup = new GameObject("--- 01. ENVIRONMENT & BACKGROUND ---");

                GameObject bgGo = new GameObject("Background_Yellow");
                bgGo.transform.SetParent(envGroup.transform);
                bgGo.transform.position = new Vector3((LevelMinX + LevelMaxX) / 2f, 5f, 5f);
                SpriteRenderer bgSr = bgGo.AddComponent<SpriteRenderer>();
                bgSr.sprite = bgSprite;
                bgSr.drawMode = SpriteDrawMode.Tiled;
                bgSr.size = new Vector2(LevelMaxX - LevelMinX + 30f, 35f);
                bgSr.sortingOrder = -100;

                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.transform.SetParent(envGroup.transform);
                camGo.transform.position = new Vector3(3.5f, 2.0f, -10f);
                Camera cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.backgroundColor = new Color(0.93f, 0.78f, 0.45f, 1f); // Vàng đất sa thạch
                camGo.AddComponent<AudioListener>();

                CameraController camCtrl = camGo.AddComponent<CameraController>();
                SerializedObject camSo = new SerializedObject(camCtrl);
                camSo.FindProperty("targetOrthoSize").floatValue = 5.5f;
                camSo.FindProperty("smoothSpeed").floatValue = 6f;
                camSo.FindProperty("offset").vector3Value = new Vector3(0f, 1.5f, -10f);
                camSo.FindProperty("minY").floatValue = -3f;
                camSo.FindProperty("maxY").floatValue = 12f;
                camSo.FindProperty("autoFindInterval").floatValue = 0.25f;
                camSo.ApplyModifiedProperties();

                EditorUtility.DisplayProgressBar("One Hit Down", "Dựng địa hình hẻm núi sa thạch...", 0.3f);

                // 4. 02. TERRAIN & ARCHITECTURE
                GameObject terrainGroup = new GameObject("--- 02. TERRAIN & ARCHITECTURE ---");
                Transform tg = terrainGroup.transform;

                // Vách chắn hai đầu màn (cao 10 tiles, không cho rơi khỏi bản đồ)
                BuildTerrainBlock(tg, "Wall_Left", -2, -1, 10f, FloorBottomY, true, true, false, sand);
                BuildTerrainBlock(tg, "Wall_Right", 172, 173, 10f, FloorBottomY, true, true, false, sand);

                // LỐI NGẦM (y = 0): các đoạn sàn ngăn cách bởi 3 hố phủ gạch vỡ (x 40..43, 63..66, 100..103)
                // và một vực trống (x 128..134) sau bậc thang đá.
                BuildTerrainBlock(tg, "Floor_1_Start", 0, 39, 0f, FloorBottomY, false, true, false, sand);
                BuildTerrainBlock(tg, "Floor_2_Mid", 43, 62, 0f, FloorBottomY, true, true, false, sand);
                BuildTerrainBlock(tg, "Floor_3_Canyon", 66, 99, 0f, FloorBottomY, true, true, false, sand);
                BuildTerrainBlock(tg, "Floor_4_Bridge", 103, 118, 0f, FloorBottomY, true, false, false, sand);

                // BẬC THANG ZÍC-ZẮC (mỗi bậc cao 2 tiles < tầm nhảy 3.5 tiles); bậc cuối có vách đứng bên vực
                BuildTerrainBlock(tg, "Step_A", 119, 121, 2f, FloorBottomY, false, false, false, sand);
                BuildTerrainBlock(tg, "Step_B", 122, 124, 4f, FloorBottomY, false, false, false, sand);
                BuildTerrainBlock(tg, "Step_C", 125, 127, 6f, FloorBottomY, false, true, false, sand);

                // Vực cuối x 128..133 (rộng 6 tiles): đi bộ rơi thẳng xuống vực, phải bật nhảy từ đỉnh Step_C
                BuildTerrainBlock(tg, "Floor_5_Gate", 134, 171, 0f, FloorBottomY, true, false, false, sand);

                // HỐC ĐÁ TRÊN ĐỈNH HẺM NÚI (bệ treo dày 2 tiles, chứa Chuối)
                // Hốc 1: x 28..34, mặt y = 6 — lên bằng đệm #1 + giữ phải + Nhảy Đôi
                BuildTerrainBlock(tg, "Ledge_Banana_1", 28, 34, 6f, 4f, true, true, true, sand);
                // Hốc 2: x 52..55, mặt y = 7 — lên bằng đệm #2 + Nhảy Đôi, cưa ray ngang quét qua
                BuildTerrainBlock(tg, "Ledge_Banana_2", 52, 55, 7f, 5f, true, true, true, sand);
                // Hốc 3: x 82..88, mặt y = 6 — lên bằng đệm #3 + giữ phải + Nhảy Đôi
                BuildTerrainBlock(tg, "Ledge_Banana_3", 82, 88, 6f, 4f, true, true, true, sand);

                EditorUtility.DisplayProgressBar("One Hit Down", "Đặt cưa xích, đệm da thú và gạch vỡ...", 0.5f);

                // 5. 03. TRAPS & HAZARDS
                GameObject trapGroup = new GameObject("--- 03. TRAPS & HAZARDS ---");
                Transform trg = trapGroup.transform;

                GameObject sawPrefab = LoadPrefab(SawPrefabPath);
                GameObject trampPrefab = LoadPrefab(TrampolinePrefabPath);
                GameObject deathPitPrefab = LoadPrefab(DeathPitPrefabPath);

                // --- Đoạn 1: Cửa hẻm núi ---
                // Cưa ray sàn #1: người chơi canh nhịp nhảy qua (cao 2.1 tiles < tầm nhảy 3.5 tiles)
                SpawnSaw(sawPrefab, trg, "Saw_Floor_1", new Vector3(12f, SawFloorY, 0f), PatrolAxis.Horizontal, 2.5f, 5f);
                // Đệm da thú #1: bật thẳng đứng cạnh hốc Chuối 1 (giữ phải + Nhảy Đôi để đáp lên hốc)
                SpawnTrampoline(trampPrefab, trg, "Trampoline_Ledge_1", new Vector3(22.5f, TrampolineY, 0f), BounceUp);
                // Cưa ray dọc phía TRÁI-TRÊN đệm #1: người chạy tới giữ phải nên trôi sang phải, không chạm; người quay đầu
                // (đang giữ phím trái) giẫm đệm nảy lên rồi trôi về trái, lao thẳng vào cưa (Backtrack).
                SpawnSaw(sawPrefab, trg, "Saw_Backtrack_1", new Vector3(19.5f, BacktrackSawY, 0f), PatrolAxis.Vertical, 2.2f, BacktrackSawRange);

                // --- Đoạn 2: Hành lang gạch vỡ ---
                // Hố gạch vỡ #1 (x 40..43): 2 khối Blocks ngụy trang, giẫm vào là sập vĩnh viễn
                SpawnDeathPit(deathPitPrefab, trg, "DeathPit_Blocks_1A", new Vector3(40.75f, DeathPitBlockY, 0f));
                SpawnDeathPit(deathPitPrefab, trg, "DeathPit_Blocks_1B", new Vector3(42.25f, DeathPitBlockY, 0f));
                // Cưa ray sàn #2
                SpawnSaw(sawPrefab, trg, "Saw_Floor_2", new Vector3(46f, SawFloorY, 0f), PatrolAxis.Horizontal, 3f, 7f);
                // Đệm da thú #2: bật thẳng đứng cạnh hốc Chuối 2
                SpawnTrampoline(trampPrefab, trg, "Trampoline_Ledge_2", new Vector3(57.5f, TrampolineY, 0f), BounceUp);
                // Cưa ray ngang quét qua hốc Chuối 2 (P2 ghì dây kéo P1 lùi lại trước khi cưa trượt ngang qua)
                SpawnSaw(sawPrefab, trg, "Saw_Ledge_2", new Vector3(50.5f, 8.3f, 0f), PatrolAxis.Horizontal, 2f, 6f);
                // Hố gạch vỡ #2 (x 63..66) — có Dưa hấu mồi nhử phía trên
                SpawnDeathPit(deathPitPrefab, trg, "DeathPit_Blocks_2A", new Vector3(63.75f, DeathPitBlockY, 0f));
                SpawnDeathPit(deathPitPrefab, trg, "DeathPit_Blocks_2B", new Vector3(65.25f, DeathPitBlockY, 0f));

                // --- Đoạn 3: Đỉnh hẻm núi & Cổng Thần ---
                SpawnSaw(sawPrefab, trg, "Saw_Floor_3", new Vector3(66.8f, SawFloorY, 0f), PatrolAxis.Horizontal, 3.2f, 4.5f);
                SpawnTrampoline(trampPrefab, trg, "Trampoline_Ledge_3", new Vector3(76.5f, TrampolineY, 0f), BounceUp);
                SpawnSaw(sawPrefab, trg, "Saw_Backtrack_3", new Vector3(73.5f, BacktrackSawY, 0f), PatrolAxis.Vertical, 2.4f, BacktrackSawRange);

                // --- Đoạn 4: Cầu gạch vỡ & bậc thang zíc-zắc ---
                // Hố gạch vỡ #3 (x 100..103) ngay sau đỉnh hẻm
                SpawnDeathPit(deathPitPrefab, trg, "DeathPit_Blocks_3A", new Vector3(100.75f, DeathPitBlockY, 0f));
                SpawnDeathPit(deathPitPrefab, trg, "DeathPit_Blocks_3B", new Vector3(102.25f, DeathPitBlockY, 0f));
                // Cặp cưa ray sàn chạy so le trên cầu đá trước bậc thang
                SpawnSaw(sawPrefab, trg, "Saw_Floor_4A", new Vector3(105f, SawFloorY, 0f), PatrolAxis.Horizontal, 3f, 5f);
                SpawnSaw(sawPrefab, trg, "Saw_Floor_4B", new Vector3(111f, SawFloorY, 0f), PatrolAxis.Horizontal, 3.4f, 6f);

                // --- Đoạn 5: Vực cuối & Cổng Thần ---
                // Đệm da thú sát mép vực (bật thẳng đứng): lối bật về bậc thang khi quay đầu
                // (nảy lên + giữ trái + Nhảy Đôi).
                SpawnTrampoline(trampPrefab, trg, "Trampoline_Back_5", new Vector3(135.5f, TrampolineY, 0f), BounceUp);
                SpawnSaw(sawPrefab, trg, "Saw_Floor_5A", new Vector3(141f, SawFloorY, 0f), PatrolAxis.Horizontal, 3f, 6f);
                // Cưa ray dọc canh lối vào sân Cổng Thần: canh nhịp đi qua khi cưa nâng lên
                SpawnSaw(sawPrefab, trg, "Saw_Gate_Guard", new Vector3(153f, 1.6f, 0f), PatrolAxis.Vertical, 2.6f, 4f);
                SpawnSaw(sawPrefab, trg, "Saw_Floor_5B", new Vector3(156f, SawFloorY, 0f), PatrolAxis.Horizontal, 3.4f, 5f);

                // Vực sâu dưới các hố gạch vỡ và vực cuối: rơi xuống là chết
                GameObject killZone = new GameObject("KillZone_Abyss");
                killZone.transform.SetParent(trg);
                killZone.transform.position = new Vector3((LevelMinX + LevelMaxX) / 2f, -7.5f, 0f);
                killZone.tag = "Hazard";
                int trapsLayer = LayerMask.NameToLayer("Traps");
                killZone.layer = trapsLayer >= 0 ? trapsLayer : 10;
                BoxCollider2D killCol = killZone.AddComponent<BoxCollider2D>();
                killCol.isTrigger = true;
                killCol.size = new Vector2(LevelMaxX - LevelMinX + 10f, 4f);
                killZone.AddComponent<KillZone>();

                EditorUtility.DisplayProgressBar("One Hit Down", "Đặt Chuối cổ vật, Dưa hấu và Cổng Thần...", 0.7f);

                // 6. 04. ITEMS & OBJECTIVES
                GameObject itemGroup = new GameObject("--- 04. ITEMS & OBJECTIVES ---");
                Transform ig = itemGroup.transform;

                LevelStartPoint startPoint = null;
                GameObject startPrefab = LoadPrefab(LevelStartPrefabPath);
                if (startPrefab != null)
                {
                    GameObject startGo = (GameObject)PrefabUtility.InstantiatePrefab(startPrefab);
                    startGo.transform.SetParent(ig);
                    startGo.transform.position = new Vector3(1.5f, 0f, 0f);
                    startPoint = startGo.GetComponentInChildren<LevelStartPoint>(true);
                }

                ExitGate exitGate = null;
                GameObject exitPrefab = LoadPrefab(ExitGatePrefabPath);
                if (exitPrefab != null)
                {
                    GameObject exitGo = (GameObject)PrefabUtility.InstantiatePrefab(exitPrefab);
                    exitGo.transform.SetParent(ig);
                    exitGo.transform.position = new Vector3(166f, 0f, 0f);
                    // Sprite cúp có pivot ở tâm: nâng lên đúng nửa chiều cao để đáy cúp chạm mặt sàn y = 0.
                    SpriteRenderer gateSr = exitGo.GetComponent<SpriteRenderer>();
                    if (gateSr != null && gateSr.sprite != null)
                    {
                        exitGo.transform.position = new Vector3(166f, -gateSr.bounds.min.y, 0f);
                    }
                    exitGate = exitGo.GetComponentInChildren<ExitGate>(true);
                }

                // 3 Chuối Sa Mạc Hoàng Gia (Cổ Vật mở cổng [3/3])
                GameObject bananasPrefab = LoadPrefab(BananasPrefabPath);
                SpawnFruit(bananasPrefab, ig, "Banana_Relic_1", new Vector3(32.5f, 7f, 0f), true);
                SpawnFruit(bananasPrefab, ig, "Banana_Relic_2", new Vector3(53.5f, 8f, 0f), true);
                SpawnFruit(bananasPrefab, ig, "Banana_Relic_3", new Vector3(86.5f, 7f, 0f), true);

                // Dưa hấu thưởng phụ: mồi nhử lơ lửng phía trên hố gạch vỡ #2
                SpawnFruit(LoadPrefab(MelonPrefabPath), ig, "Melon_Bait", new Vector3(64.5f, 1.6f, 0f), false);

                EditorUtility.DisplayProgressBar("One Hit Down", "Liên kết Core Managers...", 0.85f);

                // 7. 00. CORE MANAGERS
                GameObject coreGroup = new GameObject("--- 00. CORE MANAGERS ---");

                // AudioManager lấy từ bản lồng sẵn trong CoreRig (singleton duy nhất, đã gán đủ 6 BGM).
                AudioManager audioMgr = null;
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
                    audioMgr = coreRigGo.GetComponentInChildren<AudioManager>(true);

                    // Icon Cổ Vật trên HUD: CoreRig mặc định là Kiwi (Màn 3) — Màn 2 đổi sang Chuối (Bananas.png).
                    SetHudRelicIcon(hud, LoadSprite(BananasTexturePath, "Bananas_0"));
                }

                if (audioMgr == null)
                {
                    Debug.LogWarning($"{LogTag} CoreRig không chứa AudioManager — Màn 2 sẽ không có nhạc nền.");
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

                    gmSo.FindProperty("levelIndexOverride").intValue = 2; // Level 02
                    gmSo.FindProperty("totalRelicsRequired").intValue = 3;
                    gmSo.FindProperty("respawnDelay").floatValue = 0.18f;

                    gmSo.FindProperty("mainCamera").objectReferenceValue = camCtrl;
                    gmSo.FindProperty("hudController").objectReferenceValue = hud;
                    gmSo.FindProperty("pauseMenu").objectReferenceValue = pauseMenu;
                    gmSo.FindProperty("irisTransition").objectReferenceValue = iris;
                    gmSo.FindProperty("audioManager").objectReferenceValue = audioMgr;
                    gmSo.ApplyModifiedProperties();
                }
                else
                {
                    Debug.LogWarning($"{LogTag} Không tìm thấy GameManager trong CoreRig — chưa gán tham chiếu hạ tầng.");
                }

                // 8. Lưu Scene và đăng ký Build Settings (ngay sau Level_01)
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
                Debug.LogError($"[Level02SceneBuilder] LỖI KHI DỰNG SCENE LEVEL_02: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        #region Spawn helpers

        /// <summary>Đặt lưỡi cưa xích tuần tra PingPong theo trục chỉ định; ray xích do SawSpinner tự vẽ.</summary>
        private static void SpawnSaw(GameObject prefab, Transform parent, string name, Vector3 pos, PatrolAxis axis, float speed, float range)
        {
            if (prefab == null) return;
            GameObject saw = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            saw.name = name;
            saw.transform.SetParent(parent);
            saw.transform.position = pos;

            SawSpinner spinner = saw.GetComponent<SawSpinner>();
            if (spinner == null) return;
            SerializedObject so = new SerializedObject(spinner);
            so.FindProperty("patrolMode").enumValueIndex = (int)PatrolMode.PingPong;
            so.FindProperty("patrolAxis").enumValueIndex = (int)axis;
            so.FindProperty("patrolSpeed").floatValue = speed;
            so.FindProperty("patrolRange").floatValue = range;
            so.ApplyModifiedProperties();
        }

        /// <summary>Đặt đệm da thú với hướng bật cho trước, giữ lực chuẩn bounceForce = 20 của prefab.</summary>
        private static void SpawnTrampoline(GameObject prefab, Transform parent, string name, Vector3 pos, Vector2 direction)
        {
            if (prefab == null) return;
            GameObject tr = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            tr.name = name;
            tr.transform.SetParent(parent);
            tr.transform.position = pos;

            TrampolineTrap trap = tr.GetComponent<TrampolineTrap>();
            if (trap == null) return;
            SerializedObject so = new SerializedObject(trap);
            so.FindProperty("bounceDirection").vector2Value = direction.normalized;
            so.ApplyModifiedProperties();
        }

        /// <summary>Đặt một khối gạch vỡ (Blocks/Idle.png) phủ trên miệng hố tử thần.</summary>
        private static void SpawnDeathPit(GameObject prefab, Transform parent, string name, Vector3 pos)
        {
            if (prefab == null) return;
            GameObject dp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            dp.name = name;
            dp.transform.SetParent(parent);
            dp.transform.position = pos;
        }

        /// <summary>Đặt một quả; countsAsRelic quyết định quả có tính vào điều kiện mở Cổng Thần.</summary>
        private static void SpawnFruit(GameObject prefab, Transform parent, string name, Vector3 pos, bool countsAsRelic)
        {
            if (prefab == null) return;
            GameObject fruit = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            fruit.name = name;
            fruit.transform.SetParent(parent);
            fruit.transform.position = pos;

            CollectibleItem item = fruit.GetComponent<CollectibleItem>();
            if (item == null) return;
            SerializedObject so = new SerializedObject(item);
            so.FindProperty("countsAsRelic").boolValue = countsAsRelic;
            so.ApplyModifiedProperties();
        }

        /// <summary>Gán sprite Cổ Vật của màn cho Image "relicIcon" trên HUD và ghi override vào instance CoreRig.</summary>
        private static void SetHudRelicIcon(HUDController hud, Sprite sprite)
        {
            if (hud == null || sprite == null)
            {
                Debug.LogWarning($"{LogTag} Không gán được icon Cổ Vật trên HUD (thiếu HUDController hoặc sprite Bananas_0).");
                return;
            }

            SerializedObject hudSo = new SerializedObject(hud);
            UnityEngine.UI.Image icon = hudSo.FindProperty("relicIcon").objectReferenceValue as UnityEngine.UI.Image;
            if (icon == null)
            {
                Debug.LogWarning($"{LogTag} HUDController chưa tham chiếu Image relicIcon.");
                return;
            }

            icon.sprite = sprite;
            icon.enabled = true;
            PrefabUtility.RecordPrefabInstancePropertyModifications(icon);
            EditorUtility.SetDirty(icon);
        }

        private static GameObject LoadPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) Debug.LogWarning($"{LogTag} Không tìm thấy prefab: {path}");
            return prefab;
        }

        #endregion

        #region Terrain helpers

        private struct TerrainSprites
        {
            public Sprite topLeft, topMid, topRight;
            public Sprite midLeft, center, midRight;
            public Sprite botLeft, botMid, botRight;
        }

        /// <summary>
        /// Dựng khối sa thạch liền mạch từ cột startX đến endX (bao gồm), mặt trên ở surfaceY, đáy ở bottomY.
        /// Có viền trái/phải tùy chọn; hasBottomEdge vẽ hàng đáy cho bệ treo. Kèm BoxCollider2D Layer Ground (8).
        /// </summary>
        private static void BuildTerrainBlock(
            Transform parent,
            string name,
            int startX,
            int endX,
            float surfaceY,
            float bottomY,
            bool hasLeftEdge,
            bool hasRightEdge,
            bool hasBottomEdge,
            TerrainSprites s)
        {
            float width = endX - startX + 1;
            float height = surfaceY - bottomY;

            GameObject block = new GameObject(name);
            block.transform.SetParent(parent);
            block.transform.position = new Vector3(startX + width / 2f, surfaceY - height / 2f, 0f);
            block.layer = 8; // Layer 8: Ground

            BoxCollider2D col = block.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, height);

            float leftInset = hasLeftEdge ? 1f : 0f;
            float rightInset = hasRightEdge ? 1f : 0f;
            float midWidth = width - leftInset - rightInset;
            float midCenterX = -width / 2f + leftInset + midWidth / 2f;
            float leftX = -width / 2f + 0.5f;
            float rightX = width / 2f - 0.5f;

            // Hàng mặt trên (cao 1 tile)
            float topY = height / 2f - 0.5f;
            AddRow(block.transform, "Top", topY, 1f, hasLeftEdge ? s.topLeft : null, s.topMid, hasRightEdge ? s.topRight : null,
                leftX, rightX, midCenterX, midWidth);

            // Hàng đáy (chỉ bệ treo)
            float bottomRows = hasBottomEdge && height >= 2f ? 1f : 0f;
            if (bottomRows > 0f)
            {
                float botY = -height / 2f + 0.5f;
                AddRow(block.transform, "Bottom", botY, 1f, hasLeftEdge ? s.botLeft : null, s.botMid, hasRightEdge ? s.botRight : null,
                    leftX, rightX, midCenterX, midWidth);
            }

            // Thân đất ở giữa
            float bodyHeight = height - 1f - bottomRows;
            if (bodyHeight > 0f)
            {
                float bodyY = -height / 2f + bottomRows + bodyHeight / 2f;
                AddRow(block.transform, "Body", bodyY, bodyHeight, hasLeftEdge ? s.midLeft : null, s.center, hasRightEdge ? s.midRight : null,
                    leftX, rightX, midCenterX, midWidth);
            }
        }

        /// <summary>Vẽ một hàng gồm viền trái, phần giữa lát Tiled và viền phải.</summary>
        private static void AddRow(Transform parent, string label, float localY, float rowHeight,
            Sprite left, Sprite mid, Sprite right, float leftX, float rightX, float midCenterX, float midWidth)
        {
            if (left != null) AddTiled(parent, label + "_Left", new Vector3(leftX, localY, 0f), left, new Vector2(1f, rowHeight));
            if (mid != null && midWidth > 0f) AddTiled(parent, label + "_Mid", new Vector3(midCenterX, localY, 0f), mid, new Vector2(midWidth, rowHeight));
            if (right != null) AddTiled(parent, label + "_Right", new Vector3(rightX, localY, 0f), right, new Vector2(1f, rowHeight));
        }

        private static void AddTiled(Transform parent, string name, Vector3 localPos, Sprite sprite, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = size;
            sr.sortingOrder = 1;
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

        /// <summary>Thêm Level_02 vào Build Settings, chèn ngay sau Level_01 để giữ thứ tự màn.</summary>
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
                if (scenes[i].path.EndsWith("Level_01.unity")) { insertAt = i + 1; break; }
            }

            EditorBuildSettingsScene[] result = new EditorBuildSettingsScene[scenes.Length + 1];
            Array.Copy(scenes, 0, result, 0, insertAt);
            result[insertAt] = new EditorBuildSettingsScene(scenePath, true);
            Array.Copy(scenes, insertAt, result, insertAt + 1, scenes.Length - insertAt);
            EditorBuildSettings.scenes = result;
        }

        /// <summary>Điểm vào batchmode: -executeMethod OneHitDown.EditorTools.Level02SceneBuilder.BuildFromCommandLine</summary>
        public static void BuildFromCommandLine()
        {
            bool ok = BuildSceneInternal();
            EditorApplication.Exit(ok ? 0 : 1);
        }
    }
}
#endif
