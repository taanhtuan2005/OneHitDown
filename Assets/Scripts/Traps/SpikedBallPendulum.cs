using UnityEngine;

namespace OneHitDown.Traps
{
    public class SpikedBallPendulum : TrapBase
    {
        [SerializeField] private Transform pivotPoint;
        [SerializeField] private float armLength = 3.5f;
        [SerializeField] private float maxAngle = 65f;
        [SerializeField] private float swingSpeed = 2.5f;
        [SerializeField] private float phaseOffset = 0f;
        [SerializeField] private Transform ball;
        [SerializeField] private Sprite chainSprite;
        private Transform[] links;
        private float elapsedTime;

        protected override void Awake()
        {
            base.Awake();
            if (pivotPoint == null) pivotPoint = transform;
            if (ball != null) { ball.gameObject.tag = "Hazard"; ball.gameObject.layer = 10; }
            int count = chainSprite != null ? Mathf.CeilToInt(armLength / Mathf.Max(0.1f, chainSprite.bounds.size.y)) : 0;
            links = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                var link = new GameObject("ChainLink").AddComponent<SpriteRenderer>();
                link.sprite = chainSprite;
                link.sortingOrder = -1;
                link.transform.SetParent(transform, false);
                links[i] = link.transform;
            }
            UpdatePendulum();
        }

        protected override void Update()
        {
            elapsedTime += Time.deltaTime;
            UpdatePendulum();
        }

        private void UpdatePendulum()
        {
            if (ball == null || pivotPoint == null) return;
            float angle = Mathf.Sin((elapsedTime + phaseOffset) * swingSpeed) * maxAngle;
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.down;
            ball.position = pivotPoint.position + direction * armLength;
            for (int i = 0; i < links.Length; i++)
            {
                links[i].position = pivotPoint.position + direction * (armLength * (i + 0.5f) / links.Length);
                links[i].rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        public override void ResetState()
        {
            base.ResetState();
            elapsedTime = 0f;
            UpdatePendulum();
        }
    }
}
