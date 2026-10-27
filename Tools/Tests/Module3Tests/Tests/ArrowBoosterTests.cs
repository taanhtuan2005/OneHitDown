// Trần Văn Tuyên, phiên bản 1.0
// Bộ kiểm thử tự động cho ArrowBooster (TC-AB-01 đến TC-AB-05 + ca bổ sung) theo Test Matrix §8.2.
using NUnit.Framework;
using OneHitDown.Player;
using OneHitDown.Traps;
using UnityEngine;

namespace OneHitDown.Tests
{
    [TestFixture]
    public class ArrowBoosterTests
    {
        [SetUp]
        public void SetUp()
        {
            TestRuntime.Reset();
        }

        private (GameObject go, ArrowBooster booster, CircleCollider2D col, Animator anim, SpriteRenderer sr)
            CreateBooster(Vector2 direction = default, float force = 14f, float delay = 2.5f)
        {
            GameObject go = new GameObject("ArrowBooster");

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            Animator anim = go.AddComponent<Animator>();
            ArrowBooster booster = go.AddComponent<ArrowBooster>();

            TestRuntime.SetField(booster, "boostDirection", direction);
            TestRuntime.SetField(booster, "boostForce", force);
            TestRuntime.SetField(booster, "reactivateDelay", delay);
            TestRuntime.SetField(booster, "boosterCollider", col);
            TestRuntime.SetField(booster, "spriteRenderer", sr);
            TestRuntime.SetField(booster, "animator", anim);

            TestRuntime.InvokeLifecycle(booster, "Awake");

            return (go, booster, col, anim, sr);
        }

        [Test]
        [Description("TC-AB-01: Boost Vector Math — dir = (0, 1), force = 14f -> vector truyền cho player == (0, 14f)")]
        public void TC_AB_01_BoostVectorMath()
        {
            var (_, booster, _, _, _) = CreateBooster(Vector2.up, 14f);
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();

            Assert.That(booster.IsReady, Is.True, "Mũi tên khởi tạo phải ở trạng thái sẵn sàng");

            TestRuntime.InvokeLifecycle(booster, "OnTriggerEnter2D", playerCol);

            Assert.That(player.BoostCount, Is.EqualTo(1), "Phải gọi GrantDoubleJumpAndBoost đúng 1 lần");
            Vector2 applied = player.BoostHistory[0];
            Assert.That(applied.x, Is.EqualTo(0f), $"Thành phần X kỳ vọng 0f nhưng là {applied.x}");
            Assert.That(Mathf.Approximately(applied.y, 14f), Is.True, $"Thành phần Y kỳ vọng 14f nhưng là {applied.y}");
        }

        [Test]
        [Description("TC-AB-02: Double Jump Granted — Player chạm mũi tên khi isReady = true -> GrantDoubleJumpAndBoost được gọi đúng 1 lần")]
        public void TC_AB_02_DoubleJumpGranted()
        {
            var (_, booster, _, _, _) = CreateBooster(Vector2.up, 14f);
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();

            Assert.That(booster.IsReady, Is.True);

            TestRuntime.InvokeLifecycle(booster, "OnTriggerEnter2D", playerCol);

            Assert.That(player.BoostCount, Is.EqualTo(1), "Hồi Nhảy Đôi phải được gọi đúng 1 lần, không gọi lặp");
            Assert.That(booster.IsReady, Is.False, "Sau khi hồi Nhảy Đôi, mũi tên phải vào hồi chiêu");
        }

        [Test]
        [Description("TC-AB-03: Cooldown Lockout — Chạm lần 2 trong 2.5s cooldown -> bỏ qua hoàn toàn, không gọi lại GrantDoubleJumpAndBoost")]
        public void TC_AB_03_CooldownLockout()
        {
            var (_, booster, _, _, _) = CreateBooster(Vector2.up, 14f, 2.5f);
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();

            TestRuntime.InvokeLifecycle(booster, "OnTriggerEnter2D", playerCol);
            Assert.That(player.BoostCount, Is.EqualTo(1));

            // Chạm lần 2 sau 0.5s (vẫn trong 2.5s hồi chiêu)
            TestRuntime.AdvanceTime(0.5f);
            TestRuntime.InvokeLifecycle(booster, "OnTriggerEnter2D", playerCol);

            Assert.That(player.BoostCount, Is.EqualTo(1), "Trong thời gian hồi chiêu không được hồi Nhảy Đôi thêm");
            Assert.That(booster.IsReady, Is.False, "Mũi tên vẫn phải ở trạng thái hồi chiêu");

            // Gọi trực tiếp qua API công khai cũng phải bị chặn bởi cờ isReady
            booster.TriggerBoost(player);
            Assert.That(player.BoostCount, Is.EqualTo(1), "TriggerBoost khi đang hồi chiêu phải là no-op");
        }

        [Test]
        [Description("TC-AB-04: Reactivation After Delay — Sau 2.5s -> isReady == true, collider và renderer được bật lại")]
        public void TC_AB_04_ReactivationAfterDelay()
        {
            var (_, booster, col, _, sr) = CreateBooster(Vector2.up, 14f, 2.5f);
            var (_, _, playerCol, _) = PlayerController.CreateMockPlayer();

            TestRuntime.InvokeLifecycle(booster, "OnTriggerEnter2D", playerCol);
            Assert.That(booster.IsReady, Is.False);
            Assert.That(col.enabled, Is.False, "Collider phải tắt ngay khi kích hoạt");

            // Sau 1.0s: vẫn đang hồi chiêu, renderer đã ẩn sau clip Hit 0.2s
            TestRuntime.AdvanceTime(1f);
            Assert.That(booster.IsReady, Is.False, "Chưa đủ 2.5s thì chưa được hồi");
            Assert.That(col.enabled, Is.False);
            Assert.That(sr.enabled, Is.False, "Trong thời gian hồi, mũi tên phải ẩn");

            // Tua qua mốc 2.5s
            TestRuntime.AdvanceTime(1.6f);

            Assert.That(booster.IsReady, Is.True, "Sau reactivateDelay mũi tên phải sẵn sàng lại");
            Assert.That(col.enabled, Is.True, "Collider phải bật lại để nhận va chạm");
            Assert.That(sr.enabled, Is.True, "Renderer phải bật lại để hiển thị mũi tên");
        }

        [Test]
        [Description("TC-AB-05: Instant ResetState — Gọi ResetState() khi đang cooldown 1.0s -> ngay lập tức isReady = true, collider bật, không cần đợi nốt 1.5s")]
        public void TC_AB_05_InstantResetState()
        {
            var (_, booster, col, anim, sr) = CreateBooster(Vector2.up, 14f, 2.5f);
            var (_, _, playerCol, _) = PlayerController.CreateMockPlayer();

            TestRuntime.InvokeLifecycle(booster, "OnTriggerEnter2D", playerCol);

            // Đang ở giây thứ 1.0 của cooldown 2.5s
            TestRuntime.AdvanceTime(1f);
            Assert.That(booster.IsReady, Is.False);

            booster.ResetState();

            Assert.That(booster.IsReady, Is.True, "ResetState phải hồi ngay lập tức, không đợi nốt 1.5s");
            Assert.That(col.enabled, Is.True, "Collider phải bật lại ngay");
            Assert.That(sr.enabled, Is.True, "Renderer phải bật lại ngay");
            Assert.That(anim.CurrentState, Is.EqualTo("Idle"), "Animator phải trở về clip Idle");

            // Coroutine cũ đã bị hủy: tua tiếp quá mốc 2.5s không được làm hỏng trạng thái
            TestRuntime.AdvanceTime(3f);
            Assert.That(booster.IsReady, Is.True, "Trạng thái Ready phải ổn định sau khi reset");

            // ResetState idempotent: gọi lại nhiều lần vẫn cho cùng kết quả
            booster.ResetState();
            booster.ResetState();
            Assert.That(booster.IsReady, Is.True);
            Assert.That(col.enabled, Is.True);
            Assert.That(sr.enabled, Is.True);
        }

        [Test]
        [Description("TC-AB-EXT-01 (bổ sung, ngoài matrix): Visual Hit Cycle — Kích hoạt -> Animator chạy Hit -> sau 0.2s renderer ẩn -> hồi xong bật lại + clip Idle")]
        public void TC_AB_EXT_01_VisualHitCycle()
        {
            var (_, booster, _, anim, sr) = CreateBooster(Vector2.up, 14f, 2.5f);
            var (_, _, playerCol, _) = PlayerController.CreateMockPlayer();

            TestRuntime.InvokeLifecycle(booster, "OnTriggerEnter2D", playerCol);

            Assert.That(anim.CurrentState, Is.EqualTo("Hit"), "Phải chạy clip nổ Hit ngay khi kích hoạt");
            Assert.That(sr.enabled, Is.True, "Trong 0.2s đầu vẫn phải hiển thị clip nổ");

            TestRuntime.AdvanceTime(0.25f);
            Assert.That(sr.enabled, Is.False, "Sau clip Hit, mũi tên phải ẩn đi trong thời gian hồi");

            TestRuntime.AdvanceTime(2.5f);
            Assert.That(sr.enabled, Is.True, "Hồi xong phải hiện lại mũi tên");
            Assert.That(anim.CurrentState, Is.EqualTo("Idle"), "Phải trở về clip Idle");
        }

        [Test]
        [Description("TC-AB-EXT-02 (bổ sung, ngoài matrix): Guard Clauses — Player đã chết hoặc vật thể không phải Player -> không kích hoạt")]
        public void TC_AB_EXT_02_GuardClauses()
        {
            var (_, booster, col, _, _) = CreateBooster(Vector2.up, 14f, 2.5f);

            // Player đã chết
            var (_, deadPlayer, deadCol, _) = PlayerController.CreateMockPlayer("DeadPlayer");
            deadPlayer.IsDead = true;
            TestRuntime.InvokeLifecycle(booster, "OnTriggerEnter2D", deadCol);

            Assert.That(deadPlayer.BoostCount, Is.EqualTo(0), "Player đã chết không được hồi Nhảy Đôi");
            Assert.That(booster.IsReady, Is.True, "Mũi tên phải giữ nguyên trạng thái sẵn sàng");

            // Vật thể không có tag Player
            GameObject other = new GameObject("Enemy") { tag = "Enemy" };
            BoxCollider2D otherCol = other.AddComponent<BoxCollider2D>();
            TestRuntime.InvokeLifecycle(booster, "OnTriggerEnter2D", otherCol);

            Assert.That(booster.IsReady, Is.True, "Vật thể không phải Player không được kích hoạt mũi tên");
            Assert.That(col.enabled, Is.True);
        }
    }
}