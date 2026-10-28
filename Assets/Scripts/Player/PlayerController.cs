// Trần Văn Tuyên & Nhóm 505
// Stub PlayerController tạm thời cho Module 3 biên dịch trong Unity Editor
// Tuân thủ đúng danh pháp và chữ ký phương thức theo Docs/SCHEMA_CHI_TIET.md Mục 3.2.
using System;
using UnityEngine;

namespace OneHitDown.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private bool isDead = false;
        [SerializeField] private bool isGrounded = true;
        [SerializeField] private bool isOnIce = false;
        [SerializeField] private Rigidbody2D rb;

        public bool IsDead => isDead;
        public bool IsGrounded => isGrounded;
        public bool IsOnIce => isOnIce;
        public Rigidbody2D Rigidbody => rb != null ? rb : (rb = GetComponent<Rigidbody2D>());

        public event Action<PlayerController> OnPlayerDied;
        public event Action OnSingleJumped;
        public event Action OnDoubleJumped;
        public event Action OnLanded;

        public void GrantDoubleJumpAndBoost(Vector2 boostVelocity)
        {
            if (rb != null)
            {
                rb.linearVelocity = boostVelocity;
            }
        }

        public void AddExternalVelocity(Vector2 extVel)
        {
            if (rb != null)
            {
                rb.linearVelocity += extVel;
            }
        }

        public void SetOnIce(bool onIce)
        {
            isOnIce = onIce;
        }

        public void SetPlatformVelocity(Vector2 platVel)
        {
        }

        public void Die()
        {
            isDead = true;
            OnPlayerDied?.Invoke(this);
        }

        public void Respawn(Vector2 spawnPosition)
        {
            isDead = false;
            isOnIce = false;
            transform.position = spawnPosition;
        }
    }
}

