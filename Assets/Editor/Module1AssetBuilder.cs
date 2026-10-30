// Tạ Anh Tuấn (2351060494) — Module 1: Player Controller, Physics, 4 Skins & VFX
// Docs/SCHEMA_CHI_TIET.md §4 (bảng Prefab dòng 551-552) — Docs/Phan_cong.md Mục III (Người 1).
//
// Script Unity Editor: Tự động hóa đóng gói toàn bộ Prefabs của Module 1:
//   1. Cắt lại Sprite Sheets 96×96: Appearing (96x96).png và Desappearing (96x96).png (7 frames).
//   2. Tạo 2 Animation Clips: Appearing.anim và Disappearing.anim (20 FPS).
//   3. Tạo 2 Animator Controllers: Appearing_Controller và Disappearing_Controller.
//   4. Đóng gói 2 Prefabs VFX: Assets/Prefabs/AppearingVFX.prefab và Assets/Prefabs/DisappearingVFX.prefab.
//   5. Đóng gói 2 Prefabs Player: Assets/Prefabs/Player1.prefab và Assets/Prefabs/Player2.prefab
//      (đủ 4 skins, Rigidbody2D, BoxCollider2D, PlayerController, PlayerVFX, child GroundShadow & DustParticles).
// Chạy từ Menu Editor: Tools -> One Hit Down -> Build Module 1 Assets

#if UNITY_EDITOR
#pragma warning disable CS0618

using System;
using System.IO;
using OneHitDown.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Công cụ Editor tự động hóa việc đóng gói Prefab nhân vật và hiệu ứng hình ảnh cho Module 1.
    /// </summary>
    public static class Module1AssetBuilder
    {
        #region Hằng số đường dẫn

        private const string PrefabsDir = "Assets/Prefabs";
        private const string VfxAnimDir = "Assets/Art/Animations/VFX";
        private const string MainCharactersDir = "Assets/Art/01_NhanVat/PA1_MainCharacters";
        private const string EffectsDir = "Assets/Art/07_HieuUng/PA1_Other";

        private const string AppearingTexturePath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Appearing (96x96).png";
        private const string DisappearingTexturePath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Desappearing (96x96).png";
        private const string ShadowTexturePath = "Assets/Art/07_HieuUng/PA1_Other/Shadow.png";
        private const string DustTexturePath = "Assets/Art/07_HieuUng/PA1_Other/Dust Particle.png";

        private const string NinjaFrogAnimPath = "Assets/Art/Animations/NinjaFrog/NinjaFrog_Controller.controller";
        private const string VirtualGuyAnimPath = "Assets/Art/Animations/VirtualGuy/VirtualGuy_Controller.controller";
        private const string PinkManAnimPath = "Assets/Art/Animations/PinkMan/PinkMan_Controller.controller";
        private const string MaskDudeAnimPath = "Assets/Art/Animations/MaskDude/MaskDude_Controller.controller";

        private const string NinjaFrogIdlePath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Ninja Frog/Idle (32x32).png";
        private const string VirtualGuyIdlePath = "Assets/Art/01_NhanVat/PA1_MainCharacters/Virtual Guy/Idle (32x32).png";

        #endregion

        #region Menu Editor

        [MenuItem("Tools/One Hit Down/Build Module 1 Assets", false, 5)]
        public static void BuildAll()
        {
            try
            {
                EditorUtility.DisplayProgressBar("One Hit Down - Module 1", "Đang chuẩn bị thư mục...", 0.05f);
                Debug.Log("<b>[Module1AssetBuilder]</b> BẮT ĐẦU ĐÓNG GÓI PREFABS MODULE 1 (PLAYER & VFX)...");

                EnsureFolder(PrefabsDir);
                EnsureFolder(VfxAnimDir);

                // BƯỚC 1: Cắt lưới 7 frames 96x96 cho Appearing & Desappearing
                EditorUtility.DisplayProgressBar("One Hit Down - Module 1", "Đang cắt lại Sprite Sheets VFX 96x96...", 0.20f);
                SliceTextures();

                // BƯỚC 2: Tạo Animation Clips VFX
                EditorUtility.DisplayProgressBar("One Hit Down - Module 1", "Đang tạo Animation Clips...", 0.40f);
                CreateVfxAnimationClips(
                    out AnimationClip appearingClip,
                    out AnimationClip disappearingClip);

                // BƯỚC 3: Tạo Animator Controllers VFX
                EditorUtility.DisplayProgressBar("One Hit Down - Module 1", "Đang tạo Animator Controllers VFX...", 0.55f);
                CreateVfxAnimatorControllers(
                    appearingClip, disappearingClip,
                    out RuntimeAnimatorController appearingController,
                    out RuntimeAnimatorController disappearingController);

                // BƯỚC 4: Đóng gói 2 Prefabs VFX (AppearingVFX & DisappearingVFX)
                EditorUtility.DisplayProgressBar("One Hit Down - Module 1", "Đang đóng gói Prefabs VFX...", 0.70f);
                GameObject appearingVfxPrefab = BuildVfxPrefab("AppearingVFX", appearingController);
                GameObject disappearingVfxPrefab = BuildVfxPrefab("DisappearingVFX", disappearingController);

                // BƯỚC 5: Đóng gói 2 Prefabs Player (Player1 & Player2)
                EditorUtility.DisplayProgressBar("One Hit Down - Module 1", "Đang đóng gói Player1 & Player2 Prefabs...", 0.85f);
                BuildPlayerPrefabs(appearingVfxPrefab, disappearingVfxPrefab);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                Debug.Log("<b>[Module1AssetBuilder]</b> HOÀN TẤT ĐÓNG GÓI TOÀN BỘ PREFABS MODULE 1!");
                EditorUtility.DisplayDialog(
                    "Hoàn Tất Module 1",
                    "ĐÃ ĐÓNG GÓI THÀNH CÔNG 4 PREFABS CỦA MODULE 1!\n\n" +
                    "1. Assets/Prefabs/Player1.prefab\n" +
                    "   - Rigidbody2D + BoxCollider2D (0.85, 1.55) + PlayerController + PlayerVFX\n" +
                    "   - Cấu hình Player 1 (A/D/W/Space), default skin NinjaFrog, đủ mảng 4 skins\n" +
                    "   - Child GroundShadow (Shadow.png) + Child DustParticles (Dust Particle.png)\n\n" +
                    "2. Assets/Prefabs/Player2.prefab\n" +
                    "   - Cấu hình Player 2 (Arrows), default skin VirtualGuy, đủ mảng 4 skins\n" +
                    "   - Hỗ trợ chế độ Co-op 2P chung bàn phím\n\n" +
                    "3. Assets/Prefabs/AppearingVFX.prefab & DisappearingVFX.prefab\n" +
                    "   - Hoạt ảnh 7 frames 96x96 (20 FPS) cho hiệu ứng hồi sinh và chết tan biến\n\n" +
                    "GameManager đã có thể tham chiếu trực tiếp đến Player1 & Player2 Prefabs!",
                    "Đồng Ý");
            }
            catch (Exception ex)
            {
                Debug.LogError($"<b>[Module1AssetBuilder] LỖI:</b> {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("Lỗi Build Module 1", $"Có lỗi xảy ra: {ex.Message}", "Đóng");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void BuildFromCommandLine()
        {
            BuildAll();
        }

        #endregion

        #region 1. Slicing Textures

        private static void SliceTextures()
        {
            // Appearing (96x96).png: 672x96 px -> cắt 7 frames 96x96, pivot tâm (0.5, 0.5)
            SliceGrid(AppearingTexturePath, 96, 96, 7, 1, "Appearing (96x96)", new Vector2(0.5f, 0.5f));

            // Desappearing (96x96).png: 672x96 px -> cắt 7 frames 96x96, pivot tâm (0.5, 0.5)
            SliceGrid(DisappearingTexturePath, 96, 96, 7, 1, "Desappearing (96x96)", new Vector2(0.5f, 0.5f));

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void SliceGrid(string assetPath, int cellW, int cellH, int cols, int rows, string baseName, Vector2 pivot)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[Module1AssetBuilder] Không tìm thấy TextureImporter tại: {assetPath}");
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
            Debug.Log($"[Module1AssetBuilder] Đã cắt {total} frames chuẩn cho: {assetPath}");
        }

        #endregion

        #region 2. Animation Clips & Controllers

        private static void CreateVfxAnimationClips(
            out AnimationClip appearingClip,
            out AnimationClip disappearingClip)
        {
            // 1. Appearing.anim: 7 frames @ 20 FPS (duration 0.35s)
            Sprite[] appFrames = LoadIndexedSprites(AppearingTexturePath, "Appearing (96x96)", 7);
            appearingClip = BuildClip($"{VfxAnimDir}/Appearing.anim", appFrames, 20f, false);

            // 2. Disappearing.anim: 7 frames @ 20 FPS (duration 0.35s)
            Sprite[] disFrames = LoadIndexedSprites(DisappearingTexturePath, "Desappearing (96x96)", 7);
            disappearingClip = BuildClip($"{VfxAnimDir}/Disappearing.anim", disFrames, 20f, false);
        }

        private static AnimationClip BuildClip(string path, Sprite[] frames, float fps, bool loop)
        {
            if (frames == null || frames.Length == 0 || frames[0] == null)
            {
                Debug.LogError($"[Module1AssetBuilder] Không thể tạo clip {path}: mảng frames trống hoặc chứa null!");
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
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Length];
            float interval = 1f / fps;

            for (int i = 0; i < frames.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * interval,
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void CreateVfxAnimatorControllers(
            AnimationClip appearingClip,
            AnimationClip disappearingClip,
            out RuntimeAnimatorController appearingController,
            out RuntimeAnimatorController disappearingController)
        {
            appearingController = CreateSingleStateController($"{VfxAnimDir}/Appearing_Controller.controller", "Appearing", appearingClip);
            disappearingController = CreateSingleStateController($"{VfxAnimDir}/Disappearing_Controller.controller", "Disappearing", disappearingClip);
        }

        private static RuntimeAnimatorController CreateSingleStateController(string path, string stateName, AnimationClip clip)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AnimatorController ac = AnimatorController.CreateAnimatorControllerAtPath(path);
            AnimatorStateMachine sm = ac.layers[0].stateMachine;

            AnimatorState state = sm.AddState(stateName);
            state.motion = clip;
            sm.defaultState = state;

            EditorUtility.SetDirty(ac);
            return ac;
        }

        #endregion

        #region 3. Build VFX Prefabs

        private static GameObject BuildVfxPrefab(string prefabName, RuntimeAnimatorController controller)
        {
            GameObject go = new GameObject(prefabName);
            try
            {
                go.tag = "Untagged";
                go.layer = 0; // Default

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10; // Hiển thị đè lên nhân vật

                Animator anim = go.AddComponent<Animator>();
                anim.runtimeAnimatorController = controller;

                string path = $"{PrefabsDir}/{prefabName}.prefab";
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                Debug.Log($"[Module1AssetBuilder] Đã đóng gói Prefab: {path}");
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 4. Build Player Prefabs

        private static void BuildPlayerPrefabs(GameObject appearingVfx, GameObject disappearingVfx)
        {
            // Load 4 skin controllers
            RuntimeAnimatorController frogController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(NinjaFrogAnimPath);
            RuntimeAnimatorController virtualController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(VirtualGuyAnimPath);
            RuntimeAnimatorController pinkController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PinkManAnimPath);
            RuntimeAnimatorController maskController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(MaskDudeAnimPath);

            RuntimeAnimatorController[] allSkins = new[]
            {
                frogController,
                virtualController,
                pinkController,
                maskController
            };

            Sprite shadowSprite = LoadSprite(ShadowTexturePath, "Shadow_0");
            Sprite frogIdle = LoadSprite(NinjaFrogIdlePath, "Idle (32x32)_0");
            Sprite virtualIdle = LoadSprite(VirtualGuyIdlePath, "Idle (32x32)_0");

            Texture2D dustTex = AssetDatabase.LoadAssetAtPath<Texture2D>(DustTexturePath);
            Material dustMaterial = LoadOrCreateDustMaterial(dustTex);

            int playerLayer = ResolveLayer("Player", 9);
            int groundLayer = ResolveLayer("Ground", 8);

            // 1. Build Player1.prefab
            BuildSinglePlayerPrefab(
                "Player1",
                PlayerType.Player1,
                frogIdle,
                frogController,
                allSkins,
                shadowSprite,
                dustMaterial,
                appearingVfx,
                disappearingVfx,
                playerLayer,
                groundLayer
            );

            // 2. Build Player2.prefab
            BuildSinglePlayerPrefab(
                "Player2",
                PlayerType.Player2,
                virtualIdle,
                virtualController,
                allSkins,
                shadowSprite,
                dustMaterial,
                appearingVfx,
                disappearingVfx,
                playerLayer,
                groundLayer
            );
        }

        private static void BuildSinglePlayerPrefab(
            string prefabName,
            PlayerType playerType,
            Sprite defaultSprite,
            RuntimeAnimatorController defaultController,
            RuntimeAnimatorController[] allSkins,
            Sprite shadowSprite,
            Material dustMaterial,
            GameObject appearingVfx,
            GameObject disappearingVfx,
            int playerLayer,
            int groundLayer)
        {
            GameObject root = new GameObject(prefabName);
            try
            {
                root.tag = "Player";
                root.layer = playerLayer;

                // 1. SpriteRenderer
                SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
                sr.sprite = defaultSprite;
                sr.sortingOrder = 5;

                // 2. Animator
                Animator anim = root.AddComponent<Animator>();
                anim.runtimeAnimatorController = defaultController;

                // 3. Rigidbody2D — gravityScale 3.5, Continuous, FreezeRotation
                Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.mass = 1f;
                rb.linearDamping = 0f;
                rb.angularDamping = 0.05f;
                rb.gravityScale = 3.5f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.freezeRotation = true;

                // 4. BoxCollider2D — Size (0.85, 1.55), feet at y=0 -> offset y=0.775
                BoxCollider2D col = root.AddComponent<BoxCollider2D>();
                col.isTrigger = false;
                col.offset = new Vector2(0f, 0.775f);
                col.size = new Vector2(0.85f, 1.55f);

                // 5. Child GroundShadow (Shadow.png 16x16)
                GameObject shadowGo = new GameObject("GroundShadow");
                shadowGo.transform.SetParent(root.transform, false);
                shadowGo.transform.localPosition = Vector3.zero;
                shadowGo.layer = 0; // Default
                shadowGo.tag = "Untagged";
                SpriteRenderer shadowSr = shadowGo.AddComponent<SpriteRenderer>();
                shadowSr.sprite = shadowSprite;
                shadowSr.sortingOrder = 0; // Dưới chân nhân vật

                // 6. Child DustParticles (ParticleSystem)
                GameObject dustGo = new GameObject("DustParticles");
                dustGo.transform.SetParent(root.transform, false);
                dustGo.transform.localPosition = Vector3.zero;
                dustGo.layer = 0;
                dustGo.tag = "Untagged";
                ParticleSystem dustPs = ConfigureDustParticleSystem(dustGo, dustMaterial);

                // 7. PlayerController component
                PlayerController pc = root.AddComponent<PlayerController>();
                SerializedObject soPc = new SerializedObject(pc);
                SetEnum(soPc, "playerType", (int)playerType);
                SetArray(soPc, "skinControllers", allSkins);
                SetFloat(soPc, "moveSpeed", 8f);
                SetFloat(soPc, "groundAcceleration", 65f);
                SetFloat(soPc, "groundDeceleration", 80f);
                SetFloat(soPc, "airAcceleration", 45f);
                SetFloat(soPc, "iceFriction", 4f);
                SetFloat(soPc, "jumpForce", 15.5f);
                SetFloat(soPc, "doubleJumpForce", 13.5f);
                SetFloat(soPc, "defaultGravityScale", 3.5f);
                SetFloat(soPc, "coyoteTime", 0.1f);
                SetFloat(soPc, "jumpBufferTime", 0.1f);
                SetVector2(soPc, "groundCheckSize", new Vector2(0.55f, 0.12f));
                SetLayerMask(soPc, "groundLayer", 1 << groundLayer);
                SetObject(soPc, "rb", rb);
                SetObject(soPc, "col", col);
                SetObject(soPc, "spriteRenderer", sr);
                SetObject(soPc, "anim", anim);
                soPc.ApplyModifiedPropertiesWithoutUndo();

                // 8. PlayerVFX component
                PlayerVFX vfx = root.AddComponent<PlayerVFX>();
                SerializedObject soVfx = new SerializedObject(vfx);
                SetObject(soVfx, "groundShadow", shadowGo.transform);
                SetFloat(soVfx, "shadowMaxDistance", 3f);
                SetLayerMask(soVfx, "groundLayer", 1 << groundLayer);
                SetVector2(soVfx, "shadowScaleRange", new Vector2(0.6f, 1.2f));
                SetObject(soVfx, "appearingVfxPrefab", appearingVfx);
                SetObject(soVfx, "disappearingVfxPrefab", disappearingVfx);
                SetObject(soVfx, "dustParticles", dustPs);
                soVfx.ApplyModifiedPropertiesWithoutUndo();

                // 9. Save as Prefab
                string path = $"{PrefabsDir}/{prefabName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"[Module1AssetBuilder] Đã đóng gói Prefab: {path}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static ParticleSystem ConfigureDustParticleSystem(GameObject go, Material material)
        {
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.3f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startColor = new Color(1f, 1f, 1f, 0.85f);
            main.gravityModifier = 0.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 20;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 6, 10) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = fade;

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
                renderer.sortingOrder = 4;
            }

            return ps;
        }

        private static Material LoadOrCreateDustMaterial(Texture2D texture)
        {
            string matPath = $"{VfxAnimDir}/DustParticleMat.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                }
                mat = new Material(shader);
                mat.name = "DustParticleMat";
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (texture != null)
            {
                mat.mainTexture = texture;
                EditorUtility.SetDirty(mat);
            }

            return mat;
        }

        #endregion

        #region 5. Helpers & SerializedProperty Setters

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

        private static Sprite LoadSprite(string assetPath, string spriteName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (UnityEngine.Object obj in assets)
            {
                if (obj is Sprite sp && sp.name == spriteName)
                {
                    return sp;
                }
            }

            // Fallback: sprite đầu tiên
            foreach (UnityEngine.Object obj in assets)
            {
                if (obj is Sprite sp)
                {
                    return sp;
                }
            }

            Debug.LogWarning($"[Module1AssetBuilder] Không tìm thấy Sprite '{spriteName}' tại {assetPath}");
            return null;
        }

        private static Sprite[] LoadIndexedSprites(string assetPath, string baseName, int count)
        {
            Sprite[] result = new Sprite[count];
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            for (int i = 0; i < count; i++)
            {
                string targetName = $"{baseName}_{i}";
                foreach (UnityEngine.Object obj in assets)
                {
                    if (obj is Sprite sp && sp.name == targetName)
                    {
                        result[i] = sp;
                        break;
                    }
                }

                if (result[i] == null)
                {
                    Debug.LogWarning($"[Module1AssetBuilder] Thiếu Sprite '{targetName}' trong {assetPath}");
                }
            }

            return result;
        }

        private static int ResolveLayer(string layerName, int fallback)
        {
            int layer = LayerMask.NameToLayer(layerName);
            return layer >= 0 ? layer : fallback;
        }

        private static void SetFloat(SerializedObject so, string propertyName, float value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null) { prop.floatValue = value; }
            else { Debug.LogError($"[Module1AssetBuilder] Thiếu property float: {propertyName}"); }
        }

        private static void SetEnum(SerializedObject so, string propertyName, int enumIndex)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null) { prop.enumValueIndex = enumIndex; }
            else { Debug.LogError($"[Module1AssetBuilder] Thiếu property enum: {propertyName}"); }
        }

        private static void SetVector2(SerializedObject so, string propertyName, Vector2 value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null) { prop.vector2Value = value; }
            else { Debug.LogError($"[Module1AssetBuilder] Thiếu property vector2: {propertyName}"); }
        }

        private static void SetLayerMask(SerializedObject so, string propertyName, int maskValue)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null) { prop.intValue = maskValue; }
            else { Debug.LogError($"[Module1AssetBuilder] Thiếu property layerMask: {propertyName}"); }
        }

        private static void SetObject(SerializedObject so, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null) { prop.objectReferenceValue = value; }
            else { Debug.LogError($"[Module1AssetBuilder] Thiếu property object: {propertyName}"); }
        }

        private static void SetArray(SerializedObject so, string propertyName, UnityEngine.Object[] values)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogError($"[Module1AssetBuilder] Thiếu property array: {propertyName}");
                return;
            }

            prop.ClearArray();
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        #endregion
    }
}
#endif
