# Floating Islands RPG v0.1.0 MVP — Release Notes

## 概要

浮遊島を舞台にした短編ターン制コマンドRPGです。村・フィールド・ダンジョンという古典的な3エリア構成の「浮遊島」を舞台に、探索→戦闘→成長→物語進行というRPGの基本ループを、短時間(1周30〜60分想定)で過不足なく体験できることを目指したMVP(Minimum Viable Product)です。

対象プラットフォームは **Windows(64-bit)** のみです。

## 遊べる範囲

- タイトル画面(はじめから/つづきから)
- 村・フィールド・ダンジョンの3エリア(Scene遷移、NPC会話、エンカウント)
- メインクエスト1本(村→フィールド→ダンジョン→ボス撃破→クリア)
- サブクエスト2本(メインクエストと独立に受注・進行・完了できる)
- ターン制コマンド戦闘(たたかう)、経験値・レベルアップ
- アイテム(Potion)・装備(Weapon/Armor)と、Village/Field/Dungeon共通のメニューUI
- セーブ/ロード(バージョン付きJSON)、タイトルからの「つづきから」
- ゲームオーバー時のRetry(戦闘直前の状態からの再戦)
- ゲームクリア画面

## 操作方法

詳細は同梱の [CONTROLS.md](./CONTROLS.md) を参照してください。

- 移動: `W`/`A`/`S`/`D` または矢印キー(ゲームパッド左スティック対応)
- NPCとの会話: `E` キー
- メニュー: 画面上の「Menu」ボタン

## 主な機能

### Save / Continue

- セーブ処理(`SaveGameUseCase`)はバージョン付きJSON形式で実装済み。
- タイトル画面の「Continue」から直前のセーブデータを読み込んで再開できる。
- **既知の制限**: プレイヤーが任意のタイミングで手動保存するためのゲーム内UI(セーブボタン)は本バージョンには未実装。

### Quest / SubQuest

- メインクエスト1本(村→フィールド→ダンジョン→ボス撃破)。
- サブクエスト2本(メインクエストと完全に独立して受注・進行・完了可能)。

### Battle / Retry

- ターン制コマンド戦闘。行動順は素早さ等のステータスで決定。
- 現時点でコマンドは「たたかう」のみ実装(とくぎ/どうぐ/にげるはUI未実装)。
- 敗北時はゲームオーバー画面から「Retry」で戦闘直前の状態から再戦可能。

### Inventory / Equipment

- 所持しているPotionの使用、Weapon/Armorの装備・解除がメニューUIから可能。
- 装備するとステータス(攻撃力・防御力)に反映される。

## 既知の制限

詳細は同梱の [KNOWN_LIMITATIONS.md](./KNOWN_LIMITATIONS.md) を参照してください。主なもの:

- ゲーム内セーブ実行UI(セーブボタン)が未実装。
- 戦闘コマンドは「たたかう」のみ。
- 「つづきから」は常にVillageへ復元される(保存時点のシーンへは戻らない)。
- 通貨・ショップ・クラフト・装備耐久度等は未実装(意図的にスコープ外)。
- 独自アートワーク・音声収録なし(Unity標準機能とプレースホルダー中心)。

## テスト結果

- 全EditMode: 398件 Passed(failed 0 / skipped 0 / inconclusive 0)
- 全PlayMode: 267件 Passed(failed 0 / skipped 0 / inconclusive 0、3回連続実行で確認)
- 正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear): Missing Script 0 / Missing Reference 0 / Broken Prefab 0
- Windows実行ファイル: ビルド成功(エラー0件・警告0件)、実機起動・Title画面描画・New Gameによるシーン遷移を確認
- 詳細な確認範囲・未確認事項は [PROJECT.md](../PROJECT.md) 6.現状「T-029」「T-031」を参照

## 配布ファイル

- ファイル名: `FloatingIslandsRpg-v0.1.0-win-x64.zip`
- 対象: Windows 64-bit
- 展開後、`FloatingIslandsRpg.exe` を実行して起動します。
