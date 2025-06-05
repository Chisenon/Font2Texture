using UnityEngine;
using UnityEngine.UI;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
[AddComponentMenu("ChiseNote/Font Texture Generator")]
public class FontTextureGenerator : MonoBehaviour
{    [Header("Font Settings")]
    [SerializeField] private Font fontAsset;
    [SerializeField] private string fontPath = "Assets/Fonts/YourFont.ttf";
    [SerializeField] private int fontSize = 128;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color backgroundColor = Color.clear;

    [Header("Spacing Settings")]
    [SerializeField] private int characterSpacing = 0;

    [Header("Texture Settings")]
    [SerializeField] private int baseTextureWidth = 1280;
    [SerializeField] private int textureHeight = 256;
    [SerializeField] private DefaultAsset outputFolder;
    [SerializeField] private string outputFolderPath = "Assets/Textures";
    [SerializeField] private string outputFileName = "NumberTexture";

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

        Texture2D texture = CreateNumberTexture();        if (texture != null)
        {
            generatedTexture = texture;
            SaveTexture(texture);
            string fullOutputPath = Path.Combine(outputFolderPath, outputFileName + ".png");
            Debug.Log("Number texture generated successfully at: " + fullOutputPath);
        }
        else
        {
            Debug.LogError("Failed to generate number texture");
        }
    }    private bool LoadFont()
    {
        if (fontAsset != null)
        {
            loadedFont = fontAsset;
            return true;
        }

        if (!File.Exists(fontPath))
        {
            Debug.LogError("Font file not found: " + fontPath);
            return false;
        }

        string resourcePath = fontPath.Replace("Assets/Resources/", "").Replace(".ttf", "");
        loadedFont = Resources.Load<Font>(resourcePath);

        if (loadedFont == null)
        {
#if UNITY_EDITOR
            loadedFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
#endif
        }

        return loadedFont != null;
    }    private Texture2D CreateNumberTexture()
    {
        int finalTextureWidth = baseTextureWidth + (characterSpacing * 20);
        
        RenderTexture renderTexture = new RenderTexture(finalTextureWidth, textureHeight, 24);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        
        GL.Clear(true, true, backgroundColor);
        
        Texture2D texture = new Texture2D(finalTextureWidth, textureHeight, TextureFormat.RGBA32, false);
        
        float charWidth = (float)baseTextureWidth / 10.0f;        
        GUIStyle textStyle = new GUIStyle();
        textStyle.font = loadedFont;
        textStyle.fontSize = fontSize;
        textStyle.normal.textColor = textColor;
        textStyle.alignment = TextAnchor.MiddleCenter;
        
        Camera tempCamera = new GameObject("TempCamera").AddComponent<Camera>();
        tempCamera.targetTexture = renderTexture;
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.backgroundColor = backgroundColor;
        tempCamera.orthographic = true;
        tempCamera.orthographicSize = textureHeight / 2f;        tempCamera.transform.position = new Vector3(finalTextureWidth / 2f, textureHeight / 2f, -10);
        
        GameObject canvasGO = new GameObject("TempCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = tempCamera;
        canvas.planeDistance = 1;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;        scaler.referenceResolution = new Vector2(finalTextureWidth, textureHeight);
        
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
            
            float totalCharWidthWithSpacing = charWidth + (characterSpacing * 2);
            float xPosition = (i * totalCharWidthWithSpacing) + characterSpacing;
            
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(xPosition + (charWidth / 2f), 0);
            rectTransform.sizeDelta = new Vector2(charWidth, 0);        }
        
        Canvas.ForceUpdateCanvases();
        
        tempCamera.Render();
        
        texture.ReadPixels(new Rect(0, 0, finalTextureWidth, textureHeight), 0, 0);
        texture.Apply();
        
        RenderTexture.active = previousActive;
        DestroyImmediate(canvasGO);
        DestroyImmediate(tempCamera.gameObject);
        DestroyImmediate(renderTexture);
        
        return texture;
    }    private void SaveTexture(Texture2D texture)
    {
        string fullOutputPath = Path.Combine(outputFolderPath, outputFileName + ".png");
        byte[] pngData = texture.EncodeToPNG();

        string directory = Path.GetDirectoryName(fullOutputPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(fullOutputPath, pngData);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }    private Texture2D CreateNumberTextureWithCanvas()
    {
        int finalTextureWidth = baseTextureWidth + (characterSpacing * 20);
        
        GameObject canvasGO = new GameObject("TempCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;        canvas.worldCamera = Camera.main;
        
        GameObject cameraGO = new GameObject("TempCamera");
        Camera renderCamera = cameraGO.AddComponent<Camera>();
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = backgroundColor;
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = textureHeight / 2.0f;        renderCamera.targetTexture = new RenderTexture(finalTextureWidth, textureHeight, 24);
        
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
            
            float totalCharWidthWithSpacing = charWidth + (characterSpacing * 2);
            float xPosition = (i * totalCharWidthWithSpacing) + characterSpacing + (charWidth / 2f) - (finalTextureWidth / 2f);
            rectTransform.anchoredPosition = new Vector2(xPosition, 0);
            rectTransform.sizeDelta = new Vector2(charWidth, textureHeight);        }
        
        renderCamera.Render();
        
        RenderTexture.active = renderCamera.targetTexture;
        Texture2D texture = new Texture2D(finalTextureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, finalTextureWidth, textureHeight), 0, 0);
        texture.Apply();
        
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
        serializedObject.Update();
        FontTextureGenerator generator = (FontTextureGenerator)target;        EditorGUILayout.Space(10);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 24;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
          EditorGUILayout.LabelField("Font Texture Generator", titleStyle);
        EditorGUILayout.Space(10);

        DrawSection("Font Settings", () => {
            EditorGUI.BeginChangeCheck();
            Font newFontAsset = (Font)EditorGUILayout.ObjectField("Font Asset",
                serializedObject.FindProperty("fontAsset").objectReferenceValue,
                typeof(Font), false);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.FindProperty("fontAsset").objectReferenceValue = newFontAsset;
                if (newFontAsset != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(newFontAsset);
                    serializedObject.FindProperty("fontPath").stringValue = assetPath;
                }
                serializedObject.ApplyModifiedProperties();
            }

            Font currentFontAsset = serializedObject.FindProperty("fontAsset").objectReferenceValue as Font;
            if (currentFontAsset != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Font Path", serializedObject.FindProperty("fontPath").stringValue);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fontPath"), new GUIContent("Font Path"));
                if (GUILayout.Button("Browse Font File", GUILayout.Height(25)))
                {
                    string path = EditorUtility.OpenFilePanel("Select TTF Font", "Assets", "ttf");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (path.StartsWith(Application.dataPath))
                        {
                            path = "Assets" + path.Substring(Application.dataPath.Length);
                            serializedObject.FindProperty("fontPath").stringValue = path;
                            serializedObject.ApplyModifiedProperties();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Invalid Path", "Please select a font file within the Assets folder.", "OK");
                        }
                    }
                }
            }            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontSize"), new GUIContent("Font Size"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("textColor"), new GUIContent("Text Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundColor"), new GUIContent("Background Color"));        });

        DrawSection("Spacing Settings", () => {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterSpacing"), new GUIContent("Character Spacing (px)"));
        });

        DrawSection("Texture Settings", () => {            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseTextureWidth"), new GUIContent("Base Width"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("textureHeight"), new GUIContent("Height"));

            SerializedProperty baseWidthProp = serializedObject.FindProperty("baseTextureWidth");
            SerializedProperty spacingProp = serializedObject.FindProperty("characterSpacing");
            int finalWidth = baseWidthProp.intValue + (spacingProp.intValue * 20);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Final Width (calculated)", finalWidth);
            EditorGUI.EndDisabledGroup();
        });        DrawSection("Output Settings", () => {
            EditorGUI.BeginChangeCheck();
            DefaultAsset newOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder",
                serializedObject.FindProperty("outputFolder").objectReferenceValue,
                typeof(DefaultAsset), false);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.FindProperty("outputFolder").objectReferenceValue = newOutputFolder;
                if (newOutputFolder != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(newOutputFolder);
                    if (AssetDatabase.IsValidFolder(assetPath))
                    {
                        serializedObject.FindProperty("outputFolderPath").stringValue = assetPath;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Folder", "Please select a valid folder.", "OK");
                        serializedObject.FindProperty("outputFolder").objectReferenceValue = null;
                    }
                }                serializedObject.ApplyModifiedProperties();
            }            
            DefaultAsset currentOutputFolder = serializedObject.FindProperty("outputFolder").objectReferenceValue as DefaultAsset;
            if (currentOutputFolder != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Folder Path", serializedObject.FindProperty("outputFolderPath").stringValue);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("outputFolderPath"), new GUIContent("Folder Path"));
            }
              EditorGUILayout.PropertyField(serializedObject.FindProperty("outputFileName"), new GUIContent("File Name (without extension)"));
            
            string fullPath = Path.Combine(serializedObject.FindProperty("outputFolderPath").stringValue,
                                         serializedObject.FindProperty("outputFileName").stringValue + ".png");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Full Output Path", fullPath);
            EditorGUI.EndDisabledGroup();        });

        DrawSection("Preview", () => {
            SerializedProperty textureProp = serializedObject.FindProperty("generatedTexture");
            if (textureProp.objectReferenceValue != null)
            {
                Texture2D texture = (Texture2D)textureProp.objectReferenceValue;
                float aspectRatio = (float)texture.width / texture.height;
                float maxWidth = EditorGUIUtility.currentViewWidth - 40;
                float width = Mathf.Min(maxWidth, texture.width);
                float height = width / aspectRatio;
                
                Rect rect = GUILayoutUtility.GetRect(width, height);
                EditorGUI.DrawPreviewTexture(rect, texture);
                
                EditorGUILayout.LabelField($"Size: {texture.width} x {texture.height}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox("No texture generated yet. Click 'Generate' to create texture.", MessageType.Info);
            }
        });

        serializedObject.ApplyModifiedProperties();        EditorGUILayout.Space(10);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate Number Texture", GUILayout.Height(40)))
        {
            generator.GenerateNumberTexture();
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawSection(string title, System.Action content)
    {
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 11;
        EditorGUILayout.LabelField(title, titleStyle);
        
        EditorGUILayout.Space(3);
        
        EditorGUI.indentLevel++;
        content();
        EditorGUI.indentLevel--;
        
        EditorGUILayout.EndVertical();
    }
}
#endif