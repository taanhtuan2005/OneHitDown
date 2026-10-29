// Hoàng Thị Giang Uyên (2351060497) — Module 5: 6 BGM Chiptune + 382 SFX/Jingle
// Docs/SCHEMA_CHI_TIET.md §3 Module 5.4 — Docs/Phan_cong.md Mục III (Người 5).
//
// Gán trong Inspector: 6 field BGM (Assets/Art/06_AmThanh/BGM_ChiptuneLoops/Audio/) và kéo
// TOÀN BỘ file .ogg của Assets/Art/06_AmThanh/ vào mảng sfxLibrary. Script tự lập bảng tra
// theo AudioClip.name nên mọi lời gọi PlaySFX("<tên file>") đều chạy được.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneHitDown.Core
{
    /// <summary>
    /// Quản trị âm thanh toàn màn chơi: 6 bản nhạc nền Chiptune, 382 file SFX/Jingle,
    /// bước chân theo địa hình từng màn và các âm báo sự kiện.
    /// </summary>
    [DefaultExecutionOrder(-90)]
    public class AudioManager : MonoBehaviour
    {
        #region Hằng số & Field SerializeField

        private const int LevelCount = 5;         // Level_01..Level_05
        private const int FootstepVariants = 5;   // mỗi địa hình có 5 biến thể 000..004

        [Header("Nguồn phát")]
        [SerializeField] private AudioSource bgmSource;      // nhạc nền lặp
        [SerializeField] private AudioSource sfxSource;      // hiệu ứng ngắn trong màn
        [SerializeField] private AudioSource jingleSource;   // nhạc thắng / thua

        [Header("6 bản nhạc nền Chiptune (BGM_ChiptuneLoops/Audio)")]
        [SerializeField] private AudioClip bgmMenu;            // bgm_menu.ogg
        [SerializeField] private AudioClip bgmLevel1Ruins;     // bgm_level1_ruins.ogg
        [SerializeField] private AudioClip bgmLevel2Grotto;    // bgm_level2_grotto.ogg
        [SerializeField] private AudioClip bgmLevel3IceEmber;  // bgm_level3_ice_ember.ogg
        [SerializeField] private AudioClip bgmLevel4Idols;     // bgm_level4_idols.ogg
        [SerializeField] private AudioClip bgmLevel5ChaosNexus;// bgm_level5_chaos_nexus.ogg

        [Header("Kho 382 file SFX/Jingle — kéo toàn bộ .ogg của 06_AmThanh vào đây")]
        [SerializeField] private AudioClip[] sfxLibrary;

        #endregion

        #region Bảng tra dựng sẵn (Zero-GC khi chạy)

        // Bảng tra tên file → clip, dựng một lần trong Awake để lúc chạy không cấp phát.
        private readonly Dictionary<string, AudioClip> sfxLookup =
            new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

        private AudioClip currentBgm;

        /// <summary>Singleton cho SoulTether, HUDController, PauseMenu và các module khác.</summary>
        public static AudioManager Instance { get; private set; }

        #endregion

        #region Vòng đời: Awake / OnDestroy

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            EnsureSource(ref bgmSource, true);
            EnsureSource(ref sfxSource, false);
            EnsureSource(ref jingleSource, false);

            if (sfxLibrary == null)
            {
                return;
            }

            foreach (AudioClip clip in sfxLibrary)
            {
                if (clip != null && !sfxLookup.ContainsKey(clip.name))
                {
                    sfxLookup.Add(clip.name, clip);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Tự bổ sung AudioSource nếu Inspector để trống.</summary>
        private void EnsureSource(ref AudioSource source, bool loop)
        {
            if (source != null)
            {
                return;
            }

            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
        }

        #endregion

        #region Phát nhạc nền (6 BGM Chiptune)

        /// <summary>1..5 phát bgm_level1..5; 0 hoặc ngoài khoảng phát nhạc Menu.</summary>
        public void PlayLevelBGM(int levelIndex)
        {
            AudioClip clip = levelIndex switch
            {
                1 => bgmLevel1Ruins,
                2 => bgmLevel2Grotto,
                3 => bgmLevel3IceEmber,
                4 => bgmLevel4Idols,
                5 => bgmLevel5ChaosNexus,
                _ => bgmMenu,
            };

            if (clip == null || bgmSource == null)
            {
                return;
            }

            // Bỏ qua nếu đúng bản đó đang phát dở (tránh cắt ngang khi chuyển cảnh).
            if (currentBgm == clip && bgmSource.isPlaying)
            {
                return;
            }

            currentBgm = clip;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        #endregion

        #region Phát SFX / Jingle / Bước chân

        /// <summary>Ví dụ <c>PlaySFX("powerUp2")</c>, <c>PlaySFX("impactBell_heavy_000")</c>.</summary>
        public void PlaySFX(string clipName)
        {
            PlayOn(sfxSource, clipName);
        }

        /// <summary>Ví dụ <c>PlayJingle("jingles_NES03")</c>.</summary>
        public void PlayJingle(string clipName)
        {
            PlayOn(jingleSource, clipName);
        }

        /// <summary>Tiếng bước chân ngẫu nhiên theo địa hình: Màn 1 cỏ, Màn 3 tuyết, còn lại bê tông.</summary>
        public void PlayFootstep(int levelIndex)
        {
            if (levelIndex < 1 || levelIndex > LevelCount)
            {
                return;
            }

            string prefix = levelIndex == 1 ? "footstep_grass_"
                : levelIndex == 3 ? "footstep_snow_"
                : "footstep_concrete_";

            int variant = UnityEngine.Random.Range(0, FootstepVariants);

            if (sfxLookup.TryGetValue(prefix + variant.ToString("000"), out AudioClip clip))
            {
                PlayOn(sfxSource, clip);
            }
        }

        #endregion

        #region Hàm phát nội bộ

        private void PlayOn(AudioSource source, AudioClip clip)
        {
            if (source != null && clip != null)
            {
                source.PlayOneShot(clip);
            }
        }

        private void PlayOn(AudioSource source, string clipName)
        {
            if (!string.IsNullOrEmpty(clipName) && sfxLookup.TryGetValue(clipName, out AudioClip clip))
            {
                PlayOn(source, clip);
            }
        }

        #endregion
    }
}
