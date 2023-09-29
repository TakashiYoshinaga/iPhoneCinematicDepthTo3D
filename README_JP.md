## 1. 概要

本リポジトリのサンプルは、以下の2つのプロジェクトを含みます：
- DepthExtractor：<br>
Cinematicモードで撮影したビデオからDepthとColorを分離するC#のサンプルプロジェクト。
- DepthPlayer：<br>
DepthとColorのビデオを3Dで表示するUnityのサンプルプロジェクト。

## 2. シネマティックモードの互換性
[Appleサポート](https://support.apple.com/ja-jp/HT212778)を参照してください。  
また、iPhone 15シリーズも対応しています。

## 3. iPhoneの設定

### カメラの設定
1. 設定 -> カメラ -> フォーマット
2. カメラ撮影のセクションで「高効率」を選択

### ファイル転送の設定
1. 設定 -> 写真
2. MacまたはPCに転送のセクションで「元のフォーマットのまま」を選択

## 4.ビデオファイルのPCへの転送

### [USB転送]

#### Windows用
1. iPhoneをUSBケーブルでPCに接続
2. エクスプローラを使用して、使用したいシネマティックビデオを探す
3. PC上の任意のフォルダにコピー
   **注意:** ビデオを撮影すると、IMG_0001.MOVやIMG_E0001.MOVのようなファイル名が生成されることがあります。"E"が含まれていない、IMG_0001.MOVのようなファイルを必ず選択してください。

#### Mac用
1. iPhoneをUSBケーブルでMacに接続
2. Macの写真アプリを開く
3. ウィンドウの左側のリストから「iPhone」を選択
4. 取り込みたいビデオを選択し、「選択項目を取り込む」ボタンをクリック
5. ウィンドウの左側のリストから「読み込み」を選択
6. 画面上部のメニューバーから ファイル -> 書き出すを選択
7. "1本のビデオの未編集のオリジナルを書き出す"をクリック
8. 書き出すボタンをクリック
9. 任意のフォルダに保存

### [ネットワーク転送(iOS 17以降)]
1. iPhoneの写真アプリを起動
2. シネマティックビデオを選択し、シェアボタンをタップ
3. "未編集のオリジナルを書き出す"をタップ
4. フォルダを選び、保存ボタンをタップ
5. Filesアプリを起動
6. ステップ4で保存したビデオを好きな方法で共有（例：Google Driveへアップロード）

## 5. DepthとColorビデオの分離（Windowsのみ）
1. DepthExtractor_Winフォルダを開く
2. Executableフォルダ内のDepthExtractor.exeを起動するか、またはVisual Studioを使用してProjectフォルダのプロジェクトを実行
3. Openボタンをクリックし、シネマティックビデオを選択
4. Convertボタンをクリックし、「Done」と表示されるまで待つ
5. ステップ3で選択したビデオと同じフォルダにcolor_output.webmとdepth_output.webmが生成されます。  
   **注意:** depthビデオのサイズは512x288または288x512、colorビデオのサイズは512x512です。

## 6. Unityでの3Dイメージとしての表示
1. UnityでDepthPlayerプロジェクトを開く
2. Assetsフォルダ以下の任意のフォルダにcolor_outputとdepth_outputを追加 
   例：VideoFilesフォルダ内
3. Scenesフォルダ内のDepthPlayerをダブルクリック
4. Hierarchyで[Main]オブジェクトを選択
5. Inspectorで「Color Video」にcolor_outputをドラッグ＆ドロップ
6. Inspectorで「Depth Video」にdepth_outputをドラッグ＆ドロップ
7. UnityEditorの上部でPlayボタンをクリックして再生<br>
   **注意1:** 表示オブジェクトのサイズを変更するには、[DepthMesh]オブジェクトのScaleを変更します。  
   **注意2:** Depth Scaleのみを変更する場合、[Main]オブジェクトのDepth Scaleの値（デフォルト=1.5）も変更できます。

## 7. ライセンス
このサンプル自体はMITライセンスですが、以下の依存関係とそれに関連するライセンスに注意してください：
- [GPAC2.2](https://gpac.wp.imt.fr/) - ライセンス: [GNU Lesser General Public License, version 2.1]
- [FFmpeg](https://ffmpeg.org/) - ライセンス: [GNU Lesser General Public License, version 2.1]