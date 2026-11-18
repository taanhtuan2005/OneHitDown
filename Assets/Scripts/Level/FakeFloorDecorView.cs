using OneHitDown.Traps;
using UnityEngine;

namespace OneHitDown.Level
{
    /// <summary>
    /// Đồng bộ các viên gạch trang trí của sàn giả theo trạng thái sụp của DeathPitTrap.
    /// DeathPitTrap chỉ tắt renderer của khối sàn, nên gạch con phải tự ẩn/hiện để không lơ lửng trên hố.
    /// </summary>
    public sealed class FakeFloorDecorView : MonoBehaviour
    {
        [SerializeField] private DeathPitTrap trap;
        [SerializeField] private SpriteRenderer[] decorations;

        private bool isHidden;

        private void Awake()
        {
            if (trap == null)
            {
                trap = GetComponentInParent<DeathPitTrap>();
            }

            if (trap == null)
            {
                Debug.LogError("[FakeFloorDecorView] Chưa gán DeathPitTrap, gạch trang trí sẽ không đồng bộ.", this);
                enabled = false;
                return;
            }

            ApplyHidden(trap.IsTriggered);
        }

        private void LateUpdate()
        {
            if (trap == null)
            {
                return;
            }

            bool collapsed = trap.IsTriggered;
            if (collapsed == isHidden)
            {
                return;
            }

            ApplyHidden(collapsed);
        }

        /// <summary>Ẩn toàn bộ gạch trang trí khi sàn sụp, hiện lại khi ResetState() dựng sàn.</summary>
        private void ApplyHidden(bool hidden)
        {
            isHidden = hidden;

            if (decorations == null)
            {
                return;
            }

            for (int i = 0; i < decorations.Length; i++)
            {
                SpriteRenderer decor = decorations[i];
                if (decor != null)
                {
                    decor.enabled = !hidden;
                }
            }
        }
    }
}
