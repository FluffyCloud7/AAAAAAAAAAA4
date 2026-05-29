using UnityEditor;
using UnityEngine;
using System.IO;

public class CoverScreenshotWindow : EditorWindow
{
    private Camera targetCamera;
    private int width = 2480;
    private int height = 3500;
    private bool transparentBackground = false;
    private string folderName = "Covers";

    [MenuItem("Tools/Cover Screenshot Taker")]
    public static void ShowWindow()
    {
        GetWindow<CoverScreenshotWindow>("Cover Screenshot");
    }

    private void OnGUI()
    {
        GUILayout.Label("High-Res Cover Screenshot Taker", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", targetCamera, typeof(Camera), true);
        width = EditorGUILayout.IntField("Width (px)", width);
        height = EditorGUILayout.IntField("Height (px)", height);
        transparentBackground = EditorGUILayout.Toggle("Transparent BG", transparentBackground);
        folderName = EditorGUILayout.TextField("Save Folder", folderName);

        EditorGUILayout.Space();

        if (GUILayout.Button("Render & Save Screenshot", GUILayout.Height(40)))
        {
            if (targetCamera == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Target Camera!", "OK");
                return;
            }

            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        // 1. Создаем папку в корне проекта
        string directoryPath = Path.Combine(Application.dataPath, "../" + folderName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fileName = $"Cover_{System.DateTime.Now:yyyy-MM-dd_HHmmss}.png";
        string filePath = Path.Combine(directoryPath, fileName);

        // 2. Создаем временную текстуру высокого разрешения с максимальным сглаживанием (MSAA 8x)
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 8;

        RenderTexture originalRT = targetCamera.targetTexture;
        targetCamera.targetTexture = rt;

        CameraClearFlags originalClearFlags = targetCamera.clearFlags;
        Color originalBGColor = targetCamera.backgroundColor;

        // 3. Поддержка прозрачности
        if (transparentBackground)
        {
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = new Color(0, 0, 0, 0);
        }

        // 4. Рендерим камеру (работает в Edit Mode без Play Mode!)
        targetCamera.Render();

        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = rt;

        // 5. Читаем пиксели
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        // Коррекция цвета, если проект использует Linear Color Space (предотвращает затемнение скрина)
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            Color[] pixels = screenshot.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = pixels[i].gamma;
            }
            screenshot.SetPixels(pixels);
        }

        screenshot.Apply();

        // Восстанавливаем оригинальные настройки камеры
        targetCamera.targetTexture = originalRT;
        targetCamera.clearFlags = originalClearFlags;
        targetCamera.backgroundColor = originalBGColor;
        RenderTexture.active = activeRT;

        // 6. Сохраняем файл
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        // Чистим память
        DestroyImmediate(screenshot);
        DestroyImmediate(rt);

        // Выводим сообщение и сразу открываем папку в проводнике
        EditorUtility.DisplayDialog("Success", $"Screenshot saved to:\n{filePath}", "Great!");
        EditorUtility.RevealInFinder(filePath);
    }
}