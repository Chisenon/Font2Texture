# Font2Texture

フォントから数字Texture （0-9）を生成するcomponent

## クイックスタート

1. **prefabを使用（推奨）**
   - `Font2Tex.prefab`をシーンにDrag & Drop

2. **設定**
   - `Font Asset` : フォントファイルをドラッグ
   - `Font Size` : 必要に応じて調整（デフォルト: 128）
   - `Character Spacing` : 数字間のスペースを設定
   - `Output Path` : Texture の保存先を選択

3. **生成**
   - [ Generate Number Texture ]ボタンをクリック
   - Texture がPNGとして保存されます

## 手動セットアップ

Componentを手動で追加する場合

- 任意のGameObjectに`FontTextureGenerator`Componentを追加
- Inspectorで設定を調整