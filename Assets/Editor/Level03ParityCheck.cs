// ====================================================================================================
// CÔNG CỤ AUDIT CHỈ ĐỌC: KIỂM CHỨNG SCENE 'Assets/Scenes/Level_03.unity' KHỚP "CHỈ SỐ VÀNG"
//
// Mục đích: nghiệm thu sau khi Level03SceneBuilder được sửa. Công cụ KHÔNG ghi, KHÔNG lưu scene.
// Cách đếm:
//   - GameObject / PrefabInstance: regex YAML trên file scene (`--- !u!1 &`, `--- !u!1001 &`).
//   - Prefab instance theo GUID: regex `m_SourcePrefab: {fileID: 100100000, guid: <guid>`.
//     (Duyệt cây GameObject sẽ đếm trùng instance lồng nhau nên KHÔNG dùng.)
//   - Tham chiếu Mock: đọc GUID từ `Assets/Scripts/Dev/Mocks/*.cs.meta`, đếm xuất hiện trong YAML.
//   - Tag Player / Hazard: regex `m_TagString: Player` / `m_TagString: Hazard`.
//   - Root object: mở scene additive rồi dùng `scene.GetRootGameObjects()` (chỉ đọc tên).
//
// Menu Editor: Tools -> One Hit Down -> Audit Level 03 Parity
// ====================================================================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHitDown.EditorTools
{
    public static class Level03ParityCheck
    {
        private const string ScenePath = "Assets/Scenes/Level_03.unity";
        private const string MocksDir = "Assets/Scripts/Dev/Mocks";

        /// <summary>Một dòng chỉ số audit: tên, giá trị mong đợi, giá trị thực tế.</summary>
        private sealed class Metric
        {
            public readonly string Name;
            public readonly int Expected;
            public readonly int Actual;

            public Metric(string name, int expected, int actual)
            {
                Name = name;
                Expected = expected;
                Actual = actual;
            }

            public bool Pass => Expected == Actual;
        }

        /// <summary>Cặp nhãn + GUID prefab cần đếm trong scene.</summary>
        private struct PrefabExpectation
        {
            public readonly string Label;
            public readonly string Guid;
            public readonly int Expected;

            public PrefabExpectation(string label, string guid, int expected)
            {
                Label = label;
                Guid = guid;
                Expected = expected;
            }
        }

        // "Chỉ số vàng" đo từ scene chuẩn của Màn 3.
        private static readonly PrefabExpectation[] PrefabExpectations =
        {
            new PrefabExpectation("FireTrap", "2b8cac97d1712ac4a8dbcd2dc942e156", 47),
            new PrefabExpectation("IceRoad", "f0122025810610b4aa7856280a8d0c26", 11),
            new PrefabExpectation("Fruit_Kiwi", "277d6cea6eed608459cbe0f5ea3a1813", 3),
            new PrefabExpectation("Fruit_Strawberry", "d3b9db132a8c1364d9dcd18ca8d506b7", 2),
            new PrefabExpectation("MovingPlatform", "92234422c5b57e445928a7fdfaceb73f", 2),
            new PrefabExpectation("SpikedBall", "3bb069441697a13438a2fb9058542728", 1),
            new PrefabExpectation("CoreRig", "7b897264041438e45896772e62a9fb7b", 1),
            new PrefabExpectation("ExitGate", "3076cd4792d2b3440af48047960d08db", 1),
            new PrefabExpectation("LevelStart", "c2a9972a791b8da4c9d933483b81d69a", 1)
        };

        // Tiền tố tên 10 root object bắt buộc phải có (7 group + 3 root prefab: CoreRig, LevelStart, ExitGate).
        private static readonly string[] RequiredRootPrefixes =
        {
            "--- 01.",
            "--- 02.",
            "--- 03.",
            "--- 04.",
            "--- 05.",
            "--- 07. HUD LABELS & SIGNS ---",
            "KillZone",
            "CoreRig",
            "LevelStart",
            "ExitGate"
        };

        private const int ExpectedRootCount = 10;

        [MenuItem("Tools/One Hit Down/Audit Level 03 Parity", false, 60)]
        public static void Audit()
        {
            string fullScenePath = Path.GetFullPath(ScenePath);
            if (!File.Exists(fullScenePath))
            {
                Debug.LogError($"[Level03Parity] Không tìm thấy file scene: {fullScenePath}");
                return;
            }

            string yaml;
            try
            {
                yaml = File.ReadAllText(fullScenePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Level03Parity] Lỗi đọc file scene: {ex.Message}");
                return;
            }

            List<Metric> metrics = new List<Metric>();
            List<string> rootNames = new List<string>();
            Scene scene = default;
            bool openedByTool = false;

            try
            {
                // ---- Đếm cấu trúc YAML ----
                metrics.Add(new Metric("Tổng GameObject", 251, CountYaml(yaml, @"^--- !u!1 &")));
                metrics.Add(new Metric("Tổng PrefabInstance", 79, CountYaml(yaml, @"^--- !u!1001 &")));

                foreach (PrefabExpectation expectation in PrefabExpectations)
                {
                    string pattern = Regex.Escape(
                        $"m_SourcePrefab: {{fileID: 100100000, guid: {expectation.Guid}");
                    metrics.Add(new Metric(
                        $"Instance {expectation.Label}.prefab",
                        expectation.Expected,
                        CountYaml(yaml, pattern)));
                }

                // ---- Tham chiếu Mock (namespace OneHitDown.Dev.Mocks) ----
                int mockRefs = CountMockReferences(yaml);
                metrics.Add(new Metric("Tham chiếu script Mock (OneHitDown.Dev.Mocks)", 0, mockRefs));

                // ---- Tag ----
                metrics.Add(new Metric("Object tag Player", 0, CountYaml(yaml, @"m_TagString: Player")));
                metrics.Add(new Metric("Object tag Hazard", 12, CountYaml(yaml, @"m_TagString: Hazard")));

                // ---- Root object (chỉ đọc, mở additive để không phá scene đang mở) ----
                scene = SceneManager.GetSceneByPath(ScenePath);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                    openedByTool = true;
                }

                rootNames = scene.GetRootGameObjects().Select(go => go.name).ToList();
                metrics.Add(new Metric("Tổng root object", ExpectedRootCount, rootNames.Count));

                foreach (string prefix in RequiredRootPrefixes)
                {
                    int found = rootNames.Count(n => n != null && n.StartsWith(prefix, StringComparison.Ordinal));
                    metrics.Add(new Metric($"Root group '{prefix}'", 1, found));
                }

                int charactersGroup = rootNames.Count(
                    n => n != null && n.IndexOf("06. CHARACTERS", StringComparison.Ordinal) >= 0);
                metrics.Add(new Metric("Root group '06. CHARACTERS' (phải vắng)", 0, charactersGroup));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Level03Parity] Lỗi khi audit scene: {ex.Message}\n{ex.StackTrace}");
                return;
            }
            finally
            {
                // Chỉ đóng scene do chính công cụ mở để không ảnh hưởng scene người dùng đang mở.
                if (openedByTool && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            PrintReport(metrics, rootNames);
        }

        /// <summary>Đếm số lần khớp regex (multiline) trên nội dung YAML scene.</summary>
        private static int CountYaml(string yaml, string pattern)
        {
            return Regex.Matches(yaml, pattern, RegexOptions.Multiline).Count;
        }

        /// <summary>Đọc GUID các script Mock rồi đếm tổng số lần xuất hiện trong YAML scene.</summary>
        private static int CountMockReferences(string yaml)
        {
            if (!Directory.Exists(MocksDir))
            {
                return 0;
            }

            int total = 0;
            string[] metaFiles = Directory.GetFiles(MocksDir, "*.cs.meta", SearchOption.TopDirectoryOnly);
            foreach (string metaFile in metaFiles)
            {
                string guid = ReadGuid(metaFile);
                if (string.IsNullOrEmpty(guid))
                {
                    continue;
                }

                total += Regex.Matches(yaml, Regex.Escape(guid)).Count;
            }

            return total;
        }

        /// <summary>Lấy dòng `guid: ...` trong file .meta.</summary>
        private static string ReadGuid(string metaPath)
        {
            try
            {
                foreach (string line in File.ReadAllLines(metaPath))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("guid:", StringComparison.Ordinal))
                    {
                        return trimmed.Substring("guid:".Length).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Level03Parity] Không đọc được meta '{metaPath}': {ex.Message}");
            }

            return null;
        }

        /// <summary>In báo cáo dạng bảng ra Console và tổng kết PASS/FAIL.</summary>
        private static void PrintReport(List<Metric> metrics, List<string> rootNames)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Level03Parity] ===== AUDIT PARITY MÀN 3 =====");

            int passed = 0;
            foreach (Metric metric in metrics)
            {
                if (metric.Pass)
                {
                    passed++;
                    sb.AppendLine($"[PASS] {metric.Name}: mong đợi {metric.Expected}, thực tế {metric.Actual}");
                }
                else
                {
                    sb.AppendLine($"[FAIL] {metric.Name}: mong đợi {metric.Expected}, thực tế {metric.Actual}");
                }
            }

            sb.AppendLine($"[Level03Parity] Root object hiện có ({rootNames.Count}): {string.Join(", ", rootNames)}");
            sb.AppendLine($"[Level03Parity] ===== TỔNG KẾT: PASS {passed}/{metrics.Count} =====");

            if (passed == metrics.Count)
            {
                Debug.Log(sb.ToString());
            }
            else
            {
                Debug.LogError(sb.ToString());
            }
        }
    }
}
#endif
