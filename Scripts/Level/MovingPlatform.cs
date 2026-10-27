using System.Collections.Generic;
using OneHitDown.Core;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MovingPlatform : MonoBehaviour, IResettable
    {
        [SerializeField] private Vector2 moveOffset = new Vector2(0f, 3f);
        [SerializeField] private float moveSpeed = 2f;

        private readonly HashSet<PlayerController> passengers = new HashSet<PlayerController>();
        private Vector3 startPos;
        private Vector2 currentVelocity;
        private float elapsed;

        private void Awake()
        {
            startPos = transform.position;
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;
            float travel = Mathf.PingPong(elapsed * moveSpeed, 1f);
            Vector3 destination = startPos + new Vector3(moveOffset.x, moveOffset.y, 0f) * travel;
            Vector3 previousPosition = transform.position;
            currentVelocity = (destination - previousPosition) / Time.fixedDeltaTime;
            transform.position = destination;

            foreach (PlayerController passenger in passengers)
            {
                if (passenger != null && !passenger.IsDead)
                {
                    passenger.SetPlatformVelocity(currentVelocity);
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.contactCount == 0 || col.GetContact(0).normal.y < 0.5f)
            {
                return;
            }

            PlayerController player = col.collider.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                passengers.Add(player);
                player.SetPlatformVelocity(currentVelocity);
            }
        }

        private void OnCollisionExit2D(Collision2D col)
        {
            PlayerController player = col.collider.GetComponentInParent<PlayerController>();
            if (player != null && passengers.Remove(player))
            {
                player.SetPlatformVelocity(Vector2.zero);
            }
        }

        public void ResetState()
        {
            foreach (PlayerController passenger in passengers)
            {
                if (passenger != null)
                {
                    passenger.SetPlatformVelocity(Vector2.zero);
                }
            }

            passengers.Clear();
            elapsed = 0f;
            currentVelocity = Vector2.zero;
            transform.position = startPos;
        }
    }
}
