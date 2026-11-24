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
using UnityEngine.UI;

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
        [SerializeField] private int levelIndexOverride;       // 0 = lấy từ GameSession; 1..5 = ép chỉ số màn
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
        private bool isGodMode, isResetting, showHitboxes, isLevelCompleted;
        private Coroutine stageClearRoutine;
        private GameObject stageClearOverlay;
        private GameObject grandVictoryOverlay;

        public static GameManager Instance { get; private set; }
        public int TotalRelicsRequired => totalRelicsRequired;
        public int CollectedRelicsCount => collectedRelics;
        public int TotalScore => totalScore;
        public int DeathCount => deathCount;
        public float ElapsedTime => elapsedTime;
        public bool IsGodMode => isGodMode;
        public bool IsResetting => isResetting;

        /// <summary>
        /// Chỉ số màn hiện tại (1..5). Ưu tiên <c>levelIndexOverride</c> khi được gán trong Inspector
        /// (dùng cho Scene mở trực tiếp mà không qua MainMenu), ngược lại lấy từ <see cref="GameSession"/>.
        /// </summary>
        public int LevelIndex
        {
            get
            {
                int index = levelIndexOverride > 0 ? levelIndexOverride : GameSession.CurrentLevelIndex;
                return Mathf.Clamp(index, 1, LevelScenes.Length);
            }
        }

        #endregion

        #region Vòng đời MonoBehaviour

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Theo Schema §1.3: Layer 9 (Player) không tự đẩy nhau (tắt va chạm solid giữa Player1 và Player2)
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, playerLayer, true);
            }

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
            if (!isLevelCompleted)
            {
                elapsedTime += Time.deltaTime;
            }

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
            if (activePlayers.Count > 0 && activePlayers[0] != null)
            {
                activePlayers[0].SetAcceptArrowKeys(!GameSession.IsTwoPlayerMode);
            }

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

            if (keyboard.rKey.wasPressedThisFrame && !isLevelCompleted)
            {
                ResetLevel();                                        // R: khôi phục toàn màn
            }

            if ((keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame) && !isLevelCompleted)
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

            if (isResetting || isLevelCompleted) { return; }

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
            if (stageClearRoutine != null) { StopCoroutine(stageClearRoutine); stageClearRoutine = null; }
            if (stageClearOverlay != null) { stageClearOverlay.SetActive(false); }
            if (grandVictoryOverlay != null) { grandVictoryOverlay.SetActive(false); }

            isLevelCompleted = false;
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
                if (activePlayers[i] != null)
                {
                    activePlayers[i].enabled = true;
                    activePlayers[i].Respawn(ResolveSpawnPoint(i));
                }
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

        /// <summary>Hoàn thành màn: mở khoá màn kế, phát nhạc thắng và hiện bảng Qua Màn / Phá Đảo.</summary>
        public void OnLevelCompleted()
        {
            if (isLevelCompleted) { return; }
            isLevelCompleted = true;

            if (respawnRoutine != null) { StopCoroutine(respawnRoutine); respawnRoutine = null; }
            if (pauseMenu != null && pauseMenu.IsPaused) { pauseMenu.SetPaused(false); }

            GameSession.ClearPausedSession();

            int currentLevel = LevelIndex;
            int nextLevelIndex = currentLevel + 1;
            GameSession.UnlockLevel(currentLevel);
            GameSession.CurrentLevelIndex = Mathf.Clamp(nextLevelIndex, 1, LevelScenes.Length);

            FreezePlayersForCelebration();

            if (audioManager != null) { audioManager.PlayJingle(VictoryJingle); }

            if (currentLevel < LevelScenes.Length)
            {
                ShowStageClearOverlay(currentLevel, nextLevelIndex);
            }
            else
            {
                ShowGrandVictoryOverlay();
            }
        }

        private void FreezePlayersForCelebration()
        {
            for (int i = 0; i < activePlayers.Count; i++)
            {
                PlayerController player = activePlayers[i];
                if (player == null) { continue; }
                player.enabled = false;
                if (player.Rigidbody != null)
                {
                    player.Rigidbody.linearVelocity = Vector2.zero;
                    player.Rigidbody.simulated = false;
                }
            }
        }

        private Canvas ResolveUiCanvas()
        {
            if (hudController != null)
            {
                Canvas c = hudController.GetComponentInParent<Canvas>();
                if (c != null) { return c; }
            }

            if (irisTransition != null)
            {
                Canvas c = irisTransition.GetComponentInParent<Canvas>();
                if (c != null) { return c; }
            }

            return FindFirstObjectByType<Canvas>();
        }

        private Font ResolveUiFont(Canvas canvas)
        {
            if (hudController != null)
            {
                Text hudTxt = hudController.GetComponentInChildren<Text>(true);
                if (hudTxt != null && hudTxt.font != null) { return hudTxt.font; }
            }

            if (canvas != null)
            {
                Text anyTxt = canvas.GetComponentInChildren<Text>(true);
                if (anyTxt != null && anyTxt.font != null) { return anyTxt.font; }
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private GameObject CreateOverlayRoot(string objName, Canvas canvas)
        {
            GameObject overlayGo = new GameObject(objName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlayGo.transform.SetParent(canvas.transform, false);

            // Đặt ngay trước IrisTransition để màn tròn đóng mở vẫn phủ trên cùng
            if (irisTransition != null && irisTransition.transform.parent == canvas.transform)
            {
                overlayGo.transform.SetSiblingIndex(irisTransition.transform.GetSiblingIndex());
            }

            RectTransform rt = overlayGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image dim = overlayGo.GetComponent<Image>();
            dim.color = new Color(0.02f, 0.03f, 0.07f, 0.84f);
            dim.raycastTarget = true;
            return overlayGo;
        }

        private void ShowStageClearOverlay(int currentLevel, int nextLevelIndex)
        {
            Canvas canvas = ResolveUiCanvas();
            if (canvas == null)
            {
                ProceedWithTransition(nextLevelIndex);
                return;
            }

            if (stageClearOverlay != null) { Destroy(stageClearOverlay); }

            Font uiFont = ResolveUiFont(canvas);
            stageClearOverlay = CreateOverlayRoot("StageClearOverlay", canvas);

            RectTransform cardInner = RuntimeUiHelper.CreateFramedCard(
                "StageClearCard", stageClearOverlay.transform, Vector2.zero, new Vector2(880f, 500f),
                new Color(1f, 0.85f, 0f, 1f), new Color(0.08f, 0.10f, 0.19f, 0.98f), 6f);

            RectTransform logoRt = RuntimeUiHelper.CreateUiRect("LogoBadge", cardInner, new Vector2(0f, 186f), new Vector2(72f, 72f));
            Image logoImg = logoRt.gameObject.AddComponent<Image>();
            logoImg.sprite = RuntimeUiHelper.GetTeam505LogoSprite();
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = false;

            RuntimeUiHelper.CreateLegacyText("ClearTitle", cardInner, uiFont, new Vector2(0f, 114f), new Vector2(820f, 64f),
                $"QUA MÀN {currentLevel:00} THÀNH CÔNG!", 60, new Color(1f, 0.87f, 0f, 1f));

            RuntimeUiHelper.CreateLegacyText("StageSubtitle", cardInner, uiFont, new Vector2(0f, 60f), new Vector2(800f, 40f),
                $"KHU VỰC: {GameSession.GetLevelDisplayName(currentLevel)}", 34, new Color(0.45f, 0.94f, 1f, 1f));

            int sec = Mathf.FloorToInt(elapsedTime);
            string statsLine = $"CỔ VẬT: {collectedRelics}/{totalRelicsRequired}  |  ĐIỂM: {totalScore}  |  THỜI GIAN: {sec / 60:00}:{sec % 60:00}  |  CHẾT: {deathCount:00}";
            RectTransform statsBox = RuntimeUiHelper.CreateFramedCard("StatsBox", cardInner, new Vector2(0f, -18f),
                new Vector2(780f, 84f), new Color(0.24f, 0.34f, 0.56f, 1f), new Color(0.05f, 0.06f, 0.11f, 1f), 3f);
            RuntimeUiHelper.CreateLegacyText("StatsText", statsBox, uiFont, Vector2.zero, new Vector2(750f, 70f),
                statsLine, 30, Color.white);

            Button nextBtn = RuntimeUiHelper.CreateLegacyButton(
                "NextLevelButton", cardInner, uiFont, new Vector2(-120f, -152f), new Vector2(470f, 66f),
                $"MÀN KẾ TIẾP: MÀN {nextLevelIndex:00} (2.5s)", 32, new Color(0.14f, 0.76f, 0.34f, 1f), Color.white,
                () =>
                {
                    if (stageClearRoutine != null) { StopCoroutine(stageClearRoutine); stageClearRoutine = null; }
                    ProceedWithTransition(nextLevelIndex);
                });

            RuntimeUiHelper.CreateLegacyButton(
                "BackMenuButton", cardInner, uiFont, new Vector2(250f, -152f), new Vector2(230f, 66f),
                "VỀ MENU CHÍNH", 30, new Color(0.26f, 0.30f, 0.44f, 1f), Color.white,
                () =>
                {
                    if (stageClearRoutine != null) { StopCoroutine(stageClearRoutine); stageClearRoutine = null; }
                    ProceedWithTransition(0);
                });

            Text nextBtnLabel = nextBtn.GetComponentInChildren<Text>(true);
            if (stageClearRoutine != null) { StopCoroutine(stageClearRoutine); }
            stageClearRoutine = StartCoroutine(StageClearCountdownRoutine(nextBtnLabel, nextLevelIndex));
        }

        private IEnumerator StageClearCountdownRoutine(Text nextBtnLabel, int nextLevelIndex)
        {
            float remaining = 2.5f;
            int lastShownTenths = -1;
            string nextName = GameSession.GetLevelDisplayName(nextLevelIndex);

            while (remaining > 0f)
            {
                remaining -= Time.unscaledDeltaTime;
                int tenths = Mathf.Max(0, Mathf.CeilToInt(remaining * 2f));
                if (tenths != lastShownTenths && nextBtnLabel != null)
                {
                    lastShownTenths = tenths;
                    float shownSec = tenths * 0.5f;
                    nextBtnLabel.text = $"SANG MÀN {nextLevelIndex:00}: {nextName} ({shownSec:0.0}s)";
                }

                yield return null;
            }

            stageClearRoutine = null;
            ProceedWithTransition(nextLevelIndex);
        }

        private void ShowGrandVictoryOverlay()
        {
            Canvas canvas = ResolveUiCanvas();
            if (canvas == null)
            {
                GameSession.CurrentLevelIndex = 1;
                ProceedWithTransition(0);
                return;
            }

            if (grandVictoryOverlay != null) { Destroy(grandVictoryOverlay); }

            Font uiFont = ResolveUiFont(canvas);
            grandVictoryOverlay = CreateOverlayRoot("GrandVictoryOverlay", canvas);

            RectTransform cardInner = RuntimeUiHelper.CreateFramedCard(
                "VictoryCard", grandVictoryOverlay.transform, Vector2.zero, new Vector2(980f, 650f),
                new Color(1f, 0.85f, 0f, 1f), new Color(0.08f, 0.06f, 0.16f, 0.99f), 6f);

            // Cúp Vàng Pixel trung tâm + 2 Huy hiệu Nhóm 505 hai bên
            RectTransform trophyRt = RuntimeUiHelper.CreateUiRect("TrophyIcon", cardInner, new Vector2(0f, 240f), new Vector2(92f, 92f));
            Image trophyImg = trophyRt.gameObject.AddComponent<Image>();
            trophyImg.sprite = RuntimeUiHelper.GetTrophySprite();
            trophyImg.preserveAspect = true;
            trophyImg.raycastTarget = false;

            RectTransform leftShield = RuntimeUiHelper.CreateUiRect("ShieldLeft", cardInner, new Vector2(-380f, 240f), new Vector2(68f, 68f));
            Image leftImg = leftShield.gameObject.AddComponent<Image>();
            leftImg.sprite = RuntimeUiHelper.GetTeam505LogoSprite();
            leftImg.preserveAspect = true;
            leftImg.raycastTarget = false;

            RectTransform rightShield = RuntimeUiHelper.CreateUiRect("ShieldRight", cardInner, new Vector2(380f, 240f), new Vector2(68f, 68f));
            Image rightImg = rightShield.gameObject.AddComponent<Image>();
            rightImg.sprite = RuntimeUiHelper.GetTeam505LogoSprite();
            rightImg.preserveAspect = true;
            rightImg.raycastTarget = false;

            RuntimeUiHelper.CreateLegacyText("VictoryTitle", cardInner, uiFont, new Vector2(0f, 152f), new Vector2(920f, 66f),
                "CHÚC MỪNG PHÁ ĐẢO ONE HIT DOWN!", 58, new Color(1f, 0.87f, 0f, 1f));

            RuntimeUiHelper.CreateLegacyText("VictorySubtitle", cardInner, uiFont, new Vector2(0f, 102f), new Vector2(880f, 40f),
                "BẠN ĐÃ CHINH PHỤC TOÀN BỘ 5 MÀN THỬ THÁCH!", 34, new Color(0.40f, 1f, 0.62f, 1f));

            int sec = Mathf.FloorToInt(elapsedTime);
            string modeStr = GameSession.IsTwoPlayerMode ? "2P CO-OP" : "1P SOLO";
            string statsLine =
                $"CHẾ ĐỘ: {modeStr}  |  CỔ VẬT MÀN 5: {collectedRelics}/{totalRelicsRequired}  |  ĐIỂM MÀN 5: {totalScore}\n" +
                $"THỜI GIAN MÀN 5: {sec / 60:00}:{sec % 60:00}  |  SỐ LẦN CHẾT MÀN 5: {deathCount:00}";

            RectTransform statsBox = RuntimeUiHelper.CreateFramedCard("VictoryStatsBox", cardInner, new Vector2(0f, 18f),
                new Vector2(860f, 104f), new Color(0f, 0.82f, 0.94f, 1f), new Color(0.05f, 0.06f, 0.11f, 1f), 3f);
            RuntimeUiHelper.CreateLegacyText("VictoryStatsText", statsBox, uiFont, Vector2.zero, new Vector2(830f, 92f),
                statsLine, 29, Color.white);

            // Bảng Vinh danh 5 thành viên Nhóm 505
            RectTransform creditsBox = RuntimeUiHelper.CreateFramedCard("CreditsBox", cardInner, new Vector2(0f, -104f),
                new Vector2(860f, 110f), new Color(1f, 0.6f, 0.1f, 1f), new Color(0.07f, 0.09f, 0.16f, 1f), 3f);
            RuntimeUiHelper.CreateLegacyText("CreditsText", creditsBox, uiFont, Vector2.zero, new Vector2(830f, 98f),
                "NHÓM PHÁT TRIỂN - NHÓM 505 (ĐẠI HỌC THỦY LỢI)\nTạ Anh Tuấn  -  Trần Đức Trung  -  Trần Văn Tuyên  -  Phan Văn Tú  -  Hoàng Thị Giang Uyên",
                27, new Color(1f, 0.94f, 0.72f, 1f));

            // Đợi người chơi bấm nút xác nhận để quay lại Main Menu
            RuntimeUiHelper.CreateLegacyButton(
                "ConfirmMenuButton", cardInner, uiFont, new Vector2(-150f, -226f), new Vector2(460f, 68f),
                "XÁC NHẬN & VỀ MENU CHÍNH", 34, new Color(0.14f, 0.78f, 0.36f, 1f), Color.white,
                () =>
                {
                    GameSession.CurrentLevelIndex = 1;
                    ProceedWithTransition(0);
                });

            RuntimeUiHelper.CreateLegacyButton(
                "ReplayFinalButton", cardInner, uiFont, new Vector2(260f, -226f), new Vector2(280f, 68f),
                "CHƠI LẠI MÀN 5", 32, new Color(0.20f, 0.44f, 0.80f, 1f), Color.white,
                ResetLevel);
        }

        private void ProceedWithTransition(int targetLevelIndex)
        {
            if (irisTransition != null)
            {
                irisTransition.PlayTransition(() => LoadSceneOrMenu(targetLevelIndex));
            }
            else
            {
                LoadSceneOrMenu(targetLevelIndex);
            }
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
