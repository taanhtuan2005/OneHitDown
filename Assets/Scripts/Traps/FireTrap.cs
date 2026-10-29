using System.Collections;
using OneHitDown.Core;
using UnityEngine;

namespace OneHitDown.Traps
{
    public class FireTrap : MonoBehaviour, IResettable
    {
        [SerializeField, Min(0.01f)] private float cooldownTime = 2f;
        [SerializeField, Min(0.01f)] private float warningTime = 0.35f;
        [SerializeField, Min(0.01f)] private float activeTime = 2f;
        [SerializeField] private Collider2D flameHitbox;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Sprite[] warningFrames;
        [SerializeField] private Sprite[] activeFrames;
        private SpriteRenderer sr;
        private Coroutine cycle;
        public bool IsActive => flameHitbox != null && flameHitbox.gameObject.activeSelf;
        public bool IsWarning { get; private set; }

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            gameObject.tag = "Untagged";
            gameObject.layer = 10;
            if (flameHitbox != null)
            {
                flameHitbox.gameObject.tag = "Hazard";
                flameHitbox.gameObject.layer = 10;
                flameHitbox.isTrigger = true;
                flameHitbox.gameObject.SetActive(false);
            }
        }

        private void OnEnable() => StartCycle();
        private void OnDisable()
        {
            if (cycle != null) StopCoroutine(cycle);
            cycle = null;
            SetFlame(false);
            IsWarning = false;
            if (sr != null) sr.enabled = true;
        }

        public void StartCycle()
        {
            if (cycle != null) StopCoroutine(cycle);
            SetFlame(false);
            IsWarning = false;
            if (sr != null) sr.enabled = true;
            if (sr != null && offSprite != null) sr.sprite = offSprite;
            if (isActiveAndEnabled) cycle = StartCoroutine(FireCycleRoutine());
        }

        private IEnumerator FireCycleRoutine()
        {
            while (true)
            {
                SetFlame(true);
                yield return AnimatePhase(activeFrames, activeTime);
                SetFlame(false);
                if (sr != null && offSprite != null) sr.sprite = offSprite;
                // Cảnh báo nằm TRONG 2 giây tắt, không kéo dài chu kỳ thành 4.35 giây.
                float warningDuration = Mathf.Clamp(warningTime, 0f, cooldownTime);
                float restDuration = cooldownTime - warningDuration;
                if (restDuration > 0f) yield return new WaitForSeconds(restDuration);
                IsWarning = true;
                yield return AnimatePhase(warningFrames, warningDuration, true);
                IsWarning = false;
                if (sr != null) sr.enabled = true;
            }
        }

        private IEnumerator AnimatePhase(Sprite[] frames, float duration, bool blink = false)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (sr != null) sr.enabled = !blink || (int)(elapsed / 0.05f) % 2 == 0;
                if (sr != null && frames != null && frames.Length > 0)
                    sr.sprite = frames[Mathf.Min((int)(elapsed / duration * frames.Length), frames.Length - 1)];
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void SetFlame(bool active)
        {
            if (flameHitbox != null) flameHitbox.gameObject.SetActive(active);
        }

        // Khởi động lại từ pha bật 2 giây; child Hazard chỉ hoạt động khi đang phun lửa.
        public void ResetState() => StartCycle();
    }
}
