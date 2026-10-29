// ====================================================================================================
// BẢN MOCK TẠM THỜI CHO SCENE LEVEL_03 — DO TRẦN VĂN TUYÊN (NGƯỜI 3) TẠO ĐỂ TEST MÀN 3.
// CẢNH BÁO CHO TEAM: ĐÂY LÀ MOCK ĐỂ TEST GAMEPLAY, HOÀN TOÀN KHÔNG THAY THẾ SCRIPT CHÍNH THỨC!
// NGƯỜI 4 (PHAN VĂN TÚ) VẪN BẮT BUỘC PHẢI TỰ VIẾT 'Assets/Scripts/Level/MovingPlatform.cs'
// THEO ĐÚNG SCHEMA §3 MODULE 4.2!
// ====================================================================================================

using System.Collections.Generic;
using OneHitDown.Core;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Dev.Mocks
{
    /// <summary>
    /// Mock Bệ Nâng Di Động: tuần tra ping-pong giữa 2 điểm và truyền vận tốc cho Player.
    /// </summary>
    public class MockMovingPlatform : MonoBehaviour, IResettable
    {
        [Header("Tuần tra Di chuyển")]
        [SerializeField] private Vector2 moveOffset = new Vector2(8f, 0f); // Khoảng dịch chuyển mục tiêu
        [SerializeField] private float speed = 3.0f;                       // Vận tốc bệ di chuyển

        private Vector2 pointA;
        private Vector2 pointB;
        private Vector2 targetPoint;
        private Vector2 currentVelocity;
        private Vector2 previousPos;
        private readonly HashSet<PlayerController> passengers = new HashSet<PlayerController>();

        private void Awake()
        {
            pointA = transform.position;
            pointB = pointA + moveOffset;
            targetPoint = pointB;
            previousPos = pointA;
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
            transform.position = pointA;
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
