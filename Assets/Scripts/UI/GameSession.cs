using OneHitDown.Player;
using UnityEngine;

namespace OneHitDown.UI
{
    public static class GameSession
    {
        private const string TwoPlayerKey = "OneHitDown.IsTwoPlayerMode";
        private const string P1SkinKey = "OneHitDown.P1Skin";
        private const string P2SkinKey = "OneHitDown.P2Skin";
        private const string CurrentLevelKey = "OneHitDown.CurrentLevelIndex";
        private const string UnlockedLevelKey = "OneHitDown.UnlockedLevel";
        private const int FirstLevel = 1;
        private const int LastLevel = 5;

        public static bool IsTwoPlayerMode
        {
            get => PlayerPrefs.GetInt(TwoPlayerKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(TwoPlayerKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static CharacterSkin P1Skin
        {
            get => ReadSkin(P1SkinKey, CharacterSkin.NinjaFrog);
            set
            {
                PlayerPrefs.SetInt(P1SkinKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static CharacterSkin P2Skin
        {
            get => ReadSkin(P2SkinKey, CharacterSkin.VirtualGuy);
            set
            {
                PlayerPrefs.SetInt(P2SkinKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static int CurrentLevelIndex
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(CurrentLevelKey, FirstLevel), FirstLevel, LastLevel);
            set
            {
                PlayerPrefs.SetInt(CurrentLevelKey, Mathf.Clamp(value, FirstLevel, LastLevel));
                PlayerPrefs.Save();
            }
        }

        public static int UnlockedLevel => Mathf.Clamp(PlayerPrefs.GetInt(UnlockedLevelKey, FirstLevel), FirstLevel, LastLevel);

        public static bool IsLevelUnlocked(int levelIndex)
        {
            return levelIndex >= FirstLevel && levelIndex <= UnlockedLevel;
        }

        public static void UnlockLevel(int levelIndex)
        {
            int nextLevel = Mathf.Clamp(levelIndex + 1, FirstLevel, LastLevel);
            if (nextLevel > UnlockedLevel)
            {
                PlayerPrefs.SetInt(UnlockedLevelKey, nextLevel);
                PlayerPrefs.Save();
            }
        }

        private static CharacterSkin ReadSkin(string key, CharacterSkin fallback)
        {
            int value = PlayerPrefs.GetInt(key, (int)fallback);
            if (value < (int)CharacterSkin.NinjaFrog || value > (int)CharacterSkin.MaskDude)
            {
                value = (int)fallback;
            }

            return (CharacterSkin)value;
        }
    }
}
