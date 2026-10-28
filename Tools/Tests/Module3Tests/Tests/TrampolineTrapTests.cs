// Trần Văn Tuyên, phiên bản 1.0
// Bộ kiểm thử tự động cho TrampolineTrap (TC-TR-01 đến TC-TR-05) theo Test Matrix §8.2.
using NUnit.Framework;
using OneHitDown.Player;
using OneHitDown.Traps;
using UnityEngine;

namespace OneHitDown.Tests
{
    [TestFixture]
    public class TrampolineTrapTests
    {
        [SetUp]
        public void SetUp()
        {
            TestRuntime.Reset();
        }

        private (GameObject go, TrampolineTrap trap, BoxCollider2D col, Animator anim) CreateTrampoline(
            Vector2 direction,
            float force = 20f)
        {
            GameObject go = new GameObject("Trampoline");
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            Animator anim = go.AddComponent<Animator>();
            TrampolineTrap trap = go.AddComponent<TrampolineTrap>();

            TestRuntime.SetField(trap, "bounceDirection", direction);
            TestRuntime.SetField(trap, "bounceForce", force);
            TestRuntime.SetField(trap, "animator", anim);

            TestRuntime.InvokeLifecycle(trap, "Awake");

            return (go, trap, col, anim);
        }

        [Test]
        [Description("TC-TR-01: Vector Normalization — bounceDirection (3, 4) được chuẩn hóa thành độ dài 1.0f và hướng (0.6, 0.8)")]
        public void TC_TR_01_VectorNormalization()
        {
            var (_, trap, _, _) = CreateTrampoline(new Vector2(3f, 4f), 20f);

            Vector2 dir = trap.BounceDirection;

            Assert.That(Mathf.Approximately(dir.magnitude, 1f), Is.True, $"Độ dài vector phải là 1.0f nhưng thực tế là {dir.magnitude}");
            Assert.That(Mathf.Approximately(dir.x, 0.6f), Is.True, $"X kỳ vọng 0.6f nhưng là {dir.x}");
            Assert.That(Mathf.Approximately(dir.y, 0.8f), Is.True, $"Y kỳ vọng 0.8f nhưng là {dir.y}");
        }

        [Test]
        [Description("TC-TR-02: Zero Vector Fallback — bounceDirection (0, 0) tự fallback về Vector2.up (0, 1), không gây chia cho 0")]
        public void TC_TR_02_ZeroVectorFallback()
        {
            var (_, trap, _, _) = CreateTrampoline(Vector2.zero, 20f);

            Vector2 dir = trap.BounceDirection;

            Assert.That(dir, Is.EqualTo(Vector2.up), "Khi truyền Vector2.zero, bounceDirection phải tự fallback về Vector2.up");
            Assert.That(trap.BounceForce, Is.EqualTo(20f));
        }

        [Test]
        [Description("TC-TR-03: Boost Velocity Calculation — direction = (0, 1), force = 20f -> velocity == (0, 20f)")]
        public void TC_TR_03_BoostVelocityCalculation()
        {
            var (_, trap, _, _) = CreateTrampoline(Vector2.up, 20f);

            Vector2 calcVel = trap.BounceDirection * trap.BounceForce;

            Assert.That(calcVel.x, Is.EqualTo(0f));
            Assert.That(calcVel.y, Is.EqualTo(20f));
        }

        [Test]
        [Description("TC-TR-04: Dead Player Rejection — player.IsDead = true không được nhận lực đẩy")]
        public void TC_TR_04_DeadPlayerRejection()
        {
            var (_, trap, _, _) = CreateTrampoline(Vector2.up, 20f);
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();
            player.IsDead = true;

            TestRuntime.InvokeLifecycle(trap, "OnTriggerEnter2D", playerCol);

            Assert.That(player.BoostCount, Is.EqualTo(0), "Player đã chết không được gọi GrantDoubleJumpAndBoost");
        }

        [Test]
        [Description("TC-TR-05: Alive Player Trigger — player.IsDead = false nhận đúng 1 lần boost với vector (0, 20f) và animation Jump")]
        public void TC_TR_05_AlivePlayerTrigger()
        {
            var (_, trap, _, anim) = CreateTrampoline(Vector2.up, 20f);
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();
            player.IsDead = false;

            TestRuntime.InvokeLifecycle(trap, "OnTriggerEnter2D", playerCol);

            Assert.That(player.BoostCount, Is.EqualTo(1), "Player sống phải được boost đúng 1 lần");
            Assert.That(player.BoostHistory[0].x, Is.EqualTo(0f));
            Assert.That(player.BoostHistory[0].y, Is.EqualTo(20f));
            Assert.That(anim.CurrentState, Is.EqualTo("Jump"), "Animator phải chuyển sang clip Jump");
        }

        [Test]
        [Description("TC-TR-10 (Edge): No Animator Tolerance — Khi animator == null, đệm nhún vẫn đẩy player an toàn không văng exception")]
        public void TC_TR_10_NoAnimatorTolerance()
        {
            GameObject go = new GameObject("Trampoline_NoAnim");
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            TrampolineTrap trap = go.AddComponent<TrampolineTrap>();

            TestRuntime.SetField(trap, "bounceDirection", Vector2.up);
            TestRuntime.SetField(trap, "bounceForce", 20f);
            TestRuntime.SetField(trap, "animator", null);

            TestRuntime.InvokeLifecycle(trap, "Awake");

            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();
            Assert.DoesNotThrow(() => TestRuntime.InvokeLifecycle(trap, "OnTriggerEnter2D", playerCol));

            Assert.That(player.BoostCount, Is.EqualTo(1));
            Assert.That(player.BoostHistory[0].y, Is.EqualTo(20f));
        }

        [Test]
        [Description("TC-TR-11 (Edge): Awake Auto Finds Animator — Khi không gán trong Inspector, Awake() tự GetComponent<Animator>()")]
        public void TC_TR_11_AwakeAutoFindsAnimator()
        {
            GameObject go = new GameObject("Trampoline_AutoFind");
            Animator animOnGo = go.AddComponent<Animator>();
            TrampolineTrap trap = go.AddComponent<TrampolineTrap>();

            TestRuntime.SetField(trap, "animator", null);
            TestRuntime.InvokeLifecycle(trap, "Awake");

            Animator resolved = TestRuntime.GetField<Animator>(trap, "animator");
            Assert.That(resolved, Is.Not.Null, "Awake phải tự cache Animator từ GameObject");
            Assert.That(resolved, Is.SameAs(animOnGo));
        }

        [Test]
        [Description("TC-TR-12 (Edge): Negative Force Reset — bounceForce <= 0 tự reset về DefaultBounceForce (20f)")]
        public void TC_TR_12_NegativeForceReset()
        {
            GameObject go = new GameObject("Trampoline_BadForce");
            TrampolineTrap trap = go.AddComponent<TrampolineTrap>();

            TestRuntime.SetField(trap, "bounceForce", -5f);
            TestRuntime.InvokeLifecycle(trap, "Awake");

            Assert.That(trap.BounceForce, Is.EqualTo(20f), "Lực bật âm phải tự reset về 20f");
        }
    }
}