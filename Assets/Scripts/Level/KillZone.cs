using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class KillZone : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player"))
            {
                return;
            }

            PlayerController player = col.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}
