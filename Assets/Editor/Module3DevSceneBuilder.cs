// Trần Văn Tuyên & Nhóm 505
// Script Unity Editor: Tự động tạo hoàn chỉnh 100% Scene nháp Assets/Scenes/Dev/Dev_Nguoi3.unity
// Thiết lập Camera 2D, Sàn gạch Layer Ground, Test Player điều khiển A/D + Space
// và bố trí tuần tự cả 6 Prefabs thành 6 trạm kiểm thử độc lập.
// Menu Editor: Tools -> One Hit Down -> Build Dev_Nguoi3 Scene

#if UNITY_EDITOR
using System;
using System.IO;
using OneHitDown.Dev;
using OneHitDown.Player;
using OneHitDown.Traps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Công cụ Editor tự động hóa Bước 4:
    /// Xây dựng Scene kiểm thử độc lập Dev_Nguoi3.unity cho toàn bộ Module 3:
    /// Trạm 1: Trampoline (rơi xuống nảy bổng 20f + hồi nhảy đôi).
    /// Trạm 2: FanTrap (1 quạt thổi bổng Y, 1 quạt thổi ngang X).
    /// Trạm 3: FallingPlatform (bệ đá rung 0.4s rồi sụp rơi).
    /// Trạm 4: DeathPit (giẫm lên vỡ nứt thành hố tử thần vĩnh viễn).
    /// Trạm 5: IceRoad (sàn băng trơn trượt mất ma sát).
    /// Trạm 6: ArrowBooster (mũi tên đẩy khí giữa trời).
    /// </summary>
    public static class Module3DevSceneBuilder
    {
        private const string SceneDir = "Assets/Scenes/Dev";
        private const string ScenePath = "Assets/Scenes/Dev/Dev_Nguoi3.unity";
        private const string PrefabsTrapsDir = "Assets/Prefabs/Traps";
        private const string PrefabsRootDir = "Assets/Prefabs";
        private const string TerrainTexturePath = "Assets/Art/02_MoiTruong/PA1_Terrain/Terrain (16x16).png";
        private const string PlayerControllerAssetPath = "Assets/Art/Animations/NinjaFrog/NinjaFrog_Controller.controller";
        private const string PlayerSpritePath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Ninja Frog/Idle (32x32).png";

        [MenuItem("Tools/One Hit Down/Build Dev_Nguoi3 Scene", false, 20)]
        public static void BuildDevSceneMenu()
        {
            BuildDevScene(openScene: true);
        }

        [MenuItem("Tools/One Hit Down/Build Everything (Assets + Dev Scene)", false, 5)]
        public static void BuildEverythingMenu()
        {
            Module3AssetBuilder.BuildAll();
            BuildDevScene(openScene: true);
        }

        [InitializeOnLoadMethod]
        private static void OnProjectLoaded()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(ScenePath))
                {
                    Debug.Log("<b>[Module3DevSceneBuilder]</b> Phát hiện chưa có Scene Dev_Nguoi3.unity. Đang tự động khởi tạo...");
                    BuildDevScene(openScene: false);
                }
            };
        }

        /// <summary>
        /// Tạo mới hoàn toàn Scene Dev_Nguoi3.unity, nạp đầy đủ môi trường, camera, nhân vật và 6 trạm kiểm thử.
        /// </summary>
        public static void BuildDevScene(bool openScene = true)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Module 3 - Build Dev Scene", "Đang khởi tạo Scene mới...", 0.05f);
                EnsureDirectory(SceneDir);

                // 1. Tạo Scene sạch trong Editor
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // 2. Tải tài nguyên đồ họa chuẩn
                Sprite groundSprite = LoadFirstSprite(TerrainTexturePath, "Terrain (16x16)_1");
                Sprite playerSprite = LoadFirstSprite(PlayerSpritePath, "Idle (32x32)_0");

                // Tạo các nhóm GameObject quản lý trong Hierarchy
                GameObject envGroup = new GameObject("--- ENVIRONMENT ---");
                GameObject stationsGroup = new GameObject("--- 6 TEST STATIONS ---");
                GameObject playerGroup = new GameObject("--- CHARACTERS ---");

                // 3. Tạo Camera 2D
                EditorUtility.DisplayProgressBar("Module 3 - Build Dev Scene", "Đang thiết lập Camera 2D...", 0.20f);
                GameObject cameraGo = CreateCamera2D(envGroup.transform);

                // 4. Tạo các sàn gạch kiểm thử (Layer 8: Ground)
                EditorUtility.DisplayProgressBar("Module 3 - Build Dev Scene", "Đang tạo sàn gạch thử nghiệm...", 0.40f);
                CreatePlatforms(envGroup.transform, groundSprite);

                // 5. Tạo Nhân vật Player thử nghiệm (A/D di chuyển, Space nhảy)
                EditorUtility.DisplayProgressBar("Module 3 - Build Dev Scene", "Đang thiết lập Test Player...", 0.60f);
                GameObject playerGo = CreateTestPlayer(playerGroup.transform, playerSprite);

                // Gắn mục tiêu theo dõi cho Camera
                DevCameraFollow camFollow = cameraGo.GetComponent<DevCameraFollow>();
                if (camFollow != null && playerGo != null)
                {
                    camFollow.SetTarget(playerGo.transform);
                }

                // 6. Sắp xếp 6 trạm kiểm thử độc lập
                EditorUtility.DisplayProgressBar("Module 3 - Build Dev Scene", "Đang sắp xếp 6 trạm kiểm thử...", 0.80f);
                BuildStation1_Trampoline(stationsGroup.transform);
                BuildStation2_FanTrap(stationsGroup.transform);
                BuildStation3_FallingPlatform(stationsGroup.transform);
                BuildStation4_DeathPit(stationsGroup.transform);
                BuildStation5_IceRoad(stationsGroup.transform);
                BuildStation6_ArrowBooster(stationsGroup.transform);

                // Tạo bảng hiệu / biển chỉ dẫn giữa các trạm
                CreateStationLabels(stationsGroup.transform);

                // 7. Lưu và làm mới Scene
                EditorUtility.DisplayProgressBar("Module 3 - Build Dev Scene", "Đang lưu Scene...", 0.95f);
                bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (saved)
                {
                    Debug.Log($"<b>[Module3DevSceneBuilder]</b> TẠO THÀNH CÔNG SCENE: <color=green>{ScenePath}</color>");
                    if (openScene)
                    {
                        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                    }
                    EditorUtility.DisplayDialog(
                        "One Hit Down - Module 3",
                        $"Đã tạo thành công Scene thử nghiệm:\n{ScenePath}\n\n" +
                        "Bao gồm:\n" +
                        "• Camera 2D theo dõi nhân vật mượt mà\n" +
                        "• Sàn gạch Layer 8 Ground\n" +
                        "• Test Player (A/D di chuyển, Space nhảy đơn & nhảy đôi, R hồi sinh)\n" +
                        "• Tuần tự cả 6 trạm kiểm thử độc lập.\n\n" +
                        "Bạn có thể bấm nút Play để thử nghiệm ngay!",
                        "Tuyệt vời"
                    );
                }
                else
                {
                    Debug.LogError($"[Module3DevSceneBuilder] Không thể lưu Scene tại: {ScenePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Module3DevSceneBuilder] Lỗi trong quá trình tạo Scene: {ex}");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #region Setup Components

        private static GameObject CreateCamera2D(Transform parent)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.transform.SetParent(parent);
            camGo.transform.position = new Vector3(-2f, 2.5f, -10f);
            camGo.tag = "MainCamera";

            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            // Màu nền slate tối trầm giúp pixel art nổi bật
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.18f, 1f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;

            camGo.AddComponent<AudioListener>();

            // Script Camera theo dõi
            DevCameraFollow follow = camGo.AddComponent<DevCameraFollow>();
            SerializedObject so = new SerializedObject(follow);
            so.FindProperty("offset").vector3Value = new Vector3(0f, 1.8f, -10f);
            so.FindProperty("smoothSpeed").floatValue = 6f;
            so.FindProperty("minBounds").vector2Value = new Vector2(-6f, -3f);
            so.FindProperty("maxBounds").vector2Value = new Vector2(75f, 16f);
            so.ApplyModifiedProperties();

            return camGo;
        }

        private static GameObject CreateTestPlayer(Transform parent, Sprite playerSprite)
        {
            GameObject playerGo = new GameObject("TestPlayer");
            playerGo.transform.SetParent(parent);
            playerGo.transform.position = new Vector3(-2.5f, 1.2f, 0f);
            playerGo.tag = "Player";

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                playerGo.layer = playerLayer;
            }

            // SpriteRenderer
            SpriteRenderer sr = playerGo.AddComponent<SpriteRenderer>();
            sr.sprite = playerSprite;
            sr.sortingOrder = 10;

            // BoxCollider2D (Khớp hình dáng nhân vật 32x32 PPU 16)
            // Sprite Ninja Frog có pivot Bottom-Center (y = 0 là bàn chân) nên đáy collider phải trùng y = 0:
            // offset.y = size.y / 2. Trước đây đáy ở -0.6 khiến nhân vật lơ lửng cách đất ~0.6 unit.
            BoxCollider2D col = playerGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.3f);
            col.offset = new Vector2(0f, 0.65f);

            // Animator Ninja Frog: Idle (thở/nhún) - Run - Jump - Fall - DoubleJump
            Animator animator = playerGo.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerControllerAssetPath);
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"[Module3DevSceneBuilder] Không tìm thấy Animator Controller tại {PlayerControllerAssetPath}.");
            }

            // Rigidbody2D (Chuẩn Unity 6 & Schema §3.2)
            Rigidbody2D rb = playerGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            rb.gravityScale = 3.5f;

            // PlayerController
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

            int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
            so.FindProperty("groundLayer").intValue = groundLayerMask;
            so.ApplyModifiedProperties();

            return playerGo;
        }

        private static void CreatePlatforms(Transform parent, Sprite groundSprite)
        {
            GameObject groundGroup = new GameObject("GroundPlatforms");
            groundGroup.transform.SetParent(parent);

            // 1. Khu xuất phát (X: -6 đến 1, Y: top 0)
            CreateGroundBox("Start_Platform", new Vector2(-2.5f, -0.5f), new Vector2(7f, 1f), groundGroup.transform, groundSprite);

            // 2. Sàn Trạm 1 - Trampoline (X: 1 đến 8, Y: top 0)
            CreateGroundBox("Station1_Floor", new Vector2(4.5f, -0.5f), new Vector2(7f, 1f), groundGroup.transform, groundSprite);
            // Bệ đón trên cao cho Trampoline nảy lên (X: 8.5 đến 11.5, Y: top 6.0)
            CreateGroundBox("Station1_HighLedge", new Vector2(10f, 5.5f), new Vector2(3f, 1f), groundGroup.transform, groundSprite);
            // Cầu thang xuống Trạm 2
            CreateGroundBox("Station1_Stair1", new Vector2(11.5f, 3.5f), new Vector2(2f, 1f), groundGroup.transform, groundSprite);
            CreateGroundBox("Station1_Stair2", new Vector2(12f, 1.5f), new Vector2(2f, 1f), groundGroup.transform, groundSprite);

            // 3. Sàn Trạm 2 - FanTrap (X: 12 đến 22, Y: top 0)
            CreateGroundBox("Station2_Floor", new Vector2(17f, -0.5f), new Vector2(10f, 1f), groundGroup.transform, groundSprite);
            // Trụ gắn quạt ngang
            CreateGroundBox("Station2_FanPillar", new Vector2(16.5f, 0.75f), new Vector2(1f, 1.5f), groundGroup.transform, groundSprite);

            // 4. Sàn Trạm 3 - FallingPlatform (Bệ đá qua vực)
            CreateGroundBox("Station3_LeftLedge", new Vector2(23.5f, 1.0f), new Vector2(3f, 1f), groundGroup.transform, groundSprite);
            CreateGroundBox("Station3_RightLedge", new Vector2(32.5f, 1.0f), new Vector2(3f, 1f), groundGroup.transform, groundSprite);
            // Đáy vực an toàn phía dưới
            CreateGroundBox("Station3_PitBottom", new Vector2(28f, -4.5f), new Vector2(10f, 1f), groundGroup.transform, groundSprite);

            // 5. Sàn Trạm 4 - DeathPit
            CreateGroundBox("Station4_LeftApproach", new Vector2(35f, 1.0f), new Vector2(2f, 1f), groundGroup.transform, groundSprite);
            CreateGroundBox("Station4_RightExit", new Vector2(41.5f, 1.0f), new Vector2(2f, 1f), groundGroup.transform, groundSprite);
            // Đáy hố tử thần
            CreateGroundBox("Station4_PitBottom", new Vector2(38.3f, -4.5f), new Vector2(6f, 1f), groundGroup.transform, groundSprite);

            // 6. Sàn Trạm 5 - IceRoad
            CreateGroundBox("Station5_IceFoundation", new Vector2(48f, 0.8f), new Vector2(8f, 1f), groundGroup.transform, groundSprite);

            // 7. Sàn Trạm 6 - ArrowBooster (Hẻm núi rộng 10 unit)
            CreateGroundBox("Station6_LaunchLedge", new Vector2(53f, 1.5f), new Vector2(2.5f, 2f), groundGroup.transform, groundSprite);
            CreateGroundBox("Station6_CanyonBottom", new Vector2(59f, -4.5f), new Vector2(12f, 1f), groundGroup.transform, groundSprite);
            // Bệ Đích / Chiến Thắng (Victory Altar)
            CreateGroundBox("Victory_Altar", new Vector2(66.5f, 2.5f), new Vector2(5f, 2f), groundGroup.transform, groundSprite);
        }

        private static GameObject CreateGroundBox(string name, Vector2 center, Vector2 size, Transform parent, Sprite sprite)
        {
            GameObject boxGo = new GameObject(name);
            boxGo.transform.SetParent(parent);
            boxGo.transform.position = new Vector3(center.x, center.y, 0f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                boxGo.layer = groundLayer;
            }

            SpriteRenderer sr = boxGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingOrder = 0;

            BoxCollider2D col = boxGo.AddComponent<BoxCollider2D>();
            col.size = size;
            col.offset = Vector2.zero;

            return boxGo;
        }

        #endregion

        #region 6 Test Stations Setup

        /// <summary>
        /// Trạm 1: Trampoline - Rơi xuống là nảy bổng 20f + hồi nhảy đôi, tiếp đất lên bệ cao 6f.
        /// </summary>
        private static void BuildStation1_Trampoline(Transform parent)
        {
            GameObject group = new GameObject("Station_1_Trampoline");
            group.transform.SetParent(parent);

            GameObject prefab = LoadPrefab("Trampoline");
            if (prefab == null) return;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
            instance.name = "Trampoline";
            instance.transform.position = new Vector3(5.0f, 0.25f, 0f);

            TrampolineTrap trap = instance.GetComponent<TrampolineTrap>();
            if (trap != null)
            {
                SerializedObject so = new SerializedObject(trap);
                so.FindProperty("bounceForce").floatValue = 20f;
                so.FindProperty("bounceDirection").vector2Value = Vector2.up;
                so.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Trạm 2: FanTrap - 1 quạt thổi bổng Y (nâng người chơi), 1 quạt thổi ngang X (đẩy xuôi hành lang).
        /// </summary>
        private static void BuildStation2_FanTrap(Transform parent)
        {
            GameObject group = new GameObject("Station_2_FanTrap");
            group.transform.SetParent(parent);

            GameObject prefab = LoadPrefab("FanTrap");
            if (prefab == null) return;

            // 1. Quạt thổi dọc Y
            GameObject fanVertical = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
            fanVertical.name = "FanTrap_Vertical_Y";
            fanVertical.transform.position = new Vector3(14.0f, 0.25f, 0f);

            FanTrap trapV = fanVertical.GetComponent<FanTrap>();
            if (trapV != null)
            {
                SerializedObject so = new SerializedObject(trapV);
                so.FindProperty("windDirection").vector2Value = Vector2.up;
                so.FindProperty("windForce").floatValue = 15f;
                so.FindProperty("isActive").boolValue = true;
                so.ApplyModifiedProperties();
            }

            // 2. Quạt thổi ngang X
            GameObject fanHorizontal = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
            fanHorizontal.name = "FanTrap_Horizontal_X";
            fanHorizontal.transform.position = new Vector3(17.2f, 1.8f, 0f);

            FanTrap trapH = fanHorizontal.GetComponent<FanTrap>();
            if (trapH != null)
            {
                SerializedObject so = new SerializedObject(trapH);
                so.FindProperty("windDirection").vector2Value = Vector2.right;
                so.FindProperty("windForce").floatValue = 15f;
                so.FindProperty("isActive").boolValue = true;
                so.ApplyModifiedProperties();

                // Điều chỉnh collider vùng gió thổi sang phải
                Transform windZone = fanHorizontal.transform.Find("WindZone");
                if (windZone != null)
                {
                    BoxCollider2D wzCol = windZone.GetComponent<BoxCollider2D>();
                    if (wzCol != null)
                    {
                        wzCol.size = new Vector2(4.5f, 1.5f);
                        wzCol.offset = new Vector2(2.25f, 0f);
                    }
                }
            }
        }

        /// <summary>
        /// Trạm 3: FallingPlatform - Bệ đá rung 0.4s rồi sụp rơi vĩnh viễn suốt lượt chơi.
        /// </summary>
        private static void BuildStation3_FallingPlatform(Transform parent)
        {
            GameObject group = new GameObject("Station_3_FallingPlatform");
            group.transform.SetParent(parent);

            GameObject prefab = LoadPrefab("FallingPlatform");
            if (prefab == null) return;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
            instance.name = "FallingPlatform";
            instance.transform.position = new Vector3(28.0f, 1.5f, 0f);

            FallingPlatform fp = instance.GetComponent<FallingPlatform>();
            if (fp != null)
            {
                SerializedObject so = new SerializedObject(fp);
                so.FindProperty("shakeDuration").floatValue = 0.4f;
                so.FindProperty("fallSpeed").floatValue = 12f;
                so.FindProperty("isPermanentInRun").boolValue = true;
                so.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Trạm 4: DeathPit - Khối ngụy trang giẫm lên rung 0.2s vỡ nát thành Hố Tử Thần vĩnh viễn.
        /// </summary>
        private static void BuildStation4_DeathPit(Transform parent)
        {
            GameObject group = new GameObject("Station_4_DeathPit");
            group.transform.SetParent(parent);

            GameObject prefab = LoadPrefab("DeathPit");
            if (prefab == null) return;

            // Đặt 2 khối liền kề nhau tạo hố rộng 2.75 tiles (22px = 1.375 unit/khối)
            GameObject block1 = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
            block1.name = "DeathPit_Block1";
            block1.transform.position = new Vector3(37.5f, 1.5f, 0f);

            GameObject block2 = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
            block2.name = "DeathPit_Block2";
            block2.transform.position = new Vector3(38.875f, 1.5f, 0f);

            foreach (var b in new[] { block1, block2 })
            {
                DeathPitTrap trap = b.GetComponent<DeathPitTrap>();
                if (trap != null)
                {
                    SerializedObject so = new SerializedObject(trap);
                    so.FindProperty("shakeDuration").floatValue = 0.2f;
                    so.ApplyModifiedProperties();
                }
            }
        }

        /// <summary>
        /// Trạm 5: IceRoad - Sàn băng trơn trượt mất ma sát (iceFriction = 4f), phát hiệu ứng bụi tuyết.
        /// </summary>
        private static void BuildStation5_IceRoad(Transform parent)
        {
            GameObject group = new GameObject("Station_5_IceRoad");
            group.transform.SetParent(parent);

            GameObject prefab = LoadPrefab("IceRoad");
            if (prefab == null) return;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
            instance.name = "IceRoad";
            instance.transform.position = new Vector3(48.0f, 1.48f, 0f);

            // Mở rộng bề rộng sàn băng trải dài 7 unit
            SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(7f, 0.375f);
            }

            // Prefab IceRoad có 2 BoxCollider2D (solid + trigger): phải chỉnh cả hai cho khớp bề rộng 7 unit,
            // nếu không trigger nhỏ hơn mặt băng sẽ tắt trạng thái trượt khi nhân vật vẫn đang đứng trên băng.
            foreach (BoxCollider2D col in instance.GetComponents<BoxCollider2D>())
            {
                if (col.isTrigger)
                {
                    col.size = new Vector2(7f, 0.6f);
                    col.offset = new Vector2(0f, 0.1f);
                }
                else
                {
                    col.size = new Vector2(7f, 0.375f);
                    col.offset = Vector2.zero;
                }
            }

            // Dải hạt bụi tuyết phủ hết bề rộng mặt băng
            ParticleSystem dust = instance.GetComponentInChildren<ParticleSystem>();
            if (dust != null)
            {
                ParticleSystem.ShapeModule shape = dust.shape;
                shape.scale = new Vector3(7f, 0.1f, 0.1f);
            }
        }

        /// <summary>
        /// Trạm 6: ArrowBooster - Mũi tên đẩy khí giữa trời, phóng qua vực sâu và hồi ngay lượt nhảy đôi.
        /// </summary>
        private static void BuildStation6_ArrowBooster(Transform parent)
        {
            GameObject group = new GameObject("Station_6_ArrowBooster");
            group.transform.SetParent(parent);

            GameObject prefab = LoadPrefab("ArrowBooster");
            if (prefab == null) return;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
            instance.name = "ArrowBooster";
            // Treo lơ lửng giữa vực thẳm
            instance.transform.position = new Vector3(58.5f, 3.5f, 0f);

            Vector2 dir = new Vector2(1f, 0.75f).normalized;
            // Xoay mũi tên theo hướng đẩy
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            instance.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            ArrowBooster ab = instance.GetComponent<ArrowBooster>();
            if (ab != null)
            {
                SerializedObject so = new SerializedObject(ab);
                so.FindProperty("boostDirection").vector2Value = dir;
                so.FindProperty("boostForce").floatValue = 14f;
                so.FindProperty("reactivateDelay").floatValue = 2.5f;
                so.ApplyModifiedProperties();
            }
        }

        private static void CreateStationLabels(Transform parent)
        {
            GameObject labelsGroup = new GameObject("StationLabels");
            labelsGroup.transform.SetParent(parent);

            CreateLabel("Label_Start", "START: A/D Di Chuyen | Space Nhay | R Hoi Sinh", new Vector3(-2.5f, 2.5f, 0f), labelsGroup.transform);
            CreateLabel("Label_1", "[TRAM 1] Trampoline: Nhay xuong -> Bat 20f + Hoi Nhay Doi", new Vector3(4.5f, 2.5f, 0f), labelsGroup.transform);
            CreateLabel("Label_2", "[TRAM 2] FanTrap: Quat Y thoi bong | Quat X thoi ngang", new Vector3(15.5f, 3.8f, 0f), labelsGroup.transform);
            CreateLabel("Label_3", "[TRAM 3] FallingPlatform: Rung 0.4s -> Sup roi vinh vien", new Vector3(28.0f, 3.2f, 0f), labelsGroup.transform);
            CreateLabel("Label_4", "[TRAM 4] DeathPit: Giam len -> Vo nat thanh Ho Tu Than", new Vector3(38.2f, 3.2f, 0f), labelsGroup.transform);
            CreateLabel("Label_5", "[TRAM 5] IceRoad: San bang tron truot mat ma sat", new Vector3(48.0f, 2.8f, 0f), labelsGroup.transform);
            CreateLabel("Label_6", "[TRAM 6] ArrowBooster: Phong khi giua troi + Hoi Nhay Doi", new Vector3(58.5f, 5.2f, 0f), labelsGroup.transform);
            CreateLabel("Label_Win", "VICTORY! DA TEST HOAN TAT 6 CO QUAN MODULE 3", new Vector3(66.5f, 4.2f, 0f), labelsGroup.transform);
        }

        private static void CreateLabel(string name, string text, Vector3 position, Transform parent)
        {
            GameObject labelGo = new GameObject(name);
            labelGo.transform.SetParent(parent);
            labelGo.transform.position = position;

            TextMesh tm = labelGo.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 22;
            tm.characterSize = 0.08f;
            tm.color = new Color(0.95f, 0.95f, 1f, 0.9f);
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
        }

        #endregion

        #region Helpers

        private static GameObject LoadPrefab(string prefabName)
        {
            string pathTraps = $"{PrefabsTrapsDir}/{prefabName}.prefab";
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(pathTraps);
            if (go != null) return go;

            string pathRoot = $"{PrefabsRootDir}/{prefabName}.prefab";
            go = AssetDatabase.LoadAssetAtPath<GameObject>(pathRoot);
            if (go != null) return go;

            Debug.LogError($"[Module3DevSceneBuilder] Không tìm thấy Prefab '{prefabName}' tại cả {pathTraps} và {pathRoot}. Vui lòng chạy Tools -> One Hit Down -> Build Module 3 Assets trước!");
            return null;
        }

        private static Sprite LoadFirstSprite(string texturePath, string fallbackName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            foreach (var asset in assets)
            {
                if (asset is Sprite s && s.name == fallbackName)
                {
                    return s;
                }
            }

            foreach (var asset in assets)
            {
                if (asset is Sprite s)
                {
                    return s;
                }
            }

            Debug.LogWarning($"[Module3DevSceneBuilder] Không tải được sprite '{fallbackName}' từ: {texturePath}");
            return null;
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
