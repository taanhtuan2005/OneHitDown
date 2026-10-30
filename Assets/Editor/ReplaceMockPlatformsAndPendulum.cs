// Trần Văn Tuyên — đợt 3: thay 3 mock còn lại trong Level_03 bằng prefab thật.
//
//   P3_MovingPlatform_Y / P3_MovingPlatform_X  -> MovingPlatform.prefab
//   P5_SpikedBallPendulum                      -> SpikedBall.prefab
//
// P5_VictoryAltar_Goal (MockGoalTrigger) KHÔNG đụng tới: ExitGate.prefab cần
// GameManager + Canvas + TextMeshPro, mà Level_03 chưa có. Thay lúc này sẽ lỗi.
//
// Trước khi chạy, MovingPlatform.prefab đã được sửa điều kiện normal.y
// (trước đây ngược dấu nên không nhận passenger). SpikedBall.prefab đã đặt
// armLength 9.8 / maxAngle 48 / swingSpeed 2 cho khớp mock.
//
// Cách dùng: menu Tools/One Hit Down/Replace Mock Platforms & Pendulum in Level 3

using OneHitDown.Dev.Mocks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHitDown.EditorTools
{
    public static class ReplaceMockPlatformsAndPendulum
    {
        private const string ScenePath = "Assets/Scenes/Level_03.unity";
        private const string MovingPlatformPrefab = "Assets/Prefabs/MovingPlatform.prefab";
        private const string SpikedBallPrefab = "Assets/Prefabs/SpikedBall.prefab";

        [MenuItem("Tools/One Hit Down/Replace Mock Platforms & Pendulum in Level 3")]
        public static void Run()
        {
            var platform = AssetDatabase.LoadAssetAtPath<GameObject>(MovingPlatformPrefab);
            var pendulum = AssetDatabase.LoadAssetAtPath<GameObject>(SpikedBallPrefab);
            if (platform == null || pendulum == null)
            {
                Debug.LogError("[ReplaceP3] Thiếu MovingPlatform.prefab hoặc SpikedBall.prefab.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int replaced = 0;

            replaced += Swap<MockMovingPlatform>(platform, "MovingPlatform");
            replaced += Swap<MockSpikedBallPendulum>(pendulum, "SpikedBall");

            if (replaced == 0)
            {
                Debug.LogWarning("[ReplaceP3] Không tìm thấy mock nào để thay.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            Debug.Log($"[ReplaceP3] Xong: thay {replaced} object.");
        }

        private static int Swap<T>(GameObject prefab, string label) where T : Component
        {
            var mocks = Object.FindObjectsByType<T>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            int count = 0;
            foreach (var mock in mocks)
            {
                Transform old = mock.transform;
                Transform parent = old.parent;

                Vector3 pos = old.position;
                Quaternion rot = old.rotation;
                Vector3 scale = old.localScale;
                string goName = old.name;
                int siblingIndex = old.GetSiblingIndex();

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
                count++;
                Debug.Log($"[ReplaceP3] {goName} -> {label}");
            }
            return count;
        }
    }
}
