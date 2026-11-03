using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Các tiện ích an toàn cho quy trình dựng lại Scene Màn 3.
    /// Builder (Level03SceneBuilder) xóa sạch và tạo lại Scene nên BẮT BUỘC phải
    /// sao lưu + xác nhận trước khi dựng, và chặn dựng khi thiếu prefab chính thức.
    /// </summary>
    public static class Level03BuildSafety
    {
        /// <summary>Đường dẫn Scene đích của Màn 3.</summary>
        public const string ScenePath = "Assets/Scenes/Level_03.unity";

        /// <summary>Thư mục chứa các bản sao lưu Scene.</summary>
        public const string BackupDirectory = "Assets/Scenes/_Backup";

        /// <summary>Tiền tố tên file sao lưu.</summary>
        private const string BackupFilePrefix = "Level_03_";

        /// <summary>
        /// Sao lưu Scene hiện có vào Assets/Scenes/_Backup/Level_03_{yyyyMMdd_HHmmss}.unity.
        /// Trả false nếu không sao lưu được (builder phải hủy dựng).
        /// Nếu Scene gốc chưa tồn tại (dựng lần đầu) thì trả true và không tạo file sao lưu.
        /// </summary>
        public static bool BackupBeforeBuild(string scenePath)
        {
            string ignoredBackupPath;
            return BackupBeforeBuild(scenePath, out ignoredBackupPath);
        }

        /// <summary>
        /// Overload lấy thêm đường dẫn file sao lưu vừa tạo (null nếu không có Scene để sao lưu).
        /// </summary>
        public static bool BackupBeforeBuild(string scenePath, out string backupPath)
        {
            backupPath = null;

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("[Level03BuildSafety] Đường dẫn Scene rỗng. Hủy dựng để bảo toàn dữ liệu.");
                return false;
            }

            try
            {
                // Dựng lần đầu: chưa có Scene gốc thì không có gì để sao lưu.
                if (!File.Exists(scenePath))
                {
                    Debug.Log("[Level03BuildSafety] Chưa có Scene gốc, bỏ qua sao lưu (dựng lần đầu).");
                    return true;
                }

                Directory.CreateDirectory(BackupDirectory);

                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                backupPath = BackupDirectory + "/" + BackupFilePrefix + stamp + ".unity";

                // Tránh ghi đè nếu trong cùng một giây có hai lần gọi.
                int suffix = 1;
                while (File.Exists(backupPath))
                {
                    backupPath = BackupDirectory + "/" + BackupFilePrefix + stamp + "_" + suffix + ".unity";
                    suffix++;
                }

                File.Copy(scenePath, backupPath, false);
                AssetDatabase.Refresh();

                Debug.Log("[Level03BuildSafety] Đã sao lưu Scene: " + backupPath);
                return true;
            }
            catch (Exception ex)
            {
                backupPath = null;
                Debug.LogError("[Level03BuildSafety] Sao lưu Scene thất bại: " + scenePath + "\n" + ex);
                return false;
            }
        }

        /// <summary>
        /// Hỏi người dùng xác nhận trước khi xóa và dựng lại Scene.
        /// Tự động trả true khi chạy batch mode (CI) để không treo tiến trình.
        /// </summary>
        public static bool ConfirmDestructiveRebuild()
        {
            if (Application.isBatchMode)
            {
                Debug.Log("[Level03BuildSafety] Batch mode: bỏ qua hộp thoại xác nhận.");
                return true;
            }

            return EditorUtility.DisplayDialog(
                "Xây lại Màn 3",
                "Thao tác này XÓA toàn bộ Level_03.unity hiện tại và dựng lại từ đầu. " +
                "Bản sao lưu đã được tạo trong Assets/Scenes/_Backup/. Tiếp tục?",
                "Xây lại",
                "Hủy");
        }

        /// <summary>
        /// Log toàn bộ prefab chính thức bị thiếu. Trả false nếu danh sách có phần tử.
        /// Null-safe: danh sách null hoặc rỗng đều được coi là hợp lệ.
        /// </summary>
        public static bool ReportMissingPrefabs(IEnumerable<string> missing)
        {
            // Duyệt một lần để tránh nhiều lần enumerate nguồn lazy.
            List<string> items = new List<string>();
            if (missing != null)
            {
                foreach (string item in missing)
                {
                    items.Add(item);
                }
            }

            if (items.Count == 0)
            {
                return true;
            }

            Debug.LogError("[Level03SceneBuilder] Thiếu prefab: " + string.Join(", ", items.ToArray()));
            return false;
        }
    }
}
