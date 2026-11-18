// Trần Văn Tuyên, phiên bản 1.0
// Bộ kiểm thử tự động cho FallingPlatform (TC-FP-01 đến TC-FP-06) theo Test Matrix §8.2.
using NUnit.Framework;
using OneHitDown.Player;
using OneHitDown.Traps;
using UnityEngine;

namespace OneHitDown.Tests
{
    [TestFixture]
    public class FallingPlatformTests
    {
        [SetUp]
        public void SetUp()
        {
            TestRuntime.Reset();
        }

        private (GameObject go, FallingPlatform platform, BoxCollider2D col, SpriteRenderer sr, Sprite onSprite, Sprite offSprite)
            CreatePlatform(bool permanent = true, float respawnDuration = 3.0f, float shakeDuration = 0.4f, float fallSpeed = 12f)
        {
            GameObject go = new GameObject("FallingPlatform");
            go.transform.position = new Vector3(10f, 5f, 0f);

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            FallingPlatform platform = go.AddComponent<FallingPlatform>();

            Sprite onSp = Sprite.Create("On");
            Sprite offSp = Sprite.Create("Off");

            TestRuntime.SetField(platform, "solidCollider", col);
            TestRuntime.SetField(platform, "spriteRenderer", sr);
            TestRuntime.SetField(platform, "activeSprite", onSp);
            TestRuntime.SetField(platform, "offSprite", offSp);
            TestRuntime.SetField(platform, "isPermanentInRun", permanent);
            TestRuntime.SetField(platform, "respawnDuration", respawnDuration);
            TestRuntime.SetField(platform, "shakeDuration", shakeDuration);
            TestRuntime.SetField(platform, "fallSpeed", fallSpeed);

            TestRuntime.InvokeLifecycle(platform, "Awake");

            return (go, platform, col, sr, onSp, offSp);
        }

        [Test]
        [Description("TC-FP-01: State Machine Transitions — Bắt đầu tại Solid -> Trigger -> Shaking (0.4s) -> sập và rơi (Falling)")]
        public void TC_FP_01_StateMachineTransitions()
        {
            var (_, platform, col, sr, onSp, offSp) = CreatePlatform();

            Assert.That(platform.IsCollapsing, Is.False, "Ban đầu bệ phải ở trạng thái Solid");
            Assert.That(col.enabled, Is.True, "Collider ban đầu phải bật");

            // Kích hoạt sập
            platform.TriggerCollapse();
            Assert.That(platform.IsCollapsing, Is.True, "Khi kích hoạt, bệ phải chuyển sang Shaking");

            // Tua 0.2s: vẫn đang trong pha rung (shakeDuration = 0.4s)
            TestRuntime.AdvanceTime(0.2f);
            Assert.That(col.enabled, Is.True, "Trong 0.4s đầu bệ vẫn phải đỡ chân player");

            // Tua tiếp 0.25s (tổng 0.45s): đã chuyển sang pha sập rơi
            TestRuntime.AdvanceTime(0.25f);
            Assert.That(col.enabled, Is.False, "Sau 0.4s collider phải tắt");
            Assert.That(sr.sprite, Is.EqualTo(offSp), "Sprite phải đổi sang offSprite");
        }

        [Test]
        [Description("TC-FP-02: Trigger Debounce — Đang ở Shaking nhận tiếp va chạm -> Bỏ qua, không tạo coroutine thứ 2")]
        public void TC_FP_02_TriggerDebounce()
        {
            var (_, platform, _, _, _, _) = CreatePlatform();
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();

            platform.TriggerCollapse();
            Assert.That(platform.IsCollapsing, Is.True);

            // Gửi collision từ trên xuống tiếp
            Collision2D colFromAbove = Collision2D.Create(playerCol, new Vector2(0f, -1f));
            TestRuntime.InvokeLifecycle(platform, "OnCollisionEnter2D", colFromAbove);

            // Bệ vẫn duy trì isCollapsing = true ổn định, không bị reset hay lỗi coroutine
            Assert.That(platform.IsCollapsing, Is.True);
        }

        [Test]
        [Description("TC-FP-03: Bottom Hit Rejection — Va chạm từ dưới (contact.normal.y > 0) -> Không kích hoạt rung")]
        public void TC_FP_03_BottomHitRejection()
        {
            var (_, platform, col, _, _, _) = CreatePlatform();
            var (_, _, playerCol, _) = PlayerController.CreateMockPlayer();

            // Va chạm từ dưới: normal.y = 1f (> -0.5f)
            Collision2D bottomHit = Collision2D.Create(playerCol, new Vector2(0f, 1f));
            TestRuntime.InvokeLifecycle(platform, "OnCollisionEnter2D", bottomHit);

            Assert.That(platform.IsCollapsing, Is.False, "Cú chạm từ dưới không được kích hoạt bệ sập");
            Assert.That(col.enabled, Is.True);
        }

        [Test]
        [Description("TC-FP-04: Permanent Collapse — isPermanentInRun = true -> Sau khi rơi không tự hồi")]
        public void TC_FP_04_PermanentCollapse()
        {
            var (go, platform, col, sr, _, _) = CreatePlatform(permanent: true, respawnDuration: 3f, fallSpeed: 12f);

            platform.TriggerCollapse();

            // Tua 0.4s rung + khoảng 2s rơi (20 units / 12 units/s ~ 1.7s) + 5s chờ
            TestRuntime.AdvanceTime(10f);

            Assert.That(col.enabled, Is.False, "Bệ sập vĩnh viễn không được tự bật collider");
            Assert.That(sr.enabled, Is.False, "Renderer phải tắt sau khi rơi khỏi khung hình");
            Assert.That(platform.IsCollapsing, Is.True, "Bệ vẫn ghi nhận trạng thái đã sập");
        }

        [Test]
        [Description("TC-FP-05: Training Respawn Mode — isPermanentInRun = false, respawnDuration = 3f -> Tự hồi về Solid tại startPosition sau 3s")]
        public void TC_FP_05_TrainingRespawnMode()
        {
            var (go, platform, col, sr, onSp, _) = CreatePlatform(permanent: false, respawnDuration: 3f, fallSpeed: 12f);
            Vector3 startPos = go.transform.position;

            platform.TriggerCollapse();

            // Tua qua pha rung 0.4s + rơi ~1.7s (tổng 2.5s)
            TestRuntime.AdvanceTime(2.5f);
            Assert.That(col.enabled, Is.False, "Bệ đã rơi");

            // Tua tiếp 3.5s (quá respawnDuration = 3s)
            TestRuntime.AdvanceTime(3.5f);

            Assert.That(platform.IsCollapsing, Is.False, "Bệ phải tự phục hồi về Solid");
            Assert.That(col.enabled, Is.True, "Collider phải bật lại");
            Assert.That(sr.enabled, Is.True, "SpriteRenderer phải bật lại");
            Assert.That(sr.sprite, Is.EqualTo(onSp), "Sprite phải là activeSprite");
            Assert.That(go.transform.position, Is.EqualTo(startPos), "Vị trí phải trả lại startPosition");
        }

        [Test]
        [Description("TC-FP-06: Idempotent ResetState — Gọi ResetState() 5 lần liên tiếp -> Trạng thái luôn là Solid, vị trí startPosition, collider bật")]
        public void TC_FP_06_IdempotentResetState()
        {
            var (go, platform, col, sr, onSp, _) = CreatePlatform();
            Vector3 startPos = go.transform.position;

            platform.TriggerCollapse();
            TestRuntime.AdvanceTime(0.5f);

            for (int i = 0; i < 5; i++)
            {
                platform.ResetState();
                Assert.That(platform.IsCollapsing, Is.False);
                Assert.That(col.enabled, Is.True);
                Assert.That(sr.enabled, Is.True);
                Assert.That(sr.sprite, Is.EqualTo(onSp));
                Assert.That(go.transform.position, Is.EqualTo(startPos));
            }
        }
    }
}