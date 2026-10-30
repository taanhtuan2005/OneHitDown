// Trần Văn Tuyên — thay 5 MockCollectibleItem trong Level_03 bằng prefab thật.
//
// Bối cảnh: Level_03.unity đang chứa 5 object dùng MockCollectibleItem (3 Kiwi relic
// + 2 Berry bonus). Prefab Fruit_Kiwi / Fruit_Strawberry đã sẵn sàng và có cấu trúc
// tương đương, nên thay được mà không phải dựng lại scene.
//
// Script tìm theo component MockCollectibleItem (không dựa vào tên object), nên
// không vỡ khi ai đó đổi tên hay di chuyển object trong Hierarchy.
//
// Cách dùng: menu Tools/One Hit Down/Replace Mock Collectibles in Level 3
// Nên Commit trước khi chạy để có thể revert nếu cần.

using OneHitDown.Dev.Mocks;
using OneHitDown.Items;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHitDown.EditorTools
{
    public static class ReplaceMockCollectibles
    {
        private const string ScenePath = "Assets/Scenes/Level_03.unity";
        private const string KiwiPrefabPath = "Assets/Prefabs/Fruit_Kiwi.prefab";
        private const string StrawberryPrefabPath = "Assets/Prefabs/Fruit_Strawberry.prefab";

        [MenuItem("Tools/One Hit Down/Replace Mock Collectibles in Level 3")]
        public static void Run()
        {
            var kiwi = AssetDatabase.LoadAssetAtPath<GameObject>(KiwiPrefabPath);
            var strawberry = AssetDatabase.LoadAssetAtPath<GameObject>(StrawberryPrefabPath);
            if (kiwi == null || strawberry == null)
            {
                Debug.LogError("[ReplaceMock] Thiếu prefab Fruit_Kiwi hoặc Fruit_Strawberry.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var mocks = Object.FindObjectsByType<MockCollectibleItem>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (mocks.Length == 0)
            {
                Debug.LogWarning("[ReplaceMock] Không tìm thấy MockCollectibleItem nào trong Level_03.");
                return;
            }

            int replaced = 0;
            foreach (var mock in mocks)
            {
                Transform old = mock.transform;
                Transform parent = old.parent;

                // Kiwi là cổ vật mở cổng, Berry là quà thưởng phụ
                bool isRelic = mock.IsRelic;
                GameObject prefab = isRelic ? kiwi : strawberry;

                Vector3 pos = old.position;
                Quaternion rot = old.rotation;
                Vector3 scale = old.localScale;
                string goName = old.name;
                int siblingIndex = old.GetSiblingIndex();

                // Mock xếp sortingOrder 9 để nổi trên nền; prefab mặc định 0 nên phải giữ lại
                var oldSr = old.GetComponent<SpriteRenderer>();
                int sortingOrder = oldSr != null ? oldSr.sortingOrder : 0;
                string sortingLayer = oldSr != null ? oldSr.sortingLayerName : "Default";

                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                inst.transform.position = pos;
                inst.transform.rotation = rot;
                inst.transform.localScale = scale;
                inst.name = goName;
                inst.transform.SetSiblingIndex(siblingIndex);

                var newSr = inst.GetComponent<SpriteRenderer>();
                if (newSr != null && oldSr != null)
                {
                    newSr.sortingOrder = sortingOrder;
                    newSr.sortingLayerName = sortingLayer;
                }

                // Giữ đúng vai trò: 2 Berry không được tính vào cổng [x/3]
                var item = inst.GetComponent<CollectibleItem>();
                if (item != null)
                {
                    var so = new SerializedObject(item);
                    so.FindProperty("countsAsRelic").boolValue = isRelic;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                Object.DestroyImmediate(old.gameObject);
                replaced++;
                Debug.Log($"[ReplaceMock] {goName} -> {prefab.name} (relic={isRelic})");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            Debug.Log($"[ReplaceMock] Xong: thay {replaced}/{mocks.Length} object.");
        }
    }
}
