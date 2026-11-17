#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Công cụ Editor tự động tối ưu hóa toàn bộ Sprite nhân vật chuẩn Pixel Art:
    /// 1. Filter Mode: Point (no filter) -> Triệt tiêu 100% hiện tượng out nét / mờ nhòe.
    /// 2. Compression: None (Uncompressed) -> Giữ nguyên viền sắc nét của pixel 32x32.
    /// 3. Aniso Level: 0, Mipmap: False.
    /// </summary>
    public static class CharacterPixelArtFixer
    {
        private const string CharactersFolder = "Assets/Art/01_NhanVat";

        [MenuItem("Tools/One Hit Down/Fix Character Pixel Art Sharpness", false, 3)]
        public static void FixAllCharacterTextures()
        {
            try
            {
                EditorUtility.DisplayProgressBar("One Hit Down", "Đang tối ưu độ sắc nét Pixel Art cho nhân vật...", 0.2f);
                Debug.Log("<b>[CharacterPixelArtFixer]</b> BẮT ĐẦU TỐI ƯU SẮC NÉT PIXEL ART CHO NHÂN VẬT...");

                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { CharactersFolder });
                int fixedCount = 0;

                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    bool modified = false;

                    if (importer.filterMode != FilterMode.Point)
                    {
                        importer.filterMode = FilterMode.Point;
                        modified = true;
                    }

                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        modified = true;
                    }

                    if (importer.anisoLevel != 0)
                    {
                        importer.anisoLevel = 0;
                        modified = true;
                    }

                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        modified = true;
                    }

                    TextureImporterPlatformSettings defSettings = importer.GetDefaultPlatformTextureSettings();
                    if (defSettings.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        defSettings.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SetPlatformTextureSettings(defSettings);
                        modified = true;
                    }

                    if (modified)
                    {
                        importer.SaveAndReimport();
                        fixedCount++;
                        Debug.Log($"[CharacterPixelArtFixer] Đã tối ưu độ sắc nét cho: {path}");
                    }
                }

                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"<b>[CharacterPixelArtFixer]</b> HOÀN TẤT! Đã xử lý {fixedCount}/{guids.Length} texture nhân vật sang chế độ Point + Uncompressed!");
                EditorUtility.DisplayDialog("One Hit Down", $"Đã tối ưu sắc nét Pixel Art thành công cho {fixedCount} texture nhân vật!", "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[CharacterPixelArtFixer] Lỗi: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
#endif

