using OneHitDown.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Các Panel UI")]
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Nút chế độ 1P / 2P")]
        [SerializeField] private Button singlePlayerButton;
        [SerializeField] private Button twoPlayerButton;
        [SerializeField] private Button modeToggleButton;
        [SerializeField] private TMP_Text modeLabel;

        [Header("Chọn Skin Nhân Vật (4 Skins)")]
        [SerializeField] private Button[] player1SkinButtons = new Button[4];
        [SerializeField] private Button[] player2SkinButtons = new Button[4];
        [SerializeField] private GameObject[] player1SelectionMarks = new GameObject[4];
        [SerializeField] private GameObject[] player2SelectionMarks = new GameObject[4];
        [SerializeField] private TMP_Text[] characterLabels = new TMP_Text[4];

        [Header("Các Nút Hành Động Chính")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Image audioIcon;
        [SerializeField] private Sprite audioOnSprite;
        [SerializeField] private Sprite audioOffSprite;

        [Header("Phản hồi & Âm thanh")]
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip errorClip;
        [SerializeField] private AudioClip clickClip;

        private void Awake()
        {
            if (singlePlayerButton != null)
            {
                singlePlayerButton.onClick.AddListener(SetSinglePlayerMode);
            }

            if (twoPlayerButton != null)
            {
                twoPlayerButton.onClick.AddListener(SetTwoPlayerMode);
            }

            if (modeToggleButton != null)
            {
                modeToggleButton.onClick.AddListener(ToggleMode);
            }

            if (playButton != null)
            {
                playButton.onClick.AddListener(PlayDirectly);
            }

            if (levelsButton != null)
            {
                levelsButton.onClick.AddListener(ShowLevelSelect);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(ToggleSettings);
            }

            if (audioButton != null)
            {
                audioButton.onClick.AddListener(ToggleAudio);
            }

            BindSkinButtons(player1SkinButtons, true);
            BindSkinButtons(player2SkinButtons, false);

            RefreshSelection();
            RefreshAudioIcon();

            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(false);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void ToggleMode()
        {
            PlayClickClip();
            GameSession.IsTwoPlayerMode = !GameSession.IsTwoPlayerMode;
            RefreshSelection();
        }

        public void SetSinglePlayerMode()
        {
            PlayClickClip();
            GameSession.IsTwoPlayerMode = false;
            RefreshSelection();
        }

        public void SetTwoPlayerMode()
        {
            PlayClickClip();
            GameSession.IsTwoPlayerMode = true;
            RefreshSelection();
        }

        public void ToggleAudio()
        {
            PlayClickClip();
            bool isMuted = AudioListener.pause;
            AudioListener.pause = !isMuted;
            RefreshAudioIcon();
        }

        private void RefreshAudioIcon()
        {
            if (audioIcon != null)
            {
                audioIcon.sprite = AudioListener.pause ? audioOffSprite : audioOnSprite;
            }
        }

        public void ToggleSettings()
        {
            PlayClickClip();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
            else
            {
                SetFeedback("Điều khiển: P1: A/D/W | P2: Phím Mũi Tên | R: Reset | ESC: Tạm dừng");
            }
        }

        public void OnCharacterCardClicked(int skinIndex)
        {
            PlayClickClip();
            if (!TryGetSkin(skinIndex, out CharacterSkin skin))
            {
                return;
            }

            if (!GameSession.IsTwoPlayerMode)
            {
                // Chế độ 1 người: Click là chọn cho P1
                GameSession.P1Skin = skin;
            }
            else
            {
                // Chế độ 2 người: Nếu chưa là P1 thì gán P1, nếu đã là P1 thì gán P2
                if (GameSession.P1Skin != skin)
                {
                    GameSession.P1Skin = skin;
                }
                else
                {
                    GameSession.P2Skin = skin;
                }
            }

            RefreshSelection();
        }

        public void SelectPlayer1Skin(int skinIndex)
        {
            PlayClickClip();
            if (!TryGetSkin(skinIndex, out CharacterSkin skin))
            {
                return;
            }

            if (GameSession.IsTwoPlayerMode && skin == GameSession.P2Skin)
            {
                PlayErrorClip();
            }

            GameSession.P1Skin = skin;
            RefreshSelection();
        }

        public void SelectPlayer2Skin(int skinIndex)
        {
            PlayClickClip();
            if (!TryGetSkin(skinIndex, out CharacterSkin skin))
            {
                return;
            }

            if (GameSession.IsTwoPlayerMode && skin == GameSession.P1Skin)
            {
                PlayErrorClip();
            }

            GameSession.P2Skin = skin;
            RefreshSelection();
        }

        public void PlayDirectly()
        {
            PlayClickClip();
            if (GameSession.IsTwoPlayerMode && GameSession.P1Skin == GameSession.P2Skin)
            {
                SetFeedback("P1 và P2 cần chọn hai nhân vật khác nhau.");
                PlayErrorClip();
                return;
            }

            int targetLevel = Mathf.Clamp(GameSession.CurrentLevelIndex, 1, 5);
            string sceneName = $"Level_{targetLevel:00}";
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                // Nếu màn hiện tại chưa có trong build, tìm màn chơi có sẵn (Level_03 hoặc Level_05)
                sceneName = Application.CanStreamedLevelBeLoaded("Level_03") ? "Level_03" : "Level_05";
            }

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                SetFeedback($"Chưa tìm thấy Scene {sceneName} trong Build Settings.");
            }
        }

        public void ShowLevelSelect()
        {
            PlayClickClip();
            if (GameSession.IsTwoPlayerMode && GameSession.P1Skin == GameSession.P2Skin)
            {
                SetFeedback("P1 và P2 cần chọn hai nhân vật khác nhau.");
                PlayErrorClip();
                return;
            }

            SetFeedback(string.Empty);
            if (characterPanel != null)
            {
                characterPanel.SetActive(false);
            }

            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(true);
            }
        }

        public void ShowCharacterSelect()
        {
            PlayClickClip();
            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(false);
            }

            if (characterPanel != null)
            {
                characterPanel.SetActive(true);
            }
        }

        public void QuitGame()
        {
            PlayClickClip();
            Application.Quit();
        }

        private void BindSkinButtons(Button[] buttons, bool player1)
        {
            if (buttons == null)
            {
                return;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                {
                    continue;
                }

                int skinIndex = i;
                if (player1)
                {
                    buttons[i].onClick.AddListener(() => SelectPlayer1Skin(skinIndex));
                }
                else
                {
                    buttons[i].onClick.AddListener(() => SelectPlayer2Skin(skinIndex));
                }
            }
        }

        private void RefreshSelection()
        {
            if (modeLabel != null)
            {
                modeLabel.text = GameSession.IsTwoPlayerMode
                    ? "CHỌN CHẾ ĐỘ: 1 PLAYER / <color=#00FFFF>[2 PLAYERS - CO-OP TETHER]</color>"
                    : "CHỌN CHẾ ĐỘ: <color=#00FFFF>[1 PLAYER]</color> / 2 PLAYERS - CO-OP TETHER";
            }

            RefreshMarks(player1SelectionMarks, (int)GameSession.P1Skin);
            RefreshMarks(player2SelectionMarks, (int)GameSession.P2Skin);
            RefreshCharacterLabels();

            if (playButton != null)
            {
                playButton.interactable = !GameSession.IsTwoPlayerMode || GameSession.P1Skin != GameSession.P2Skin;
            }

            SetFeedback(string.Empty);
        }

        private void RefreshCharacterLabels()
        {
            if (characterLabels == null)
            {
                return;
            }

            string[] names = { "NINJA FROG", "PINK MAN", "VIRTUAL GUY", "MASK DUDE" };
            for (int i = 0; i < characterLabels.Length && i < names.Length; i++)
            {
                if (characterLabels[i] == null)
                {
                    continue;
                }

                bool isP1 = (int)GameSession.P1Skin == i;
                bool isP2 = GameSession.IsTwoPlayerMode && (int)GameSession.P2Skin == i;

                if (isP1 && isP2)
                {
                    characterLabels[i].text = $"P1+P2: {names[i]}";
                    characterLabels[i].color = new Color(1f, 0.35f, 0.35f);
                }
                else if (isP1)
                {
                    characterLabels[i].text = $"P1: {names[i]}";
                    characterLabels[i].color = Color.white;
                }
                else if (isP2)
                {
                    characterLabels[i].text = $"P2: {names[i]}";
                    characterLabels[i].color = Color.white;
                }
                else
                {
                    characterLabels[i].text = names[i];
                    characterLabels[i].color = new Color(0.7f, 0.7f, 0.8f);
                }
            }
        }

        private static void RefreshMarks(GameObject[] marks, int selectedIndex)
        {
            if (marks == null)
            {
                return;
            }

            for (int i = 0; i < marks.Length; i++)
            {
                if (marks[i] != null)
                {
                    marks[i].SetActive(i == selectedIndex);
                }
            }
        }

        private void SetFeedback(string message)
        {
            if (feedbackLabel != null)
            {
                feedbackLabel.text = message;
            }
        }

        private void PlayErrorClip()
        {
            if (uiAudioSource != null && errorClip != null)
            {
                uiAudioSource.PlayOneShot(errorClip);
            }
        }

        private void PlayClickClip()
        {
            if (uiAudioSource != null && clickClip != null)
            {
                uiAudioSource.PlayOneShot(clickClip);
            }
        }

        private static bool TryGetSkin(int skinIndex, out CharacterSkin skin)
        {
            if (skinIndex < (int)CharacterSkin.NinjaFrog || skinIndex > (int)CharacterSkin.MaskDude)
            {
                skin = CharacterSkin.NinjaFrog;
                return false;
            }

            skin = (CharacterSkin)skinIndex;
            return true;
        }
    }
}
