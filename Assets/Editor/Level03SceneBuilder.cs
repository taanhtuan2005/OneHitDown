// ====================================================================================================
// SCRIPT UNITY EDITOR: XÂY DỰNG CHUẨN XÁC 100% SCENE MÀN 3 'Assets/Scenes/Level_03.unity'
// BẢN CẢI TIẾN THẨM MỸ & KỸ THUẬT (ART & TECH ART UPGRADE):
// "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA" (Catacombs of Ice & Ember) — DÀI 200 UNITS (3200 PX, PPU = 16)
//
// NGUYÊN TẮC CỐT LÕI (TUÂN THỦ 100% GEMINI.MD & SCHEMA_CHI_TIET.MD):
// 1. CHỈ DÙNG ASSET ĐƯỢC PHÂN CHO MÀN 3 TẠI 'Assets/Art/' (PA1_Terrain, PA1_Background/Gray, Sand Mud Ice, Fire, Spikes, Spiked Ball, Fruits, Boxes).
// 2. TUYỆT ĐỐI KHÔNG TẠO SPRITE THỦ CÔNG / PROCEDURAL (ZERO FAKE ASSETS).
// 3. TOÀN BỘ 6 CƠ QUAN MODULE 3 ĐƯỢC LẤY TRỰC TIẾP TỪ 'Assets/Prefabs/Traps/'.
// 4. MOCK SCRIPTS ĐẶT RIÊNG TẠI 'Assets/Scripts/Dev/Mocks/' THEO 'Docs/LEVEL_03_MOCK_NOTICE.md'.
// 5. ĐỒNG BỘ 100% VỚI BỘ 11 ẢNH CẢI TIẾN TRONG 'Anh_Minh_Hoa_Bao_Cao/màn 3/':
//    - Vòm Trần Hầm Mộ liền mạch (Y = 16..20) kết hợp Trụ Thép (Steel Pillars) và Dầm Thép (Steel Beams).
//    - Khối Băng 9-Slice (48x48, border 16px) giữ nguyên viền đen sắc nét và mặt tuyết phản quang.
//    - Khối Đá 9-Slice điểm xuyết các cụm gạch đá lẻ (Stone Inner Bricks 32x32) từ Terrain (16x16).png.
//    - Hốc Vạc Lửa âm tường (Recessed Fire Alcoves), Đường ray mắt xích (Chain.png) cho Bệ Nâng & Bệ Treo.
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
        private const string IceTexturePath = "Assets/Art/03_Bay/PA1_Traps/Sand Mud Ice/Sand Mud Ice (16x6).png";
        private const string BackgroundTexturePath = "Assets/Art/02_MoiTruong/PA1_Background/Gray.png";
        private const string PlayerControllerAssetPath = "Assets/Art/Animations/NinjaFrog/NinjaFrog_Controller.controller";
        private const string PlayerSpritePath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Ninja Frog/Idle (32x32).png";
        private const string SpikesTexturePath = "Assets/Art/03_Bay/PA1_Traps/Spikes/Idle.png";
        private const string FireOffPath = "Assets/Art/03_Bay/PA1_Traps/Fire/Off.png";
        private const string FireHitPath = "Assets/Art/03_Bay/PA1_Traps/Fire/Hit (16x32).png";
        private const string FireOnPath = "Assets/Art/03_Bay/PA1_Traps/Fire/On (16x32).png";
        private const string SpikedBallPath = "Assets/Art/03_Bay/PA1_Traps/Spiked Ball/Spiked Ball.png";
        private const string SpikedChainPath = "Assets/Art/03_Bay/PA1_Traps/Spiked Ball/Chain.png";
        private const string MovingPlatformGreyPath = "Assets/Art/03_Bay/PA1_Traps/Platforms/Grey On (32x8).png";
        private const string MovingPlatformBrownPath = "Assets/Art/03_Bay/PA1_Traps/Platforms/Brown On (32x8).png";
        private const string PlatformChainPath = "Assets/Art/03_Bay/PA1_Traps/Platforms/Chain.png";
        private const string KiwiPath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Fruits/Kiwi.png";
        private const string StrawberryPath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Fruits/Strawberry.png";
        private const string StartSignPath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Checkpoints/Start/Start (Moving) (64x64).png";
        private const string TrophyPath = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Checkpoints/End/End (Idle).png";
        private const string Box1Path = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Boxes/Box1/Idle.png";
        private const string Box3Path = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Boxes/Box3/Idle.png";

        // --- Định nghĩa các vùng Sprite chuẩn từ Terrain (16x16).png (352x176 px, gốc dưới-trái của Unity) ---
        // 1. Khối đá 9-slice (48x48 góc trên-trái: x=0..48, y=128..176, border 16px)
        private const string StoneBlockSpriteName = "Terrain (16x16)_StoneBlock";
        private static readonly Rect StoneBlockRect = new Rect(0f, 128f, 48f, 48f);
        private const float SliceBorder16 = 16f;

        // 2. Cụm gạch đá lẻ trang trí bên trong lòng khối đá (32x32 tại x=48..80, y=144..176)
        private const string StoneInnerBrickSpriteName = "Terrain (16x16)_StoneInnerBricks";
        private static readonly Rect StoneInnerBrickRect = new Rect(48f, 144f, 32f, 32f);

        // 3. Dầm thép ngang 9-slice (48x16 tại x=192..240, y=96..112, border trái/phải 16px)
        private const string SteelBeamHorizSpriteName = "Terrain (16x16)_SteelBeamHoriz";
        private static readonly Rect SteelBeamHorizRect = new Rect(192f, 96f, 48f, 16f);

        // 4. Trụ thép dọc 9-slice (16x48 tại x=240..256, y=64..112, border trên/dưới 16px)
        private const string SteelPillarVertSpriteName = "Terrain (16x16)_SteelPillarVert";
        private static readonly Rect SteelPillarVertRect = new Rect(240f, 64f, 16f, 48f);

        // 5. Khối thép vuông 2x2 (32x32 tại x=208..240, y=64..96)
        private const string SteelBlock2x2SpriteName = "Terrain (16x16)_SteelBlock2x2";
        private static readonly Rect SteelBlock2x2Rect = new Rect(208f, 64f, 32f, 32f);

        // 6. Thanh bệ mỏng One-Way vàng kim (48x16 tại x=272..320, y=160..176) & xám (x=272..320, y=128..144)
        private const string OneWayGoldSpriteName = "Terrain (16x16)_OneWayGold";
        private static readonly Rect OneWayGoldRect = new Rect(272f, 160f, 48f, 16f);
        private const string OneWayGreySpriteName = "Terrain (16x16)_OneWayGrey";
        private static readonly Rect OneWayGreyRect = new Rect(272f, 128f, 48f, 16f);

        // --- Định nghĩa Khối Băng 9-Slice chuẩn từ Sand Mud Ice (16x6).png (176x80 px, gốc dưới-trái Unity) ---
        // Khối băng 48x48 nằm ở x=128..176, y=32..80 (3x3 tiles có viền đen sắc nét & mặt tuyết trắng trên đỉnh)
        private const string IceBlock9SliceSpriteName = "Sand Mud Ice (16x6)_IceBlock9Slice";
        private static readonly Rect IceBlock9SliceRect = new Rect(128f, 32f, 48f, 48f);

        private static Sprite stoneBlockSprite;
        private static Sprite stoneInnerBrickSprite;
        private static Sprite steelBeamHorizSprite;
        private static Sprite steelPillarVertSprite;
        private static Sprite steelBlock2x2Sprite;
        private static Sprite oneWayGoldSprite;
        private static Sprite oneWayGreySprite;
        private static Sprite iceBlock9SliceSprite;

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
        /// Tạo mới hoàn toàn Scene Level_03.unity đồng bộ 100% với bộ 11 ảnh cải tiến Màn 3.
        /// </summary>
        public static void BuildLevel03Scene(bool openScene = true)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Khởi tạo & cắt Sprite 9-Slice chuẩn...", 0.05f);
                EnsureDirectory(SceneDir);

                // 1. Đảm bảo toàn bộ Sprite 9-Slice và Detail Sprites được khai báo trong TextureImporter
                stoneBlockSprite = EnsureExtraSprite(TerrainTexturePath, StoneBlockSpriteName, StoneBlockRect, new Vector4(SliceBorder16, SliceBorder16, SliceBorder16, SliceBorder16));
                stoneInnerBrickSprite = EnsureExtraSprite(TerrainTexturePath, StoneInnerBrickSpriteName, StoneInnerBrickRect, Vector4.zero);
                steelBeamHorizSprite = EnsureExtraSprite(TerrainTexturePath, SteelBeamHorizSpriteName, SteelBeamHorizRect, new Vector4(SliceBorder16, 0f, SliceBorder16, 0f));
                steelPillarVertSprite = EnsureExtraSprite(TerrainTexturePath, SteelPillarVertSpriteName, SteelPillarVertRect, new Vector4(0f, SliceBorder16, 0f, SliceBorder16));
                steelBlock2x2Sprite = EnsureExtraSprite(TerrainTexturePath, SteelBlock2x2SpriteName, SteelBlock2x2Rect, Vector4.zero);
                oneWayGoldSprite = EnsureExtraSprite(TerrainTexturePath, OneWayGoldSpriteName, OneWayGoldRect, new Vector4(SliceBorder16, 0f, SliceBorder16, 0f));
                oneWayGreySprite = EnsureExtraSprite(TerrainTexturePath, OneWayGreySpriteName, OneWayGreyRect, new Vector4(SliceBorder16, 0f, SliceBorder16, 0f));
                iceBlock9SliceSprite = EnsureExtraSprite(IceTexturePath, IceBlock9SliceSpriteName, IceBlock9SliceRect, new Vector4(SliceBorder16, SliceBorder16, SliceBorder16, SliceBorder16));

                if (stoneBlockSprite == null)
                {
                    Debug.LogError("[Level03SceneBuilder] Không tạo được sprite khối đá 9-slice từ Terrain (16x16).png. Hủy dựng Scene.");
                    return;
                }

                Sprite bgSprite = LoadFirstSprite(BackgroundTexturePath, "Gray_0");
                Sprite playerSprite = LoadFirstSprite(PlayerSpritePath, "Idle (32x32)_0");

                // 2. Tạo Scene sạch trong Editor
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // 3. Hierarchy phân nhóm quản lý theo đúng quy chuẩn
                GameObject envGroup = new GameObject("--- 01. ENVIRONMENT & BACKGROUND ---");
                GameObject terrainGroup = new GameObject("--- 02. TERRAIN & CATACOMB ARCHITECTURE ---");
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

                // 7. Địa hình đá 9-Slice, Vòm Trần Hầm Mộ, Trụ Thép và Họa tiết gạch lẻ
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Dựng Vòm Trần Hầm Mộ & Địa hình đá...", 0.50f);
                BuildAllStoneTerrainAndArchitecture(terrainGroup.transform);

                // 8. Các khối Sàn Băng IceRoad.prefab chính thức (9-Slice sắc nét, viền đen + mặt tuyết)
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Bố trí các Khối Băng 9-Slice IceRoad.prefab...", 0.65f);
                BuildAllIceBlocks(module3Group.transform);

                // 9. 5 Cơ quan Module 3 còn lại của Tuyên (Trampoline, FallingPlatform, Fan, DeathPit, Arrow)
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Bố trí 5 Cơ quan Module 3...", 0.75f);
                BuildModule3Gadgets(module3Group.transform);

                // 10. Mock-Safe Người 2 (Vạc Lửa chu kỳ 2s-1s, Chông Gai, Cầu Gai Con Lắc)
                EditorUtility.DisplayProgressBar("Level 03 - Build Scene", "Bố trí Vạc Lửa, Gai, Cầu Gai...", 0.85f);
                BuildModule2Mocks(mockGroup2.transform);

                // 11. Mock-Safe Người 4 (Bệ Nâng X/Y có ray xích, Kiwi [3/3], Dâu Tây, Cúp Đích)
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
                    Debug.Log($"<b>[Level03SceneBuilder]</b> TẠO THÀNH CÔNG SCENE CẢI TIẾN: <color=green>{ScenePath}</color>");
                    if (openScene)
                    {
                        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                    }
                    EditorUtility.DisplayDialog(
                        "One Hit Down - Màn 3 (Bản Nâng Cấp Thẩm Mỹ)",
                        $"Đã đồng bộ thành công 100% thiết kế cải tiến vào Scene:\n{ScenePath}\n\n" +
                        "★ Kiến trúc Vòm Trần Hầm Mộ liền mạch (Y=16..20) + Trụ Thép & Dầm Thép\n" +
                        "★ Khối Băng IceRoad.prefab cắt chuẩn 9-Slice (48x48) có viền đen sắc nét\n" +
                        "★ Khối Đá 9-Slice điểm xuyết cụm gạch lẻ (Inner Bricks) từ Terrain (16x16).png\n" +
                        "★ Hốc Vạc Lửa âm tường + Ray mắt xích cho Bệ Nâng & Bệ Treo One-Way\n" +
                        "★ Đầy đủ 6 Prefabs Module 3 chính thức & Mock-Safe theo đúng quy chuẩn!",
                        "Tuyệt vời!"
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
            camGo.transform.position = new Vector3(4.5f, 6.5f, -10f);
            camGo.tag = "MainCamera";

            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.13f, 0.12f, 0.19f, 1f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;

            camGo.AddComponent<AudioListener>();

            DevCameraFollow follow = camGo.AddComponent<DevCameraFollow>();
            SerializedObject so = new SerializedObject(follow);
            so.FindProperty("offset").vector3Value = new Vector3(0f, 2.2f, -10f);
            so.FindProperty("smoothSpeed").floatValue = 6f;
            so.FindProperty("minBounds").vector2Value = new Vector2(10f, 6.5f);
            so.FindProperty("maxBounds").vector2Value = new Vector2(190f, 13.5f);
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
            sr.size = new Vector2(230f, 28f);
            sr.sortingOrder = -50;
        }

        private static GameObject CreateTestPlayer(Transform parent, Sprite playerSprite)
        {
            GameObject playerGo = new GameObject("Player_NinjaFrog");
            playerGo.transform.SetParent(parent);
            playerGo.transform.position = new Vector3(4.5f, 4.1f, 0f);
            playerGo.tag = "Player";

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0) playerGo.layer = playerLayer;

            SpriteRenderer sr = playerGo.AddComponent<SpriteRenderer>();
            sr.sprite = playerSprite;
            sr.sortingOrder = 10;

            BoxCollider2D col = playerGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.3f);
            col.offset = new Vector2(0f, 0.65f);

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

        #region Terrain & Catacomb Architecture (Đá 9-Slice, Vòm Trần Hầm Mộ, Trụ Thép & Họa Tiết Gạch)

        private static void BuildAllStoneTerrainAndArchitecture(Transform parent)
        {
            GameObject ceilingGroup = new GameObject("01_Catacomb_Ceiling_Archways");
            ceilingGroup.transform.SetParent(parent);

            GameObject pillarsGroup = new GameObject("02_Steel_Pillars_And_Beams");
            pillarsGroup.transform.SetParent(parent);

            GameObject groundGroup = new GameObject("03_Stone_Ground_Platforms");
            groundGroup.transform.SetParent(parent);

            GameObject propsGroup = new GameObject("04_Dungeon_Crates_And_Chains");
            propsGroup.transform.SetParent(parent);

            // =========================================================================
            // A. HỆ THỐNG VÒM TRẦN HẦM MỘ LIỀN MẠCH (Y = 15..20 DỌC TOÀN BỘ 200 UNITS)
            // =========================================================================
            (string name, float x, float yBottom, float w, float h)[] ceilingSegments =
            {
                // Phần 1 (0..40)
                ("C1_EntranceArch",      0f,  17f, 6f,  3f),
                ("C1_HighVault1",        6f,  18f, 9f,  2f),
                ("C1_SpikeCeiling",     15f,  17f, 10f, 3f),
                ("C1_BerryVault",       25f,  18f, 10f, 2f),
                ("C1_DividerArch",      35f,  16f, 5f,  4f),

                // Phần 2 (40..80)
                ("C2_RunupCeiling",     40f,  17f, 8f,  3f),
                ("C2_Kiwi1Vault",       48f,  18f, 7f,  2f),
                ("C2_SpikeCeiling",     55f,  17f, 8f,  3f),
                ("C2_TrampolineVault",  63f,  18f, 11f, 2f),
                ("C2_DividerArch",      74f,  16f, 6f,  4f),

                // Phần 3 (80..120)
                ("C3_LakeEntryArch",    80f,  17f, 6f,  3f),
                ("C3_MovingX_Vault",    86f,  18f, 9f,  2f),
                ("C3_SpikeCeiling",     95f,  17f, 10f, 3f),
                ("C3_Berry2Vault",     105f,  18f, 11f, 2f),
                ("C3_HighWallArch",    116f,  16f, 5f,  4f),

                // Phần 4 (120..160)
                ("C4_Slope1Vault",     121f,  18f, 7f,  2f),
                ("C4_Slope2Vault",     128f,  17f, 7f,  3f),
                ("C4_SpikeCeiling",    135f,  16f, 10f, 4f),
                ("C4_DeathPitVault",   145f,  18f, 10f, 2f),
                ("C4_DividerArch",     155f,  16f, 5f,  4f),

                // Phần 5 (160..200)
                ("C5_PrePitVault",     160f,  18f, 9f,  2f),
                ("C5_FirePitArch",     169f,  17f, 7f,  3f),
                ("C5_PendulumMount",   176f,  17f, 10f, 3f),
                ("C5_AltarVault",      186f,  18f, 8f,  2f),
                ("C5_SanctumBackArch", 194f,  15f, 6f,  5f)
            };

            foreach (var seg in ceilingSegments)
            {
                CreateStoneRect(seg.name, seg.x, seg.yBottom, seg.w, seg.h, ceilingGroup.transform, addInnerBricks: true);
            }

            // =========================================================================
            // B. TRỤ THÉP (STEEL PILLARS), KHỐI THÉP 2x2 & DẦM THÉP TRANG TRÍ
            // =========================================================================
            CreateSteelPillar("P1_EntranceSteelPillar", 0f, 4f, 13f, pillarsGroup.transform, withCollider: true);
            CreateSteelBlock2x2("P1_EntranceSteelCap", 0f, 15f, pillarsGroup.transform);

            CreateSteelBlock2x2("P1_P2_ArchSteelBlock", 36f, 14f, pillarsGroup.transform, withCollider: true);
            CreateSteelBlock2x2("P2_P3_ArchSteelBlock", 76f, 14f, pillarsGroup.transform, withCollider: true);

            // Trụ thép đỡ Cối xay gió ngang ở đầu Phần 4 (X = 119..120, Y = 8..16)
            CreateSteelPillar("P4_WindTunnelPillar", 119f, 8f, 6f, pillarsGroup.transform, withCollider: false);
            CreateSteelBlock2x2("P4_WindTunnelCap", 118f, 14f, pillarsGroup.transform, withCollider: true);

            CreateSteelBlock2x2("P4_P5_ArchSteelBlock", 156f, 14f, pillarsGroup.transform, withCollider: true);

            // Trụ thép hậu cảnh khóa cuối Điện Thờ Cổng Thần (X = 199..200, Y = 4..15)
            CreateSteelPillar("P5_EndSanctumPillar", 199f, 4f, 11f, pillarsGroup.transform, withCollider: true);
            CreateSteelBlock2x2("P5_EndSanctumCap", 198f, 13f, pillarsGroup.transform);

            // =========================================================================
            // C. ĐỊA HÌNH SÀN ĐÁ HẦM MỘ & CÁC HỐC LÒ NUNG (GROUND PLATFORMS)
            // =========================================================================

            // --- PHẦN 1: KHỞI ĐẦU (0..40) ---
            // 1. Thềm xuất phát (X: 0..8, Y: 0..4) + Dầm thép viền mặt trên
            CreateStoneRect("P1_StartLedge", 0f, 0f, 8f, 4f, groundGroup.transform, addInnerBricks: true);
            CreateSteelBeam("P1_StartSteelTrim", 1f, 3f, 6f, pillarsGroup.transform, withCollider: false, sortingOrder: 2);

            // 2. Cụm đá Hốc Vạc Lửa Đôi (X: 14..24): Gờ trái (14..18, Y: 0..4), Đáy hốc (18..20, Y: 0..3), Gờ phải (20..24, Y: 0..4)
            CreateStoneRect("P1_FireAlcove_Left", 14f, 0f, 4f, 4f, groundGroup.transform, addInnerBricks: true);
            CreateStoneRect("P1_FireAlcove_Bed",  18f, 0f, 2f, 3f, groundGroup.transform, addInnerBricks: false);
            CreateStoneRect("P1_FireAlcove_Right",20f, 0f, 4f, 4f, groundGroup.transform, addInnerBricks: true);

            // 3. Đáy Hố Chông #1 (X: 33..37, Y: 0..1)
            CreateStoneRect("P1_SpikePit1_Bottom", 33f, 0f, 4f, 1f, groundGroup.transform, addInnerBricks: false);

            // 4. Bệ mỏng One-Way vàng kim treo dây xích đỡ Dâu Tây #1 (X: 27..32, đỉnh Y = 11.8)
            CreateSuspendedOneWayBar("P1_OneWay_Berry1", 27f, 11.8f, 5f, gold: true, ceilingY: 18f, groundGroup.transform, propsGroup.transform);

            // --- PHẦN 2: KIWI #1 & ĐỆM LÒ XO (40..80) ---
            // 5. Đài Tế Lửa Kiwi #1 (X: 48..56): Gờ trái (48..50, Y: 0..4), Đáy hốc 4 Vạc Lửa (50..54, Y: 0..3), Gờ phải (54..56, Y: 0..4)
            CreateStoneRect("P2_Kiwi1Altar_Left", 48f, 0f, 2f, 4f, groundGroup.transform, addInnerBricks: false);
            CreateStoneRect("P2_Kiwi1Altar_Bed",  50f, 0f, 4f, 3f, groundGroup.transform, addInnerBricks: false);
            CreateStoneRect("P2_Kiwi1Altar_Right",54f, 0f, 2f, 4f, groundGroup.transform, addInnerBricks: false);

            // Thanh bệ treo xích sắt phía trên Kiwi #1 (X: 50..54, đỉnh Y = 10.5)
            CreateSuspendedOneWayBar("P2_OneWay_AboveKiwi1", 50f, 10.5f, 4f, gold: false, ceilingY: 18f, groundGroup.transform, propsGroup.transform);

            // 6. Đáy Hố Chông Sâu #2 (X: 56..60, Y: 0..1)
            CreateStoneRect("P2_SpikePit2_Bottom", 56f, 0f, 4f, 1f, groundGroup.transform, addInnerBricks: false);

            // 7. Thềm đá đặt Đệm lò xo Trampoline (X: 68..73, Y: 0..4)
            CreateStoneRect("P2_TrampolineLedge", 68f, 0f, 5f, 4f, groundGroup.transform, addInnerBricks: true);

            // 8. Vách đá cao nối Phần 2 sang Phần 3 (X: 73..82, Y: 0..6)
            CreateStoneRect("P2_P3_StepUpWall", 73f, 0f, 9f, 6f, groundGroup.transform, addInnerBricks: true);

            // --- PHẦN 3: HỒ LỬA NGẦM & KIWI #2 (80..120) ---
            // 9. Đáy Hồ Lửa Ngầm (X: 82..118, Y: 0..1)
            CreateStoneRect("P3_LavaLakeFloor", 82f, 0f, 36f, 1f, groundGroup.transform, addInnerBricks: false);

            // 10. Trụ đá giữa Hồ Lửa Ngầm (X: 96..101, Y: 1..5, đỡ mũ băng ở Y: 5..6)
            CreateStoneRect("P3_CenterPillarBase", 96f, 1f, 5f, 4f, groundGroup.transform, addInnerBricks: true);

            // 11. Vách núi cao đón người chơi lên đỉnh Thác Băng Phần 4 (X: 118..122, Y: 0..8)
            CreateStoneRect("P3_P4_HighCliffWall", 118f, 0f, 4f, 8f, groundGroup.transform, addInnerBricks: true);

            // --- PHẦN 4: THÁC BĂNG GIẬT CẤP 3 BẬC & HỐ TỬ THẦN (120..160) ---
            // 12. Chân đá đỡ Bậc Băng 1 (X: 122..129, Y: 0..5)
            CreateStoneRect("P4_Step1_StoneBase", 122f, 0f, 7f, 5f, groundGroup.transform, addInnerBricks: true);

            // 13. Chân đá đỡ Bậc Băng 2 (X: 129..136, Y: 0..3)
            CreateStoneRect("P4_Step2_StoneBase", 129f, 0f, 7f, 3f, groundGroup.transform, addInnerBricks: true);

            // 14. Chân đá đỡ Bậc Băng 3 (X: 136..142, Y: 0..1) + Trụ Đá Mỏ Neo Co-op trước Hố Tử Thần (X: 142..145, Y: 0..4)
            CreateStoneRect("P4_Step3_StoneBase", 136f, 0f, 6f, 1f, groundGroup.transform, addInnerBricks: false);
            CreateStoneRect("P4_CoopAnchorLedge", 142f, 0f, 3f, 4f, groundGroup.transform, addInnerBricks: true);

            // 15. Đáy Hố Tử Thần Vĩnh Viễn (X: 145..148, Y: 0..1, rộng đúng 3 tiles = 48px)
            CreateStoneRect("P4_DeathPitFloor", 145f, 0f, 3f, 1f, groundGroup.transform, addInnerBricks: false);

            // 16. Trụ đá ngăn cách cuối Phần 4 (X: 156..158, Y: 0..4)
            CreateStoneRect("P4_DividerLedge", 156f, 0f, 2f, 4f, groundGroup.transform, addInnerBricks: false);

            // --- PHẦN 5: VỀ ĐÍCH - HÀNH LANG CON LẮC & ĐIỆN THỜ CỔNG THẦN (160..200) ---
            // 17. Đáy Vực Lửa Cuối (X: 168..175, Y: 0..1)
            CreateStoneRect("P5_FinalFirePitFloor", 168f, 0f, 7f, 1f, groundGroup.transform, addInnerBricks: false);

            // 18. Thềm Điện Thờ Đích (X: 187..200, Y: 0..4) + Bục Thép Tế Lễ (X: 190..198, Y: 4..5)
            CreateStoneRect("P5_VictoryAltarPlatform", 187f, 0f, 13f, 4f, groundGroup.transform, addInnerBricks: true);
            CreateSteelBeam("P5_AltarRaisedDais", 190f, 4f, 8f, pillarsGroup.transform, withCollider: true, sortingOrder: 2);

            // 19. Thùng gỗ trang trí (Boxes) tạo điểm nhấn thị giác
            Sprite box1Sprite = LoadFirstSprite(Box1Path, "Idle_0");
            Sprite box3Sprite = LoadFirstSprite(Box3Path, "Idle_0");
            if (box1Sprite != null)
            {
                CreateDecoProp("P1_DecoCrate_Box1", new Vector3(15.4f, 4.75f, 0f), box1Sprite, propsGroup.transform, sortingOrder: 2);
            }
            if (box3Sprite != null)
            {
                CreateDecoProp("P2_DecoCrate_Box3", new Vector3(75.1f, 6.75f, 0f), box3Sprite, propsGroup.transform, sortingOrder: 2);
            }
        }

        private static GameObject CreateStoneRect(string name, float xLeft, float yBottom, float width, float height, Transform parent, bool addInnerBricks)
        {
            Vector2 center = new Vector2(xLeft + width * 0.5f, yBottom + height * 0.5f);
            Vector2 size = new Vector2(width, height);

            GameObject boxGo = new GameObject(name);
            boxGo.transform.SetParent(parent);
            boxGo.transform.position = new Vector3(center.x, center.y, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) boxGo.layer = groundLayer;

            SpriteRenderer sr = boxGo.AddComponent<SpriteRenderer>();
            sr.sprite = stoneBlockSprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = size;
            sr.sortingOrder = 1;

            BoxCollider2D col = boxGo.AddComponent<BoxCollider2D>();
            col.size = size;
            col.offset = Vector2.zero;

            // Điểm xuyết các cụm gạch đá lẻ (32x32 = 2x2 units) bên trong lòng khối đá lớn
            if (addInnerBricks && stoneInnerBrickSprite != null && width >= 3f && height >= 3f)
            {
                int wInt = Mathf.RoundToInt(width);
                int hInt = Mathf.RoundToInt(height);
                for (int r = 1; r < hInt - 1; r += 2)
                {
                    for (int c = 1; c < wInt - 1; c += 3)
                    {
                        int offsetC = c + ((r / 2) % 2);
                        if (offsetC + 2 <= wInt - 1 && r + 2 <= hInt - 1)
                        {
                            GameObject brickChild = new GameObject($"InnerBrick_{r}_{offsetC}");
                            brickChild.transform.SetParent(boxGo.transform, false);
                            float localX = -width * 0.5f + offsetC + 1.0f;
                            float localY = height * 0.5f - r - 1.0f;
                            brickChild.transform.localPosition = new Vector3(localX, localY, 0f);

                            SpriteRenderer bsr = brickChild.AddComponent<SpriteRenderer>();
                            bsr.sprite = stoneInnerBrickSprite;
                            bsr.sortingOrder = 2;
                        }
                    }
                }
            }

            return boxGo;
        }

        private static void CreateSteelPillar(string name, float xLeft, float yBottom, float height, Transform parent, bool withCollider)
        {
            if (steelPillarVertSprite == null) return;
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(xLeft + 0.5f, yBottom + height * 0.5f, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) go.layer = groundLayer;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = steelPillarVertSprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = new Vector2(1f, height);
            sr.sortingOrder = 0;

            if (withCollider)
            {
                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, height);
            }
        }

        private static void CreateSteelBeam(string name, float xLeft, float yBottom, float width, Transform parent, bool withCollider, int sortingOrder = 2)
        {
            if (steelBeamHorizSprite == null) return;
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(xLeft + width * 0.5f, yBottom + 0.5f, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) go.layer = groundLayer;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = steelBeamHorizSprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = new Vector2(width, 1f);
            sr.sortingOrder = sortingOrder;

            if (withCollider)
            {
                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(width, 1f);
            }
        }

        private static void CreateSteelBlock2x2(string name, float xLeft, float yBottom, Transform parent, bool withCollider = false)
        {
            if (steelBlock2x2Sprite == null) return;
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(xLeft + 1.0f, yBottom + 1.0f, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) go.layer = groundLayer;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = steelBlock2x2Sprite;
            sr.sortingOrder = 2;

            if (withCollider)
            {
                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(2f, 2f);
            }
        }

        private static void CreateSuspendedOneWayBar(string name, float xLeft, float yTop, float width, bool gold, float ceilingY, Transform platformParent, Transform chainParent)
        {
            Sprite barSprite = gold ? oneWayGoldSprite : oneWayGreySprite;
            if (barSprite == null) return;

            GameObject platGo = new GameObject(name);
            platGo.transform.SetParent(platformParent);
            platGo.transform.position = new Vector3(xLeft + width * 0.5f, yTop - 0.25f, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) platGo.layer = groundLayer;

            SpriteRenderer sr = platGo.AddComponent<SpriteRenderer>();
            sr.sprite = barSprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = new Vector2(width, 0.5f);
            sr.sortingOrder = 2;

            BoxCollider2D col = platGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, 0.5f);
            col.usedByEffector = true;

            PlatformEffector2D effector = platGo.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 180f;

            // Tạo 2 sợi dây xích treo từ trần xuống thanh bệ
            Sprite chainSprite = LoadFirstSprite(PlatformChainPath, "Chain_0");
            if (chainSprite != null)
            {
                GameObject chainGroup = new GameObject($"{name}_Chains");
                chainGroup.transform.SetParent(chainParent);
                float[] chainXs = { xLeft + 0.5f, xLeft + width - 0.5f };
                foreach (float cx in chainXs)
                {
                    for (float cy = yTop + 0.25f; cy < ceilingY; cy += 0.5f)
                    {
                        GameObject link = new GameObject("ChainLink");
                        link.transform.SetParent(chainGroup.transform);
                        link.transform.position = new Vector3(cx, cy, 0f);
                        SpriteRenderer lsr = link.AddComponent<SpriteRenderer>();
                        lsr.sprite = chainSprite;
                        lsr.sortingOrder = 0;
                    }
                }
            }
        }

        private static void CreateDecoProp(string name, Vector3 pos, Sprite sprite, Transform parent, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
        }

        #endregion

        #region Ice Blocks (Sử dụng trực tiếp IceRoad.prefab chính thức với Sprite 9-Slice 48x48)

        private static void BuildAllIceBlocks(Transform parent)
        {
            // Phần 1: Khởi đầu
            InstantiateIceRoadRect(parent, "P1_IceRoad_Intro",        8f,  0f, 6f,  4f);
            InstantiateIceRoadRect(parent, "P1_IceRoad_ToSpikePit",  24f,  0f, 9f,  4f);

            // Phần 2: Đà trượt băng nối Phần 1 -> Phần 2 & Sàn băng đáp sau hố gai
            InstantiateIceRoadRect(parent, "P2_IceRoad_Runup",       37f,  0f, 11f, 4f);
            InstantiateIceRoadRect(parent, "P2_IceRoad_Landing",     60f,  0f, 8f,  4f);

            // Phần 3: Mũ băng trên đỉnh cột trụ giữa Hồ Lửa Ngầm
            InstantiateIceRoadRect(parent, "P3_IceRoad_CenterPillar",96f,  5f, 5f,  1f);

            // Phần 4: Thác băng giật cấp 3 bậc & Sàn băng sau Hố Tử Thần
            InstantiateIceRoadRect(parent, "P4_IceRoad_Slope1",     122f,  5f, 7f,  3f);
            InstantiateIceRoadRect(parent, "P4_IceRoad_Slope2",     129f,  3f, 7f,  3f);
            InstantiateIceRoadRect(parent, "P4_IceRoad_Slope3",     136f,  1f, 6f,  3f);
            InstantiateIceRoadRect(parent, "P4_IceRoad_AfterPit",   148f,  0f, 8f,  4f);

            // Phần 5: Thềm băng trước vực lửa cuối & Hành lang băng dưới Cầu Gai Con Lắc
            InstantiateIceRoadRect(parent, "P5_IceRoad_PrePit",     158f,  0f, 10f, 4f);
            InstantiateIceRoadRect(parent, "P5_IceRoad_Pendulum",   175f,  0f, 12f, 4f);
        }

        private static void InstantiateIceRoadRect(Transform parent, string name, float xLeft, float yBottom, float width, float height)
        {
            GameObject prefab = LoadPrefab("IceRoad");
            if (prefab == null) return;

            Vector2 center = new Vector2(xLeft + width * 0.5f, yBottom + height * 0.5f);
            Vector2 size = new Vector2(width, height);

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            inst.name = name;
            inst.transform.position = new Vector3(center.x, center.y, 0f);

            SpriteRenderer sr = inst.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (iceBlock9SliceSprite != null) sr.sprite = iceBlock9SliceSprite;
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.tileMode = SpriteTileMode.Continuous;
                sr.size = size;
                sr.sortingOrder = 3;
            }

            inst.transform.localScale = Vector3.one;

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
            // 1. ĐỆM LÒ XO TRAMPOLINE (Phần 2: X = 70.5, Y = 4.25, bật lên vách Y = 6)
            GameObject trampGo = InstantiatePrefab("Trampoline", parent);
            if (trampGo != null)
            {
                trampGo.name = "P2_Trampoline_To_P3";
                trampGo.transform.position = new Vector3(70.5f, 4.25f, 0f);
            }

            // 2. BỆ ĐÁ SỤP FALLING PLATFORM (Phần 1: 2 bệ lên Dâu Tây #1; Phần 3: 1 bệ dưới Dâu Tây #2)
            GameObject fp1 = InstantiatePrefab("FallingPlatform", parent);
            if (fp1 != null)
            {
                fp1.name = "P1_FallingPlatform_1";
                fp1.transform.position = new Vector3(18.0f, 7.8f, 0f);
            }

            GameObject fp2 = InstantiatePrefab("FallingPlatform", parent);
            if (fp2 != null)
            {
                fp2.name = "P1_FallingPlatform_2";
                fp2.transform.position = new Vector3(23.0f, 10.0f, 0f);
            }

            GameObject fp3 = InstantiatePrefab("FallingPlatform", parent);
            if (fp3 != null)
            {
                fp3.name = "P3_FallingPlatform_Berry2";
                fp3.transform.position = new Vector3(114.0f, 10.0f, 0f);
            }

            // 3. CỐI XAY GIÓ NGANG FANTRAP (Phần 4: cặp quạt gắn trên trụ thép X = 120.3, Y = 9.5 & 11.0 thổi sang phải)
            CreateHorizontalFan(parent, "P4_FanTrap_Horizontal_Lower", new Vector3(120.3f, 9.5f, 0f));
            CreateHorizontalFan(parent, "P4_FanTrap_Horizontal_Upper", new Vector3(120.3f, 11.0f, 0f));

            // 4. HỐ TỬ THẦN VĨNH VIỄN DEATHPIT (Phần 4: X = 145..148, Y = 3.35, rộng đúng 3 tiles = 48px)
            GameObject dp1 = InstantiatePrefab("DeathPit", parent);
            if (dp1 != null)
            {
                dp1.name = "P4_DeathPit_Block1";
                dp1.transform.position = new Vector3(145.75f, 3.35f, 0f);
            }

            GameObject dp2 = InstantiatePrefab("DeathPit", parent);
            if (dp2 != null)
            {
                dp2.name = "P4_DeathPit_Block2";
                dp2.transform.position = new Vector3(147.25f, 3.35f, 0f);
            }

            // 5. MŨI TÊN ĐẨY KHÍ ARROWBOOSTER:
            // Phần 3: Treo lơ lửng tại X = 109.5, Y = 7.5 phóng lên Dâu Tây #2
            GameObject ab1 = InstantiatePrefab("ArrowBooster", parent);
            if (ab1 != null)
            {
                ab1.name = "P3_ArrowBooster_To_Berry2";
                ab1.transform.position = new Vector3(109.5f, 7.5f, 0f);
                SetupArrowBooster(ab1, new Vector2(1f, 0.85f).normalized, 14f);
            }

            // Phần 5: Phóng vượt vực lửa cuối (X = 171.5, Y = 6.2)
            GameObject ab2 = InstantiatePrefab("ArrowBooster", parent);
            if (ab2 != null)
            {
                ab2.name = "P5_ArrowBooster_FinalPit";
                ab2.transform.position = new Vector3(171.5f, 6.2f, 0f);
                SetupArrowBooster(ab2, new Vector2(1f, 0.7f).normalized, 14f);
            }
        }

        private static void CreateHorizontalFan(Transform parent, string name, Vector3 pos)
        {
            GameObject fanGo = InstantiatePrefab("FanTrap", parent);
            if (fanGo == null) return;

            fanGo.name = name;
            fanGo.transform.position = pos;
            fanGo.transform.rotation = Quaternion.Euler(0f, 0f, -90f); // Xoay mặt quạt hướng sang phải

            FanTrap ft = fanGo.GetComponent<FanTrap>();
            if (ft != null)
            {
                SerializedObject so = new SerializedObject(ft);
                so.FindProperty("windDirection").vector2Value = Vector2.right;
                so.FindProperty("windForce").floatValue = 15f;
                so.FindProperty("isActive").boolValue = true;
                so.ApplyModifiedProperties();

                Transform wz = fanGo.transform.Find("WindZone");
                if (wz != null)
                {
                    BoxCollider2D wzCol = wz.GetComponent<BoxCollider2D>();
                    if (wzCol != null)
                    {
                        wzCol.size = new Vector2(2.0f, 12.0f);
                        wzCol.offset = new Vector2(0f, 6.25f);
                    }
                }
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
            // Cắt chuẩn từng khung 16x32 (1x2 units, tâm chính giữa) cho Vạc Lửa để không dùng nhầm sprite bị auto-slice gộp 3 đế (48x17)
            Sprite fireOff = EnsureExtraSprite(FireOffPath, "Fire_Off_Full16x32", new Rect(0f, 0f, 16f, 32f), Vector4.zero);
            Sprite fireHit = EnsureExtraSprite(FireHitPath, "Fire_Hit_Full16x32_0", new Rect(0f, 0f, 16f, 32f), Vector4.zero);
            Sprite[] fireOnFrames = new Sprite[3]
            {
                EnsureExtraSprite(FireOnPath, "Fire_On_Full16x32_0", new Rect(0f,  0f, 16f, 32f), Vector4.zero),
                EnsureExtraSprite(FireOnPath, "Fire_On_Full16x32_1", new Rect(16f, 0f, 16f, 32f), Vector4.zero),
                EnsureExtraSprite(FireOnPath, "Fire_On_Full16x32_2", new Rect(32f, 0f, 16f, 32f), Vector4.zero)
            };
            Sprite spikesSprite = LoadFirstSprite(SpikesTexturePath, "Idle_0");

            // 1. CHÔNG GAI TRẦN (Gắn sát đáy các vòm trần đá):
            CreateSpikeRowFromLeft("P1_CeilingSpikes",      16f, 16.5f, 8, parent, spikesSprite, pointingDown: true);
            CreateSpikeRowFromLeft("P1_P2_ArchSpikes",      36f, 13.5f, 2, parent, spikesSprite, pointingDown: true);
            CreateSpikeRowFromLeft("P2_CeilingSpikes",      56f, 16.5f, 6, parent, spikesSprite, pointingDown: true);
            CreateSpikeRowFromLeft("P3_CeilingSpikes",      96f, 16.5f, 8, parent, spikesSprite, pointingDown: true);
            CreateSpikeRowFromLeft("P4_CeilingSpikes",     136f, 15.5f, 8, parent, spikesSprite, pointingDown: true);
            CreateSpikeRowFromLeft("P5_CeilingSpikes",     177f, 16.5f, 8, parent, spikesSprite, pointingDown: true);

            // 2. CHÔNG GAI ĐÁY HỐ SÀN:
            CreateSpikeRowFromLeft("P1_FloorSpikes_Pit1",   33f,  1.5f, 4, parent, spikesSprite, pointingDown: false);
            CreateSpikeRowFromLeft("P2_FloorSpikes_Pit2",   56f,  1.5f, 4, parent, spikesSprite, pointingDown: false);

            // 3. VẠC LỬA CHU KỲ (2s tắt - 0.35s báo - 1s bật, đặt âm trong hốc đá sao cho mặt vạc khớp mặt sàn):
            // Phần 1: Hốc 2 Vạc Lửa (X = 18.5 & 19.5, tâm Y = 4.0 -> đế vạc nằm ở Y = 3..4, ngọn lửa ở Y = 4..5)
            CreateMockFireTrap(parent, "P1_FireTrap_0", new Vector3(18.5f, 4.0f, 0f), fireOff, fireHit, fireOnFrames, previewHitState: false, frameIdx: 0);
            CreateMockFireTrap(parent, "P1_FireTrap_1", new Vector3(19.5f, 4.0f, 0f), fireOff, fireHit, fireOnFrames, previewHitState: true,  frameIdx: 1);

            // Phần 2: Đài Tế Lửa 4 Vạc dưới Kiwi #1 (X = 50.5, 51.5, 52.5, 53.5 tại Y = 4.0)
            for (int i = 0; i < 4; i++)
            {
                bool isHitPreview = (i == 1 || i == 2);
                CreateMockFireTrap(parent, $"P2_FireTrap_Kiwi1_{i}", new Vector3(50.5f + i * 1.0f, 4.0f, 0f), fireOff, fireHit, fireOnFrames, previewHitState: isHitPreview, frameIdx: i);
            }

            // Phần 3: Hồ Lửa Ngầm Liên Hoàn ở 2 lòng hồ (Trái: X = 82..96, Phải: X = 101..118, tâm Y = 2.0 trên đáy Y = 1.0)
            for (int fx = 82; fx < 96; fx++)
            {
                bool isHitPreview = (fx % 2 != 0);
                CreateMockFireTrap(parent, $"P3_LavaFire_Left_{fx}", new Vector3(fx + 0.5f, 2.0f, 0f), fireOff, fireHit, fireOnFrames, previewHitState: isHitPreview, frameIdx: fx);
            }
            for (int fx = 101; fx < 118; fx++)
            {
                bool isHitPreview = (fx % 2 == 0);
                CreateMockFireTrap(parent, $"P3_LavaFire_Right_{fx}", new Vector3(fx + 0.5f, 2.0f, 0f), fireOff, fireHit, fireOnFrames, previewHitState: isHitPreview, frameIdx: fx);
            }

            // Phần 4: 3 Vạc lửa đáy Hố Tử Thần Vĩnh Viễn (X = 145.5, 146.5, 147.5 tại Y = 2.0)
            for (int i = 0; i < 3; i++)
            {
                CreateMockFireTrap(parent, $"P4_DeathPitFire_{i}", new Vector3(145.5f + i * 1.0f, 2.0f, 0f), fireOff, fireHit, fireOnFrames, previewHitState: false, frameIdx: i);
            }

            // Phần 5: 7 Vạc lửa Vực Cuối (X = 168.5..174.5 tại Y = 2.0)
            for (int i = 0; i < 7; i++)
            {
                bool isHitPreview = (i % 2 != 0);
                CreateMockFireTrap(parent, $"P5_FinalPitFire_{i}", new Vector3(168.5f + i * 1.0f, 2.0f, 0f), fireOff, fireHit, fireOnFrames, previewHitState: isHitPreview, frameIdx: i);
            }

            // 4. CẦU GAI CON LẮC XÍCH SẮT (Phần 5: treo từ vòm trần X = 181.0, Y = 16.8 xuống Y = 7.0)
            BuildMockPendulum(parent, new Vector3(181.0f, 16.8f, 0f), length: 9.8f);
        }

        private static void CreateSpikeRowFromLeft(string name, float xLeft, float yCenter, int count, Transform parent, Sprite sprite, bool pointingDown)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent);
            row.transform.position = new Vector3(xLeft + count * 0.5f, yCenter, 0f);

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
            col.size = new Vector2(count * 1.0f, 0.55f);
            col.offset = new Vector2(0f, pointingDown ? 0.15f : -0.15f);
        }

        private static void CreateMockFireTrap(Transform parent, string name, Vector3 pos, Sprite off, Sprite hit, Sprite[] onFrames, bool previewHitState = false, int frameIdx = 0)
        {
            GameObject fireGo = new GameObject(name);
            fireGo.transform.SetParent(parent);
            fireGo.transform.position = pos;

            int trapsLayer = LayerMask.NameToLayer("Traps");
            if (trapsLayer >= 0) fireGo.layer = trapsLayer;

            SpriteRenderer sr = fireGo.AddComponent<SpriteRenderer>();
            Sprite initialOn = (onFrames != null && onFrames.Length > 0) ? onFrames[Mathf.Abs(frameIdx) % onFrames.Length] : off;
            sr.sprite = previewHitState && hit != null ? hit : (initialOn != null ? initialOn : off);
            sr.sortingOrder = 6;

            // Đế vạc lửa (nửa dưới 16x16) có collider Ground để người chơi đứng lên được khi lửa tắt
            GameObject baseBlock = new GameObject("FireBaseSolid");
            baseBlock.transform.SetParent(fireGo.transform, false);
            baseBlock.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) baseBlock.layer = groundLayer;
            BoxCollider2D baseCol = baseBlock.AddComponent<BoxCollider2D>();
            baseCol.size = new Vector2(1.0f, 1.0f);

            // Ngọn lửa (nửa trên 16x16) là Trigger Hazard
            GameObject hitbox = new GameObject("FireHitbox");
            hitbox.transform.SetParent(fireGo.transform, false);
            hitbox.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            hitbox.tag = "Hazard";
            if (trapsLayer >= 0) hitbox.layer = trapsLayer;

            BoxCollider2D hitCol = hitbox.AddComponent<BoxCollider2D>();
            hitCol.isTrigger = true;
            hitCol.size = new Vector2(0.8f, 0.9f);

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

            int chainCount = Mathf.RoundToInt(length / 0.65f);
            for (int i = 0; i < chainCount; i++)
            {
                GameObject link = new GameObject($"ChainLink_{i}");
                link.transform.SetParent(pendulumGo.transform, false);
                link.transform.localPosition = new Vector3(0f, -i * 0.65f, 0f);

                SpriteRenderer lsr = link.AddComponent<SpriteRenderer>();
                lsr.sprite = chainSprite;
                lsr.sortingOrder = 7;
            }

            GameObject ballGo = new GameObject("Ball_Hazard");
            ballGo.transform.SetParent(pendulumGo.transform, false);
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
            so.FindProperty("swingAngle").floatValue = 48f;
            so.FindProperty("swingSpeed").floatValue = 2.0f;
            so.FindProperty("armLength").floatValue = length;
            so.FindProperty("ballTransform").objectReferenceValue = ballGo.transform;
            so.ApplyModifiedProperties();
        }

        #endregion

        #region Module 4 Mocks (Bệ Nâng kèm Ray Xích, Kiwi [3/3], Dâu Tây, Đích)

        private static void BuildModule4Mocks(Transform parent)
        {
            Sprite kiwiSprite = LoadFirstSprite(KiwiPath, "Kiwi_0");
            Sprite strawberrySprite = LoadFirstSprite(StrawberryPath, "Strawberry_0");
            // Cắt chuẩn 1 frame 32x8 (2.0 x 0.5 units) cho Bệ Nâng thay vì dùng sprite mặc định bị gộp cả 8 frames (256x8)
            Sprite greyPlatSprite = EnsureExtraSprite(MovingPlatformGreyPath, "GreyOn_Frame0_32x8", new Rect(0f, 0f, 32f, 8f), Vector4.zero);
            Sprite brownPlatSprite = EnsureExtraSprite(MovingPlatformBrownPath, "BrownOn_Frame0_32x8", new Rect(0f, 0f, 32f, 8f), Vector4.zero);
            Sprite chainSprite = LoadFirstSprite(PlatformChainPath, "Chain_0");
            Sprite trophySprite = LoadFirstSprite(TrophyPath, "End (Idle)_0");
            Sprite startSignSprite = LoadFirstSprite(StartSignPath, "Start (Moving) (64x64)_0");

            // 1. BỆ XUẤT PHÁT LEVEL START Ở ĐẦU MÀN (X = 2.2, mặt sàn Y = 4.0 -> tâm Y = 6.0 cho sprite 64x64)
            if (startSignSprite != null)
            {
                GameObject startGo = new GameObject("P1_LevelStart_Sign");
                startGo.transform.SetParent(parent);
                startGo.transform.position = new Vector3(2.2f, 6.0f, 0f);
                SpriteRenderer sr = startGo.AddComponent<SpriteRenderer>();
                sr.sprite = startSignSprite;
                sr.sortingOrder = 4;
            }

            // 2. 3 TRÁI KIWI CỔ VẬT BẮT BUỘC [3/3] ĐỂ MỞ CỔNG THẦN:
            CreateCollectible(parent, "Kiwi_Relic_1", new Vector3(52.0f, 6.6f, 0f), kiwiSprite, isRelic: true, "Kiwi Lửa Thiêu [1/3]");
            CreateCollectible(parent, "Kiwi_Relic_2", new Vector3(104.5f, 11.8f, 0f), kiwiSprite, isRelic: true, "Kiwi Bệ Nâng [2/3]");
            CreateCollectible(parent, "Kiwi_Relic_3", new Vector3(148.2f, 5.3f, 0f), kiwiSprite, isRelic: true, "Kiwi Đáy Dốc Băng [3/3]");

            // 3. 2 QUẢ THƯỞNG PHỤ: DÂU TÂY
            CreateCollectible(parent, "Berry_Bonus_1", new Vector3(29.5f, 13.0f, 0f), strawberrySprite, isRelic: false, "Dâu Tây Thưởng #1");
            CreateCollectible(parent, "Berry_Bonus_2", new Vector3(114.0f, 11.8f, 0f), strawberrySprite, isRelic: false, "Dâu Tây Thưởng #2");

            // 4. BỆ NÂNG DI ĐỘNG (MovingPlatform) KÈM ĐƯỜNG RAY MẮT XÍCH (Chain.png) KHỚP 100% ẢNH DEMO:
            // Bệ Nâng Ngang X ở Phần 3: Ray xích từ X = 84.75 -> 94.5 tại Y = 5.5; Bệ đặt tại chính giữa (89.5, 5.5), ghép 2 đốt 32x8 nguyên vẹn (rộng 4.0 units)
            CreateMovingPlatformWithChainTrack(
                parent,
                "P3_MovingPlatform_X",
                initialPos: new Vector3(89.5f, 5.5f, 0f),
                trackStart: new Vector3(84.75f, 5.5f, 0f),
                trackEnd: new Vector3(94.5f, 5.5f, 0f),
                startOffset: new Vector2(-3.0f, 0f),
                moveOffset: new Vector2(6.0f, 0f),
                speed: 3.5f,
                segmentHalfSpan: 1.0f,
                colliderWidth: 4.0f,
                platSprite: brownPlatSprite,
                chainSprite: chainSprite);

            // Bệ Nâng Dọc Y ở Phần 3: Ray xích dọc tại X = 104.5 từ Y = 4.25 (dưới bệ) lên tới Y = 11.0 (dưới Kiwi #2); Bệ đặt tại (104.5, 6.8) đúng như ảnh demo
            CreateMovingPlatformWithChainTrack(
                parent,
                "P3_MovingPlatform_Y",
                initialPos: new Vector3(104.5f, 6.8f, 0f),
                trackStart: new Vector3(104.5f, 4.25f, 0f),
                trackEnd: new Vector3(104.5f, 11.0f, 0f),
                startOffset: new Vector2(0f, -2.55f),
                moveOffset: new Vector2(0f, 6.75f),
                speed: 3.0f,
                segmentHalfSpan: 0.7f,
                colliderWidth: 3.4f,
                platSprite: greyPlatSprite,
                chainSprite: chainSprite);

            // 5. ĐIỆN THỜ CỔNG THẦN CUỐI MÀN (Đặt trên Bục Thép Tế Lễ Y = 5.0 -> tâm Y = 7.0 cho sprite 64x64)
            CreateVictoryAltar(parent, new Vector3(194.0f, 7.0f, 0f), trophySprite);
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

        private static void CreateMovingPlatformWithChainTrack(
            Transform parent,
            string name,
            Vector3 initialPos,
            Vector3 trackStart,
            Vector3 trackEnd,
            Vector2 startOffset,
            Vector2 moveOffset,
            float speed,
            float segmentHalfSpan,
            float colliderWidth,
            Sprite platSprite,
            Sprite chainSprite)
        {
            // Vẽ đường ray mắt xích cố định nối giữa điểm đầu và điểm cuối với bước mắt xích chuẩn 0.75 unit (12px)
            if (chainSprite != null)
            {
                GameObject trackGo = new GameObject($"{name}_ChainTrack");
                trackGo.transform.SetParent(parent);
                float dist = Vector3.Distance(trackStart, trackEnd);
                int steps = Mathf.Max(2, Mathf.RoundToInt(dist / 0.75f));
                for (int i = 0; i <= steps; i++)
                {
                    float t = (float)i / steps;
                    Vector3 p = Vector3.Lerp(trackStart, trackEnd, t);
                    GameObject link = new GameObject($"TrackLink_{i}");
                    link.transform.SetParent(trackGo.transform);
                    link.transform.position = p;
                    SpriteRenderer lsr = link.AddComponent<SpriteRenderer>();
                    lsr.sprite = chainSprite;
                    lsr.sortingOrder = 1;
                }
            }

            GameObject platGo = new GameObject(name);
            platGo.transform.SetParent(parent);
            platGo.transform.position = initialPos;

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) platGo.layer = groundLayer;

            // Ghép 2 đốt bệ nâng 32x8 nguyên vẹn (không bị cắt cụt đầu bên phải như khi Tiled 3.0 unit trên sheet 256x8)
            float[] segOffsets = { -segmentHalfSpan, segmentHalfSpan };
            for (int i = 0; i < segOffsets.Length; i++)
            {
                GameObject segGo = new GameObject($"PlatformSegment_{i}");
                segGo.transform.SetParent(platGo.transform, false);
                segGo.transform.localPosition = new Vector3(segOffsets[i], 0f, 0f);
                SpriteRenderer sr = segGo.AddComponent<SpriteRenderer>();
                sr.sprite = platSprite;
                sr.sortingOrder = 4 + i;
            }

            BoxCollider2D col = platGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(colliderWidth, 0.5f);

            MockMovingPlatform mmp = platGo.AddComponent<MockMovingPlatform>();
            SerializedObject so = new SerializedObject(mmp);
            so.FindProperty("startOffset").vector2Value = startOffset;
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
            col.size = new Vector2(2.0f, 2.5f);

            altar.AddComponent<MockGoalTrigger>();
        }

        #endregion

        #region HUD Labels & Signs

        private static void CreateHudLabels(Transform parent)
        {
            CreateLabel("HUD_Header1", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 1/5: KHỞI ĐẦU (0-40)]", new Vector3(20f, 15.2f, 0f), parent);
            CreateLabel("HUD_Header2", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 2/5: KIWI 1 (40-80)]", new Vector3(60f, 15.2f, 0f), parent);
            CreateLabel("HUD_Header3", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 3/5: KIWI 2 (80-120)]", new Vector3(100f, 15.2f, 0f), parent);
            CreateLabel("HUD_Header4", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 4/5: KIWI 3 (120-160)]", new Vector3(140f, 14.5f, 0f), parent);
            CreateLabel("HUD_Header5", "MÀN 3: HẦM MỘ BĂNG LÃNH & VẠC LỬA [HÌNH 5/5: VỀ ĐÍCH (160-200)]", new Vector3(180f, 15.2f, 0f), parent);

            CreateLabel("HUD_GoalSign", "[OPEN 3/3]", new Vector3(194.0f, 9.5f, 0f), parent);
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
        /// Thêm 1 sprite vào texture (nếu chưa có) hoặc cập nhật rect/border, đặt mesh FullRect, giữ nguyên các sprite cũ và GUID của chúng.
        /// </summary>
        private static Sprite EnsureExtraSprite(string texturePath, string spriteName, Rect rect, Vector4 border)
        {
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) return null;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteMeshType != SpriteMeshType.FullRect)
            {
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }

            Sprite existing = LoadFirstSprite(texturePath, spriteName);
            if (existing != null && existing.name == spriteName) return existing;

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
            var map = new Dictionary<string, Sprite>();

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
