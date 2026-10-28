// Trần Văn Tuyên, phiên bản 1.0
// Bộ kiểm thử tự động cho FanTrap (TC-FA-01 đến TC-FA-06) theo Test Matrix §8.2.
using NUnit.Framework;
using OneHitDown.Player;
using OneHitDown.Traps;
using UnityEngine;

namespace OneHitDown.Tests
{
    [TestFixture]
    public class FanTrapTests
    {
        [SetUp]
        public void SetUp()
        {
            TestRuntime.Reset();
        }

        private (GameObject go, FanTrap fan, BoxCollider2D col, SpriteRenderer sr) CreateFan(
            Vector2 direction,
            float force = 15f,
            bool isActive = true)
        {
            GameObject go = new GameObject("FanTrap");
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            FanTrap fan = go.AddComponent<FanTrap>();

            Sprite[] frames = new Sprite[]
            {
                Sprite.Create("Frame0"),
                Sprite.Create("Frame1"),
                Sprite.Create("Frame2"),
                Sprite.Create("Frame3")
            };
            Sprite off = Sprite.Create("Off");

            TestRuntime.SetField(fan, "windDirection", direction);
            TestRuntime.SetField(fan, "windForce", force);
            TestRuntime.SetField(fan, "isActive", isActive);
            TestRuntime.SetField(fan, "animationFrames", frames);
            TestRuntime.SetField(fan, "offSprite", off);
            TestRuntime.SetField(fan, "spriteRenderer", sr);

            TestRuntime.InvokeLifecycle(fan, "Awake");

            return (go, fan, col, sr);
        }

        [Test]
        [Description("TC-FA-01: Upward Wind Velocity Math — vy = 2f, windDir = (0, 1), force = 15f -> newVy = max(2f, 15f) == 15f")]
        public void TC_FA_01_UpwardWindVelocityMath()
        {
            var (_, fan, _, _) = CreateFan(Vector2.up, 15f, true);
            var (_, player, playerCol, rb) = PlayerController.CreateMockPlayer();
            rb.linearVelocity = new Vector2(0f, 2f);

            TestRuntime.InvokeLifecycle(fan, "OnTriggerStay2D", playerCol);

            Assert.That(rb.linearVelocity.y, Is.EqualTo(15f), "newVy phải nâng lên bằng 15f");
        }

        [Test]
        [Description("TC-FA-02: Upward Wind Clamping — vy = 18f, windDir = (0, 1), force = 15f -> newVy = max(18f, 15f) == 18f (không giảm tốc khi đang nhảy cao hơn)")]
        public void TC_FA_02_UpwardWindClamping()
        {
            var (_, fan, _, _) = CreateFan(Vector2.up, 15f, true);
            var (_, player, playerCol, rb) = PlayerController.CreateMockPlayer();
            rb.linearVelocity = new Vector2(0f, 18f);

            TestRuntime.InvokeLifecycle(fan, "OnTriggerStay2D", playerCol);

            Assert.That(rb.linearVelocity.y, Is.EqualTo(18f), "newVy không được bị kéo tụt xuống khi đang nhảy cao hơn sức gió");
        }

        [Test]
        [Description("TC-FA-03: Horizontal Wind Force Math — windDir = (1, 0), force = 15f -> extVx == 15f * 0.35f == 5.25f")]
        public void TC_FA_03_HorizontalWindForceMath()
        {
            var (_, fan, _, _) = CreateFan(Vector2.right, 15f, true);
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();

            TestRuntime.InvokeLifecycle(fan, "OnTriggerStay2D", playerCol);

            Assert.That(player.ExternalVelocityCount, Is.EqualTo(1));
            Assert.That(Mathf.Approximately(player.ExternalVelocityHistory[0].x, 5.25f), Is.True,
                $"extVx kỳ vọng 5.25f nhưng thực tế là {player.ExternalVelocityHistory[0].x}");
            Assert.That(player.ExternalVelocityHistory[0].y, Is.EqualTo(0f));
        }

        [Test]
        [Description("TC-FA-04: Horizontal Reverse Wind Math — windDir = (-1, 0), force = 15f -> extVx == -5.25f")]
        public void TC_FA_04_HorizontalReverseWindMath()
        {
            var (_, fan, _, _) = CreateFan(Vector2.left, 15f, true);
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();

            TestRuntime.InvokeLifecycle(fan, "OnTriggerStay2D", playerCol);

            Assert.That(player.ExternalVelocityCount, Is.EqualTo(1));
            Assert.That(Mathf.Approximately(player.ExternalVelocityHistory[0].x, -5.25f), Is.True,
                $"extVx ngược chiều kỳ vọng -5.25f nhưng thực tế là {player.ExternalVelocityHistory[0].x}");
        }

        [Test]
        [Description("TC-FA-05: Inactive Fan Rejection — isActive = false -> không tác dụng lực lên player")]
        public void TC_FA_05_InactiveFanRejection()
        {
            var (_, fan, _, _) = CreateFan(Vector2.up, 15f, false);
            var (_, player, playerCol, rb) = PlayerController.CreateMockPlayer();
            rb.linearVelocity = new Vector2(0f, 2f);

            TestRuntime.InvokeLifecycle(fan, "OnTriggerStay2D", playerCol);

            Assert.That(rb.linearVelocity.y, Is.EqualTo(2f), "Quạt tắt không được can thiệp linearVelocity của player");
            Assert.That(player.ExternalVelocityCount, Is.EqualTo(0), "Quạt tắt không được truyền ngoại lực");
        }

        [Test]
        [Description("TC-FA-06: Zero Vector Guard — windDirection = (0, 0) tự fallback về (0, 1)")]
        public void TC_FA_06_ZeroVectorGuard()
        {
            var (_, fan, _, _) = CreateFan(Vector2.zero, 15f, true);

            Assert.That(fan.WindDirection, Is.EqualTo(Vector2.up), "Khi truyền Vector2.zero, windDirection phải tự fallback về Vector2.up");
        }

        [Test]
        [Description("TC-FA-07 (Edge): Null Player Guard — Collider có tag Player nhưng không có PlayerController -> Bỏ qua an toàn, không exception")]
        public void TC_FA_07_NullPlayerGuard()
        {
            var (_, fan, _, _) = CreateFan(Vector2.up, 15f, true);
            GameObject fake = new GameObject("FakePlayer") { tag = "Player" };
            BoxCollider2D fakeCol = fake.AddComponent<BoxCollider2D>();

            Assert.DoesNotThrow(() => TestRuntime.InvokeLifecycle(fan, "OnTriggerStay2D", fakeCol));
        }

        [Test]
        [Description("TC-FA-08 (Edge): Dead Player Guard — Player đã chết trong vùng gió không bị tác dụng lực")]
        public void TC_FA_08_DeadPlayerGuard()
        {
            var (_, fan, _, _) = CreateFan(Vector2.up, 15f, true);
            var (_, player, playerCol, rb) = PlayerController.CreateMockPlayer();
            player.IsDead = true;
            rb.linearVelocity = new Vector2(0f, 1f);

            TestRuntime.InvokeLifecycle(fan, "OnTriggerStay2D", playerCol);

            Assert.That(rb.linearVelocity.y, Is.EqualTo(1f), "Player chết không bị gió đẩy");
            Assert.That(player.ExternalVelocityCount, Is.EqualTo(0));
        }

        [Test]
        [Description("TC-FA-09 (Edge): Awake Auto Finds Renderer — spriteRenderer null trong Inspector -> Awake() tự GetComponent")]
        public void TC_FA_09_AwakeAutoFindsRenderer()
        {
            GameObject go = new GameObject("Fan_AutoFind");
            SpriteRenderer srOnGo = go.AddComponent<SpriteRenderer>();
            FanTrap fan = go.AddComponent<FanTrap>();

            TestRuntime.SetField(fan, "spriteRenderer", null);
            TestRuntime.InvokeLifecycle(fan, "Awake");

            SpriteRenderer resolved = TestRuntime.GetField<SpriteRenderer>(fan, "spriteRenderer");
            Assert.That(resolved, Is.Not.Null);
            Assert.That(resolved, Is.SameAs(srOnGo));
        }

        [Test]
        [Description("TC-FA-10 (Edge): Negative Force Reset — windForce <= 0 tự reset về DefaultWindForce (15f)")]
        public void TC_FA_10_NegativeForceReset()
        {
            GameObject go = new GameObject("Fan_BadForce");
            FanTrap fan = go.AddComponent<FanTrap>();

            TestRuntime.SetField(fan, "windForce", -10f);
            TestRuntime.InvokeLifecycle(fan, "Awake");

            Assert.That(fan.WindForce, Is.EqualTo(15f), "windForce âm phải tự reset về 15f");
        }
    }
}