#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using TMPro;

[InitializeOnLoad]
public static class CyrillicFontEditorSetup
{
    static CyrillicFontEditorSetup()
    {
        EditorApplication.delayCall += EnsureCyrillicFontAsset;
    }

    [MenuItem("Tools/WarDeck/Setup Cyrillic Fonts")]
    public static void EnsureCyrillicFontAsset()
    {
        string targetFolder = "Assets/TextMesh Pro/Resources/Fonts & Materials";
        string assetPath = targetFolder + "/Arial_Cyrillic_SDF.asset";

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);

        // Проверяем, существует ли ассет и имеет ли он валидный материал
        if (fontAsset == null || fontAsset.material == null)
        {
            string fontFilePath = Path.GetFullPath("Assets/Fonts/arial.ttf");
            if (!File.Exists(fontFilePath))
            {
                fontFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts), "arial.ttf");
            }

            if (File.Exists(fontFilePath))
            {
                fontAsset = TMP_FontAsset.CreateFontAsset(
                    fontFilePath,
                    0,
                    90,
                    9,
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                    1024,
                    1024
                );

                if (fontAsset != null)
                {
                    fontAsset.name = "Arial_Cyrillic_SDF";

                    if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro/Resources/Fonts & Materials"))
                    {
                        if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro/Resources"))
                        {
                            AssetDatabase.CreateFolder("Assets/TextMesh Pro", "Resources");
                        }
                        AssetDatabase.CreateFolder("Assets/TextMesh Pro/Resources", "Fonts & Materials");
                    }

                    AssetDatabase.CreateAsset(fontAsset, assetPath);

                    if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
                    {
                        fontAsset.atlasTextures[0].name = "Arial_Cyrillic_SDF Atlas";
                        AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
                    }

                    if (fontAsset.material != null)
                    {
                        fontAsset.material.name = "Arial_Cyrillic_SDF Material";
                        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                    }

                    EditorUtility.SetDirty(fontAsset);
                    AssetDatabase.SaveAssets();
                    Debug.Log("<color=green>[CyrillicFont]</color> Создан валидный ассет шрифта Arial_Cyrillic_SDF с материалом и атласом!");
                }
            }
        }

        if (fontAsset != null && fontAsset.material != null)
        {
            // Подключаем к дефолтному шрифту LiberationSans SDF
            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null)
            {
                if (defaultFont.fallbackFontAssetTable == null)
                    defaultFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

                // Удаляем битые ссылки
                defaultFont.fallbackFontAssetTable.RemoveAll(f => f == null || f.material == null);

                if (!defaultFont.fallbackFontAssetTable.Contains(fontAsset))
                {
                    defaultFont.fallbackFontAssetTable.Insert(0, fontAsset);
                    EditorUtility.SetDirty(defaultFont);
                }
            }

            // Подключаем к глобальным фоллбэкам TMP Settings
            if (TMP_Settings.fallbackFontAssets == null)
                TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>();

            // Удаляем битые ссылки
            TMP_Settings.fallbackFontAssets.RemoveAll(f => f == null || f.material == null);

            if (!TMP_Settings.fallbackFontAssets.Contains(fontAsset))
            {
                TMP_Settings.fallbackFontAssets.Insert(0, fontAsset);
                if (TMP_Settings.instance != null)
                {
                    EditorUtility.SetDirty(TMP_Settings.instance);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("<color=green>[CyrillicFont]</color> Фоллбэки шрифтов TextMeshPro успешно настроены.");
        }
    }
}
#endif
