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
    /// Script Editor tự động hóa việc dựng hoàn chỉnh Scene 'Assets/Scenes/Level_01.unity'
    /// Đạt chuẩn 100% mỹ thuật, gameplay và vật lý:
    /// 1. TẢNG ĐẤT NÂU BÊN DƯỚI TỰ ĐỘNG BIẾN MẤT KHI SẬP BẪY:
    ///    - Mỗi hố sập gồm nắp cỏ xanh bên trên và tảng đất nâu DirtBody bên dưới.
    ///    - Khi chưa kích hoạt: Đất và cỏ lấp đầy hố, núi phẳng lì đặc 100%.
    ///    - Khi giẫm vào: Cả nắp cỏ VÀ tảng đất nâu bên dưới CÙNG BIẾN MẤT, mở toang vực thẳm!
    /// 2. HẠ ĐỘ CAO CHUẨN TỈ LỆ - XÓA CẢM GIÁC NHẢY QUÁ CAO:
    ///    - Tầng 1 ở y = 0, đỉnh tường đá chắn ở y = 6, Cao nguyên Tầng 2 ở y = 8.
    ///    - Lò xo nhún với lực 16f đẩy nhân vật bay bổng 6.5 tiles theo quỹ đạo parabol vừa tầm mắt,
    ///      tiếp đất êm ái trên đỉnh tường đá mà không bị văng giật camera hay cụng đầu.
    /// 3. LƯỠI CƯA TUẦN TRA THEO NHỊP ĐỘ DỌC (VERTICAL PATROL):
    ///    - Cưa di chuyển lên xuống nhịp nhàng, khi cưa nâng lên mở lối đi an toàn bên dưới
    ///      để người chơi nhảy qua bệ rơi, nhặt táo và tiếp đất sang Cao nguyên.
    /// </summary>
    public static class Level01SceneBuilder
    {
        private const string SceneDir = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/Level_01.unity";

        // Asset Paths
        private const string BgGreenPath = "Assets/Art/02_MoiTruong/PA1_Background/Green.png";
        private const string TerrainPath = "Assets/Art/02_MoiTruong/PA1_Terrain/Terrain (16x16).png";

        // Prefab Paths
        private const string CoreRigPrefabPath = "Assets/Prefabs/CoreRig.prefab";
        private const string AudioManagerPrefabPath = "Assets/Prefabs/AudioManager.prefab";
        private const string LevelStartPrefabPath = "Assets/Prefabs/LevelStart.prefab";
        private const string ExitGatePrefabPath = "Assets/Prefabs/ExitGate.prefab";

        private const string Player1PrefabPath = "Assets/Prefabs/Player1.prefab";
        private const string Player2PrefabPath = "Assets/Prefabs/Player2.prefab";
        private const string SoulTetherPrefabPath = "Assets/Prefabs/SoulTether.prefab";

        private const string ApplePrefabPath = "Assets/Prefabs/Fruit_Apple.prefab";
        private const string CherriesPrefabPath = "Assets/Prefabs/Fruit_Cherries.prefab";

        private const string SpikesPrefabPath = "Assets/Prefabs/Spikes.prefab";
        private const string SawPrefabPath = "Assets/Prefabs/Saw.prefab";
        private const string DeathPitPrefabPath = "Assets/Prefabs/Traps/DeathPit.prefab";
        private const string FallingPlatformPrefabPath = "Assets/Prefabs/Traps/FallingPlatform.prefab";
        private const string TrampolinePrefabPath = "Assets/Prefabs/Traps/Trampoline.prefab";

        [MenuItem("Tools/One Hit Down/Build Level 01 Scene", false, 2)]
        public static void BuildScene()
        {
            try
            {
                if (File.Exists(ScenePath))
                {
                    if (!EditorUtility.DisplayDialog(
                        "Xác nhận ghi đè Level 01",
                        "Màn chơi 'Level_01.unity' hiện tại đã được bạn tinh chỉnh hoàn chỉnh.\nBạn có chắc chắn muốn DỰNG LẠI và GHI ĐÈ toàn bộ không?",
                        "Ghi đè",
                        "Hủy bỏ (Bảo vệ Scene)"))
                    {
                        Debug.Log("<b>[Level01SceneBuilder]</b> Đã hủy lệnh dựng lại để bảo vệ trọn vẹn màn chơi Level 01 hiện tại của bạn!");
                        return;
                    }
                }

                EditorUtility.DisplayProgressBar("One Hit Down", "Đang dựng Scene Level_01 chuẩn gameplay...", 0.1f);
                Debug.Log("<b>[Level01SceneBuilder]</b> BẮT ĐẦU DỰNG SCENE LEVEL_01.UNITY...");

                if (!Directory.Exists(SceneDir))
                {
                    Directory.CreateDirectory(SceneDir);
                }

                // 1. Tạo mới Scene trống
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // 2. Load các Sprite cần thiết
                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgGreenPath);

                Sprite grassTopMid = LoadSprite(TerrainPath, "Grass_TopMid");
                Sprite grassCenter = LoadSprite(TerrainPath, "Grass_Center");
                Sprite grassTopLeft = LoadSprite(TerrainPath, "Grass_TopLeft");
                Sprite grassTopRight = LoadSprite(TerrainPath, "Grass_TopRight");
                Sprite grassMidLeft = LoadSprite(TerrainPath, "Grass_MidLeft");
                Sprite grassMidRight = LoadSprite(TerrainPath, "Grass_MidRight");

                // 3. Header: 01. ENVIRONMENT & BACKGROUND
                GameObject envGroup = new GameObject("--- 01. ENVIRONMENT & BACKGROUND ---");

                // Background Green Tiled bao phủ trọn vẹn
                GameObject bgGo = new GameObject("Background_Green");
                bgGo.transform.SetParent(envGroup.transform);
                bgGo.transform.position = new Vector3(50f, 6f, 5f);
                SpriteRenderer bgSr = bgGo.AddComponent<SpriteRenderer>();
                bgSr.sprite = bgSprite;
                bgSr.drawMode = SpriteDrawMode.Tiled;
                bgSr.size = new Vector2(130f, 35f);
                bgSr.sortingOrder = -100;

                // Main Camera
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.transform.SetParent(envGroup.transform);
                camGo.transform.position = new Vector3(3.5f, 2.0f, -10f);
                Camera cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.backgroundColor = new Color(0.6f, 0.77f, 0.47f, 1f); // Xanh nền
                camGo.AddComponent<AudioListener>();

                CameraController camCtrl = camGo.AddComponent<CameraController>();
                SerializedObject camSo = new SerializedObject(camCtrl);
                camSo.FindProperty("targetOrthoSize").floatValue = 5.5f;
                camSo.FindProperty("smoothSpeed").floatValue = 6f;
                camSo.FindProperty("offset").vector3Value = new Vector3(0f, 1.5f, -10f);
                camSo.FindProperty("minY").floatValue = -3f;
                camSo.FindProperty("maxY").floatValue = 16f; // Vừa tầm mắt khi hạ Tầng 2 xuống y = 8
                camSo.FindProperty("autoFindInterval").floatValue = 0.25f;
                camSo.ApplyModifiedProperties();

                // 4. Header: 02. TERRAIN ARCHITECTURE
                GameObject terrainGroup = new GameObject("--- 02. TERRAIN & ARCHITECTURE ---");

                // Helper tạo khối núi đất đặc liền mạch không viền rác
                Action<string, int, int, float, float, bool, bool> makeSolidPillar = 
                    (name, startX, endX, surfaceY, bottomY, hasLeftCliff, hasRightCliff) =>
                {
                    BuildSolidTerrainPillar(
                        terrainGroup.transform,
                        name,
                        startX,
                        endX,
                        surfaceY,
                        bottomY,
                        hasLeftCliff,
                        hasRightCliff,
                        grassTopMid,
                        grassCenter,
                        grassTopLeft,
                        grassTopRight,
                        grassMidLeft,
                        grassMidRight
                    );
                };

                // Helper thêm BoxCollider2D độc lập cho mặt đất
                Action<string, float, float, float, float> addGroundCollider = (name, xMin, xMax, surfaceY, height) =>
                {
                    float width = xMax - xMin;
                    float centerX = (xMin + xMax) / 2f;
                    float centerY = surfaceY - (height / 2f);

                    GameObject colGo = new GameObject(name);
                    colGo.transform.SetParent(terrainGroup.transform);
                    colGo.transform.position = new Vector3(centerX, centerY, 0f);
                    colGo.layer = 8; // Layer 8: Ground
                    BoxCollider2D bc = colGo.AddComponent<BoxCollider2D>();
                    bc.size = new Vector2(width, height);
                };

                // =========================================================================
                // TẦNG DƯỚI (HẦM MỘ: Y = 0)
                // =========================================================================
                // 1. Sàn xuất phát: x: 0 .. 9 (rộng 10 tiles, y: 0, cao 5)
                makeSolidPillar("T1_Start", 0, 9, 0f, -5f, true, true);
                addGroundCollider("T1_Col_Start", 0f, 10f, 0f, 5f);

                // 2. Bậc gờ nâng nhịp 1: x: 10 .. 15 (rộng 6 tiles, y: 2, cao 7, chứa Quả Táo 1)
                makeSolidPillar("T1_Step1", 10, 15, 2f, -5f, true, true);
                addGroundCollider("T1_Col_Step1", 10f, 16f, 2f, 7f);

                // 3. Sàn đi tiếp tới hố gai: x: 16 .. 24 (rộng 9 tiles, y: 0, cao 5)
                makeSolidPillar("T1_Approach", 16, 24, 0f, -5f, true, true);
                addGroundCollider("T1_Col_Approach", 16f, 25f, 0f, 5f);

                // 4. Đáy hố gai cố định: x: 25 .. 29 (rộng 5 tiles, sâu y: -4, cao 1)
                makeSolidPillar("T1_SpikePit", 25, 29, -4f, -5f, true, true);
                addGroundCollider("T1_Col_SpikeFloor", 25f, 30f, -4f, 1f);

                // 5. SÀN TRƯỚC BẪY ẨN: x = 30 .. 34 (rộng 5 tiles, y: 0, cao 5)
                // Bờ phải không dùng góc viền đen để nối phẳng khít vào nắp bẫy sập!
                makeSolidPillar("T1_Floor_PreTrap", 30, 34, 0f, -5f, true, false);
                addGroundCollider("T1_Col_FloorPreTrap", 30f, 35f, 0f, 5f);

                // >>> KHOANG HỐ BẪY ẨN: x: 35 .. 37 <<<
                // Chứa nắp cỏ bề mặt VÀ tảng đất nâu bên dưới (do DeathPit_Hidden_1 quản lý 100%)

                // 6. SÀN SAU BẪY ẨN ĐẾN ĐỆM LÒ XO: x = 38 .. 43 (rộng 6 tiles, y: 0, cao 5)
                // Bờ trái không dùng góc viền đen để nối phẳng khít từ nắp bẫy sập!
                makeSolidPillar("T1_Floor_PostTrap", 38, 43, 0f, -5f, false, true);
                addGroundCollider("T1_Col_FloorPostTrap", 38f, 44f, 0f, 5f);

                // 7. TƯỜNG ĐÁ PHÂN TẦNG VỪA TẦM MẮT: x = 44 .. 48 (rộng 5 tiles, đỉnh tường y = 6, cao 11)
                // Độ cao 6 tiles cực kỳ lý tưởng để lò xo bắn nhẹ nhàng tiếp đất lên nóc!
                makeSolidPillar("T1_DivideWall", 44, 48, 6f, -5f, true, true);
                addGroundCollider("T1_Col_DivideWall", 44f, 49f, 6f, 11f);

                // =========================================================================
                // KHU VỰC CHUYỂN TẦNG VƯỢT VỰC (CỘT ĐÁ & BỆ RƠI LEO DỐC TỰ NHIÊN)
                // =========================================================================
                // Cột mỏm đá đứng giữa: x: 54 .. 55 (rộng 2 tiles, y = 7, cao 5)
                makeSolidPillar("T2_PillarMid", 54, 55, 7f, 2f, true, true);
                addGroundCollider("T2_Col_PillarMid", 54f, 56f, 7f, 5f);

                // =========================================================================
                // CAO NGUYÊN ĐỀN CỔ TẦNG 2 (Y = 8, CAO 8 TILES)
                // CÁC MỎM ĐÁ AN TOÀN ĐỨNG VỮNG XEN KẼ 4 HỐ SẬP ĐẦY ĐỦ CỎ VÀ ĐẤT
                // KHI CHƯA GIẪM: NẮP CỎ VÀ TẢNG ĐẤT NÂU LẤP KÍN HỐ -> ĐẶC PHẲNG LÌ 100%!
                // KHI GIẪM VÀO: CẢ CỎ VÀ TẢNG ĐẤT NÂU CÙNG BIẾN MẤT -> MỞ TOANG VỰC SÂU 8 TILES!
                // =========================================================================
                // 1. Sân chờ trước chuỗi bẫy: x = 65 .. 70 (rộng 6 tiles, y: 8, cao 8)
                makeSolidPillar("T2_PreRhythm", 65, 70, 8f, 0f, true, false);
                addGroundCollider("T2_Col_PreRhythmPlaza", 65f, 71f, 8f, 8f);

                // >>> HỐ 1: x: 71 .. 73 <<<

                // 2. Mỏm đá an toàn 1 (P2 Neo dây): x = 74 .. 75 (rộng 2 tiles, y: 8, cao 8)
                makeSolidPillar("T2_SafeLedge_1", 74, 75, 8f, 0f, false, false);
                addGroundCollider("T2_Col_SafeLedge_1", 74f, 76f, 8f, 8f);

                // >>> HỐ 2: x: 76 .. 78 <<<

                // 3. Mỏm đá an toàn 2 (P1 Nhảy tới): x = 79 .. 80 (rộng 2 tiles, y: 8, cao 8)
                makeSolidPillar("T2_SafeLedge_2", 79, 80, 8f, 0f, false, false);
                addGroundCollider("T2_Col_SafeLedge_2", 79f, 81f, 8f, 8f);

                // >>> HỐ 3: x: 81 .. 83 <<<

                // 4. Mỏm đá an toàn 3 [Chứa Quả Táo 3]: x = 84 .. 85 (rộng 2 tiles, y: 8, cao 8)
                makeSolidPillar("T2_SafeLedge_3", 84, 85, 8f, 0f, false, false);
                addGroundCollider("T2_Col_SafeLedge_3", 84f, 86f, 8f, 8f);

                // >>> HỐ 4: x: 86 .. 88 <<<

                // 5. Sân Cổng Thần Exit Gate: x = 89 .. 95 (rộng 7 tiles, y: 8, cao 8)
                makeSolidPillar("T2_ExitPlaza", 89, 95, 8f, 0f, false, true);
                addGroundCollider("T2_Col_ExitPlaza", 89f, 96f, 8f, 8f);


                // 5. Header: 03. TRAPS & HAZARDS
                GameObject trapGroup = new GameObject("--- 03. TRAPS & HAZARDS ---");

                GameObject spikesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpikesPrefabPath);
                GameObject deathPitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DeathPitPrefabPath);
                GameObject fallingPlatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FallingPlatformPrefabPath);
                GameObject sawPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SawPrefabPath);
                GameObject trampPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TrampolinePrefabPath);

                // Spikes ở Hố Gai (Tầng 1)
                if (spikesPrefab != null)
                {
                    for (int sx = 25; sx <= 29; sx++)
                    {
                        GameObject spk = PrefabUtility.InstantiatePrefab(spikesPrefab) as GameObject;
                        spk.transform.SetParent(trapGroup.transform);
                        spk.transform.position = new Vector3(sx + 0.5f, -3.5f, 0f);
                    }
                }

                // =========================================================================
                // BẪY ẨN SỐ 1 Ở TẦNG DƯỚI (x: 35..37)
                // GỒM: NẮP CỎ BỀ MẶT + TẢNG ĐẤT NÂU BÊN DƯỚI (DirtBody)
                // KHI GIẪM VÀO: CẢ CỎ VÀ TẢNG ĐẤT NÂU CÙNG BIẾN MẤT -> MỞ TOANG HỐ SÂU!
                // =========================================================================
                if (deathPitPrefab != null)
                {
                    GameObject dp1 = PrefabUtility.InstantiatePrefab(deathPitPrefab) as GameObject;
                    dp1.name = "DeathPit_Hidden_1";
                    dp1.transform.SetParent(trapGroup.transform);
                    dp1.transform.position = new Vector3(36.5f, -0.5f, 0f); // Center hố 35..37

                    BoxCollider2D dp1Col = dp1.GetComponent<BoxCollider2D>();
                    if (dp1Col != null)
                    {
                        dp1Col.size = new Vector2(3f, 1f);
                        dp1Col.offset = Vector2.zero;
                    }

                    // Vùng chết Hazard nằm sâu trong lòng hố
                    Transform dtz1 = dp1.transform.Find("DeathTriggerZone");
                    if (dtz1 != null)
                    {
                        BoxCollider2D dtzCol = dtz1.GetComponent<BoxCollider2D>();
                        if (dtzCol != null)
                        {
                            dtzCol.size = new Vector2(3f, 4f);
                            dtzCol.offset = new Vector2(0f, -2.5f);
                        }
                    }

                    // TẢNG ĐẤT MÀU NÂU BÊN DƯỚI (DirtBody)
                    GameObject dirtGo = new GameObject("DirtBody");
                    dirtGo.transform.SetParent(dp1.transform);
                    dirtGo.transform.localPosition = new Vector3(0f, -2.5f, 0f);
                    SpriteRenderer dirtSr = dirtGo.AddComponent<SpriteRenderer>();
                    dirtSr.sprite = grassCenter;
                    dirtSr.drawMode = SpriteDrawMode.Tiled;
                    dirtSr.tileMode = SpriteTileMode.Continuous;
                    dirtSr.size = new Vector2(3f, 4f);
                    dirtSr.sortingOrder = 1;

                    if (grassTopMid != null)
                    {
                        SpriteRenderer dpSr = dp1.GetComponent<SpriteRenderer>();
                        if (dpSr != null)
                        {
                            dpSr.sprite = grassTopMid;
                            dpSr.drawMode = SpriteDrawMode.Tiled;
                            dpSr.tileMode = SpriteTileMode.Continuous;
                            dpSr.size = new Vector2(3f, 1f);
                            dpSr.sortingOrder = 2; // Đè khít bề mặt sàn
                        }

                        DeathPitTrap dpt = dp1.GetComponent<DeathPitTrap>();
                        if (dpt != null)
                        {
                            SerializedObject dptSo = new SerializedObject(dpt);
                            dptSo.FindProperty("idleSprite").objectReferenceValue = grassTopMid;
                            dptSo.FindProperty("dirtRenderer").objectReferenceValue = dirtSr;
                            dptSo.ApplyModifiedProperties();
                        }
                    }
                }

                // =========================================================================
                // ĐỆM LÒ XO TRAMPOLINE - NHẢY BỔNG VỪA TẦM MẮT LÊN ĐỈNH TƯỜNG ĐÁ (Y = 6)
                // Lực nhún 16f nhẹ nhàng, vòng cung parabol đẹp mắt, tiếp đất êm ái x = 46!
                // =========================================================================
                if (trampPrefab != null)
                {
                    GameObject tr = PrefabUtility.InstantiatePrefab(trampPrefab) as GameObject;
                    tr.name = "Trampoline_To_Tier2";
                    tr.transform.SetParent(trapGroup.transform);
                    tr.transform.position = new Vector3(41.5f, 0.45f, 0f);

                    TrampolineTrap tt = tr.GetComponent<TrampolineTrap>();
                    if (tt != null)
                    {
                        SerializedObject ttSo = new SerializedObject(tt);
                        // Đẩy hướng lên trên chếch nhẹ về phía trước để rơi trúng đỉnh tường đá x = 46, y = 6
                        Vector2 bounceDir = new Vector2(0.35f, 0.937f).normalized;
                        ttSo.FindProperty("bounceDirection").vector2Value = bounceDir;
                        ttSo.FindProperty("bounceForce").floatValue = 16f; // Lực 16f cực kỳ mượt mà, bay bổng 6.5 tiles
                        ttSo.ApplyModifiedProperties();
                    }
                }

                // Chuỗi Bệ rơi và Cưa máy leo dốc từ Tường đá (y = 6) sang Cao nguyên (y = 8)
                Action<Vector3, string> spawnFallingPlatform = (pos, name) =>
                {
                    if (fallingPlatPrefab != null)
                    {
                        GameObject fp = PrefabUtility.InstantiatePrefab(fallingPlatPrefab) as GameObject;
                        fp.name = name;
                        fp.transform.SetParent(trapGroup.transform);
                        fp.transform.position = pos;

                        FallingPlatform fps = fp.GetComponent<FallingPlatform>();
                        if (fps != null)
                        {
                            SerializedObject fpsSo = new SerializedObject(fps);
                            fpsSo.FindProperty("isPermanentInRun").boolValue = false;
                            fpsSo.FindProperty("shakeDuration").floatValue = 0.8f; // Đủ 0.8s để người chơi dễ dàng vượt qua
                            fpsSo.FindProperty("respawnDuration").floatValue = 3f;
                            fpsSo.ApplyModifiedProperties();
                        }
                    }
                };

                // Bệ rơi 1: x = 51.5, y = 6.5 (bước nhảy từ tường đá x: 48 sang)
                spawnFallingPlatform(new Vector3(51.5f, 6.5f, 0f), "FallingPlatform_1");
                // Cột đá đứng giữa ở x: 54..55 tại y: 7
                // Bệ rơi 2: x = 58.5, y = 7.5 (chứa Quả Táo 2 bên trên)
                spawnFallingPlatform(new Vector3(58.5f, 7.5f, 0f), "FallingPlatform_2");
                // Bệ rơi 3: x = 62.5, y = 7.5 (bước nhảy chuẩn bị bước vào Cao nguyên x = 65)
                spawnFallingPlatform(new Vector3(62.5f, 7.5f, 0f), "FallingPlatform_3");

                // =========================================================================
                // LƯỠI CƯA TUẦN TRA THEO CHIỀU DỌC (VERTICAL PATROL)
                // Cưa di chuyển lên xuống: khi cưa nâng lên trên, khoảng trống bên dưới mở toang
                // giúp người chơi dễ dàng canh nhịp nhảy qua an toàn và nhặt Quả Táo 2!
                // =========================================================================
                Action<Vector3, string, float, float> spawnVerticalSaw = (pos, name, speed, range) =>
                {
                    if (sawPrefab != null)
                    {
                        GameObject saw = PrefabUtility.InstantiatePrefab(sawPrefab) as GameObject;
                        saw.name = name;
                        saw.transform.SetParent(trapGroup.transform);
                        saw.transform.position = pos;

                        SawSpinner ss = saw.GetComponent<SawSpinner>();
                        if (ss != null)
                        {
                            SerializedObject ssSo = new SerializedObject(ss);
                            ssSo.FindProperty("patrolMode").enumValueIndex = (int)PatrolMode.PingPong;
                            ssSo.FindProperty("patrolAxis").enumValueIndex = (int)PatrolAxis.Vertical; // Di chuyển dọc!
                            ssSo.FindProperty("patrolSpeed").floatValue = speed;
                            ssSo.FindProperty("patrolRange").floatValue = range;
                            ssSo.ApplyModifiedProperties();
                        }
                    }
                };

                // Saw 1: Di chuyển dọc từ y = 9.2 lên y = 11.7 (khi lên cao, khoảng y = 7..9.2 hoàn toàn thông thoáng!)
                spawnVerticalSaw(new Vector3(56.5f, 9.2f, 0f), "Saw_1", 1.8f, 2.5f);
                // Saw 2: Di chuyển dọc từ y = 9.5 lên y = 12.0
                spawnVerticalSaw(new Vector3(61.0f, 9.5f, 0f), "Saw_2", 1.8f, 2.5f);

                // =========================================================================
                // CHUỖI 4 HỐ SẬP NHỊP ĐIỆU TRÊN CAO NGUYÊN ĐỀN CỔ (Y = 8)
                // MỖI HỐ CÓ: NẮP CỎ BỀ MẶT (height = 1) + TẢNG ĐẤT NÂU BÊN DƯỚI (height = 7)
                // KHI CHƯA GIẪM: LẤP KÍN HỐ NÊN NHÌN NÚI ĐẶC 100% LIỀN MẠCH!
                // KHI GIẪM VÀO: CẢ CỎ VÀ TẢNG ĐẤT NÂU ĐỀU MẤT -> MỞ TOANG VỰC SÂU 8 TILES!
                // =========================================================================
                Action<float, float, string> spawnOverlayDeathPit = (xMin, xMax, name) =>
                {
                    float width = xMax - xMin;
                    float centerX = (xMin + xMax) / 2f;

                    if (deathPitPrefab != null)
                    {
                        GameObject rdp = PrefabUtility.InstantiatePrefab(deathPitPrefab) as GameObject;
                        rdp.name = name;
                        rdp.transform.SetParent(trapGroup.transform);
                        // Đặt ngay sát mặt cỏ: y = 7.5 (surface flush tại y = 8.0)
                        rdp.transform.position = new Vector3(centerX, 7.5f, 0f);

                        BoxCollider2D rdpCol = rdp.GetComponent<BoxCollider2D>();
                        if (rdpCol != null)
                        {
                            rdpCol.size = new Vector2(width, 1f);
                            rdpCol.offset = Vector2.zero;
                        }

                        // Vùng chết Hazard nằm sâu trong lòng hố vực sâu
                        Transform dtz = rdp.transform.Find("DeathTriggerZone");
                        if (dtz != null)
                        {
                            BoxCollider2D dtzCol = dtz.GetComponent<BoxCollider2D>();
                            if (dtzCol != null)
                            {
                                dtzCol.size = new Vector2(width, 6f);
                                dtzCol.offset = new Vector2(0f, -3.5f);
                            }
                        }

                        // TẢNG ĐẤT MÀU NÂU BÊN DƯỚI (DirtBody: cao 7 tiles từ y = 7 xuống y = 0)
                        GameObject dirtChild = new GameObject("DirtBody");
                        dirtChild.transform.SetParent(rdp.transform);
                        dirtChild.transform.localPosition = new Vector3(0f, -4.0f, 0f);
                        SpriteRenderer rdpDirtSr = dirtChild.AddComponent<SpriteRenderer>();
                        rdpDirtSr.sprite = grassCenter;
                        rdpDirtSr.drawMode = SpriteDrawMode.Tiled;
                        rdpDirtSr.tileMode = SpriteTileMode.Continuous;
                        rdpDirtSr.size = new Vector2(width, 7f);
                        rdpDirtSr.sortingOrder = 1;

                        if (grassTopMid != null)
                        {
                            SpriteRenderer rdpSr = rdp.GetComponent<SpriteRenderer>();
                            if (rdpSr != null)
                            {
                                rdpSr.sprite = grassTopMid;
                                rdpSr.drawMode = SpriteDrawMode.Tiled;
                                rdpSr.tileMode = SpriteTileMode.Continuous;
                                rdpSr.size = new Vector2(width, 1f);
                                rdpSr.sortingOrder = 2; // Đè khít lên thảm cỏ cao nguyên
                            }

                            DeathPitTrap dpt = rdp.GetComponent<DeathPitTrap>();
                            if (dpt != null)
                            {
                                SerializedObject dptSo = new SerializedObject(dpt);
                                dptSo.FindProperty("idleSprite").objectReferenceValue = grassTopMid;
                                dptSo.FindProperty("dirtRenderer").objectReferenceValue = rdpDirtSr;
                                dptSo.ApplyModifiedProperties();
                            }
                        }
                    }
                };

                // 4 Khối Bẫy Sập ngụy trang (đầy đủ nắp cỏ và tảng đất nâu)
                spawnOverlayDeathPit(71f, 74f, "DeathPit_Rhythm_1"); // Hố 1: 71 .. 74
                spawnOverlayDeathPit(76f, 79f, "DeathPit_Rhythm_2"); // Hố 2: 76 .. 79
                spawnOverlayDeathPit(81f, 84f, "DeathPit_Rhythm_3"); // Hố 3: 81 .. 84
                spawnOverlayDeathPit(86f, 89f, "DeathPit_Rhythm_4"); // Hố 4: 86 .. 89


                // 6. Header: 04. ITEMS & OBJECTIVES
                GameObject itemGroup = new GameObject("--- 04. ITEMS & OBJECTIVES ---");

                GameObject startPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelStartPrefabPath);
                GameObject exitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExitGatePrefabPath);
                GameObject applePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ApplePrefabPath);
                GameObject cherryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CherriesPrefabPath);

                // Level Start Point
                GameObject startGo = null;
                LevelStartPoint startPoint = null;
                if (startPrefab != null)
                {
                    startGo = PrefabUtility.InstantiatePrefab(startPrefab) as GameObject;
                    startGo.transform.SetParent(itemGroup.transform);
                    startGo.transform.position = new Vector3(1.5f, 0f, 0f);
                    startPoint = startGo.GetComponent<LevelStartPoint>();
                }

                // Exit Gate (tại Sân Cổng Thần y = 8)
                GameObject exitGo = null;
                ExitGate exitGate = null;
                if (exitPrefab != null)
                {
                    exitGo = PrefabUtility.InstantiatePrefab(exitPrefab) as GameObject;
                    exitGo.transform.SetParent(itemGroup.transform);
                    exitGo.transform.position = new Vector3(92.5f, 8.0f, 0f);
                    exitGate = exitGo.GetComponent<ExitGate>();
                }

                // 3 Quả Táo Cổ Vật (countsAsRelic = true)
                Action<Vector3, string> spawnRelicApple = (pos, name) =>
                {
                    if (applePrefab != null)
                    {
                        GameObject ap = PrefabUtility.InstantiatePrefab(applePrefab) as GameObject;
                        ap.name = name;
                        ap.transform.SetParent(itemGroup.transform);
                        ap.transform.position = pos;

                        CollectibleItem ci = ap.GetComponent<CollectibleItem>();
                        if (ci != null)
                        {
                            SerializedObject ciSo = new SerializedObject(ci);
                            ciSo.FindProperty("countsAsRelic").boolValue = true;
                            ciSo.ApplyModifiedProperties();
                        }
                    }
                };

                // Quả Táo 1: Trên bậc gờ Tầng 1 (x = 12.5, y = 3.0)
                spawnRelicApple(new Vector3(12.5f, 3.0f, 0f), "Apple_Relic_1");

                // Quả Táo 2: Lơ lửng trên bệ rơi 2 giữa 2 lưỡi cưa (x = 58.5, y = 9.0)
                spawnRelicApple(new Vector3(58.5f, 9.0f, 0f), "Apple_Relic_2");

                // Quả Táo 3: Đặt trên Mỏm đá an toàn 3 giữa chuỗi bẫy (x = 84.5, y = 9.0)
                spawnRelicApple(new Vector3(84.5f, 9.0f, 0f), "Apple_Relic_3");

                // Quả Cherry mồi nhử phía trên Bẫy ẩn số 1 (x = 36.5, y = 1.2, countsAsRelic = false)
                if (cherryPrefab != null)
                {
                    GameObject ch = PrefabUtility.InstantiatePrefab(cherryPrefab) as GameObject;
                    ch.name = "Cherry_Bait";
                    ch.transform.SetParent(itemGroup.transform);
                    ch.transform.position = new Vector3(36.5f, 1.2f, 0f);

                    CollectibleItem ci = ch.GetComponent<CollectibleItem>();
                    if (ci != null)
                    {
                        SerializedObject ciSo = new SerializedObject(ci);
                        ciSo.FindProperty("countsAsRelic").boolValue = false;
                        ciSo.ApplyModifiedProperties();
                    }
                }


                // 7. Header: 00. CORE MANAGERS
                GameObject coreGroup = new GameObject("--- 00. CORE MANAGERS ---");

                // AudioManager
                GameObject audioMgrGo = null;
                AudioManager audioMgr = null;
                GameObject audioPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AudioManagerPrefabPath);
                if (audioPrefab != null)
                {
                    audioMgrGo = PrefabUtility.InstantiatePrefab(audioPrefab) as GameObject;
                    audioMgrGo.transform.SetParent(coreGroup.transform);
                    audioMgr = audioMgrGo.GetComponent<AudioManager>();
                }

                // CoreRig (Chứa GameManager, HUD, Canvas, PauseMenu, IrisTransition)
                GameObject coreRigGo = null;
                GameManager gm = null;
                HUDController hud = null;
                PauseMenu pauseMenu = null;
                IrisTransition iris = null;

                GameObject coreRigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoreRigPrefabPath);
                if (coreRigPrefab != null)
                {
                    coreRigGo = PrefabUtility.InstantiatePrefab(coreRigPrefab) as GameObject;
                    coreRigGo.transform.SetParent(coreGroup.transform);

                    gm = coreRigGo.GetComponentInChildren<GameManager>(true);
                    hud = coreRigGo.GetComponentInChildren<HUDController>(true);
                    pauseMenu = coreRigGo.GetComponentInChildren<PauseMenu>(true);
                    iris = coreRigGo.GetComponentInChildren<IrisTransition>(true);
                }

                // Thiết lập liên kết cho GameManager
                if (gm != null)
                {
                    SerializedObject gmSo = new SerializedObject(gm);
                    gmSo.FindProperty("startPoint").objectReferenceValue = startPoint;
                    gmSo.FindProperty("exitGate").objectReferenceValue = exitGate;

                    GameObject p1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Player1PrefabPath);
                    GameObject p2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Player2PrefabPath);
                    GameObject tetherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SoulTetherPrefabPath);

                    if (p1Prefab != null) gmSo.FindProperty("player1Prefab").objectReferenceValue = p1Prefab.GetComponent<PlayerController>();
                    if (p2Prefab != null) gmSo.FindProperty("player2Prefab").objectReferenceValue = p2Prefab.GetComponent<PlayerController>();
                    if (tetherPrefab != null) gmSo.FindProperty("soulTetherPrefab").objectReferenceValue = tetherPrefab.GetComponent<SoulTether>();

                    gmSo.FindProperty("levelIndexOverride").intValue = 1; // Level 01
                    gmSo.FindProperty("totalRelicsRequired").intValue = 3;
                    gmSo.FindProperty("respawnDelay").floatValue = 0.18f;

                    gmSo.FindProperty("mainCamera").objectReferenceValue = camCtrl;
                    gmSo.FindProperty("hudController").objectReferenceValue = hud;
                    gmSo.FindProperty("pauseMenu").objectReferenceValue = pauseMenu;
                    gmSo.FindProperty("irisTransition").objectReferenceValue = iris;
                    gmSo.FindProperty("audioManager").objectReferenceValue = audioMgr;

                    gmSo.ApplyModifiedProperties();
                }

                // 8. Lưu Scene
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log($"<b>[Level01SceneBuilder]</b> ĐÃ LƯU SCENE TẠI: {ScenePath}");

                RegisterInBuildSettings(ScenePath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                Debug.Log("<b>[Level01SceneBuilder]</b> HOÀN TẤT DỰNG SCENE LEVEL_01 THÀNH CÔNG 100%!");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[Level01SceneBuilder] LỖI KHI DỰNG SCENE LEVEL_01: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Dựng một khối núi đất đặc liền mạch duy nhất (Solid Pillar) từ startX đến endX.
        /// Sử dụng Grass_TopMid trên mặt cỏ và Grass_Center dưới thân đất.
        /// Tùy biến bờ trái / bờ phải để nối phẳng khít không viền với nắp bẫy sập!
        /// </summary>
        private static void BuildSolidTerrainPillar(
            Transform parent,
            string name,
            int startX,
            int endX,
            float surfaceY,
            float bottomY,
            bool hasLeftCliff,
            bool hasRightCliff,
            Sprite topMid,
            Sprite centerDirt,
            Sprite topLeft,
            Sprite topRight,
            Sprite midLeft,
            Sprite midRight)
        {
            float width = (endX - startX + 1);
            float height = (surfaceY - bottomY);
            float centerX = startX + (width / 2f);
            float centerY = surfaceY - (height / 2f);

            GameObject p = new GameObject(name);
            p.transform.SetParent(parent);
            p.transform.position = new Vector3(centerX, centerY, 0f);

            float leftInset = hasLeftCliff ? 1f : 0f;
            float rightInset = hasRightCliff ? 1f : 0f;
            float middleWidth = width - leftInset - rightInset;

            // 1. VẼ MẶT TRÊN CỎ XANH (SURFACE GRASS)
            if (hasLeftCliff && topLeft != null)
            {
                GameObject tlGo = new GameObject("Grass_Corner_Left");
                tlGo.transform.SetParent(p.transform);
                tlGo.transform.localPosition = new Vector3(-width / 2f + 0.5f, height / 2f - 0.5f, 0f);
                SpriteRenderer srTl = tlGo.AddComponent<SpriteRenderer>();
                srTl.sprite = topLeft;
                srTl.sortingOrder = 1;
            }

            if (middleWidth > 0f && topMid != null)
            {
                GameObject tmGo = new GameObject("Grass_Surface_Mid");
                tmGo.transform.SetParent(p.transform);
                float midCenterX = -width / 2f + leftInset + (middleWidth / 2f);
                tmGo.transform.localPosition = new Vector3(midCenterX, height / 2f - 0.5f, 0f);
                SpriteRenderer srTm = tmGo.AddComponent<SpriteRenderer>();
                srTm.sprite = topMid;
                srTm.drawMode = SpriteDrawMode.Tiled;
                srTm.tileMode = SpriteTileMode.Continuous;
                srTm.size = new Vector2(middleWidth, 1f);
                srTm.sortingOrder = 1;
            }

            if (hasRightCliff && topRight != null)
            {
                GameObject trGo = new GameObject("Grass_Corner_Right");
                trGo.transform.SetParent(p.transform);
                trGo.transform.localPosition = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, 0f);
                SpriteRenderer srTr = trGo.AddComponent<SpriteRenderer>();
                srTr.sprite = topRight;
                srTr.sortingOrder = 1;
            }

            // 2. VẼ THÂN ĐẤT ĐẶC BÊN DƯỚI (SOLID DIRT FILL)
            float dirtHeight = height - 1f;
            if (dirtHeight > 0f)
            {
                float dirtCenterY = -height / 2f + (dirtHeight / 2f);

                if (hasLeftCliff && midLeft != null)
                {
                    GameObject mlGo = new GameObject("Dirt_Cliff_Left");
                    mlGo.transform.SetParent(p.transform);
                    mlGo.transform.localPosition = new Vector3(-width / 2f + 0.5f, dirtCenterY, 0f);
                    SpriteRenderer srMl = mlGo.AddComponent<SpriteRenderer>();
                    srMl.sprite = midLeft;
                    srMl.drawMode = SpriteDrawMode.Tiled;
                    srMl.tileMode = SpriteTileMode.Continuous;
                    srMl.size = new Vector2(1f, dirtHeight);
                    srMl.sortingOrder = 1;
                }

                if (middleWidth > 0f && centerDirt != null)
                {
                    GameObject cdGo = new GameObject("Dirt_Body_Mid");
                    cdGo.transform.SetParent(p.transform);
                    float midCenterX = -width / 2f + leftInset + (middleWidth / 2f);
                    cdGo.transform.localPosition = new Vector3(midCenterX, dirtCenterY, 0f);
                    SpriteRenderer srCd = cdGo.AddComponent<SpriteRenderer>();
                    srCd.sprite = centerDirt;
                    srCd.drawMode = SpriteDrawMode.Tiled;
                    srCd.tileMode = SpriteTileMode.Continuous;
                    srCd.size = new Vector2(middleWidth, dirtHeight);
                    srCd.sortingOrder = 1;
                }

                if (hasRightCliff && midRight != null)
                {
                    GameObject mrGo = new GameObject("Dirt_Cliff_Right");
                    mrGo.transform.SetParent(p.transform);
                    mrGo.transform.localPosition = new Vector3(width / 2f - 0.5f, dirtCenterY, 0f);
                    SpriteRenderer srMr = mrGo.AddComponent<SpriteRenderer>();
                    srMr.sprite = midRight;
                    srMr.drawMode = SpriteDrawMode.Tiled;
                    srMr.tileMode = SpriteTileMode.Continuous;
                    srMr.size = new Vector2(1f, dirtHeight);
                    srMr.sortingOrder = 1;
                }
            }
        }

        private static Sprite LoadSprite(string texturePath, string preferredName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    if (asset is Sprite s && s.name == preferredName) return s;
                }
            }
            return null;
        }

        private static void RegisterInBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == scenePath)
                {
                    scenes[i].enabled = true;
                    EditorBuildSettings.scenes = scenes;
                    return;
                }
            }

            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            Array.Copy(scenes, newScenes, scenes.Length);
            newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }

        public static void BuildFromCommandLine()
        {
            BuildScene();
            EditorApplication.Exit(0);
        }
    }
}
#endif
