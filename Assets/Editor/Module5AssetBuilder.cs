// Hoàng Thị Giang Uyên (2351060497) — Module 5: Core Manager, Soul Tether, Audio & HUD
// Docs/SCHEMA_CHI_TIET.md §4 (bảng Prefab dòng 553) — Docs/Phan_cong.md Mục III (Người 5).
//
// Script Unity Editor: tự động đóng gói 2 Prefab của Module 5
//   1. Assets/Prefabs/SoulTether.prefab  — Transform + LineRenderer + SoulTether + child SparkVFX (spark_01.png)
//   2. Assets/Prefabs/AudioManager.prefab — 3 AudioSource + 6 BGM Chiptune + TOÀN BỘ 383 file SFX/Jingle
// Chạy từ Menu Editor: Tools -> One Hit Down -> Build Module 5 Assets
//
// Vì sao phải dùng công cụ Editor thay vì viết tay file .prefab: mỗi component trong Prefab YAML
// cần fileID duy nhất và m_Script cần GUID do Unity sinh khi import .cs lần đầu. Viết tay dễ làm
// hỏng asset, sai GUID và mất tham chiếu — đúng quy tắc CLAUDE.md Mục 6 (cấm sửa tay .meta/.prefab).
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using OneHitDown.Core;
using OneHitDown.Player;
using UnityEditor;
using UnityEngine;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Công cụ Editor tự động hóa việc đóng gói Prefab và gán tài nguyên âm thanh cho Module 5.
    /// </summary>
    public static class Module5AssetBuilder
    {
        #region Hằng số đường dẫn

        private const string PrefabsDir = "Assets/Prefabs";
        private const string AnimationsDir = "Assets/Art/Animations";
        private const string AudioRootDir = "Assets/Art/06_AmThanh";
        private const string BgmDir = "Assets/Art/06_AmThanh/BGM_ChiptuneLoops/Audio";
        private const string SparkTexturePath =
            "Assets/Art/07_HieuUng/Kenney_Particles/VisualEffects/PNG (Transparent)/spark_01.png";

        #endregion

        #region Menu Editor

        [MenuItem("Tools/One Hit Down/Build Module 5 Assets", false, 15)]
        public static void BuildAll()
        {
            try
            {
                EditorUtility.DisplayProgressBar("One Hit Down - Module 5", "Đang chuẩn bị thư mục...", 0.05f);
                Debug.Log("<b>[Module5AssetBuilder]</b> BẮT ĐẦU ĐÓNG GÓI PREFAB & GÁN ÂM THANH MODULE 5...");

                EnsureFolder(PrefabsDir);
                EnsureFolder(AnimationsDir);

                EditorUtility.DisplayProgressBar("One Hit Down - Module 5", "Đang tạo SoulTether.prefab...", 0.30f);
                BuildSoulTetherPrefab();

                EditorUtility.DisplayProgressBar("One Hit Down - Module 5", "Đang tạo AudioManager.prefab...", 0.70f);
                BuildAudioManagerPrefab();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                Debug.Log("<b>[Module5AssetBuilder]</b> HOÀN TẤT: SoulTether.prefab + AudioManager.prefab.");
                EditorUtility.DisplayDialog(
                    "Hoàn Tất Module 5",
                    "ĐÃ ĐÓNG GÓI XONG 2 PREFAB CỦA MODULE 5!\n\n" +
                    "1. Assets/Prefabs/SoulTether.prefab\n" +
                    "   - Transform + LineRenderer + SoulTether\n" +
                    "   - Child SparkVFX (ParticleSystem dùng spark_01.png)\n" +
                    "   - Tag Untagged, Layer Default (0), maxDistance = 4f (64px)\n\n" +
                    "2. Assets/Prefabs/AudioManager.prefab\n" +
                    "   - 3 AudioSource con: BGM / SFX / Jingle\n" +
                    "   - 6 BGM Chiptune: bgm_menu + bgm_level1..5\n" +
                    "   - Toàn bộ file .ogg trong Assets/Art/06_AmThanh/ đã vào mảng sfxLibrary\n\n" +
                    "Kéo 2 Prefab này vào Scene màn chơi là dùng được ngay.",
                    "Đồng Ý");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<b>[Module5AssetBuilder] LỖI:</b> {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("Lỗi Build Module 5", $"Có lỗi xảy ra: {ex.Message}", "Đóng");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #endregion

        #region 1. SoulTether.prefab

        /// <summary>
        /// Dựng SoulTether.prefab đúng bảng Prefab §4 dòng 553:
        /// Root có Transform + LineRenderer + SoulTether, child SparkVFX dùng spark_01.png.
        /// </summary>
        private static void BuildSoulTetherPrefab()
        {
            GameObject root = new GameObject("SoulTether");

            try
            {
                root.tag = "Untagged";
                root.layer = 0; // Default (0)

                // 1. LineRenderer — dây xích nối 2 đầu, đổi màu do SoulTether điều khiển.
                LineRenderer line = root.AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.startWidth = 0.08f;
                line.endWidth = 0.08f;
                line.numCapVertices = 4;
                line.numCornerVertices = 2;
                line.textureMode = LineTextureMode.Stretch;
                line.alignment = LineAlignment.View;
                line.sharedMaterial = LoadOrCreateSpriteMaterial("SoulTetherLine", null);
                line.startColor = Color.cyan;
                line.endColor = Color.cyan;
                line.sortingOrder = 5;

                // 2. Child SparkVFX — tia lửa bật khi dây căng cực đại (>= 60px).
                ParticleSystem spark = CreateSparkVfx(root.transform);

                // 3. SoulTether — gán toàn bộ field theo đúng Schema §3 Module 5.3.
                SoulTether tether = root.AddComponent<SoulTether>();
                SerializedObject so = new SerializedObject(tether);
                SetFloat(so, "maxDistance", 4f);          // 4 tiles = 64 pixels
                SetColor(so, "safeColor", Color.cyan);    // < 3.0f (48px)
                SetColor(so, "tensionColor", new Color(1f, 0.6f, 0.1f)); // 48–60px
                SetColor(so, "criticalColor", Color.red); // >= 3.75f (60px)
                SetObject(so, "tensionSparkVfx", spark);
                SetObject(so, "lineRenderer", line);
                SetFloat(so, "pullStiffness", 12f);
                SetFloat(so, "anchorResistance", 0.25f);
                SetFloat(so, "warningInterval", 0.5f);
                so.ApplyModifiedPropertiesWithoutUndo();

                string path = $"{PrefabsDir}/SoulTether.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"[Module5AssetBuilder] Đã đóng gói Prefab: {path}");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        /// <summary>Tạo child SparkVFX (ParticleSystem) dùng sprite spark_01.png, tắt sẵn chờ SoulTether gọi Play().</summary>
        private static ParticleSystem CreateSparkVfx(Transform parent)
        {
            GameObject child = new GameObject("SparkVFX");
            child.transform.SetParent(parent, false);
            child.transform.localPosition = Vector3.zero;

            ParticleSystem ps = child.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startColor = Color.white;
            main.gravityModifier = 0f;
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 30f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.85f, 0.3f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            Texture2D sparkTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SparkTexturePath);
            if (sparkTexture == null)
            {
                Debug.LogWarning($"[Module5AssetBuilder] Không tìm thấy spark_01.png tại: {SparkTexturePath}");
            }

            ParticleSystemRenderer psr = child.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            psr.sharedMaterial = LoadOrCreateSpriteMaterial("SparkVfx", sparkTexture);
            psr.sortingOrder = 6;

            return ps;
        }

        #endregion

        #region 2. AudioManager.prefab

        /// <summary>
        /// Dựng AudioManager.prefab: 3 AudioSource con (BGM/SFX/Jingle), gán 6 BGM Chiptune
        /// và nạp toàn bộ file .ogg của Assets/Art/06_AmThanh/ vào mảng sfxLibrary.
        /// </summary>
        private static void BuildAudioManagerPrefab()
        {
            GameObject root = new GameObject("AudioManager");

            try
            {
                root.tag = "Untagged";
                root.layer = 0;

                AudioSource bgmSource = CreateChildSource(root.transform, "BGM Source", true);
                AudioSource sfxSource = CreateChildSource(root.transform, "SFX Source", false);
                AudioSource jingleSource = CreateChildSource(root.transform, "Jingle Source", false);

                AudioManager manager = root.AddComponent<AudioManager>();
                SerializedObject so = new SerializedObject(manager);

                SetObject(so, "bgmSource", bgmSource);
                SetObject(so, "sfxSource", sfxSource);
                SetObject(so, "jingleSource", jingleSource);

                // 6 bản nhạc nền Chiptune — tên file đúng Schema §3 Module 5.4.
                SetObject(so, "bgmMenu", LoadClip($"{BgmDir}/bgm_menu.ogg"));
                SetObject(so, "bgmLevel1Ruins", LoadClip($"{BgmDir}/bgm_level1_ruins.ogg"));
                SetObject(so, "bgmLevel2Grotto", LoadClip($"{BgmDir}/bgm_level2_grotto.ogg"));
                SetObject(so, "bgmLevel3IceEmber", LoadClip($"{BgmDir}/bgm_level3_ice_ember.ogg"));
                SetObject(so, "bgmLevel4Idols", LoadClip($"{BgmDir}/bgm_level4_idols.ogg"));
                SetObject(so, "bgmLevel5ChaosNexus", LoadClip($"{BgmDir}/bgm_level5_chaos_nexus.ogg"));

                // Kho 382 file SFX/Jingle: quét toàn bộ AudioClip trong 06_AmThanh.
                AudioClip[] library = CollectAllAudioClips();
                SetArray(so, "sfxLibrary", library);

                so.ApplyModifiedPropertiesWithoutUndo();

                string path = $"{PrefabsDir}/AudioManager.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"[Module5AssetBuilder] Đã đóng gói Prefab: {path} — nạp {library.Length} file âm thanh vào sfxLibrary.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        /// <summary>Tạo GameObject con chứa một AudioSource đã cấu hình sẵn.</summary>
        private static AudioSource CreateChildSource(Transform parent, string name, bool loop)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);

            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f; // 2D — âm thanh toàn màn chơi
            source.volume = 1f;
            return source;
        }

        /// <summary>Quét toàn bộ AudioClip (.ogg) trong Assets/Art/06_AmThanh/ để nạp vào sfxLibrary.</summary>
        private static AudioClip[] CollectAllAudioClips()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { AudioRootDir });
            List<AudioClip> clips = new List<AudioClip>(guids.Length);

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);

                if (clip != null)
                {
                    clips.Add(clip);
                }
            }

            clips.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return clips.ToArray();
        }

        private static AudioClip LoadClip(string path)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

            if (clip == null)
            {
                Debug.LogWarning($"[Module5AssetBuilder] Không tìm thấy AudioClip: {path}");
            }

            return clip;
        }

        #endregion

        #region 3. Hàm phụ trợ

        /// <summary>Tạo Material sprite mới (nếu chưa có) để LineRenderer / ParticleSystem hiển thị đúng.</summary>
        private static Material LoadOrCreateSpriteMaterial(string name, Texture2D texture)
        {
            string path = $"{AnimationsDir}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(Shader.Find("Sprites/Default"));
                material.name = name;
                AssetDatabase.CreateAsset(material, path);
            }

            if (texture != null)
            {
                material.mainTexture = texture;
                EditorUtility.SetDirty(material);
            }

            return material;
        }

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

        private static void SetFloat(SerializedObject so, string propertyName, float value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);

            if (prop == null)
            {
                Debug.LogError($"[Module5AssetBuilder] Không tìm thấy property '{propertyName}'!");
                return;
            }

            prop.floatValue = value;
        }

        private static void SetColor(SerializedObject so, string propertyName, Color value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);

            if (prop == null)
            {
                Debug.LogError($"[Module5AssetBuilder] Không tìm thấy property '{propertyName}'!");
                return;
            }

            prop.colorValue = value;
        }

        private static void SetObject(SerializedObject so, string propertyName, Object value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);

            if (prop == null)
            {
                Debug.LogError($"[Module5AssetBuilder] Không tìm thấy property '{propertyName}'!");
                return;
            }

            prop.objectReferenceValue = value;
        }

        private static void SetArray(SerializedObject so, string propertyName, Object[] values)
        {
            SerializedProperty prop = so.FindProperty(propertyName);

            if (prop == null)
            {
                Debug.LogError($"[Module5AssetBuilder] Không tìm thấy property mảng '{propertyName}'!");
                return;
            }

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
