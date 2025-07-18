# Font2Texture

任意のFontからTextureを生成するComponent

![image](https://github.com/user-attachments/assets/883d7739-6024-485a-a0d5-f808ec239fd9)


## クイックスタート

1. **Prefabを使用**
   - Packages/Chise - Font2Texture/`Font2Tex.prefab`をシーンにDrag & Drop

2. **設定**
   - `Font Asset` : フォントファイルをドラッグ
   - `Font Size` : 必要に応じて調整（デフォルト: 128）
   - `Character Spacing` : 数字間のスペースを設定
   - `Output Path` : Texture の保存先を選択

3. **生成**
   - [ Generate Number Texture ]ボタンをクリック
   - Texture がPNGとして設定された場所に保存される

## 手動セットアップ

Componentを手動で追加する場合

- 任意のオブジェクト(EmptyObjectとか)に`ChiseNote/Font Texture Generator`ComponentをAttach
- Inspectorで設定を調整

## 生成される画像の例
![NumberTexture](https://github.com/user-attachments/assets/341bffcc-7286-4949-91b8-43a2f043a54a)
使用フォント：[JetBrainsMono-Bold](https://www.jetbrains.com/ja-jp/lp/mono/)
