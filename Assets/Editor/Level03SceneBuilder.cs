// ====================================================================================================
// SCRIPT UNITY EDITOR: XÂY DỰNG CHUẨN XÁC 100% SCENE MÀN 3 'Assets/Scenes/Level_03.unity'
// BẢN 1: TOÀN CẢNH 5 CHẶNG DÀI 200 UNITS (3200 PX, PPU = 16) TỪ THƯ MỤC 'màn 3/'
// "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA" (Catacombs of Ice & Ember)
//
// NGUYÊN TẮC CỐT LÕI (TUÂN THỦ 100% CLAUDE.MD DỰ ÁN):
// 1. CHỈ DÙNG ASSET ĐƯỢC PHÂN CHO MÀN 3 TẠI 'Assets/Art/' (PA1_Terrain, PA1_Background/Gray, Sand Mud Ice, Fire, Spikes, Spiked Ball, Fruits).
// 2. TUYỆT ĐỐI KHÔNG TẠO SPRITE THỦ CÔNG / PROCEDURAL (ZERO FAKE ASSETS).
// 3. TOÀN BỘ 6 CƠ QUAN MODULE 3 ĐƯỢC LẤY TRỰC TIẾP TỪ 'Assets/Prefabs/Traps/'.
// 4. MOCK SCRIPTS ĐẶT RIÊNG TẠI 'Assets/Scripts/Dev/Mocks/' THEO 'Docs/LEVEL_03_MOCK_NOTICE.md'.
// 5. TOÀN BỘ ĐỊA HÌNH KHỐI ĐÁ VÀ KHỐI BĂNG KHỚP KÍN HOÀN TOÀN TỪ Y = 0 ĐẾN Y = 4+, KHÔNG CÒN KHE HỞ VÀ KHÔNG CÒN KHỐI ĐEN LẠ.
//
// Menu Editor: Tools -> One Hit Down -> Build Level 03 Scene (Catacombs of Ice & Ember)
// ====================================================================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using OneHitDown.Dev;
using OneHitDown.Dev.Mocks;
using OneHitDown.Player;
using OneHitDown.Traps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHitDown.EditorTools
{
    public static class Level03SceneBuilder
    {
        private const string SceneDir = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/Level_03.unity";
        private const string PrefabsTrapsDir = "Assets/Prefabs/Traps";

        // Asset paths chính thức của dự án (Màn 3)
        private const string TerrainTexturePath = "Assets/Art/02_MoiTruong/PA1_Terrain/Terrain (16x16).png";

        // Khối đá 9-slice: vùng 48x48 góc trên-trái của Terrain (16x16).png (khung gạch xám, lõi tối), viền 16px = 1 tile
        private const string StoneBlockSpriteName = "Terrain (16x16)_StoneBlock";
        private static readonly Rect StoneBlockRect = new Rect(0f, 128f, 48f, 48f); // tọa độ Unity (gốc dưới-trái), ảnh cao 176px
        private const float StoneBlockBorder = 16f;

        // Ô băng 16x16 lấy từ lõi đặc của sprite băng (Sand Mud Ice (16x6)_2, x 127..175); ảnh cao 80px nên y Unity = 80 - (y ảnh + 16)
        private const string IceTexturePath = "Assets/Art/03_Bay/PA1_Traps/Sand Mud Ice/Sand Mud Ice (16x6).png";
        private const string IceTileSpriteName = "Sand Mud Ice (16x6)_IceTile";
        private static readonly Rect IceTileRect = new Rect(131f, 61f, 16f, 16f);
        private const string BackgroundTexturePath = "Assets/Art/02_MoiTruong/PA1_Background/Gray.png";
        private const string PlayerControllerAssetPath = "Assets/Art/Animations/NinjaFrog/NinjaFrog_Controller.controller";
        private const string PlayerSpritePath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Ninja Frog/Idle (32x32).png";
        private const string SpikesTexturePath = "Assets/Art/03_Bay/PA1_Traps/Spikes/Idle.png";
        private const string FireOffPath = "Assets/Art/03_Bay/PA1_Traps/Fire/Off.png";
        private const string FireHitPath = "Assets/Art/03_Bay/PA1_Traps/Fire/Hit (16x32).png";
        private const string FireOnPath = "Assets/Art/03_Bay/PA1_Traps/Fire/On (16x32).png";
        private const string SpikedBallPath = "Assets/Art/03_Bay/PA1_Traps/Spiked Ball/Spiked Ball.png";
        private const string SpikedChainPath = "Assets/Art/03_Bay/PA1_Traps/Spiked Ball/Chain.png";
        private const string MovingPlatformPath = "Assets/Art/03_Bay/PA1_Traps/Platforms/Grey On (32x8).png";
        private const string KiwiPath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Fruits/Kiwi.png";
        private const string StrawberryPath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Fruits/Strawberry.png";
        private const string StartSignPath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Checkpoints/Start/Start (Moving) (64x64).png";
        private const string TrophyPath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Checkpoints/End/End (Idle).png";

        [MenuItem("Tools/One Hit Down/Build Level 03 Scene (Catacombs of Ice & Ember)", false, 30)]
        public static void BuildLevel03Menu()
        {
            BuildLevel03Scene(openScene: true);
        }

        [InitializeOnLoadMethod]
        private static void OnProjectLoaded()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(ScenePath))
                {
                    Debug.Log("<b>[Level03SceneBuilder]</b> Chưa có Scene Level_03.unity. Đang tự động tạo hoàn chỉnh...");
                    BuildLevel03Scene(openScene: false);
                }
            };
        }

        /// <summary>
        /// Tạo mới hoàn toàn Scene Level_03.unity đúng 100% tài nguyên Màn 3 và 5 ảnh thiết kế.
        /// </summary>
        public static void BuildLevel03Scene(bool openScene = true)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Khởi tạo Scene mới...", 0.05f);
                EnsureDirectory(SceneDir);

                // 1. Nạp toàn bộ Sprite chính thức từ Assets/Art/
                Sprite stoneSprite = EnsureStoneBlockSprite(); // Khối đá 9-slice: viền gạch 1 tile, lõi tối, không lặp cụm sprite
                if (stoneSprite == null)
                {
                    Debug.LogError("[Level03SceneBuilder] Không tạo được sprite khối đá 9-slice từ Terrain (16x16).png. Hủy dựng Scene.");
                    return;
                }
                Sprite thinPlatformSprite = LoadFirstSprite(TerrainTexturePath, "Terrain (16x16)_3"); // Thanh bệ mỏng One-Way
                Sprite bgSprite = LoadFirstSprite(BackgroundTexturePath, "Gray_0"); // Phông nền xám Màn 3
                Sprite playerSprite = LoadFirstSprite(PlayerSpritePath, "Idle (32x32)_0"); // Ninja Frog

                // 2. Tạo Scene sạch trong Editor
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // 3. Hierarchy phân nhóm quản lý theo đúng quy chuẩn
                GameObject envGroup = new GameObject("--- 01. ENVIRONMENT & BACKGROUND ---");
                GameObject terrainGroup = new GameObject("--- 02. TERRAIN & PLATFORMS ---");
                GameObject module3Group = new GameObject("--- 03. MODULE 3 PREFABS (CHINH THUC - TUYEN) ---");
                GameObject mockGroup2 = new GameObject("--- 04. [MOCK FOR TEST - CHO NGUOI 2 HOAN THIEN: LUA, GAI, CAU GAI] ---");
                GameObject mockGroup4 = new GameObject("--- 05. [MOCK FOR TEST - CHO NGUOI 4 HOAN THIEN: BE NANG, KIWI, DAU TAY, DICH] ---");
                GameObject playerGroup = new GameObject("--- 06. CHARACTERS ---");
                GameObject labelsGroup = new GameObject("--- 07. HUD LABELS & SIGNS ---");

                // 4. Background xám Gray.png lặp toàn màn hình
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Tạo Background...", 0.15f);
                CreateBackground(envGroup.transform, bgSprite);

                // 5. Camera 2D theo dõi mượt mà
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Thiết lập Camera 2D...", 0.25f);
                GameObject cameraGo = CreateCamera2D(envGroup.transform);

                // 6. Player Ninja Frog đầy đủ Animator nhún thở
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Thiết lập Player...", 0.35f);
                GameObject playerGo = CreateTestPlayer(playerGroup.transform, playerSprite);
                DevCameraFollow camFollow = cameraGo.GetComponent<DevCameraFollow>();
                if (camFollow != null && playerGo != null)
                {
                    camFollow.SetTarget(playerGo.transform);
                }

                // 7. Địa hình đá xám tro Terrain (16x16)_0 và nền móng liền mạch
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Tạo Địa hình gạch sàn, vách đá...", 0.50f);
                BuildAllStoneTerrain(terrainGroup.transform, stoneSprite, thinPlatformSprite);

                // 8. Các khối Sàn Băng IceRoad.prefab chính thức (dày 4 tiles từ y=0 đến y=4, khớp hoàn toàn)
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Bố trí các Khối Băng IceRoad.prefab...", 0.65f);
                BuildAllIceBlocks(module3Group.transform);

                // 9. 5 Cơ quan Module 3 còn lại của Tuyên (Trampoline, FallingPlatform, Fan, DeathPit, Arrow)
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Bố trí 5 Cơ quan Module 3...", 0.75f);
                BuildModule3Gadgets(module3Group.transform);

                // 10. Mock-Safe Người 2 (Vạc Lửa chu kỳ 2s-1s, Chông Gai, Cầu Gai Con Lắc)
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Bố trí Vạc Lửa, Gai, Cầu Gai...", 0.85f);
                BuildModule2Mocks(mockGroup2.transform);

                // 11. Mock-Safe Người 4 (Bệ Nâng X/Y, Kiwi [3/3], Dâu Tây, Cúp Đích)
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Bố trí Bệ Nâng, Kiwi, Dâu Tây, Cúp Đích...", 0.92f);
                BuildModule4Mocks(mockGroup4.transform);

                // 12. Bảng hiệu hướng dẫn 5 chặng
                CreateHudLabels(labelsGroup.transform);

                // 13. Lưu và mở Scene
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Lưu Scene Level_03.unity...", 0.98f);
                bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (saved)
                {
                    Debug.Log($"<b>[Level03SceneBuilder]</b> TẠO THÀNH CÔNG SCENE: <color=green>{ScenePath}</color>");
                    if (openScene)
                    {
                        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                    }
                    EditorUtility.DisplayDialog(
                        "One Hit Down - Màn 3",
                        $"Đã tạo thành công Màn 3 hoàn chỉnh:\n{ScenePath}\n\n" +
                        "★ 100% sử dụng Asset gốc của Màn 3 (Terrain đá xám, Băng Sand Mud Ice, Gray bg)\n" +
                        "★ Toàn bộ 6 Cơ quan Module 3 lấy trực tiếp từ Assets/Prefabs/Traps/\n" +
                        "★ Khối đá và khối băng đều có đáy tại Y=0, khớp khít hoàn toàn không còn khe hở\n" +
                        "★ Nền móng đá xám tro mỏng, chia đoạn, chừa trống dưới các hố sâu\n" +
                        "★ Đầy đủ Mock-Safe theo Docs/LEVEL_03_MOCK_NOTICE.md!",
                        "Tuyệt vời! Chơi thử ngay"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Level03SceneBuilder] Lỗi tạo Scene: {ex}");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #region Setup Core (Camera, Background, Player)

        private static GameObject CreateCamera2D(Transform parent)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.transform.SetParent(parent);
            camGo.transform.position = new Vector3(2.5f, 6.0f, -10f);
            camGo.tag = "MainCamera";

            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.0f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.16f, 1f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;

            camGo.AddComponent<AudioListener>();

            DevCameraFollow follow = camGo.AddComponent<DevCameraFollow>();
            SerializedObject so = new SerializedObject(follow);
            so.FindProperty("offset").vector3Value = new Vector3(0f, 2.0f, -10f);
            so.FindProperty("smoothSpeed").floatValue = 6f;
            // Giới hạn biên camera: đáy camera không bao giờ tụt xuống dưới đáy sàn
            so.FindProperty("minBounds").vector2Value = new Vector2(-2f, 5.5f);
            so.FindProperty("maxBounds").vector2Value = new Vector2(208f, 22f);
            so.ApplyModifiedProperties();

            return camGo;
        }

        private static void CreateBackground(Transform parent, Sprite bgSprite)
        {
            if (bgSprite == null) return;
            GameObject bgGo = new GameObject("Background_Gray");
            bgGo.transform.SetParent(parent);
            bgGo.transform.position = new Vector3(100f, 10f, 10f);

            SpriteRenderer sr = bgGo.AddComponent<SpriteRenderer>();
            sr.sprite = bgSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(230f, 25f);
            sr.sortingOrder = -50;
        }

        private static GameObject CreateTestPlayer(Transform parent, Sprite playerSprite)
        {
            GameObject playerGo = new GameObject("Player_NinjaFrog");
            playerGo.transform.SetParent(parent);
            playerGo.transform.position = new Vector3(2.5f, 4.1f, 0f);
            playerGo.tag = "Player";

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0) playerGo.layer = playerLayer;

            SpriteRenderer sr = playerGo.AddComponent<SpriteRenderer>();
            sr.sprite = playerSprite;
            sr.sortingOrder = 10;

            BoxCollider2D col = playerGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.3f);
            col.offset = new Vector2(0f, 0.65f); // Đáy collider trùng chân y = 0

            Animator animator = playerGo.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerControllerAssetPath);

            Rigidbody2D rb = playerGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            rb.gravityScale = 3.5f;

            PlayerController pc = playerGo.AddComponent<PlayerController>();
            SerializedObject so = new SerializedObject(pc);
            so.FindProperty("moveSpeed").floatValue = 8f;
            so.FindProperty("groundAcceleration").floatValue = 65f;
            so.FindProperty("groundDeceleration").floatValue = 80f;
            so.FindProperty("airAcceleration").floatValue = 45f;
            so.FindProperty("iceFriction").floatValue = 4f;
            so.FindProperty("jumpForce").floatValue = 15.5f;
            so.FindProperty("doubleJumpForce").floatValue = 13.5f;
            so.FindProperty("defaultGravityScale").floatValue = 3.5f;
            so.FindProperty("coyoteTime").floatValue = 0.1f;
            so.FindProperty("jumpBufferTime").floatValue = 0.1f;
            so.FindProperty("groundCheckSize").vector2Value = new Vector2(0.55f, 0.12f);
            so.FindProperty("groundLayer").intValue = 1 << LayerMask.NameToLayer("Ground");
            so.ApplyModifiedProperties();

            return playerGo;
        }

        #endregion

        #region Terrain & Platforms (Đá xám tro Terrain (16x16)_0)

        private static void BuildAllStoneTerrain(Transform parent, Sprite stoneSprite, Sprite thinPlatformSprite)
        {
            // === NỀN MÓNG ĐÁ XÁM TRO (Y: -4 đến 0, tâm Y = -2, dày 4 tiles), chia đoạn, chỉ để làm nền thị giác ===
            // Khe hở nằm dưới các hố sâu nên hố đọc như vực thật thay vì một khối đặc kéo dài cả màn
            CreateStoneFoundationSegments(parent, stoneSprite);

            // === PHẦN 1: KHỞI ĐẦU (0-40, Sàn y=0 đến y=4, có hố gai ở cuối dải băng thứ hai) ===
            // 1. Thềm xuất phát: X: 0 đến 8, Y: 0 đến 4
            CreateStoneBox("P1_StartLedge", new Vector2(4f, 2f), new Vector2(8f, 4f), parent, stoneSprite);

            // 2. Gờ đá vạc lửa đơn: X: 12 đến 24, Y: 0 đến 4 (dải băng làm quen đứng trước ngắn lại còn 4 tiles)
            CreateStoneBox("P1_FirePlatform", new Vector2(18f, 2f), new Vector2(12f, 4f), parent, stoneSprite);

            // 3. Hố gai cuối dải băng thứ hai: X: 33 đến 37, đáy Y: 0 đến 1 (băng trượt thẳng ra mép vực)
            CreateStoneBox("P1_SpikePitBottom", new Vector2(35f, 0.5f), new Vector2(4f, 1f), parent, stoneSprite);

            // Sau hố gai (X: 37 đến 48) là dải băng P2_IceRoad_Runup, không còn thềm đá nối

            // 4. Thanh bệ dài mảnh One-Way đỡ Dâu Tây #1 trên cao: X: 27 đến 31, Y: 11.5
            CreateOneWayPlatform("P1_OneWay_Berry1", new Vector2(29f, 11.5f), new Vector2(4f, 0.4f), parent, thinPlatformSprite);

            // 5. Bệ trần treo gai Hình 1: X: 16 đến 24, Y: 19 đến 20
            CreateStoneBox("P1_CeilingBox", new Vector2(20f, 19.5f), new Vector2(8f, 1f), parent, stoneSprite);

            // === PHẦN 2: KIWI 1 (40-80, y=4 -> y=6) ===
            // 6. Bệ gạch đỡ cụm vạc lửa Kiwi 1: X: 48 đến 56, Y: 0 đến 4
            CreateStoneBox("P2_FirePlatform", new Vector2(52f, 2f), new Vector2(8f, 4f), parent, stoneSprite);

            // 7. Sàn đáy hố gai: X: 56 đến 60, Y: 0 đến 1 (đáy sâu y=1)
            CreateStoneBox("P2_SpikePitBottom", new Vector2(58f, 0.5f), new Vector2(4f, 1f), parent, stoneSprite);

            // 8. Bệ đất đặt đệm lò xo Trampoline: X: 68 đến 73, Y: 0 đến 4
            CreateStoneBox("P2_TrampolineLedge", new Vector2(70.5f, 2f), new Vector2(5f, 4f), parent, stoneSprite);

            // 9. Vách tường cao bước lên y=6 (Nối sang Hình 3): X: 73 đến 80, Y: 0 đến 6 (cao 6 tiles)
            CreateStoneBox("P2_StepUpLedge", new Vector2(76.5f, 3f), new Vector2(7f, 6f), parent, stoneSprite);

            // 10. Bệ trần treo gai Hình 2: X: 56 đến 62, Y: 19 đến 20
            CreateStoneBox("P2_CeilingBox", new Vector2(59f, 19.5f), new Vector2(6f, 1f), parent, stoneSprite);

            // === PHẦN 3: KIWI 2 & HỒ LỬA NGẦM (80-120) ===
            // 11. Gờ đất đầu Hình 3: X: 80 đến 82, Y: 0 đến 6 (top ở y=6)
            CreateStoneBox("P3_LeftLedge", new Vector2(81f, 3f), new Vector2(2f, 6f), parent, stoneSprite);

            // 12. Đáy hồ lửa ngầm: X: 82 đến 118, Y: 0 đến 1 (sàn đáy sâu y=1)
            CreateStoneBox("P3_LavaPitFloor", new Vector2(100f, 0.5f), new Vector2(36f, 1f), parent, stoneSprite);

            // 13. Cột trụ đứng ở giữa hồ: X: 97 đến 101, Y: 0 đến 5 (đỡ bệ băng ở y=5..6)
            CreateStoneBox("P3_CenterPillarBase", new Vector2(99f, 2.5f), new Vector2(4f, 5f), parent, stoneSprite);

            // 14. Bệ đỡ Dâu Tây #2 trên cao: X: 113 đến 116, Y: 9.5
            CreateStoneBox("P3_Berry2Ledge", new Vector2(114.5f, 9.5f), new Vector2(3f, 0.5f), parent, stoneSprite);

            // 15. Vách núi cao bên phải đón người chơi (Nối sang Hình 4): X: 118 đến 122, Y: 0 đến 8 (cao 8 tiles)
            CreateStoneBox("P3_HighWallRight", new Vector2(120f, 4f), new Vector2(4f, 8f), parent, stoneSprite);

            // 16. Bệ trần treo gai Hình 3: X: 96 đến 104, Y: 19 đến 20
            CreateStoneBox("P3_CeilingBox", new Vector2(100f, 19.5f), new Vector2(8f, 1f), parent, stoneSprite);

            // === PHẦN 4: KIWI 3 & DỐC BĂNG GIẬT CẤP (120-160) ===
            // 17. Khối đất đỡ bậc băng 1 (y=8): X: 121 đến 128, Y: 0 đến 5
            CreateStoneBox("P4_Step1_Base", new Vector2(124.5f, 2.5f), new Vector2(7f, 5f), parent, stoneSprite);

            // 18. Khối đất đỡ bậc băng 2 (y=6): X: 128 đến 135, Y: 0 đến 3
            CreateStoneBox("P4_Step2_Base", new Vector2(131.5f, 1.5f), new Vector2(7f, 3f), parent, stoneSprite);

            // 19. Khối đất đỡ bậc băng 3 (y=4): X: 135 đến 145, Y: 0 đến 1
            CreateStoneBox("P4_Step3_Base", new Vector2(140f, 0.5f), new Vector2(10f, 1f), parent, stoneSprite);

            // 20. Đáy hố tử thần DeathPit: X: 145 đến 148, Y: 0 đến 1
            CreateStoneBox("P4_DeathPitBottom", new Vector2(146.5f, 0.5f), new Vector2(3f, 1f), parent, stoneSprite);

            // 21. Thềm đá ngắn làm chỗ nghỉ giữa hai dải băng: X: 156 đến 158, Y: 0 đến 4
            CreateStoneBox("P4_ExitLedge", new Vector2(157f, 2f), new Vector2(2f, 4f), parent, stoneSprite);

            // 22. Bệ trần treo gai Hình 4: X: 136 đến 144, Y: 19 đến 20
            CreateStoneBox("P4_CeilingBox", new Vector2(140f, 19.5f), new Vector2(8f, 1f), parent, stoneSprite);

            // === PHẦN 5: VỀ ĐÍCH (160-200) ===
            // 23. Đáy vực lửa cuối: X: 168 đến 175, Y: 0 đến 1 (sàn đáy sâu y=1)
            CreateStoneBox("P5_FinalPitBottom", new Vector2(171.5f, 0.5f), new Vector2(7f, 1f), parent, stoneSprite);

            // 24. Thềm Đích Cuối Màn: X: 187 đến 205, Y: 0 đến 4 (dài 18 units, dải băng cuối kéo dài tới X=187)
            CreateStoneBox("P5_VictoryAltarPlatform", new Vector2(196f, 2f), new Vector2(18f, 4f), parent, stoneSprite);

            // 25. Bệ trần treo gai và cầu gai Hình 5: X: 177 đến 185, Y: 19 đến 20
            CreateStoneBox("P5_CeilingBox", new Vector2(181f, 19.5f), new Vector2(8f, 1f), parent, stoneSprite);
        }

        private static GameObject CreateStoneBox(string name, Vector2 center, Vector2 size, Transform parent, Sprite sprite)
        {
            GameObject boxGo = new GameObject(name);
            boxGo.transform.SetParent(parent);
            boxGo.transform.position = new Vector3(center.x, center.y, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) boxGo.layer = groundLayer;

            SpriteRenderer sr = boxGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Sliced; // 9-slice: 4 góc giữ nguyên 1 tile, cạnh và lõi theo size
            sr.tileMode = SpriteTileMode.Continuous; // Cạnh gạch lặp theo tile thay vì bị kéo giãn
            sr.size = size;
            sr.sortingOrder = 1;

            BoxCollider2D col = boxGo.AddComponent<BoxCollider2D>();
            col.size = size;
            col.offset = Vector2.zero;

            return boxGo;
        }

        /// <summary>
        /// Nền móng đá xám tro trải dài dưới đáy màn hình (Y: -6 đến 0), vẽ bằng khối 9-slice, không có Collider, cùng màu bệ đá.
        /// </summary>
        private static void CreateStoneFoundation(string name, Vector2 center, Vector2 size, Transform parent, Sprite sprite)
        {
            GameObject baseGo = new GameObject(name);
            baseGo.transform.SetParent(parent);
            baseGo.transform.position = new Vector3(center.x, center.y, 0f);

            SpriteRenderer sr = baseGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = size;
            sr.color = Color.white; // Cùng màu với các bệ đá như P1_FirePlatform
            sr.sortingOrder = -1; // Nằm phía sau các bệ đá và khối băng (sortingOrder 1..3)
        }

        /// <summary>
        /// Nền móng chia đoạn (Y: -4 đến 0, tâm Y = -2, dày 4 tiles). Chừa trống dưới hố lửa ngầm, Hố Tử Thần và vực cuối
        /// để các hố đọc như vực thật; mỗi đoạn là một khối 9-slice riêng, không Collider.
        /// Đáy hạ xuống Y=-4 để phủ kín dải nền phông lộ ra dưới đáy cũ (Y=-2).
        /// </summary>
        private static void CreateStoneFoundationSegments(Transform parent, Sprite sprite)
        {
            // Mỗi phần tử: X bắt đầu, X kết thúc
            float[,] segments =
            {
                { -8f, 82f },    // Phần 1-2 và thềm đầu hồ lửa
                { 118f, 145f },  // Vách núi và 3 bậc băng
                { 148f, 168f },  // Sau Hố Tử Thần đến mép vực cuối
                { 175f, 212f }   // Thềm vực cuối đến Đích
            };

            for (int i = 0; i < segments.GetLength(0); i++)
            {
                float start = segments[i, 0];
                float end = segments[i, 1];
                CreateStoneFoundation($"Stone_Foundation_{i + 1}", new Vector2((start + end) * 0.5f, -2f), new Vector2(end - start, 4f), parent, sprite);
            }
        }

        private static void CreateOneWayPlatform(string name, Vector2 center, Vector2 size, Transform parent, Sprite sprite)
        {
            GameObject platGo = new GameObject(name);
            platGo.transform.SetParent(parent);
            platGo.transform.position = new Vector3(center.x, center.y, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) platGo.layer = groundLayer;

            SpriteRenderer sr = platGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingOrder = 2;

            BoxCollider2D col = platGo.AddComponent<BoxCollider2D>();
            col.size = size;
            col.offset = Vector2.zero;
            col.usedByEffector = true;

            PlatformEffector2D effector = platGo.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 180f;
        }

        #endregion

        #region Ice Blocks (Sử dụng trực tiếp IceRoad.prefab chính thức của Tuyên)

        // Ô băng 16x16 đặc, dùng lặp Tiled cho mọi khối IceRoad để không còn khoảng trống
        private static Sprite iceTileSprite;

        private static void BuildAllIceBlocks(Transform parent)
        {
            iceTileSprite = EnsureIceTileSprite();
            if (iceTileSprite == null)
            {
                Debug.LogWarning("[Level03SceneBuilder] Không tạo được ô băng 16x16, giữ sprite gốc của IceRoad.prefab.");
            }

            // === PHẦN 1: KHỞI ĐẦU ===
            // Sàn băng làm quen (X: 8 đến 12, Y: 0 đến 4): ngắn 4 tiles, phía sau chỉ là nền đá, chưa có bẫy
            InstantiateIceRoad(parent, "P1_IceRoad_Intro", new Vector2(10.0f, 2.0f), new Vector2(4f, 4f));
            // Sàn băng dài hơn (X: 24 đến 33, Y: 0 đến 4): trượt thẳng ra mép hố gai P1_SpikePitBottom
            InstantiateIceRoad(parent, "P1_IceRoad_Cont", new Vector2(28.5f, 2.0f), new Vector2(9f, 4f));

            // === PHẦN 2: KIWI 1 ===
            // Đường trượt băng dài nhất đoạn đầu (X: 37 đến 48, Y: 0 đến 4): đáp xuống sau hố gai, trượt tới cụm vạc lửa ở cuối
            InstantiateIceRoad(parent, "P2_IceRoad_Runup", new Vector2(42.5f, 2.0f), new Vector2(11f, 4f));
            // Sàn băng tiếp đất sau hố gai (X: 60 đến 68, Y: 0 đến 4, dày 4 tiles)
            InstantiateIceRoad(parent, "P2_IceRoad_Landing", new Vector2(64.0f, 2.0f), new Vector2(8f, 4f));

            // === PHẦN 3: KIWI 2 ===
            // Sàn băng trên đỉnh cột trụ giữa hồ lửa (X: 97 đến 101, Y: 5 đến 6, dày 1 tile)
            InstantiateIceRoad(parent, "P3_IceRoad_CenterPillar", new Vector2(99.0f, 5.5f), new Vector2(4f, 1f));

            // === PHẦN 4: KIWI 3 & 3 BẬC DỐC BĂNG GIẬT CẤP ===
            // Bậc 1 (X: 121 đến 128, Y: 5 đến 8, dày 3 tiles, ngồi trên trụ đá y=0..5)
            InstantiateIceRoad(parent, "P4_IceRoad_Slope1", new Vector2(124.5f, 6.5f), new Vector2(7f, 3f));
            // Bậc 2 (X: 128 đến 135, Y: 3 đến 6, dày 3 tiles, ngồi trên trụ đá y=0..3)
            InstantiateIceRoad(parent, "P4_IceRoad_Slope2", new Vector2(131.5f, 4.5f), new Vector2(7f, 3f));
            // Bậc 3 (X: 135 đến 145, Y: 1 đến 4, dày 3 tiles, ngồi trên gờ đá y=0..1)
            InstantiateIceRoad(parent, "P4_IceRoad_Slope3", new Vector2(140.0f, 2.5f), new Vector2(10f, 3f));
            // Sàn băng sau Hố Tử Thần (X: 148 đến 156, Y: 0 đến 4, dày 4 tiles)
            InstantiateIceRoad(parent, "P4_IceRoad_AfterPit", new Vector2(152.0f, 2.0f), new Vector2(8f, 4f));

            // === PHẦN 5: VỀ ĐÍCH ===
            // Thềm băng trước vực lửa (X: 158 đến 168, Y: 0 đến 4, dài 10 tiles): trượt thẳng ra mép vực lửa
            InstantiateIceRoad(parent, "P5_IceRoad_PrePit", new Vector2(163.0f, 2.0f), new Vector2(10f, 4f));
            // Thềm băng dưới cầu gai con lắc (X: 175 đến 187, Y: 0 đến 4, dài 12 tiles): dải băng dài nhất màn
            InstantiateIceRoad(parent, "P5_IceRoad_PendulumZone", new Vector2(181.0f, 2.0f), new Vector2(12f, 4f));
        }

        private static void InstantiateIceRoad(Transform parent, string name, Vector2 center, Vector2 size)
        {
            GameObject prefab = LoadPrefab("IceRoad");
            if (prefab == null) return;

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            inst.name = name;
            inst.transform.position = new Vector3(center.x, center.y, 0f);

            // SpriteRenderer khối băng
            SpriteRenderer sr = inst.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Sprite gốc 49x80 có viền trong suốt và phần thân hẹp nên lặp ra khoảng trống; dùng ô lõi 16x16 đặc
                if (iceTileSprite != null) sr.sprite = iceTileSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.tileMode = SpriteTileMode.Continuous;
                sr.size = size;
                sr.sortingOrder = 3;
            }

            // Đổi sprite trong Editor làm Unity tự co scale Transform (16/49 x 16/80); trả về 1 để size, collider, hạt bụi đúng kích thước
            inst.transform.localScale = Vector3.one;

            // Collider Solid & Trigger
            foreach (BoxCollider2D col in inst.GetComponents<BoxCollider2D>())
            {
                if (col.isTrigger)
                {
                    col.size = new Vector2(size.x, size.y + 0.2f);
                    col.offset = new Vector2(0f, 0.1f);
                }
                else
                {
                    col.size = size;
                    col.offset = Vector2.zero;
                }
            }

            // Hạt bụi tuyết trên mặt băng
            ParticleSystem dust = inst.GetComponentInChildren<ParticleSystem>();
            if (dust != null)
            {
                dust.transform.localPosition = new Vector3(0f, size.y * 0.5f, 0f);
                ParticleSystem.ShapeModule shape = dust.shape;
                shape.scale = new Vector3(size.x, 0.1f, 0.1f);
            }
        }

        #endregion

        #region Module 3 Gadgets (Trampoline, FallingPlatform, Fan, DeathPit, Arrow)

        private static void BuildModule3Gadgets(Transform parent)
        {
            // 1. ĐỆM LÒ XO TRAMPOLINE (Hình 2: X=70.5, Y=4.25f, nảy lên vách y=6)
            GameObject trampGo = InstantiatePrefab("Trampoline", parent);
            if (trampGo != null)
            {
                trampGo.name = "P2_Trampoline_To_P3";
                trampGo.transform.position = new Vector3(70.5f, 4.25f, 0f);
            }

            // 2. BỆ ĐÁ SỤP FALLING PLATFORM (Hình 1: Bệ 1 tại x=18, y=8.0; Bệ 2 tại x=23, y=10.0)
            GameObject fp1 = InstantiatePrefab("FallingPlatform", parent);
            if (fp1 != null)
            {
                fp1.name = "P1_FallingPlatform_1";
                fp1.transform.position = new Vector3(18.0f, 8.0f, 0f);
            }

            GameObject fp2 = InstantiatePrefab("FallingPlatform", parent);
            if (fp2 != null)
            {
                fp2.name = "P1_FallingPlatform_2";
                fp2.transform.position = new Vector3(23.0f, 10.0f, 0f);
            }

            // 3. QUẠT GIÓ FANTRAP THỔI NGANG (Hình 4: gắn vách trái x=120.5, y=9.0 thổi sang phải)
            GameObject fanH = InstantiatePrefab("FanTrap", parent);
            if (fanH != null)
            {
                fanH.name = "P4_FanTrap_Horizontal";
                fanH.transform.position = new Vector3(120.5f, 9.0f, 0f);
                FanTrap ft = fanH.GetComponent<FanTrap>();
                if (ft != null)
                {
                    SerializedObject so = new SerializedObject(ft);
                    so.FindProperty("windDirection").vector2Value = Vector2.right;
                    so.FindProperty("windForce").floatValue = 15f;
                    so.FindProperty("isActive").boolValue = true;
                    so.ApplyModifiedProperties();

                    Transform wz = fanH.transform.Find("WindZone");
                    if (wz != null)
                    {
                        BoxCollider2D wzCol = wz.GetComponent<BoxCollider2D>();
                        if (wzCol != null)
                        {
                            wzCol.size = new Vector2(10f, 3.0f);
                            wzCol.offset = new Vector2(5.0f, 0f);
                        }
                    }
                }
            }

            // 4. HỐ TỬ THẦN DEATHPIT (Hình 4: X=145 đến 148, Y=4.0, rộng đúng 3 ô = 48px)
            GameObject dp1 = InstantiatePrefab("DeathPit", parent);
            if (dp1 != null)
            {
                dp1.name = "P4_DeathPit_Block1";
                dp1.transform.position = new Vector3(145.5f, 4.0f, 0f);
            }

            GameObject dp2 = InstantiatePrefab("DeathPit", parent);
            if (dp2 != null)
            {
                dp2.name = "P4_DeathPit_Block2";
                dp2.transform.position = new Vector3(146.875f, 4.0f, 0f);
            }

            // 5. MŨI TÊN ĐẨY KHÍ ARROWBOOSTER:
            // Hình 3: Treo lơ lửng tại X=109, Y=7.0 phóng lên Dâu Tây #2
            GameObject ab1 = InstantiatePrefab("ArrowBooster", parent);
            if (ab1 != null)
            {
                ab1.name = "P3_ArrowBooster_To_Berry2";
                ab1.transform.position = new Vector3(109.0f, 7.0f, 0f);
                SetupArrowBooster(ab1, new Vector2(1f, 0.8f).normalized, 14f);
            }

            // Hình 5: Phóng vượt vực lửa cuối (X=171.5, Y=5.5)
            GameObject ab2 = InstantiatePrefab("ArrowBooster", parent);
            if (ab2 != null)
            {
                ab2.name = "P5_ArrowBooster_FinalPit";
                ab2.transform.position = new Vector3(171.5f, 5.5f, 0f);
                SetupArrowBooster(ab2, new Vector2(1f, 0.7f).normalized, 14f);
            }
        }

        private static void SetupArrowBooster(GameObject abGo, Vector2 dir, float force)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            abGo.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            ArrowBooster ab = abGo.GetComponent<ArrowBooster>();
            if (ab != null)
            {
                SerializedObject so = new SerializedObject(ab);
                so.FindProperty("boostDirection").vector2Value = dir;
                so.FindProperty("boostForce").floatValue = force;
                so.FindProperty("reactivateDelay").floatValue = 2.5f;
                so.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Module 2 Mocks (Vạc Lửa, Chông Gai, Cầu Gai Con Lắc)

        private static void BuildModule2Mocks(Transform parent)
        {
            Sprite fireOff = LoadFirstSprite(FireOffPath, "Off_0");
            Sprite fireHit = LoadFirstSprite(FireHitPath, "Hit (16x32)_0");
            Sprite[] fireOnFrames = LoadIndexedSprites(FireOnPath, "On (16x32)", 3);
            Sprite spikesSprite = LoadFirstSprite(SpikesTexturePath, "Idle_0");

            // 1. CHÔNG GAI TRẦN VÀ SÀN:
            CreateSpikeRow("P1_CeilingSpikes", new Vector2(20f, 18.25f), 7, parent, spikesSprite, pointingDown: true);
            CreateSpikeRow("P2_CeilingSpikes", new Vector2(59f, 18.25f), 6, parent, spikesSprite, pointingDown: true);
            CreateSpikeRow("P3_CeilingSpikes", new Vector2(100f, 18.25f), 6, parent, spikesSprite, pointingDown: true);
            CreateSpikeRow("P4_CeilingSpikes", new Vector2(140f, 18.25f), 6, parent, spikesSprite, pointingDown: true);
            CreateSpikeRow("P5_CeilingSpikes", new Vector2(181f, 18.25f), 6, parent, spikesSprite, pointingDown: true);

            // Hố chông gai cuối dải băng thứ hai ở Hình 1 (X: 33 đến 37, Y = 1.25)
            CreateSpikeRow("P1_FloorSpikes_Pit", new Vector2(35f, 1.25f), 4, parent, spikesSprite, pointingDown: false);

            // Hố chông gai sàn Hình 2 (X: 56.5 đến 59.5, Y = 1.25) — bắt buộc Nhảy Đôi
            CreateSpikeRow("P2_FloorSpikes_Pit", new Vector2(58f, 1.25f), 4, parent, spikesSprite, pointingDown: false);

            // 2. VẠC LỬA CHU KỲ (2s tắt - 0.35s báo - 1s bật):
            // Hình 1: Vạc Lửa Đơn (X=20.5, Y=4.0)
            CreateMockFireTrap(parent, "P1_FireTrap_Single", new Vector3(20.5f, 4.0f, 0f), fireOff, fireHit, fireOnFrames);

            // Hình 2: Cụm 4 Vạc Lửa dưới Kiwi 1 (X=50.0, 51.5, 53.0, 54.5 tại Y=4.0)
            for (int i = 0; i < 4; i++)
            {
                CreateMockFireTrap(parent, $"P2_FireTrap_Kiwi1_{i}", new Vector3(50.0f + i * 1.5f, 4.0f, 0f), fireOff, fireHit, fireOnFrames);
            }

            // Hình 3: Hồ Lửa Ngầm Liên Hoàn ở đáy vực (X: 83 đến 117 tại Y=1.0)
            for (float x = 83.5f; x <= 116.5f; x += 1.8f)
            {
                if (x >= 96.5f && x <= 101.5f) continue;
                CreateMockFireTrap(parent, $"P3_LavaFireTrap_{x:F0}", new Vector3(x, 1.0f, 0f), fireOff, fireHit, fireOnFrames);
            }

            // Hình 4: 3 Vạc lửa đáy hố tử thần DeathPit (X: 145.5, 146.5, 147.5 tại Y=1.0)
            for (int i = 0; i < 3; i++)
            {
                CreateMockFireTrap(parent, $"P4_DeathPitFire_{i}", new Vector3(145.5f + i * 1.0f, 1.0f, 0f), fireOff, fireHit, fireOnFrames);
            }

            // Hình 5: 4 Vạc lửa vực cuối (X: 169.5, 171.0, 172.5, 174.0 tại Y=1.0)
            for (int i = 0; i < 4; i++)
            {
                CreateMockFireTrap(parent, $"P5_FinalPitFire_{i}", new Vector3(169.5f + i * 1.4f, 1.0f, 0f), fireOff, fireHit, fireOnFrames);
            }

            // 3. CẦU GAI CON LẮC XÍCH SẮT (Hình 5: treo từ trần x=181, y=19 xuống y=6)
            BuildMockPendulum(parent, new Vector3(181.0f, 18.5f, 0f), length: 12f);
        }

        private static void CreateSpikeRow(string name, Vector2 center, int count, Transform parent, Sprite sprite, bool pointingDown)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent);
            row.transform.position = new Vector3(center.x, center.y, 0f);

            int trapsLayer = LayerMask.NameToLayer("Traps");
            if (trapsLayer >= 0) row.layer = trapsLayer;
            row.tag = "Hazard";

            float startX = -((count - 1) * 0.5f);
            for (int i = 0; i < count; i++)
            {
                GameObject sGo = new GameObject($"Spike_{i}");
                sGo.transform.SetParent(row.transform);
                sGo.transform.localPosition = new Vector3(startX + i * 1.0f, 0f, 0f);
                if (pointingDown)
                {
                    sGo.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
                }

                SpriteRenderer sr = sGo.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 5;
            }

            BoxCollider2D col = row.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(count * 1.0f, 0.6f);
            col.offset = Vector2.zero;
        }

        private static void CreateMockFireTrap(Transform parent, string name, Vector3 pos, Sprite off, Sprite hit, Sprite[] onFrames)
        {
            GameObject fireGo = new GameObject(name);
            fireGo.transform.SetParent(parent);
            fireGo.transform.position = pos;

            int trapsLayer = LayerMask.NameToLayer("Traps");
            if (trapsLayer >= 0) fireGo.layer = trapsLayer;

            SpriteRenderer sr = fireGo.AddComponent<SpriteRenderer>();
            sr.sprite = off;
            sr.sortingOrder = 6;

            GameObject hitbox = new GameObject("FireHitbox");
            hitbox.transform.SetParent(fireGo.transform);
            hitbox.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            hitbox.tag = "Hazard";
            if (trapsLayer >= 0) hitbox.layer = trapsLayer;

            BoxCollider2D hitCol = hitbox.AddComponent<BoxCollider2D>();
            hitCol.isTrigger = true;
            hitCol.size = new Vector2(1.0f, 1.5f);

            MockFireTrap mft = fireGo.AddComponent<MockFireTrap>();
            SerializedObject so = new SerializedObject(mft);
            so.FindProperty("offTime").floatValue = 2.0f;
            so.FindProperty("warningTime").floatValue = 0.35f;
            so.FindProperty("activeTime").floatValue = 1.0f;
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("fireHitbox").objectReferenceValue = hitbox;
            so.FindProperty("offSprite").objectReferenceValue = off;
            so.FindProperty("warningSprite").objectReferenceValue = hit;

            SerializedProperty framesProp = so.FindProperty("activeFrames");
            if (onFrames != null)
            {
                framesProp.arraySize = onFrames.Length;
                for (int i = 0; i < onFrames.Length; i++)
                {
                    framesProp.GetArrayElementAtIndex(i).objectReferenceValue = onFrames[i];
                }
            }
            so.ApplyModifiedProperties();
        }

        private static void BuildMockPendulum(Transform parent, Vector3 pivotPos, float length)
        {
            GameObject pendulumGo = new GameObject("P5_SpikedBallPendulum");
            pendulumGo.transform.SetParent(parent);
            pendulumGo.transform.position = pivotPos;

            Sprite chainSprite = LoadFirstSprite(SpikedChainPath, "Chain_0");
            Sprite ballSprite = LoadFirstSprite(SpikedBallPath, "Spiked Ball_0");

            int chainCount = Mathf.RoundToInt(length / 0.5f);
            for (int i = 0; i < chainCount; i++)
            {
                GameObject link = new GameObject($"ChainLink_{i}");
                link.transform.SetParent(pendulumGo.transform);
                link.transform.localPosition = new Vector3(0f, -i * 0.5f, 0f);

                SpriteRenderer lsr = link.AddComponent<SpriteRenderer>();
                lsr.sprite = chainSprite;
                lsr.sortingOrder = 7;
            }

            GameObject ballGo = new GameObject("Ball_Hazard");
            ballGo.transform.SetParent(pendulumGo.transform);
            ballGo.transform.localPosition = new Vector3(0f, -length, 0f);
            ballGo.tag = "Hazard";

            int trapsLayer = LayerMask.NameToLayer("Traps");
            if (trapsLayer >= 0) ballGo.layer = trapsLayer;

            SpriteRenderer bsr = ballGo.AddComponent<SpriteRenderer>();
            bsr.sprite = ballSprite;
            bsr.sortingOrder = 8;

            CircleCollider2D col = ballGo.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.85f;

            MockSpikedBallPendulum pendulum = pendulumGo.AddComponent<MockSpikedBallPendulum>();
            SerializedObject so = new SerializedObject(pendulum);
            so.FindProperty("swingAngle").floatValue = 55f;
            so.FindProperty("swingSpeed").floatValue = 1.8f;
            so.FindProperty("armLength").floatValue = length;
            so.FindProperty("ballTransform").objectReferenceValue = ballGo.transform;
            so.ApplyModifiedProperties();
        }

        #endregion

        #region Module 4 Mocks (Bệ Nâng, Kiwi, Dâu Tây, Đích)

        private static void BuildModule4Mocks(Transform parent)
        {
            Sprite kiwiSprite = LoadFirstSprite(KiwiPath, "Kiwi_0");
            Sprite strawberrySprite = LoadFirstSprite(StrawberryPath, "Strawberry_0");
            Sprite platformSprite = LoadFirstSprite(MovingPlatformPath, "Grey On (32x8)_0");
            Sprite trophySprite = LoadFirstSprite(TrophyPath, "End (Idle)_0");
            Sprite startSignSprite = LoadFirstSprite(StartSignPath, "Start (Moving) (64x64)_0");

            // 1. BIỂN BÁO XUẤT PHÁT LEVEL START Ở ĐẦU MÀN (X=1.2, Y=4.0)
            if (startSignSprite != null)
            {
                GameObject startGo = new GameObject("P1_LevelStart_Sign");
                startGo.transform.SetParent(parent);
                startGo.transform.position = new Vector3(1.2f, 4.6f, 0f);
                SpriteRenderer sr = startGo.AddComponent<SpriteRenderer>();
                sr.sprite = startSignSprite;
                sr.sortingOrder = 4;
            }

            // 2. 3 TRÁI KIWI BẮT BUỘC [3/3] ĐỂ MỞ CỔNG:
            // Trái 1: Kiwi Lửa Thiêu [1/3] (X=52.5, Y=6.5) lơ lửng trên cụm 4 vạc lửa
            CreateCollectible(parent, "Kiwi_Relic_1", new Vector3(52.5f, 6.5f, 0f), kiwiSprite, isRelic: true, "Kiwi Lửa Thiêu [1/3]");

            // Trái 2: Kiwi Bệ Nâng [2/3] (X=104.0, Y=11.5) trên đỉnh hành trình bệ nâng dọc
            CreateCollectible(parent, "Kiwi_Relic_2", new Vector3(104.0f, 11.5f, 0f), kiwiSprite, isRelic: true, "Kiwi Bệ Nâng [2/3]");

            // Trái 3: Kiwi Đáy Dốc Băng [3/3] (X=144.0, Y=5.0) sát mép hố tử thần
            CreateCollectible(parent, "Kiwi_Relic_3", new Vector3(144.0f, 5.0f, 0f), kiwiSprite, isRelic: true, "Kiwi Đáy Dốc Băng [3/3]");

            // 3. 2 QUẢ THƯỞNG PHỤ: DÂU TÂY
            // Dâu Tây #1 ở Hình 1: X=29.0, Y=12.5 trên thanh bệ mỏng One-Way
            CreateCollectible(parent, "Berry_Bonus_1", new Vector3(29.0f, 12.5f, 0f), strawberrySprite, isRelic: false, "Dâu Tây Thưởng #1");

            // Dâu Tây #2 ở Hình 3: X=114.5, Y=11.0 trên bệ cao cạnh bệ nâng
            CreateCollectible(parent, "Berry_Bonus_2", new Vector3(114.5f, 11.0f, 0f), strawberrySprite, isRelic: false, "Dâu Tây Thưởng #2");

            // 4. BỆ NÂNG DI ĐỘNG (MovingPlatform):
            // Bệ Nâng Ngang X ở Hình 3: X: 84 -> 94, Y = 5.0
            CreateMovingPlatform(parent, "P3_MovingPlatform_X", new Vector3(84f, 5.0f, 0f), new Vector2(10f, 0f), speed: 3.5f, platformSprite);

            // Bệ Nâng Dọc Y ở Hình 3: X = 104, Y: 4.5 -> 10.0 (chở lên Kiwi 2)
            CreateMovingPlatform(parent, "P3_MovingPlatform_Y", new Vector3(104f, 4.5f, 0f), new Vector2(0f, 5.5f), speed: 3.0f, platformSprite);

            // 5. CÚP CHIẾN THẮNG / CỔNG ĐÍCH CUỐI MÀN (X=192.0, Y=5.0)
            CreateVictoryAltar(parent, new Vector3(192.0f, 5.0f, 0f), trophySprite);
        }

        private static void CreateCollectible(Transform parent, string name, Vector3 pos, Sprite sprite, bool isRelic, string itemName)
        {
            GameObject itemGo = new GameObject(name);
            itemGo.transform.SetParent(parent);
            itemGo.transform.position = pos;
            itemGo.tag = "Item";

            SpriteRenderer sr = itemGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 9;

            CircleCollider2D col = itemGo.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;

            MockCollectibleItem mock = itemGo.AddComponent<MockCollectibleItem>();
            SerializedObject so = new SerializedObject(mock);
            so.FindProperty("itemName").stringValue = itemName;
            so.FindProperty("isRelic").boolValue = isRelic;
            so.FindProperty("bobAmplitude").floatValue = 0.15f;
            so.FindProperty("bobSpeed").floatValue = 3.0f;
            so.ApplyModifiedProperties();
        }

        private static void CreateMovingPlatform(Transform parent, string name, Vector3 pos, Vector2 moveOffset, float speed, Sprite sprite)
        {
            GameObject platGo = new GameObject(name);
            platGo.transform.SetParent(parent);
            platGo.transform.position = pos;

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) platGo.layer = groundLayer;

            SpriteRenderer sr = platGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(2.5f, 0.5f);
            sr.sortingOrder = 4;

            BoxCollider2D col = platGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.5f, 0.5f);

            MockMovingPlatform mmp = platGo.AddComponent<MockMovingPlatform>();
            SerializedObject so = new SerializedObject(mmp);
            so.FindProperty("moveOffset").vector2Value = moveOffset;
            so.FindProperty("speed").floatValue = speed;
            so.ApplyModifiedProperties();
        }

        private static void CreateVictoryAltar(Transform parent, Vector3 pos, Sprite trophySprite)
        {
            GameObject altar = new GameObject("P5_VictoryAltar_Goal");
            altar.transform.SetParent(parent);
            altar.transform.position = pos;

            SpriteRenderer sr = altar.AddComponent<SpriteRenderer>();
            sr.sprite = trophySprite;
            sr.sortingOrder = 9;

            BoxCollider2D col = altar.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 2.0f);

            MockGoalTrigger goal = altar.AddComponent<MockGoalTrigger>();
        }

        #endregion

        #region HUD Labels & Signs (Chuẩn theo 5 ảnh thiết kế)

        private static void CreateHudLabels(Transform parent)
        {
            CreateLabel("HUD_Header1", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 1/5: KHỞI ĐẦU (0-40)]", new Vector3(20f, 16.5f, 0f), parent);
            CreateLabel("HUD_Header2", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 2/5: KIWI 1 (40-80)]", new Vector3(60f, 16.5f, 0f), parent);
            CreateLabel("HUD_Header3", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 3/5: KIWI 2 (80-120)]", new Vector3(100f, 16.5f, 0f), parent);
            CreateLabel("HUD_Header4", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 4/5: KIWI 3 (120-160)]", new Vector3(140f, 16.5f, 0f), parent);
            CreateLabel("HUD_Header5", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 5/5: VỀ ĐÍCH (160-200)]", new Vector3(180f, 16.5f, 0f), parent);

            CreateLabel("HUD_GoalSign", "[OPEN 3/3] CÚP CHIẾN THẮNG", new Vector3(192f, 7.5f, 0f), parent);
        }

        private static void CreateLabel(string name, string text, Vector3 position, Transform parent)
        {
            GameObject labelGo = new GameObject(name);
            labelGo.transform.SetParent(parent);
            labelGo.transform.position = position;

            TextMesh tm = labelGo.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 24;
            tm.characterSize = 0.08f;
            tm.color = new Color(1f, 0.95f, 0.8f, 0.95f);
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
        }

        #endregion

        #region Helpers

        private static GameObject InstantiatePrefab(string prefabName, Transform parent)
        {
            GameObject prefab = LoadPrefab(prefabName);
            if (prefab == null) return null;
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        }

        private static GameObject LoadPrefab(string prefabName)
        {
            string path = $"{PrefabsTrapsDir}/{prefabName}.prefab";
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go != null) return go;

            Debug.LogError($"[Level03SceneBuilder] Không tìm thấy Prefab chuẩn tại: {path}");
            return null;
        }

        /// <summary>
        /// Đảm bảo Terrain (16x16).png có sprite 9-slice "Terrain (16x16)_StoneBlock" (viền 16px).
        /// Chỉ thêm 1 sprite mới vào danh sách slice, giữ nguyên các sprite sẵn có và GUID của chúng.
        /// </summary>
        private static Sprite EnsureStoneBlockSprite()
        {
            float b = StoneBlockBorder;
            return EnsureExtraSprite(TerrainTexturePath, StoneBlockSpriteName, StoneBlockRect, new Vector4(b, b, b, b));
        }

        /// <summary>
        /// Đảm bảo texture có thêm sprite "IceTile" 16x16 lấy từ lõi đặc của khối băng, dùng để lặp Tiled không để lộ khoảng trống.
        /// </summary>
        private static Sprite EnsureIceTileSprite()
        {
            return EnsureExtraSprite(IceTexturePath, IceTileSpriteName, IceTileRect, Vector4.zero);
        }

        /// <summary>
        /// Thêm 1 sprite vào texture (nếu chưa có), đặt mesh FullRect, giữ nguyên các sprite cũ và GUID của chúng.
        /// </summary>
        private static Sprite EnsureExtraSprite(string texturePath, string spriteName, Rect rect, Vector4 border)
        {
            Sprite existing = LoadFirstSprite(texturePath, spriteName);
            if (existing != null && existing.name == spriteName) return existing;

            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) return null;

            // Sliced/Tiled cần mesh FullRect, nếu không Unity cảnh báo và vẽ sai
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteMeshType != SpriteMeshType.FullRect)
            {
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }

            SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) return null;
            provider.InitSpriteEditorDataProvider();

            List<SpriteRect> rects = new List<SpriteRect>(provider.GetSpriteRects());
            SpriteRect block = new SpriteRect
            {
                name = spriteName,
                rect = rect,
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = border,
                spriteID = GUID.Generate()
            };
            rects.Add(block);
            provider.SetSpriteRects(rects.ToArray());

            // Đồng bộ bảng tên - fileID để tham chiếu tới sprite cũ không bị đổi khi reimport
            ISpriteNameFileIdDataProvider nameProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameProvider != null)
            {
                List<SpriteNameFileIdPair> pairs = new List<SpriteNameFileIdPair>(nameProvider.GetNameFileIdPairs());
                pairs.Add(new SpriteNameFileIdPair(block.name, block.spriteID));
                nameProvider.SetNameFileIdPairs(pairs);
            }

            provider.Apply();
            importer.SaveAndReimport();

            return LoadFirstSprite(texturePath, spriteName);
        }

        private static Sprite LoadFirstSprite(string texturePath, string fallbackName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            foreach (var asset in assets)
            {
                if (asset is Sprite s && s.name == fallbackName) return s;
            }

            foreach (var asset in assets)
            {
                if (asset is Sprite s) return s;
            }

            return null;
        }

        private static Sprite[] LoadIndexedSprites(string texturePath, string baseName, int count)
        {
            Sprite[] result = new Sprite[count];
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            var map = new System.Collections.Generic.Dictionary<string, Sprite>();

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite s) map[s.name] = s;
            }

            for (int i = 0; i < count; i++)
            {
                string target = $"{baseName}_{i}";
                if (map.TryGetValue(target, out Sprite sp)) result[i] = sp;
            }

            return result;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        #endregion
    }
}
#endif
