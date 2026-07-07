# Release Checklist — v0.1.0

T-029〜T-032(`feature/t029-t032-release-prep`ブランチ)で実施したリリース準備の確認項目と実測結果です。日付はすべて2026-07-07(本セッション)。

## ビルド(T-029)

- [x] Windows 64-bit、非Development Build、Script Debugging無効、Autoconnect Profiler無効でビルド成功(エラー0件・警告0件、約104MB)
- [x] Build Settings順序: 0 Title / 1 Village / 2 Battle / 3 GameClear / 4 Field / 5 Dungeon(既存設定のまま、変更なし)
- [x] 全EditMode 398件 Passed(failed 0 / skipped 0 / inconclusive 0)
- [x] 全PlayMode 267件 Passed × 3回連続(failed 0 / skipped 0 / inconclusive 0)
- [x] 正式6Scene `SceneWiringValidator`: Missing Script 0 / Missing Reference 0 / Broken Prefab 0
- [x] ビルド済みexeの起動確認(プロセスレベル、クラッシュなし・応答あり)
- [x] Title画面の実描画確認(ウィンドウキャプチャ)
- [x] New Gameクリック → Village遷移の実描画確認(ウィンドウキャプチャ)
- [x] Development Consoleが表示されないことを確認
- [x] SaveファイルがProjectフォルダ内ではなくユーザーデータ領域(`Application.persistentDataPath`)へ保存されることをコード確認+実ファイル存在で確認
- [ ] Menu/Save/Continue/Battle/Retry/Dungeon/Boss戦/GameClear/再起動後Continueの、ビルド済みexeへの直接インタラクティブ操作による確認 — **未実施**(自動化されたPlayModeテスト(実SceneベースE2E含む)による代替検証のみ。詳細はPROJECT.md 6.現状「T-029」参照)
- [x] 発見した不具合(`TitleScreenController.cs`の`Application.Quit()`名前空間衝突、Playerビルド限定で顕在化)を修正

## パッケージング(T-030)

- [x] `FloatingIslandsRpg-v0.1.0-win-x64.zip`作成(Git管理外)
- [x] zip内に実行ファイル・`_Data`フォルダ・LICENSE・README.md・CONTROLS.md・KNOWN_LIMITATIONS.mdが含まれる
- [x] Library/Temp/Logs/UserSettings/.git/.vscodeが含まれない
- [x] Burstデバッグシンボル(`*_BurstDebugInformation_DoNotShip`)を除外
- [x] 展開後、実行ファイルの起動確認(プロセスレベル、クラッシュなし)
- [x] 展開後、Title画面の実描画確認(ウィンドウキャプチャ)
- [ ] 展開後、New Gameクリックによる遷移の再現確認 — 2回試行し、いずれも遷移せず(元ビルドでは成功しているため、自動化クリック手法自体の再現性の問題である可能性が高いが原因未特定。展開後ビルド固有の不具合の可能性は`git diff`上コード変更がないため低いと判断)
- [x] zip生成物・展開先ともにGit管理外であることを確認(`git status --short`に出現しない)

## 最終QA(T-031)

- [x] Editor(全EditMode 398件・全PlayMode 267件×3回連続Passed)、Windowsビルド、zip展開後ビルドの3環境でQAを実施
- [x] 発見したCritical/Major不具合(`Application.Quit()`名前空間衝突)を修正、修正後の残存Critical/Major不具合は0件
- [x] 既知のMinor(セーブUI未実装、戦闘コマンド「たたかう」のみ、Continue常時Village復元、通貨・ショップ等スコープ外)はいずれも既存の記録済み制限であり、リリース可能と判断
- [x] 既知の制限をREADME.md/`docs/KNOWN_LIMITATIONS.md`へ記録
- [x] QA結果をPROJECT.md 6.現状「T-031」へ記録
- [x] ショップ・クラフト・演出強化等T-033以降相当の新機能追加なし
- [x] テストのIgnore化・弱体化・削除なし
- [x] Build生成物のGit追加なし
- 詳細はPROJECT.md 6.現状「T-031」参照。

## リリース準備(T-032)

- [x] Release Notes作成(`docs/RELEASE_NOTES_v0.1.0.md`)
- [x] Release Checklist作成(本ファイル)
- [x] README.md/PROJECT.mdへT-029〜T-032の実測結果を反映
- [ ] git tag作成 — 未実施(方針により実施しない)
- [ ] `gh release create` — 未実施(方針により実施しない)
- [ ] zipの外部公開・アップロード — 未実施(方針により実施しない)

## GitHub Release手順(人手作業用、本セッションでは未実施)

以下は、`feature/t029-t032-release-prep`ブランチのレビュー・`main`へのマージが完了した後に、人手で実施する場合の手順です。本セッションでは一切実行していません。

1. `main`へのマージ後、マージコミットを確認する。
2. タグを作成する: `git tag -a v0.1.0 -m "Floating Islands RPG v0.1.0 MVP"`
3. タグをpushする: `git push origin v0.1.0`
4. GitHub Releaseを作成する:
   ```
   gh release create v0.1.0 \
     --title "Floating Islands RPG v0.1.0 MVP" \
     --notes-file docs/RELEASE_NOTES_v0.1.0.md \
     Release/FloatingIslandsRpg-v0.1.0-win-x64.zip
   ```
5. 公開後、Release Assetsに`FloatingIslandsRpg-v0.1.0-win-x64.zip`が添付されていることを確認する。

Version候補: `v0.1.0` / Release title候補: `Floating Islands RPG v0.1.0 MVP`(`ProjectSettings/ProjectSettings.asset`の`bundleVersion: 0.1.0`と一致)。
