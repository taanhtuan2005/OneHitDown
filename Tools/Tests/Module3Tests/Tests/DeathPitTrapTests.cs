// Trần Văn Tuyên, phiên bản 1.0
// Bộ kiểm thử tự động cho DeathPitTrap (TC-DP-01 đến TC-DP-05) theo Test Matrix §8.2.
using NUnit.Framework;
using OneHitDown.Player;
using OneHitDown.Traps;
using UnityEngine;

namespace OneHitDown.Tests
{
    [TestFixture]
    public class DeathPitTrapTests
    {
        [SetUp]
        public void SetUp()
        {
            TestRuntime.Reset();
        }

        private (GameObject go, DeathPitTrap pit, BoxCollider2D solidCol, BoxCollider2D deathTrigger, SpriteRenderer sr, Sprite crackSp)
            CreateDeathPit(float shakeDuration = 0.2f)
        {
            GameObject go = new GameObject("DeathPitTrap");
            BoxCollider2D solidCol = go.AddComponent<BoxCollider2D>();
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

            GameObject childHazard = new GameObject("DeathTriggerZone");
            childHazard.transform.parent = go;
            childHazard.tag = "Hazard";
            BoxCollider2D deathTrigger = childHazard.AddComponent<BoxCollider2D>();
            deathTrigger.isTrigger = true;

            DeathPitTrap pit = go.AddComponent<DeathPitTrap>();

            Sprite idleSp = Sprite.Create("Idle");
            Sprite crackSp = Sprite.Create("Crack");
            Sprite part1 = Sprite.Create("Part1");
            Sprite part2 = Sprite.Create("Part2");

            TestRuntime.SetField(pit, "solidCollider", solidCol);
            TestRuntime.SetField(pit, "deathTrigger", deathTrigger);
            TestRuntime.SetField(pit, "blockRenderer", sr);
            TestRuntime.SetField(pit, "idleSprite", idleSp);
            TestRuntime.SetField(pit, "crackSprite", crackSp);
            TestRuntime.SetField(pit, "part1Sprite", part1);
            TestRuntime.SetField(pit, "part2Sprite", part2);
            TestRuntime.SetField(pit, "shakeDuration", shakeDuration);

            TestRuntime.InvokeLifecycle(pit, "Awake");

            return (go, pit, solidCol, deathTrigger, sr, crackSp);
        }

        [Test]
        [Description("TC-DP-01: Initial State Verification — Khởi tạo: solidCollider.enabled == true, deathTrigger.gameObject.activeSelf == false")]
        public void TC_DP_01_InitialStateVerification()
        {
            var (_, pit, solidCol, deathTrigger, _, _) = CreateDeathPit();

            Assert.That(pit.IsTriggered, Is.False);
            Assert.That(solidCol.enabled, Is.True, "solidCollider ban đầu phải bật");
            Assert.That(deathTrigger.gameObject.activeSelf, Is.False, "deathTrigger ban đầu phải tắt để không gây chết oan");
        }

        [Test]
        [Description("TC-DP-02: Collapse Sequence — Player giẫm mặt trên: Rung 0.2s -> solidCollider.enabled == false -> deathTrigger.activeSelf == true")]
        public void TC_DP_02_CollapseSequence()
        {
            var (_, pit, solidCol, deathTrigger, sr, crackSp) = CreateDeathPit(shakeDuration: 0.2f);
            var (_, _, playerCol, _) = PlayerController.CreateMockPlayer();

            // Va chạm từ trên xuống (normal.y = -1f < -0.5f)
            Collision2D colFromAbove = Collision2D.Create(playerCol, new Vector2(0f, -1f));
            TestRuntime.InvokeLifecycle(pit, "OnCollisionEnter2D", colFromAbove);

            Assert.That(pit.IsTriggered, Is.True, "Bẫy phải chuyển sang trạng thái đã kích hoạt");
            Assert.That(sr.sprite, Is.EqualTo(crackSp), "Pha nứt phải đổi sang crackSprite");

            // Trong lúc rung 0.1s: sàn vẫn còn đỡ, hazard chưa mở
            TestRuntime.AdvanceTime(0.1f);
            Assert.That(solidCol.enabled, Is.True);
            Assert.That(deathTrigger.gameObject.activeSelf, Is.False);

            // Hết 0.2s rung: sàn sập, mở toang hazard
            TestRuntime.AdvanceTime(0.15f);
            Assert.That(solidCol.enabled, Is.False, "solidCollider phải tắt sau khi gạch vỡ");
            Assert.That(deathTrigger.gameObject.activeSelf, Is.True, "deathTrigger phải bật để giết player rơi vào");
        }

        [Test]
        [Description("TC-DP-03: Permanent Open In Run — Sau khi sập, hố giữ nguyên trạng thái mở suốt lượt chơi")]
        public void TC_DP_03_PermanentOpenInRun()
        {
            var (_, pit, solidCol, deathTrigger, _, _) = CreateDeathPit();

            pit.TriggerCollapse();
            TestRuntime.AdvanceTime(0.3f); // Hoàn thành sập

            // Chờ 10s trong lượt chơi
            TestRuntime.AdvanceTime(10f);

            Assert.That(solidCol.enabled, Is.False, "solidCollider không được tự bật");
            Assert.That(deathTrigger.gameObject.activeSelf, Is.True, "deathTrigger phải luôn mở toang suốt lượt chơi");
        }

        [Test]
        [Description("TC-DP-04: Idempotent ResetState — Gọi ResetState() khi hố đang mở -> solidCollider bật, deathTrigger tắt, isTriggered = false")]
        public void TC_DP_04_IdempotentResetState()
        {
            var (_, pit, solidCol, deathTrigger, sr, _) = CreateDeathPit();

            pit.TriggerCollapse();
            TestRuntime.AdvanceTime(0.3f);
            Assert.That(pit.IsTriggered, Is.True);

            for (int i = 0; i < 3; i++)
            {
                pit.ResetState();
                Assert.That(pit.IsTriggered, Is.False, "isTriggered phải được reset về false");
                Assert.That(solidCol.enabled, Is.True, "solidCollider phải bật lại");
                Assert.That(deathTrigger.gameObject.activeSelf, Is.False, "deathTrigger phải tắt");
                Assert.That(sr.enabled, Is.True);
            }
        }

        [Test]
        [Description("TC-DP-05: Side Contact Ignored — Va chạm từ cạnh bên -> Không kích hoạt sập")]
        public void TC_DP_05_SideContactIgnored()
        {
            var (_, pit, solidCol, deathTrigger, _, _) = CreateDeathPit();
            var (_, _, playerCol, _) = PlayerController.CreateMockPlayer();

            // Va chạm từ cạnh bên (normal.y = 0, normal.x = 1f)
            Collision2D sideContact = Collision2D.Create(playerCol, new Vector2(1f, 0f));
            TestRuntime.InvokeLifecycle(pit, "OnCollisionEnter2D", sideContact);

            Assert.That(pit.IsTriggered, Is.False, "Va chạm ngang không được làm sập hố");
            Assert.That(solidCol.enabled, Is.True);
            Assert.That(deathTrigger.gameObject.activeSelf, Is.False);
        }
    }
}