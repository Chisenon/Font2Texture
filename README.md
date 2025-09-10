# Font2Texture Generator

UnityでProject内のフォントファイルから、専用のSpriteTextureを生成します

## Download for VPM Installer(VCC/ALCOM対応)
[▶️ **Font2Texture Generator Installer (v1.3.1)**](https://github.com/Chisenon/Font2Texture/releases/download/1.3.1/Font2Texture-1.3.1_VPMInstaller.unitypackage)

## 使用方法

1. **Prefabを使用**
   - Packages/Chise - Font2Texture/`Font2Tex.prefab`をシーンに配置

2. **Inspectorで設定**
   - `Target Font`：任意のフォントを指定
   - `Font Size`：数字のサイズを調整
   - `Character Spacing`：数字間のスペースを調整
   - `Text Color`/`Background Color`：色を設定
   - `Output Folder`/`File Name`：保存先とファイル名を指定

3. **生成**
   - [Generate Number Texture] ボタンをクリック
   - 指定した場所にPNG画像が保存されます

## UI
![image](https://github.com/user-attachments/assets/5feceaba-945e-4285-a929-ee5764510767)

## 手動セットアップ

Componentを手動で追加する場合
- 任意のオブジェクト(EmptyObjectなど)に`ChiseNote/Font Texture Generator`ComponentをAttach
- Inspectorで設定を調整

## 生成例
![NumberTexture](https://github.com/user-attachments/assets/341bffcc-7286-4949-91b8-43a2f043a54a)
使用フォント：[JetBrainsMono-Bold](https://www.jetbrains.com/ja-jp/lp/mono/)
