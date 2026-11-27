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
    /// Phiên bản mở rộng ~182 tiles:
    /// - Đồng bộ 3 quả Kiwi cổ vật (Fruit_Kiwi.prefab) khớp với icon HUD.
    /// - Nửa đầu (x = 0..95): làm quen nhảy, hố gai, DeathPit ẩn, lò xo, 3 bệ rơi tự hồi (3s) + 2 cưa dọc, chuỗi 4 DeathPit nhịp điệu.
    /// - Nửa sau (x = 96..182): lò xo 2, chuỗi 5 bệ rơi vĩnh viễn (isPermanentInRun = true), cưa 3, ArrowBooster,
    ///   2 DeathPit hành lang và 3 mỏm đá so le trên cao tạo cơ chế "Ác Mộng Quay Đầu".
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

        private const string KiwiPrefabPath = "Assets/Prefabs/Fruit_Kiwi.prefab";
        private const string CherriesPrefabPath = "Assets/Prefabs/Fruit_Cherries.prefab";

        private const string SpikesPrefabPath = "Assets/Prefabs/Spikes.prefab";
        private const string SawPrefabPath = "Assets/Prefabs/Saw.prefab";
        private const string DeathPitPrefabPath = "Assets/Prefabs/Traps/DeathPit.prefab";
        private const string FallingPlatformPrefabPath = "Assets/Prefabs/Traps/FallingPlatform.prefab";
        private const string TrampolinePrefabPath = "Assets/Prefabs/Traps/Trampoline.prefab";
        private const string ArrowBoosterPrefabPath = "Assets/Prefabs/Traps/ArrowBooster.prefab";

        [MenuItem("Tools/One Hit Down/Build Level 01 Scene", false, 2)]
        public static void BuildScene()
        {
            try
            {
                if (File.Exists(ScenePath))
                {
                    if (!EditorUtility.DisplayDialog(
                        "Xác nhận ghi đè Level 01",
                        "Màn chơi 'Level_01.unity' hiện tại sẽ được dựng lại theo bản thiết kế ~182 tiles (3 Kiwi + Ác Mộng Quay Đầu).\nBạn có chắc chắn muốn GHI ĐÈ không?",
                        "Ghi đè",
                        "Hủy bỏ (Bảo vệ Scene)"))
                    {
                        Debug.Log("<b>[Level01SceneBuilder]</b> Đã hủy lệnh dựng lại để bảo vệ Scene Level 01 hiện tại.");
                        return;
                    }
                }

                EditorUtility.DisplayProgressBar("One Hit Down", "Đang dựng Scene Level_01 (~182 tiles)...", 0.1f);

                if (!Directory.Exists(SceneDir))
                {
                    Directory.CreateDirectory(SceneDir);
                }

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgGreenPath);
                Sprite grassTopMid = LoadSprite(TerrainPath, "Grass_TopMid");
                Sprite grassCenter = LoadSprite(TerrainPath, "Grass_Center");
                Sprite grassTopLeft = LoadSprite(TerrainPath, "Grass_TopLeft");
                Sprite grassTopRight = LoadSprite(TerrainPath, "Grass_TopRight");
                Sprite grassMidLeft = LoadSprite(TerrainPath, "Grass_MidLeft");
                Sprite grassMidRight = LoadSprite(TerrainPath, "Grass_MidRight");

                // 01. ENVIRONMENT & BACKGROUND
                GameObject envGroup = new GameObject("--- 01. ENVIRONMENT & BACKGROUND ---");

                GameObject bgGo = new GameObject("Background_Green");
                bgGo.transform.SetParent(envGroup.transform);
                bgGo.transform.position = new Vector3(95f, 8f, 5f);
                SpriteRenderer bgSr = bgGo.AddComponent<SpriteRenderer>();
                bgSr.sprite = bgSprite;
                bgSr.drawMode = SpriteDrawMode.Tiled;
                bgSr.size = new Vector2(230f, 42f);
                bgSr.sortingOrder = -100;

                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.transform.SetParent(envGroup.transform);
                camGo.transform.position = new Vector3(3.5f, 2.0f, -10f);
                Camera cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.backgroundColor = new Color(0.6f, 0.77f, 0.47f, 1f);
                camGo.AddComponent<AudioListener>();

                CameraController camCtrl = camGo.AddComponent<CameraController>();
                SerializedObject camSo = new SerializedObject(camCtrl);
                camSo.FindProperty("targetOrthoSize").floatValue = 5.5f;
                camSo.FindProperty("smoothSpeed").floatValue = 6f;
                camSo.FindProperty("offset").vector3Value = new Vector3(0f, 1.5f, -10f);
                camSo.FindProperty("minY").floatValue = -3f;
                camSo.FindProperty("maxY").floatValue = 20f;
                camSo.FindProperty("autoFindInterval").floatValue = 0.25f;
                camSo.ApplyModifiedProperties();

                // 02. TERRAIN ARCHITECTURE
                GameObject terrainGroup = new GameObject("--- 02. TERRAIN & ARCHITECTURE ---");

                Action<string, int, int, float, float, bool, bool> makeSolidPillar =
                    (name, startX, endX, surfaceY, bottomY, hasLeftCliff, hasRightCliff) =>
                {
                    BuildSolidTerrainPillar(
                        terrainGroup.transform, name, startX, endX, surfaceY, bottomY,
                        hasLeftCliff, hasRightCliff,
                        grassTopMid, grassCenter, grassTopLeft, grassTopRight, grassMidLeft, grassMidRight);
                };

                Action<string, float, float, float, float> addGroundCollider = (name, xMin, xMax, surfaceY, height) =>
                {
                    float width = xMax - xMin;
                    float centerX = (xMin + xMax) / 2f;
                    float centerY = surfaceY - (height / 2f);

                    GameObject colGo = new GameObject(name);
                    colGo.transform.SetParent(terrainGroup.transform);
                    colGo.transform.position = new Vector3(centerX, centerY, 0f);
                    colGo.layer = 8; // Ground
                    BoxCollider2D bc = colGo.AddComponent<BoxCollider2D>();
                    bc.size = new Vector2(width, height);
                };

                // === NỬA ĐẦU (x = 0 .. 95) ===
                makeSolidPillar("T1_Start", 0, 9, 0f, -5f, true, true);
                addGroundCollider("T1_Col_Start", 0f, 10f, 0f, 5f);

                makeSolidPillar("T1_Step1", 10, 15, 2f, -5f, true, true);
                addGroundCollider("T1_Col_Step1", 10f, 16f, 2f, 7f);

                makeSolidPillar("T1_Approach", 16, 24, 0f, -5f, true, true);
                addGroundCollider("T1_Col_Approach", 16f, 25f, 0f, 5f);

                makeSolidPillar("T1_SpikePit", 25, 29, -4f, -5f, true, true);
                addGroundCollider("T1_Col_SpikeFloor", 25f, 30f, -4f, 1f);

                makeSolidPillar("T1_Floor_PreTrap", 30, 34, 0f, -5f, true, false);
                addGroundCollider("T1_Col_FloorPreTrap", 30f, 35f, 0f, 5f);

                makeSolidPillar("T1_Floor_PostTrap", 38, 43, 0f, -5f, false, true);
                addGroundCollider("T1_Col_FloorPostTrap", 38f, 44f, 0f, 5f);

                makeSolidPillar("T1_DivideWall", 44, 48, 6f, -5f, true, true);
                addGroundCollider("T1_Col_DivideWall", 44f, 49f, 6f, 11f);

                makeSolidPillar("T2_PillarMid", 54, 55, 7f, 2f, true, true);
                addGroundCollider("T2_Col_PillarMid", 54f, 56f, 7f, 5f);

                makeSolidPillar("T2_PreRhythm", 65, 70, 8f, 0f, true, false);
                addGroundCollider("T2_Col_PreRhythmPlaza", 65f, 71f, 8f, 8f);

                makeSolidPillar("T2_SafeLedge_1", 74, 75, 8f, 0f, false, false);
                addGroundCollider("T2_Col_SafeLedge_1", 74f, 76f, 8f, 8f);

                makeSolidPillar("T2_SafeLedge_2", 79, 80, 8f, 0f, false, false);
                addGroundCollider("T2_Col_SafeLedge_2", 79f, 81f, 8f, 8f);

                makeSolidPillar("T2_SafeLedge_3", 84, 85, 8f, 0f, false, false);
                addGroundCollider("T2_Col_SafeLedge_3", 84f, 86f, 8f, 8f);

                makeSolidPillar("T2_TransitionPlaza", 89, 95, 8f, 0f, false, false);
                addGroundCollider("T2_Col_TransitionPlaza", 89f, 96f, 8f, 8f);

                // === NỬA SAU MỞ RỘNG (x = 96 .. 182) ===
                makeSolidPillar("T2_SpringPad", 99, 103, 8f, 0f, false, true);
                addGroundCollider("T2_Col_SpringPad", 99f, 104f, 8f, 8f);

                makeSolidPillar("T3_Landing", 104, 107, 11f, 0f, true, true);
                addGroundCollider("T3_Col_Landing", 104f, 108f, 11f, 11f);

                makeSolidPillar("T3_Junction", 124, 129, 10f, 0f, true, false);
                addGroundCollider("T3_Col_Junction", 124f, 130f, 10f, 10f);

                makeSolidPillar("T3_SpikeIsland", 133, 136, 10f, 0f, false, false);
                addGroundCollider("T3_Col_SpikeIsland", 133f, 137f, 10f, 10f);

                makeSolidPillar("T3_PreNightmare", 140, 145, 10f, 0f, false, true);
                addGroundCollider("T3_Col_PreNightmare", 140f, 146f, 10f, 10f);

                makeSolidPillar("T3_PostNightmare", 156, 163, 10f, 0f, true, true);
                addGroundCollider("T3_Col_PostNightmare", 156f, 164f, 10f, 10f);

                // 3 Mỏm đá so le trên cao (Ác Mộng Quay Đầu)
                makeSolidPillar("T3_StaggerLedge_3", 158, 159, 12.5f, 11.5f, true, true);
                addGroundCollider("T3_Col_StaggerLedge_3", 158f, 160f, 12.5f, 1f);

                makeSolidPillar("T3_StaggerLedge_2", 152, 153, 14.0f, 13.0f, true, true);
                addGroundCollider("T3_Col_StaggerLedge_2", 152f, 154f, 14.0f, 1f);

                makeSolidPillar("T3_StaggerLedge_1", 146, 147, 12.5f, 11.5f, true, true);
                addGroundCollider("T3_Col_StaggerLedge_1", 146f, 148f, 12.5f, 1f);

                makeSolidPillar("T3_PreExitSpikes", 164, 166, 7f, 0f, true, true);
                addGroundCollider("T3_Col_PreExitSpikes", 164f, 167f, 7f, 7f);

                makeSolidPillar("T3_ExitPlaza", 167, 181, 10f, 0f, true, true);
                addGroundCollider("T3_Col_ExitPlaza", 167f, 182f, 10f, 10f);

                // 03. TRAPS & HAZARDS
                GameObject trapGroup = new GameObject("--- 03. TRAPS & HAZARDS ---");

                GameObject spikesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpikesPrefabPath);
                GameObject deathPitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DeathPitPrefabPath);
                GameObject fallingPlatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FallingPlatformPrefabPath);
                GameObject sawPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SawPrefabPath);
                GameObject trampPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TrampolinePrefabPath);
                GameObject arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowBoosterPrefabPath);

                if (spikesPrefab != null)
                {
                    for (int sx = 25; sx <= 29; sx++)
                    {
                        GameObject spk = PrefabUtility.InstantiatePrefab(spikesPrefab) as GameObject;
                        spk.transform.SetParent(trapGroup.transform);
                        spk.transform.position = new Vector3(sx + 0.5f, -3.5f, 0f);
                    }
                    for (int sx = 134; sx <= 135; sx++)
                    {
                        GameObject spk = PrefabUtility.InstantiatePrefab(spikesPrefab) as GameObject;
                        spk.transform.SetParent(trapGroup.transform);
                        spk.transform.position = new Vector3(sx + 0.5f, 10.5f, 0f);
                    }
                    for (int sx = 164; sx <= 166; sx++)
                    {
                        GameObject spk = PrefabUtility.InstantiatePrefab(spikesPrefab) as GameObject;
                        spk.transform.SetParent(trapGroup.transform);
                        spk.transform.position = new Vector3(sx + 0.5f, 7.5f, 0f);
                    }
                }

                Action<float, float, float, float, string> spawnOverlayDeathPit = (xMin, xMax, surfaceY, dirtDepth, name) =>
                {
                    if (deathPitPrefab == null) return;
                    float width = xMax - xMin;
                    float centerX = (xMin + xMax) / 2f;

                    GameObject rdp = PrefabUtility.InstantiatePrefab(deathPitPrefab) as GameObject;
                    rdp.name = name;
                    rdp.transform.SetParent(trapGroup.transform);
                    rdp.transform.position = new Vector3(centerX, surfaceY - 0.5f, 0f);

                    BoxCollider2D rdpCol = rdp.GetComponent<BoxCollider2D>();
                    if (rdpCol != null)
                    {
                        rdpCol.size = new Vector2(width, 1f);
                        rdpCol.offset = Vector2.zero;
                    }

                    Transform dtz = rdp.transform.Find("DeathTriggerZone");
                    if (dtz != null)
                    {
                        BoxCollider2D dtzCol = dtz.GetComponent<BoxCollider2D>();
                        if (dtzCol != null)
                        {
                            dtzCol.size = new Vector2(width, 3f);
                            dtzCol.offset = new Vector2(0f, -(dirtDepth + 1.5f));
                        }
                    }

                    GameObject dirtChild = new GameObject("DirtBody");
                    dirtChild.transform.SetParent(rdp.transform);
                    dirtChild.transform.localPosition = new Vector3(0f, -(0.5f + dirtDepth / 2f), 0f);
                    SpriteRenderer rdpDirtSr = dirtChild.AddComponent<SpriteRenderer>();
                    rdpDirtSr.sprite = grassCenter;
                    rdpDirtSr.drawMode = SpriteDrawMode.Tiled;
                    rdpDirtSr.tileMode = SpriteTileMode.Continuous;
                    rdpDirtSr.size = new Vector2(width, dirtDepth);
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
                            rdpSr.sortingOrder = 2;
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
                };

                spawnOverlayDeathPit(35f, 38f, 0f, 4f, "DeathPit_Hidden_1");
                spawnOverlayDeathPit(71f, 74f, 8f, 7f, "DeathPit_Rhythm_1");
                spawnOverlayDeathPit(76f, 79f, 8f, 7f, "DeathPit_Rhythm_2");
                spawnOverlayDeathPit(81f, 84f, 8f, 7f, "DeathPit_Rhythm_3");
                spawnOverlayDeathPit(86f, 89f, 8f, 7f, "DeathPit_Rhythm_4");

                spawnOverlayDeathPit(96f, 99f, 8f, 7f, "DeathPit_Hidden_2");
                spawnOverlayDeathPit(130f, 133f, 10f, 9f, "DeathPit_Perm_1");
                spawnOverlayDeathPit(137f, 140f, 10f, 9f, "DeathPit_Perm_2");

                Action<Vector3, string> spawnTrampoline = (pos, name) =>
                {
                    if (trampPrefab == null) return;
                    GameObject tr = PrefabUtility.InstantiatePrefab(trampPrefab) as GameObject;
                    tr.name = name;
                    tr.transform.SetParent(trapGroup.transform);
                    tr.transform.position = pos;
                    TrampolineTrap tt = tr.GetComponent<TrampolineTrap>();
                    if (tt != null)
                    {
                        SerializedObject ttSo = new SerializedObject(tt);
                        ttSo.FindProperty("bounceDirection").vector2Value = new Vector2(0.35f, 0.937f).normalized;
                        ttSo.FindProperty("bounceForce").floatValue = 16f;
                        ttSo.ApplyModifiedProperties();
                    }
                };

                spawnTrampoline(new Vector3(41.5f, 0.45f, 0f), "Trampoline_To_Tier2");
                spawnTrampoline(new Vector3(102.5f, 8.45f, 0f), "Trampoline_2");

                Action<Vector3, string, bool, float> spawnFallingPlatform = (pos, name, permanent, shake) =>
                {
                    if (fallingPlatPrefab == null) return;
                    GameObject fp = PrefabUtility.InstantiatePrefab(fallingPlatPrefab) as GameObject;
                    fp.name = name;
                    fp.transform.SetParent(trapGroup.transform);
                    fp.transform.position = pos;
                    FallingPlatform fps = fp.GetComponent<FallingPlatform>();
                    if (fps != null)
                    {
                        SerializedObject fpsSo = new SerializedObject(fps);
                        fpsSo.FindProperty("isPermanentInRun").boolValue = permanent;
                        fpsSo.FindProperty("shakeDuration").floatValue = shake;
                        fpsSo.FindProperty("respawnDuration").floatValue = 3f;
                        fpsSo.ApplyModifiedProperties();
                    }
                };

                spawnFallingPlatform(new Vector3(51.5f, 6.5f, 0f), "FallingPlatform_1", false, 0.8f);
                spawnFallingPlatform(new Vector3(58.5f, 7.5f, 0f), "FallingPlatform_2", false, 0.8f);
                spawnFallingPlatform(new Vector3(62.5f, 7.5f, 0f), "FallingPlatform_3", false, 0.8f);

                spawnFallingPlatform(new Vector3(110.5f, 11.0f, 0f), "FallingPlatform_Perm_1", true, 0.4f);
                spawnFallingPlatform(new Vector3(114.5f, 11.5f, 0f), "FallingPlatform_Perm_2", true, 0.4f);
                spawnFallingPlatform(new Vector3(118.5f, 11.0f, 0f), "FallingPlatform_Perm_3", true, 0.4f);
                spawnFallingPlatform(new Vector3(148.5f, 10.0f, 0f), "FallingPlatform_Perm_4", true, 0.4f);
                spawnFallingPlatform(new Vector3(152.5f, 10.0f, 0f), "FallingPlatform_Perm_5", true, 0.4f);

                Action<Vector3, string, float, float> spawnVerticalSaw = (pos, name, speed, range) =>
                {
                    if (sawPrefab == null) return;
                    GameObject saw = PrefabUtility.InstantiatePrefab(sawPrefab) as GameObject;
                    saw.name = name;
                    saw.transform.SetParent(trapGroup.transform);
                    saw.transform.position = pos;
                    SawSpinner ss = saw.GetComponent<SawSpinner>();
                    if (ss != null)
                    {
                        SerializedObject ssSo = new SerializedObject(ss);
                        ssSo.FindProperty("patrolMode").enumValueIndex = (int)PatrolMode.PingPong;
                        ssSo.FindProperty("patrolAxis").enumValueIndex = (int)PatrolAxis.Vertical;
                        ssSo.FindProperty("patrolSpeed").floatValue = speed;
                        ssSo.FindProperty("patrolRange").floatValue = range;
                        ssSo.ApplyModifiedProperties();
                    }
                };

                spawnVerticalSaw(new Vector3(56.5f, 9.2f, 0f), "Saw_1", 1.8f, 2.5f);
                spawnVerticalSaw(new Vector3(61.0f, 9.5f, 0f), "Saw_2", 1.8f, 2.5f);
                spawnVerticalSaw(new Vector3(112.5f, 13.0f, 0f), "Saw_3", 1.8f, 2.5f);

                if (arrowPrefab != null)
                {
                    GameObject ab = PrefabUtility.InstantiatePrefab(arrowPrefab) as GameObject;
                    ab.name = "ArrowBooster_1";
                    ab.transform.SetParent(trapGroup.transform);
                    ab.transform.position = new Vector3(121.5f, 11.5f, 0f);
                    ArrowBooster abComp = ab.GetComponent<ArrowBooster>();
                    if (abComp != null)
                    {
                        SerializedObject abSo = new SerializedObject(abComp);
                        abSo.FindProperty("boostForce").floatValue = 14f;
                        abSo.FindProperty("boostDirection").vector2Value = new Vector2(1f, 0.35f).normalized;
                        abSo.FindProperty("reactivateDelay").floatValue = 2.5f;
                        abSo.ApplyModifiedProperties();
                    }
                }

                GameObject abyssKillZone = new GameObject("KillZone_Abyss");
                abyssKillZone.transform.SetParent(trapGroup.transform);
                abyssKillZone.transform.position = new Vector3(114.5f, -7.5f, 0f);
                abyssKillZone.tag = "Hazard";
                abyssKillZone.layer = LayerMask.NameToLayer("Traps") >= 0 ? LayerMask.NameToLayer("Traps") : 10;
                BoxCollider2D abyssCol = abyssKillZone.AddComponent<BoxCollider2D>();
                abyssCol.isTrigger = true;
                abyssCol.size = new Vector2(141f, 4f);
                abyssKillZone.AddComponent<KillZone>();

                // 04. ITEMS & OBJECTIVES
                GameObject itemGroup = new GameObject("--- 04. ITEMS & OBJECTIVES ---");

                GameObject startPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelStartPrefabPath);
                GameObject exitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExitGatePrefabPath);
                GameObject kiwiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(KiwiPrefabPath);
                GameObject cherryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CherriesPrefabPath);

                LevelStartPoint startPoint = null;
                if (startPrefab != null)
                {
                    GameObject startGo = PrefabUtility.InstantiatePrefab(startPrefab) as GameObject;
                    startGo.transform.SetParent(itemGroup.transform);
                    startGo.transform.position = new Vector3(1.5f, 0f, 0f);
                    startPoint = startGo.GetComponent<LevelStartPoint>();
                }

                ExitGate exitGate = null;
                if (exitPrefab != null)
                {
                    GameObject exitGo = PrefabUtility.InstantiatePrefab(exitPrefab) as GameObject;
                    exitGo.transform.SetParent(itemGroup.transform);
                    exitGo.transform.position = new Vector3(176.0f, 10.0f, 0f);
                    exitGate = exitGo.GetComponent<ExitGate>();
                }

                Action<Vector3, string> spawnRelicKiwi = (pos, name) =>
                {
                    if (kiwiPrefab == null) return;
                    GameObject kw = PrefabUtility.InstantiatePrefab(kiwiPrefab) as GameObject;
                    kw.name = name;
                    kw.transform.SetParent(itemGroup.transform);
                    kw.transform.position = pos;
                    CollectibleItem ci = kw.GetComponent<CollectibleItem>();
                    if (ci != null)
                    {
                        SerializedObject ciSo = new SerializedObject(ci);
                        ciSo.FindProperty("countsAsRelic").boolValue = true;
                        ciSo.ApplyModifiedProperties();
                    }
                };

                spawnRelicKiwi(new Vector3(12.5f, 3.0f, 0f), "Kiwi_Relic_1");
                spawnRelicKiwi(new Vector3(114.5f, 13.0f, 0f), "Kiwi_Relic_2");
                spawnRelicKiwi(new Vector3(153.0f, 15.5f, 0f), "Kiwi_Relic_3");

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

                // 00. CORE MANAGERS
                GameObject coreGroup = new GameObject("--- 00. CORE MANAGERS ---");

                AudioManager audioMgr = null;
                GameObject audioPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AudioManagerPrefabPath);
                if (audioPrefab != null)
                {
                    GameObject audioMgrGo = PrefabUtility.InstantiatePrefab(audioPrefab) as GameObject;
                    audioMgrGo.transform.SetParent(coreGroup.transform);
                    audioMgr = audioMgrGo.GetComponent<AudioManager>();
                }

                GameManager gm = null;
                HUDController hud = null;
                PauseMenu pauseMenu = null;
                IrisTransition iris = null;

                GameObject coreRigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoreRigPrefabPath);
                if (coreRigPrefab != null)
                {
                    GameObject coreRigGo = PrefabUtility.InstantiatePrefab(coreRigPrefab) as GameObject;
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

                    GameObject p1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Player1PrefabPath);
                    GameObject p2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Player2PrefabPath);
                    GameObject tetherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SoulTetherPrefabPath);

                    if (p1Prefab != null) gmSo.FindProperty("player1Prefab").objectReferenceValue = p1Prefab.GetComponent<PlayerController>();
                    if (p2Prefab != null) gmSo.FindProperty("player2Prefab").objectReferenceValue = p2Prefab.GetComponent<PlayerController>();
                    if (tetherPrefab != null) gmSo.FindProperty("soulTetherPrefab").objectReferenceValue = tetherPrefab.GetComponent<SoulTether>();

                    gmSo.FindProperty("levelIndexOverride").intValue = 1;
                    gmSo.FindProperty("totalRelicsRequired").intValue = 3;
                    gmSo.FindProperty("respawnDelay").floatValue = 0.18f;

                    gmSo.FindProperty("mainCamera").objectReferenceValue = camCtrl;
                    gmSo.FindProperty("hudController").objectReferenceValue = hud;
                    gmSo.FindProperty("pauseMenu").objectReferenceValue = pauseMenu;
                    gmSo.FindProperty("irisTransition").objectReferenceValue = iris;
                    gmSo.FindProperty("audioManager").objectReferenceValue = audioMgr;
                    gmSo.ApplyModifiedProperties();
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                RegisterInBuildSettings(ScenePath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                Debug.Log($"<b>[Level01SceneBuilder]</b> ĐÃ LƯU SCENE LEVEL 01 (~182 TILES) TẠI: {ScenePath}");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[Level01SceneBuilder] LỖI KHI DỰNG SCENE LEVEL_01: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void BuildSolidTerrainPillar(
            Transform parent, string name, int startX, int endX, float surfaceY, float bottomY,
            bool hasLeftCliff, bool hasRightCliff,
            Sprite topMid, Sprite centerDirt, Sprite topLeft, Sprite topRight, Sprite midLeft, Sprite midRight)
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
