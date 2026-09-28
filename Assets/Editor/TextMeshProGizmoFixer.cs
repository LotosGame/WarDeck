#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TextMeshProGizmoFixer
{
    static TextMeshProGizmoFixer()
    {
        EditorApplication.delayCall += DisableTmpGizmoIcons;
    }

    [MenuItem("Tools/WarDeck/Hide TMP Gizmo Icons")]
    public static void DisableTmpGizmoIcons()
    {
        try
        {
            Assembly editorAssembly = typeof(Editor).Assembly;
            Type annotationUtilityType = editorAssembly.GetType("UnityEditor.AnnotationUtility");
            if (annotationUtilityType == null) return;

            MethodInfo getAnnotationsMethod = annotationUtilityType.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo setIconEnabledMethod = annotationUtilityType.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);

            if (getAnnotationsMethod != null && setIconEnabledMethod != null)
            {
                Array annotations = (Array)getAnnotationsMethod.Invoke(null, null);
                if (annotations != null)
                {
                    foreach (object ann in annotations)
                    {
                        Type annType = ann.GetType();
                        FieldInfo scriptClassField = annType.GetField("scriptClass", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        FieldInfo classIdField = annType.GetField("classID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        if (scriptClassField != null && classIdField != null)
                        {
                            string scriptClass = scriptClassField.GetValue(ann) as string;
                            int classId = (int)classIdField.GetValue(ann);

                            if (!string.IsNullOrEmpty(scriptClass) && 
                               (scriptClass.IndexOf("TextMeshPro", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                scriptClass.IndexOf("TMP_Text", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                setIconEnabledMethod.Invoke(null, new object[] { classId, scriptClass, 0 });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TextMeshProGizmoFixer] Не удалось автоматически отключить иконку TMP: {ex.Message}");
        }
    }
}
#endif
