using UnityEngine;

namespace OneHitDown.Traps
{
    /// <summary>
    /// Quả cầu gai con lắc: xoay quanh trục treo theo hàm Sin.
    /// Cơ chế giữ nguyên theo bản mock đã kiểm chứng trên Level_03 (xoay root,
    /// quả cầu và xích là con cố định ở -armLength), chỉ khác là xích được dựng
    /// lúc Awake thay vì đặt sẵn trong Scene.
    /// </summary>
    public class SpikedBallPendulum : TrapBase
    {
        [SerializeField] private Transform pivotPoint;
        [SerializeField] private float armLength = 9.8f;
        [SerializeField] private float maxAngle = 48f;
        [SerializeField] private float swingSpeed = 2f;
        [SerializeField] private Transform ball;
        [SerializeField] private Sprite chainSprite;

        private SpriteRenderer[] links;
        private float timer;

        protected override void Awake()
        {
            base.Awake();
            if (pivotPoint == null) pivotPoint = transform;

            if (ball != null)
            {
                ball.gameObject.tag = "Hazard";
                ball.gameObject.layer = 10;
                ball.localPosition = new Vector3(0f, -armLength, 0f);
            }

            BuildChain();
            ResetPose();
        }

        private void BuildChain()
        {
            if (chainSprite == null) return;

            float segment = Mathf.Max(0.1f, chainSprite.bounds.size.y);
            int count = Mathf.CeilToInt(armLength / segment);
            links = new SpriteRenderer[count];

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("ChainLink");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = chainSprite;
                sr.sortingOrder = -1;
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, -armLength * (i + 0.5f) / count, 0f);
                links[i] = sr;
            }
        }

        protected override void Update()
        {
            timer += Time.deltaTime;
            float angle = Mathf.Sin(timer * swingSpeed) * maxAngle;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void ResetPose()
        {
            timer = 0f;
            transform.localRotation = Quaternion.identity;
        }

        public override void ResetState()
        {
            base.ResetState();
            ResetPose();
        }
    }
}
