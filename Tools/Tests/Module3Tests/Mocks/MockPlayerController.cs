// Trần Văn Tuyên, phiên bản 1.0
// Mock PlayerController dùng riêng cho bộ unit & integration tests của Module 3.
using System.Collections.Generic;
using UnityEngine;

namespace OneHitDown.Player
{
    public class PlayerController : MonoBehaviour
    {
        private bool isDead;
        private Rigidbody2D cachedRb;

        public bool IsDead
        {
            get => isDead;
            set => isDead = value;
        }

        public Rigidbody2D Rigidbody
        {
            get
            {
                if (cachedRb == null)
                {
                    cachedRb = GetComponent<Rigidbody2D>();
                }

                return cachedRb;
            }
        }

        // Ghi lại lịch sử các lệnh gọi để assert trong test
        public List<Vector2> BoostHistory { get; } = new List<Vector2>();
        public List<Vector2> ExternalVelocityHistory { get; } = new List<Vector2>();
        public List<bool> IceStateHistory { get; } = new List<bool>();

        public int BoostCount => BoostHistory.Count;
        public int ExternalVelocityCount => ExternalVelocityHistory.Count;
        public bool IsCurrentlyOnIce { get; private set; }

        public void GrantDoubleJumpAndBoost(Vector2 boostVelocity)
        {
            BoostHistory.Add(boostVelocity);
        }

        public void AddExternalVelocity(Vector2 extVel)
        {
            ExternalVelocityHistory.Add(extVel);
        }

        public void SetOnIce(bool onIce)
        {
            IsCurrentlyOnIce = onIce;
            IceStateHistory.Add(onIce);
        }

        public void ResetMock()
        {
            isDead = false;
            BoostHistory.Clear();
            ExternalVelocityHistory.Clear();
            IceStateHistory.Clear();
            IsCurrentlyOnIce = false;
        }

        /// <summary>Helper tạo nhanh một Player GameObject có đầy đủ Collider2D, Rigidbody2D và PlayerController.</summary>
        public static (GameObject go, PlayerController player, BoxCollider2D col, Rigidbody2D rb) CreateMockPlayer(string name = "MockPlayer")
        {
            GameObject go = new GameObject(name)
            {
                tag = "Player"
            };

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            PlayerController player = go.AddComponent<PlayerController>();

            return (go, player, col, rb);
        }
    }
}