// Trần Văn Tuyên, phiên bản 1.0
// Module 3: Interactive Gadgets & Environmental Mechanics
using System.Collections.Generic;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Traps
{
    /// <summary>
    /// Sàn băng trơn trượt (Màn 3): khi nhân vật bước vào, chuyển chế độ ma sát sang <c>iceFriction</c> (4f),
    /// khiến nhân vật trượt dài và khó đổi hướng. Tự dọn dẹp trạng thái khi nhân vật rời sàn hoặc script bị tắt.
    /// </summary>
    public class IceSurface : MonoBehaviour
    {
        [SerializeField] private PhysicsMaterial2D iceMaterial; // Vật liệu ma sát thấp (friction <= 0.05f)
        [SerializeField] private ParticleSystem iceParticle;     // Hiệu ứng hạt băng tuyết khi trượt

        private readonly HashSet<PlayerController> currentPlayers = new();

        /// <summary>Số lượng nhân vật đang có mặt trên mặt băng này.</summary>
        public int ActivePlayerCount => currentPlayers.Count;

        /// <summary>Áp dụng PhysicsMaterial2D lên collider nếu đã gán trong Inspector.</summary>
        private void Awake()
        {
            if (iceMaterial == null)
            {
                return;
            }

            if (TryGetComponent(out Collider2D col))
            {
                col.sharedMaterial = iceMaterial;
            }
        }

        /// <summary>Khi nhân vật bước vào vùng băng, kích hoạt trạng thái trượt.</summary>
        private void OnTriggerEnter2D(Collider2D col)
        {
            HandlePlayerEnter(col);
        }

        /// <summary>Khi nhân vật rời vùng băng, trả lại gia tốc mặt đất bình thường.</summary>
        private void OnTriggerExit2D(Collider2D col)
        {
            HandlePlayerExit(col);
        }

        /// <summary>Hỗ trợ cả trường hợp level designer sử dụng collider thường không tick IsTrigger.</summary>
        private void OnCollisionEnter2D(Collision2D col)
        {
            HandlePlayerEnter(col.collider);
        }

        /// <summary>Hỗ trợ nhả trạng thái băng khi nhân vật nhảy rời sàn bằng collision thường.</summary>
        private void OnCollisionExit2D(Collision2D col)
        {
            HandlePlayerExit(col.collider);
        }

        /// <summary>
        /// Giải phóng an toàn khi bệ băng bị tắt hoặc chuyển màn: trả lại trạng thái không trượt
        /// cho mọi nhân vật đang đứng trên, tránh rò rỉ trạng thái trượt sang khu vực khác.
        /// </summary>
        private void OnDisable()
        {
            foreach (PlayerController player in currentPlayers)
            {
                if (player != null)
                {
                    player.SetOnIce(false);
                }
            }

            currentPlayers.Clear();

            if (iceParticle != null)
            {
                iceParticle.Stop();
            }
        }

        /// <summary>Xử lý nhân vật bước vào mặt băng.</summary>
        private void HandlePlayerEnter(Collider2D col)
        {
            if (!col.CompareTag("Player"))
            {
                return;
            }

            if (!col.TryGetComponent(out PlayerController player))
            {
                return;
            }

            if (currentPlayers.Add(player))
            {
                player.SetOnIce(true);
            }

            if (iceParticle != null && !iceParticle.isPlaying)
            {
                iceParticle.Play();
            }
        }

        /// <summary>Xử lý nhân vật rời mặt băng.</summary>
        private void HandlePlayerExit(Collider2D col)
        {
            if (!col.CompareTag("Player"))
            {
                return;
            }

            if (!col.TryGetComponent(out PlayerController player))
            {
                return;
            }

            if (currentPlayers.Remove(player))
            {
                player.SetOnIce(false);
            }

            if (currentPlayers.Count == 0 && iceParticle != null && iceParticle.isPlaying)
            {
                iceParticle.Stop();
            }
        }
    }
}