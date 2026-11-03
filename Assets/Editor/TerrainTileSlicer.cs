#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Công cụ Editor cắt tự động toàn bộ 12 cụm Tileset trong 'Terrain (16x16).png'
    /// Bao gồm:
    /// - Khối Cỏ Xanh 9-slice (Terrain (16x16)_GrassBlock: 48x48, border 16px) chuẩn Màn 1
    /// - 9 Tile lẻ 16x16 của cụm Cỏ Xanh
    /// - Các khối 9-slice cho Đá, Gỗ, Đất cam, Cỏ hồng, Gạch đỏ, Thép, Vàng
    /// - Lưới 242 Tile 16x16 bao phủ 100% diện tích Texture 352x176 px.
    /// </summary>
    public static class TerrainTileSlicer
    {
        private const string TerrainTexturePath = "Assets/Art/02_MoiTruong/PA1_Terrain/Terrain (16x16).png";

        [MenuItem("Tools/One Hit Down/Slice All Terrain Sprites", false, 1)]
        public static void SliceAllSprites()
        {
            try
            {
                EditorUtility.DisplayProgressBar("One Hit Down", "Đang cắt toàn bộ Sprite trong Terrain (16x16).png...", 0.3f);
                Debug.Log("<b>[TerrainTileSlicer]</b> BẮT ĐẦU CẮT TOÀN BỘ TILES TERRAIN (16x16)...");

                TextureImporter importer = AssetImporter.GetAtPath(TerrainTexturePath) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogError($"[TerrainTileSlicer] Không tìm thấy TextureImporter tại {TerrainTexturePath}");
                    EditorUtility.ClearProgressBar();
                    return;
                }

                importer.isReadable = true;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
                factory.Init();
                ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
                if (provider == null)
                {
                    Debug.LogError("[TerrainTileSlicer] Không lấy được ISpriteEditorDataProvider.");
                    EditorUtility.ClearProgressBar();
                    return;
                }
                provider.InitSpriteEditorDataProvider();

                List<SpriteRect> rects = new List<SpriteRect>();

                // Helper thêm 1 sprite
                System.Action<string, Rect, Vector4> addRect = (name, rect, border) =>
                {
                    rects.Add(new SpriteRect
                    {
                        name = name,
                        rect = rect,
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                        border = border,
                        spriteID = GUID.Generate()
                    });
                };

                // =========================================================================
                // 1. CỤM CỎ XANH & ĐẤT (HÀNG 1, CỘT 2: x=96..144, y=128..176) - CHUẨN LEVEL 1
                // =========================================================================
                // Khối Cỏ Xanh 9-slice chuẩn (48x48, border 16px): Tự động co giãn ra mọi kích thước sàn
                addRect("Terrain (16x16)_GrassBlock", new Rect(96f, 128f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f));
                addRect("Terrain (16x16)_1", new Rect(96f, 128f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f)); // Đồng bộ tên cũ

                // Khối Dải Đất Cỏ Liền Mạch (16x48, border: 0, 16, 0, 16): Không có viền trái/phải, lặp ngang 100% liền mạch!
                addRect("Grass_SeamlessPillarStrip", new Rect(112f, 128f, 16f, 48f), new Vector4(0f, 16f, 0f, 16f));

                // 9 Tile lẻ 16x16 của Cỏ Xanh
                addRect("Grass_TopLeft", new Rect(96f, 160f, 16f, 16f), Vector4.zero);
                addRect("Grass_TopMid", new Rect(112f, 160f, 16f, 16f), Vector4.zero);
                addRect("Grass_TopRight", new Rect(128f, 160f, 16f, 16f), Vector4.zero);
                addRect("Grass_MidLeft", new Rect(96f, 144f, 16f, 16f), Vector4.zero);
                addRect("Grass_Center", new Rect(112f, 144f, 16f, 16f), Vector4.zero);
                addRect("Grass_MidRight", new Rect(128f, 144f, 16f, 16f), Vector4.zero);
                addRect("Grass_BotLeft", new Rect(96f, 128f, 16f, 16f), Vector4.zero);
                addRect("Grass_BotMid", new Rect(112f, 128f, 16f, 16f), Vector4.zero);
                addRect("Grass_BotRight", new Rect(128f, 128f, 16f, 16f), Vector4.zero);
                addRect("Grass_SingleBush", new Rect(144f, 144f, 16f, 16f), Vector4.zero);

                // =========================================================================
                // 2. CÁC CỤM KHỐI 9-SLICE KHÁC
                // =========================================================================
                // Đá Hầm Mộ (Row 1, Col 1: x=0..48, y=128..176)
                addRect("Terrain (16x16)_StoneBlock", new Rect(0f, 128f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f));
                addRect("Terrain (16x16)_StoneInnerBricks", new Rect(48f, 144f, 32f, 32f), Vector4.zero);

                // Thép & Dầm (Row 2, Col 3: x=192..256, y=64..112)
                addRect("Terrain (16x16)_SteelBlock2x2", new Rect(208f, 64f, 32f, 32f), Vector4.zero);
                addRect("Terrain (16x16)_SteelBeamHoriz", new Rect(192f, 96f, 48f, 16f), new Vector4(16f, 0f, 16f, 0f));
                addRect("Terrain (16x16)_SteelPillarVert", new Rect(240f, 64f, 16f, 48f), new Vector4(0f, 16f, 0f, 16f));

                // One-Way Platforms (Row 1, Col 4: x=272..320, y=128..176)
                addRect("Terrain (16x16)_OneWayGold", new Rect(272f, 160f, 48f, 16f), new Vector4(16f, 0f, 16f, 0f));
                addRect("Terrain (16x16)_OneWayGrey", new Rect(272f, 128f, 48f, 16f), new Vector4(16f, 0f, 16f, 0f));

                // Gỗ (Row 2, Col 1: x=0..48, y=64..112)
                addRect("Terrain (16x16)_WoodBlock", new Rect(0f, 64f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f));

                // Đất Cam / Mùa Thu (Row 2, Col 2: x=96..144, y=64..112)
                addRect("Terrain (16x16)_AutumnGrassBlock", new Rect(96f, 64f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f));

                // Gạch Đỏ (Row 2, Col 4: x=272..320, y=64..112)
                addRect("Terrain (16x16)_RedBrickBlock", new Rect(272f, 64f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f));

                // Hầm Ngục Xanh Lục / Cyan (Row 3, Col 1: x=0..48, y=0..48)
                addRect("Terrain (16x16)_CyanDungeonBlock", new Rect(0f, 0f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f));

                // Cỏ Hồng / Cát (Row 3, Col 2: x=96..144, y=0..48)
                addRect("Terrain (16x16)_PinkGrassBlock", new Rect(96f, 0f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f));

                // Đồng / Bronze (Row 3, Col 3: x=192..240, y=0..48)
                addRect("Terrain (16x16)_CopperBlock", new Rect(192f, 0f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f));

                // Vàng Kim / Golden (Row 3, Col 4: x=272..320, y=0..48)
                addRect("Terrain (16x16)_GoldenBlock", new Rect(272f, 0f, 48f, 48f), new Vector4(16f, 16f, 16f, 16f));

                // =========================================================================
                // 3. TOÀN BỘ LƯỚI TILE 16x16 (22 x 11 = 242 ô)
                // =========================================================================
                for (int gy = 0; gy < 11; gy++)
                {
                    for (int gx = 0; gx < 22; gx++)
                    {
                        float x = gx * 16f;
                        float y = gy * 16f;
                        string tileName = $"Tile_16x16_X{gx:D2}_Y{gy:D2}";
                        addRect(tileName, new Rect(x, y, 16f, 16f), Vector4.zero);
                    }
                }

                provider.SetSpriteRects(rects.ToArray());

                ISpriteNameFileIdDataProvider nameProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
                if (nameProvider != null)
                {
                    List<SpriteNameFileIdPair> pairs = new List<SpriteNameFileIdPair>();
                    foreach (var r in rects)
                    {
                        pairs.Add(new SpriteNameFileIdPair(r.name, r.spriteID));
                    }
                    nameProvider.SetNameFileIdPairs(pairs);
                }

                provider.Apply();
                importer.SaveAndReimport();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                Debug.Log($"<b>[TerrainTileSlicer]</b> ĐÃ CẮT THÀNH CÔNG {rects.Count} TILES CHO 'Terrain (16x16).png'!");
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[TerrainTileSlicer] Lỗi khi cắt Tiles: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
#endif

