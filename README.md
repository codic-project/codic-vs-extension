# Codic Extension for Visual Studio
Visual Studio IDE向けのcodic拡張です。


### Install
1. [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/b24a6e62-c6af-4cc2-aa5b-c2ae39195d7f?SRC=Home)よりVSIXをダウンロードし、実行します。

2. Visual Studioをリスタート後、メニューの Tools > Options からオプションダイアログを開き、Codic Extensionページより、アクセストークンを設定します。アクセストークンは、
 [Codic](https://codic.jp)にログイン後、APIステータスのページより取得できます。

### How to use

エディタ上で、<kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>D</kbd> でネーミング生成するためのポップアップを開きます。テキストを選択状態で開くと、ダイレクト生成できます。

![codic plugin](https://raw.githubusercontent.com/codic-project/codic-vs-extension/master/CodicExtension/Resources/Screenshot.png)

ケース (Letter Case) は、最後に選択されたモノがファイルの拡張子ごとに記憶されます。時間があれば、パーサーにアクセスして変数宣言、関数宣言など文脈ごとに記憶させたいと思っています。

### Change log

_1.0.3_
- Fix bug #1 (Support VS 2013).

_1.0.2_
- Fix bug #2 (NullReferenceException).
- Remove extra margin on popup.

_1.0.1_
- Use v1.1 API.

_1.0.0_
- First Release
      

### Bug report

バグ・要望などがありましたら、[Issue](https://github.com/codic-project/codic-vs-extension/issues)へ登録してください。