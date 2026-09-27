using UnityEditor;
using UnityEngine;

/// <summary>
/// Картинки из Assets/Cards/Art сразу импортируются как Sprite
/// с полной прямоугольной сеткой, чтобы заполнять карточку без щелей.
/// </summary>
public class CardArtPostprocessor : AssetPostprocessor
{
    private const string ArtFolder = "Assets/Cards/Art/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.Replace('\\', '/').StartsWith(ArtFolder))
        {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.textureType = TextureImporterType.Sprite;
        settings.spriteMode = (int)SpriteImportMode.Single;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spritePivot = new Vector2(0.5f, 0.5f);
        settings.spriteExtrude = 0;
        settings.alphaIsTransparency = true;
        importer.SetTextureSettings(settings);
    }
}
