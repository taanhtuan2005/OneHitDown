using System.Collections.Generic;
using OneHitDown.Core;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Level
{
    /// <summary>
    /// Bệ nâng di động: tuần tra ping-pong giữa 2 điểm và truyền vận tốc cho Player.
    /// Cơ chế giữ nguyên theo bản mock đã kiểm chứng trên Level_03 (Vector2.MoveTowards +
    /// normal.y &lt; -0.5f để nhận passenger), chỉ đổi tên lớp sang bản chính thức.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MovingPlatform : MonoBehaviour, IResettable
    {
        [Header("Tuần tra Di chuyển")]
        [SerializeField] private Vector2 startOffset = Vector2.zero;       // Độ lệch điểm đầu A so với vị trí đặt bệ ban đầu trong Scene
        [SerializeField] private Vector2 moveOffset = new Vector2(8f, 0f); // Khoảng dịch chuyển mục tiêu từ điểm A tới điểm B
        [SerializeField] private float speed = 3.0f;                       // Vận tốc bệ di chuyển

        private Vector2 initialScenePos;
        private Vector2 pointA;
        private Vector2 pointB;
        private Vector2 targetPoint;
        private Vector2 currentVelocity;
        private Vector2 previousPos;
        private readonly HashSet<PlayerController> passengers = new HashSet<PlayerController>();

        private void Awake()
        {
            initialScenePos = transform.position;
            pointA = initialScenePos + startOffset;
            pointB = pointA + moveOffset;
            targetPoint = pointB;
            previousPos = initialScenePos;
        }

        private void FixedUpdate()
        {
            Vector2 currentPos = transform.position;
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPoint, speed * Time.fixedDeltaTime);
            transform.position = newPos;

            // Tính vận tốc thực tế của bệ
            currentVelocity = (newPos - currentPos) / Time.fixedDeltaTime;

            // Cập nhật vận tốc cho mọi player đang đứng trên bệ
            foreach (PlayerController p in passengers)
            {
                if (p != null)
                {
                    p.SetPlatformVelocity(currentVelocity);
                }
            }

            if (Vector2.Distance(newPos, targetPoint) < 0.02f)
            {
                targetPoint = (targetPoint == pointA) ? pointB : pointA;
            }

            previousPos = newPos;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.collider.CompareTag("Player") && col.collider.TryGetComponent(out PlayerController p))
            {
                // Chỉ nhận khi player đứng trên mặt bệ (normal hướng lên)
                if (col.contactCount > 0 && col.GetContact(0).normal.y < -0.5f)
                {
                    passengers.Add(p);
                }
            }
        }

        private void OnCollisionExit2D(Collision2D col)
        {
            if (col.collider.CompareTag("Player") && col.collider.TryGetComponent(out PlayerController p))
            {
                if (passengers.Remove(p))
                {
                    p.SetPlatformVelocity(Vector2.zero);
                }
            }
        }

        private void OnDisable()
        {
            foreach (PlayerController p in passengers)
            {
                if (p != null) p.SetPlatformVelocity(Vector2.zero);
            }
            passengers.Clear();
        }

        public void ResetState()
        {
            transform.position = initialScenePos;
            targetPoint = pointB;
            currentVelocity = Vector2.zero;
            foreach (PlayerController p in passengers)
            {
                if (p != null) p.SetPlatformVelocity(Vector2.zero);
            }
            passengers.Clear();
        }
    }
}
