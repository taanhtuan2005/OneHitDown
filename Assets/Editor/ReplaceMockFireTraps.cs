// Trần Văn Tuyên — thay toàn bộ MockFireTrap trong Level_03 bằng FireTrap.prefab.
//
// Bối cảnh: Level_03.unity đang chứa 47 object dùng MockFireTrap (trải trên các khu
// P1..P5). FireTrap.prefab trước đây KHÔNG có bệ đứng layer Ground(8) nên player
// đứng trên mặt bẫy nhưng IsGrounded = false → không nhảy được. Đã bổ sung child
// FireBaseSolid (layer 8, 1x1) vào prefab, nên giờ thay được an toàn.
//
// Script tìm theo component MockFireTrap (không dựa vào tên), nên không vỡ khi
// ai đó đổi tên object.
//
// Cách dùng: menu Tools/One Hit Down/Replace Mock FireTraps in Level 3
// Nên commit trước khi chạy để có thể revert nếu cần.

using OneHitDown.Dev.Mocks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHitDown.EditorTools
{
    public static class ReplaceMockFireTraps
    {
        private const string ScenePath = "Assets/Scenes/Level_03.unity";
        private const string FireTrapPrefabPath = "Assets/Prefabs/FireTrap.prefab";

        [MenuItem("Tools/One Hit Down/Replace Mock FireTraps in Level 3")]
        public static void Run()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FireTrapPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[ReplaceFire] Không thấy prefab: {FireTrapPrefabPath}");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var mocks = Object.FindObjectsByType<MockFireTrap>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (mocks.Length == 0)
            {
                Debug.LogWarning("[ReplaceFire] Không tìm thấy MockFireTrap nào trong Level_03.");
                return;
            }

            int replaced = 0;
            foreach (var mock in mocks)
            {
                Transform old = mock.transform;
                Transform parent = old.parent;

                Vector3 pos = old.position;
                Quaternion rot = old.rotation;
                Vector3 scale = old.localScale;
                string goName = old.name;
                int siblingIndex = old.GetSiblingIndex();

                // Mock đặt sortingOrder riêng để ngọn lửa nổi trên nền — prefab mặc định 0
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

                Object.DestroyImmediate(old.gameObject);
                replaced++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            Debug.Log($"[ReplaceFire] Xong: thay {replaced}/{mocks.Length} MockFireTrap bằng FireTrap.prefab.");
        }
    }
}
