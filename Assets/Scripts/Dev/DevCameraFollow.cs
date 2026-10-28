// Trần Văn Tuyên & Nhóm 505
// Script Camera theo dõi người chơi trong Scene thử nghiệm Dev_Nguoi3.unity
using UnityEngine;

namespace OneHitDown.Dev
{
    /// <summary>
    /// Camera 2D bám mượt theo người chơi theo trục X và Y, có giới hạn biên.
    /// Giúp người chơi di chuyển qua lại giữa 6 trạm kiểm thử một cách trực quan.
    /// </summary>
    public class DevCameraFollow : MonoBehaviour
    {
        [Header("Mục tiêu theo dõi")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.8f, -10f);
        [SerializeField] private float smoothSpeed = 6f;

        [Header("Giới hạn biên Camera")]
        [SerializeField] private Vector2 minBounds = new Vector2(-6f, -2f);
        [SerializeField] private Vector2 maxBounds = new Vector2(75f, 16f);

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                GameObject playerGo = GameObject.FindWithTag("Player");
                if (playerGo != null)
                {
                    target = playerGo.transform;
                }
                else
                {
                    return;
                }
            }

            Vector3 desiredPosition = target.position + offset;
            float clampedX = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            float clampedY = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            Vector3 targetPosition = new Vector3(clampedX, clampedY, offset.z);

            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
