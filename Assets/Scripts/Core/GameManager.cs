// Hoàng Thị Giang Uyên (2351060497) — Module 5: Core Manager & Instant Reset
// Docs/SCHEMA_CHI_TIET.md §3 Module 5.1 — Docs/Phan_cong.md Mục III (Người 5).
//
// Phụ thuộc liên module (Module 1 & 4 cung cấp):
//   OneHitDown.Level.LevelStartPoint  : Transform P1SpawnPoint, P2SpawnPoint
//   OneHitDown.Level.ExitGate         : UpdateProgress(int, int), UnlockGate()
//   OneHitDown.UI.GameSession         : IsTwoPlayerMode, P1Skin, P2Skin, CurrentLevelIndex, UnlockLevel(int)
//   OneHitDown.Player.PlayerController: ApplySkin(CharacterSkin) + enum CharacterSkin
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OneHitDown.Level;
using OneHitDown.Player;
using OneHitDown.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHitDown.Core
{
    /// <summary>
    /// Trọng tài một màn chơi: quản lý 1P/2P, đếm 3/3 Cổ Vật mở Cổng Thần, xử lý chết chùm
    /// Co-op và hồi sinh tức thì về Bệ Xuất Phát kèm ResetState() cho mọi đối tượng IResettable.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        #region Hằng số & Field SerializeField

        private const string MainMenuSceneName = "MainMenu";
        private const string GateUnlockSfx = "impactBell_heavy_000";
        private const string RelicSfx = "powerUp2";
        private const string BonusFruitSfx = "threeTone2";
        private const string VictoryJingle = "jingles_NES03";

        private static readonly string[] LevelScenes =
            { "Level_01", "Level_02", "Level_03", "Level_04", "Level_05" };

        private static readonly float[] TimeScaleSteps = { 1f, 0.5f, 1.5f };

        [Header("Tham chiếu Scene")]
        [SerializeField] private LevelStartPoint startPoint;   // Bệ xuất phát đầu màn
        [SerializeField] private ExitGate exitGate;            // Cổng Thần cuối màn
        [SerializeField] private PlayerController player1Prefab;
        [SerializeField] private PlayerController player2Prefab;
        [SerializeField] private SoulTether soulTetherPrefab;

        [Header("Thông số màn chơi")]
        [SerializeField] private int totalRelicsRequired = 3;  // Chuẩn 3/3 Cổ Vật
        [SerializeField] private float respawnDelay = 0.18f;   // Hồi sinh tức thì < 0.2s

        [Header("Hệ thống")]
        [SerializeField] private CameraController mainCamera;
        [SerializeField] private HUDController hudController;
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private IrisTransition irisTransition;
        [SerializeField] private AudioManager audioManager;

        #endregion

        #region Trạng thái runtime & Property công khai

        private readonly List<PlayerController> activePlayers = new();
        private IResettable[] resettables = Array.Empty<IResettable>();
        private Collider2D[] sceneColliders;   // Cache cho phím H
        private Coroutine respawnRoutine;

        private int collectedRelics, totalScore, deathCount, timeScaleStep;
        private float elapsedTime;
        private bool isGodMode, isResetting, showHitboxes;

        public static GameManager Instance { get; private set; }
        public int TotalRelicsRequired => totalRelicsRequired;
        public int CollectedRelicsCount => collectedRelics;
        public int TotalScore => totalScore;
        public int DeathCount => deathCount;
        public float ElapsedTime => elapsedTime;
        public bool IsGodMode => isGodMode;
        public bool IsResetting => isResetting;

        /// <summary>Chỉ số màn hiện tại (1..5) lấy từ GameSession, kẹp về khoảng hợp lệ.</summary>
        private static int LevelIndex => Mathf.Clamp(GameSession.CurrentLevelIndex, 1, LevelScenes.Length);

        #endregion

        #region Vòng đời MonoBehaviour

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Gom Quả / Bẫy sập / Hố Tử Thần / Cổng đích có sẵn trong Scene.
            resettables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IResettable>().ToArray();

            SpawnPlayers();

            // Quét sau khi sinh nhân vật để phím H thấy cả Hitbox của Player1/Player2.
            sceneColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        }

        // AudioManager có DefaultExecutionOrder(-90) nên chỉ sẵn sàng từ Start của GameManager.
        private void Start() => PlayBgm(LevelIndex);

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            HandleHotkeys();
        }

        private void OnDestroy()
        {
            if (Instance == this) { Instance = null; }
        }

        #endregion

        #region Sinh nhân vật 1P / 2P

        private void SpawnPlayers()
        {
            activePlayers.Clear();

            if (startPoint == null || player1Prefab == null)
            {
                Debug.LogError("[GameManager] Chưa gán startPoint hoặc player1Prefab trong Inspector.", this);
                return;
            }

            Spawn(player1Prefab, (Vector2)startPoint.P1SpawnPoint.position, "Player1", GameSession.P1Skin);

            if (GameSession.IsTwoPlayerMode)
            {
                if (player2Prefab == null || soulTetherPrefab == null)
                {
                    Debug.LogError("[GameManager] Chế độ 2P cần cả player2Prefab và soulTetherPrefab.", this);
                    return;
                }

                Spawn(player2Prefab, (Vector2)startPoint.P2SpawnPoint.position, "Player2", GameSession.P2Skin);

                // Dây xích sinh sau bước quét Scene nên phải nối tay vào danh sách reset.
                SoulTether tether = Instantiate(soulTetherPrefab);
                tether.gameObject.name = "SoulTether";
                tether.SetupPhysicsJoint(activePlayers[0], activePlayers[1]);

                Array.Resize(ref resettables, resettables.Length + 1);
                resettables[^1] = tether;
            }

            if (mainCamera != null)
            {
                mainCamera.SetTargets(activePlayers[0].transform,
                    activePlayers.Count > 1 ? activePlayers[1].transform : null);
            }
        }

        /// <summary>Sinh một nhân vật, áp Skin từ Menu và đăng ký lắng nghe sự kiện chết.</summary>
        private void Spawn(PlayerController prefab, Vector2 position, string playerName, CharacterSkin skin)
        {
            PlayerController player = Instantiate(prefab, position, Quaternion.identity);
            player.gameObject.name = playerName;
            player.ApplySkin(skin);
            player.OnPlayerDied += OnAnyPlayerDied;
            activePlayers.Add(player);
        }

        #endregion

        #region Phím tắt: R / ESC-P / G / H / T / 1..5

        private void HandleHotkeys()
        {
            // Dự án chỉ bật New Input System (ProjectSettings.activeInputHandler = 1).
#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                ResetLevel();                                        // R: khôi phục toàn màn
            }

            if (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)
            {
                if (pauseMenu != null) { pauseMenu.TogglePause(); }   // ESC/P: bảng tạm dừng
            }

            if (keyboard.gKey.wasPressedThisFrame)
            {
                isGodMode = !isGodMode;                              // G: chết không tính lượt
            }

            if (keyboard.hKey.wasPressedThisFrame)
            {
                showHitboxes = !showHitboxes;                        // H: khung Hitbox
            }

            if (keyboard.tKey.wasPressedThisFrame && (pauseMenu == null || !pauseMenu.IsPaused))
            {
                timeScaleStep = (timeScaleStep + 1) % TimeScaleSteps.Length;   // T: 1x → 0.5x → 1.5x
                Time.timeScale = TimeScaleSteps[timeScaleStep];
            }

            for (int i = 0; i < LevelScenes.Length; i++)
            {
                if (keyboard[UnityEngine.InputSystem.Key.Digit1 + i].wasPressedThisFrame)
                {
                    LoadSceneOrMenu(i + 1);                          // 1..5: nhảy sang Level_01..05
                    break;
                }
            }
#endif
        }

        #endregion

        #region Chết chùm Co-op & Hồi sinh tức thì

        /// <summary>
        /// Người chơi báo chết: tăng DEATHS, kéo người còn lại chết theo ở 2P Co-op
        /// rồi hồi sinh tức thì cả đội về Bệ Xuất Phát đầu màn.
        /// </summary>
        public void OnAnyPlayerDied(PlayerController victim)
        {
            // GodMode: hồi sinh ngay tại chỗ, không tính lượt chết và không reset màn.
            if (isGodMode)
            {
                if (victim != null) { victim.Respawn(ResolveSpawnPoint(activePlayers.IndexOf(victim))); }
                return;
            }

            if (isResetting) { return; }

            isResetting = true;
            deathCount++;

            // 2P Co-op: người còn lại chết theo để cả đội cùng tan biến.
            foreach (PlayerController other in activePlayers)
            {
                if (other != null && other != victim && !other.IsDead) { other.Die(); }
            }

            respawnRoutine = StartCoroutine(RespawnAndResetRoutine());
        }

        /// <summary>Chờ đúng <c>respawnDelay</c> (0.18s) rồi khôi phục toàn màn chơi.</summary>
        private IEnumerator RespawnAndResetRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            respawnRoutine = null;
            ResetLevel();
        }

        /// <summary>
        /// Khôi phục toàn màn: đưa Cổ Vật / Bẫy / Cổng về trạng thái gốc và hồi sinh
        /// mọi người chơi về Bệ Xuất Phát đầu màn.
        /// </summary>
        public void ResetLevel()
        {
            if (respawnRoutine != null) { StopCoroutine(respawnRoutine); respawnRoutine = null; }

            collectedRelics = 0;
            totalScore = 0;

            for (int i = 0; i < resettables.Length; i++)
            {
                // Bỏ qua đối tượng đã bị Unity huỷ (so sánh null kiểu UnityEngine.Object).
                if (resettables[i] is UnityEngine.Object target && target == null) { continue; }
                resettables[i].ResetState();
            }

            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (activePlayers[i] != null) { activePlayers[i].Respawn(ResolveSpawnPoint(i)); }
            }

            if (hudController != null) { hudController.RefreshHud(); }
            isResetting = false;
        }

        /// <summary>Toạ độ hồi sinh theo chỉ số người chơi (0 = Player1, 1 = Player2).</summary>
        private Vector2 ResolveSpawnPoint(int playerIndex)
        {
            if (startPoint == null) { return Vector2.zero; }

            return playerIndex == 1
                ? (Vector2)startPoint.P2SpawnPoint.position
                : (Vector2)startPoint.P1SpawnPoint.position;
        }

        #endregion

        #region Cổ Vật, Cổng Thần & Hoàn thành màn

        /// <summary>Cộng điểm khi nhặt Quả; đếm Cổ Vật chính và mở Cổng Thần khi đủ 3/3.</summary>
        public void OnRelicCollected(int score, bool countsAsRelic)
        {
            totalScore += score;

            if (countsAsRelic)
            {
                collectedRelics = Mathf.Min(collectedRelics + 1, totalRelicsRequired);
                if (exitGate != null) { exitGate.UpdateProgress(collectedRelics, totalRelicsRequired); }
            }

            if (collectedRelics >= totalRelicsRequired)
            {
                if (exitGate != null) { exitGate.UnlockGate(); }
                PlaySfx(GateUnlockSfx);
            }
            else
            {
                // Cổ Vật chính: powerUp2. Quả thưởng phụ: threeTone2.
                PlaySfx(countsAsRelic ? RelicSfx : BonusFruitSfx);
            }

            if (hudController != null) { hudController.RefreshHud(); }
        }

        /// <summary>Hoàn thành màn: mở khoá màn kế, phát nhạc chiến thắng và chạy hiệu ứng màn tròn.</summary>
        public void OnLevelCompleted()
        {
            int nextLevelIndex = LevelIndex + 1;
            GameSession.UnlockLevel(nextLevelIndex);

            if (audioManager != null) { audioManager.PlayJingle(VictoryJingle); }

            if (irisTransition != null) { irisTransition.PlayTransition(() => LoadSceneOrMenu(nextLevelIndex)); }
            else { LoadSceneOrMenu(nextLevelIndex); }
        }

        #endregion

        #region Tiện ích: chuyển Scene, phát âm & Gizmos Hitbox

        /// <summary>Chuyển sang màn kế tiếp; hết 5 màn thì về MainMenu. Scene phải có trong Build Settings.</summary>
        private void LoadSceneOrMenu(int levelIndex)
        {
            Time.timeScale = 1f;

            string target = levelIndex >= 1 && levelIndex <= LevelScenes.Length
                ? LevelScenes[levelIndex - 1]
                : MainMenuSceneName;

            if (Application.CanStreamedLevelBeLoaded(target)) { SceneManager.LoadScene(target); return; }

            Debug.LogWarning($"[GameManager] Scene '{target}' chưa có trong Build Settings — bỏ qua lệnh chuyển màn.", this);
        }

        private void PlaySfx(string clipName)
        {
            if (audioManager != null) { audioManager.PlaySFX(clipName); }
        }

        private void PlayBgm(int levelIndex)
        {
            if (audioManager != null) { audioManager.PlayLevelBGM(levelIndex); }
        }

#if UNITY_EDITOR
        /// <summary>Phím H: vẽ khung Hitbox toàn bộ Collider trong Scene (chỉ trong Editor).</summary>
        private void OnDrawGizmos()
        {
            if (!showHitboxes || sceneColliders == null) { return; }

            foreach (Collider2D collider in sceneColliders)
            {
                if (collider == null || !collider.enabled) { continue; }

                Gizmos.color = collider.CompareTag("Hazard") ? Color.red
                    : collider.CompareTag("Player") ? Color.green
                    : Color.yellow;

                Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
            }
        }
#endif

        #endregion
    }
}
