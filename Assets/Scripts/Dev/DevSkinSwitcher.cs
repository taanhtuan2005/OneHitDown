// Script CHỈ DÙNG ĐỂ TEST trong Scene Dev_Nguoi3.unity — có thể xóa khi phát hành.
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Dev
{
    /// <summary>
    /// Đổi qua lại 4 bộ skin của nhân vật bằng phím số 1..4 để kiểm thử Module 1.
    /// Gắn vào cùng GameObject với PlayerController (hoặc để trống, script tự tìm).
    /// Đọc phím qua New Input System (dự án đặt activeInputHandler = 1 - chỉ Input System).
    /// </summary>
    public class DevSkinSwitcher : MonoBehaviour
    {
        [SerializeField] private PlayerController target;

        private void Awake()
        {
            if (target == null)
            {
                target = FindAnyObjectByType<PlayerController>();
            }
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            int pressedIndex = ReadSkinKeyIndex();
            if (pressedIndex >= 0)
            {
                var skin = (CharacterSkin)pressedIndex;
                target.ApplySkin(skin);
                Debug.Log($"[DevSkinSwitcher] Đổi sang skin {skin}");
            }
        }

        /// <summary>
        /// Trả về chỉ số skin (0..3) ứng với phím số vừa nhấn, hoặc -1 nếu không nhấn phím nào.
        /// </summary>
        private static int ReadSkinKeyIndex()
        {
            int index = -1;

#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame)
                {
                    index = 0;
                }
                else if (keyboard.digit2Key.wasPressedThisFrame)
                {
                    index = 1;
                }
                else if (keyboard.digit3Key.wasPressedThisFrame)
                {
                    index = 2;
                }
                else if (keyboard.digit4Key.wasPressedThisFrame)
                {
                    index = 3;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (index < 0)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    index = 0;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    index = 1;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    index = 2;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    index = 3;
                }
            }
#endif

            return index;
        }
    }
}
