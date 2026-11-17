// Module 4: Items, Moving Platform, Level Start & Exit Gate.
// Docs/SCHEMA_CHI_TIET.md §4 (bảng Prefab dòng 566-569) — Docs/Phan_cong.md (Người 4).
//
// Script Unity Editor: Tự động đóng gói 11 Prefabs của Module 4:
//   1. Fruit_Apple, Fruit_Bananas, Fruit_Kiwi, Fruit_Orange, Fruit_Pineapple,
//      Fruit_Cherries, Fruit_Melon, Fruit_Strawberry (CircleCollider2D, CollectibleItem; tag Item).
//   2. MovingPlatform (SpriteRenderer Grey On, BoxCollider2D 2.0 x 0.5, MovingPlatform; layer Ground).
//   3. LevelStart (SpriteRenderer Start (Moving), Animator, LevelStartPoint với P1SpawnPoint & P2SpawnPoint).
//   4. ExitGate (SpriteRenderer End, BoxCollider2D, ExitGate; con StatusText ở trên cổng & Confetti ở giữa).
// Chạy từ Menu Editor: Tools -> One Hit Down -> Build Module 4 Assets

#if UNITY_EDITOR
#pragma warning disable CS0618

using System;
using OneHitDown.Items;
using OneHitDown.Level;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Công cụ Editor tự động hóa việc đóng gói Prefab cho Module 4 (vật phẩm, bệ di chuyển, cổng vào/ra).
    /// </summary>
    public static class Module4AssetBuilder
    {
        #region Hằng số đường dẫn

        private const string PrefabsDir = "Assets/Prefabs";
        private const string FruitsDir = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Fruits";
        private const string StartDir = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Checkpoints/Start";
        private const string EndDir = "Assets/Art/04_VatPham_dungOrkhong/PA1_Items/Checkpoints/End";
        private const string PlatformDir = "Assets/Art/03_Bay/PA1_Traps/Platforms";

        private const string StartSpritePath = StartDir + "/Start (Moving) (64x64).png";
        private const string EndSpritePath = EndDir + "/End (Idle).png";
        private const string PlatformSpritePath = PlatformDir + "/Grey On (32x8).png";
        private const string ConfettiSpritePath = "Assets/Art/07_HieuUng/PA1_Other/Confetti (16x16).png";
        private const string ParticleMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat";

        private const int DefaultLayer = 0;
        private const int GroundLayer = 8;

        #endregion

        #region Menu Editor

        [MenuItem("Tools/One Hit Down/Build Module 4 Assets", false, 6)]
        public static void BuildAll()
        {
            try
            {
                BuildFruitPrefabs();
                BuildMovingPlatformPrefab();
                BuildLevelStartPrefab();
                BuildExitGatePrefab();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("<b>[Module4AssetBuilder]</b> Hoàn tất đóng gói 11 Prefabs Module 4.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Module4AssetBuilder] Lỗi: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Điểm vào cho batch mode: Unity.exe -batchmode -executeMethod OneHitDown.EditorTools.Module4AssetBuilder.BuildFromCommandLine
        /// </summary>
        public static void BuildFromCommandLine()
        {
            try
            {
                BuildAll();
                EditorApplication.Exit(0);
            }
            catch (Exception)
            {
                EditorApplication.Exit(1);
            }
        }

        #endregion

        #region Bệ di chuyển

        /// <summary>
        /// Tạo MovingPlatform.prefab: BoxCollider2D 2.0 x 0.5, không trigger, layer Ground (8), tag Untagged.
        /// </summary>
        private static void BuildMovingPlatformPrefab()
        {
            var go = new GameObject("MovingPlatform");
            try
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = LoadFirstSprite(PlatformSpritePath);

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(2f, 0.5f);
                col.isTrigger = false;

                go.AddComponent<MovingPlatform>();

                go.tag = "Untagged";
                go.layer = GroundLayer;
                SavePrefab(go, PrefabsDir + "/MovingPlatform.prefab");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region Điểm bắt đầu màn

        /// <summary>
        /// Tạo LevelStart.prefab: SpriteRenderer Start (Moving), Animator, LevelStartPoint với P1SpawnPoint & P2SpawnPoint là con của prefab.
        /// </summary>
        private static void BuildLevelStartPrefab()
        {
            var go = new GameObject("LevelStart");
            try
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = LoadFirstSprite(StartSpritePath);

                go.AddComponent<Animator>();

                var p1 = new GameObject("P1SpawnPoint").transform;
                p1.SetParent(go.transform, false);
                var p2 = new GameObject("P2SpawnPoint").transform;
                p2.SetParent(go.transform, false);

                var point = go.AddComponent<LevelStartPoint>();
                var so = new SerializedObject(point);
                so.FindProperty("p1SpawnPoint").objectReferenceValue = p1;
                so.FindProperty("p2SpawnPoint").objectReferenceValue = p2;
                so.ApplyModifiedPropertiesWithoutUndo();

                go.tag = "Untagged";
                go.layer = DefaultLayer;
                SavePrefab(go, PrefabsDir + "/LevelStart.prefab");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region Cổng ra

        /// <summary>
        /// Tạo ExitGate.prefab: SpriteRenderer End, BoxCollider2D, ExitGate; con StatusText (TextMeshPro) và Confetti (ParticleSystem).
        /// </summary>
        private static void BuildExitGatePrefab()
        {
            var go = new GameObject("ExitGate");
            try
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = LoadFirstSprite(EndSpritePath);

                go.AddComponent<BoxCollider2D>();

                var statusGo = new GameObject("StatusText");
                statusGo.transform.SetParent(go.transform, false);
                var statusText = statusGo.AddComponent<TextMeshPro>();
                statusText.alignment = TextAlignmentOptions.Center;
                statusText.fontSize = 3f;

                var confettiGo = new GameObject("Confetti");
                confettiGo.transform.SetParent(go.transform, false);
                var confetti = confettiGo.AddComponent<ParticleSystem>();
                confetti.Stop();

                // Shape mặc định là Mesh rỗng nên mọi hạt đều sinh ra tại 1 điểm giữa cúp.
                // Dùng ConeVolume xoay -90 quanh X: trục phun +Z thành +Y, hạt bắn lên từ miệng cúp.
                var confettiShape = confetti.shape;
                confettiShape.shapeType = ParticleSystemShapeType.ConeVolume;
                confettiShape.angle = 25f;
                confettiShape.position = new Vector3(0f, 0.9f, 0f);
                confettiShape.rotation = new Vector3(-90f, 0f, 0f);
                confettiShape.scale = new Vector3(1.6f, 0.2f, 0.1f);

                // Hạt rơi xuống thay vì bay thẳng mãi.
                var confettiMain = confetti.main;
                confettiMain.gravityModifier = 0.35f;

                // ParticleSystem mặc định không có material nên Unity render ra màu hồng
                // (magenta). Phải gán material URP Particle Unlit + sprite Confetti.
                var confettiRenderer = confettiGo.GetComponent<ParticleSystemRenderer>();
                confettiRenderer.material = AssetDatabase.LoadAssetAtPath<Material>(ParticleMaterialPath);
                var confettiSprite = LoadFirstSprite(ConfettiSpritePath);
                if (confettiSprite != null)
                {
                    var textureSheet = confetti.textureSheetAnimation;
                    textureSheet.enabled = true;
                    textureSheet.numTilesX = 6;
                    textureSheet.numTilesY = 1;
                    textureSheet.SetSprite(0, confettiSprite);
                }

                var gate = go.AddComponent<ExitGate>();
                var so = new SerializedObject(gate);
                so.FindProperty("statusLabel").objectReferenceValue = statusText;
                so.FindProperty("confettiVfx").objectReferenceValue = confetti;
                so.ApplyModifiedPropertiesWithoutUndo();

                go.tag = "Untagged";
                go.layer = DefaultLayer;
                SavePrefab(go, PrefabsDir + "/ExitGate.prefab");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region Helper dùng chung

        /// <summary>
        /// Lấy sprite đầu tiên trong một file đã slice. Trả về null nếu không tìm thấy.
        /// </summary>
        private static Sprite LoadFirstSprite(string path)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite) return sprite;
            }
            Debug.LogWarning($"[Module4AssetBuilder] Không tìm thấy Sprite tại: {path}");
            return null;
        }

        /// <summary>
        /// Lưu GameObject thành Prefab asset tại đường dẫn chỉ định.
        /// </summary>
        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }

        #endregion

        #region Vật phẩm trái cây

        private static readonly string[] FruitNames =
        {
            "Apple", "Bananas", "Kiwi", "Orange", "Pineapple", "Cherries", "Melon", "Strawberry"
        };

        /// <summary>
        /// 3 quả thưởng KHÔNG tính vào cổng khoá [x/3] — theo SCHEMA §1.5.
        /// </summary>
        private static readonly string[] BonusFruitNames = { "Cherries", "Melon", "Strawberry" };

        /// <summary>
        /// Tạo 8 Prefab Fruit_*: CircleCollider2D (radius 0.5, isTrigger), CollectibleItem (IResettable), tag Item.
        /// </summary>
        private static void BuildFruitPrefabs()
        {
            foreach (string fruitName in FruitNames)
            {
                var fruitType = (FruitType)Enum.Parse(typeof(FruitType), fruitName);
                var go = new GameObject("Fruit_" + fruitName);
                try
                {
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = LoadFirstSprite(FruitsDir + "/" + fruitName + ".png");

                    var col = go.AddComponent<CircleCollider2D>();
                    col.radius = 0.5f;
                    col.isTrigger = true;

                    var item = go.AddComponent<CollectibleItem>();
                    var so = new SerializedObject(item);
                    so.FindProperty("fruitType").enumValueIndex = (int)fruitType;
                    so.FindProperty("countsAsRelic").boolValue =
                        Array.IndexOf(BonusFruitNames, fruitName) < 0;
                    so.ApplyModifiedPropertiesWithoutUndo();

                    go.tag = "Item";
                    go.layer = DefaultLayer;
                    SavePrefab(go, PrefabsDir + "/Fruit_" + fruitName + ".prefab");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
        }

        #endregion
    }
}
#endif
