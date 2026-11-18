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
            AutoBindLevelButtons();

            int buttonCount = levelButtons != null ? levelButtons.Length : 0;
            for (int i = 0; i < buttonCount; i++)
            {
                if (levelButtons[i] != null)
                {
                    int levelIndex = i + 1;
                    levelButtons[i].onClick.RemoveAllListeners();
                    levelButtons[i].onClick.AddListener(() => SelectLevel(levelIndex));

                    // Đảm bảo icon lock và các ảnh con không chặn click vào button
                    Image[] childImgs = levelButtons[i].GetComponentsInChildren<Image>(true);
                    foreach (var img in childImgs)
                    {
                        if (img.gameObject != levelButtons[i].gameObject)
                        {
                            img.raycastTarget = false;
                        }
                    }

                    TMP_Text[] childTexts = levelButtons[i].GetComponentsInChildren<TMP_Text>(true);
                    foreach (var txt in childTexts)
                    {
                        txt.raycastTarget = false;
                    }
                }
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(GoBack);

                Image[] bImgs = backButton.GetComponentsInChildren<Image>(true);
                foreach (var img in bImgs)
                {
                    if (img.gameObject != backButton.gameObject) img.raycastTarget = false;
                }
                TMP_Text[] bTexts = backButton.GetComponentsInChildren<TMP_Text>(true);
                foreach (var txt in bTexts) txt.raycastTarget = false;
            }

            RefreshLevels();
        }

        private void AutoBindLevelButtons()
        {
            if (levelButtons == null || levelButtons.Length < 5 || levelButtons[0] == null)
            {
                levelButtons = new Button[5];
                for (int i = 0; i < 5; i++)
                {
                    int lvlNum = i + 1;
                    Transform tr = transform.Find($"LevelBox/Inner/LevelBtn_{lvlNum:00}");
                    if (tr == null)
                    {
                        GameObject found = GameObject.Find($"LevelBtn_{lvlNum:00}");
                        if (found != null) tr = found.transform;
                    }
                    if (tr != null)
                    {
                        levelButtons[i] = tr.GetComponent<Button>();
                    }
                }
            }

            if (backButton == null)
            {
                Transform btr = transform.Find("LevelBox/Inner/BackButton");
                if (btr == null)
                {
                    GameObject found = GameObject.Find("BackButton");
                    if (found != null) btr = found.transform;
                }
                if (btr != null) backButton = btr.GetComponent<Button>();
            }

            if (feedbackLabel == null)
            {
                Transform ftr = transform.Find("LevelBox/Inner/Feedback");
                if (ftr != null) feedbackLabel = ftr.GetComponent<TMP_Text>();
            }
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
                SetFeedback($"<color=#FF4444>Màn {levelIndex} đang khóa! Hãy vượt qua màn {levelIndex - 1}.</color>");
                return;
            }

            string sceneName = $"Level_{levelIndex:00}";
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SetFeedback($"<color=#FF4444>Chưa có scene {sceneName} trong Build Settings.</color>");
                return;
            }

            GameSession.CurrentLevelIndex = levelIndex;
            Debug.Log($"[LevelSelectMenu] Vào màn chơi: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public void GoBack()
        {
            SetFeedback(string.Empty);
            if (mainMenu != null)
            {
                mainMenu.ShowCharacterSelect();
            }
            else
            {
                gameObject.SetActive(false);
                GameObject cp = GameObject.Find("CharacterPanel");
                if (cp != null) cp.SetActive(true);
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
