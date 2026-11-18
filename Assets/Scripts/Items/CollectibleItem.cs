using OneHitDown.Core;
using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.Items
{
    public enum FruitType
    {
        Apple,
        Bananas,
        Kiwi,
        Orange,
        Pineapple,
        Cherries,
        Melon,
        Strawberry
    }

    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
    public sealed class CollectibleItem : MonoBehaviour, IResettable
    {
        [SerializeField] private FruitType fruitType = FruitType.Apple;
        [SerializeField] private bool countsAsRelic = true;
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private GameObject collectedVfxPrefab;

        private SpriteRenderer spriteRenderer;
        private Collider2D itemCollider;
        private bool isCollected;

        public FruitType Type => fruitType;
        public int ScoreValue => scoreValue;
        public bool CountsAsRelic => countsAsRelic;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            itemCollider = GetComponent<Collider2D>();
            itemCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (isCollected || !col.CompareTag("Player"))
            {
                return;
            }

            Collect();
        }

        public void Collect()
        {
            if (isCollected)
            {
                return;
            }

            isCollected = true;
            if (collectedVfxPrefab != null)
            {
                Instantiate(collectedVfxPrefab, transform.position, Quaternion.identity);
            }

            spriteRenderer.enabled = false;
            itemCollider.enabled = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRelicCollected(scoreValue, countsAsRelic);
            }
        }

        public void ResetState()
        {
            isCollected = false;
            spriteRenderer.enabled = true;
            itemCollider.enabled = true;
        }
    }
}
