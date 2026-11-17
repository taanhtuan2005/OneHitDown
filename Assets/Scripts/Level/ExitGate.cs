using System.Collections.Generic;
using OneHitDown.Core;
using OneHitDown.Player;
using OneHitDown.UI;
using TMPro;
using UnityEngine;

namespace OneHitDown.Level
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class ExitGate : MonoBehaviour, IResettable
    {
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private ParticleSystem confettiVfx;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip unlockClip;
        [SerializeField] private AudioClip blockedClip;

        private readonly HashSet<PlayerController> playersInGate = new HashSet<PlayerController>();
        private Collider2D gateCollider;
        private SpriteRenderer spriteRenderer;
        private bool isUnlocked;

        public bool IsUnlocked => isUnlocked;

        private void Awake()
        {
            gateCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            gateCollider.isTrigger = true;
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            UpdateProgress(0, GameManager.Instance != null ? GameManager.Instance.TotalRelicsRequired : 3);
        }

        public void UpdateProgress(int collected, int required)
        {
            int safeRequired = Mathf.Max(1, required);
            int safeCollected = Mathf.Clamp(collected, 0, safeRequired);
            if (statusLabel != null)
            {
                statusLabel.text = isUnlocked
                    ? $"[OPEN {safeRequired}/{safeRequired}]"
                    : $"[LOCKED {safeCollected}/{safeRequired}]";
                statusLabel.color = isUnlocked
                    ? new Color(0.35f, 1f, 0.48f, 1f)
                    : new Color(1f, 0.42f, 0.42f, 1f);
            }

            // Cúp giữ nguyên màu gốc ở mọi trạng thái; chỉ label đổi màu.
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }

        public void UnlockGate()
        {
            if (isUnlocked)
            {
                return;
            }

            isUnlocked = true;
            int required = GameManager.Instance != null ? GameManager.Instance.TotalRelicsRequired : 3;
            UpdateProgress(required, required);
            if (confettiVfx != null)
            {
                confettiVfx.Play();
            }

            PlayClip(unlockClip);
            CheckCompletion();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player"))
            {
                return;
            }

            PlayerController player = col.GetComponentInParent<PlayerController>();
            if (player == null || player.IsDead)
            {
                return;
            }

            playersInGate.Add(player);
            if (!isUnlocked)
            {
                PlayClip(blockedClip);
                return;
            }

            CheckCompletion();
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            PlayerController player = col.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                playersInGate.Remove(player);
            }
        }

        private void CheckCompletion()
        {
            if (!isUnlocked || GameManager.Instance == null)
            {
                return;
            }

            int livingPlayersInGate = 0;
            foreach (PlayerController player in playersInGate)
            {
                if (player != null && !player.IsDead)
                {
                    livingPlayersInGate++;
                }
            }

            int requiredPlayers = GameSession.IsTwoPlayerMode ? 2 : 1;
            if (livingPlayersInGate >= requiredPlayers)
            {
                GameManager.Instance.OnLevelCompleted();
            }
        }

        public void ResetState()
        {
            isUnlocked = false;
            playersInGate.Clear();
            UpdateProgress(0, 3);
            if (confettiVfx != null)
            {
                confettiVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
