using UnityEngine;
using UnityEditor;
using System.IO;
using ChiseNote.Font2Texture.Runtime;

namespace ChiseNote.Font2Texture.Editor
{
    [CustomEditor(typeof(FontTextureGenerator))]
    public class FontTextureGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            FontTextureGenerator generator = (FontTextureGenerator)target;

            EditorGUILayout.Space(10);

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
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("fontSize"), new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textColor"), new GUIContent("Text Color"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundColor"), new GUIContent("Background Color"));
            });

            DrawSection("Spacing Settings", () => {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("characterSpacing"), new GUIContent("Character Spacing (px)"));
            });

            DrawSection("Texture Settings", () => {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("baseTextureWidth"), new GUIContent("Base Width"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textureHeight"), new GUIContent("Height"));

                SerializedProperty baseWidthProp = serializedObject.FindProperty("baseTextureWidth");
                SerializedProperty spacingProp = serializedObject.FindProperty("characterSpacing");
                int finalWidth = baseWidthProp.intValue + (spacingProp.intValue * 20);

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Final Width (calculated)", finalWidth);
                EditorGUI.EndDisabledGroup();
            });

            DrawSection("Output Settings", () => {
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
                    }
                    serializedObject.ApplyModifiedProperties();
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
                EditorGUI.EndDisabledGroup();
            });

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

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);

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
}
