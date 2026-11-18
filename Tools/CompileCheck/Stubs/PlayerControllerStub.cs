// Trần Văn Tuyên, phiên bản 1.0
// Harness stub chỉ dùng để headless compile check ngoài thư mục Assets
namespace OneHitDown.Player
{
    using UnityEngine;

    public class PlayerController : MonoBehaviour
    {
        public bool IsDead => false;
        public Rigidbody2D Rigidbody => GetComponent<Rigidbody2D>();
        public void GrantDoubleJumpAndBoost(Vector2 boostVelocity) {}
        public void AddExternalVelocity(Vector2 extVel) {}
        public void SetOnIce(bool onIce) {}
    }
}
