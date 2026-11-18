// Trần Văn Tuyên, phiên bản 1.0
// Script Unity Editor: Tự động hóa Bước 2 (cắt Sprite) và Bước 3 (Animation Clips, Animator Controllers, 6 Prefabs)
// theo đúng đặc tả của Docs/specs/module3-interactive-traps-spec.md §6 và Docs/SCHEMA_CHI_TIET.md §4.
// Chạy từ Menu Editor: Tools -> One Hit Down -> Build Module 3 Assets

#if UNITY_EDITOR
#pragma warning disable CS0618 // Bỏ qua cảnh báo TextureImporter.spritesheet trong Unity 6 (API này vẫn là chuẩn tương thích cao nhất)

using System;
using System.Collections.Generic;
using System.IO;
using OneHitDown.Traps;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Công cụ Editor tự động hóa Bước 2 & Bước 3:
    /// 1. Cắt lại 2 Texture: Falling Platforms/On (32x10).png (4 frames) và Arrow/Idle (18x18).png (10 frames).
    /// 2. Tạo 4 Animation Clips: Trampoline_Idle, Trampoline_Jump, Arrow_Idle, Arrow_Hit.
    /// 3. Tạo 2 Animator Controllers: TrampolineAnimator (alias Trampoline) và Arrow_Controller (alias ArrowBooster).
    /// 4. Đóng gói 6 Prefabs chuẩn PPU = 16: Trampoline, FanTrap, FallingPlatform, DeathPit, IceRoad, ArrowBooster
    ///    vào cả Assets/Prefabs/Traps/ (đúng spec §6.1) và Assets/Prefabs/ (đúng Schema §4).
    /// </summary>
    public static class Module3AssetBuilder
    {
        private const string TrapsArtPath = "Assets/Art/03_Bay/PA1_Traps";
        private const string AnimationsDir = "Assets/Art/Animations/Traps";
        private const string PrefabsTrapsDir = "Assets/Prefabs/Traps";
        private const string PrefabsRootDir = "Assets/Prefabs";

        [MenuItem("Tools/One Hit Down/Build Module 3 Assets", false, 10)]
        public static void BuildAll()
        {
            try
            {
                EditorUtility.DisplayProgressBar("One Hit Down - Module 3", "Đang chuẩn bị thư mục...", 0.05f);
                Debug.Log("<b>[Module3AssetBuilder]</b> BẮT ĐẦU QUY TRÌNH TỰ ĐỘNG HÓA MODULE 3 (BƯỚC 2 + BƯỚC 3)...");

                // BƯỚC 1: Đảm bảo cấu trúc thư mục
                EnsureFolder(AnimationsDir);
                EnsureFolder(PrefabsTrapsDir);

                // BƯỚC 2: Cắt lại 2 Sprite Sheet có dị thường (Falling Platforms & Arrow Idle)
                EditorUtility.DisplayProgressBar("One Hit Down - Module 3", "Đang cắt lại Sprite Sheets...", 0.15f);
                SliceTextures();

                // BƯỚC 3: Tạo 4 Animation Clips
                EditorUtility.DisplayProgressBar("One Hit Down - Module 3", "Đang tạo Animation Clips...", 0.40f);
                CreateAnimationClips(
                    out AnimationClip trampIdle,
                    out AnimationClip trampJump,
                    out AnimationClip arrowIdle,
                    out AnimationClip arrowHit
                );

                // BƯỚC 4: Tạo 2 Animator Controllers
                EditorUtility.DisplayProgressBar("One Hit Down - Module 3", "Đang tạo Animator Controllers...", 0.65f);
                CreateAnimatorControllers(trampIdle, trampJump, arrowIdle, arrowHit,
                    out RuntimeAnimatorController trampController,
                    out RuntimeAnimatorController arrowController);

                // BƯỚC 5: Đóng gói 6 Prefabs
                EditorUtility.DisplayProgressBar("One Hit Down - Module 3", "Đang đóng gói 6 Prefabs...", 0.85f);
                BuildAllPrefabs(trampController, arrowController);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                Debug.Log("<b>[Module3AssetBuilder]</b> HOÀN TẤT 100% BƯỚC 2 VÀ BƯỚC 3!");
                EditorUtility.DisplayDialog(
                    "Hoàn Tất Module 3",
                    "ĐÃ HOÀN TẤT 100% BƯỚC 2 & BƯỚC 3!\n\n" +
                    "1. Cắt lại Sprite Sheet:\n" +
                    "   - Falling Platforms/On (32x10).png: 4 frames 32x10 (On (32x10)_0..3)\n" +
                    "   - Arrow/Idle (18x18).png: 10 frames 18x18 (Idle (18x18)_0..9)\n\n" +
                    "2. Tạo 4 Animation Clips (Assets/Art/Animations/Traps/):\n" +
                    "   - Trampoline_Idle.anim (1 frame, loop, 12 FPS)\n" +
                    "   - Trampoline_Jump.anim (8 frames, non-loop, 20 FPS)\n" +
                    "   - Arrow_Idle.anim (10 frames, loop, 12 FPS)\n" +
                    "   - Arrow_Hit.anim (4 frames, non-loop, 20 FPS, 0.2s duration)\n\n" +
                    "3. Tạo Animator Controllers:\n" +
                    "   - TrampolineAnimator.controller (+ alias Trampoline.controller)\n" +
                    "   - Arrow_Controller.controller (+ alias ArrowBooster.controller)\n\n" +
                    "4. Đóng gói 6 Prefabs chuẩn PPU=16:\n" +
                    "   - Trampoline.prefab\n" +
                    "   - FanTrap.prefab\n" +
                    "   - FallingPlatform.prefab\n" +
                    "   - DeathPit.prefab (2 tầng: Root Ground 8 + Child Hazard 10)\n" +
                    "   - IceRoad.prefab (Solid Ground 8 + Trigger IceSurface)\n" +
                    "   - ArrowBooster.prefab\n\n" +
                    "Prefabs được lưu tại cả Assets/Prefabs/Traps/ và Assets/Prefabs/.",
                    "Đồng Ý"
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"<b>[Module3AssetBuilder] LỖI:</b> {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("Lỗi Build Module 3", $"Có lỗi xảy ra: {ex.Message}", "Đóng");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>Điểm gọi dành cho Batchmode CLI nếu cần tự động hóa hoàn toàn.</summary>
        public static void BuildFromCommandLine()
        {
            BuildAll();
        }

        #region 1. Folder Management

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        #endregion

        #region 2. Slice Textures

        private static void SliceTextures()
        {
            // 1. Falling Platforms/On (32x10).png: kích thước 128x10 px -> cắt lưới 4 ô 32x10, pivot tâm (0.5, 0.5)
            string fallingPath = $"{TrapsArtPath}/Falling Platforms/On (32x10).png";
            SliceGrid(fallingPath, 32, 10, 4, 1, "On (32x10)", new Vector2(0.5f, 0.5f));

            // 2. Arrow/Idle (18x18).png: kích thước 180x18 px -> cắt lưới 10 ô 18x18, pivot tâm (0.5, 0.5)
            string arrowIdlePath = $"{TrapsArtPath}/Arrow/Idle (18x18).png";
            SliceGrid(arrowIdlePath, 18, 18, 10, 1, "Idle (18x18)", new Vector2(0.5f, 0.5f));

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void SliceGrid(string assetPath, int cellW, int cellH, int cols, int rows, string baseName, Vector2 pivot)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[Module3AssetBuilder] Không tìm thấy TextureImporter tại: {assetPath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            int total = cols * rows;
            SpriteMetaData[] metas = new SpriteMetaData[total];
            int idx = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // Tọa độ Unity Texture gốc ở góc dưới bên trái (bottom-left)
                    int x = c * cellW;
                    int y = (rows - 1 - r) * cellH;

                    metas[idx] = new SpriteMetaData
                    {
                        name = $"{baseName}_{idx}",
                        rect = new Rect(x, y, cellW, cellH),
                        alignment = 0, // Center
                        pivot = pivot
                    };
                    idx++;
                }
            }

            importer.spritesheet = metas;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            Debug.Log($"[Module3AssetBuilder] Đã cắt {total} frames chuẩn cho: {assetPath}");
        }

        #endregion

        #region 3. Animation Clips

        private static void CreateAnimationClips(
            out AnimationClip trampIdle,
            out AnimationClip trampJump,
            out AnimationClip arrowIdle,
            out AnimationClip arrowHit)
        {
            // 1. Trampoline_Idle: 1 frame Idle.png, loop, 12 FPS
            Sprite sTrampIdle = LoadSprite($"{TrapsArtPath}/Trampoline/Idle.png", "Idle_0");
            trampIdle = BuildClip(
                $"{AnimationsDir}/Trampoline_Idle.anim",
                new[] { sTrampIdle },
                fps: 12f,
                loop: true
            );

            // 2. Trampoline_Jump: 8 frames Jump (28x28).png, non-loop, 20 FPS (spec §6.2)
            Sprite[] sTrampJump = LoadIndexedSprites($"{TrapsArtPath}/Trampoline/Jump (28x28).png", "Jump (28x28)", 8);
            trampJump = BuildClip(
                $"{AnimationsDir}/Trampoline_Jump.anim",
                sTrampJump,
                fps: 20f,
                loop: false
            );

            // 3. Arrow_Idle: 10 frames Idle (18x18).png, loop, 12 FPS (người dùng chọn 12 FPS)
            Sprite[] sArrowIdle = LoadIndexedSprites($"{TrapsArtPath}/Arrow/Idle (18x18).png", "Idle (18x18)", 10);
            arrowIdle = BuildClip(
                $"{AnimationsDir}/Arrow_Idle.anim",
                sArrowIdle,
                fps: 12f,
                loop: true
            );

            // 4. Arrow_Hit: 4 frames Hit (18x18).png, non-loop, 20 FPS (4 frames @ 20 FPS = 0.20s khớp HitAnimationDuration = 0.2f)
            Sprite[] sArrowHit = LoadIndexedSprites($"{TrapsArtPath}/Arrow/Hit (18x18).png", "Hit (18x18)", 4);
            arrowHit = BuildClip(
                $"{AnimationsDir}/Arrow_Hit.anim",
                sArrowHit,
                fps: 20f,
                loop: false
            );
        }

        private static AnimationClip BuildClip(string path, Sprite[] frames, float fps, bool loop)
        {
            if (frames == null || frames.Length == 0 || frames[0] == null)
            {
                Debug.LogError($"[Module3AssetBuilder] Không thể tạo clip {path}: mảng frames trống hoặc chứa null!");
                return null;
            }

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }
            else
            {
                clip.ClearCurves();
            }

            clip.frameRate = fps;

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            // Clip 1 frame có loop: tạo 2 keyframe ở 0s và 1/fps để duration > 0, tránh cảnh báo Unity
            ObjectReferenceKeyframe[] keyframes;
            if (frames.Length == 1)
            {
                keyframes = new ObjectReferenceKeyframe[2];
                keyframes[0] = new ObjectReferenceKeyframe { time = 0f, value = frames[0] };
                keyframes[1] = new ObjectReferenceKeyframe { time = 1f / fps, value = frames[0] };
            }
            else
            {
                keyframes = new ObjectReferenceKeyframe[frames.Length];
                for (int i = 0; i < frames.Length; i++)
                {
                    keyframes[i] = new ObjectReferenceKeyframe
                    {
                        time = (float)i / fps,
                        value = frames[i]
                    };
                }
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
            Debug.Log($"[Module3AssetBuilder] Đã tạo clip: {path} ({frames.Length} frames, {fps} FPS, loop={loop})");
            return clip;
        }

        #endregion

        #region 4. Animator Controllers

        private static void CreateAnimatorControllers(
            AnimationClip trampIdle,
            AnimationClip trampJump,
            AnimationClip arrowIdle,
            AnimationClip arrowHit,
            out RuntimeAnimatorController trampController,
            out RuntimeAnimatorController arrowController)
        {
            // 1. TrampolineAnimator.controller (spec §6.2)
            string trampPath = $"{AnimationsDir}/TrampolineAnimator.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(trampPath) != null)
            {
                AssetDatabase.DeleteAsset(trampPath);
            }
            AnimatorController tc = AnimatorController.CreateAnimatorControllerAtPath(trampPath);
            AnimatorStateMachine tSm = tc.layers[0].stateMachine;

            AnimatorState tIdle = tSm.AddState("Idle");
            tIdle.motion = trampIdle;
            tSm.defaultState = tIdle;

            AnimatorState tJump = tSm.AddState("Jump");
            tJump.motion = trampJump;

            // Transition: Jump -> Idle khi hết clip Jump (hasExitTime = true)
            AnimatorStateTransition tTrans = tJump.AddTransition(tIdle);
            tTrans.hasExitTime = true;
            tTrans.exitTime = 1f;
            tTrans.hasFixedDuration = true;
            tTrans.duration = 0f;

            EditorUtility.SetDirty(tc);
            trampController = tc;

            // Tạo thêm alias Trampoline.controller
            string trampAliasPath = $"{AnimationsDir}/Trampoline.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(trampAliasPath) != null)
            {
                AssetDatabase.DeleteAsset(trampAliasPath);
            }
            AssetDatabase.CopyAsset(trampPath, trampAliasPath);
            Debug.Log($"[Module3AssetBuilder] Đã tạo Animator Controller: {trampPath} (+ alias {trampAliasPath})");

            // 2. Arrow_Controller.controller (spec §6.2)
            string arrowPath = $"{AnimationsDir}/Arrow_Controller.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(arrowPath) != null)
            {
                AssetDatabase.DeleteAsset(arrowPath);
            }
            AnimatorController ac = AnimatorController.CreateAnimatorControllerAtPath(arrowPath);
            AnimatorStateMachine aSm = ac.layers[0].stateMachine;

            AnimatorState aIdle = aSm.AddState("Idle");
            aIdle.motion = arrowIdle;
            aSm.defaultState = aIdle;

            AnimatorState aHit = aSm.AddState("Hit");
            aHit.motion = arrowHit;

            // Transition: Hit -> Idle (dù script điều khiển Play("Idle") sau hồi chiêu, giữ transition exitTime an toàn)
            AnimatorStateTransition aTrans = aHit.AddTransition(aIdle);
            aTrans.hasExitTime = true;
            aTrans.exitTime = 1f;
            aTrans.hasFixedDuration = true;
            aTrans.duration = 0f;

            EditorUtility.SetDirty(ac);
            arrowController = ac;

            // Tạo thêm alias ArrowBooster.controller
            string arrowAliasPath = $"{AnimationsDir}/ArrowBooster.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(arrowAliasPath) != null)
            {
                AssetDatabase.DeleteAsset(arrowAliasPath);
            }
            AssetDatabase.CopyAsset(arrowPath, arrowAliasPath);
            Debug.Log($"[Module3AssetBuilder] Đã tạo Animator Controller: {arrowPath} (+ alias {arrowAliasPath})");
        }

        #endregion

        #region 5. Prefabs Assembly

        private static void BuildAllPrefabs(RuntimeAnimatorController trampController, RuntimeAnimatorController arrowController)
        {
            int layerTraps = ResolveLayer("Traps", 10);
            int layerGround = ResolveLayer("Ground", 8);

            BuildTrampoline(layerTraps, trampController);
            BuildFanTrap(layerTraps);
            BuildFallingPlatform(layerGround);
            BuildDeathPit(layerGround, layerTraps);
            BuildIceRoad(layerGround);
            BuildArrowBooster(layerTraps, arrowController);
        }

        // Prefab 1: Trampoline.prefab (Schema §4 dòng 560, spec §6.2)
        private static void BuildTrampoline(int layerTraps, RuntimeAnimatorController controller)
        {
            GameObject go = new GameObject("Trampoline");
            try
            {
                go.tag = "Untagged";
                go.layer = layerTraps;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = LoadSprite($"{TrapsArtPath}/Trampoline/Idle.png", "Idle_0");

                Animator anim = go.AddComponent<Animator>();
                anim.runtimeAnimatorController = controller;

                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.6f, 0.5f);
                col.offset = new Vector2(0f, 0.25f);

                TrampolineTrap trap = go.AddComponent<TrampolineTrap>();
                SerializedObject so = new SerializedObject(trap);
                SetProperty(so, "bounceForce", 20f);
                SetProperty(so, "bounceDirection", Vector2.up);
                SetProperty(so, "animator", anim);
                so.ApplyModifiedPropertiesWithoutUndo();

                SavePrefab(go, "Trampoline");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // Prefab 2: FanTrap.prefab (Schema §4 dòng 561, spec §6.2)
        private static void BuildFanTrap(int layerTraps)
        {
            GameObject go = new GameObject("FanTrap");
            try
            {
                go.tag = "Untagged";
                // Thân quạt là bệ đứng được: layer Ground để CheckGround() của PlayerController nhận diện và cho nhảy.
                // Luồng gió ở child WindZone vẫn giữ layer Traps.
                go.layer = ResolveLayer("Ground", 8);

                // Rigidbody2D Kinematic ở gốc: để trigger của child WindZone được chuyển lên script FanTrap.
                Rigidbody2D fanBody = go.AddComponent<Rigidbody2D>();
                fanBody.bodyType = RigidbodyType2D.Kinematic;
                fanBody.useFullKinematicContacts = true;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = LoadSprite($"{TrapsArtPath}/Fan/On (24x8).png", "On (24x8)_0");

                // Solid body collider (24x8 px / 16 PPU = 1.5 x 0.5)
                BoxCollider2D solidCol = go.AddComponent<BoxCollider2D>();
                solidCol.isTrigger = false;
                solidCol.size = new Vector2(1.5f, 0.5f);
                solidCol.offset = Vector2.zero;

                // Child WindZone (mang BoxCollider2D trigger)
                GameObject childWind = new GameObject("WindZone");
                childWind.transform.SetParent(go.transform, false);
                childWind.tag = "Untagged";
                childWind.layer = layerTraps;

                BoxCollider2D windCol = childWind.AddComponent<BoxCollider2D>();
                windCol.isTrigger = true;
                windCol.size = new Vector2(1.5f, 4.0f);
                windCol.offset = new Vector2(0f, 2.25f); // Đặt luồng gió thổi cao 4 tiles phía trên bệ

                FanTrap fan = go.AddComponent<FanTrap>();
                Sprite[] frames = LoadIndexedSprites($"{TrapsArtPath}/Fan/On (24x8).png", "On (24x8)", 4);
                Sprite offSprite = LoadSprite($"{TrapsArtPath}/Fan/Off.png", "Off_0");

                SerializedObject so = new SerializedObject(fan);
                SetProperty(so, "windDirection", Vector2.up);
                SetProperty(so, "windForce", 15f);
                SetProperty(so, "isActive", true);
                SetProperty(so, "offSprite", offSprite);
                SetProperty(so, "spriteRenderer", sr);
                SetSpriteArray(so, "animationFrames", frames);
                so.ApplyModifiedPropertiesWithoutUndo();

                SavePrefab(go, "FanTrap");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // Prefab 3: FallingPlatform.prefab (Schema §4 dòng 562, spec §6.2)
        private static void BuildFallingPlatform(int layerGround)
        {
            GameObject go = new GameObject("FallingPlatform");
            try
            {
                go.tag = "Untagged";
                go.layer = layerGround; // Ground (8) để PlayerController nhận diện CheckGround

                Sprite activeSprite = LoadSprite($"{TrapsArtPath}/Falling Platforms/On (32x10).png", "On (32x10)_0");
                Sprite offSprite = LoadSprite($"{TrapsArtPath}/Falling Platforms/Off.png", "Off_0");

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = activeSprite;

                // Solid collider (32x10 px / 16 PPU = 2.0 x 0.625)
                BoxCollider2D solidCol = go.AddComponent<BoxCollider2D>();
                solidCol.isTrigger = false;
                solidCol.size = new Vector2(2.0f, 0.625f);
                solidCol.offset = Vector2.zero;

                FallingPlatform platform = go.AddComponent<FallingPlatform>();
                SerializedObject so = new SerializedObject(platform);
                SetProperty(so, "shakeDuration", 0.4f);
                SetProperty(so, "isPermanentInRun", true);
                SetProperty(so, "respawnDuration", 3.0f);
                SetProperty(so, "fallSpeed", 12f);
                SetProperty(so, "activeSprite", activeSprite);
                SetProperty(so, "offSprite", offSprite);
                SetProperty(so, "solidCollider", solidCol);
                SetProperty(so, "spriteRenderer", sr);
                so.ApplyModifiedPropertiesWithoutUndo();

                SavePrefab(go, "FallingPlatform");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // Prefab 4: DeathPit.prefab (Hierarchy 2 tầng theo Schema §3.4 & §4 dòng 563)
        private static void BuildDeathPit(int layerGround, int layerTraps)
        {
            GameObject go = new GameObject("DeathPit");
            try
            {
                go.tag = "Untagged";
                go.layer = layerGround; // Ngụy trang mặt đất

                Sprite idleSprite = LoadSprite($"{TrapsArtPath}/Blocks/Idle.png", "Idle_0");
                Sprite crackSprite = LoadSprite($"{TrapsArtPath}/Blocks/HitTop (22x22).png", "HitTop (22x22)_0");
                Sprite part1Sprite = LoadSprite($"{TrapsArtPath}/Blocks/Part 1 (22x22).png", "Part 1 (22x22)_0");
                Sprite part2Sprite = LoadSprite($"{TrapsArtPath}/Blocks/Part 2 (22x22).png", "Part 2 (22x22)_0");

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = idleSprite;

                // Solid collider sàn lúc chưa vỡ (22x22 px / 16 PPU = 1.375 x 1.375)
                BoxCollider2D solidCol = go.AddComponent<BoxCollider2D>();
                solidCol.isTrigger = false;
                solidCol.size = new Vector2(1.375f, 1.375f);
                solidCol.offset = Vector2.zero;

                // Child DeathTriggerZone: mang Tag Hazard và Layer Traps
                GameObject childHazard = new GameObject("DeathTriggerZone");
                childHazard.transform.SetParent(go.transform, false);
                childHazard.tag = "Hazard";
                childHazard.layer = layerTraps;

                BoxCollider2D hazardCol = childHazard.AddComponent<BoxCollider2D>();
                hazardCol.isTrigger = true;
                hazardCol.size = new Vector2(1.375f, 1.0f);
                hazardCol.offset = new Vector2(0f, -0.8f); // Đặt thấp hơn mặt sàn 0.8f (Schema §3.4)

                childHazard.SetActive(false); // Ban đầu ở trạng thái tắt theo spec §6.2

                DeathPitTrap trap = go.AddComponent<DeathPitTrap>();
                SerializedObject so = new SerializedObject(trap);
                SetProperty(so, "shakeDuration", 0.2f);
                SetProperty(so, "solidCollider", solidCol);
                SetProperty(so, "deathTrigger", hazardCol);
                SetProperty(so, "blockRenderer", sr);
                SetProperty(so, "idleSprite", idleSprite);
                SetProperty(so, "crackSprite", crackSprite);
                SetProperty(so, "part1Sprite", part1Sprite);
                SetProperty(so, "part2Sprite", part2Sprite);
                so.ApplyModifiedPropertiesWithoutUndo();

                SavePrefab(go, "DeathPit");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // Prefab 5: IceRoad.prefab (Schema §4 dòng 564, spec §6.2)
        private static void BuildIceRoad(int layerGround)
        {
            GameObject go = new GameObject("IceRoad");
            try
            {
                go.tag = "Untagged";
                go.layer = layerGround; // Ground (8)

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                // Frame 2 là Băng xanh (Sand Mud Ice (16x6)_2) theo spec §6.2 & §9.1
                Sprite iceSprite = LoadSprite($"{TrapsArtPath}/Sand Mud Ice/Sand Mud Ice (16x6).png", "Sand Mud Ice (16x6)_2");
                sr.sprite = iceSprite;

                // 1. Solid collider đỡ nhân vật (49x80 px / 16 PPU ≈ 3.0625 x 5.0)
                BoxCollider2D solidCol = go.AddComponent<BoxCollider2D>();
                solidCol.isTrigger = false;
                solidCol.size = new Vector2(3.0625f, 5.0f);
                solidCol.offset = Vector2.zero;

                // 2. Trigger collider hơi nhô cao lên trên mặt băng (+0.1f) để đón nhân vật vào trạng thái trượt ngay khi chạm chân
                BoxCollider2D triggerCol = go.AddComponent<BoxCollider2D>();
                triggerCol.isTrigger = true;
                triggerCol.size = new Vector2(3.0625f, 5.2f);
                triggerCol.offset = new Vector2(0f, 0.1f);

                IceSurface ice = go.AddComponent<IceSurface>();

                // Hạt bụi tuyết (Ice Particle.png) bay lên từ mặt băng khi nhân vật trượt
                ParticleSystem dust = CreateIceDustParticle(go.transform);
                SerializedObject iceSo = new SerializedObject(ice);
                SetProperty(iceSo, "iceParticle", dust);
                iceSo.ApplyModifiedPropertiesWithoutUndo();

                SavePrefab(go, "IceRoad");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // Prefab 6: ArrowBooster.prefab (Schema §4 dòng 565, spec §6.2)
        private static void BuildArrowBooster(int layerTraps, RuntimeAnimatorController controller)
        {
            GameObject go = new GameObject("ArrowBooster");
            try
            {
                go.tag = "Untagged";
                go.layer = layerTraps;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = LoadSprite($"{TrapsArtPath}/Arrow/Idle (18x18).png", "Idle (18x18)_0");

                Animator anim = go.AddComponent<Animator>();
                anim.runtimeAnimatorController = controller;

                CircleCollider2D col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.6f;
                col.offset = Vector2.zero;

                ArrowBooster booster = go.AddComponent<ArrowBooster>();
                SerializedObject so = new SerializedObject(booster);
                SetProperty(so, "boostForce", 14f);
                SetProperty(so, "boostDirection", Vector2.up);
                SetProperty(so, "reactivateDelay", 2.5f);
                SetProperty(so, "animator", anim);
                SetProperty(so, "boosterCollider", col);
                SetProperty(so, "spriteRenderer", sr);
                so.ApplyModifiedPropertiesWithoutUndo();

                SavePrefab(go, "ArrowBooster");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Tạo child "IceDust" (ParticleSystem) dùng sprite Ice Particle.png; Stop khi khởi tạo,
        /// IceSurface sẽ Play/Stop theo sự có mặt của nhân vật.
        /// </summary>
        private static ParticleSystem CreateIceDustParticle(Transform parent)
        {
            string texPath = $"{TrapsArtPath}/Sand Mud Ice/Ice Particle.png";
            string matPath = $"{AnimationsDir}/IceDust.mat";

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Sprites/Default"));
                mat.name = "IceDust";
                AssetDatabase.CreateAsset(mat, matPath);
            }

            mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            EditorUtility.SetDirty(mat);

            GameObject child = new GameObject("IceDust");
            child.transform.SetParent(parent, false);
            child.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            ParticleSystem ps = child.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startColor = new Color(0.85f, 0.95f, 1f, 0.9f);
            main.gravityModifier = -0.1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 30f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(3f, 0.1f, 0.1f);

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = fade;

            ParticleSystemRenderer psr = child.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = mat;
            psr.sortingOrder = 11;

            return ps;
        }

        private static void SavePrefab(GameObject go, string name)
        {
            string pathTraps = $"{PrefabsTrapsDir}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, pathTraps);
            Debug.Log($"[Module3AssetBuilder] Đã đóng gói Prefab chuẩn Module 3: {pathTraps}");
        }

        #endregion

        #region Helpers

        private static Sprite LoadSprite(string texturePath, string spriteName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite s && s.name == spriteName)
                {
                    return s;
                }
            }

            // Fallback: tìm sprite đầu tiên nếu tên chưa khớp
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite s)
                {
                    Debug.LogWarning($"[Module3AssetBuilder] Không tìm thấy '{spriteName}', dùng fallback: '{s.name}' tại {texturePath}");
                    return s;
                }
            }

            Debug.LogError($"[Module3AssetBuilder] Không load được Sprite nào từ: {texturePath}");
            return null;
        }

        private static Sprite[] LoadIndexedSprites(string texturePath, string baseName, int count)
        {
            Sprite[] result = new Sprite[count];
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            Dictionary<string, Sprite> map = new Dictionary<string, Sprite>();

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite s)
                {
                    map[s.name] = s;
                }
            }

            for (int i = 0; i < count; i++)
            {
                string target = $"{baseName}_{i}";
                if (map.TryGetValue(target, out Sprite sp))
                {
                    result[i] = sp;
                }
                else
                {
                    Debug.LogWarning($"[Module3AssetBuilder] Thiếu sprite '{target}' trong {texturePath}");
                }
            }

            return result;
        }

        private static int ResolveLayer(string layerName, int fallbackIndex)
        {
            int idx = LayerMask.NameToLayer(layerName);
            return idx >= 0 ? idx : fallbackIndex;
        }

        private static void SetProperty(SerializedObject so, string propertyName, object val)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogError($"[Module3AssetBuilder] Không tìm thấy property '{propertyName}' trên {so.targetObject.GetType().Name}!");
                return;
            }

            switch (val)
            {
                case float f:
                    prop.floatValue = f;
                    break;
                case bool b:
                    prop.boolValue = b;
                    break;
                case int i:
                    prop.intValue = i;
                    break;
                case Vector2 v2:
                    prop.vector2Value = v2;
                    break;
                case Vector3 v3:
                    prop.vector3Value = v3;
                    break;
                case UnityEngine.Object obj:
                    prop.objectReferenceValue = obj;
                    break;
                default:
                    Debug.LogWarning($"[Module3AssetBuilder] Kiểu dữ liệu chưa hỗ trợ cho property '{propertyName}': {val?.GetType().Name}");
                    break;
            }
        }

        private static void SetSpriteArray(SerializedObject so, string propertyName, Sprite[] sprites)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogError($"[Module3AssetBuilder] Không tìm thấy property mảng '{propertyName}' trên {so.targetObject.GetType().Name}!");
                return;
            }

            prop.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }

        #endregion
    }
}

#endif
