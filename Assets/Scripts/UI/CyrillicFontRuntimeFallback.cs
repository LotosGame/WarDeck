using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class CyrillicFontRuntimeFallback
{
    private static TMP_FontAsset cyrillicFontAsset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeBeforeSceneLoad()
    {
        EnsureCyrillicFallback();
    }

    public static TMP_FontAsset GetCyrillicFont()
    {
        if (cyrillicFontAsset == null)
        {
            EnsureCyrillicFallback();
        }
        return cyrillicFontAsset;
    }

    public static void Apply(TMP_Text textComponent)
    {
        if (textComponent == null) return;
        TMP_FontAsset font = GetCyrillicFont();
        if (font != null)
        {
            textComponent.font = font;
        }
    }

    private static void EnsureCyrillicFallback()
    {
        if (cyrillicFontAsset != null) return;

        try
        {
            // 1. Пытаемся загрузить ассет из Resources
            TMP_FontAsset loaded = Resources.Load<TMP_FontAsset>("Fonts & Materials/Arial_Cyrillic_SDF");
            if (loaded != null && loaded.material != null)
            {
                cyrillicFontAsset = loaded;
            }

            // 2. Если ассет не найден или ссылается на несуществующий файл, создаем динамический инстанс из TTF
            if (cyrillicFontAsset == null)
            {
                string fontFilePath = System.IO.Path.Combine(Application.dataPath, "Fonts", "arial.ttf");
                if (!System.IO.File.Exists(fontFilePath))
                {
                    fontFilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts), "arial.ttf");
                }

                if (System.IO.File.Exists(fontFilePath))
                {
                    cyrillicFontAsset = TMP_FontAsset.CreateFontAsset(
                        fontFilePath,
                        0,
                        90,
                        9,
                        UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                        1024,
                        1024
                    );

                    if (cyrillicFontAsset != null)
                    {
                        cyrillicFontAsset.name = "Arial_Cyrillic_Dynamic";
                        cyrillicFontAsset.hideFlags = HideFlags.DontSave;
                        if (cyrillicFontAsset.material != null)
                        {
                            cyrillicFontAsset.material.hideFlags = HideFlags.DontSave;
                            Object.DontDestroyOnLoad(cyrillicFontAsset.material);
                        }
                        if (cyrillicFontAsset.atlasTextures != null)
                        {
                            foreach (var tex in cyrillicFontAsset.atlasTextures)
                            {
                                if (tex != null)
                                {
                                    tex.hideFlags = HideFlags.DontSave;
                                    Object.DontDestroyOnLoad(tex);
                                }
                            }
                        }
                        Object.DontDestroyOnLoad(cyrillicFontAsset);
                    }
                }
            }

            if (cyrillicFontAsset != null)
            {
                // Подключаем к глобальным фоллбэкам TMP Settings
                if (TMP_Settings.fallbackFontAssets == null)
                {
                    TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>();
                }

                if (!TMP_Settings.fallbackFontAssets.Contains(cyrillicFontAsset))
                {
                    TMP_Settings.fallbackFontAssets.Insert(0, cyrillicFontAsset);
                }

                // Подключаем к фоллбэкам дефолтного шрифта (LiberationSans SDF)
                if (TMP_Settings.defaultFontAsset != null)
                {
                    if (TMP_Settings.defaultFontAsset.fallbackFontAssetTable == null)
                    {
                        TMP_Settings.defaultFontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    }

                    if (!TMP_Settings.defaultFontAsset.fallbackFontAssetTable.Contains(cyrillicFontAsset))
                    {
                        TMP_Settings.defaultFontAsset.fallbackFontAssetTable.Insert(0, cyrillicFontAsset);
                    }
                }

                Debug.Log("<color=green>[CyrillicFont]</color> Шрифт Arial с поддержкой кириллицы успешно подключен как фоллбэк!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[CyrillicFont] Исключение при настройке кириллического шрифта: " + ex.Message);
        }
    }
}
