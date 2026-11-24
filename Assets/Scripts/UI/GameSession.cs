using OneHitDown.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHitDown.UI
{
    public static class GameSession
    {
        private const string TwoPlayerKey = "OneHitDown.IsTwoPlayerMode";
        private const string P1SkinKey = "OneHitDown.P1Skin";
        private const string P2SkinKey = "OneHitDown.P2Skin";
        private const string CurrentLevelKey = "OneHitDown.CurrentLevelIndex";
        private const string UnlockedLevelKey = "OneHitDown.UnlockedLevel";
        private const string HasPausedKey = "OneHitDown.HasPausedLevel";
        private const string PausedLevelKey = "OneHitDown.PausedLevelIndex";
        private const string PausedTimeKey = "OneHitDown.PausedElapsedTime";
        private const string PausedDeathsKey = "OneHitDown.PausedDeathCount";
        private const int FirstLevel = 1;
        private const int LastLevel = 5;

        public static bool HasPausedLevel
        {
            get => PlayerPrefs.GetInt(HasPausedKey, 0) == 1;
            private set
            {
                PlayerPrefs.SetInt(HasPausedKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static int PausedLevelIndex
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(PausedLevelKey, FirstLevel), FirstLevel, LastLevel);
            private set
            {
                PlayerPrefs.SetInt(PausedLevelKey, Mathf.Clamp(value, FirstLevel, LastLevel));
                PlayerPrefs.Save();
            }
        }

        public static float PausedElapsedTime
        {
            get => Mathf.Max(0f, PlayerPrefs.GetFloat(PausedTimeKey, 0f));
            private set
            {
                PlayerPrefs.SetFloat(PausedTimeKey, Mathf.Max(0f, value));
                PlayerPrefs.Save();
            }
        }

        public static int PausedDeathCount
        {
            get => Mathf.Max(0, PlayerPrefs.GetInt(PausedDeathsKey, 0));
            private set
            {
                PlayerPrefs.SetInt(PausedDeathsKey, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

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

        /// <summary>Lưu thông tin màn đang chơi khi người chơi tạm dừng và quay về Menu chính.</summary>
        public static void SetPausedSession(int levelIndex, float elapsedTime, int deathCount)
        {
            int clamped = Mathf.Clamp(levelIndex, FirstLevel, LastLevel);
            PlayerPrefs.SetInt(HasPausedKey, 1);
            PlayerPrefs.SetInt(PausedLevelKey, clamped);
            PlayerPrefs.SetFloat(PausedTimeKey, Mathf.Max(0f, elapsedTime));
            PlayerPrefs.SetInt(PausedDeathsKey, Mathf.Max(0, deathCount));
            PlayerPrefs.SetInt(CurrentLevelKey, clamped);
            PlayerPrefs.Save();
        }

        /// <summary>Xóa trạng thái tạm dừng (khi hoàn thành màn hoặc đặt lại tiến độ).</summary>
        public static void ClearPausedSession()
        {
            PlayerPrefs.SetInt(HasPausedKey, 0);
            PlayerPrefs.SetFloat(PausedTimeKey, 0f);
            PlayerPrefs.SetInt(PausedDeathsKey, 0);
            PlayerPrefs.Save();
        }

        /// <summary>Mở khóa toàn bộ 5 màn chơi để kiểm thử hoặc trình diễn nhanh.</summary>
        public static void UnlockAllLevels()
        {
            PlayerPrefs.SetInt(UnlockedLevelKey, LastLevel);
            PlayerPrefs.Save();
        }

        /// <summary>Mỗi lần khởi động game mới: chỉ mở Màn 1, khóa Màn 2..5.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeFreshSessionOnStartup()
        {
            ResetAllProgress();
        }

        /// <summary>Đặt lại tiến độ mở khóa về Màn 1 và xóa trạng thái tạm dừng.</summary>
        public static void ResetAllProgress()
        {
            PlayerPrefs.SetInt(UnlockedLevelKey, FirstLevel);
            PlayerPrefs.SetInt(CurrentLevelKey, FirstLevel);
            PlayerPrefs.SetInt(HasPausedKey, 0);
            PlayerPrefs.SetFloat(PausedTimeKey, 0f);
            PlayerPrefs.SetInt(PausedDeathsKey, 0);
            PlayerPrefs.Save();
        }

        /// <summary>Trả về tên chủ đề tiếng Việt của từng màn (1..5).</summary>
        public static string GetLevelDisplayName(int levelIndex)
        {
            return levelIndex switch
            {
                1 => "ĐỒI XANH",
                2 => "HẺM SA THẠCH",
                3 => "HANG BĂNG TUYẾT",
                4 => "LÂU ĐÀI CỔ",
                5 => "ĐỊA NGỤC DUNG NHAM",
                _ => "MÀN CHƠI"
            };
        }

        /// <summary>Trả về tên hiển thị có dấu cách chuẩn của Skin nhân vật.</summary>
        public static string GetSkinDisplayName(CharacterSkin skin)
        {
            return skin switch
            {
                CharacterSkin.NinjaFrog => "NINJA FROG",
                CharacterSkin.VirtualGuy => "VIRTUAL GUY",
                CharacterSkin.PinkMan => "PINK MAN",
                CharacterSkin.MaskDude => "MASK DUDE",
                _ => skin.ToString().ToUpperInvariant()
            };
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

    /// <summary>
    /// Tiện ích dựng UI runtime & Pixel Art Sprite dùng chung cho Menu, Pause, Stage Clear và Victory.
    /// Toàn bộ Sprite chỉ sinh 1 lần rồi cache (Zero-GC trong Update).
    /// </summary>
    internal static class RuntimeUiHelper
    {
        private static Sprite cachedLogoSprite;
        private static Sprite cachedTrophySprite;
        private static Sprite cachedPauseButtonSprite;

        /// <summary>Trả về Font TextMeshPro chuẩn có đủ atlas (fallback an toàn nếu chưa gán).</summary>
        public static TMP_FontAsset GetSafeTmpFont()
        {
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            return font != null ? font : TMP_Settings.defaultFontAsset;
        }

        /// <summary>Tạo (hoặc trả về bản cache) Sprite nút Tạm dừng Pixel Art 22x22 đồng bộ với Restart.png.</summary>
        public static Sprite GetPauseButtonSprite()
        {
            if (cachedPauseButtonSprite != null) return cachedPauseButtonSprite;

            const int size = 22;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color border = new Color(0.13f, 0.14f, 0.22f, 1f);
            Color highlight = Color.white;
            Color face = new Color(0.91f, 0.93f, 0.97f, 1f);
            Color shade = new Color(0.68f, 0.72f, 0.82f, 1f);

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            void SetPx(int x, int y, Color c)
            {
                if (x >= 0 && x < size && y >= 0 && y < size) pixels[y * size + x] = c;
            }

            // Vẽ khung nút bo 4 góc 1px kiểu Pixel Adventure
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isCorner = (x == 0 || x == size - 1) && (y == 0 || y == size - 1);
                    if (isCorner) continue;

                    bool isOuter = x == 0 || x == size - 1 || y == 0 || y == size - 1
                        || ((x == 1 || x == size - 2) && (y == 1 || y == size - 2));
                    if (isOuter) SetPx(x, y, border);
                    else if (y == size - 2 || x == 1) SetPx(x, y, highlight);
                    else if (y == 1 || y == 2 || x == size - 2) SetPx(x, y, shade);
                    else SetPx(x, y, face);
                }
            }

            // Hai thanh Tạm dừng || sắc nét ở giữa
            for (int y = 6; y <= 15; y++)
            {
                for (int x = 6; x <= 9; x++) SetPx(x, y, border);
                for (int x = 12; x <= 15; x++) SetPx(x, y, border);
            }

            tex.SetPixels(pixels);
            tex.Apply();

            cachedPauseButtonSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            return cachedPauseButtonSprite;
        }

        /// <summary>Tạo (hoặc trả về bản cache) Huy hiệu Pixel Art 48x48 của Nhóm 505 × One Hit Down.</summary>
        public static Sprite GetTeam505LogoSprite()
        {
            if (cachedLogoSprite != null) return cachedLogoSprite;

            const int size = 48;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color goldLight = new Color(1f, 0.94f, 0.35f, 1f);
            Color gold = new Color(1f, 0.84f, 0f, 1f);
            Color goldDark = new Color(0.75f, 0.52f, 0.05f, 1f);
            Color navy = new Color(0.09f, 0.11f, 0.22f, 1f);
            Color navyInner = new Color(0.14f, 0.17f, 0.32f, 1f);
            Color cyan = new Color(0f, 0.95f, 1f, 1f);
            Color orange = new Color(1f, 0.58f, 0.08f, 1f);
            Color white = Color.white;

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            void SetPx(int x, int y, Color c)
            {
                if (x >= 0 && x < size && y >= 0 && y < size) pixels[y * size + x] = c;
            }

            void FillRect(int x0, int y0, int w, int h, Color c)
            {
                for (int y = y0; y < y0 + h; y++)
                    for (int x = x0; x < x0 + w; x++)
                        SetPx(x, y, c);
            }

            // Vẽ hình khiên cân đối (y tính từ đáy 0 lên đỉnh 47)
            for (int y = 4; y <= 38; y++)
            {
                int halfWidth;
                if (y >= 20)
                {
                    halfWidth = 19;
                }
                else
                {
                    // Thu nhọn dần về mũi khiên ở đáy
                    float t = (y - 4) / 16f;
                    halfWidth = Mathf.RoundToInt(Mathf.Lerp(2f, 19f, t));
                }

                int cx = size / 2;
                for (int x = cx - halfWidth; x < cx + halfWidth; x++)
                {
                    bool isOuter = (y == 38 || y == 37 || y == 4 || x <= cx - halfWidth + 1 || x >= cx + halfWidth - 2);
                    bool isInnerEdge = (y == 36 || x == cx - halfWidth + 2 || x == cx + halfWidth - 3);
                    if (isOuter) SetPx(x, y, gold);
                    else if (isInnerEdge) SetPx(x, y, goldDark);
                    else SetPx(x, y, (x + y) % 4 == 0 ? navyInner : navy);
                }
            }

            // Vương miện pixel phía trên khiên (y = 39..45)
            FillRect(14, 39, 20, 3, gold);
            FillRect(14, 42, 4, 3, goldLight);
            FillRect(22, 42, 4, 4, goldLight);
            FillRect(30, 42, 4, 3, goldLight);
            SetPx(15, 45, cyan);
            SetPx(16, 45, cyan);
            SetPx(23, 46, white);
            SetPx(24, 46, white);
            SetPx(31, 45, orange);
            SetPx(32, 45, orange);

            // Hai viên ngọc Cyan (P1) & Cam (P2) ở hai vai khiên nối bởi đường xích vàng
            FillRect(10, 29, 5, 5, cyan);
            FillRect(11, 30, 2, 2, white);
            FillRect(33, 29, 5, 5, orange);
            FillRect(34, 30, 2, 2, white);
            for (int x = 16; x <= 31; x += 3)
            {
                FillRect(x, 31, 2, 2, goldLight);
            }

            // Vẽ số "505" bằng pixel 5x7 ở trung tâm khiên (y = 16..24)
            string[] g5 = { "11111", "10000", "11110", "00001", "00001", "10001", "01110" };
            string[] g0 = { "01110", "10001", "10011", "10101", "11001", "10001", "01110" };
            void DrawGlyph(string[] g, int startX, int topY, Color c)
            {
                for (int row = 0; row < g.Length; row++)
                {
                    for (int col = 0; col < g[row].Length; col++)
                    {
                        if (g[row][col] == '1')
                        {
                            SetPx(startX + col, topY - row, c);
                        }
                    }
                }
            }

            DrawGlyph(g5, 13, 24, goldDark);
            DrawGlyph(g0, 21, 24, goldDark);
            DrawGlyph(g5, 29, 24, goldDark);
            DrawGlyph(g5, 13, 25, white);
            DrawGlyph(g0, 21, 25, goldLight);
            DrawGlyph(g5, 29, 25, white);

            // Mũi sáng dưới đáy khiên
            FillRect(22, 10, 4, 2, cyan);
            FillRect(22, 7, 4, 2, orange);

            tex.SetPixels(pixels);
            tex.Apply();

            cachedLogoSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            return cachedLogoSprite;
        }

        /// <summary>Tạo (hoặc trả về bản cache) Cúp Vàng Pixel Art 32x32 cho màn hình Chiến thắng.</summary>
        public static Sprite GetTrophySprite()
        {
            if (cachedTrophySprite != null) return cachedTrophySprite;

            const int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color goldLight = new Color(1f, 0.96f, 0.52f, 1f);
            Color gold = new Color(1f, 0.84f, 0f, 1f);
            Color goldDark = new Color(0.76f, 0.5f, 0.04f, 1f);
            Color baseBrown = new Color(0.45f, 0.28f, 0.1f, 1f);
            Color white = Color.white;

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            void FillRect(int x0, int y0, int w, int h, Color c)
            {
                for (int y = y0; y < y0 + h; y++)
                    for (int x = x0; x < x0 + w; x++)
                        if (x >= 0 && x < size && y >= 0 && y < size) pixels[y * size + x] = c;
            }

            // Đế cúp
            FillRect(9, 2, 14, 3, baseBrown);
            FillRect(11, 5, 10, 2, goldDark);
            // Cổ cúp
            FillRect(14, 7, 4, 6, gold);
            FillRect(14, 7, 1, 6, goldLight);
            // Bầu cúp
            FillRect(9, 13, 14, 4, goldDark);
            FillRect(8, 17, 16, 10, gold);
            FillRect(10, 18, 3, 8, goldLight);
            // Miệng cúp
            FillRect(7, 26, 18, 2, goldLight);
            // Tay cầm trái/phải
            FillRect(4, 18, 4, 2, gold);
            FillRect(4, 20, 2, 5, gold);
            FillRect(6, 23, 2, 2, gold);
            FillRect(24, 18, 4, 2, goldDark);
            FillRect(26, 20, 2, 5, goldDark);
            FillRect(24, 23, 2, 2, goldDark);
            // Tia sáng lấp lánh
            FillRect(2, 27, 3, 1, white);
            FillRect(3, 26, 1, 3, white);
            FillRect(27, 27, 3, 1, white);
            FillRect(28, 26, 1, 3, white);

            tex.SetPixels(pixels);
            tex.Apply();

            cachedTrophySprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            return cachedTrophySprite;
        }

        /// <summary>Tạo đối tượng UI cơ bản có RectTransform gắn vào parent.</summary>
        public static RectTransform CreateUiRect(string name, Transform parent, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return rt;
        }

        /// <summary>Tạo Image phủ toàn bộ kích thước RectTransform cha.</summary>
        public static Image CreateStretchPanel(string name, Transform parent, Color color, bool raycastTarget = true)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = raycastTarget;
            return img;
        }

        /// <summary>Tạo hộp khung viền 2 lớp chuẩn Pixel Arcade, trả về RectTransform lõi bên trong.</summary>
        public static RectTransform CreateFramedCard(string name, Transform parent, Vector2 anchoredPos, Vector2 size,
            Color borderColor, Color innerColor, float borderThickness = 5f)
        {
            RectTransform outer = CreateUiRect(name, parent, anchoredPos, size);
            Image outerImg = outer.gameObject.AddComponent<Image>();
            outerImg.color = borderColor;
            outerImg.raycastTarget = true;

            Image innerImg = CreateStretchPanel("Inner", outer, innerColor, false);
            RectTransform inner = innerImg.rectTransform;
            inner.offsetMin = new Vector2(borderThickness, borderThickness);
            inner.offsetMax = new Vector2(-borderThickness, -borderThickness);
            return inner;
        }

        /// <summary>Tạo Text UI chuẩn Unity (dùng font VT323 từ CoreRig để đồng bộ HUD trong màn chơi).</summary>
        public static Text CreateLegacyText(string name, Transform parent, Font font, Vector2 pos, Vector2 size,
            string content, int fontSize, Color color, TextAnchor align = TextAnchor.MiddleCenter)
        {
            RectTransform rt = CreateUiRect(name, parent, pos, size);
            Text txt = rt.gameObject.AddComponent<Text>();
            if (font != null) txt.font = font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        /// <summary>Tạo nút bấm UI chuẩn Unity (dùng font VT323 trong màn chơi).</summary>
        public static Button CreateLegacyButton(string name, Transform parent, Font font, Vector2 pos, Vector2 size,
            string label, int fontSize, Color bgColor, Color textColor, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform rt = CreateUiRect(name, parent, pos, size);
            Image bg = rt.gameObject.AddComponent<Image>();
            bg.color = bgColor;
            bg.raycastTarget = true;

            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;

            Text txt = CreateLegacyText("Label", rt, font, Vector2.zero, new Vector2(size.x - 16f, size.y - 8f),
                label, fontSize, textColor, TextAnchor.MiddleCenter);
            txt.raycastTarget = false;

            if (onClick != null) btn.onClick.AddListener(onClick);
            return btn;
        }
    }
}
