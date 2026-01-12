using UnityEngine;
using UnityEngine.UI;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChiseNote.Font2Texture.Runtime
{
    [System.Serializable]
    [AddComponentMenu("ChiseNote/Font Texture Generator")]
    public class FontTextureGenerator : MonoBehaviour
    {
        [Header("Font Settings")]
        [SerializeField] private Font fontAsset;
        [SerializeField] private string fontPath = "Assets/Fonts/YourFont.ttf";
        [SerializeField] private int fontSize = 128;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundColor = Color.clear;
        [SerializeField] private int characterSpacing = 0;
        [SerializeField] private int baseTextureWidth = 1280;
        [SerializeField] private int textureHeight = 256;
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset outputFolder;
#endif
        [SerializeField] private string outputFolderPath = "Assets/Textures";
        [SerializeField] private string outputFileName = "NumberTexture";

        [Header("Preview")]
        [SerializeField] private Texture2D generatedTexture;

        private Font loadedFont;
    private const int NUM_CHARACTERS = 10;
    private const int SPACING_SIDES = 2;

        [ContextMenu("Generate Number Texture")]
        public void GenerateNumberTexture()
        {
#if UNITY_EDITOR
            if (outputFolder == null)
            {
                if (string.IsNullOrEmpty(outputFolderPath))
                {
                    outputFolderPath = "Assets/Textures";
                }
                outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(outputFolderPath);
            }

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
                string fullOutputPath = Path.Combine(outputFolderPath, outputFileName + ".png").Replace("\\", "/");
                Debug.Log("Number texture generated successfully at: " + fullOutputPath);

                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullOutputPath);
                if (obj != null)
                {
                    EditorGUIUtility.PingObject(obj);
                }
            }
            else
            {
                Debug.LogError("Failed to generate number texture");
            }
#else
            Debug.LogWarning("FontTextureGenerator: This feature is only available in the Unity Editor.");
#endif
        }

        private bool LoadFont()
        {
#if UNITY_EDITOR
            if (fontAsset != null)
            {
                loadedFont = fontAsset;
                return true;
            }

            loadedFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (loadedFont == null)
            {
                Debug.LogError("Font not found. Please assign a font in Target Font field.");
                return false;
            }

            return true;
#else
            return false;
#endif
        }

        private Texture2D CreateNumberTexture()
        {
#if UNITY_EDITOR
            int finalTextureWidth = baseTextureWidth + (characterSpacing * NUM_CHARACTERS * SPACING_SIDES);

            RenderTexture renderTexture = new RenderTexture(finalTextureWidth, textureHeight, 24);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

            GL.Clear(true, true, backgroundColor);

            Texture2D texture = new Texture2D(finalTextureWidth, textureHeight, TextureFormat.RGBA32, false);
            float charWidth = (float)baseTextureWidth / 10.0f;

            Camera tempCamera = new GameObject("TempCamera").AddComponent<Camera>();
            tempCamera.cullingMask = 1 << renderLayer;
            tempCamera.targetTexture = renderTexture;
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = backgroundColor;
            tempCamera.orthographic = true;
            tempCamera.orthographicSize = textureHeight / 2f;
            tempCamera.transform.position = new Vector3(finalTextureWidth / 2f + 50000f, textureHeight / 2f + 50000f, -10);

            GameObject canvasGO = new GameObject("TempCanvas");
            canvasGO.layer = renderLayer;
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = tempCamera;
            canvas.planeDistance = 1;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(finalTextureWidth, textureHeight);

            for (int i = 0; i < 10; i++)
            {
                GameObject textGO = new GameObject("Number" + i);
                textGO.layer = renderLayer;
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
                rectTransform.sizeDelta = new Vector2(charWidth, 0);
            }

            Canvas.ForceUpdateCanvases();

            tempCamera.Render();

            texture.ReadPixels(new Rect(0, 0, finalTextureWidth, textureHeight), 0, 0);
            texture.Apply();

            RenderTexture.active = previousActive;
            DestroyImmediate(canvasGO);
            DestroyImmediate(tempCamera.gameObject);
            DestroyImmediate(renderTexture);

            return texture;
#else
            return null;
#endif
        }

        private void SaveTexture(Texture2D texture)
        {
#if UNITY_EDITOR
            string fullOutputPath = Path.Combine(outputFolderPath, outputFileName + ".png").Replace("\\", "/");
            byte[] pngData = texture.EncodeToPNG();

            string directory = Path.GetDirectoryName(fullOutputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(fullOutputPath, pngData);

            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(fullOutputPath) as TextureImporter;
            if (importer != null)
            {
                importer.mipmapEnabled = true;
                importer.streamingMipmaps = true;
                importer.textureType = TextureImporterType.Default;
                importer.SaveAndReimport();
            }
#endif
        }
    }
}
