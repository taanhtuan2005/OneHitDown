#if UNITY_EDITOR
using OneHitDown.UI;
using UnityEditor;
using UnityEngine;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Công cụ dev phục vụ kiểm thử Module 5.
    /// Chỉ tồn tại trong Editor, không sinh mã trong bản build.
    /// Dùng để bật/tắt chế độ 2 người (kiểm thử SoulTether) và xoá PlayerPrefs
    /// khi chưa có MainMenu.unity để chọn chế độ chơi.
    /// </summary>
    internal static class Module5DevTools
    {
        private const string MenuRoot = "Tools/One Hit Down/Dev/";
        private const string ToggleTwoPlayerItem = MenuRoot + "Toggle 2-Player Mode";
        private const string ResetPrefsItem = MenuRoot + "Reset PlayerPrefs";

        /// <summary>
        /// Bật/tắt cờ 2 người trong GameSession.
        /// Giá trị được ghi ngay vào PlayerPrefs nên cần vào Play lại mới áp dụng.
        /// </summary>
        [MenuItem(ToggleTwoPlayerItem, false, 60)]
        private static void ToggleTwoPlayerMode()
        {
            bool nextValue = !GameSession.IsTwoPlayerMode;
            GameSession.IsTwoPlayerMode = nextValue;

            Debug.Log(nextValue
                ? "<b>[Module5DevTools]</b> Đã BẬT chế độ 2 người (Player1 + Player2 + SoulTether). Vào Play lại để áp dụng."
                : "<b>[Module5DevTools]</b> Đã TẮT chế độ 2 người (chỉ còn Player1). Vào Play lại để áp dụng.");
        }

        /// <summary>
        /// Hiển thị dấu tick khi chế độ 2 người đang bật.
        /// </summary>
        [MenuItem(ToggleTwoPlayerItem, true)]
        private static bool ToggleTwoPlayerModeValidate()
        {
            Menu.SetChecked(ToggleTwoPlayerItem, GameSession.IsTwoPlayerMode);
            return true;
        }

        /// <summary>
        /// Xoá toàn bộ PlayerPrefs của project (chế độ chơi, skin, màn đã mở khoá).
        /// </summary>
        [MenuItem(ResetPrefsItem, false, 61)]
        private static void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("<b>[Module5DevTools]</b> Đã xoá toàn bộ PlayerPrefs: chế độ 1 người, skin mặc định, màn đã mở khoá về 1.");
        }
    }
}
#endif
