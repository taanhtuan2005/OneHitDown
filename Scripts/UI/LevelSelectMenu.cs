using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    public sealed class LevelSelectMenu : MonoBehaviour
    {
        [SerializeField] private Button[] levelButtons = new Button[5];
        [SerializeField] private Image[] lockIndicators = new Image[5];
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private MainMenuController mainMenu;

        private void Awake()
        {
            int buttonCount = levelButtons != null ? levelButtons.Length : 0;
            for (int i = 0; i < buttonCount; i++)
            {
                if (levelButtons[i] != null)
                {
                    int levelIndex = i + 1;
                    levelButtons[i].onClick.AddListener(() => SelectLevel(levelIndex));
                }
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(GoBack);
            }

            RefreshLevels();
        }

        private void OnEnable()
        {
            RefreshLevels();
        }

        public void RefreshLevels()
        {
            int buttonCount = levelButtons != null ? levelButtons.Length : 0;
            int iconCount = lockIndicators != null ? lockIndicators.Length : 0;
            int count = Mathf.Min(5, Mathf.Max(buttonCount, iconCount));

            for (int i = 0; i < count; i++)
            {
                bool unlocked = GameSession.IsLevelUnlocked(i + 1);
                if (i < buttonCount && levelButtons[i] != null)
                {
                    levelButtons[i].interactable = unlocked;
                }

                if (i < iconCount && lockIndicators[i] != null)
                {
                    lockIndicators[i].sprite = unlocked ? unlockedSprite : lockedSprite;
                    lockIndicators[i].enabled = lockIndicators[i].sprite != null;
                }
            }
        }

        public void SelectLevel(int levelIndex)
        {
            if (!GameSession.IsLevelUnlocked(levelIndex))
            {
                SetFeedback("Màn chơi này đang bị khóa.");
                return;
            }

            string sceneName = $"Level_{levelIndex:00}";
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SetFeedback($"Chưa có scene {sceneName} trong Build Settings.");
                return;
            }

            GameSession.CurrentLevelIndex = levelIndex;
            SceneManager.LoadScene(sceneName);
        }

        public void GoBack()
        {
            SetFeedback(string.Empty);
            if (mainMenu != null)
            {
                mainMenu.ShowCharacterSelect();
            }
        }

        private void SetFeedback(string message)
        {
            if (feedbackLabel != null)
            {
                feedbackLabel.text = message;
            }
        }
    }
}
