using UnityEditor;
using UnityEngine;

namespace Abissal.EditorTools
{
    /// <summary>
    /// Aplica as configurações de pixel art a toda imagem colocada em Art/Sprites.
    /// Assim, uma arte nova ou substituída já entra nítida, sem ajuste manual.
    /// </summary>
    public class SpriteImportRules : AssetPostprocessor
    {
        private const string SpriteRoot = "Assets/_Abissal/Art/Sprites/";
        private const int PixelsPerUnit = 16;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(SpriteRoot))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spritePixelsPerUnit = PixelsPerUnit;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            settings.spriteAlignment = (int)(UsesBottomPivot() ? SpriteAlignment.BottomCenter : SpriteAlignment.Center);
            importer.SetTextureSettings(settings);
        }

        /// <summary>Plantas e objetos apoiados crescem a partir da base.</summary>
        private bool UsesBottomPivot()
        {
            return assetPath.Contains("/Plants/") || assetPath.Contains("/Props/");
        }
    }
}
