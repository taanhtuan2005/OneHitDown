// Trần Văn Tuyên, phiên bản 1.0
// Bộ kiểm thử tự động cho IceSurface (TC-IC-01 đến TC-IC-04 + ca bổ sung) theo Test Matrix §8.2.
using NUnit.Framework;
using OneHitDown.Player;
using OneHitDown.Traps;
using UnityEngine;

namespace OneHitDown.Tests
{
    [TestFixture]
    public class IceSurfaceTests
    {
        [SetUp]
        public void SetUp()
        {
            TestRuntime.Reset();
        }

        private (GameObject go, IceSurface ice, ParticleSystem particle, BoxCollider2D iceCol)
            CreateIce(bool withMaterial = true, bool withParticle = true)
        {
            GameObject go = new GameObject("IceSurface");
            BoxCollider2D iceCol = go.AddComponent<BoxCollider2D>();
            iceCol.isTrigger = true;
            ParticleSystem particle = go.AddComponent<ParticleSystem>();
            IceSurface ice = go.AddComponent<IceSurface>();

            PhysicsMaterial2D iceMaterial = null;
            if (withMaterial)
            {
                iceMaterial = new PhysicsMaterial2D { friction = 0.02f, bounciness = 0f };
            }

            TestRuntime.SetField(ice, "iceMaterial", iceMaterial);
            TestRuntime.SetField(ice, "iceParticle", withParticle ? particle : null);

            TestRuntime.InvokeLifecycle(ice, "Awake");

            return (go, ice, particle, iceCol);
        }

        [Test]
        [Description("TC-IC-01: Player Enter Surface — Player chạm collider -> player.SetOnIce(true), ActivePlayerCount == 1")]
        public void TC_IC_01_PlayerEnterSurface()
        {
            var (_, ice, particle, _) = CreateIce();
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();

            TestRuntime.InvokeLifecycle(ice, "OnTriggerEnter2D", playerCol);

            Assert.That(player.IsCurrentlyOnIce, Is.True, "Player phải được bật trạng thái trượt băng");
            Assert.That(player.IceStateHistory.Count, Is.EqualTo(1));
            Assert.That(player.IceStateHistory[0], Is.True, "SetOnIce phải được gọi với tham số true");
            Assert.That(ice.ActivePlayerCount, Is.EqualTo(1), "Phải ghi nhận đúng 1 player đang trên băng");
            Assert.That(particle.isPlaying, Is.True, "Hiệu ứng hạt băng phải chạy khi có player trượt");
        }

        [Test]
        [Description("TC-IC-02: Player Exit Surface — Player rời collider -> player.SetOnIce(false), ActivePlayerCount == 0, particle.Stop()")]
        public void TC_IC_02_PlayerExitSurface()
        {
            var (_, ice, particle, _) = CreateIce();
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();

            TestRuntime.InvokeLifecycle(ice, "OnTriggerEnter2D", playerCol);
            Assert.That(particle.isPlaying, Is.True);

            TestRuntime.InvokeLifecycle(ice, "OnTriggerExit2D", playerCol);

            Assert.That(player.IsCurrentlyOnIce, Is.False, "Player phải được trả lại ma sát mặt đất bình thường");
            Assert.That(player.IceStateHistory.Count, Is.EqualTo(2));
            Assert.That(player.IceStateHistory[1], Is.False, "SetOnIce phải được gọi với tham số false");
            Assert.That(ice.ActivePlayerCount, Is.EqualTo(0));
            Assert.That(particle.isPlaying, Is.False, "Hết player trên băng thì phải dừng hiệu ứng hạt");
        }

        [Test]
        [Description("TC-IC-03: Non-Player Ignored — Vật thể không có PlayerController (có tag Player và không tag) -> Không làm gì")]
        public void TC_IC_03_NonPlayerIgnored()
        {
            var (_, ice, particle, _) = CreateIce();

            // Trường hợp 1: object có tag "Player" nhưng thiếu component PlayerController.
            GameObject taggedWithoutController = new GameObject("FakePlayer") { tag = "Player" };
            BoxCollider2D taggedCol = taggedWithoutController.AddComponent<BoxCollider2D>();

            // Trường hợp 2: object không có tag "Player".
            GameObject other = new GameObject("Enemy") { tag = "Enemy" };
            BoxCollider2D otherCol = other.AddComponent<BoxCollider2D>();

            TestRuntime.InvokeLifecycle(ice, "OnTriggerEnter2D", taggedCol);
            TestRuntime.InvokeLifecycle(ice, "OnTriggerEnter2D", otherCol);

            Assert.That(ice.ActivePlayerCount, Is.EqualTo(0), "Vật thể không phải PlayerController không được ghi nhận");
            Assert.That(particle.isPlaying, Is.False, "Không có player thì không được bật hiệu ứng");

            // Rời đi cũng không được làm gì (không ném exception, count vẫn 0).
            TestRuntime.InvokeLifecycle(ice, "OnTriggerExit2D", taggedCol);
            TestRuntime.InvokeLifecycle(ice, "OnTriggerExit2D", otherCol);
            Assert.That(ice.ActivePlayerCount, Is.EqualTo(0));
        }

        [Test]
        [Description("TC-IC-04: OnDisable Cleanup — 2 player đang trên băng, script bị disable -> cả 2 player đều nhận SetOnIce(false)")]
        public void TC_IC_04_OnDisableCleanup()
        {
            var (go, ice, particle, _) = CreateIce();
            var (_, player1, player1Col, _) = PlayerController.CreateMockPlayer("MockPlayer1");
            var (_, player2, player2Col, _) = PlayerController.CreateMockPlayer("MockPlayer2");

            TestRuntime.InvokeLifecycle(ice, "OnTriggerEnter2D", player1Col);
            TestRuntime.InvokeLifecycle(ice, "OnTriggerEnter2D", player2Col);

            Assert.That(ice.ActivePlayerCount, Is.EqualTo(2), "Phải ghi nhận đủ 2 player");
            Assert.That(particle.isPlaying, Is.True);

            go.SetActive(false); // Unity tự gọi OnDisable

            Assert.That(player1.IsCurrentlyOnIce, Is.False, "Player 1 phải được trả lại trạng thái ma sát thường");
            Assert.That(player2.IsCurrentlyOnIce, Is.False, "Player 2 phải được trả lại trạng thái ma sát thường");
            Assert.That(player1.IceStateHistory[player1.IceStateHistory.Count - 1], Is.False);
            Assert.That(player2.IceStateHistory[player2.IceStateHistory.Count - 1], Is.False);
            Assert.That(ice.ActivePlayerCount, Is.EqualTo(0), "Danh sách player phải được dọn sạch");
            Assert.That(particle.isPlaying, Is.False, "Hiệu ứng hạt phải dừng khi script bị disable");
        }

        [Test]
        [Description("TC-IC-EXT-01 (bổ sung, ngoài matrix): Duplicate Enter Guard — cùng 1 player nhận 3 lần OnTriggerEnter2D -> SetOnIce(true) chỉ gọi 1 lần")]
        public void TC_IC_EXT_01_DuplicateEnterGuard()
        {
            var (_, ice, _, _) = CreateIce();
            var (_, player, playerCol, _) = PlayerController.CreateMockPlayer();

            TestRuntime.InvokeLifecycle(ice, "OnTriggerEnter2D", playerCol);
            TestRuntime.InvokeLifecycle(ice, "OnTriggerEnter2D", playerCol);
            TestRuntime.InvokeLifecycle(ice, "OnTriggerEnter2D", playerCol);

            Assert.That(player.IceStateHistory.Count, Is.EqualTo(1), "Chỉ được gọi SetOnIce(true) đúng 1 lần cho cùng 1 player");
            Assert.That(ice.ActivePlayerCount, Is.EqualTo(1));
        }
    }
}