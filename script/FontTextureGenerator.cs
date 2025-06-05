using UnityEngine;
using UnityEngine.UI;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class FontTextureGenerator : MonoBehaviour
{
    [Header("Font Settings")]
    [SerializeField] private Font fontAsset; // D&D用のフォントアセット
    [SerializeField] private string fontPath = "Assets/Fonts/YourFont.ttf";
    [SerializeField] private int fontSize = 128;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color backgroundColor = Color.clear;

    [Header("Spacing Settings")]
    [SerializeField] private int characterSpacing = 0; // 文字間のスペーシング（ピクセル）

    [Header("Texture Settings")]
    [SerializeField] private int baseTextureWidth = 1280; // 基本の横幅
    [SerializeField] private int textureHeight = 256; // テクスチャの高さ
    [SerializeField] private string outputPath = "Assets/Textures/NumberTexture.png";

    [Header("Preview")]
    [SerializeField] private Texture2D generatedTexture;

    private Font loadedFont;

    [ContextMenu("Generate Number Texture")]
    public void GenerateNumberTexture()
    {
        if (!LoadFont())
        {
            Debug.LogError("Failed to load font from: " + fontPath);
            return;
        }

        Texture2D texture = CreateNumberTexture();
        if (texture != null)
        {
            generatedTexture = texture;
            SaveTexture(texture);
            Debug.Log("Number texture generated successfully at: " + outputPath);
        }
        else
        {
            Debug.LogError("Failed to generate number texture");
        }
    }

    private bool LoadFont()
    {
        // 優先的にD&DされたフォントアセットをMESウィンドウに使用
        if (fontAsset != null)
        {
            loadedFont = fontAsset;
            return true;
        }

        // フォントアセットが設定されていない場合は従来のパス方式を使用
        if (!File.Exists(fontPath))
        {
            Debug.LogError("Font file not found: " + fontPath);
            return false;
        }

        // Load font from Resources or StreamingAssets
        string resourcePath = fontPath.Replace("Assets/Resources/", "").Replace(".ttf", "");
        loadedFont = Resources.Load<Font>(resourcePath);

        if (loadedFont == null)
        {
            // Try loading as asset
#if UNITY_EDITOR
            loadedFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
#endif
        }

        return loadedFont != null;
    }

    private Texture2D CreateNumberTexture()
    {
        // Calculate final texture width with spacing on both sides of each character
        int finalTextureWidth = baseTextureWidth + (characterSpacing * 20); // 10 characters * 2 sides each
        
        // Create render texture
        RenderTexture renderTexture = new RenderTexture(finalTextureWidth, textureHeight, 24);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        
        // Clear with background color
        GL.Clear(true, true, backgroundColor);
        
        // Create texture to read pixels into
        Texture2D texture = new Texture2D(finalTextureWidth, textureHeight, TextureFormat.RGBA32, false);
        
        // Calculate character width (excluding spacing)
        float charWidth = (float)baseTextureWidth / 10.0f;
        
        // Create GUI style for text rendering
        GUIStyle textStyle = new GUIStyle();
        textStyle.font = loadedFont;
        textStyle.fontSize = fontSize;
        textStyle.normal.textColor = textColor;
        textStyle.alignment = TextAnchor.MiddleCenter;
        
        // Use OnGUI-like rendering with Graphics.DrawTexture approach
        Camera tempCamera = new GameObject("TempCamera").AddComponent<Camera>();
        tempCamera.targetTexture = renderTexture;
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.backgroundColor = backgroundColor;
        tempCamera.orthographic = true;
        tempCamera.orthographicSize = textureHeight / 2f;
        tempCamera.transform.position = new Vector3(finalTextureWidth / 2f, textureHeight / 2f, -10);
        
        // Create a temporary canvas for proper GUI rendering
        GameObject canvasGO = new GameObject("TempCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = tempCamera;
        canvas.planeDistance = 1;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(finalTextureWidth, textureHeight);
        
        // Draw numbers 0-9 using UI Text components with spacing on both sides
        for (int i = 0; i < 10; i++)
        {
            GameObject textGO = new GameObject("Number" + i);
            textGO.transform.SetParent(canvasGO.transform, false);
            
            Text textComponent = textGO.AddComponent<Text>();
            textComponent.text = i.ToString();
            textComponent.font = loadedFont;
            textComponent.fontSize = fontSize;
            textComponent.color = textColor;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            
            // Calculate position with spacing on both sides of each character
            float totalCharWidthWithSpacing = charWidth + (characterSpacing * 2); // Left + Right spacing
            float xPosition = (i * totalCharWidthWithSpacing) + characterSpacing; // Start position + left spacing
            
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(xPosition + (charWidth / 2f), 0);
            rectTransform.sizeDelta = new Vector2(charWidth, 0);
        }
        
        // Force canvas update
        Canvas.ForceUpdateCanvases();
        
        // Render the camera
        tempCamera.Render();
        
        // Read pixels from render texture
        texture.ReadPixels(new Rect(0, 0, finalTextureWidth, textureHeight), 0, 0);
        texture.Apply();
        
        // Clean up
        RenderTexture.active = previousActive;
        DestroyImmediate(canvasGO);
        DestroyImmediate(tempCamera.gameObject);
        DestroyImmediate(renderTexture);
        
        return texture;
    }

    private void SaveTexture(Texture2D texture)
    {
        byte[] pngData = texture.EncodeToPNG();

        // Ensure directory exists
        string directory = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(outputPath, pngData);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    // Alternative method using Canvas and Text component
    private Texture2D CreateNumberTextureWithCanvas()
    {
        // Calculate final texture width with spacing on both sides of each character
        int finalTextureWidth = baseTextureWidth + (characterSpacing * 20); // 10 characters * 2 sides each
        
        // Create temporary canvas
        GameObject canvasGO = new GameObject("TempCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        // Create camera for rendering
        GameObject cameraGO = new GameObject("TempCamera");
        Camera renderCamera = cameraGO.AddComponent<Camera>();
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = backgroundColor;
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = textureHeight / 2.0f;
        renderCamera.targetTexture = new RenderTexture(finalTextureWidth, textureHeight, 24);
        
        // Create text objects for each number with spacing on both sides
        float charWidth = (float)baseTextureWidth / 10.0f;
        
        for (int i = 0; i < 10; i++)
        {
            GameObject textGO = new GameObject("Number" + i);
            textGO.transform.SetParent(canvasGO.transform);
            
            Text textComponent = textGO.AddComponent<Text>();
            textComponent.text = i.ToString();
            textComponent.font = loadedFont;
            textComponent.fontSize = fontSize;
            textComponent.color = textColor;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            
            // Calculate position with spacing on both sides of each character
            float totalCharWidthWithSpacing = charWidth + (characterSpacing * 2); // Left + Right spacing
            float xPosition = (i * totalCharWidthWithSpacing) + characterSpacing + (charWidth / 2f) - (finalTextureWidth / 2f);
            rectTransform.anchoredPosition = new Vector2(xPosition, 0);
            rectTransform.sizeDelta = new Vector2(charWidth, textureHeight);
        }
        
        // Render to texture
        renderCamera.Render();
        
        // Read pixels
        RenderTexture.active = renderCamera.targetTexture;
        Texture2D texture = new Texture2D(finalTextureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, finalTextureWidth, textureHeight), 0, 0);
        texture.Apply();
        
        // Clean up
        RenderTexture.active = null;
        DestroyImmediate(renderCamera.targetTexture);
        DestroyImmediate(cameraGO);
        DestroyImmediate(canvasGO);
        
        return texture;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FontTextureGenerator))]
public class FontTextureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FontTextureGenerator generator = (FontTextureGenerator)target;

        // Font Asset D&D field
        EditorGUILayout.LabelField("Font Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        Font newFontAsset = (Font)EditorGUILayout.ObjectField("Font Asset (D&D)",
            serializedObject.FindProperty("fontAsset").objectReferenceValue,
            typeof(Font), false);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.FindProperty("fontAsset").objectReferenceValue = newFontAsset;

            // フォントアセットが設定された場合、パスも自動更新
            if (newFontAsset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(newFontAsset);
                serializedObject.FindProperty("fontPath").stringValue = assetPath;
            }

            serializedObject.ApplyModifiedProperties();
        }

        // Show font path as read-only when font asset is set
        Font currentFontAsset = serializedObject.FindProperty("fontAsset").objectReferenceValue as Font;
        if (currentFontAsset != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Font Path", serializedObject.FindProperty("fontPath").stringValue);
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            // Show editable font path when no font asset is set
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontPath"));
        }

        // Draw other properties
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fontSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("textColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundColor"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spacing Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("characterSpacing"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Texture Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseTextureWidth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("textureHeight"));

        // Show calculated final width
        SerializedProperty baseWidthProp = serializedObject.FindProperty("baseTextureWidth");
        SerializedProperty spacingProp = serializedObject.FindProperty("characterSpacing");
        int finalWidth = baseWidthProp.intValue + (spacingProp.intValue * 20); // 10 characters * 2 sides each
        
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.IntField("Final Texture Width", finalWidth);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("outputPath"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("generatedTexture"));

        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Number Texture", GUILayout.Height(30)))
        {
            generator.GenerateNumberTexture();
        }

        GUILayout.Space(5);

        // Show font file selector only when no font asset is set
        if (currentFontAsset == null)
        {
            if (GUILayout.Button("Select Font File"))
            {
                string path = EditorUtility.OpenFilePanel("Select TTF Font", "Assets", "ttf");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                        var fontPathProperty = serializedObject.FindProperty("fontPath");
                        fontPathProperty.stringValue = path;
                        serializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Path", "Please select a font file within the Assets folder.", "OK");
                    }
                }
            }
        }

        if (GUILayout.Button("Select Output Path"))
        {
            string path = EditorUtility.SaveFilePanel("Save Texture", "Assets", "NumberTexture", "png");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    var outputPathProperty = serializedObject.FindProperty("outputPath");
                    outputPathProperty.stringValue = path;
                    serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Please select a path within the Assets folder.", "OK");
                }
            }
        }
    }
}
#endif