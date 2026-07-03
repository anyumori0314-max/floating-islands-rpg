# floating-islands-rpg

## 概要

浮遊島エリア制オープンワールドRPG。

村・フィールド・ダンジョンという古典的な3エリア構成の「浮遊島」を舞台に、探索→戦闘→成長→物語進行というRPGの基本ループを、短時間(1周30〜60分想定)で過不足なく体験できることを目指すプロジェクトです。

**現在はMVP開発初期段階です。プレイ可能な完成版ではありません。**

## 使用技術

- Unity 6 (6000.3.17f1)
- URP (Universal Render Pipeline)
- C#
- Unity MCP (Claude Code等のAIエージェントからUnity Editorを操作するための接続基盤)

## 開発環境

- Unity Hub + Unity 6000.3.17f1
- OS: Windows
- IDE: Visual Studio Code / Visual Studio / Rider いずれか(`.csproj` / `.sln` はUnityが自動生成するためGit管理対象外)

### Unity Hubから開く方法

1. Unity Hubを起動する。
2. 「開く」からこのリポジトリのルートディレクトリ(`floating-islands-rpg/`、`Assets/`や`ProjectSettings/`があるフォルダ)を選択する。
3. Unity 6000.3.17f1 がインストールされていない場合はUnity Hub経由でインストールする。
4. プロジェクトを開くとパッケージの解決・インポートが自動的に行われる。

## 現在の実装状況

- PROJECT.md(設計・スコープ・タスク管理ドキュメント)作成済み。
- 要承認事項7件について人間による方針決定が完了。
- Unity MCP接続確認済み(Console Error 0件 / Warning 0件)。
- Git・リポジトリ管理基盤(本コミット)を整備中。
- ゲームロジック・UI・Prefab・ScriptableObjectは**未実装**(コード0行)。
- 詳細な進捗・タスク一覧は [PROJECT.md](./PROJECT.md) を参照してください。

## PROJECT.mdについて

本プロジェクトの目的、スコープ、仕様、設計方針、規約、現状、実装タスク一覧は [PROJECT.md](./PROJECT.md) に一元管理されています。実装に着手する前に必ずPROJECT.mdを確認してください。

## 開発フロー

本プロジェクトは以下の開発フローに従います。

1. **ImplementerとReviewerを別コンテキストにする**: 実装を行うエージェント/担当と、レビューを行うエージェント/担当のコンテキストを分離し、実装者の思い込みをそのまま通さない。
2. **PROJECT.md承認後に実装**: 設計・仕様・要承認事項が人間によって承認された後に、対応するタスクの実装へ着手する。
3. **featureブランチ**: `main`へ直接コミットせず、タスク単位でfeatureブランチを作成して作業する。
4. **テスト**: Domain/Applicationのロジックは EditMode テスト、シーン遷移や戦闘フロー等の結合的な動作は PlayMode テストで検証する。
5. **第三者レビュー**: 実装後、Implementerとは別のレビュープロセスでコードレビューを行う。
6. **PR**: レビューを経てPull Requestを作成し、`main`へマージする。

## 今後追加予定のCI項目

現時点のCI(`.github/workflows/repository-check.yml`)はリポジトリ構成の健全性チェックのみです。以下は将来追加予定です。

- EditMode Test の自動実行
- PlayMode Test の自動実行
- Unity Build の自動実行

## ライセンス

本リポジトリのコードは [MIT License](./LICENSE) の下で公開されています。著作権者表記は暫定的に `floating-islands-rpg contributors` としており、後から変更される可能性があります。

## 注意事項

- 現時点ではプレイ可能な完成版ではありません。MVP開発の初期段階です。
- 外部アセット(モデル・テクスチャ・音源・フォント等)を導入する場合、そのライセンスは各アセットごとに個別に管理・記載します。本リポジトリのMITライセンスは自作コードにのみ適用され、外部アセットのライセンスを上書きするものではありません。
