// Tạ Anh Tuấn & Nhóm 505
// Module 1: Hiệu ứng hình ảnh nhân vật (Bóng đổ, Appearing, Disappearing, Dust)
// Tuân thủ 100% tài liệu Docs/SCHEMA_CHI_TIET.md Mục 3.2 Phần 2.
using UnityEngine;

namespace OneHitDown.Player
{
    /// <summary>
    /// Quản lý hiệu ứng hình ảnh nhân vật: bóng đổ raycast, Appearing/Disappearing VFX, bụi chạm đất.
    /// </summary>
    public class PlayerVFX : MonoBehaviour
    {
        [Header("Bóng đổ Raycast (Shadow.png 16×16)")]
        [SerializeField] private Transform groundShadow; // Child GameObject chứa SpriteRenderer hiển thị Shadow.png
        [SerializeField] private float shadowMaxDistance = 3f; // Khoảng cách tối đa raycast xuống mặt đất
        [SerializeField] private LayerMask groundLayer; // Layer Ground (8) để raycast bóng đổ
        [SerializeField] private Vector2 shadowScaleRange = new Vector2(0.6f, 1.2f); // Min/Max tỷ lệ co giãn bóng đổ

        [Header("Hiệu ứng Xuất hiện/Biến mất (96×96)")]
        [SerializeField] private GameObject appearingVfxPrefab; // Prefab Appearing (96x96).png — 7 frames, 20 FPS, tự hủy 0.35s
        [SerializeField] private GameObject disappearingVfxPrefab; // Prefab Desappearing (96x96).png — 7 frames, 20 FPS, tự hủy 0.35s

        [Header("Hạt bụi tiếp đất (Dust Particle.png 16×16)")]
        [SerializeField] private ParticleSystem dustParticles; // Particle System phát hạt bụi khi tiếp đất hoặc ghì chân

        private PlayerController playerController;
        private SpriteRenderer shadowRenderer;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            if (groundShadow != null)
            {
                shadowRenderer = groundShadow.GetComponent<SpriteRenderer>();
            }

            // Mặc định layer Ground nếu chưa gán trong Inspector (Layer 8)
            if (groundLayer.value == 0)
            {
                int groundLayerIndex = LayerMask.NameToLayer("Ground");
                if (groundLayerIndex >= 0)
                {
                    groundLayer = 1 << groundLayerIndex;
                }
            }

            // Subscribe event tiếp đất để phát bụi
            if (playerController != null)
            {
                playerController.OnLanded += PlayDust;
            }
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.OnLanded -= PlayDust;
            }
        }

        private void Update()
        {
            UpdateShadowRaycast();
        }

        /// <summary>
        /// Cập nhật vị trí và tỷ lệ bóng đổ dựa trên khoảng cách raycast xuống mặt đất.
        /// Bóng đổ co giãn theo khoảng cách: càng xa mặt đất thì bóng càng nhỏ.
        /// </summary>
        public void UpdateShadowRaycast()
        {
            if (groundShadow == null || shadowRenderer == null)
            {
                return;
            }

            Vector2 origin = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, shadowMaxDistance, groundLayer);

            if (hit.collider != null)
            {
                // Đặt bóng đổ sát mặt đất
                groundShadow.position = new Vector3(hit.point.x, hit.point.y + 0.05f, groundShadow.position.z);

                // Tính tỷ lệ co giãn: gần mặt đất = to, xa mặt đất = nhỏ
                float distance = hit.distance;
                float t = Mathf.Clamp01(1f - (distance / shadowMaxDistance));
                float scale = Mathf.Lerp(shadowScaleRange.x, shadowScaleRange.y, t);
                groundShadow.localScale = Vector3.one * scale;

                // Hiển thị bóng đổ
                if (!shadowRenderer.enabled)
                {
                    shadowRenderer.enabled = true;
                }
            }
            else
            {
                // Không chạm mặt đất trong shadowMaxDistance -> ẩn bóng đổ
                if (shadowRenderer.enabled)
                {
                    shadowRenderer.enabled = false;
                }
            }
        }

        /// <summary>
        /// Phát hiệu ứng xuất hiện (Appearing 96×96) tại vị trí spawn.
        /// Gọi từ PlayerController.Respawn() hoặc GameManager khi nhân vật hồi sinh.
        /// </summary>
        public void PlaySpawnVFX(Vector3 position)
        {
            if (appearingVfxPrefab != null)
            {
                GameObject vfx = Instantiate(appearingVfxPrefab, position, Quaternion.identity);
                Destroy(vfx, 0.5f); // Tự hủy sau 0.5s (7 frames × 1/20 fps = 0.35s + buffer)
            }
        }

        /// <summary>
        /// Phát hiệu ứng biến mất (Desappearing 96×96) tại vị trí chết.
        /// Gọi từ PlayerController.Die() hoặc khi nhân vật bị tiêu diệt.
        /// </summary>
        public void PlayDeathVFX(Vector3 position)
        {
            if (disappearingVfxPrefab != null)
            {
                GameObject vfx = Instantiate(disappearingVfxPrefab, position, Quaternion.identity);
                Destroy(vfx, 0.5f); // Tự hủy sau 0.5s
            }
        }

        /// <summary>
        /// Phát hạt bụi khi tiếp đất hoặc ghì chân (Ground Anchor).
        /// Gọi từ event OnLanded của PlayerController.
        /// </summary>
        public void PlayDust()
        {
            if (dustParticles != null && !dustParticles.isPlaying)
            {
                dustParticles.Play();
            }
        }
    }
}
