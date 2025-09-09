using UnityEngine;
using UnityEditor;
using System.IO;
using ChiseNote.Font2Texture.Runtime;

namespace ChiseNote.Font2Texture.Editor
{
    [CustomEditor(typeof(FontTextureGenerator))]
    public class FontTextureGeneratorEditor : UnityEditor.Editor
    {
        private static readonly Color SECTION_LEFT_COLOR = new Color32(0xB8, 0x2C, 0x2C, 0xFF);
        private static readonly Color SECTION_RIGHT_COLOR = new Color32(0x4A, 0x4A, 0x4A, 0xFF);
        private const int LEFT_BORDER_SIZE = 4;
        private const int TITLE_HEADER_HEIGHT = 44;
        private const int TITLE_VERTICAL_OFFSET = 12;
        private const int SUBTITLE_VERTICAL_OFFSET = 28;
        private const int BUTTON_HEIGHT = 40;
        private const int PREVIEW_MARGIN = 40;
        private static GUIStyle _bannerStyle;
        private static GUIStyle _subTitleStyle;
        private static GUIStyle _sectionTitleStyle;
        private SerializedProperty _fontAssetProp;
        private SerializedProperty _fontPathProp;
        private SerializedProperty _fontSizeProp;
        private SerializedProperty _textColorProp;
        private SerializedProperty _backgroundColorProp;
        private SerializedProperty _characterSpacingProp;
        private SerializedProperty _baseTextureWidthProp;
        private SerializedProperty _textureHeightProp;
        private SerializedProperty _outputFolderProp;
        private SerializedProperty _outputFolderPathProp;
        private SerializedProperty _outputFileNameProp;
        private SerializedProperty _generatedTextureProp;

        private void OnEnable()
        {
            _fontAssetProp = serializedObject.FindProperty("fontAsset");
            _fontPathProp = serializedObject.FindProperty("fontPath");
            _fontSizeProp = serializedObject.FindProperty("fontSize");
            _textColorProp = serializedObject.FindProperty("textColor");
            _backgroundColorProp = serializedObject.FindProperty("backgroundColor");
            _characterSpacingProp = serializedObject.FindProperty("characterSpacing");
            _baseTextureWidthProp = serializedObject.FindProperty("baseTextureWidth");
            _textureHeightProp = serializedObject.FindProperty("textureHeight");
            _outputFolderProp = serializedObject.FindProperty("outputFolder");
            _outputFolderPathProp = serializedObject.FindProperty("outputFolderPath");
            _outputFileNameProp = serializedObject.FindProperty("outputFileName");
            _generatedTextureProp = serializedObject.FindProperty("generatedTexture");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var generator = (FontTextureGenerator)target;

            InitializeStyles();
            DrawTitleHeader();

            DrawSection("Font Settings", DrawFontSettings);
            DrawSection("Spacing Settings", DrawSpacingSettings);
            DrawSection("Texture Settings", DrawTextureSettings);
            DrawSection("Output Settings", DrawOutputSettings);
            DrawSection("Preview", DrawPreview);

            serializedObject.ApplyModifiedProperties();

            DrawGenerateButton(generator);
        }

        private void InitializeStyles()
        {
            if (_bannerStyle == null)
            {
                _bannerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };
            }

            if (_subTitleStyle == null)
            {
                _subTitleStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.LowerRight,
                    fontSize = 10,
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = new Color(1f, 1f, 1f, 0.7f) }
                };
            }

            if (_sectionTitleStyle == null)
            {
                _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(6, 0, 0, 0),
                    normal = { textColor = Color.white },
                };
            }
        }

        private void DrawFontSettings()
        {
            EditorGUI.BeginChangeCheck();
            var newFont = (Font)EditorGUILayout.ObjectField("Target Font", _fontAssetProp.objectReferenceValue, typeof(Font), false);
            if (EditorGUI.EndChangeCheck())
            {
                _fontAssetProp.objectReferenceValue = newFont;
                if (newFont != null)
                {
                    _fontPathProp.stringValue = AssetDatabase.GetAssetPath(newFont);
                }
            }

            var currentFont = _fontAssetProp.objectReferenceValue as Font;
            if (currentFont == null)
            {
                EditorGUILayout.HelpBox("Please add a font file to your Project and assign it here.", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(_fontSizeProp, new GUIContent("Font Size"));
            EditorGUILayout.PropertyField(_textColorProp, new GUIContent("Text Color"));
            EditorGUILayout.PropertyField(_backgroundColorProp, new GUIContent("Background Color"));
        }

        private void DrawSpacingSettings()
        {
            EditorGUILayout.PropertyField(_characterSpacingProp, new GUIContent("Character Spacing (px)"));
        }

        private void DrawTextureSettings()
        {
            EditorGUILayout.PropertyField(_baseTextureWidthProp, new GUIContent("Width"));
            EditorGUILayout.PropertyField(_textureHeightProp, new GUIContent("Height"));

            int finalWidth = _baseTextureWidthProp.intValue + (_characterSpacingProp.intValue * 20);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Final Width", finalWidth);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawOutputSettings()
        {
            EditorGUI.BeginChangeCheck();
            var newOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", _outputFolderProp.objectReferenceValue, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                _outputFolderProp.objectReferenceValue = newOutputFolder;
                if (newOutputFolder != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(newOutputFolder);
                    if (AssetDatabase.IsValidFolder(assetPath))
                    {
                        _outputFolderPathProp.stringValue = assetPath;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Folder", "Please select a valid folder.", "OK");
                        _outputFolderProp.objectReferenceValue = null;
                    }
                }
            }

            EditorGUILayout.PropertyField(_outputFileNameProp, new GUIContent("File Name"));

            string outputPath = _outputFolderPathProp.stringValue;
            string fileName = _outputFileNameProp.stringValue;
            if (!string.IsNullOrEmpty(outputPath) && !string.IsNullOrEmpty(fileName))
            {
                string fullPath = Path.Combine(outputPath, fileName + ".png");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Full Output Path", fullPath);
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawPreview()
        {
            if (_generatedTextureProp.objectReferenceValue != null)
            {
                Texture2D texture = (Texture2D)_generatedTextureProp.objectReferenceValue;
                float aspectRatio = (float)texture.width / texture.height;
                float maxWidth = EditorGUIUtility.currentViewWidth - PREVIEW_MARGIN;
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
        }

        private void DrawGenerateButton(FontTextureGenerator generator)
        {
            EditorGUILayout.Space(10);
            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Generate Number Texture", GUILayout.Height(BUTTON_HEIGHT)))
            {
                generator.GenerateNumberTexture();
            }
            GUI.backgroundColor = prevBg;
        }

        private void DrawTitleHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, TITLE_HEADER_HEIGHT, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, SECTION_LEFT_COLOR);

            var labelRect = new Rect(rect.x, rect.y + TITLE_VERTICAL_OFFSET, rect.width, 20);
            GUI.Label(labelRect, "Font Texture Generator", _bannerStyle);

            var subLabelRect = new Rect(rect.x, rect.y + SUBTITLE_VERTICAL_OFFSET, rect.width - 5, 14);
            GUI.Label(subLabelRect, "Edit by Chisenon", _subTitleStyle);

            EditorGUILayout.Space(6);
        }

        private void DrawSection(string title, System.Action content)
        {
            EditorGUILayout.Space(5);

            DrawSectionTitle(title);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.Space(3);
            EditorGUI.indentLevel++;
            content();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawSectionTitle(string title)
        {
            GUILayout.BeginHorizontal();
            var lineRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
            var leftRect = new Rect(lineRect.x, lineRect.y, LEFT_BORDER_SIZE, lineRect.height);
            var rightRect = new Rect(lineRect.x + LEFT_BORDER_SIZE, lineRect.y, lineRect.width - LEFT_BORDER_SIZE, lineRect.height);

            EditorGUI.DrawRect(leftRect, SECTION_LEFT_COLOR);
            EditorGUI.DrawRect(rightRect, SECTION_RIGHT_COLOR);

            GUI.Label(rightRect, title, _sectionTitleStyle);
            GUILayout.EndHorizontal();
        }
    }
}
