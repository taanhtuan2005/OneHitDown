using UnityEngine;

namespace OneHitDown.Traps
{
    public class SawSpinner : TrapBase
    {
        [SerializeField] private float rotationSpeed = 720f; // Độ/giây.
        [SerializeField] private Sprite chainSprite;
        private Quaternion startRotation;
        private Transform rail;

        protected override void Awake()
        {
            base.Awake();
            startRotation = transform.rotation;
            if (chainSprite == null || patrolMode == PatrolMode.None || patrolRange <= 0f) return;
            // Ray là con của prefab nhưng giữ tọa độ thế giới, không xoay theo cưa.
            rail = new GameObject("ChainRail").transform;
            rail.SetParent(transform, false);
            float spacing = Mathf.Max(0.1f, chainSprite.bounds.size.y);
            int count = Mathf.CeilToInt(patrolRange / spacing);
            for (int i = 0; i <= count; i++)
            {
                var link = new GameObject("ChainLink").AddComponent<SpriteRenderer>();
                link.sprite = chainSprite;
                link.sortingOrder = -1;
                link.transform.SetParent(rail, false);
                link.transform.localPosition = PatrolDirection * (patrolRange * i / count);
                link.transform.localRotation = patrolAxis == PatrolAxis.Horizontal ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
            }
            UpdateRail();
        }

        protected override void Update()
        {
            base.Update();
            transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
            UpdateRail();
        }

        private void UpdateRail()
        {
            if (rail != null) rail.SetPositionAndRotation(startPosition, Quaternion.identity);
        }

        public override void ResetState()
        {
            base.ResetState();
            transform.rotation = startRotation;
            UpdateRail();
        }
    }
}
