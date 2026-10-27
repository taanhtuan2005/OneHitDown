using OneHitDown.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private Button singlePlayerButton;
        [SerializeField] private Button twoPlayerButton;
        [SerializeField] private Button[] player1SkinButtons = new Button[4];
        [SerializeField] private Button[] player2SkinButtons = new Button[4];
        [SerializeField] private GameObject[] player1SelectionMarks = new GameObject[4];
        [SerializeField] private GameObject[] player2SelectionMarks = new GameObject[4];
        [SerializeField] private Button playButton;
        [SerializeField] private TMP_Text modeLabel;
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip errorClip;

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

            BindSkinButtons(player1SkinButtons, true);
            BindSkinButtons(player2SkinButtons, false);

            if (playButton != null)
            {
                playButton.onClick.AddListener(ShowLevelSelect);
            }

            RefreshSelection();
            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(false);
            }
        }

        public void SetSinglePlayerMode()
        {
            GameSession.IsTwoPlayerMode = false;
            RefreshSelection();
        }

        public void SetTwoPlayerMode()
        {
            GameSession.IsTwoPlayerMode = true;
            RefreshSelection();
        }

        public void SelectPlayer1Skin(int skinIndex)
        {
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

        public void ShowLevelSelect()
        {
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
                modeLabel.text = GameSession.IsTwoPlayerMode ? "CHẾ ĐỘ: 2 NGƯỜI" : "CHẾ ĐỘ: 1 NGƯỜI";
            }

            RefreshMarks(player1SelectionMarks, (int)GameSession.P1Skin);
            RefreshMarks(player2SelectionMarks, (int)GameSession.P2Skin);

            if (playButton != null)
            {
                playButton.interactable = !GameSession.IsTwoPlayerMode || GameSession.P1Skin != GameSession.P2Skin;
            }

            SetFeedback(string.Empty);
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
