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
                string fullOutputPath = Path.Combine(outputFolderPath, outputFileName + ".png");
                Debug.Log("Number texture generated successfully at: " + fullOutputPath);
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

            if (!File.Exists(fontPath))
            {
                Debug.LogError("Font file not found: " + fontPath);
                return false;
            }

            string resourcePath = fontPath.Replace("Assets/Resources/", "").Replace(".ttf", "");
            loadedFont = Resources.Load<Font>(resourcePath);

            if (loadedFont == null)
            {
                loadedFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            }

            return loadedFont != null;
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
            tempCamera.orthographicSize = textureHeight / 2f;
            tempCamera.transform.position = new Vector3(finalTextureWidth / 2f, textureHeight / 2f, -10);

            GameObject canvasGO = new GameObject("TempCanvas");
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
            string fullOutputPath = Path.Combine(outputFolderPath, outputFileName + ".png");
            byte[] pngData = texture.EncodeToPNG();

            string directory = Path.GetDirectoryName(fullOutputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(fullOutputPath, pngData);

            AssetDatabase.Refresh();
#endif
        }

        private Texture2D CreateNumberTextureWithCanvas()
        {
#if UNITY_EDITOR
            int finalTextureWidth = baseTextureWidth + (characterSpacing * NUM_CHARACTERS * SPACING_SIDES);

            GameObject canvasGO = new GameObject("TempCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            GameObject cameraGO = new GameObject("TempCamera");
            Camera renderCamera = cameraGO.AddComponent<Camera>();
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = backgroundColor;
            renderCamera.orthographic = true;
            renderCamera.orthographicSize = textureHeight / 2.0f;
            renderCamera.targetTexture = new RenderTexture(finalTextureWidth, textureHeight, 24);

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
                rectTransform.sizeDelta = new Vector2(charWidth, textureHeight);
            }

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
#else
            return null;
#endif
        }
        public Font FontAsset
        {
            get { return fontAsset; }
            set { fontAsset = value; }
        }

        public string FontPath
        {
            get { return fontPath; }
            set { fontPath = value; }
        }

        public Texture2D GeneratedTexture
        {
            get { return generatedTexture; }
        }

#if UNITY_EDITOR
        public UnityEditor.DefaultAsset OutputFolder
        {
            get { return outputFolder; }
            set { outputFolder = value; }
        }
#endif

        public string OutputFolderPath
        {
            get { return outputFolderPath; }
            set { outputFolderPath = value; }
        }
    }
}
