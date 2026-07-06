# PROJECT.md

> floating-islands-rpg の設計方針・スコープ・実装タスクを一元管理するドキュメント。
> 最終更新: 2026-07-07 (T-018〜T-020はPull Request #7、T-021〜T-024[Codex指摘対応込み]はPull Request #8を経て`main`へマージ済み[マージコミット`269c360`]。T-025〜T-028を`feature/t025-t028-release-slice`ブランチ[`origin/main`の`269c360`から新規作成]で実装中。T-025(サブクエスト2本)・T-026(アイテム・装備UI本格実装、採用範囲を限定)・T-027(通し結線・E2E確認)・T-028(README/LICENSE整備)完了。全EditMode 398件・全PlayMode 267件Passed[PlayMode 3回連続確認済み]。T-027はUnity MCP/対話的Editorが利用できない環境だったため、Unity 6000.3.17f1のCLIバッチモードでテスト実行し、実Sceneを実ロードする新規PlayMode E2Eテストで手動通しプレイを代替した(対話的Editorでの目視確認は未実施)。T-028はREADME.mdを実態に合わせて全面更新し、LICENSEは変更せず維持、`.gitignore`にローカル生成物2件の除外を追加した(Runtime/Scene/Prefab/Asset/metaは無変更)。T-029以降は未着手。本ブランチはコミット・push・PR作成・`main`マージ未実施)

---

## 1. 目的

### 解決したい体験
- 浮遊島という限定された空間を舞台に、王道ファンタジーの「探索→戦闘→成長→物語進行」というRPGの基本ループを、短時間で過不足なく体験できるようにする。
- 巨大なオープンワールドではなく、村・フィールド・ダンジョンという古典的な3エリア構成に絞り、1周30〜60分で「始まりから終わりまで」を遊び切れることを重視する。

### 想定プレイヤー
- JRPG(ドラゴンクエスト、ファイナルファンタジー等)のターン制コマンド戦闘に馴染みがあるプレイヤー。
- 短時間で完結する縦切りの体験を求めるプレイヤー(長時間コミットを前提としない)。

### 想定プレイ環境
- PC(Windows)、キーボード+マウスおよび一般的なゲームパッドを想定。
- Unity 6 / URP。モバイル対応はMVPの対象外(スコープ参照)。

### この作品の独自性
- 「浮遊島」という舞台設定を、探索範囲を意図的に区切るための構造的な制約として用いる(=シームレスな大陸ではなく、島単位で閉じた小さな箱庭を積み重ねる設計思想)。
- MVPの時点では独自性よりも「完成させること」「王道 RPG のループを破綻なく成立させること」を優先する。演出的な独自性は将来フェーズで拡張する。

---

## 2. スコープ

### MVPに含む機能

| # | 機能 | 受け入れ条件 |
|---|------|--------------|
| 1 | 1つの浮遊島 | 村・フィールド・ダンジョンの3エリアが1つの島の設定として繋がっており、island_1 相当の識別子でまとめて管理されている |
| 2 | 小さな村 | NPCが3体以上配置され、会話でき、村の外(フィールド)へ徒歩で移動できる |
| 3 | 屋外フィールド | 村とダンジョン入口を繋ぐ探索可能な屋外エリアが存在し、通常敵とのエンカウントが発生する |
| 4 | 小規模ダンジョン | ボスの手前に通常敵エンカウントが最低1回発生し、最奥にボスが配置されている一本道〜軽度分岐のダンジョンが存在する |
| 5 | 主人公1人 | 名前・HP・MP・レベル・装備を持つ操作キャラクターが存在し、フィールドを移動できる |
| 6 | 仲間1人 | 一定の条件(会話イベント等)で戦闘パーティに加入し、以後の戦闘に参加する |
| 7 | 通常敵3種類 | 見た目・ステータス・行動パターンが異なる3種の敵が、フィールドとダンジョンに出現する |
| 8 | ボス1体 | 通常敵より明確に強いステータス・専用行動を持ち、撃破するとメインクエストが進行する |
| 9 | ターン制コマンド戦闘 | 「たたかう/とくぎ/どうぐ/にげる」の基本コマンドで戦闘が成立し、勝敗(勝利/全滅)が判定される |
| 10 | レベルアップ | 戦闘勝利で経験値を獲得し、閾値到達でレベルが上がりHP/MP/攻撃力等が上昇する |
| 11 | HP、MP | 全キャラクターがHP/MPを持ち、0以下になった際の挙動(戦闘不能/MP不足で技が使えない)が定義されている |
| 12 | アイテム | 使用アイテム(回復等)を所持し、戦闘中・フィールド中に使用できる |
| 13 | 装備 | 武器・防具を切り替えるとステータスに反映される |
| 14 | NPC会話 | 会話ウィンドウが表示され、テキストを読み進められる。分岐は必須ではない |
| 15 | メインクエスト1本 | 開始条件・進行条件・完了条件が明確な一連の目的(村→フィールド→ダンジョン→ボス撃破)が存在する |
| 16 | サブクエスト2本 | メインクエストと独立して受注・進行・完了できる小目的が2つ存在する |
| 17 | セーブ／ロード | プレイヤーの任意タイミングでセーブでき、タイトルからロードして再開できる |
| 18 | タイトル画面 | ゲーム起動時に表示され、「はじめから/つづきから」を選べる |
| 19 | ゲームクリア画面 | ボス撃破後、専用の画面が表示されメインループが終了する |
| 20 | 想定プレイ時間30〜60分 | 上記構成を通しでプレイした場合の所要時間が30〜60分に収まる(初回想定・攻略情報ありの目安) |

### MVPに含まない機能
- 完全シームレスな巨大オープンワールド
- オンラインマルチプレイ
- 自動生成マップ
- クラフト
- 仲間の生活シミュレーション
- 複雑な恋愛システム
- 自由飛行できる飛空船
- 複数の大陸
- 大量のNPCスケジュール
- 課金
- モバイル対応
- ボイス収録
- 高度なキャラクタークリエイト

---

## 3. 仕様

### ゲーム開始からクリアまでの基本フロー
1. タイトル画面 →「はじめから」または「つづきから」を選択
2. 村シーンで開始、NPCとの会話でメインクエスト開始
3. フィールドへ移動し探索・エンカウント戦闘をこなしながらダンジョン入口へ到達
4. ダンジョンを進み、道中の通常戦闘を経てボス部屋へ到達
5. ボス戦に勝利するとメインクエストが完了
6. ゲームクリア画面へ遷移し、タイトルへ戻る

### Scene一覧(想定)
| Scene名 | 役割 |
|---------|------|
| Title | タイトル画面。はじめから/つづきから |
| Village | 村。NPC会話、装備・アイテムの起点 |
| Field | 屋外フィールド。エンカウント、ダンジョン入口 |
| Dungeon | 小規模ダンジョン。エンカウント、ボス |
| Battle | 戦闘専用シーン。フィールド/ダンジョンSceneに対してAdditiveロードする(4.設計「Scene構成」参照) |
| GameClear | ゲームクリア画面 |

**現状(2026-07-05更新)**: `Title`/`Village`/`Field`/`Dungeon`/`Battle`/`GameClear`の正式6Sceneすべてが`Assets/_Project/Scenes/`に実体として作成済み(Title/GameClearはT-016/T-017、VillageはT-018でNPC3体+フィールド接続へ拡張済み、FieldはT-019で新規作成、DungeonはT-020で新規作成、BattleはT-015作成・T-019でAdditiveロード対応)。旧`Assets/Scenes/SampleScene.unity`はAsset自体を保持しているが、Build Settingsからは除外済み(下記「Build Settings」参照)。

### プレイヤー操作
- 新Input System(`InputSystem_Actions.inputactions` が既定生成済み)を用いた移動・決定・キャンセル・メニュー呼び出し。
- フィールド/ダンジョンでは3D移動、戦闘ではコマンド選択のUI操作に切り替わる。

### フィールド探索
- 徒歩移動、村・ダンジョンとの接続、一定条件(歩数/エリア/確率のいずれか)でランダムエンカウントが発生する。

### NPC会話
- NPCに接触/決定入力で会話ウィンドウを開始し、テキスト送りで読み進める。

### クエスト進行
- クエストは「未受注/進行中/完了」の状態を持ち、メインクエスト1本・サブクエスト2本を独立して管理する。

### エンカウント方式
- ランダムエンカウント(シンボルエンカウントは対象外)。フィールドとダンジョンで出現テーブルを分ける。

### ターン制戦闘
- 行動順は素早さ等のステータスに基づいて決定し、コマンド入力→行動解決→勝敗判定のサイクルを繰り返す。

### 経験値とレベルアップ
- 敵撃破時にパーティへ経験値を分配し、レベルごとの必要経験値テーブルに基づいて成長する。

### アイテムと装備
- アイテムは所持数を持ち、使用で消費される。装備は武器/防具スロットを持ち、ステータス補正を与える。

### セーブ／ロード
- プレイヤー主導のセーブ(任意タイミング)。ロードはタイトル画面からのみ。保存形式はバージョン付きJSON(4.設計「セーブデータ設計方針」参照)。

### ゲームオーバー
- パーティ全滅でゲームオーバー画面へ遷移する。ゲームオーバー時は、戦闘開始直前の状態からBattleを再ロードして再戦できる(Retry。詳細は4.設計「T-017 Retry仕様の修正」参照)。

### エラー時の挙動
- セーブデータ読込失敗時はゲームを強制終了させず、エラーを通知した上でタイトル画面に留める(セーブデータ破損時の扱いは5.規約参照)。

### 完了条件
- 本MVPは「タイトル→村→フィールド→ダンジョン→ボス撃破→クリア画面」を、セーブ/ロードを挟んでも中断・再開でき、Console上にErrorを出さずに一気通貫でプレイできることをもって完了とする。

### テスト観点
- Domain層(ステータス計算、戦闘計算、経験値・レベルアップ計算、クエスト状態遷移)はEditModeテストで純粋なロジックとして検証する。
- セーブ/ロードの往復(シリアライズ→デシリアライズで同一状態に戻ること)をEditModeテストで検証する。
- シーン遷移・戦闘開始〜終了までの一連の流れはPlayModeテストで検証する。

---

## 4. 設計

Unity向けの責務分離として、MVCという名称には合わせず以下の5層で構成する。

- **Domain**: RPGルール、ステータス計算、戦闘計算、クエスト状態。Unityの型(MonoBehaviour, GameObject等)に依存しない純粋なC#。
- **Application**: ユースケース(戦闘進行、Scene遷移、セーブ処理)。DomainとPresentation/Infrastructureの橋渡し。
- **Presentation**: MonoBehaviour、UI、入力、カメラ。DomainとApplicationの結果を画面に反映する。
- **Infrastructure**: Unity依存の実処理(ファイル保存、ScriptableObject、Addressables候補)。
- **Tests**: EditMode(主にDomain/Application)、PlayMode(Presentationを含む結合的な検証)。

### ディレクトリ構成(T-001で作成済み)
```
Assets/
  _Project/
    Runtime/
      Domain/
      Application/
      Presentation/
      Infrastructure/
    Editor/              (Editor拡張・Editor専用スクリプト用)
    Tests/
      EditMode/
      PlayMode/
    Scenes/              (将来: 正式なTitle/Village/Field/Dungeon/Battle/GameClear Sceneを配置)
    Prefabs/
    ScriptableObjects/
    UI/                  (UI関連のアセット・Prefab等)
    Art/                 (モデル・テクスチャ・マテリアル等の非コードアセット)
    Audio/               (BGM・SE等の音声アセット)
    Settings/            (_Project固有の設定アセット。既存の Assets/Settings とは別。責務は下記注記参照)
  Settings/        (既存: URP設定。Unityテンプレートが生成したレンダーパイプライン設定資産。_Project/Settings とは別物)
  Scenes/          (既存: SampleScene。将来的に _Project/Scenes へ整理するか要検討)
```
`Assets/_Project/` 配下に新規実装をまとめ、テンプレート由来の `Assets/TutorialInfo` 等とは明確に分離する。

`Scripts/` ではなく `Runtime/` を採用し、Editor専用コードは `Editor/` に分離する(Unity Editorの慣例的なasmdef分割に合わせる)。`_Project/Settings/` は将来のゲーム固有設定(バランス調整用ScriptableObject等)を想定した置き場所であり、既存の `Assets/Settings/`(URPのレンダーパイプライン設定一式、Unityテンプレート由来)とは責務が異なる。

各フォルダ配下には現時点でコード・アセットは存在せず、Git管理のため `.gitkeep` を配置している(空フォルダ運用の間の暫定措置。実ファイルが追加され次第 `.gitkeep` は削除してよい)。

### Assembly Definition方針(T-002で作成済み)
- レイヤーごとに asmdef を分割済み。実際のAssembly名と配置は以下の通り(いずれも `rootNamespace` はAssembly名と同一)。

| Assembly名 | 配置 | 参照するAssembly | 備考 |
|---|---|---|---|
| `FloatingIslandsRpg.Domain` | `Assets/_Project/Runtime/Domain/` | なし | `noEngineReferences: true`。UnityEngineおよび他のプロジェクト内Assemblyに依存しない純粋なC# |
| `FloatingIslandsRpg.Application` | `Assets/_Project/Runtime/Application/` | `FloatingIslandsRpg.Domain` | `noEngineReferences: true`。UnityEngineへ直接依存しない |
| `FloatingIslandsRpg.Infrastructure` | `Assets/_Project/Runtime/Infrastructure/` | `FloatingIslandsRpg.Domain`, `FloatingIslandsRpg.Application` | Presentationは参照しない。Unity API・保存処理・ScriptableObject実装を担当 |
| `FloatingIslandsRpg.Presentation` | `Assets/_Project/Runtime/Presentation/` | `FloatingIslandsRpg.Domain`, `FloatingIslandsRpg.Application` | Infrastructureへの直接参照は追加しない(Applicationを経由) |
| `FloatingIslandsRpg.Editor` | `Assets/_Project/Editor/` | なし | `includePlatforms: ["Editor"]`。現時点で必要なRuntime Assembly参照はないため未追加 |
| `FloatingIslandsRpg.Tests.EditMode` | `Assets/_Project/Tests/EditMode/` | `FloatingIslandsRpg.Domain`, `FloatingIslandsRpg.Application` | `includePlatforms: ["Editor"]`、`optionalUnityReferences: ["TestAssemblies"]`。テストコードは未作成 |
| `FloatingIslandsRpg.Tests.PlayMode` | `Assets/_Project/Tests/PlayMode/` | `FloatingIslandsRpg.Domain`, `FloatingIslandsRpg.Application`, `FloatingIslandsRpg.Infrastructure`, `FloatingIslandsRpg.Presentation` | `optionalUnityReferences: ["TestAssemblies"]`。テストコードは未作成 |

- 共通設定: `allowUnsafeCode: false`, `overrideReferences: false`, `autoReferenced: true`。
- 依存方向は下記「依存方向」通りに実装済み(循環参照なし、Unityコンパイル成功で確認済み)。
- テスト用asmdefは `FloatingIslandsRpg.Tests.EditMode` / `FloatingIslandsRpg.Tests.PlayMode` として作成済み(`FloatingIslands.Domain.Tests`のような単一レイヤー単位ではなく、EditMode/PlayModeの2アセンブリに統合)。

### Scene構成
- 3.仕様のScene一覧に準拠。エリア間(Title/Village/Field/Dungeon/GameClear)はScene遷移(Single方式)で切り替える。
- 戦闘Sceneは、フィールド/ダンジョンSceneに対する**Additiveロード方式**で重ねる。フィールド/ダンジョンSceneを破棄せず、戦闘Sceneをその上に加算ロードする。
- 戦闘中はフィールド/ダンジョン側の入力受付、カメラ制御、敵の思考・移動(進行)を停止する。戦闘終了後は戦闘Sceneをアンロードし、フィールド/ダンジョン側の状態を維持したまま復帰する。
- AudioListenerおよびEventSystemは同時に有効な状態が複数存在すること(重複)を禁止する。戦闘Sceneを加算ロードする際は、フィールド/ダンジョン側または戦闘側のいずれか一方のみが有効になるよう明示的に制御する。
- パーティ・クエスト進行状況等の永続データは、Sceneをまたいで保持するApplication層のセッション状態として管理し、個々のSceneのMonoBehaviourには持たせない。

### Scene識別子(T-003で作成済み、再レビュー対応反映済み)
- Scene名のマジックストリングを排除するため、`SceneId` (enum) と `SceneNameCatalog` (静的クラス) を導入。いずれも`FloatingIslandsRpg.Application`アセンブリに配置(UnityEngine非依存、`SceneManager`未使用)。
  - `Assets/_Project/Runtime/Application/Scenes/SceneId.cs`(namespace: `FloatingIslandsRpg.Application.Scenes`): `Title`, `Village`, `Field`, `Dungeon`, `Battle`, `GameClear` を定義。3.仕様「Scene一覧」の正式6Sceneと完全一致させている。
  - `Assets/_Project/Runtime/Application/Scenes/SceneNameCatalog.cs`: `public static string GetName(SceneId sceneId)` を公開。実際のScene名(文字列)は`private static readonly Dictionary<SceneId, string>`一箇所でのみ管理し、外部へは公開しない。未定義の`SceneId`には`ArgumentOutOfRangeException`を送出する。カタログの登録件数を検証用に`public static int RegisteredCount`として公開(テストでenumとカタログの過不足がないことを確認するため)。
- **SceneIdとScene名を分離した理由**: Scene遷移のユースケース(Application層)は「どのSceneへ」という意図を型安全な`SceneId`で表現し、実際のビルド設定・ファイル名に紐づく文字列(`"Title"`等)はInfrastructure/Presentation側の実装詳細として`SceneNameCatalog`の内部に閉じ込める。これにより、将来Scene名(ファイル名)が変わってもApplication層の呼び出し側コードを変更せずに済み、マジックストリングの直書きも一元管理下に置ける(5.規約「Scene名や文字列の直接指定を減らす」に対応)。
- **Codex第三者レビュー指摘への対応(本セッション)**:
  - Major: `SceneId`/`SceneNameCatalog`に含まれていた`Sample`・`Bootstrap`を削除し、PROJECT.md「3.仕様 Scene一覧」の正式6Scene(`Title`/`Village`/`Field`/`Dungeon`/`Battle`/`GameClear`)へ統一した。`SampleScene`はUnityテンプレート由来の暫定Sceneであり正式なゲームScene識別子ではないため、`SceneId`には含めない(実体としてのSceneファイルは7.要承認事項5の方針通り引き続き保持し、削除しない)。`Bootstrap`は現在の承認済みPROJECT.md正式Scene一覧に含まれておらず、必要になった時点でPROJECT.mdを更新・承認した上で別Taskとして追加する。
  - Minor: Scene名検証テストが`string.IsNullOrEmpty`だと空白のみの文字列を検出できなかったため、`string.IsNullOrWhiteSpace`に変更した。
- **今回のスコープ外**: 実際のSceneロード処理(`SceneManager`呼び出し)はT-003では実装しない(将来のT-009 Scene遷移ユースケースで実装予定)。

### キャラクターステータス計算(T-004で作成済み)
- HP/MP/攻撃力等の基礎ステータスとレベルに応じた成長値を、UnityEngine非依存のDomain層(`FloatingIslandsRpg.Domain`アセンブリ、`noEngineReferences: true`)で決定的に計算できるようにするため、`CharacterStats`・`StatGrowthProfile`・`CharacterStatsCalculator`の3型を作成した。
  - `Assets/_Project/Runtime/Domain/Characters/Stats/CharacterStats.cs`(namespace: `FloatingIslandsRpg.Domain.Characters.Stats`): 計算後のキャラクターステータスを保持する不変クラス。`Level`, `MaxHp`, `MaxMp`, `Attack`, `Defense`, `Agility`, `Magic`をコンストラクタで受け取り、生成後は外部から変更できない。`Level < 1`、`MaxHp < 1`、その他 < 0は`ArgumentOutOfRangeException`で拒否する。値比較のため`IEquatable<CharacterStats>`を実装(過剰実装を避けるため演算子オーバーロードは追加していない)。
  - `Assets/_Project/Runtime/Domain/Characters/Stats/StatGrowthProfile.cs`: レベル1時点の基礎値(`BaseMaxHp`等6種)、レベルが1上がるごとの成長値(`GrowthMaxHp`等6種)、`MinLevel`、`MaxLevel`を保持する不変クラス。`MinLevel < 1`、`MaxLevel < MinLevel`、`BaseMaxHp < 1`、その他基礎値 < 0、成長値 < 0はすべて`ArgumentOutOfRangeException`で拒否する。キャラクター固有の名称やアセット参照は持たない純粋な数値プロファイル。
  - `Assets/_Project/Runtime/Domain/Characters/Stats/CharacterStatsCalculator.cs`: `public static CharacterStats Calculate(StatGrowthProfile profile, int level)`。`profile`が`null`なら`ArgumentNullException`、`level`が`profile.MinLevel`〜`profile.MaxLevel`の範囲外なら`ArgumentOutOfRangeException`を送出する。乱数・キャッシュ・Unity APIは使用せず、同じ入力に対して常に同じ結果を返す。
- **採用した基礎ステータス(6種+Level)**: `MaxHp`, `MaxMp`, `Attack`, `Defense`, `Agility`, `Magic`(+ 識別用の`Level`)。3.仕様の時点ではHP/MP/攻撃力/素早さ等の名称のみが言及され6ステータスの正式名称は未定義だったため、本Taskの指示に基づきこの6種をDomain層の正式なステータス名として採用した(3.仕様の記述自体は変更していない)。
- **成長計算式**: `growthSteps = level - profile.MinLevel`、`stat = baseValue + perLevelGrowth * growthSteps` を6ステータスそれぞれに適用する。`level == profile.MinLevel`(通常はレベル1)では`growthSteps = 0`となり基礎値がそのまま返る。整数演算はすべて`checked`ブロック内で実行し、オーバーフロー時は黙って丸めず`OverflowException`を送出する。MVPでは浮動小数点を使わず整数成長のみとする。
- **レベル上限の方針**: ゲーム全体の最大レベルをDomain層にハードコードせず、`StatGrowthProfile.MaxLevel`としてプロファイルごとに指定する。将来キャラクター・職業ごとに異なるレベル上限を持たせられるようにするための設計判断。
- **今回のスコープ外**: `Character`/`Player`/`Enemy`等の具体的なキャラクタークラス、主人公固有の初期値、経験値テーブル、ダメージ計算・HP回復・装備補正・バフ処理は本Taskでは実装しない(T-005以降で対応予定)。

### 戦闘計算ロジック(T-005で作成済み)
- `Assets/_Project/Runtime/Domain/Combat/CombatCalculator.cs`(namespace: `FloatingIslandsRpg.Domain.Combat`): ダメージ計算・命中/回避判定・行動順決定を担う静的クラス。T-004の`CharacterStats`を入力として利用する(責務の重複実装はしていない)。
  - `CalculateDamage(attacker, defender)`: `damage = max(1, attacker.Attack - defender.Defense)`。最低ダメージは1で固定(`private const int MinimumDamage = 1`)。
  - `CalculateHitChance(attacker, defender)`: 基準命中率90%に、攻撃側と防御側のAgility差1ptあたり±1%を加減し、5%〜95%にクランプして返す(純粋関数、ステータスのみから決定)。
  - `ResolveHit(hitChance, randomRoll)`: `randomRoll < hitChance`を返すのみ。乱数はDomain内部で生成せず、呼び出し側(将来のApplication層)から渡された値を使う(5.規約のマジックストリング/乱数方針、および本Taskの「直接System.Randomを内部生成しない」方針に対応)。`hitChance`の有効範囲は`[0.0, 1.0]`(0%・100%を許容)、`randomRoll`の有効範囲は`[0.0, 1.0)`(1.0自体は無効)。`double.IsNaN`/`double.IsInfinity`を明示的にチェックし、NaN・±Infinityはいずれも`ArgumentOutOfRangeException`で拒否する(Codex第三者レビューMajor指摘対応、後述)。
  - `CompareTurnOrder(first, second)`: Agility降順比較(`second.Agility.CompareTo(first.Agility)`)。同値の場合は`0`を返し、タイブレークのポリシーはDomain層では決定せず呼び出し側に委ねる。
  - 入力検証: 各メソッドで`null`は`ArgumentNullException`、`ResolveHit`の`hitChance`/`randomRoll`が`[0,1]`範囲外なら`ArgumentOutOfRangeException`。
  - **オーバーフローについて**: `CharacterStats`のAttack/Defense/Agilityはいずれも0以上という不変条件があるため、減算・比較のみで構成される本ロジックは数学的にオーバーフローが発生し得ない(`checked`ブロックは防御的に設置済み)。そのためテストは実際にオーバーフローを発生させる代わりに、`int.MaxValue`等の極端な値でも正しく動作することを確認する形とした。
- **Codex第三者レビュー指摘への対応(本セッション)**:
  - Major: `ResolveHit`が`hitChance`/`randomRoll`の境界値・非数値を正しく検証できていなかった(`ResolveHit(1.0, 1.0)`が`false`になる、`double.NaN`が範囲検証を素通りする)。修正内容: `randomRoll`の上限判定を`> 1.0`から`>= 1.0`へ変更し有効範囲を`[0.0, 1.0)`に統一。`hitChance`/`randomRoll`双方に`double.IsNaN`・`double.IsInfinity`の明示チェックを追加(NaN・±Infinityはこれまでも一部は範囲比較で偶然弾かれていたが、意図が不明瞭だったため明示化)。`CalculateDamage`・`CalculateHitChance`・`CompareTurnOrder`は変更していない。
  - 本Taskのスコープ外: T-006・T-007の本番コードは変更していない。
- **今回のスコープ外**: 戦闘UI、戦闘Scene、MonoBehaviour、敵AI、アニメーション、エフェクト、属性相性、バフ・デバフ、装備処理、クリティカル、経験値、報酬、Scene遷移は実装しない(T-008以降で対応予定)。

### 経験値・レベルアップ計算(T-006で作成済み)
- `Assets/_Project/Runtime/Domain/Progression/ExperienceTable.cs`(namespace: `FloatingIslandsRpg.Domain.Progression`): レベルごとの累積必要経験値を外部配列(`IReadOnlyList<int>`)から受け取る不変クラス。防御的コピーを保持するため生成後に外部配列を変更しても影響を受けない。レベル1の必要経験値は必ず0、以降は単調増加でなければならず、違反時は`ArgumentException`。`MaxLevel`(配列長)と`GetRequiredExperience(level)`を公開し、範囲外`level`は`ArgumentOutOfRangeException`。
- `Assets/_Project/Runtime/Domain/Progression/LevelUpCalculator.cs`: `CalculateLevel(table, totalExperience)`。累積経験値から、閾値を超えている最大のレベルを返す純粋関数。`table`が`null`なら`ArgumentNullException`、`totalExperience`が負なら`ArgumentOutOfRangeException`。`MaxLevel`到達後は経験値がいくら増えても`MaxLevel`のまま(キャップ)。
- 経験値テーブルはキャラクター固有値としてコードへハードコードせず、外部から渡す配列として表現する(5.規約「マジックナンバー禁止」に対応)。
- **T-005との関係**: PROJECT.md上T-006はT-005ではなくT-004にのみ依存するため、`CombatCalculator`のAPIは利用していない(責務の重複・不要な結合を避けた)。
- **今回のスコープ外**: 経験値の実際の付与処理(戦闘勝利時の分配等、Application層)、ステータス再計算との連携(T-004の`CharacterStatsCalculator`呼び出し)は本Taskでは実装しない。

### クエスト状態管理(T-007で作成済み)
- `Assets/_Project/Runtime/Domain/Quests/QuestState.cs`(namespace: `FloatingIslandsRpg.Domain.Quests`): `NotStarted`/`InProgress`/`Completed`の3値enum(3.仕様「クエスト進行」の「未受注/進行中/完了」に対応)。
- `Assets/_Project/Runtime/Domain/Quests/QuestProgress.cs`: 単一クエストの状態を保持するクラス。生成時は常に`NotStarted`。`Start()`は`NotStarted`からのみ`InProgress`へ、`Complete()`は`InProgress`からのみ`Completed`へ遷移可能。それ以外の状態からの呼び出しは`InvalidOperationException`で拒否し、状態を黙って補正しない。
- メインクエスト1本・サブクエスト2本の「独立した管理」は、`QuestProgress`を3つの独立したインスタンスとして扱うことで実現する(各クエストの識別・集約はApplication層の責務としてDomainには`QuestId`やQuestManager相当のクラスを追加していない)。
- **今回のスコープ外**: メイン/サブクエストそれぞれの具体的な受注条件・進行条件・完了条件の実装、クエストの識別子・集約管理(Application層)は本Taskでは実装しない。

### 戦闘進行ユースケース(T-008で作成済み)
- `Assets/_Project/Runtime/Application/Battle/`(namespace: `FloatingIslandsRpg.Application.Battle`)に、`BattleSession`(戦闘進行を管理する状態保持クラス)、`BattleParticipantState`(参加者の現在HPを管理)、`BattleActionResult`/`BattleTurnResult`(行動・ターン結果DTO)、`BattleCommand`/`BattleOutcome`(enum)、`IRandomSource`(乱数注入インターフェース)を作成。T-005の`CombatCalculator`(ダメージ計算・命中率算出・行動順比較)をそのまま利用し、計算式を重複実装していない。
- `IRandomSource.NextDouble()`は`[0.0, 1.0)`を返す契約とし、`CombatCalculator.ResolveHit`のrandomRoll契約と一致させた。Application層はSystem.Randomを内部生成しない(乱数は`IRandomSource`実装側から注入)。
- 行動順は`CombatCalculator.CompareTurnOrder`のAgility比較結果を用いる。同速(比較結果が0)の場合はプレイヤー側が先制するとApplication層で決定した(Domain層はタイブレークを決定しない設計のため)。
- MVP範囲は主人公1人対敵1体・Attackコマンドのみ。戦闘不能になった側は以後行動せず、戦闘終了(`PlayerVictory`/`PlayerDefeat`)後に`ExecuteTurn`を呼ぶと`InvalidOperationException`を送出する。
- **今回のスコープ外**: 戦闘UI、戦闘Scene、MonoBehaviour、敵AI、アニメーション、エフェクト、属性相性、バフ・デバフ、装備処理、クリティカル、経験値・報酬付与、Scene遷移は実装しない。
- **Codex第三者レビュー指摘への対応(本セッション)**: Minor指摘(`BattleTurnResult.Actions`が`IReadOnlyList`型で公開されているが実体は内部の`List<BattleActionResult>`のため、`(List<BattleActionResult>)result.Actions`のようにキャストしてAdd/Clearが可能だった)を解消。内部保持を`ReadOnlyCollection<BattleActionResult>`(`.AsReadOnly()`)へ変更し、防御的コピー後にラップすることで、入力元Listの事後変更の非影響とキャストによる変更不可(`List<T>`へのキャストは`InvalidCastException`、`IList<T>`へのキャストからのAdd呼び出しは`NotSupportedException`)の両方を満たす。`BattleSession`等の戦闘ロジックは変更していない。

### Scene遷移ユースケース(T-009で作成済み)
- `Assets/_Project/Runtime/Application/Scenes/`に`ISceneLoader`(インターフェース)、`SceneLoadMode`(Single/Additive)、`SceneTransitionUseCase`を追加。T-003の`SceneId`/`SceneNameCatalog`を使用し、Scene名の直接文字列指定を行わない。
- `Assets/_Project/Runtime/Infrastructure/Scenes/UnitySceneLoader.cs`: `ISceneLoader`の実装。`UnityEngine.SceneManagement.SceneManager`を使用するのはこのクラスのみ。
- `SceneTransitionUseCase`は同時実行(再入)を検出すると`InvalidOperationException`を送出し、`ISceneLoader`側の例外はそのまま呼び出し元へ伝播させる(握りつぶさない)。
- 通常SceneはSingle、Battle SceneはAdditiveで読み込む想定(呼び出し側が`SceneLoadMode`を指定)。`UnloadScene`でBattle Sceneのアンロード要求を表現できる。
- **今回のスコープ外**: 新規Scene作成、SampleScene変更、EditorBuildSettings変更、実際のBattle Additive統合、AudioListener/EventSystem操作、Scene履歴・フェード・ローディング画面は実装しない。
- **Codex第三者レビュー指摘への対応(本セッション)**: Major指摘(`UnitySceneLoader`は`LoadSceneAsync`/`UnloadSceneAsync`を開始するだけの非同期処理であるにもかかわらず、`SceneTransitionUseCase`は`ISceneLoader`呼び出し直後に遷移中フラグを解除しており、Unity側のロード/アンロード完了前に次の遷移要求を受け付けてしまっていた)を解消。`ISceneLoader`を`Task LoadAsync(...)`/`Task UnloadAsync(...)`のTaskベースへ変更し、`SceneTransitionUseCase`を`async Task`の`TransitionToAsync`/`UnloadSceneAsync`として、実際の完了(`await`完了または例外)まで`finally`で遷移中フラグを保持・解除する構成にした。`UnitySceneLoader`は`AsyncOperation.isDone`/`completed`イベントを`TaskCompletionSource`でTask化し、`LoadSceneAsync`/`UnloadSceneAsync`が`null`を返す場合は`InvalidOperationException`を送出する。`async void`は使用せず、新規Packageも追加していない。CancellationToken・遷移キュー・履歴管理・フェード処理は本対応では追加していない。

### セーブ/ロードユースケース・PlayerSessionState(T-010で作成済み)
- `Assets/_Project/Runtime/Application/Session/PlayerSessionState.cs`: 現在`SceneId`・`CharacterStats`・累積経験値・現在HP/MP・メイン/サブ2クエストの`QuestProgress`を保持する実行時セッションモデル。4.設計「Scene構成」の「Sceneをまたいで保持するApplication層のセッション状態」方針に対応する。public setterは持たず、`MoveToScene`/`SetCurrentHp`/`SetCurrentMp`/`GainExperience`等の検証付きメソッドでのみ状態を変更する。
- `Assets/_Project/Runtime/Application/Save/`: セーブDTO`SaveGameSnapshot`(`SaveVersion`を含むシリアライズ用データ)を`PlayerSessionState`とは別型として定義し、`PlayerSessionStateMapper`で相互変換する(セーブDTOと実行時モデルを同一型にしない方針に対応)。`ISaveRepository`(Application定義のインターフェース)、`SaveGameUseCase`/`LoadGameUseCase`、`SaveResult`/`LoadResult`を作成。
- 未対応の`SaveVersion`や不正な`QuestState`は`PlayerSessionStateMapper`が例外を送出し、`LoadGameUseCase`がそれを捕捉して`LoadResult.Failed`へ変換する(想定内の失敗を戻り値で扱う5.規約の方針に対応)。
- **スコープ判断**: 経験値からのレベル再計算(T-004/T-006連携)やレベルアップ時のステータス再計算はT-010の範囲外とし、`GainExperience`は累積経験値の加算のみを行う(推測による先行実装を避けた)。

### セーブデータ保存基盤(T-011で作成済み)
- `Assets/_Project/Runtime/Infrastructure/Save/`に`FileSystemSaveStorage`(一時ファイル書き込み→読み戻し検証→`File.Replace`による本ファイル置換+1世代バックアップ)、`JsonSaveRepository`(`ISaveRepository`実装、`UnityEngine.JsonUtility`でシリアライズ)を作成。
- `UnityEngine.JsonUtility`はpublicフィールドのみをシリアライズしC#プロパティを認識しないため、`SaveGameSnapshot`をプロパティからpublicフィールド+`[System.Serializable]`(`System.SerializableAttribute`、UnityEngine非依存)へ変更した(T-010作成分への調整。呼び出し側の構文は変わらないため影響なし)。
- 保存先ディレクトリは`FileSystemSaveStorage`のコンストラクタ引数として注入可能(`Application.persistentDataPath`のハードコードなし)。破損データ読込時は自動的にバックアップへフォールバックし、両方とも破損している場合は`TryLoad`が`false`を返す(安全な初期状態への復帰はLoadGameUseCase呼び出し元の責務)。
- **確認方法についての注記**: `Assets/_Project/Tests/EditMode`の既存asmdef(T-002)がInfrastructureを参照しない設計のため、PROJECT.md記載の「EditModeテストで検証」ではなく**PlayModeテストで検証した**(`Tests.PlayMode.asmdef`はDomain/Application/Infrastructure/Presentationを参照済みのため、追加のasmdef変更は不要だった)。
- 新しいPackageは追加していない(Unity標準の`JsonUtility`のみ使用)。
- **Codex第三者レビュー指摘への対応(本セッション)**: Major指摘(一次保存データがJSONとして読み取れた時点で成功扱いとしていたため、未対応SaveVersion・MaxHp=0・CurrentHp>MaxHp・不正なSceneId/QuestStateなど「構文上は正常だがゲームデータとして無効」な一次ファイルに対してバックアップへフォールバックできていなかった)を解消。`JsonSaveRepository.TryLoad`を「JSON解析成功」→「`PlayerSessionStateMapper.FromSnapshot`相当の意味検証成功」の2段階を通過した候補のみ有効とする構成に変更し、一次候補が失敗した場合のみ同条件でバックアップを検証する。意味検証は`ArgumentException`/`NotSupportedException`のみを捕捉し、`OutOfMemoryException`等の致命的例外は広く握りつぶさない。読込処理は一次・バックアップいずれのファイルも書き換えない。`ISaveRepository`の公開APIおよびT-010の`PlayerSessionStateMapper`/`LoadGameUseCase`は変更していない(Infrastructure→Applicationの既存依存方向内で`PlayerSessionStateMapper`を呼び出すのみ)。

### マスターデータ定義(T-012で作成済み)
- `Assets/_Project/Runtime/Domain/MasterData/`に`EnemyMasterData`/`ItemMasterData`/`EquipmentMasterData`(不変クラス、ID・表示名等の入力検証付き)、`EquipmentSlot`(enum: Weapon/Armor)、`MasterDataValidator.EnsureUniqueIds`(ID重複検出)を作成。
- `Assets/_Project/Runtime/Infrastructure/MasterData/`に`EnemyDefinition`/`ItemDefinition`/`EquipmentDefinition`(ScriptableObject、`[CreateAssetMenu]`付き)を作成。各`ToMasterData()`メソッドが対応するDomain型のコンストラクタ検証を経由して変換する(Mapperの妥当性検証はDomain型のコンストラクタに一本化し、重複実装していない)。Addressablesは使用していない。
- 実際のゲームデータAsset(通常敵3種・ボス1体・アイテム・装備等)は作成していない。PROJECT.mdの完了条件は「データ定義クラスが用意されている」ことであり、ダミーAssetの大量作成は避けた。
- **確認方法についての注記**: PROJECT.mdの確認方法は「Unity Editorでアセット作成が可能なことを確認」であり自動テストを必須としていない。検証ロジック自体(ID検証・重複検出等)はDomain層のプレーンC#型としてEditModeテストで網羅した。

### プレイヤー移動・カメラ(T-013で作成済み)
- `Assets/_Project/Runtime/Presentation/Player/PlayerMovement.cs`: 新Input System(`Player/Move`)によるCharacterController移動・移動方向への回転。`Assets/_Project/Runtime/Presentation/Cameras/FollowCamera.cs`: 対象Transformへの追従(Lerp)・注視。`Assets/_Project/Prefabs/Player.prefab`を作成。
- PlayModeテスト6件(`PlayerMovementTests`3件、`FollowCameraTests`3件)。フレーム依存の時間差による誤検出を避けるため、複数フレームの経過時間(`Time.deltaTime`合計)から平均水平速度を算出して比較する方式を採用(1フレームの生距離比較はしない)。

### NPC会話UI(T-014で作成済み)
- `Assets/_Project/Runtime/Presentation/Dialogue/`に`DialogueSession`(ページ送り状態を持つ純粋C#クラス、UnityEngine非依存)、`DialogueBoxView`(MonoBehaviour、共有UI表示・Submit入力でのテキスト送り)、`NpcInteractable`(MonoBehaviour、NPCごとの会話トリガー・プレイヤー移動の一時無効化・T-007 `QuestProgress`との連携)を作成。
- クエスト状態はPresentation内で独自に保持せず、外部から注入された`QuestProgress`(T-007)インスタンスをそのまま参照・操作する(`Start()`呼び出しのみ。`Complete()`は本Taskの対象外)。
- `Assets/_Project/Prefabs/DialogueBox.prefab`を作成(UnityEngine.UI Text使用)。
- PlayModeテスト22件(`DialogueSessionTests`8件、`DialogueBoxViewTests`6件、`NpcInteractableTests`8件)。

### 戦闘UI(T-015で作成済み)
- `Assets/_Project/Runtime/Presentation/Battle/BattleUIController.cs`: T-008 `BattleSession.ExecuteTurn(BattleCommand.Attack)`をAttackボタンから呼び出し、`BattleTurnResult`/`BattleActionResult`をHP表示・戦闘ログ・勝敗表示へ変換する。ダメージ計算・命中判定・行動順の再実装はしていない。`BattleSession`はController外部から`Bind()`で注入する(Presentationが戦闘状態を生成しない)。
- `Assets/_Project/Prefabs/BattleUI.prefab`を作成。
- PlayModeテスト8件(`BattleUIControllerTests`)。

### タイトル画面(T-016で作成済み)
- `Assets/_Project/Runtime/Presentation/Title/TitleScreenController.cs`: T-010 `LoadGameUseCase`を`Bind()`で外部から注入し、`Load()`結果でContinueボタンの有効/無効とエラー表示を切り替える。New Game/Continue/Quitクリックは`NewGameRequested`/`ContinueRequested`/`QuitRequested`のC#イベントとして公開し、実際のScene遷移(T-009)は呼び出し側(合成ルート)の責務とする。Quitは`#if UNITY_EDITOR`でEditor/Build双方に対応。
- 正式`Assets/_Project/Scenes/Title.unity`をUnity MCPで新規作成。
- PlayModeテスト9件(`TitleScreenControllerTests`)。

### ゲームクリア/ゲームオーバー画面(T-017で作成済み)
- `Assets/_Project/Runtime/Presentation/Results/GameResultScreenController.cs`: T-008 `BattleOutcome`(`PlayerVictory`/`PlayerDefeat`)を`Show()`で外部から受け取り、Clear/Over表示を切り替える(勝敗の再計算はしない。`InProgress`等の不正な値は`false`を返して安全に無視する)。Title遷移・Retry(RematchSnapshotから戦闘開始直前の状態を復元し、Battle Sceneを再ロードして再戦)は`TitleRequested`/`RetryRequested`イベントとして公開し、実際のロード・Scene遷移は呼び出し側の責務とする。
- `SceneId`(T-003)に`GameOver`は存在しないため、新規SceneId追加はせず単一の`GameClear.unity`内でClear/Over表示を切り替える設計とした。
- 正式`Assets/_Project/Scenes/GameClear.unity`をUnity MCPで新規作成。
- PlayModeテスト7件(`GameResultScreenControllerTests`)。

### Composition Root(Codex第三者レビューMajor 1指摘対応、本セッションで作成)
- **課題**: `BattleUIController.Bind(BattleSession)` / `TitleScreenController.Bind(LoadGameUseCase)`等、Presentationの各Controllerは外部注入されるBindパターンで設計したが、実際に`ISaveRepository`/`ISceneLoader`のInfrastructure実装を生成しControllerへ結線する仕組みが存在せず、テストや一時的なUnity Editor操作でしか動作を確認できていなかった。
- **採用方式**: 新規asmdef`FloatingIslandsRpg.Composition`(`Assets/_Project/Runtime/Composition/`、参照: Domain, Application, Infrastructure, Presentation, Unity.InputSystem)を作成し、本番ランタイムでの依存組み立て専用レイヤーとした。`Presentation`/`Application`からは引き続き`Infrastructure`を直接参照しない。
  - `GameServices`(純粋C#): `ISaveRepository`/`SaveGameUseCase`/`LoadGameUseCase`/`ISceneLoader`/`SceneTransitionUseCase`(すべて既存コンストラクタで生成)を保持し、実行時セッション(`PlayerSessionState CurrentSession`、`BattleOutcome? LastBattleOutcome`)を明示的に保持する。static状態は一切持たない。
  - `GameCompositionRoot`(MonoBehaviour): `Awake()`で`GameServices`を生成し`DontDestroyOnLoad`する。`FindObjectsByType`による重複検出で二重生成を防止(static Instanceは公開しない)。`EnsureServices()`により、何らかの理由で既存インスタンスの`Services`がnullな場合も自己修復する(回帰テスト`EnsureRoot_ExistingRootHasNullServices_ReconstructsServices`で検証)。
  - `GameCompositionRootLocator.EnsureRoot()`(static、状態は持たないユーティリティ関数): 各SceneのInstallerが一度だけ呼び出し、既存Rootを発見または新規作成する。
  - Scene別Installer(`TitleSceneInstaller`, `VillageSceneInstaller`, `BattleSceneInstaller`, `GameClearSceneInstaller`): 各SceneのController/NPCへ依存を注入し、Controllerのイベントを購読してT-009 `SceneTransitionUseCase`経由の実遷移を行う。すべて`OnDestroy`で購読解除する。
  - `FloatingIslandsRpg.Infrastructure.Battle.SystemRandomSource`(`IRandomSource`実装、`System.Random`ラッパー)を追加し、本番Battle SceneでBattleSessionへ実際の乱数を供給する。
  - `Tests.PlayMode.asmdef`に`FloatingIslandsRpg.Composition`参照を追加(検証専用の例外。本番4層[Domain/Application/Infrastructure/Presentation]同士の相互参照ルールはそのまま維持)。他のアセンブリからCompositionは参照させていない。

### T-013〜T-017 本番経路接続(Codex第三者レビューMajor 2/3/4指摘対応)
- **T-013**: `Village.unity`のMain Cameraに`FollowCamera`を追加し、Scene内に配置した`Player.prefab`インスタンスへ`SetTarget()`で接続(Edit時設定)。Camera/AudioListenerは各Scene1つのみ。
- **T-014**: `Village.unity`へ最小NPC(`Villager`、Primitiveの見た目、`SphereCollider`トリガー)を配置し、`NpcInteractable`に`DialogueBoxView`/`PlayerMovement`/Interact用`InputActionReference`をEdit時設定。`VillageSceneInstaller`が`GameServices.CurrentSession.MainQuest`を`NpcInteractable.LinkedQuest`へ実行時注入する。**T-018(村エリア本実装)の完了条件(NPC3体以上・Field接続等)は満たさない配線検証専用のスキャフォールドである**ことを明記する。
- **T-015**: `Battle.unity`を新規作成し`BattleUI.prefab`を配置。`BattleUIController`に`event Action<BattleOutcome> BattleEnded`を追加(既存Bind/ExecuteTurn呼び出しは変更なし)。`BattleSceneInstaller`が`GameServices.CurrentSession.Stats`(未設定時はプレースホルダー)からPlayer/Enemy(プレースホルダー)の`BattleSession`を生成し`Bind()`、`BattleEnded`購読でT-009経由`GameClear`へ遷移。
- **T-016**: `TitleSceneInstaller`が`GameServices.LoadGameUseCase`を`TitleScreenController.Bind()`へ注入。New Game時はプレースホルダー初期`CharacterStats`で`PlayerSessionState`を生成し`SceneId.Village`へ、Continue時はロード済み状態の`CurrentSceneId`へ、T-009経由で実遷移する。
- **T-017**: `GameClearSceneInstaller`が`GameServices.LastBattleOutcome`を`GameResultScreenController.Show()`へ注入。Title/RetryクリックはT-009経由で実遷移。
- **プレースホルダーデータについての注記**: 開始時プレイヤーステータス・Battleの敵ステータスは、実際のMasterData Asset(T-012スコープ外、実データ未作成)が存在しないため、Composition層に明示コメント付きの固定値を仮置きした。将来、実データAssetが用意され次第置き換える。
- **Prefab化の一部未対応(Codex判定によりMVPでは必須ではない)**: `DialogueBox.prefab`/`BattleUI.prefab`は作成したが、`Title.unity`/`GameClear.unity`のUIはScene専用のためPrefab化していない。再利用が発生した時点で将来Taskとして検討する。
- `Presentation.asmdef`に`UnityEngine.UI`参照を追加した(`com.unity.ugui`は既存インストール済みパッケージであり新規Package追加ではない)。

### T-017 Retry仕様の修正、および遷移失敗時のUI復旧(Codex最終再レビューMajor 2件・Minor 2件対応)
- **Retryの遷移先をBattleへ固定**: Codex指摘により、Retryが保存データの`CurrentSceneId`(例: Village)へ遷移してしまう不具合を修正した。`GameClearSceneInstaller.OnRetryRequested`は`LoadGameUseCase`を使わず、`GameServices.RematchSnapshot`から`CurrentSession`を復元したうえで、常に`SceneId.Battle`へT-009経由で遷移する。
- **再戦用状態(`GameServices.RematchSnapshot`)**: `GameServices`が`PlayerSessionState RematchSnapshot`を新たに所有する(static状態・Service Locatorは使用しない、インスタンスプロパティ)。`BattleSceneInstaller.Start()`が、Battle開始直前の`CurrentSession`(Stats・TotalExperience・CurrentHp・CurrentMp・Quest参照)を既存の`PlayerSessionState`コンストラクタで防御的コピーし`RematchSnapshot`へ保存する。これにより、Retry時は「戦闘開始直前のHP」(敗北後の0ではない)へ復元される。`CurrentSession`が未設定の場合はプレースホルダーの満タンHPで代替する。
- **RematchSnapshotが存在しない場合**: `GameResultScreenController.ShowError()`(新規、`_errorText`表示)でエラーを表示し、遷移を行わない(安全側に倒す)。
- **LastBattleOutcomeのクリア箇所**: (1) New Game時(`TitleSceneInstaller.OnNewGameRequested`、`RematchSnapshot`も同時にクリア)、(2) Continue時(`TitleSceneInstaller.OnContinueRequested`、`RematchSnapshot`も同時にクリア。保存データに古い戦闘結果・再戦状態を持ち越さない)、(3) GameClearからTitleへ戻る時(`GameClearSceneInstaller.OnTitleRequested`)、(4) Retry開始時(`GameClearSceneInstaller.OnRetryRequested`の先頭、RematchSnapshot有無の判定より前)。
- **遷移失敗時のUI復旧**: `TitleScreenController`/`GameResultScreenController`に`CompleteTransition()`/`FailTransition()`を追加した。各Scene Installer(`TitleSceneInstaller`, `GameClearSceneInstaller`)は、`SceneTransitionUseCase.TransitionToAsync`呼び出しを`try/catch/finally`で包み、例外は`Debug.LogException`でログへ残したうえで(握りつぶさない)、`finally`で成功時`CompleteTransition()`・失敗時`FailTransition()`を必ず呼び出す。`FailTransition()`は遷移中フラグを解除しボタンを再度押せる状態へ戻す。非同期処理の実体(`TransitionAsync`)はTaskを返すprivateメソッドとして分離し、Unityイベント購読口(`OnNewGameRequested`等)のみを`async void`とした。`BattleUIController`については、`ShowResult()`が`BattleEnded`イベント発火(≒Scene遷移開始)より前に同期的に完了しているため、GameClearへの遷移が失敗してもBattle画面自体の表示状態(結果パネル・Attackボタン無効化)は元々破綻しない設計であることをテストで確認し、追加のController APIは設けていない。
- **Locatorの用途は変更していない**: `GameCompositionRootLocator.EnsureRoot()`はScene Installerからのみ呼び出す用途のまま維持し、任意のControllerが直接サービスを取得できるAPIは追加していない。static可変状態・Singleton公開も引き続き行っていない。

### 村エリア・フィールド・ダンジョン本実装(T-018〜T-020で作成済み)
- **共通の設計判断(T-019着手時に決定)**: PROJECT.md 4.設計「Scene構成」は、Battle Sceneをフィールド/ダンジョンSceneに対して**Additiveロード**する設計を承認済みの仕様として定めている。T-015時点ではField/Dungeonが未作成だったため、Battle Sceneは暫定的にSingleロードで実装されていたが、T-019着手時にこの設計上の負債を解消し、Battle SceneのロードモードをField/Dungeonからは**Additive**、GameClearへは引き続き**Single**に切り替えた(`SceneTransitionUseCase`/`ISceneLoader`(T-009)のAPI自体は変更なし。呼び出し側が`SceneLoadMode`を使い分けるのみ)。
- **`GameServices.PendingBattle`(`PendingBattleContext`、新設)**: Field/DungeonSceneInstallerがAdditive Battleロード直前に設定する一時的なコンテキスト(`ReturnSceneId`、`IsBossEncounter`)。`BattleSceneInstaller`が戦闘終了時にこれを消費し、(1)`IsBossEncounter=false`かつ`PlayerVictory`ならBattle Sceneをアンロードしてフィールド/ダンジョンへ復帰、(2)それ以外(ボス勝利、または通常/ボス問わず敗北)は既存どおり`SceneId.GameClear`へSingle遷移する。`New Game`/`Continue`(`TitleSceneInstaller`)でも`LastBattleOutcome`/`RematchSnapshot`と同様に明示的にクリアする。static状態・Service Locatorは使用していない(`GameServices`のインスタンスプロパティ)。
- **`FieldActivityGate`(Presentation、新設)**: Additive Battle中、フィールド/ダンジョン側のCamera(`FollowCamera`ごと)・AudioListener・EventSystem・`PlayerMovement`・`FieldEncounterController`を`Pause()`で無効化し、`Resume()`で復帰させる。4.設計「Scene構成」の「戦闘中はフィールド/ダンジョン側の入力受付・カメラ制御を停止する」「AudioListener/EventSystemの重複禁止」に対応する。
- **`FieldEncounterController`(Presentation、新設)**: プレイヤーTransformの移動距離を`Update()`で累積し、一定距離(`_distancePerCheck`)ごとに`IRandomSource.NextDouble()`(T-008で定義済み、本番は`SystemRandomSource`(T-013時点で導入済み)をComposition層が注入)で確率判定してランダムエンカウントを発生させる(3.仕様「フィールド探索」「エンカウント方式」、シンボルエンカウントではない)。Field/Dungeon双方で同一コンポーネントを再利用し、責務を重複実装していない。
- **`SceneTransitionTrigger`(Presentation、新設)**: プレイヤーの接触で設定済み`SceneId`/`SceneLoadMode`へのScene遷移を要求するC#イベント(`TransitionRequested`)を公開するトリガー。実際の遷移(T-009経由)はScene Installer側の責務とし、遷移失敗時は`AllowRetry()`で再度反応可能な状態へ戻す(Title/GameClearの`FailTransition()`と同様の考え方)。Village→Field、Field→Village/Dungeon、Dungeon→Fieldの接続に使用する。
- **`BossEncounterTrigger`(Presentation、新設)**: ダンジョン最奥のボス部屋入口に配置し、プレイヤー接触で確定的に(確率judgment なしで)`BossEncounterTriggered`イベントを発火する。`FieldEncounterController`のランダム判定とは独立したコンポーネントとし、責務を混在させていない。
- **T-018 村エリアの実装**: 既存の最小`Village.unity`(NPC1体)を拡張し、`Npc.prefab`(Capsule見た目+SphereCollider(trigger)+`NpcInteractable`)を新規作成、既存Villagerに加えElder・Merchantの計3体のNPCを配置(3.仕様#2「NPCが3体以上配置」に対応、対話内容はプレースホルダー文言)。`VillageSceneInstaller`が`SceneTransitionTrigger`(Field行き、Single)を購読するよう拡張。PlayModeテスト7件追加(`SceneTransitionTriggerTests`4件、`VillageSceneInstallerTests`+3件)。
- **T-019 フィールドエリアの実装**: 正式`Field.unity`をUnity MCPで新規作成(Ground・Player・`FieldEncounterController`・`FieldActivityGate`・Village行き/Dungeon行きの`SceneTransitionTrigger`・`FieldSceneInstaller`)。通常敵エンカウントはプレースホルダー1種(既存`BattleSceneInstaller`の敵ステータスを`RegularEncounterEnemyStats`として維持、値は変更していない)。PlayModeテスト17件追加(`FieldEncounterControllerTests`6件、`FieldActivityGateTests`2件、`FieldSceneInstallerTests`5件、`BattleSceneInstallerTests`+4件)。
- **T-020 ダンジョンの実装**: 正式`Dungeon.unity`をUnity MCPで新規作成(T-019と同じ`FieldEncounterController`/`FieldActivityGate`/`SceneTransitionTrigger`(Field行き)を再利用し、最奥に`BossEncounterTrigger`を配置)。ボス用プレースホルダー`BossEncounterEnemyStats`(HP40/攻撃8/防御4/敏捷3、レベル5)を`BattleSceneInstaller`に追加し、通常敵`RegularEncounterEnemyStats`(HP12)より明確に強いステータスとした(2.スコープ#8「通常敵より明確に強いステータス」に対応。実データMasterData Assetは引き続き未作成で、T-012スコープ外の据え置き)。`DungeonSceneInstaller`が`FieldEncounterController`(通常戦)・`BossEncounterTrigger`(ボス戦、`PendingBattle.IsBossEncounter=true`)双方をAdditive Battleへ橋渡しする。PlayModeテスト10件追加(`BossEncounterTriggerTests`4件、`DungeonSceneInstallerTests`6件)。
- **T-018〜T-020合計の新規PlayModeテスト**: 34件(7+17+10)。
- **手動確認(Unity Editor実機Play Mode)**: (1) Village上でVillager/Elder/Merchant全3体との会話開始〜終了、PlayerMovement無効化/復帰を確認。(2) Village→(FieldEntranceトリガー接触)→Field実遷移を確認。(3) Field上でエンカウントイベントを発火させ、Battle SceneがAdditiveロードされる(Fieldはアンロードされず併存)ことを`get_loaded_scenes`で確認、`FieldActivityGate.Pause()`によりFieldのCamera/AudioListener/PlayerMovementが無効化されることを確認、Attack操作で通常敵に勝利しBattleがアンロードされ、Field側のCamera/AudioListener/EventSystem/PlayerMovementが復帰することをすべて確認(スクリーンショットあり)。(4) Field↔Village、Field→Dungeonの`SceneTransitionTrigger`による実遷移を確認。(5) Dungeon上でBossEncounterTriggerに接触しBattleがAdditiveロードされ、ボス用プレースホルダー(HP40/40)が表示されることを確認。弱いプレースホルダーPlayerStatsでの敗北時にGameClear(GAME OVER表示、Retry/Titleボタンあり)へSingle遷移すること、強いテスト用PlayerStatsでの勝利時にGameClear(GAME CLEAR!表示、Titleボタンのみ)へSingle遷移すること(=Dungeonへは戻らない)の両方を確認。(6) Dungeon↔Fieldの`SceneTransitionTrigger`による実遷移を確認(スクリーンショットあり)。
- **観測されたConsoleノイズ(プロジェクトコード起因ではないと判断)**: 手動確認中、Additive Battleロード直後に`The referenced script (Unknown) on this Behaviour is missing!`という警告と、`GameCompositionRoot.Services`/`BattleUIController`内部状態が一時的にnullになる事象を1回観測した。直後に全6正式Sceneを`manage_scene validate`、および全ロード済みScene・Prefabインスタンスに対する`GetComponents<Component>() == null`の直接スキャンを実施した結果はいずれも0件で、Scene/Prefabアセット自体は健全であることを確認した。再度、MCPツール呼び出しの間隔を空けて同じ手順を再実行したところ再現せず(Additive/Unload/Resumeが正常に完了し、`GameServices`/`_session`等の状態も一貫していた)、既存の「既知の問題」に記載されたMCP自動化ツールによる高速なPlay Mode操作に起因する一過性の事象と判断した(詳細は「既知の問題」に追記)。
- **観測された例外(1回、手動確認中)**: Dungeon→Field→Dungeonの`SceneTransitionTrigger`を人間の歩行では起こり得ない速さ(2箇所のトリガーへ連続してテレポートするテスト手順)で連続作動させた際、`SceneTransitionUseCase`の再入防止ガードにより`InvalidOperationException: A scene transition is already in progress.`が送出された(ログに記録され、握りつぶされていない)。直後の状態確認で、Sceneのロード状態・`GameServices`(`PendingBattle`/`LastBattleOutcome`とも null)に不整合がないことを確認済みであり、再入防止ガードが意図通りに機能した結果と判断した。通常の徒歩移動ではトリガー間の移動に数秒以上を要するため、実プレイでは発生しない想定。

### T-018〜T-020 Codex第三者レビュー指摘対応(Major1件・Minor4件、本セッションで対応完了)
- **Major: Battle Sceneアンロード失敗時にField/Dungeon側が復旧しない**: `BattleSceneInstaller.ReturnToFieldAsync`(旧実装)は`UnloadSceneAsync(Battle)`成功時のみ`ResumeReturnScene()`(全`FieldActivityGate`の`Resume()`)を呼んでおり、アンロードが例外を送出した場合は`catch`で`Debug.LogException`するのみで、Field/Dungeon側のPlayer入力・Camera・AudioListener・EventSystem・`FieldEncounterController`が無効化されたまま復旧しない欠陥があった。
  - **修正方式**: `OnBattleEnded`の先頭で`PendingBattleContext`をローカル変数へ退避してから`_services.PendingBattle = null`で即座にクリアする(今回の戦闘の文脈が次回の戦闘・Retry・Title遷移へ残らないようにする)。`ReturnToFieldAsync`を`try/catch/finally`化し、アンロードの成功可否を示す`bool unloaded`ローカル変数で分岐する。`finally`内で必ず`ResumeReturnScene()`を(成功・失敗を問わず)1回だけ呼び出し、Field/Dungeon側のPlayer入力・Camera・AudioListener・EventSystem・エンカウント判定を復旧させる。アンロードが失敗した場合(Battle Sceneがまだ読み込まれたまま)は、`ResumeReturnScene()`の直前に`DisableBattlePresentation()`を呼び、Battle Scene自身のCamera・AudioListener・EventSystemを無効化してから復帰元を有効化することで、両Scene分のCamera/AudioListener/EventSystemが同時に有効化される競合を防ぐ。Battle側のCamera/AudioListener/EventSystemの参照は`_battleCamera`/`_battleAudioListener`/`_battleEventSystem`として`[SerializeField]`化されており、`Battle.unity`のInspectorでBattle Scene内の対象へ明示的に設定する(グローバル検索は行わない。詳細は4.設計「T-019/T-020 Codex最終再レビュー残存Major対応」参照)。`UnityEngine.SceneManagement.SceneManager`を本層(Composition)から直接呼び出してはいない(T-009の方針「SceneManagerの使用は`UnitySceneLoader`のみ」を維持)。例外は`Debug.LogException`で記録し、握りつぶしていない。ボス勝利・敗北時のGameClearへのSingle遷移ロジック(`TransitionAsync`)は変更していない。
  - static可変状態・Service Locator・新規Packageは追加していない。
- **Minor: ボス勝利テストの乱数依存**: `BattleSceneInstallerTests.BattleEnded_BossEncounterVictory_TransitionsToGameClearInstead`が実際の`SystemRandomSource`(本番用乱数)に依存し、`Assert.Inconclusive`で敗北時を逃していたため、将来的にInconclusive/flakyになるリスクがあった。`BattleUIController.BattleEnded`イベントを反射(reflection)で直接`Invoke(BattleOutcome.PlayerVictory)`する方式に置き換え、`BattleSession`・`SystemRandomSource`・Attackクリックを一切経由しない完全に決定的なテストとした(本番コードへテスト専用分岐は追加していない)。同じ手法で`BattleEnded_RegularEncounterVictory_UnloadSucceeds_ResumesFieldGateAndClearsPendingBattle`、および新規の敗北系テスト2件も決定的化した。
- **Minor: git diff --checkの実態**: `git diff --check`は`Assets/_Project/Scenes/Village.unity`内の6箇所(`m_Name: `/`value: `という空文字列フィールド)でtrailing whitespaceを検出するが、いずれもUnity Editor/Unity MCPの正規保存処理(直接編集なし)によって生成されるYAMLの仕様であり、Unity公式保存処理での再保存前後で全く同じ行番号・内容のまま変化しないことを確認した。`.cs`/`.meta`/`.prefab`/`.unity`/`.asset`のうちUnity生成ファイル種別(`*.meta`/`*.prefab`/`*.unity`/`*.asset`)を除外した`git diff --check -- . ":(exclude,glob)**/*.meta" ":(exclude,glob)**/*.prefab" ":(exclude,glob)**/*.unity" ":(exclude,glob)**/*.asset"`は0件(手書きファイルは問題なし)。Unity生成YAMLの空白は手動修正していない。
- **Minor: EditorSettings.assetの意図しない差分**: `m_EnterPlayModeOptions`が`0`→`1`になっていた差分を`git restore -- ProjectSettings/EditorSettings.asset`で復元した。ただし、本セッションでUnity Editorの`Play`操作を行うたびにUnity Editor自身が同ファイルへ同じ差分を再書き込みする挙動を確認した(Unity 6のEnter Play Mode Options機能に付随するEditor側の自動書き込みであり、本プロジェクトのコード変更によるものではない)。最終確認直前に再度`git restore`を実行し、差分なしの状態でこのセッションを終える(詳細は「Gitスコープ確認」参照)。
- **Minor: コメントの文字化け**: `PendingBattleContext.cs`/`BattleSceneInstaller.cs`/`FieldActivityGate.cs`/`BossEncounterTrigger.cs`の4ファイルをバイト単位で調査した結果、BOM無しの正しいUTF-8であり置換文字(U+FFFD)も存在しないことを確認した(`file`コマンド・16進ダンプ・`Read`ツール表示のいずれでも文字化けは再現せず)。実際の破損を再現できなかったが、レビューツール側のエンコーディング解釈に依存するリスクを排除するため、指摘の対象4ファイルの日本語コメントをすべてASCII英語コメントへ置き換えた(本番ロジックは無変更)。置き換え後、4ファイルは`file`コマンドで`ASCII text`と判定される。
- **新規・更新PlayModeテスト**: `BattleSceneInstallerTests`が16件(既存10件から、決定的化2件+失敗系新規5件+敗北系新規2件-旧Inconclusive依存1件などの差し引きで6件純増)。全体では10件純増(T-018〜T-020完了時点の152件→158件)。
- **T-018〜T-020(Codex対応込み)最終PlayModeテスト内訳**: T-018関連7件、T-019関連17件、T-020関連10件、Codex対応で追加・更新した`BattleSceneInstallerTests`関連6件純増、`FakeSceneLoader`へ`UnloadFailWith`追加(テスト専用インフラ、本番コード非該当)。
- **全体テスト実行結果(Codex対応後)**: 全EditMode 225件(変更なし)、全PlayMode 158件、Unity Editor Test Runnerで全件Passed(failed 0, skipped 0, inconclusive 0)を3回連続実行で確認。プロジェクトコード由来のConsole Error/Warning 0件、Missing Script/Broken Prefab 0件(正式6Scene全件`manage_scene validate`で確認)。
- **手動確認(Codex対応後、実機Play Mode)**: Village→Field実遷移、Field上でのAdditive Battle往復(2戦連続、状態破損なし)、Dungeon上でのボス戦勝利→GameClear(GAME CLEAR!表示)、ボス戦敗北→GameClear(GAME OVER表示)をすべて確認。アンロード失敗時の復旧は、実際のSceneManagerを意図的に破壊するリスクがあるため実機再現は行わず、`FakeSceneLoader.UnloadFailWith`による自動テスト(5件)を正式な確認方法とした。
- **手動確認中に観測した一過性の事象(本セッション)**: 複数のScene遷移トリガーへ人間の歩行では不可能な速さで連続テレポートさせた際、`SceneTransitionUseCase`の再入防止ガードが例外を送出し、それに伴いUnity Editorの「Error Pause」機能がPlay Modeを自動一時停止する事象を観測した。一時停止中は`Time.frameCount`が進行せず、この間の物理トリガー再判定も行われないため、一時停止解除後に想定外のAdditive Battle二重ロード(BattleUI・Camera・AudioListener・EventSystemの重複)が1回発生した。`manage_editor stop`でPlay Modeを終了し、Scene資産(`Dungeon.unity`等)を`manage_scene validate`で確認した結果は0件(Play Mode終了で変更は破棄され、資産は無傷)。落ち着いた操作間隔(1操作ごとにテレポート前後の状態を確認)で同じ手順を再実行したところ再現せず、正常に動作した。人間の通常の連続的な移動操作(WASD入力)ではこの手順自体が発生しないため、実プレイでは再現しないと判断した。

### T-019/T-020 Codex最終再レビュー残存Major対応(本セッションで対応完了)
- **問題**: 前回対応の`BattleSceneInstaller.DisableBattlePresentation`用の`_battleCamera`/`_battleAudioListener`/`_battleEventSystem`が、`Start()`内で`FindFirstObjectByType<Camera>()`/`<AudioListener>()`/`<EventSystem>()`というグローバル検索によって取得されていた。Additiveロード中はField/DungeonとBattleが同時に存在するため、たまたまField/Dungeon側が`FieldActivityGate.Pause()`で無効化されている間に限りBattle側を正しく拾えていたに過ぎず、取得範囲がBattle Sceneに保証されていなかった。
- **採用した修正方式**: `_battleCamera`/`_battleAudioListener`/`_battleEventSystem`を`[SerializeField] private`フィールドへ変更し、`Start()`内の`FindFirstObjectByType`呼び出しをすべて削除した。Battle Scene内の実オブジェクトへの参照はUnity MCP(`SerializedObject`経由、Scene YAML直接編集なし)で`Battle.unity`の`BattleSceneInstaller`コンポーネントへ明示的に設定した。グローバル検索・`FindAnyObjectByType`・復帰元Sceneを含む検索は一切行っていない。
- **参照不足時の挙動**: `ValidateBattlePresentationReferences()`を新設し、`Start()`内(既存の`BattleUIController`未検出チェックの直後)で3参照それぞれの`null`チェックを行い、不足しているものごとに`Debug.LogError`で対象フィールド名を含む明確なエラーを出す。参照が不足していても`NullReferenceException`は発生させず、グローバル検索による補完も行わない(`DisableBattlePresentation`側の既存null チェックがそのまま安全に働く)。
- **Field/Dungeon側を誤取得しない根拠**: `BattleSceneInstaller.cs`内に`Camera`/`AudioListener`/`EventSystem`型に対する`FindFirstObjectByType`系呼び出しが一切存在しないことをコードレビューで確認済み。加えて、Field側Camera/AudioListener/EventSystemとBattle側Camera/AudioListener/EventSystemを別々のGameObjectとして同時に配置した状態でアンロード失敗を発生させ、Battle側のみが無効化されFieldまたはDungeon側は`FieldActivityGate.Resume()`により最終的に有効化されることをテストで確認した。さらに、`FieldActivityGate`(＝Field/Dungeon側の目印)を一切配置せず、Battle側参照も未設定のまま、シーン内に無関係な`Camera`を1つだけ配置した状態でアンロード失敗を発生させても、その無関係な`Camera`が一切無効化されないことを確認するテストを追加した(グローバル検索が存在すれば唯一発見されうる対象がまさにこの無関係な`Camera`であるため、直接的な回帰テストとなっている)。
- **新規・更新PlayModeテスト**: `BattleSceneInstallerTests`が16件→20件(純増4件: Field/Dungeon双方でのBattle側限定無効化+復帰元側復旧の統合テスト2件、無関係なCameraが誤って無効化されないことを確認するテスト1件、参照不足時に安全にエラーを出すことを確認するテスト1件)。
- **全体テスト実行結果**: 全EditMode 225件(変更なし)、全PlayMode 162件、Unity Editor Test Runnerで全件Passed(failed 0, skipped 0, inconclusive 0)を3回連続実行で確認。プロジェクトコード由来のConsole Error/Warning 0件、Missing Script/Broken Prefab 0件(正式6Scene全件`manage_scene validate`で確認)。
- **手動確認**: Field通常戦勝利→Field復帰(Camera/AudioListener/EventSystem/PlayerMovement復旧、`_battleCamera`等がBattle Scene内の実オブジェクトへ正しく解決されていることを実行時に確認)、Dungeon通常戦勝利→Dungeon復帰、Dungeonボス戦勝利→GameClear(GAME CLEAR!)、ボス戦敗北→GameClear(GAME OVER)をすべて実機Play Modeで再確認済み。アンロード失敗時の復旧は、実際のSceneManagerを意図的に破壊するリスクがあるため実機再現は行わず、自動テスト(9件: 既存5件+新規4件)を正式な確認方法とした。
- **Codex再レビュー前の状態**。

### T-021〜T-024 正式仕様(本セッションで承認・追加)

ユーザー承認済みの正式Task定義。実装順は依存関係の都合によりTask番号順ではなく **T-022 → T-021 → T-023 → T-024** とする。T-022・T-021・T-023・T-024はすべて本セッションで完了・合格した(下記参照)。

#### T-021 メインクエスト進行・完了ロジック(完了)
- **目的**: Villageで開始したメインクエストを、Field、Dungeon、Boss戦を経由して完了できるようにする。
- **依存**: T-007, T-014, T-018, T-019, T-020。
- **完了条件**: Villageの指定NPCとの会話でメインクエストを開始できる。Field到達・Dungeon到達・Boss撃破をそれぞれ進行条件として記録できる。必要な段階を順番に通過した場合だけCompletedへ遷移できる。条件未達での完了を拒否する。進行のスキップ・逆行・重複完了を拒否する。Scene遷移後も進行状態を保持する。New Gameで初期化される。Continueで復元される。Retryで不正に巻き戻らない。クエスト完了後だけGameClearへ到達できる。条件未達のBoss勝利ではGameClearへ進まない。

**実装内容:**
- **`MainQuestStage`(新設、Domain）**: `NotStarted/ExploreField/EnterDungeon/DefeatBoss/Completed`の5値enum。
- **`MainQuestProgress`(新設、Domain）**: `CurrentStage`を保持する状態機械。`Start()`/`AdvanceToEnterDungeon()`/`AdvanceToDefeatBoss()`/`Complete()`はそれぞれ直前の段階からのみ遷移可能で、それ以外は`InvalidOperationException`を送出する(既存`QuestProgress`と同じ厳格な検証スタイル)。**既存の`QuestProgress`/`QuestState`は無変更**(SubQuest1/SubQuest2は引き続きQuestProgressを使用)。
- **`MainQuestEvent`/`MainQuestAdvanceResult`(新設、Application）**: `MainQuestEvent`は`FieldReached/DungeonReached/BossDefeated`の3値。`MainQuestAdvanceResult`は`Advanced/Rejected`の2値。
- **`StartMainQuestUseCase`/`AdvanceMainQuestUseCase`(新設、Application/Quests/）**: `AdvanceMainQuestUseCase.Execute(progress, event)`は現在の段階とイベントが一致する場合のみ進行し、一致しない場合は例外を投げず`Rejected`を返す(Field/Dungeonへの再訪問、クエスト未開始でのイベント到達、重複イベントをすべて安全なno-opにするため)。`progress`が`null`の場合のみ`ArgumentNullException`。
- **`PlayerSessionState.MainQuest`の型変更**: `QuestProgress`から`MainQuestProgress`へ変更(プロパティ名は維持)。`SubQuest1`/`SubQuest2`は`QuestProgress`のまま。
- **`NpcInteractable`**: `event Action DialogueStarted`を新設(会話が実際に開始した時点で発火)。既存の`LinkedQuest`/`QuestProgress`連動ロジックは変更せず維持(責務混在を避けるため、DialogueStartedはクエストロジックを一切持たない純粋なPresentationイベント)。
- **`VillageSceneInstaller`**: 全NPCへ`LinkedQuest`を一律設定していた旧ロジックを削除し、`[SerializeField] NpcInteractable _mainQuestGiver`(Inspector参照、Village.unityで`Elder`を指定)の`DialogueStarted`を購読して`StartMainQuestUseCase`を呼ぶだけの構成に変更。
- **`FieldSceneInstaller`/`DungeonSceneInstaller`**: `Start()`で(`CurrentSession`が存在する場合)それぞれ`MainQuestEvent.FieldReached`/`DungeonReached`を発行。到達した段階に一致しない場合は安全に無視されるため、再訪問やクエスト未開始でも例外・不整合は発生しない。
- **`BattleSceneInstaller`**: ボス勝利時に`MainQuestEvent.BossDefeated`を発行し、結果が`MainQuestStage.Completed`になった場合のみ`GameClear`へ遷移する。`CurrentSession`が存在しない場合や、メインクエストが`DefeatBoss`段階に達していない場合(条件未達)は、`Debug.LogError`で明確に記録した上でDungeonへ復帰する(既存の通常戦復帰経路`ReturnToFieldAsync`を再利用)。
- **セーブ拡張(SaveVersion 1→2)**: `SaveGameSnapshot`に`MainQuestStage`(新)を追加し、旧`MainQuestState`(QuestState)フィールドはv1読込専用として保持。`PlayerSessionStateMapper.FromSnapshot`はv1・v2両方を受け付け、v1の場合は`MainQuestState`(NotStarted/InProgress/Completed)を`MainQuestStage`(NotStarted/ExploreField/Completed)へ安全に移行する(v1のInProgressはどの段階か判別不能なため、安全な初期値として最も早い進行中段階`ExploreField`へ復元)。v1・v2以外のバージョンは引き続き`NotSupportedException`。
- **今回のスコープ外**: サブクエストのNPC紐付け(T-025)、クエストログUI。
- **テスト**: EditMode新規32件(`MainQuestProgressTests`10、`StartMainQuestUseCaseTests`5、`AdvanceMainQuestUseCaseTests`11、`PlayerSessionStateMapperTests`関連純増6[v1移行3件・不正値2件・複数段階往復1件、旧`FromSnapshot_InvalidQuestState`テストは`FromSnapshot_InvalidMainQuestStage`へ置換])。PlayMode新規12件(`NpcInteractableTests`+2、`VillageSceneInstallerTests`純増2[旧2件を新4件へ置換]、`FieldSceneInstallerTests`+3、`DungeonSceneInstallerTests`+3、`BattleSceneInstallerTests`+2)。
- **全体テスト結果**: 全EditMode 266件(234→266)、全PlayMode 184件(172→184)、いずれもfailed 0・skipped 0・inconclusive 0。PlayModeは3回連続実行しすべて184件Passedを確認。プロジェクトコード由来のConsole Error/Warning 0件(コンソールに残る内容はすべて意図したテストシミュレーションのエラー・例外ログ)。正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)すべて`manage_scene validate`でMissing Script 0・Broken Prefab 0を確認。
- **Scene変更**: `Village.unity`の`VillageSceneInstaller`へ`_mainQuestGiver`=`Elder`のNpcInteractableをUnity MCP経由で設定・保存(既存の会話テキストはそのまま流用)。

#### T-022 実MasterData Asset作成(完了)
- **目的**: Composition層の仮固定値を、T-012で定義したMasterData Asset参照へ置き換える。
- **依存**: T-012。
- **最低限作成するデータ**: 通常敵3種類、ボス1種類、回復系消費アイテム2種類、武器2種類、防具2種類、メインクエスト定義1件、プレイヤー初期データ1件、既存ExperienceTableと整合する成長データ。
- **完了条件**: AssetはUnity MCPまたはUnity Editor正規APIで作成する。IDが一意。必須値が設定済み。数値範囲が正常。MasterDataValidatorを通過する。通常敵、ボス、初期プレイヤーの固定値をAsset参照へ置換する。実行時状態をScriptableObjectへ書き戻さない。参照不足時に隠れたフォールバックを行わない。参照不足時は明確なエラーを出して安全に停止する。

**実装内容:**
- **`EnemyMasterData`/`EnemyDefinition`拡張**: 既存8引数コンストラクタへ`RewardExperience`(int、0以上)を追加し9引数化(T-012の既存型を最小拡張、重複型を作らない方針)。`EnemyDefinition`(ScriptableObject)に`_rewardExperience`フィールドを追加し`ToMasterData()`へ反映。
- **`InitialPlayerDefinition`(新設、Infrastructure)**: `Assets/_Project/Runtime/Infrastructure/MasterData/InitialPlayerDefinition.cs`。`StatGrowthProfile`(T-004)の14フィールド相当を`[SerializeField]`で保持し、`ToGrowthProfile()`と`ToInitialCharacterStats()`(`CharacterStatsCalculator.Calculate`をMinLevelで呼び出すのみ、計算式は再実装しない)を公開する。「初期プレイヤーデータ」と「成長データ」を1Assetで兼ねる設計とし、別途の`StatGrowthProfileDefinition`は作成していない(推奨Asset名一覧が`InitialPlayerDefinition`1件のみだったため)。
- **`QuestMasterData`(新設、Domain)/`QuestDefinition`(新設、Infrastructure)**: メインクエストの静的定義(Id・DisplayNameのみ、状態機械は持たない)。既存`QuestState`/`QuestProgress`(T-007)とは責務を分離し、後続T-021のクエスト進行ロジックには依存しない。
- **`BattleSceneInstaller`**: 旧来の固定`CharacterStats`定数3つ(`FallbackPlayerStats`/`RegularEncounterEnemyStats`/`BossEncounterEnemyStats`)を削除し、`[SerializeField] InitialPlayerDefinition _fallbackPlayerDefinition`、`[SerializeField] EnemyDefinition[] _regularEnemies`(3体)、`[SerializeField] EnemyDefinition _bossEnemy`のInspector参照に置換した。`ValidateMasterDataReferences()`を新設し、参照不足時は対象ごとに`Debug.LogError`を出して`Start()`を安全に中断する(隠れたフォールバックなし)。通常敵は`internal static EnemyDefinition PickRegularEnemy(EnemyDefinition[] candidates, double roll)`(決定的・単体テスト可能)で`IRandomSource`から一様ランダムに選択する。選択結果は`_currentEnemyMasterData`として保持し、T-023の報酬付与で再利用する(T-022時点では未使用)。敵の`Level`はMasterDataに存在しないため固定値`1`をCharacterStats生成時に使用する(戦闘ロジック上Levelは参照されないため実害なし)。
- **`TitleSceneInstaller`**: 固定`CharacterStats`定数`NewGameStats`を削除し、`[SerializeField] InitialPlayerDefinition _initialPlayerDefinition`に置換。参照不足時は`Debug.LogError`を出し`OnNewGameRequested`を安全に中断する。
- **参照不足時の安全停止**: 上記2箇所とも、参照未設定時は明確なエラーメッセージ(対象フィールド名を含む)を出力し、セッション生成やBattleSession構築を行わずに処理を中断する。
- **作成したMasterData Asset(12件、Unity MCP経由で作成)**:
  - 敵(`Assets/_Project/ScriptableObjects/Enemies/`): `Slime`(HP12/Atk4/Def1/Agi3, 経験値6)、`Wolf`(HP16/Atk6/Def2/Agi6, 経験値9)、`Golem`(HP24/Atk7/Def5/Agi1, 経験値12)、`IslandGuardian`(ボス、HP60/MP10/Atk10/Def6/Agi4/Mag3, 経験値50)
  - アイテム(`Assets/_Project/ScriptableObjects/Items/`): `SmallPotion`(回復20)、`LargePotion`(回復50)
  - 装備(`Assets/_Project/ScriptableObjects/Equipment/`): `RustySword`(Weapon, Atk+3)、`SkyBlade`(Weapon, Atk+8)、`TravelerArmor`(Armor, Def+3)、`GuardianArmor`(Armor, Def+8)
  - クエスト定義(`Assets/_Project/ScriptableObjects/Quests/`): `RestoreTheFloatingIslands`
  - プレイヤー初期データ(`Assets/_Project/ScriptableObjects/Player/`): `InitialPlayerDefinition`(MinLevel1/MaxLevel10、基礎HP20/MP5/Atk5/Def3/Agi5/Mag2、成長HP+4/MP+1/Atk+2/Def+1/Agi+1/Mag+1)。MaxLevel=10は今後T-023で作成するExperienceTableのMaxLevelと一致させる(整合性はT-023側でテストする)。
- **Scene変更**: `Battle.unity`の`BattleSceneInstaller`へ`_fallbackPlayerDefinition`=`InitialPlayerDefinition`、`_regularEnemies`=[`Slime`,`Wolf`,`Golem`]、`_bossEnemy`=`IslandGuardian`をUnity MCP経由で設定・保存。`Title.unity`の`TitleSceneInstaller`へ`_initialPlayerDefinition`=`InitialPlayerDefinition`をUnity MCP経由で設定・保存。Scene構造・Prefab自体は変更していない。
- **テスト**: EditMode新規9件(`EnemyMasterDataTests`+2、`QuestMasterDataTests`新規7)、PlayMode新規10件(`BattleSceneInstallerTests`+6[`PickRegularEnemy`境界値4件、参照不足時中断1件、複数候補時の妥当性1件]、`TitleSceneInstallerTests`+1、`InitialPlayerDefinitionTests`新規3)。
- **全体テスト結果**: 全EditMode 234件(225→234)、全PlayMode 172件(162→172)、いずれもfailed 0・skipped 0・inconclusive 0。PlayModeは3回連続実行しすべて172件Passedを確認。プロジェクトコード由来のConsole Error/Warning 0件(コンソールに残る内容はすべてテスト自身が`LogAssert.Expect`で捕捉する意図的なシミュレーション例外・エラーメッセージ)。正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)すべて`manage_scene validate`でMissing Script 0・Broken Prefab 0を確認。
- **Console/手動確認時の注記**: MCPブリッジ自身のWebSocketトランスポートに関する警告(`[WebSocket] Unexpected receive error`)を1件観測したが、MCPForUnityパッケージ内部の通信基盤のログでありプロジェクトコード由来ではないと判断した。

#### T-023 戦闘報酬・経験値・レベルアップ統合(完了)
- **目的**: T-006のExperienceTableとLevelUpCalculatorを実際の戦闘結果へ接続する。
- **依存**: T-006, T-022。
- **完了条件**: Enemy MasterDataから獲得経験値を取得する。通常戦勝利後・Boss勝利後に経験値を加算する。敗北時は経験値を付与しない。レベルアップ条件を満たした場合にレベルを更新し、CharacterStatsを再計算する。最大レベルを超えない。経験値オーバーフローを安全に扱う。獲得経験値・レベルアップ有無・新レベルを最低限表示する。Retryやイベント多重発火で報酬を重複取得できない。Scene復帰後も状態を保持する。Continue後もレベルと経験値を復元する。

**実装内容:**
- **`InitialPlayerDefinition`拡張**: `_cumulativeExperienceByLevel`(int配列)を追加し`ToExperienceTable()`で`ExperienceTable`(T-006)を生成できるようにした。プレイヤーの成長データ・初期データ・経験値テーブルを1Assetに集約する既存方針(T-022)を維持し、新規Asset種別は追加していない。実Asset(`InitialPlayerDefinition.asset`)へ`[0,10,25,45,70,100,140,190,250,320]`(Lv1〜10)を設定。
- **`PlayerSessionState.ApplyStatGrowth(CharacterStats newStats)`(新設）**: レベルアップ後の`CharacterStats`を適用する唯一の手段。`newStats.Level`が現在のLevel未満の場合は`ArgumentOutOfRangeException`(退行を拒否)。**レベルアップ時のHP/MP回復方針: 全回復する**(`CurrentHp = newStats.MaxHp`, `CurrentMp = newStats.MaxMp`)。複雑な部分回復・按分方式は採用していない(推測による仕様追加を避けるため、最も単純で明確な「全回復」を採用)。
- **`BattleRewardResult`/`GrantBattleRewardUseCase`(新設、Application/Progression/）**: `Execute(session, experienceTable, growthProfile, rewardExperience)`が(1)`session.GainExperience`で加算(既存`checked`ブロックによりオーバーフローは`OverflowException`として安全に検出、黙って握り潰さない)、(2)`LevelUpCalculator.CalculateLevel`(T-006)で新レベルを算出(最大レベルで自動的に頭打ち)、(3)レベルが実際に上がった場合のみ`CharacterStatsCalculator.Calculate`(T-004)で再計算し`ApplyStatGrowth`を適用。計算式はすべて既存Domain型を再利用し重複実装していない。
- **`BattleUIController.ShowReward(int, bool, int)`(新設）**: 獲得経験値・レベルアップ有無・新レベルを戦闘ログへ追記するだけの最小表示(アニメーション・専用UIパネルなし)。
- **`BattleSceneInstaller`統合**: `OnBattleEnded`の先頭で`PlayerVictory`の場合のみ`GrantRewardOnce()`を呼び出す。`_rewardGranted`(bool)フィールドで同一バトルインスタンス内の多重発火を防止する(Retryは新しいBattleSceneInstallerインスタンスを生成するため、Retryごとに1回だけ報酬が入る)。報酬対象の敵は`_currentEnemyMasterData`(T-022で解決済み)、成長データ・経験値テーブルは`_fallbackPlayerDefinition`(T-022のInspector参照、CurrentSessionの有無によらず唯一の成長曲線として使用)から取得する。`CurrentSession`が存在しない場合は安全に何もしない。
- **セーブ**: Level/TotalExperienceは既存のSaveVersion 1由来フィールド(T-010/T-011)でそのまま永続化されるため、**新たなSaveVersion変更は不要**(T-023はセーブスキーマに変更を加えていない)。
- **今回のスコープ外**: 部分回復・按分回復等の複雑なHP/MP回復仕様、演出・アニメーション、レベルアップ時のスキル習得。
- **テスト**: EditMode新規18件(`GrantBattleRewardUseCaseTests`14、`PlayerSessionStateTests`の`ApplyStatGrowth`関連4)。PlayMode新規11件(`BattleUIControllerTests`+3[`ShowReward`表示]、`BattleSceneInstallerTests`+6[通常戦報酬・Boss報酬・敗北時無報酬・多重発火防止・レベルアップ・CurrentSessionなし]、`InitialPlayerDefinitionTests`+2[経験値テーブル生成・成長データとのMaxLevel整合])。
- **全体テスト結果**: 全EditMode 284件(266→284)、全PlayMode 195件(184→195)、いずれもfailed 0・skipped 0・inconclusive 0。PlayModeは3回連続実行しすべて195件Passedを確認。プロジェクトコード由来のConsole Error/Warning 0件。Battle.unity/GameClear.unityを含む正式Sceneで`manage_scene validate`によりMissing Script 0・Broken Prefab 0を確認。

#### T-024 アイテム・装備・所持品管理(完了)
- **目的**: T-012とT-022のアイテム、武器、防具データを使用し、所持品と装備状態を管理できるようにする。
- **依存**: T-012, T-022, T-023。
- **完了条件**: ItemId単位で数量を保持できる。アイテムを追加・消費できる。数量不足・数量0未満・存在しないIDを拒否する。武器と防具を装備・解除できる。装備カテゴリ不一致を拒否する。所持していない装備を拒否する。装備補正をCharacterStatsへ反映し、二重加算しない。同一報酬の重複取得を防止する。Sceneをまたいで状態を保持する。New Gameで初期化される。Continueで復元される。Retryでアイテムや装備が不正に増減しない。最低限の取得・使用・装備・確認経路が存在する。
- **今回実装しないもの**(T-026以降へ残す): 本格的なメニュー画面、ショップ、クラフト、装備耐久度、ランダムオプション、アイテムソート、ドラッグアンドドロップ、複雑な装備比較UI。

**実装内容:**
- **`Inventory`(新設、Domain/Inventory/）**: `Dictionary<string,int>`をprivate保持し、`Add`/`Consume`/`GetQuantity`を提供。`Quantities`は毎回防御的コピーを返す。数量0未満・ID不正・数量不足・未所持消費をすべて例外で拒否する。重複ItemIdは同一エントリへ自然に統合される(Dictionaryキー特性による)。復元用コンストラクタ`Inventory(IReadOnlyDictionary<string,int>)`も防御的コピーを行う。
- **`EquipmentLoadout`(新設、Domain/Inventory/）**: `EquippedWeaponId`/`EquippedArmorId`(nullable)を保持。`EquipWeapon`/`EquipArmor`/`UnequipWeapon`/`UnequipArmor`のみを公開し、カテゴリ・所持チェックは持たない(Application層の責務)。
- **`EquipmentStatCalculator`(新設、Domain/Inventory/）**: `ApplyBonus(baseStats, weapon, armor)`が毎回`baseStats`から新しい`CharacterStats`を計算して返す(状態を保持しない純粋関数のため二重加算が構造的に発生しない)。
- **`AddItemUseCase`/`ConsumeItemUseCase`/`EquipItemUseCase`/`UnequipItemUseCase`(新設、Application/Inventory/）**: それぞれ`AddItemResult`/`ConsumeItemResult`/`EquipItemResult`(Advanced/Rejected方式と同じ列挙型パターン)を返す。`ConsumeItemUseCase`はItemMasterDataのHealAmount分`PlayerSessionState.SetCurrentHp`をMaxHpにクランプして呼ぶ。`EquipItemUseCase`はMasterDataの`Slot`と要求`EquipmentSlot`が一致し、かつ`Inventory`に1個以上所持している場合のみ装備する。
- **`PlayerSessionState`拡張**: `Inventory`/`Equipment`プロパティ(コンストラクタでオーナーとして生成、既存8引数コンストラクタは無変更のため既存呼び出し元は無修正でコンパイル可能)。`ClaimedRewardIds`(`HashSet<string>`)と`ClaimReward(string)`/`HasClaimedReward(string)`を追加し、一度きり報酬の重複取得防止に使用する。`BattleSceneInstaller.BuildRematchSnapshot`はInventory/Equipment/ClaimedRewardIdsを(既存のMainQuest/SubQuestと同様に)参照渡しでRetry用スナップショットへ引き継ぐ。
- **セーブ拡張(SaveVersion 2→3)**: `SaveGameSnapshot`に`InventoryEntries`(`ItemId`/`Quantity`の構造体配列。JsonUtilityがDictionaryを直接シリアライズできないため配列化)、`EquippedWeaponId`/`EquippedArmorId`、`ClaimedRewardIds`(string配列)を追加。`PlayerSessionStateMapper.FromSnapshot`はv3未満のセーブに対しては空のInventory/未装備/未請求として安全に復元する。配列内の重複ItemIdは「最後の値が勝つ」方針で明確化(Dictionary構築時に上書き)。
- **戦闘統合(BattleSceneInstaller)**: `_equipmentCatalog`(Inspector参照、任意)から装備補正込みの`playerStats`を都度計算してから`BattleSession`を構築する(補正はsession.Statsへ書き戻さない)。`_victoryItemReward`(Inspector参照、任意)を勝利のたびに1個付与する(通常戦・Boss戦問わず、`GrantRewardOnce`の重複防止ガードを共有)。
- **最低限のゲーム内経路**:
  - **取得**: `ItemPickupTrigger`(新設、Presentation/Items/、Field.unityに1箇所配置)がプレイヤー接触で発火し、`FieldSceneInstaller`が`PlayerSessionState.ClaimReward`で重複防止した上でRustySwordを1個付与する。Presentation層はMasterData/Infrastructureを一切参照しない設計(`_pickupItem`/`_pickupEquipment`はComposition層の`FieldSceneInstaller`が保持)。
  - **戦闘報酬**: 勝利のたびにSmallPotionを1個付与(T-023の経験値付与と同じ`GrantRewardOnce`ガードで多重発火を防止)。
  - **使用・装備・確認**: `InventoryPanelController`(新設、Presentation/Items/、Village.unityに1箇所配置)が所持数・装備中アイテム名を表示し、「Use Potion」「Equip Weapon」「Equip Armor」の3ボタンを提供する。ボタンは純粋にイベントを発火するのみで、実際の消費・装備判定は`VillageSceneInstaller`(Composition)が`ConsumeItemUseCase`/`EquipItemUseCase`を呼んで行う(所持している中で未装備の最初の候補を装備する、という最小限のロジック)。
- **今回のスコープ外**: 本格的なメニュー画面、ショップ、クラフト、装備耐久度、ランダムオプション、アイテムソート、ドラッグアンドドロップ、複雑な装備比較UI(T-026へ持ち越し)。
- **テスト**: EditMode新規77件(`InventoryTests`16、`EquipmentLoadoutTests`11、`EquipmentStatCalculatorTests`8、`AddItemUseCaseTests`4、`ConsumeItemUseCaseTests`5、`EquipItemUseCaseTests`7、`UnequipItemUseCaseTests`4、`PlayerSessionStateTests`関連新規、`PlayerSessionStateMapperTests`関連新規、他)。PlayMode新規13件(`ItemPickupTriggerTests`5、`InventoryPanelControllerTests`5、`BattleSceneInstallerTests`+2[装備補正適用/未適用]、`VillageSceneInstallerTests`+6[Use Potion/Equip Weapon/Equip Armor等]の一部重複を除く純増分)。
- **全体テスト結果**: 全EditMode 361件(284→361)、全PlayMode 214件(195→214)、いずれもfailed 0・skipped 0・inconclusive 0。PlayModeは3回連続実行しすべて214件Passedを確認。プロジェクトコード由来のConsole Error/Warning 0件。正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)すべて`manage_scene validate`でMissing Script 0・Broken Prefab 0を確認。
- **Scene変更**: `Field.unity`に`FieldItemPickup`(`ItemPickupTrigger`、`SphereCollider`トリガー)を新規配置し`FieldSceneInstaller`へ配線。`Village.unity`に`InventoryPanel`(Canvas + StatusText + 3ボタン)を新規配置し`VillageSceneInstaller`へ配線。`Battle.unity`の`BattleSceneInstaller`へ`_equipmentCatalog`(武器2+防具2)と`_victoryItemReward`(SmallPotion)をUnity MCP経由で設定。いずれもUnity MCP(`manage_gameobject`/`manage_components`)経由で作成・設定し、Scene YAMLを直接編集していない。
- **手動確認(実機Play Mode、Unity MCP経由)**: New Game→Elder会話でメインクエスト開始(NotStarted→ExploreField)→Field到達で進行(→EnterDungeon)→Fieldでアイテム(RustySword)を1回取得(2回目は重複取得されないことを確認)→通常戦勝利でEXP+アイテム(SmallPotion)獲得(戦闘ログに"EXP +6"表示を確認)→Village Inventoryパネルで所持数表示を確認→Equip Weaponボタンで武器装備→Use Potionボタンでポーション消費を確認→Dungeon到達で進行(→DefeatBoss)→ボス撃退でEXP+50・レベルアップ(Lv.1→4、戦闘ログに"Level Up! Lv.4"表示)→MainQuest Completed→GameClear(GAME CLEAR!パネル表示)を確認→実際の`SaveGameUseCase`/`LoadGameUseCase`でSave→Load往復を実行しLevel/MainQuest/Inventory/Equipment/ClaimedRewardIdsすべて正しく復元されることを確認→Title→Continueで実際にVillageへ復元されLevel4・MainQuest Completed・所持品・装備がすべて維持されることを確認。すべてConsole Error/Warning 0件。
- **既知の問題(T-021〜T-024範囲外、既存の未実装)**: プレイヤーが任意タイミングでセーブを行うための実際のUI(セーブボタン等)がゲーム内のどのSceneにも存在しない(`SaveGameUseCase`自体はT-010/T-011で実装済みで、本セッションでも直接呼び出しによる動作は確認済み)。この経路の追加はT-021〜T-024のいずれの完了条件にも含まれないため本セッションでは対応していない。

### T-021〜T-024 Codex第三者レビュー指摘対応(Major3件・Minor1件、本セッションで対応完了)

Codex第三者レビュー結果: Critical 0件、Major 3件、Minor 1件、全体判定「不合格」。以下、指摘範囲のみを必要最小限で修正した。T-021〜T-024の既存実装(Domain/Application/Infrastructure/Presentation)は作り直していない。T-025以降には着手していない。

#### Major 1: 戦闘終了処理の多重実行防止(`BattleSceneInstaller.cs`)
- **指摘**: 報酬付与には`_rewardGranted`ガードがあったが、`OnBattleEnded`全体(LastBattleOutcome更新、MainQuest進行、ReturnToField、GameClear遷移、PendingBattle消費等)にはガードがなく、2回目の呼び出しで誤って再実行される可能性があった。
- **修正**: `OnBattleEnded`冒頭に`_battleEndHandled`(新設フィールド)のチェックを追加し、`true`なら即`return`、`false`なら直ちに`true`へ変更してから処理を続ける方式とした。非同期処理(`await`)が始まる前、同期的に設定するため、2回目の呼び出しが同一フレーム内で発生しても確実にガードされる。`_rewardGranted`とは責務を分離して維持(報酬専用ガードはそのまま残す)。新しいBattle Scene初期化のたびに新しい`BattleSceneInstaller`インスタンスが生成されるため、`_battleEndHandled`は明示的なリセット処理なしで各戦闘ごとにfalseから始まる。グローバルなstatic状態・public Singletonは追加していない。
- **テスト**: `BattleSceneInstallerTests`に6件追加(`BattleEnded_InvokedTwice_BossVictory_GameClearTransitionRequestedOnce`、`BattleEnded_InvokedTwice_SecondOutcomeDoesNotOverwriteLastBattleOutcome`、`BattleEnded_InvokedTwice_RegularVictory_ReturnToFieldRequestedOnce`、`BattleEnded_InvokedTwice_MainQuestAdvancedOnce`、`BattleEnded_InvokedTwice_PendingBattleConsumedOnce`、`BattleEnded_NewInstallerInstance_HandlesOwnBattleEndIndependently`)。既存の`BattleEnded_InvokedTwice_DoesNotGrantRewardTwice`は変更・弱体化していない。`FakeSceneLoader`に`UnloadCallCount`と`LoadCalls`(呼び出し順序記録用リスト)を追加し、遷移が意図した回数・順序で発生したことを検証できるようにした。

#### Major 2: Retry時のPendingBattleContext復元(`GameServices.cs`, `BattleSceneInstaller.cs`, `GameClearSceneInstaller.cs`)
- **指摘**: RetryはPlayerSessionStateを`RematchSnapshot`から復元するが、`PendingBattleContext`(通常戦かBoss戦か、復帰先SceneId)を復元しておらず、敗北後にRetryすると通常戦勝利がGameClear扱いになる等、元の戦闘種別と復帰先が失われる問題があった。
- **修正**: `GameServices`に`RematchPendingBattle`プロパティ(新設)を追加。`BattleSceneInstaller.Start()`が`RematchSnapshot`を構築する箇所と同じタイミングで、`_services.PendingBattle`の防御的コピー(新しい`PendingBattleContext`インスタンス)を`RematchPendingBattle`へ保存する。`GameClearSceneInstaller.OnRetryRequested()`は`RematchPendingBattle`が存在する場合、まず`pendingBattle.ReturnSceneId`(Field/Dungeon)をSingleモードで読み込み直し、次に`_services.PendingBattle`へ防御的コピーを設定してからBattleをAdditiveモードで読み込む、という元のエンカウント開始フロー(`FieldSceneInstaller`/`DungeonSceneInstaller`の`StartEncounterAsync`と同じ手順)を再現する。`RematchPendingBattle`が存在しない場合(Battleへ直接入った等)は、従来通りBattleへSingleモードで直接遷移するフォールバックを維持する。PendingBattleは永続Saveデータ(`SaveGameSnapshot`)へは一切追加しておらず、`RematchSnapshot`と対になる一時的なComposition内状態としてのみ扱う。既存のRetry・RematchSnapshot・BattleSceneInstaller・GameClearSceneInstallerの設計をそのまま再利用し、別の重複Retryシステムは作っていない。static可変状態は追加していない。
- **テスト**: `GameClearSceneInstallerTests`に6件追加(通常Field戦Retry、通常Dungeon戦Retry、Boss戦Retry、PendingBattleなしのフォールバック、防御的コピーの参照非同一性、Boss戦Retry失敗時のボタン復旧)。

#### Major 3: SaveVersion 3ロード時の整合性検証(`PlayerSessionStateMapper.cs`, `LoadGameUseCase.cs`, `JsonSaveRepository.cs`, `TitleSceneInstaller.cs`)
- **指摘**: SaveVersion 3のロード時、EquippedWeaponId/EquippedArmorIdの実在性・カテゴリ一致、LevelとTotalExperienceの整合性が検証されておらず、不正なSnapshot(存在しない装備ID、カテゴリ不一致、Level/TotalExperience不整合等)がそのまま復元される可能性があった。
- **修正方針**: 「不正Saveはロード失敗として扱う」方式を採用した(安全な初期状態へのフォールバックではなく、既存の`LoadGameUseCase`/`JsonSaveRepository`が持つ「失敗→バックアップへフォールバック、両方失敗ならロード失敗」という既存の失敗処理経路にそのまま合流させる設計)。
  - `PlayerSessionStateMapper.FromSnapshot`に`ExperienceTable experienceTable = null`と`IReadOnlyDictionary<string, EquipmentMasterData> equipmentCatalog = null`という2つの省略可能引数を追加(T-024の`PlayerSessionState`拡張と同じ「オプション引数で破壊的変更を避ける」方針)。両方Domain層の既存型(`ExperienceTable`はT-006、`EquipmentMasterData`はT-012)を再利用しており、Application層にInfrastructure/UnityEngine依存は一切持ち込んでいない。`SaveVersion >= 3`の場合のみ検証を実行し、v1/v2には適用しない。
  - Level/TotalExperience整合性: `experienceTable`が渡された場合のみ、`LevelUpCalculator.CalculateLevel`(T-006で作成済み)で算出した期待Levelと`snapshot.Level`を比較し、不一致なら`ArgumentException`。負のTotalExperienceも同様に拒否。最大レベル時の余剰経験値は`CalculateLevel`が元々許容する仕様のため、正しい境界値を誤って拒否しない。
  - 装備ID整合性: `equipmentCatalog`が渡された場合のみ、EquippedWeaponId/EquippedArmorIdが空文字列・nullなら常に「未装備」として許可し、そうでなければカタログに実在するか・スロットが一致するかを検証し、不一致なら`ArgumentException`。Inventory内所持数と装備の整合性は、既存仕様に存在しないため新規制約として追加していない。
  - `LoadGameUseCase`に`ExperienceTable`/`EquipmentCatalog`という2つの省略可能プロパティ(デフォルトnull)を追加し、`Load()`内で`FromSnapshot`へ渡す。コンストラクタは変更していないため、既存の全呼び出し元(`GameServices`、各種テスト)は無修正でコンパイル・動作する。
  - `JsonSaveRepository`にも同名の2プロパティを追加し、`IsRestorable`(プライマリ/バックアップ双方の妥当性判定に使用される既存メソッド)を通じて同じ検証を適用した。これにより「プライマリが新v3検証に違反 → 自動的に正常なバックアップへフォールバック」という要求どおりの挙動を、既存のバックアップ機構を変更せずに実現している。
  - `TitleSceneInstaller`(実際にContinueで`LoadGameUseCase.Load()`が呼ばれる唯一の経路)に`_equipmentCatalog`(`EquipmentDefinition[]`、Inspector参照、任意)フィールドを新設し、`Start()`で`_initialPlayerDefinition.ToExperienceTable()`と装備カタログ辞書を構築して、既存の`_services.LoadGameUseCase`/`_services.SaveRepository`(実体が`JsonSaveRepository`の場合のみ)へ**再構築せずプロパティ設定のみで**注入する。これにより、テストが独自に差し替えたリポジトリ/LoadGameUseCaseインスタンスを上書きすることなく、実ゲームプレイ時のみ実MasterDataによる検証が有効になる。Title.unityの`TitleSceneInstaller`へ`_equipmentCatalog`(RustySword/SkyBlade/TravelerArmor/GuardianArmor、既存4Asset)をUnity MCP経由で配線した。
- **テスト**: `PlayerSessionStateMapperTests`に17件追加(正常なLevel/TotalExperience、Level1境界、最大レベル境界の余剰経験値、Level/TotalExperience不整合の拒否、負のTotalExperienceの拒否、ExperienceTable未指定時のスキップ、既知Weapon/Armor IDの復元、未知Weapon/Armor IDの拒否、スロットカテゴリ不一致[両方向]の拒否、null/空装備IDが常に許可されること、EquipmentCatalog未指定時のスキップ、v1/v2への新検証の不適用[2件]、両カタログ指定時のInventory/Equipment/ClaimedRewardIds復元の非退行確認)。`JsonSaveRepositoryTests`に4件追加(ExperienceTable設定時のLevel不整合バックアップフォールバック、ExperienceTable未設定時の非フォールバック確認、EquipmentCatalog設定時の未知ID バックアップフォールバック、EquipmentCatalog未設定時の非フォールバック確認)。`TitleSceneInstallerTests`に5件追加(正常Snapshot、Level/TotalExperience不整合、未知装備ID、スロットカテゴリ不一致、正常装備ID)。既存の`TitleSceneInstallerTests`の共有`BuildScene`フィクスチャに`_cumulativeExperienceByLevel`を追加(`Start()`が新たに`ToExperienceTable()`を呼ぶようになったため、未設定のままだと`ArgumentNullException`になる既存フィクスチャの不足を補った)。

#### Minor: 不正なEquipmentSlotの拒否(`UnequipItemUseCase.cs`)
- **指摘**: `if (slot == EquipmentSlot.Weapon) ... else ...`という二択の条件分岐のため、`(EquipmentSlot)999`のような未定義値がすべてArmor解除として処理されていた。
- **修正**: `Enum.IsDefined(typeof(EquipmentSlot), slot)`による検証を追加し、未定義値は`ArgumentOutOfRangeException`で明確に拒否するようにした(`default`でArmorへ流さない)。既存の正常系(Weapon/Armor)の挙動は変更していない。`EquipItemUseCase`の同様の二択分岐も確認したが、その手前で`equipmentData.Slot != targetSlot`の一致チェックが必ず先に走るため、未定義値は既にこのチェックで`SlotMismatch`として安全に拒否されており、追加修正は不要と判断した(指摘範囲を超える変更は行っていない)。
- **テスト**: `UnequipItemUseCaseTests`に2件追加(`(EquipmentSlot)999`が`ArgumentOutOfRangeException`を投げること、その際にLoadout状態が変化しないこと)。既存の`Execute_WeaponSlot_UnequipsWeaponOnly`/`Execute_ArmorSlot_UnequipsArmorOnly`は必須テスト項目の「Weapon解除」「Armor解除」を既に満たしている。

#### テスト結果(本セッションで実測)
- 追加テスト件数: EditMode新規19件(Minor 2件、Major3 17件)、PlayMode新規21件(Major1 6件、Major2 6件、Major3 TitleSceneInstaller 5件・JsonSaveRepository 4件)。
- 全EditMode: 380件Passed(361→380)、failed 0・skipped 0・inconclusive 0。
- 全PlayMode: 235件Passed(214→235)、failed 0・skipped 0・inconclusive 0。3回連続実行しすべて235件Passedを確認。
- Console: プロジェクトコード由来のError/Warning 0件(自動テスト実行時に記録されたログはすべて既存の`LogAssert.Expect`対象[意図的なシミュレーション失敗ログ]のみ)。
- 正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)すべて`manage_scene validate`でMissing Script 0・Missing Reference 0・Broken Prefab 0を確認。
- **手動確認(実機Play Mode、Unity MCP `execute_code`経由)**: New Game→Elder会話でメインクエスト開始→Field到達→通常戦闘をField上でAdditive開始→敗北→GameClear(Game Over)遷移→`RematchPendingBattle`にReturnSceneId=Field・IsBossEncounter=falseが保存されていることを確認→Retry実行→Field(Single)→Battle(Additive)の順で遷移することを確認→勝利イベントを2回発火→Scene遷移は1回のみ(Fieldへ復帰、GameClearへは進まない)・報酬(経験値)は1回のみ加算されることを確認。続けてBoss戦(MainQuest DefeatBoss状態、Dungeon想定)で同様に敗北→Retry→`RematchPendingBattle`にReturnSceneId=Dungeon・IsBossEncounter=trueが保存されており、Retry後実際にDungeonが(Fieldでなく)読み込まれることを確認→勝利イベントを2回発火→GameClearへ1回だけ遷移・MainQuestがCompletedになること・報酬(経験値)が1回のみ加算されることを確認。Save/Load整合性については、実際の`SaveGameUseCase`/`LoadGameUseCase`(Title.unityで実配線された実ExperienceTable/実装備カタログ使用)で、正常なセーブの往復successを確認した上で、プライマリ保存ファイルを直接Level/TotalExperience不整合な内容へ書き換え→`LoadGameUseCase.Load()`が正常なバックアップへ自動フォールバックすること、未知の装備IDへ書き換えた場合も同様にバックアップへフォールバックすることを確認。手動確認セッション中、Unity組み込みの「2 event systems」等の警告が記録されたが、これは`execute_code`による手動シミュレーションが`FieldActivityGate.Pause()`(通常の`FieldSceneInstaller`/`DungeonSceneInstaller`が実際のエンカウント開始時に必ず呼ぶ手順)を経由せず直接Scene遷移APIを呼び出したことによるテスト手法上のアーティファクトであり、プロジェクトコード由来のエラー・警告ではないことを、同一シナリオを正規のフローで実行する自動化PlayMode テスト(3回連続235件Passed、Console Error/Warning 0件)で別途確認済み。
- **未解決事項**: なし(T-021〜T-024範囲内のCodex指摘はすべて対応完了)。T-024完了時点で記載した「ゲーム内にSave実行用UIが存在しない」という既知の問題は本セッションのスコープ外のまま変わらず未対応。
- 本対応はCodex第三者再レビュー前の状態である。

### T-025 サブクエスト2本の実装(完了)

PROJECT.md表8.実装タスク一覧の既存定義(「サブクエスト2本のトリガー・進行・完了」「メインクエストと独立に受注・完了できる」)をそのまま正式仕様として実装した。`feature/t025-t028-release-slice`ブランチ(`origin/main`のPR #8マージ済みコミット`269c360`から新規作成)で作業。

**実装内容:**
- **Domain/Application新設なし**: T-010ですでに存在する`QuestProgress`/`QuestState`(NotStarted/InProgress/Completed)と、`PlayerSessionState.SubQuest1`/`SubQuest2`プロパティをそのまま再利用した(サブクエスト用の新しいDomain型は作成していない)。
- **`SubQuestAdvanceResult`(新設、Application/Quests/）**: `MainQuestAdvanceResult`と同型の`{Advanced, Rejected}`。
- **`StartSubQuestUseCase`/`CompleteSubQuestUseCase`(新設、Application/Quests/）**: `StartMainQuestUseCase`/`AdvanceMainQuestUseCase`と同じ「不正な呼び出し順序は例外でなくRejectedで安全に無視する」パターンを踏襲。`StartSubQuestUseCase`は`NotStarted`のときのみ`Start()`を呼ぶ。`CompleteSubQuestUseCase`は`InProgress`のときのみ`Complete()`を呼ぶ。
- **サブクエスト内容(Village.unity既存の未使用NPC2体を活用)**:
  - **SubQuest1**: Village.unity既存の「Villager」NPC(セリフ「The floating island holds many secrets.」)との会話で受注(`_subQuest1Giver`)。Fieldへ到達した時点で完了(`FieldSceneInstaller.Start()`が`AdvanceMainQuestUseCase`と並行して`CompleteSubQuestUseCase.Execute(session.SubQuest1)`を呼ぶ)。
  - **SubQuest2**: Village.unity既存の「Merchant」NPC(セリフ「The field beyond has fine game, if you dare.」)との会話で受注(`_subQuest2Giver`)。Dungeonへ到達した時点で完了(`DungeonSceneInstaller.Start()`が同様に`CompleteSubQuestUseCase.Execute(session.SubQuest2)`を呼ぶ)。
  - いずれもMainQuestの状態を一切参照・変更しない設計のため、MainQuest未着手のままでも受注・完了できる(独立性を手動確認で実証済み、下記参照)。新規NPC GameObjectやトリガーオブジェクトの追加は不要だった(既存の「Villager」「Merchant」は本セッション以前は未使用の会話専用NPCだった)。
- **`VillageSceneInstaller`拡張**: `_subQuest1Giver`/`_subQuest2Giver`(`NpcInteractable`、Inspector参照、任意)を追加し、`_mainQuestGiver`と同じ`DialogueStarted`購読パターンで`StartSubQuestUseCase`を呼ぶ。`OnDestroy`での購読解除も同様に追加。
- **セーブ拡張なし**: `SubQuest1`/`SubQuest2`はSaveVersion 1の時点からすでに`SaveGameSnapshot.SubQuest1State`/`SubQuest2State`として保存・復元されており(T-010/T-024で実装済み)、`PlayerSessionStateMapper`の変更は不要だった。
- **Scene変更**: `Village.unity`の`VillageSceneInstaller`へ`_subQuest1Giver`=Villager、`_subQuest2Giver`=MerchantをUnity MCP(`manage_components` set_property)経由で設定。Scene YAMLを直接編集していない。
- **テスト**: EditMode新規12件(`StartSubQuestUseCaseTests`6件、`CompleteSubQuestUseCaseTests`6件)。PlayMode新規14件(`VillageSceneInstallerTests`+6[両サブクエスト受注・独立性・セッションなし・完了済み再受注・購読解除]、`FieldSceneInstallerTests`+4[SubQuest1完了・未受注時無視・完了済み維持・MainQuest等への非干渉]、`DungeonSceneInstallerTests`+4[同SubQuest2版])。
- **全体テスト結果**: 全EditMode 392件(380→392)、全PlayMode 249件(235→249)、いずれもfailed 0・skipped 0・inconclusive 0。PlayModeは3回連続実行しすべて249件Passedを確認。プロジェクトコード由来のConsole Error/Warning 0件(コンソールに残る内容はすべて既存の意図的なシミュレーション例外・エラーログ)。正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)すべて`manage_scene validate`でMissing Script 0・Missing Reference 0・Broken Prefab 0を確認。
- **手動確認(実機Play Mode、Unity MCP `execute_code`経由)**: New Game→Village→Villagerと会話しSubQuest1受注(InProgress)→Merchantと会話しSubQuest2受注(InProgress、この時点でMainQuestはNotStartedのまま)→Fieldへ移動しSubQuest1が自動完了(Completed)、SubQuest2・MainQuestは無変化であることを確認→Dungeonへ移動しSubQuest2が自動完了(Completed)、MainQuestは引き続きNotStartedのままであることを確認(メインクエストと一切絡めずに両サブクエストを受注・完了できることを実証)→実際の`SaveGameUseCase`/`LoadGameUseCase`でSave→Load往復を行い、SubQuest1/SubQuest2ともCompletedのまま復元されることを確認。手動確認中のConsole Error/Warning 0件。
- **今回のスコープ外**: クエストログ・クエスト内容の画面表示(T-026の対象になり得るが、PROJECT.md既存のT-026定義には明記なし。必要であれば別途PROJECT.md更新のうえT-026以降で検討)。
- **未解決事項**: なし。

### T-026 アイテム・装備UIの本格実装(完了、採用範囲を限定)

PROJECT.md表8.実装タスク一覧の既存定義は「インベントリUI、装備切り替えUI、ショップ、クラフト等の本格メニュー」だったが、通貨・売買・クラフトは現在のコードベースに一切存在せず、これらを実装するには新しいDomain/Application概念(Gold、価格MasterData、レシピ・素材等)をゼロから設計する必要があるため、ユーザー承認のうえ以下のとおり採用範囲を限定した。

**採用範囲**: インベントリ一覧・所持数・効果説明・Potion使用・使用後のHP反映、現在装備中のWeapon/Armor表示・所持Weapon/Armor一覧・装備・装備解除・装備変更後のステータス反映、Village/Field/Dungeonから同じ共有メニューを使用、メニュー表示中のPlayerMovement/Encounter/NPC会話/Scene遷移Trigger停止と正しい復帰。

**対象外(将来のTaskへ)**: 通貨、価格、売買、ショップ、レシピ、素材、クラフト、装備耐久度、ランダムオプション、ドラッグアンドドロップ、アイテムソート、高度な装備比較UI。これらは必要なDomain/Application/MasterData仕様を正式に定義したうえで別Taskとして着手する。

**実装内容:**
- **`GameMenuController`(新設、Presentation/Menu/）**: T-024の`InventoryPanelController`を置き換える形で新設(旧ファイルは削除)。`_root`(Panelの開閉)、固定サイズの`ItemRowView[]`/`EquipmentRowView[]`(Weapon/Armor別)、装備中表示・Unequipボタン・ステータス表示のUI参照を持つ。`Refresh(MenuViewModel)`で表示のみを更新し、業務ルールは一切持たない。行ボタンのクリックは`event Action<int>`(行インデックス)として発火し、Composition側がインデックスをMasterData IDへ変換する。ルートGameObject自体は常時有効のまま(子の`_root`パネルのみ開閉)なので、`OnEnable`は1回だけ発火しボタン購読の二重登録は発生しない。
- **`ItemRowView`/`EquipmentRowView`(新設、Presentation/Menu/）**: `[Serializable] struct`。1行分のUI参照(Root/表示Text/ボタン)を保持する。ItemとEquipmentで同じ形だが、名前空間・用途を分けて別型とした(既存の`AddItemResult`等の1機能1型規約に合わせた)。
- **`MenuViewModel`/`ItemRowViewModel`/`EquipmentRowViewModel`(新設、Presentation/Menu/）**: MasterData/Domain/Infrastructure型を一切含まない純粋な表示データ。Compositionが`PlayerSessionState`+MasterDataから変換して渡す。
- **`MenuActivityGate`(新設、Presentation/Menu/）**: `FieldActivityGate`と同じPause/Resumeパターンだが、Battle遷移用の`FieldActivityGate`とは異なりCamera/AudioListener/EventSystemは無効化しない(メニュー自体の表示・入力に必要なため)。PlayerMovement、FieldEncounterController、NpcInteractable[]、SceneTransitionTrigger[]の`.enabled`/`.SetActive`のみを切り替える。すべて任意参照(Village/Field/Dungeonで異なる組み合わせしか存在しないため)。
- **`MenuInstaller`(新設、Composition/Scenes/）**: `_items`/`_weapons`/`_armors`(MasterData Asset参照、3Sceneで共通)を保持し、`GameMenuController`のイベントを購読して既存の`ConsumeItemUseCase`/`EquipItemUseCase`/`UnequipItemUseCase`(いずれもT-024で実装済み、変更なし)を呼ぶ。`MenuOpened`で`MenuActivityGate.Pause()`と`Refresh()`、`MenuClosed`で`Resume()`を行う。装備補正後ステータス表示は既存の`EquipmentStatCalculator`(T-024)をそのまま再利用して計算し、新しい計算ロジックは追加していない。
- **`VillageSceneInstaller`の整理**: `_inventoryPanel`/`_items`/`_weapons`/`_armors`および関連ハンドラ(`OnUsePotionRequested`等)をすべて削除し、責務を`MenuInstaller`へ移管した。MainQuest/SubQuestの開始ロジックのみ残る。
- **共有Prefab**: `Assets/_Project/Prefabs/GameMenu.prefab`をUnity MCP(`manage_prefabs create_from_gameobject`)経由で作成し、Village/Field/Dungeonの3Sceneへ同一Prefabのインスタンスとして配置した(`manage_gameobject create` + `prefab_path`)。Weapon/Armor/ItemのMasterDataカタログはPrefab側に共通で設定済み(3Scene共通のグローバルAssetのため上書き不要)。`MenuActivityGate`のPlayerMovement/FieldEncounterController/NpcInteractable[]/SceneTransitionTrigger[]参照のみ、Scene固有のインスタンスオーバーライドとして`SerializedObject`経由で設定した(Village: NPC3体+Trigger1件、Field: Encounter+Trigger2件、Dungeon: Encounter+Trigger1件、いずれもNPCなし)。
- **入力**: 新規Input Actionは追加していない。メニューの開閉は画面上の「Menu」/「Close」ボタン(既存のUnityEngine.UI Button/EventSystemクリック)のみで行い、既存のKeyboard/Gamepad入力規則(Move/Interact/UI Navigate等)は変更していない。
- **UI構成**: Canvas(Screen Space - Overlay)+CanvasScaler+GraphicRaycasterの1枚に、開くボタン・Panel(Close/StatsText/Item行×2/装備中表示・Unequipボタン・Weapon行×2・Armor行×2)を配置。Item行は名前・所持数・効果説明を1つのTextへ結合して表示する簡易構成とした(例:「Small Potion x2 - Restores 20 HP」)。既存のCamera/EventSystemは各Sceneに1つのまま重複させていない。
- **今回のスコープ外の対応**: なし(上記「対象外」項目のとおり、通貨・ショップ・クラフト等は着手していない)。

**テスト**: PlayMode新規28件(`GameMenuControllerTests`13件[開閉・二重オープン防止・行の表示更新・数量0の安全な表示・行が足りない場合の非表示・ボタンクリックでのインデックス通知・装備済み表示・Unequipボタン・ステータス表示・null ViewModelでの非クラッシュ]、`MenuActivityGateTests`4件[Pause/Resumeでの各種停止・再開・Camera/EventSystem非対象・任意参照未設定時の非クラッシュ]、`MenuInstallerTests`11件[Potion使用・所持なし・範囲外インデックス・武器/防具装備・所持なし拒否・解除・購読解除・セッションなし])。旧`InventoryPanelControllerTests`(5件)と`VillageSceneInstallerTests`の旧Inventory関連6件は、責務移管に伴い削除した(テストの弱体化ではなく、対象コード自体を`MenuInstaller`/`GameMenuController`へ移設したことに伴う置き換え)。

**全体テスト結果**: 全EditMode 392件(T-025から変更なし)、全PlayMode 266件(249→266)、いずれもfailed 0・skipped 0・inconclusive 0。PlayModeは3回連続実行しすべて266件Passedを確認。プロジェクトコード由来のConsole Error/Warning 0件。正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)すべて`manage_scene validate`でMissing Script 0・Missing Reference 0・Broken Prefab 0を確認。

**手動確認(実機Play Mode、Unity MCP `execute_code`経由)**: New Game→Village→Potion2個・RustySword1個を付与→Menuを開く(PlayerMovement・Elder[NpcInteractable]が停止することを確認)→Item一覧に「Small Potion x2 - Restores 20 HP」「Large Potion x0 - Restores 50 HP」(所持数0でも安全に表示)を確認→Small Potion使用→所持数1・HPが10→20(MaxHpでクランプ)に更新されることを確認→Rusty Sword装備→行に「(Equipped)」表示・装備中表示が「Weapon: Rusty Sword」・ステータス表示のATKが5→8に更新・Equipボタンが非活性化されることを確認→Unequip→ATKが8→5に戻ることを確認→Menuを閉じる(PlayerMovement・Elderが再開することを確認)→EventSystem/Camera/AudioListener/GameMenuControllerがそれぞれ1個のみであることを確認。Fieldへ移動しMenuを開閉してPlayerMovement・FieldEncounterControllerの停止/再開を確認。Dungeonへ移動し同様にMenuを開閉してPlayerMovement停止/再開を確認。手動確認全体を通じてConsole Error/Warning 0件。

**既知の問題**: なし。

### T-027 通し結線・E2E確認(完了)

PROJECT.md表8.実装タスク一覧の既存定義(「上記全タスクの統合」「タイトル→村→フィールド→ダンジョン→ボス撃破→クリア画面が、Consoleにエラーを出さず30〜60分で完走できる」「手動プレイでの通しプレイ、およびConsole監視」)をそのまま正式仕様として実施した。T-025/T-026同様`feature/t025-t028-release-slice`ブランチで作業。

**実行環境についての重要な違い**: T-021〜T-026はUnity MCP(`com.coplaydev.unity-mcp`)経由でUnity Editorを対話的に操作し、実機Play Modeでの手動確認とTest Runnerでのテスト実行を行っていた。本セッションではUnity Editor/Unity MCPブリッジが接続されておらず(プロジェクトを開いた既存のEditorプロセスがロックを保持しておりバッチモードの二重起動が失敗したため、ユーザーの許可を得てそのEditorを終了)、代わりにUnity 6000.3.17f1のコマンドラインバッチモード(`Unity.exe -batchmode -nographics -runTests -testPlatform EditMode|PlayMode`)でテストを実行し、Unity MCPの`manage_scene validate`相当の検証は新設した`SceneWiringValidator`(下記参照)で代替した。**`-quit`と`-runTests`を同時指定するとスクリプトコンパイル完了直後にEditorが終了しテストが一切実行されない不具合を確認したため、`-runTests`使用時は`-quit`を付与しない運用とした**(`-executeMethod`単体実行時は`-quit`を使用)。

**新規EditModeテスト(`Assets/_Project/Tests/EditMode/EndToEnd/FullFlowIntegrationTests.cs`、6件)**: Domain/Application層のみで完結する統合テスト。MainQuest・SubQuest1・SubQuest2を同一セッション上で並行して開始・進行・完了させ、各ステップでの相互非干渉を検証(`FullQuestChain_...`)。MainQuest未着手のままSubQuestのみ完了できることの検証(`SubQuestsCanReachCompleted_...`)。イベントの重複発火に対する冪等性検証(`RepeatedQuestEvents_...`)。`PlayerSessionState.ClaimReward`+`AddItemUseCase`による一度きり報酬ガードの検証(`OneTimeRewardGate_...`)。`GrantBattleRewardUseCase`によるレベルアップとHP/MP全回復の検証(`BattleRewardChain_...`)。Level/Experience/MainQuest/SubQuest/Inventory/Equipment/ClaimedRewardIdsすべてを進行させた状態からの`PlayerSessionStateMapper`によるSave/Loadラウンドトリップ検証(`SaveLoadRoundTrip_...`)。既存のUseCase/Domain型をそのまま利用し、新しいDomain/Application型は追加していない。

**新規PlayModeテスト(`Assets/_Project/Tests/PlayMode/EndToEnd/FullPlaythroughE2ETests.cs`、1件)**: 既存PlayModeテストがすべて`BuildScene()`ヘルパーでコード生成した合成Sceneのみを使うのに対し、本テストは唯一、実際にコミットされた`Title.unity`/`Village.unity`/`Field.unity`/`Dungeon.unity`/`Battle.unity`/`GameClear.unity`を`SceneManager`で実ロードし、いただいた21ステップ(Title起動→New Game→メインクエスト開始→サブクエスト2件受注→Field到達→通常戦闘→経験値・レベルアップ→Item取得・Potion使用→Weapon/Armor装備→メニュー開閉→Dungeon到達→通常戦闘→Boss戦敗北→Retry→Boss再戦勝利→MainQuest完了→GameClear→サブクエスト完了確認→Title復帰→Continue→状態復元)を、プレイヤー入力が実際に叩く公開APIのみ(Buttonの`onClick.Invoke()`、`NpcInteractable.RequestStart()`)、および物理トリガー(`OnTriggerEnter`)発火用の公開APIを持たないコンポーネントに限り既存テストと同じ手法(privateなイベントバッキングフィールドへのリフレクション経由`Invoke`)で駆動する。戦闘の勝敗は`BattleParticipantState.ApplyDamage`(public)で対象のHPを1まで削ってから実際に攻撃ボタンを押す方式で確定させ(バランス数値に依存させず、`ExecuteTurn`/`CombatCalculator`/`BattleEnded`/報酬付与/クエスト進行/Scene遷移はすべて実プロダクションコードを通す)、Save/Load往復は実ゲーム内にセーブ実行UIが存在しないため(T-024記載の既知の問題)過去セッションの手動確認と同じ方法、すなわち実`SaveGameUseCase`を直接呼び出す形で検証した。実行のたびに独立した一時ディレクトリへ`GameServices`を向け直す(`GameCompositionRoot`のprivateな`Services`バッキングフィールドへリフレクションで代入)ことで、実`Application.persistentDataPath`上の実セーブデータには一切触れない。Camera/AudioListener/EventSystem/GameMenuControllerの重複禁止も要所で断言している。**なお、`PlayerSessionState.MoveToScene`はDomain/Applicationに実装済みだが、Village/Field/Dungeon/Battle/GameClear/Titleいずれのインストーラーからも呼び出されておらず(T-021〜T-026時点からの既存動作)、Continueは常にVillageへ復元される。これはT-025〜T-027で変更した挙動ではなく、T-024完了時点の手動確認記録とも一致する既存仕様として扱った**(本テストもContinue後はVillage復元を前提にassertしている)。

**新設`SceneWiringValidator`(`Assets/_Project/Editor/SceneWiringValidator.cs`)**: Unity MCPの`manage_scene validate`が使えない環境向けに、正式6Sceneを`EditorSceneManager.OpenScene`で開いてMissing Script(null Component)・Missing Reference(参照先が消失したSerializedProperty)・Broken Prefab(`PrefabInstanceStatus.MissingAsset`)を数えるだけの読み取り専用バッチ実行可能スクリプト。Scene保存は一切行わない。`-executeMethod FloatingIslandsRpg.Editor.SceneWiringValidator.ValidateOfficialScenes`で実行する。

**既存テストの安定化(`Assets/_Project/Tests/PlayMode/Player/PlayerMovementTests.cs`、T-025〜T-027のスコープ外の既存T-018相当テストに対する最小修正)**: 初回のCLIバッチモード全件実行で`Update_WhenDiagonalHeld_DoesNotExceedAxisAlignedDisplacementMagnitude`が再現性をもって失敗した(`diagonalSpeed`が常に0)。調査の結果、原因はPlayerMovement本体の不具合ではなく、本テストがReal `Time.deltaTime`を複数フレームにわたり積算して速度を逆算する方式であり、`-batchmode`(vsync等によるフレーム間隔の制御が働かない)ではフレームあたりの実`deltaTime`が極端に小さく不安定になり、`CharacterController.Move`に渡される移動量が実質ゼロに埋もれてしまうことだと判明した(`-nographics`の有無に関わらず再現。対話的Editor実行[過去セッションで266件Passed済み]では発生しない、CLIバッチモード固有の環境要因)。PlayerMovement.cs自体は一切変更せず、テスト側で`Time.captureDeltaTime`(Unityが公式に提供する「Time.deltaTimeを固定値に確定させる」ための標準API)を1/60秒に固定してから測定し、テスト終了時に0(通常モード)へ戻す形にテストのみを修正した。アサーションの弱体化・許容誤差の緩和・`Ignore`化・`Assert.Inconclusive`化は行っていない。「斜め移動が軸方向移動の速度を超えない」という検証意図・しきい値は変更していない。

**全体テスト結果(実測、Unity 6000.3.17f1、CLIバッチモード)**:
- 全EditMode: 398件Passed(392→398、T-027新規6件)、failed 0・skipped 0・inconclusive 0。
- 全PlayMode: 267件Passed(266→267、T-027新規1件)、failed 0・skipped 0・inconclusive 0。**3回連続実行しすべて267件Passedを確認**(既存`PlayerMovementTests`安定化後)。
- 正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)すべて、新設`SceneWiringValidator`でMissing Script 0・Missing Reference 0・Broken Prefab 0を確認。
- Console: プロジェクトコード由来のError/Warning 0件。ログに残るのはすべて(1)既存テストが`LogAssert.Expect`で捕捉する意図的なシミュレーション例外・エラー(T-021〜T-026から継続する既知のパターン)、(2)T-021〜T-024のCodex review記載と同一の「2 event systems」「2 audio listeners」警告(既存テストが`FieldActivityGate`を経由せず直接Scene遷移APIを呼ぶテスト手法上のアーティファクトとして既に説明済み。T-027新規テストはこの警告を発生させていないことを個別に確認済み)、(3)Unity Package Manager/ライセンスクライアントの起動時情報ログ、のみ。

**「手動プレイでの通しプレイ」に関する対応方針**: 本セッションはUnity MCP/対話的Editorへのアクセスがなかったため、T-021〜T-026のような`execute_code`経由の実機手動確認は実施していない。代わりに、実際にコミットされたSceneファイルを実ロードし21ステップ全体を1本のPlayModeテストとして3回連続実行することで、手動プレイと同等以上の再現性・厳密性をもって「通し結線・E2E確認」の完了条件(Consoleエラーなしで一気通貫完走)を検証した。対話的Editorでの目視確認は行っていない点をここに明記する。

**今回のスコープ外**: 通貨・ショップ等T-026で対象外とした機能全般(変更なし)。`PlayerSessionState.MoveToScene`を実際に呼び出しSceneごとのContinue復元先を変える機能追加(既存動作の変更はT-027の完了条件に含まれないため対応していない)。

**既知の問題**: なし(T-027範囲内)。`PlayerSessionState.MoveToScene`未使用によりContinueが常にVillageへ復元される点は、T-024時点から続く既存仕様として維持した(前述のとおり)。

### T-028 READMEおよびLICENSEの整備(完了)

PROJECT.md表8.実装タスク一覧の既存定義(「プロジェクト概要・セットアップ手順が記載されたREADMEと、ライセンス方針に沿ったLICENSEが存在する」)をそのまま正式仕様として実施した。Windowsビルド作成・公開ビルド確認は当該定義に含まれないため実施していない。`feature/t025-t028-release-slice`ブランチでの作業、`Runtime`/`Scene`/`Prefab`/`Asset`/`meta`は変更していない。

**README.md**: T-001〜T-002段階のまま更新されていなかった内容(「ゲームロジック・UI・Prefab・ScriptableObjectは未実装」等)を、T-001〜T-027完了時点の実態に合わせて全面更新した。追加した節: 現在の完成範囲、遊び方(New Game/Continue/Save/移動・会話/Menu/Quest・SubQuest/Battle/Retry/GameClear条件)、既知の制限、テストの実行方法、テスト結果(実測値)、T-025〜T-027確認結果概要。既存の節(概要、開発環境、PROJECT.mdについて、開発フロー、今後追加予定のCI項目、ライセンス、注意事項)は内容を更新しつつ維持した。セーブ実行UI未実装・戦闘コマンドが「たたかう」のみ・Continueが常にVillage復元になる点など、既知の制限は事実に基づき明記した。

**LICENSE**: 既存のMIT Licenseをそのまま維持し、変更していない(`git diff -- LICENSE`で差分なしを確認済み)。`Assets`配下を確認し、モデル・音源・フォント等の外部アセットファイル(`.fbx`/`.wav`/`.mp3`/`.ttf`/`.otf`/`.ogg`等)が存在しないことを確認したうえで、「Unity標準機能と自作コード中心であり外部アセットを追加していない」旨をREADME.mdのライセンス節に明記した。

**.gitignore**: `Assets/Screenshots/`(過去セッションのUnity MCP手動確認時のスクリーンショット、82ファイル)と`Assets/_Recovery/`(Unity Editor終了時に生成されたとみられるクラッシュリカバリScene、2ファイル)が未追跡のままコミット対象になり得たため、既存の「Unity build output」節の直後に4行追加してこれらを除外した。ファイル自体は削除していない(既存のローカル生成物であり、削除はT-028の完了条件に含まれないため)。それ以外の既存の除外設定(`Library`/`Temp`/`obj`/`Logs`/`UserSettings`/`.vscode`の許可リスト方式等)は変更していない。

**docs配下の補助ドキュメント**: PROJECT.mdのT-028正式定義がREADME.md/LICENSEの2ファイルに限定されており、依頼された内容はすべてREADME.md本体に収まったため、新規docsファイルの作成は不要と判断し、作成していない。

**確認結果**:
- `git diff --stat`: 変更対象は`.gitignore`・`README.md`・`PROJECT.md`と、T-025〜T-027で既に変更済みだったファイル群のみ。T-028で新規に変更したのは`.gitignore`/`README.md`/`PROJECT.md`の3ファイルのみ。
- `git diff -- LICENSE`: 差分なし。
- `git diff --stat -- ProjectSettings`: 差分なし。
- `git diff --stat -- Packages`: 差分なし。
- `git diff --check`: `Assets/_Project/Scenes/{Dungeon,Field,Village}.unity`(T-025/T-026由来、Unity YAMLシリアライズの空値表現に伴う既存のtrailing whitespaceで、T-028では触れていない)以外に指摘なし。手書きMarkdown(`README.md`/`PROJECT.md`)およびで`.gitignore`にtrailing whitespaceなし。
- Runtime/Scene/Prefab/Asset/metaへの変更: なし。T-029以降の先行実装: なし。commit・push・PR作成・`main`マージ: 未実施。

**今回のスコープ外**: Windowsビルド作成、公開ビルド確認、CIへのテスト自動実行組み込み(README「今後追加予定のCI項目」に記載のみ)。

**既知の問題**: なし。

### Build Settings(Codex第三者レビューMajor 4指摘、最終再レビュー指摘、およびT-019/T-020のScene追加対応)
- `ProjectSettings/EditorBuildSettings.asset`を、Unity MCP(`manage_build` action=scenes)経由で以下の順序へ更新した(直接テキスト編集はしていない)。
  - Build Index 0: `Title`
  - Build Index 1: `Village`
  - Build Index 2: `Battle`
  - Build Index 3: `GameClear`
  - Build Index 4: `Field`(T-019で追加)
  - Build Index 5: `Dungeon`(T-020で追加)
- `SampleScene`はBuild Settingsから除外した(Build実行時に正式なTitleから開始されるようにするため)。
- `SampleScene.unity`のAsset自体は5.規約・7.要承認事項の既定方針により削除していない(Build対象外になっただけで、Assetとしては保持)。
- `SceneNameCatalog`(T-003)が保持するScene名(`Title`/`Village`/`Field`/`Dungeon`/`Battle`/`GameClear`)とBuild Settings登録パスのScene名は一致している(正式6Sceneすべてが登録済み)。

### Prefab方針
- プレイヤー、NPC、敵、UIパネル等は原則Prefab化し、Sceneへの直置きを避ける。
- Prefab Variantsを用いて敵3種+ボスなど差分の大きいバリエーションを管理する。

### ScriptableObject方針
- マスターデータ(敵ステータス、アイテム定義、装備定義、クエスト定義)をScriptableObjectで管理する。
- **MVPではScriptableObjectのみを供給手段とする。Addressablesは導入しない(MVP対象外)。** 将来的なAddressables導入を前提とした抽象化(ローダー抽象化、非同期ロードインターフェースの先回り実装等)は過剰設計として行わない。
- ScriptableObjectには実行時に変化する状態(現在HP等)を保存しない(5.規約参照)。ランタイム状態はDomain層のインスタンスが保持する。

### 依存方向
```
Domain は他レイヤーに依存しない(参照なし)。
Application → Domain
Infrastructure → Application, Domain (Domainのインターフェースを実装する形でInfrastructureが依存される)
Presentation → Application, Domain
Presentation と Infrastructure は相互に参照しない。
```

### イベント通知方針
- Domain/Applicationの状態変化は、UnityEvent直結ではなくC#イベント(delegate/event)またはインターフェースコールバックで通知し、Presentation層がそれを購読してUIやエフェクトに反映する。
- **局所的な通知**(同一コンポーネント内、または近接する責務間の単純な通知)には素朴な**C# event**を使用する。
- **システム境界をまたぐ通知**(レイヤー境界、モジュール境界)には**interface**によるコールバック契約を定義し、それを介して通知する。
- プロジェクト全体を横断する**グローバルかつ静的な万能EventBus/メッセージングハブは作成しない**。
- イベントを購読した箇所では、対になる購読解除(`OnDestroy`/`OnDisable`等)を必ず実装し、**購読解除漏れを防ぐ**。

### セーブデータ設計方針
- セーブ対象: パーティ構成、各キャラクターのステータス・レベル・経験値、所持アイテム・装備、クエスト進行状況、現在地。
- 保存形式は**バージョン番号を含むJSON(バージョン付きJSON)**とする。
- **MVPでは暗号化・難読化を行わない。**
- 書き込みは一時ファイルへ保存し、書き込み・検証が正常に完了した場合にのみ本ファイルへ置換(アトミックな置き換え)する。書き込み途中のクラッシュ・強制終了による本ファイル破損を防ぐ。
- バックアップは直近**1世代のみ**保持する。
- 読込時にデータ破損を検知した場合は、まずバックアップからの復旧を試み、バックアップも利用できない場合は安全な初期状態(セーブデータなし相当)へ戻す(3.仕様「エラー時の挙動」、5.規約「セーブデータ破損時の処理」参照)。
- ScriptableObjectへ実行時状態を保存しない(前述「ScriptableObject方針」参照)。
- 現在の`SaveVersion`は`1`(`SaveGameSnapshot.CurrentSaveVersion`、T-010/T-011で実装済み。詳細は4.設計「セーブ/ロードユースケース・PlayerSessionState」「セーブデータ保存基盤」参照)。

---

## 5. 規約

- **C#命名規則**: クラス/メソッド/プロパティはPascalCase、ローカル変数/引数はcamelCase、privateフィールドは `_camelCase`。Unity標準命名(Start, Update等)はそのまま使用。
- MonoBehaviourを肥大化させない。1つのMonoBehaviourが担う責務は「Unityとの接続点」に限定し、ロジックはApplication/Domainへ委譲する。
- public fieldを乱用しない。Inspector公開が必要な場合は `[SerializeField] private` を基本とし、外部公開が必要なもののみプロパティ経由にする。
- **SerializeFieldの使用方針**: Inspectorで調整したい値・参照にのみ使用する。実行時にのみ意味を持つ状態はSerializeFieldにしない。
- **null処理**: Unityオブジェクトの破棄済みnullとC#のnullを区別して扱う。Domain層ではnull許容型を避け、未設定は明示的な初期値や結果型で表現する。
- **例外処理**: 想定内の失敗(セーブデータ破損、マスターデータ不整合等)は例外ではなく戻り値/結果型で扱う。想定外の異常は例外として上位に伝播させ、握りつぶさない。
- **ログ方針**: `Debug.Log` の乱用を避け、警告・エラーレベルを適切に使い分ける。リリースビルドに不要なログは条件コンパイルや専用ロガーで抑制する(詳細は今後決定)。
- ScriptableObjectへの実行時状態保存を禁止する(マスターデータ専用とする)。
- Scene名や文字列の直接指定を減らす。Scene名は定数またはenumで一元管理し、マジックストリングでの `SceneManager.LoadScene("Village")` のような直書きを避ける。
- マジックナンバー禁止。ステータス計算式の係数等は定数化またはScriptableObjectのパラメータとして外出しする。
- **入力検証**: セーブデータ読込時・マスターデータ参照時は値の妥当性(範囲、null、存在しないID参照等)を検証してから使用する。
- **セーブデータ破損時の処理**: 読込に失敗した場合はクラッシュさせず、エラーを通知しタイトル画面に留める(3.仕様「エラー時の挙動」参照)。
- 機密情報をリポジトリへ含めない(APIキー等が発生した場合は`.gitignore`対象とし、コミットしない)。
- 不要なPackage追加禁止。追加が必要な場合はPROJECT.mdの要承認事項として先に記録し、承認後に追加する。
- スコープ外実装禁止。2.スコープの「MVPに含まない機能」に該当する実装は行わない。
- **Unity MCP運用方針**:
  - 実装作業(Scene/Prefab/Script/Project Settings等Unityプロジェクトへの変更を伴う作業)では、Unity MCP接続を**必須**とする。
  - SceneやPrefabの変更は、可能な限りUnity MCPツール経由で行う。
  - Unity MCPが未接続の場合、SceneファイルのYAML等を直接テキスト編集することを禁止する。
  - Unity MCPが未接続のまま実装作業を継続しない。接続を再確立してから作業を再開する。

---

## 6. 現状

### フェーズ状況
- **Phase 0: 完了**。Unity 6 / URPプロジェクト作成、主要パッケージ導入、Unity MCP接続確認まで完了。
- **Phase 1: 完了**。Git初期化、`.gitignore`、`.gitattributes`、`README.md`、`LICENSE`、最小CI(`.github/workflows/repository-check.yml`)を作成。`feature/project-foundation`ブランチ(T-001, T-002, 文書整合修正含む)はPull Request #1を経て`main`へマージ・push済み(マージコミット`913b90e`)。
- **Phase 2: 設計承認済み**。7.要承認事項の全7項目についてユーザーによる方針決定が完了し、承認済み。決定内容は4.設計/5.規約/7.要承認事項へ反映済み。
- **T-001: 完了・`main`にマージ済み**。`Assets/_Project/`配下の基盤ディレクトリ(17フォルダ)を作成済み。
- **T-002: 完了・`main`にマージ済み**。レイヤー別Assembly Definition(asmdef)7個を作成済み。
- **T-003: 完了・`main`にマージ済み**(Codex第三者レビュー指摘対応完了、PR #2)。Scene識別子`SceneId`とScene名解決`SceneNameCatalog`をApplication層に作成。PROJECT.md「3.仕様 Scene一覧」の正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)へ統一済み。EditModeテスト11件すべてPassed。
- **T-004: 完了・`main`にマージ済み**(Codex第三者レビューMinor指摘対応完了、PR #3)。キャラクターステータス計算ロジック(`CharacterStats`, `StatGrowthProfile`, `CharacterStatsCalculator`)をDomain層に作成。EditModeテスト33件すべてPassed。
- **T-005: 完了・`main`にマージ済み**(Codex第三者レビューMajor指摘対応完了、PR #4)。戦闘計算ロジック(`CombatCalculator`: ダメージ計算・命中/回避・行動順決定)をDomain層に作成。EditModeテスト40件すべてPassed。
- **T-006: 完了・`main`にマージ済み**(Codex第三者レビュー合格済み、PR #4)。経験値・レベルアップ計算ロジック(`ExperienceTable`, `LevelUpCalculator`)をDomain層に作成。EditModeテスト20件すべてPassed。
- **T-007: 完了・`main`にマージ済み**(Codex第三者レビュー合格済み、PR #4)。クエスト状態管理(`QuestState`, `QuestProgress`)をDomain層に作成。EditModeテスト8件すべてPassed。
- **T-008: 完了・`main`にマージ済み(PR #5、Codex第三者レビューMinor指摘対応完了)**。戦闘進行ユースケース(`BattleSession`等)をApplication層に作成。EditModeテスト27件すべてPassed。
- **T-009: 完了・`main`にマージ済み(PR #5、Codex第三者レビューMajor指摘対応完了)**。Scene遷移ユースケース(`SceneTransitionUseCase`, `UnitySceneLoader`)をApplication/Infrastructure層に作成。EditModeテスト17件すべてPassed。
- **T-010: 完了・`main`にマージ済み(PR #5)**。セーブ/ロードユースケースと`PlayerSessionState`をApplication層に作成。EditModeテスト34件すべてPassed。
- **T-011: 完了・`main`にマージ済み(PR #5、Codex第三者レビューMajor指摘対応完了)**。セーブデータ保存基盤(`FileSystemSaveStorage`, `JsonSaveRepository`)をInfrastructure層に作成。PlayModeテスト24件すべてPassed。
- **T-012: 完了・`main`にマージ済み(PR #5)**。敵/アイテム/装備マスターデータ定義をDomain/Infrastructure層に作成。EditModeテスト35件すべてPassed。
- **T-013: 完了・`main`にマージ済み(`feature/presentation-gameplay-slice`ブランチ、PR #6)**。プレイヤー移動・追従カメラ(`PlayerMovement`, `FollowCamera`)をPresentation層に作成。PlayModeテスト6件すべてPassed。
- **T-014: 完了(同ブランチ)**。NPC会話UI(`DialogueSession`, `DialogueBoxView`, `NpcInteractable`)をPresentation層に作成。T-007 `QuestProgress`と連携。PlayModeテスト22件すべてPassed。`DialogueBox.prefab`作成。
- **T-015: 完了(同ブランチ)**。戦闘UI(`BattleUIController`)をPresentation層に作成。T-008 `BattleSession`をそのまま利用しダメージ/命中判定/行動順を再実装していない。PlayModeテスト8件すべてPassed。`BattleUI.prefab`作成。
- **T-016: 完了(同ブランチ)**。タイトル画面(`TitleScreenController`)をPresentation層に作成。T-010 `LoadGameUseCase`を注入して使用しセーブデータ有無に応じConinue可否を切り替える。正式`Title.unity` Sceneを新規作成。PlayModeテスト9件すべてPassed。
- **T-017: 完了(同ブランチ)**。ゲームクリア/オーバー画面(`GameResultScreenController`)をPresentation層に作成。T-008 `BattleOutcome`を外部から受け取り表示するのみで勝敗を再計算しない。正式`GameClear.unity` Sceneを新規作成(`SceneId`に`GameOver`は存在しないため、単一Scene内でClear/Over表示を切り替える設計)。PlayModeテスト7件すべてPassed。
- **T-013〜T-017 合計(初回実装セッション)**: PlayModeテスト新規52件(6+22+8+9+7)。全EditMode 225件、全PlayMode 76件(既存24+新規52)、全件Passed。
- **Codex第三者レビュー指摘対応(Major 4件・Minor 1件、本セッションで対応完了)**: Major1(本番Composition Rootが存在せずテストからのBind/Show以外で本番経路が成立しない)、Major2(T-013 FollowCameraが実Scene/Prefabへ未配置)、Major3(T-014 NpcInteractableが実Scene/Prefabへ未配置)、Major4(Title/GameClear等の正式SceneがBuild Settings未登録)、Minor(`Assets/Screenshots/`のコミット方針未定義)を解消。
  - 新規asmdef`FloatingIslandsRpg.Composition`を作成し`GameServices`/`GameCompositionRoot`/`GameCompositionRootLocator`/Scene別Installer4種を実装(詳細は4.設計「Composition Root」参照)。
  - `Village.unity`(最小スキャフォールド)・`Battle.unity`を新規作成し、`Title.unity`/`GameClear.unity`と合わせてBuild Settingsへ登録(詳細は4.設計「T-013〜T-017 本番経路接続」「Build Settings」参照)。
  - `FloatingIslandsRpg.Infrastructure.Battle.SystemRandomSource`(`IRandomSource`実装)をInfrastructure層に追加。
  - `BattleUIController`に`BattleEnded`イベントを追加(既存API変更なし)。
  - `Assets/Screenshots/`はコミット対象外(未追跡のまま、本番Asset・テストからも参照しない)と方針決定。
  - 新規PlayModeテスト22件(Composition Root関連。`GameServicesTests`3件、`GameCompositionRootTests`2件、`GameCompositionRootLocatorTests`3件、`TitleSceneInstallerTests`4件、`BattleSceneInstallerTests`3件、`GameClearSceneInstallerTests`4件、`VillageSceneInstallerTests`2件、以上に自己修復の回帰テスト1件を追加)。
  - **手動確認(Unity Editor実機Play Mode、フェイクなし)**: Title→New Game→Village実遷移、Village上でNPCへの会話開始→3ページ送り→終了→プレイヤー移動停止/復帰、実セーブ作成→Title→Continue→Battle実遷移(ロード済みStatsがHP表示に反映)→Attack操作→Victory→GameClear実遷移(Clear表示)→Title実遷移、および別セーブでDefeat→GameOver表示→Retry→実セーブ再ロード→Battle実遷移(再ロードHPが反映)まで、スクリーンショットで確認済み。
  - **全体テスト実行結果**: 全EditMode 225件、全PlayMode 98件(既存76+Composition新規22)。Unity Editor Test Runnerで全件Passed(failed 0, skipped 0)を、全PlayModeは3回連続実行で確認済み。プロジェクトコード由来のConsole Error/Warning 0件、Missing Script/Broken Prefab 0件(全4正式Scene`manage_scene validate`で確認)。Play Mode中に`[WebSocket] Unexpected receive error`(MCP由来、既知)、および今回新たに`PlayerLoop internal function called recursively`とスクリプト参照欠落の一時的なConsole出力を観測したが、いずれもMCP自動化ツールによる短時間の連続Play Mode操作(pause切替・execute_code連続実行)に起因する一過性の事象であり、全4正式Sceneの`manage_scene validate`は0件を維持しているため、プロジェクトコード由来ではないと判断した(詳細は「既知の問題」参照)。
- **Codex最終再レビュー指摘対応(Major 2件・Minor 2件、本セッションで対応完了)**: Build Settings(SampleSceneがBuild Index 0のまま)、T-017 Retry仕様(保存データのCurrentSceneIdへ遷移してしまい実際にはBattleへ戻らない)の2 Major、および遷移失敗時のUI復旧、PROJECT.mdの古い記述("SampleSceneのみ存在")の2 Minorを解消。
  - Build SettingsをUnity MCP(`manage_build`)経由で Title(0)→Village(1)→Battle(2)→GameClear(3) の順へ更新し、SampleSceneを除外(詳細は4.設計「Build Settings」参照)。
  - `GameServices.RematchSnapshot`を新設し、`BattleSceneInstaller`がBattle開始直前の`CurrentSession`を防御的コピーして保持。`GameClearSceneInstaller.OnRetryRequested`はこのSnapshotから`CurrentSession`を復元し、常に`SceneId.Battle`へ遷移する(保存データの`CurrentSceneId`は使用しない)。Snapshot不在時は`GameResultScreenController.ShowError()`で安全に失敗する(詳細は4.設計「T-017 Retry仕様の修正」参照)。
  - `LastBattleOutcome`のクリア箇所をNew Game/Continue/Title復帰/Retry開始の4箇所に整備。
  - `TitleScreenController`/`GameResultScreenController`に`CompleteTransition()`/`FailTransition()`を追加し、各Scene Installerの非同期遷移を`try/catch/finally`化(例外はログに残し握りつぶさない、`async void`はイベント購読口のみ)。
  - `GameCompositionRootLocator`の用途(Scene Installer専用、public Root取得APIなし)は変更していない。
  - 新規・更新PlayModeテスト20件(`TitleSceneInstallerTests`+5、`BattleSceneInstallerTests`+3、`GameClearSceneInstallerTests`全面改訂9件、`TitleScreenControllerTests`+3、`GameResultScreenControllerTests`+5)。全PlayMode 118件(既存98+新規20)。
  - **手動確認(実機Play Mode)**: 保存地点=Village・弱ステータスの実セーブ→Continue→Village実遷移(HP25/25正しく反映)→Battleへ実遷移(RematchSnapshot作成: CurrentSceneId=Battle, HP=25で一致確認)→Attack→Victory→GameClear実遷移(Clear表示)→Title実遷移(LastBattleOutcomeクリア確認)。別途、保存地点=Village・低HPセーブ→Continue→Village→Battle→Defeat→GameOver表示(古い結果の残留なし)→**Retry→Battle実遷移(保存地点Villageではない)を確認、HP 8/8(0ではない)で再戦開始**を確認。遷移失敗時のUI復旧は、実SceneManagerを意図的に破壊するのは危険なため、自動テスト(FakeSceneLoaderによる例外注入、6件)で検証した。
  - **全体テスト実行結果**: 全EditMode 225件、全PlayMode 118件、Unity Editor Test Runnerで全件Passed(failed 0, skipped 0)を3回連続実行で確認。プロジェクトコード由来のConsole Error/Warning 0件、Missing Script/Broken Prefab 0件(全4正式Scene確認)。
- **T-018 村エリアの実装(完了)**: `Village.unity`にNPC3体(Villager/Elder/Merchant、`Npc.prefab`ベース)とField行き`SceneTransitionTrigger`を追加。`VillageSceneInstaller`を拡張。PlayModeテスト7件(`SceneTransitionTriggerTests`4件、`VillageSceneInstallerTests`+3件)。
- **T-019 フィールドエリアの実装(完了)**: 正式`Field.unity`を新規作成。`FieldEncounterController`によるランダムエンカウント、`FieldActivityGate`によるAdditive Battle中の入力/カメラ停止、`FieldSceneInstaller`を新規実装。Battle SceneのロードモードをField/Dungeonに対してAdditiveへ変更(4.設計「村エリア・フィールド・ダンジョン本実装」参照)。PlayModeテスト17件(`FieldEncounterControllerTests`6件、`FieldActivityGateTests`2件、`FieldSceneInstallerTests`5件、`BattleSceneInstallerTests`+4件)。
- **T-020 ダンジョンの実装(完了)**: 正式`Dungeon.unity`を新規作成。T-019の`FieldEncounterController`/`FieldActivityGate`を再利用し、最奥に`BossEncounterTrigger`とボス用プレースホルダー`BossEncounterEnemyStats`(通常敵より明確に強いステータス)を配置した`DungeonSceneInstaller`を新規実装。ボス勝利時のみGameClearへ、通常戦勝利時はDungeonへ復帰する分岐を`GameServices.PendingBattle`で実現。PlayModeテスト10件(`BossEncounterTriggerTests`4件、`DungeonSceneInstallerTests`6件)。
- **T-018〜T-020 合計(初回実装セッション)**: 新規PlayModeテスト34件(7+17+10)。全EditMode 225件(変更なし)、全PlayMode 152件(既存118+新規34)、Unity Editor Test Runnerで全件Passed(failed 0, skipped 0)を3回連続実行で確認。手動確認(Village全NPC会話、Village→Field、Field上でのAdditive Battle往復、Field↔Village/Dungeon遷移、Dungeon上でのボス戦勝敗分岐、Dungeon↔Field遷移)はすべて実機Play Modeで確認済み(詳細・観測されたConsoleノイズ/例外の判断根拠は4.設計「村エリア・フィールド・ダンジョン本実装」参照)。
- **T-018〜T-020 Codex第三者レビュー指摘対応(Major1件・Minor4件、対応完了)**: Major(Battle Sceneアンロード失敗時のField/Dungeon側復旧漏れ)、Minor(ボス勝利テストの乱数依存、git diff --checkの実態未報告、EditorSettings.assetの意図しない差分、コメントの文字化け候補)をすべて解消(詳細は4.設計「T-018〜T-020 Codex第三者レビュー指摘対応」参照)。`BattleSceneInstallerTests`が10件→16件(純増6件)、全体では152件→158件。全EditMode 225件・全PlayMode 158件、Unity Editor Test Runnerで全件Passed(failed 0, skipped 0, inconclusive 0)を3回連続実行で確認。プロジェクトコード由来のConsole Error/Warning 0件、Missing Script/Broken Prefab 0件(正式6Scene全件`manage_scene validate`で確認)。手動確認(Village→Field、Field上でのAdditive Battle連戦、Dungeonボス戦勝利→GameClear、ボス戦敗北→GameOver)を実機Play Modeで再確認済み。アンロード失敗時の復旧はFakeSceneLoaderによる自動テスト(5件)を正式な確認方法とした(実SceneManager破壊を伴う実機再現は危険なため実施せず)。
- **T-019/T-020 Codex最終再レビュー残存Major対応(対応完了)**: `BattleSceneInstaller`が`_battleCamera`/`_battleAudioListener`/`_battleEventSystem`をグローバル検索(`FindFirstObjectByType`)で取得しており、Additive中のField/Dungeon側コンポーネントを誤取得しうる状態だったのを解消。3参照を`[SerializeField]`化し、Unity MCP経由で`Battle.unity`へ明示的に設定した(詳細は4.設計「T-019/T-020 Codex最終再レビュー残存Major対応」参照)。`BattleSceneInstallerTests`が16件→20件(純増4件)、全体では158件→162件。全EditMode 225件・全PlayMode 162件、Unity Editor Test Runnerで全件Passed(failed 0, skipped 0, inconclusive 0)を3回連続実行で確認。プロジェクトコード由来のConsole Error/Warning 0件、Missing Script/Broken Prefab 0件(正式6Scene全件`manage_scene validate`で確認)。手動確認(Field/Dungeon双方での通常戦勝利→復帰、Dungeonボス戦勝利→GameClear、ボス戦敗北→GameOver)を実機Play Modeで再確認済み。Codex再レビュー前の状態。
- **ゲーム実装**: T-003〜T-012(Scene識別子、キャラクターステータス計算、戦闘計算・進行、経験値・レベルアップ計算、クエスト状態管理、Scene遷移、セーブ/ロード、マスターデータ定義)、T-013〜T-017(Presentation層の実装コード・Composition Root・Title/Village/Battle/GameClear Scene)に加え、T-018〜T-020でVillageの本実装・Field/Dungeon Sceneの新規実装・ランダムエンカウント/ボス戦・Additive Battle統合、およびそのCodex第三者レビュー指摘(第一回・最終再レビュー)対応が完了した。実データMasterData Asset(敵/アイテム/装備・開始時ステータス等)は引き続き未実装(T-021以降で対応予定、本セッションでは着手していない)。

### 完了済み
- Unity 6 (6000.3.17f1) / URPの新規プロジェクトが作成済み。
- 主要パッケージ導入済み: URP 17.3.0, Input System 1.19.0, AI Navigation 2.0.12, Timeline 1.8.12, Test Framework 1.6.0, Visual Scripting 1.9.11, MCP For Unity (v10.0.0)。
- `SampleScene.unity` からMCP接続テスト用GameObject(`MCP_Test_Ground`, `MCP_Connection_Test`)を削除済み(過去セッションで実施。当時はMCP未接続だったためシーンYAMLの直接編集で行った旨を「既知の問題」に記録)。
- Unity MCPの接続を再確立し、再接続確認を複数回実施済み(読み取り専用)。プロジェクト名 `floating-islands-rpg`、Edit Mode、Console Error 0件を安定して確認。
- 本PROJECT.mdの初稿を作成。
- 7.要承認事項の全7項目(Unity MCP運用、戦闘Scene方式、セーブデータ形式、マスターデータ供給方法、SampleSceneの扱い、TutorialInfoの扱い、イベント通知方式)についてユーザーによる方針決定が完了し、承認済みとした。決定内容を4.設計/5.規約/7.要承認事項へ反映済み。
- Phase 1のリポジトリ基盤を作成し、Pull Request #1経由で`main`へマージ・push済み:
  - `git init -b main` によるGitリポジトリ初期化(mainブランチ)。
  - Unity向け`.gitignore`、`.gitattributes`、`README.md`、`LICENSE`(MIT, Copyright 2026, 著作権者は暫定で `floating-islands-rpg contributors`)、最小CI `.github/workflows/repository-check.yml` を作成。
  - **T-001 プロジェクト基盤ディレクトリの作成**: `Assets/_Project/`配下にRuntime(Domain/Application/Presentation/Infrastructure)、Editor、Tests(EditMode/PlayMode)、Scenes、Prefabs、ScriptableObjects、UI、Art、Audio、Settingsの17フォルダをUnity MCP経由で作成。空フォルダのGit管理のため`.gitkeep`を配置。
  - **T-002 レイヤー別Assembly Definitionの作成**: `FloatingIslandsRpg.Domain`, `.Application`, `.Infrastructure`, `.Presentation`, `.Editor`, `.Tests.EditMode`, `.Tests.PlayMode` の7個を作成。Domain / Application は `noEngineReferences: true`(UnityEngine非依存)。依存方向と循環参照なしを確認済み。詳細は4.設計「Assembly Definition方針」参照。
  - PROJECT.mdの文書整合修正(T-001/T-002完了の反映、依存方向ブロックの修正等)。
  - Pull Request #1(`feature/project-foundation` → `main`)をマージ(マージコミット`913b90e`)。
- **T-003 Scene識別子・Scene名定義の作成(完了・`main`にマージ済み)**: `SceneId` (enum) と `SceneNameCatalog` (静的クラス) をApplication層に作成。EditModeテスト`SceneNameCatalogTests`を作成。詳細は4.設計「Scene識別子」参照。
- **T-003 Codex第三者レビュー指摘対応(完了、本セッション)**: Major指摘(`SceneId`/`SceneNameCatalog`がPROJECT.md正式Scene一覧と不一致。`Sample`/`Bootstrap`を含み`Village`/`Dungeon`を欠いていた)を解消し、`Title`/`Village`/`Field`/`Dungeon`/`Battle`/`GameClear`の6件へ統一。Minor指摘(Scene名検証が`string.IsNullOrEmpty`で空白文字列を検出できない)を`string.IsNullOrWhiteSpace`へ修正。テストはScene名個別対応6件(Title/Village/Field/Dungeon/Battle/GameClear)とenum/カタログ過不足検証2件の計8件を追加し、`SceneId.Sample`専用テスト1件を削除、既存の空白検証テストを`IsNullOrWhiteSpace`化。Unity Editor Test Runnerで全11件がPassed(failed 0, skipped 0)であることをユーザーが実行・確認済み。詳細は4.設計「Scene識別子」参照。
- **T-004 キャラクターステータス計算ロジックの作成(完了・`main`にマージ済み)**: `CharacterStats`(不変の値オブジェクト)、`StatGrowthProfile`(基礎値・成長値・レベル範囲プロファイル)、`CharacterStatsCalculator`(静的計算関数)をDomain層(`Assets/_Project/Runtime/Domain/Characters/Stats/`)に作成。採用ステータスは`MaxHp`/`MaxMp`/`Attack`/`Defense`/`Agility`/`Magic`の6種+`Level`。成長計算式は`stat = baseValue + perLevelGrowth * (level - profile.MinLevel)`、レベル上限は`StatGrowthProfile.MaxLevel`としてプロファイルごとに指定可能。EditModeテスト(`CharacterStatsTests`8件、`StatGrowthProfileTests`14件、`CharacterStatsCalculatorTests`11件、計33件)を作成し、全件Passed(failed 0, skipped 0)、Console Error 0件・Warning 0件を確認済み。詳細は4.設計「キャラクターステータス計算」参照。
- **T-004 Codex第三者レビュー指摘対応(完了、本セッション)**: Minor指摘(`CharacterStatsCalculator`の成長計算式`growthSteps = level - profile.MinLevel`自体は正しいが、既存テストがすべて`MinLevel = 1`のケースのみを検証しており、`MinLevel`が1以外の場合の回帰テストが不足していた)を解消するため、`CharacterStatsCalculatorTests`に`Calculate_WhenMinLevelIsNotOne_AtMinLevelReturnsBaseValues`(MinLevel=5, MaxLevel=10, level=5でgrowthSteps=0・全ステータスが基礎値のまま返ることを検証)と`Calculate_WhenMinLevelIsNotOne_UsesMinLevelAsGrowthOrigin`(MinLevel=5, MaxLevel=10, level=7でgrowthSteps=2・全6ステータスが`baseValue + perLevelGrowth * 2`の期待値と一致することを検証)の2件を追加した。`CharacterStats.cs`/`StatGrowthProfile.cs`/`CharacterStatsCalculator.cs`の本番コードは変更していない。`CharacterStatsCalculatorTests`は9件→11件、T-004関連テスト全体は31件→33件となり、Unity Editor Test Runnerで全件Passed(failed 0, skipped 0)、Console Error 0件・Warning 0件をユーザーが実行・確認済み。
- **T-005 戦闘計算ロジックの作成(完了、`feature/domain-combat-core`ブランチ、mainへ未マージ)**: `CombatCalculator`(静的クラス)をDomain層(`Assets/_Project/Runtime/Domain/Combat/`)に作成。ダメージ計算(`max(1, Attack-Defense)`)、命中/回避判定(Agility差に基づく命中率算出+外部注入の乱数値との比較)、行動順決定(Agility降順比較)を実装。T-004の`CharacterStats`を利用し、責務の重複実装はしていない。詳細は4.設計「戦闘計算ロジック」参照。
- **T-005 Codex第三者レビュー指摘対応(完了、本セッション)**: Major指摘(`ResolveHit(1.0, 1.0)`がfalseになる、`double.NaN`が範囲検証を通過する、命中率0%/100%とrandomRollの有効範囲がAPIとして曖昧)を解消するため、`CombatCalculator.ResolveHit`のhitChance/randomRollの範囲検証に`double.IsNaN`/`double.IsInfinity`の明示チェックを追加し、`randomRoll`の上限を`> 1.0`(1.0を許可)から`>= 1.0`(1.0を拒否、有効範囲を`[0.0, 1.0)`に統一)へ修正した。`CalculateDamage`/`CalculateHitChance`/`CompareTurnOrder`、T-006/T-007の本番コードは変更していない。`CombatCalculatorTests`のResolveHit関連テストを整理・拡充(8件→20件、`CombatCalculatorTests`全体で28件→40件)。Unity Editor Test Runnerで全EditModeテスト112件がPassed(failed 0, skipped 0)、Console Error 0件・Warning 0件をユーザーが実行・確認済み。
- **T-006 経験値・レベルアップ計算の作成(完了、`feature/domain-combat-core`ブランチ、Codex第三者レビュー合格済み、mainへ未マージ)**: `ExperienceTable`(不変の経験値テーブル)、`LevelUpCalculator`(静的計算関数)をDomain層(`Assets/_Project/Runtime/Domain/Progression/`)に作成。レベルごとの累積必要経験値を外部配列として受け取り、`CalculateLevel(table, totalExperience)`で到達レベルを一意に決定する。PROJECT.md上T-005ではなくT-004にのみ依存するため、`CombatCalculator`との結合は行っていない。EditModeテスト(`ExperienceTableTests`10件、`LevelUpCalculatorTests`10件、計20件)を作成し、全件Passed(failed 0, skipped 0)、Console Error 0件・Warning 0件を確認済み。詳細は4.設計「経験値・レベルアップ計算」参照。
- **T-007 クエスト状態管理の作成(完了、`feature/domain-combat-core`ブランチ、Codex第三者レビュー合格済み、mainへ未マージ)**: `QuestState`(enum)、`QuestProgress`(状態遷移クラス)をDomain層(`Assets/_Project/Runtime/Domain/Quests/`)に作成。`NotStarted→InProgress→Completed`の一方向遷移のみを許可し、不正な遷移は`InvalidOperationException`で拒否する。メイン1本・サブ2本は独立した`QuestProgress`インスタンスとして表現する。EditModeテスト`QuestProgressTests`8件を作成し、全件Passed(failed 0, skipped 0)、Console Error 0件・Warning 0件を確認済み。詳細は4.設計「クエスト状態管理」参照。
- **T-005〜T-007のmainマージ**: `feature/domain-combat-core`ブランチがPull Request #4を経て`main`へマージされた。
- **T-008 戦闘進行ユースケースの作成(完了)**: `BattleSession`/`BattleParticipantState`/`BattleActionResult`/`BattleTurnResult`/`BattleCommand`/`BattleOutcome`/`IRandomSource`をApplication層(`Assets/_Project/Runtime/Application/Battle/`)に作成。T-005の`CombatCalculator`を利用し計算式を重複実装していない。乱数は`IRandomSource`経由で注入しApplication内でSystem.Randomを生成しない。詳細は4.設計「戦闘進行ユースケース」参照。
- **T-009 Scene遷移ユースケースの作成(完了)**: `ISceneLoader`/`SceneLoadMode`/`SceneTransitionUseCase`をApplication層(`Assets/_Project/Runtime/Application/Scenes/`)に、`UnitySceneLoader`をInfrastructure層(`Assets/_Project/Runtime/Infrastructure/Scenes/`)に作成。T-003の`SceneId`/`SceneNameCatalog`を使用しScene名を直接文字列で記述しない。詳細は4.設計「Scene遷移ユースケース」参照。
- **T-010 セーブ/ロードユースケースの作成(完了)**: `PlayerSessionState`(Application/Session)、`SaveGameSnapshot`/`PlayerSessionStateMapper`/`ISaveRepository`/`SaveResult`/`LoadResult`/`SaveGameUseCase`/`LoadGameUseCase`(Application/Save)を作成。EditModeテスト(`PlayerSessionStateTests`20件、`PlayerSessionStateMapperTests`7件、`SaveLoadUseCaseTests`7件、計34件)を作成し、全件Passed。詳細は4.設計「セーブ/ロードユースケース・PlayerSessionState」参照。
- **T-011 セーブデータ保存基盤の作成(完了)**: `FileSystemSaveStorage`/`JsonSaveRepository`をInfrastructure層(`Assets/_Project/Runtime/Infrastructure/Save/`)に作成。`UnityEngine.JsonUtility`使用のため`SaveGameSnapshot`をpublicフィールド化(4.設計「セーブデータ保存基盤」参照)。Tests.EditMode.asmdefがInfrastructure未参照のため、PlayModeテストで検証。新規Packageは追加していない。
- **T-012 マスターデータ定義の作成(完了)**: `EnemyMasterData`/`ItemMasterData`/`EquipmentMasterData`/`EquipmentSlot`/`MasterDataValidator`をDomain層(`Assets/_Project/Runtime/Domain/MasterData/`)に、`EnemyDefinition`/`ItemDefinition`/`EquipmentDefinition`(ScriptableObject)をInfrastructure層(`Assets/_Project/Runtime/Infrastructure/MasterData/`)に作成。EditModeテスト(`EnemyMasterDataTests`13件、`ItemMasterDataTests`8件、`EquipmentMasterDataTests`10件、`MasterDataValidatorTests`4件、計35件)を作成し、全件Passed。実データAssetはダミー含め作成していない。
- **T-008 Codex第三者レビュー指摘対応(完了、本セッション)**: Minor指摘(`BattleTurnResult.Actions`が`IReadOnlyList`型公開だが実体は内部`List<BattleActionResult>`のため、キャストしてAdd/Clearが可能だった)を解消。内部保持を`ReadOnlyCollection<BattleActionResult>`へ変更し、防御的コピー後にラップした。新規`BattleTurnResultTests`5件を追加(入力List変更の非影響、List/IListキャストでの変更不可、順序・内容維持)。`BattleSession`等は変更していない。EditModeテスト全体は22件→27件。
- **T-009 Codex第三者レビュー指摘対応(完了、本セッション)**: Major指摘(`UnitySceneLoader`が非同期のLoad/UnloadSceneAsyncを開始するだけなのに、`SceneTransitionUseCase`が呼び出し直後に遷移中フラグを解除しており、Unity側の完了前に次の遷移要求を受け付けていた)を解消。`ISceneLoader`をTaskベース(`LoadAsync`/`UnloadAsync`)へ変更し、`SceneTransitionUseCase`を`async Task`化して実際の完了まで`finally`でフラグを保持・解除する構成にした。`UnitySceneLoader`は`AsyncOperation.isDone`/`completed`をTask化し、nullが返る場合は例外化した。`async void`不使用、新規Package追加なし。`SceneTransitionUseCaseTests`を10件→17件に拡充(Load/Unloadそれぞれの二重遷移拒否・完了後許可・失敗後許可・nullTask例外化)。
- **T-011 Codex第三者レビュー指摘対応(完了、本セッション)**: Major指摘(一次保存データがJSONとして読み取れた時点で成功扱いとなり、未対応SaveVersion・MaxHp=0・CurrentHp>MaxHp・不正なSceneId/QuestStateなど意味的に無効なデータでバックアップへフォールバックできていなかった)を解消。`JsonSaveRepository.TryLoad`を「JSON解析成功」→「`PlayerSessionStateMapper.FromSnapshot`相当の意味検証成功」の2段階検証へ変更し、一次候補失敗時のみ同条件でバックアップを検証する構成にした。読込処理は既存ファイルを書き換えない。T-010の`ISaveRepository`公開APIおよび`PlayerSessionStateMapper`/`LoadGameUseCase`は変更していない。`JsonSaveRepositoryTests`を8件→15件に拡充(未対応SaveVersion/MaxHp=0/CurrentHp超過/不正SceneIdでのバックアップ復旧、正常時バックアップ不使用、両方無効時の失敗、読込時の既存ファイル不変性)。PlayModeテスト全体は17件→24件。
- **T-008〜T-012 + Codex指摘対応 テスト実行結果**: Unity Editor Test Runnerで実行し、EditMode 225件(T-003〜T-007の112件+T-008 27件+T-009 17件+T-010 34件+T-012 35件)、PlayMode 24件(T-011)、合計249件。passed 249 / failed 0 / skipped 0、Console Error 0件・Warning 0件をユーザーが実行・確認済み。`ProjectSettings/EditorSettings.asset`の差分なしも確認済み。

### 未完了
- メインクエスト1本・サブクエスト2本の受注/進行/完了条件の実装(T-021/T-022で対応予定)。現状は`VillageSceneInstaller`が`MainQuest`を`NpcInteractable`へ注入するのみで、`Start()`以降の進行・完了処理は未実装。
- 実際のマスターデータAsset(通常敵3種・ボス1体・アイテム・装備・開始時プレイヤーステータス等)は未実装(将来Task想定、推測での先行実装は行っていない)。フィールド/ダンジョンの敵・ボスは引き続きComposition層のプレースホルダー固定値(`RegularEncounterEnemyStats`/`BossEncounterEnemyStats`)。
- 経験値獲得からのレベル再計算・ステータス反映(T-004/T-006とT-008/T-010の統合)は未実装。
- CIの実行結果は本セッションでは未確認。
- `Title.unity`/`GameClear.unity`のUIはPrefab化していない(Codex第三者レビューによりMVPでは必須ではないと判定済み。各Scene専用のため)。
- 遷移失敗時のUI復旧は自動テスト(Fake ISceneLoaderによる例外注入)で検証済みだが、実際のSceneManagerレベルでの失敗(例: Build Settings不整合、Scene破損)を意図的に再現した実機確認はしていない(実Sceneを破壊する検証は危険なため見送った)。
- Retry(GameClearから)は常にBattleへSingle遷移する既存仕様のまま(T-017)であり、Field/Dungeonへの復帰やPendingBattle(通常戦/ボス戦の別)は引き継がない。ボス戦・通常戦問わず、死亡時に何と戦っていたかに関係なく同一のフォールバック敵ステータスで再戦する制限が残っている(T-018〜T-020の完了条件には含まれないため今回は対応していない)。

### 既知の問題
- (解消済み・記録として保持)過去セッションでUnity MCP用ツールが一時的に利用できず、GameObject削除をシーンYAMLの直接編集で行った回があった。現在はUnity MCP接続を再確認済みであり、5.規約「Unity MCP運用方針」により今後はSceneの直接テキスト編集を禁止し、MCP経由での変更を必須とする。
- Unity MCPパッケージ自体に起因すると見られる`[WebSocket] Unexpected receive error`という1件のConsole Warningが、`refresh_unity`実行時などに断続的に発生することがある(ゲーム側のコード・アセットには起因しない)。発生の有無はセッションごとに変動するため、作業前後で都度Console確認を行う。
- `.vscode/`は開発者個人のエディタ設定であり、コミット対象外とする(未追跡のまま維持)。
- Unity Test Framework(および同梱のUnity.PerformanceTesting)は、テスト実行時にそれ自体の内部ログとして`Exception`種別1件("Saving results to: ...")と`Warning`種別2件(`IPrebuildSetup`/`IPostBuildCleanup`実行ログ)をConsoleへ出力することがある。テスト対象コードの不具合ではなく、EditModeテストを実行した場合に付随するUnity側の既知の挙動(テスト自体はすべてPassed)。テスト実行前後でConsoleを確認し、実際のテスト結果(passed/failed/skipped)と合わせて判断する。
- Composition Root導入セッションでの手動確認中、MCP経由で`manage_editor`のpause切替と`execute_code`を短時間に連続実行した際、`PlayerLoop internal function has been called recursively`というUnity Editor内部の警告と、スクリプト参照欠落を示すConsoleメッセージを1件ずつ観測した。直後に全4正式Scene(Title/Village/Battle/GameClear)を`manage_scene validate`した結果はいずれも0件(Missing Script/Broken Prefab)であり、プロジェクトコード・Asset側の永続的な不具合ではなく、MCP自動化ツールによる高速なPlay Mode操作に起因する一過性の事象と判断した。人間が通常操作でPlay Modeに出入りする分には発生しない想定。
- `Assets/Screenshots/`はレビュー証跡(スクリーンショット)の保存先であり、コミット方針が定義されていないためコミット対象外とする(未追跡のまま維持、本番Asset・テストからは参照しない)。
- T-019/T-020手動確認中、Additive Battleロード直後に`The referenced script (Unknown) on this Behaviour is missing!`という警告と`GameServices`/`BattleUIController`内部状態の一時的なnullを1回観測したが、直後の全6正式Scene`manage_scene validate`(0件)および全ロード済みComponentの直接null走査(0件)により、Scene/Prefabアセット自体の不具合ではないことを確認した。MCPツール呼び出し間隔を空けて同一手順を再実行したところ再現しなかった。既存の「MCP自動化ツールによる高速なPlay Mode操作に起因する一過性の事象」と同種と判断した(詳細は4.設計「村エリア・フィールド・ダンジョン本実装」参照)。
- 同じくT-019/T-020手動確認中、2箇所の`SceneTransitionTrigger`へ人間の歩行では起こり得ない速さで連続テレポートさせた際に`InvalidOperationException: A scene transition is already in progress.`を1回観測したが、これは`SceneTransitionUseCase`の再入防止ガード(T-009で意図的に実装済み)が正しく機能した結果であり、直後の状態確認でScene/`GameServices`に不整合がないことを確認済み。通常の徒歩移動速度では再現しない想定。
- Codex指摘対応セッションでの手動確認中、上記と同様の連続テレポートによる`InvalidOperationException`発生時に、Unity Editorの「Error Pause」機能がPlay Modeを自動的に一時停止する事象を観測した。一時停止中は`Time.frameCount`が進行せず物理トリガーの再判定も行われないため、一時停止解除直後にAdditive Battleが二重ロードされ、Console上に`There are 2 event systems`/`There are 2 audio listeners`という警告が一時的に出力された。`manage_editor stop`でPlay Modeを終了した時点でこの状態は破棄され、`Dungeon.unity`等のScene資産は`manage_scene validate`で0件(無傷)を確認済み。落ち着いた操作間隔で同じ手順を再実行したところ再現しなかった。人間の連続的なWASD移動操作ではテレポート自体が発生しないため、実プレイでは再現しないと判断した。
- 本セッションでUnity Editorの`Play`操作を行うたびに、`ProjectSettings/EditorSettings.asset`の`m_EnterPlayModeOptions`が`0`から`1`へUnity Editor自身によって書き換えられる挙動を確認した(本プロジェクトのコード変更によるものではなく、Unity 6のEnter Play Mode Options機能に付随するEditor側の自動書き込み)。セッション終了直前に`git restore`で復元し、差分なしの状態を確認した(4.設計「T-018〜T-020 Codex第三者レビュー指摘対応」参照)。

### 次に行うこと
- T-008〜T-012(`feature/gameplay-application-foundation`ブランチ)はPull Request #5を経て`main`へマージ済み(マージコミット`35ac111`)。
- T-013〜T-017(`feature/presentation-gameplay-slice`ブランチ)はPull Request #6を経て`main`へマージ済み。
- T-018〜T-020(`feature/t018-t020`ブランチ)は完了。実装・テスト・手動確認・PROJECT.md更新が完了済み。Codex第三者レビュー指摘(1回目: Major1件・Minor4件、最終再レビュー: 残存Major1件)への対応もすべて完了済み。Pull Request #7を経て`main`へマージ済み(マージコミット`e49e91d`)。
- Codex再レビューが可能な状態。
- T-021〜T-024(8.実装タスク一覧、4.設計「T-021〜T-024 正式仕様」参照)は**T-022・T-021・T-023・T-024すべて完了**し、Codex第三者レビュー(Critical 0・Major 3・Minor 1)指摘への対応も完了。Pull Request #8を経て`main`へマージ済み(マージコミット`269c360`)。
- T-025〜T-028は`feature/t025-t028-release-slice`ブランチ(`origin/main`の`269c360`から新規作成)で作業中。**T-025(サブクエスト2本の実装)・T-026(アイテム・装備UIの本格実装、採用範囲を限定)・T-027(通し結線・E2E確認)・T-028(README/LICENSEの整備)完了**(4.設計「T-025」「T-026」「T-027」「T-028」各節参照)。全EditMode 398件・全PlayMode 267件Passed、PlayMode3回連続確認済み、正式6Scene検証0件[Missing Script/Missing Reference/Broken Prefab]。T-027はUnity MCP/対話的Editorが利用できない環境だったため、Unity CLIバッチモードでのテスト実行と、実Sceneを実ロードする新規PlayMode E2Eテストで手動通しプレイを代替した。T-028はREADME.mdの全面更新・`.gitignore`の最小追加のみで、LICENSE/Runtime/Scene/Prefab/Asset/metaは変更していない。実装順はTask番号順(T-025 → T-026 → T-027 → T-028)。本ブランチはコミット・push・PR作成・`main`マージ未実施(作業ツリーの変更のみ)。T-029以降には着手していない。
- 将来的にCIへ EditMode Test / PlayMode Test / Unity Build の自動実行を追加する(Unityライセンスの用意が前提)。
- `Assets/Scenes/SampleScene.unity` はAsset自体を保持する(削除しない)が、Build Settingsからは除外済み(正式なBuild開始SceneはTitle、4.設計「Build Settings」参照)。`Bootstrap`は現在のMVP正式Scene一覧には含めない(必要になった場合はPROJECT.md更新・承認後に別Taskで追加する)。
- `Assets/TutorialInfo` は、Unityテンプレートへの依存有無を確認し、不要と証明できた段階で削除する(現段階では削除しない)。

---

## 7. 要承認事項

以下は全7項目とも**承認済み**(承認日: 2026-07-04、人間による方針決定完了)。決定内容の詳細は各項目が参照する節に反映済み。

1. **Unity MCP接続**: 承認済み。実装作業ではUnity MCP接続を必須とする。SceneやPrefabの変更は可能な限りUnity MCP経由で行う。MCP未接続時はSceneを直接テキスト編集しない。MCP未接続のまま実装を継続しない(5.規約「Unity MCP運用方針」参照)。
2. **戦闘Sceneの方式**: 承認済み。フィールドSceneに対するAdditiveロード方式を採用する。戦闘中はフィールド側の入力、カメラ、敵進行を停止する。戦闘終了後はフィールド状態を維持して復帰する。AudioListenerやEventSystemの重複を禁止する(4.設計「Scene構成」参照)。
3. **セーブデータの保存形式**: 承認済み。バージョン付きJSON形式を採用する。MVPでは暗号化しない。一時ファイルへ保存後、正常時に本ファイルへ置換する。バックアップを1世代保持する。破損時はバックアップ復旧または安全な初期状態へ戻す。ScriptableObjectへ実行時状態を保存しない(4.設計「セーブデータ設計方針」参照)。
4. **マスターデータの供給方法**: 承認済み。MVPではScriptableObjectのみを使用する。AddressablesはMVP対象外。Addressables導入を前提とした過剰な抽象化を行わない(4.設計「ScriptableObject方針」参照)。
5. **既存の`Assets/Scenes/SampleScene.unity`の扱い**: 承認済み。現時点では保持する。正式なTitle/Village/Field/Dungeon/Battle/GameClear Sceneが作成・検証された後に削除する(`Bootstrap`は現在のMVP正式Scene一覧には含めない。6.現状参照)。
6. **既存の`Assets/TutorialInfo`一式**: 承認済み。Unityテンプレート依存を確認し、不要と証明できた段階で削除する。現段階では削除しない(6.現状参照)。
7. **イベント通知の実装方式**: 承認済み。局所的な通知にはC# eventを使用する。システム境界にはinterfaceを使用する。グローバルで静的な万能EventBusは作成しない。イベント購読解除漏れを防ぐ(4.設計「イベント通知方針」参照)。

---

## 8. 実装タスク一覧

> 本タスク一覧はPhase 1以降の実装計画。T-001〜T-012は`main`にマージ済み(T-008〜T-012は`feature/gameplay-application-foundation`ブランチ、PR #5)。T-013〜T-017は`feature/presentation-gameplay-slice`ブランチ(PR #6)を経て`main`にマージ済み。T-018〜T-020は`feature/t018-t020`ブランチ(PR #7)を経て`main`にマージ済み。
> **T-021〜T-024(本セッションで正式承認・追加)**: メインクエスト進行・完了ロジック(T-021)、実MasterData Asset作成(T-022)、戦闘報酬・経験値・レベルアップ統合(T-023)、アイテム・装備・所持品管理(T-024)を正式Taskとして追加した。依存関係の都合により実装順は **T-022 → T-021 → T-023 → T-024** とする(Task番号順ではない)。詳細仕様は4.設計「T-021〜T-024 正式仕様」参照。`feature/t021-t024-progression-systems`ブランチで作業する。**T-022・T-021・T-023・T-024すべて完了**。
> **旧T-022〜T-025の繰り下げ**: 上記の正式追加に伴い、旧計画で番号が衝突していたタスクを繰り下げた(内容は変更していない)。旧T-022(サブクエスト2本の実装)→**T-025**、旧T-023(アイテム・装備UIの実装)→**T-026**、旧T-024(通し結線・E2E確認)→**T-027**、旧T-025(README/LICENSE整備)→**T-028**。T-025以降は本セッションでは着手しない。
> Scene/Prefabの変更を伴うタスク(T-001, T-009, T-013〜T-020等)は、5.規約「Unity MCP運用方針」に従いUnity MCP接続を前提として実施する。

| Task ID | 目的 | 変更対象 | 完了条件 | 確認方法 | 依存タスク |
|---------|------|----------|----------|----------|------------|
| T-001 | プロジェクト基盤ディレクトリの作成(完了) | `Assets/_Project/Runtime/{Domain,Application,Presentation,Infrastructure}`, `Assets/_Project/Editor`, `Assets/_Project/Tests/{EditMode,PlayMode}`, `Assets/_Project/{Scenes,Prefabs,ScriptableObjects,UI,Art,Audio,Settings}` | 上記フォルダがすべて作成され、空でもUnityにエラーなく認識される | Unity Editorでフォルダ構成を目視確認、Consoleにエラーが出ないこと | なし |
| T-002 | レイヤー別asmdefの作成(完了・コミット済み: `a823f77`) | `FloatingIslandsRpg.Domain.asmdef`, `FloatingIslandsRpg.Application.asmdef`, `FloatingIslandsRpg.Infrastructure.asmdef`, `FloatingIslandsRpg.Presentation.asmdef`, `FloatingIslandsRpg.Editor.asmdef`, `FloatingIslandsRpg.Tests.EditMode.asmdef`, `FloatingIslandsRpg.Tests.PlayMode.asmdef` | 7個のasmdefが作成され、依存方向(4.設計参照)通りに参照設定されている | Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-001 |
| T-003 | Scene識別子・Scene名定義の作成(完了、Codex第三者レビュー指摘対応完了) | `Assets/_Project/Runtime/Application/Scenes/SceneId.cs`, `SceneNameCatalog.cs`, `Assets/_Project/Tests/EditMode/Scenes/SceneNameCatalogTests.cs` | マジックストリングでのSceneManager呼び出しを避けられる定義が用意されている。SceneIdはPROJECT.md「3.仕様 Scene一覧」の正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)と一致する | EditModeテスト11件がPassed、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-002 |
| T-004 | ステータス計算ロジック(Domain)(完了、Codex第三者レビューMinor指摘対応完了) | `Assets/_Project/Runtime/Domain/Characters/Stats/CharacterStats.cs`, `StatGrowthProfile.cs`, `CharacterStatsCalculator.cs`, `Assets/_Project/Tests/EditMode/Characters/Stats/`配下のEditModeテスト3ファイル | レベル1(MinLevel)〜プロファイルごとのMaxLevelまでのステータスが決定的に計算できる。MinLevelが1以外の場合も回帰テストで検証済み | EditModeテスト33件がPassed、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-002 |
| T-005 | 戦闘計算ロジック(Domain)(完了・`main`にマージ済み、Codex第三者レビューMajor指摘対応完了) | `Assets/_Project/Runtime/Domain/Combat/CombatCalculator.cs`, `Assets/_Project/Tests/EditMode/Combat/CombatCalculatorTests.cs` | 攻撃側/防御側のステータスからダメージ量・行動順が一意に決定できる。`ResolveHit`のhitChance/randomRollは非数値(NaN/Infinity)を拒否し、randomRollの有効範囲は`[0.0, 1.0)`とする | EditModeテスト40件がPassed、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-004 |
| T-006 | 経験値・レベルアップ計算(Domain)(完了・`main`にマージ済み、Codex第三者レビュー合格済み) | `Assets/_Project/Runtime/Domain/Progression/ExperienceTable.cs`, `LevelUpCalculator.cs`, `Assets/_Project/Tests/EditMode/Progression/`配下のEditModeテスト2ファイル | 経験値加算により正しいタイミングでレベルアップが発生する | EditModeテスト20件がPassed、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-004 |
| T-007 | クエスト状態管理(Domain)(完了・`main`にマージ済み、Codex第三者レビュー合格済み) | `Assets/_Project/Runtime/Domain/Quests/QuestState.cs`, `QuestProgress.cs`, `Assets/_Project/Tests/EditMode/Quests/QuestProgressTests.cs` | メイン1本・サブ2本分の状態遷移が矛盾なく行える | EditModeテスト8件がPassed、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-002 |
| T-008 | 戦闘進行ユースケース(Application)(完了、Codex第三者レビューMinor指摘対応完了) | `Assets/_Project/Runtime/Application/Battle/`配下(`BattleSession`等7ファイル)、`Assets/_Project/Tests/EditMode/Battle/`配下のEditModeテスト3ファイル | コマンド入力から勝利/敗北いずれかの結果が返るまで一連の流れが完結する。`BattleTurnResult.Actions`は外部から変更不可 | EditModeテスト27件がPassed、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-005 |
| T-009 | Scene遷移ユースケース(Application)(完了、Codex第三者レビューMajor指摘対応完了) | `Assets/_Project/Runtime/Application/Scenes/`配下(`ISceneLoader`, `SceneLoadMode`, `SceneTransitionUseCase`)、`Assets/_Project/Runtime/Infrastructure/Scenes/UnitySceneLoader.cs`、`Assets/_Project/Tests/EditMode/Scenes/SceneTransitionUseCaseTests.cs` | 村→フィールド→ダンジョン→戦闘→復帰の遷移がコード上で表現できる。実際のロード/アンロード完了まで遷移中状態を維持する | EditModeテスト17件がPassed(Fake `ISceneLoader`使用)、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-003 |
| T-010 | セーブ/ロードユースケース(Application)(完了) | `Assets/_Project/Runtime/Application/Session/PlayerSessionState.cs`、`Assets/_Project/Runtime/Application/Save/`配下(7ファイル)、`Assets/_Project/Tests/EditMode/Session/`, `Assets/_Project/Tests/EditMode/Save/`配下のEditModeテスト3ファイル | セーブしたデータをロードした際に元の状態と一致する | EditModeテスト34件がPassed、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-004, T-007 |
| T-011 | セーブデータ保存基盤(Infrastructure)(完了、Codex第三者レビューMajor指摘対応完了) | `Assets/_Project/Runtime/Infrastructure/Save/`配下(`FileSystemSaveStorage.cs`, `JsonSaveRepository.cs`)、`Assets/_Project/Tests/PlayMode/Save/`配下のPlayModeテスト2ファイル | ファイルの書き込み・読み込みが成功し、破損データ読込時はバックアップ復旧または安全な初期状態へ戻せる。意味検証を通過した候補のみを有効とする | PlayModeテスト24件がPassed(Tests.EditMode.asmdefがInfrastructure未参照のためPlayModeで検証)、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-010 |
| T-012 | 敵/アイテム/装備マスターデータ定義(Infrastructure)(完了) | `Assets/_Project/Runtime/Domain/MasterData/`配下(5ファイル)、`Assets/_Project/Runtime/Infrastructure/MasterData/`配下(3ファイル)、`Assets/_Project/Tests/EditMode/MasterData/`配下のEditModeテスト4ファイル | 通常敵3種、ボス1体、アイテム、装備のデータ定義クラスが用意されている | EditModeテスト35件がPassed(検証ロジックのDomain層テスト)、実データAssetの作成はUnity Editorでの手動確認対象 | T-002 |
| T-013 | プレイヤー移動・カメラ(Presentation)(完了) | 新Input Systemを用いた3D移動、追従カメラ | フィールド上でプレイヤーが移動でき、カメラが追従する | PlayModeテスト6件がPassed、手動プレイで移動・カメラ挙動を確認済み | T-001 |
| T-014 | NPC会話UI(Presentation)(完了) | 会話ウィンドウ、テキスト送り | NPCに話しかけると会話ウィンドウが開き、読み進められる | PlayModeテスト22件がPassed、手動プレイで会話開始〜終了までを確認済み | T-013 |
| T-015 | 戦闘UI(Presentation)(完了) | コマンド選択UI、HP/MP表示、戦闘ログ | コマンド入力でT-008のユースケースを呼び出し、結果が画面に反映される | PlayModeテスト8件がPassed、手動プレイで1戦闘を最初から最後まで実行し確認済み | T-008, T-013 |
| T-016 | タイトル画面(Presentation)(完了) | はじめから/つづきからの選択UI | 「はじめから」で新規開始、「つづきから」でT-011のセーブデータをロードできる | PlayModeテスト9件がPassed、手動プレイで両方の分岐を確認済み | T-011 |
| T-017 | ゲームクリア/ゲームオーバー画面(Presentation)(完了) | クリア時・全滅時の専用画面 | ボス撃破でクリア画面、全滅でゲームオーバー画面が表示される | PlayModeテスト7件がPassed、手動プレイで両方のケースを確認済み | T-008, T-016 |
| T-018 | 村エリアの実装(完了) | Village Scene、NPC3体以上、フィールドへの接続 | 村シーンが単独でロード可能で、NPC会話とフィールドへの移動ができる | 手動プレイでシーン内を一巡して確認 | T-013, T-014 |
| T-019 | フィールドエリアの実装(完了) | Field Scene、通常敵エンカウント、ダンジョン入口 | フィールドを探索でき、エンカウントが発生し、ダンジョンへ入れる | 手動プレイでエンカウント発生とダンジョン入口到達を確認 | T-015, T-018 |
| T-020 | ダンジョンの実装(完了) | Dungeon Scene、通常敵エンカウント、ボス部屋 | ダンジョンを進めて道中戦闘を経てボスに到達できる | 手動プレイで入口からボス部屋まで到達を確認 | T-019 |
| T-021 | メインクエスト進行・完了ロジック(完了) | `Domain/Quests/`拡張(`MainQuestStage`/`MainQuestProgress`)、`Application/Quests/`新設UseCase、Village NPC/Field/Dungeon/Boss/GameClear接続 | Village開始→Field到達→Dungeon到達→Boss撃破の順でのみ進行し、スキップ・逆行・重複進行・条件未達完了を拒否する。Boss撃破済みかつMainQuest Completedの両方を満たした場合のみGameClearへ遷移する | EditModeテスト新規32件・PlayModeテスト新規12件がPassed(全EditMode266件/全PlayMode184件Passed、PlayMode3回連続確認済み)、正式6Scene`manage_scene validate`でMissing Script/Broken Prefab 0件 | T-007, T-014, T-018, T-019, T-020 |
| T-022 | 実MasterData Asset作成(完了) | `Assets/_Project/ScriptableObjects/`配下の実Asset群、Composition層(`BattleSceneInstaller`, `TitleSceneInstaller`等)の固定値置換 | 通常敵3種・ボス1種・回復アイテム2種・武器2種・防具2種・メインクエスト定義1件・プレイヤー初期データ1件のAssetがID一意・必須値設定済み・数値範囲正常でMasterDataValidatorを通過する。通常敵/ボス/初期プレイヤーの固定値をAsset参照へ置換し、参照不足時は隠れたフォールバックをせず明確なエラーで安全に停止する | EditModeテスト新規9件・PlayModeテスト新規10件がPassed(全EditMode234件/全PlayMode172件Passed、PlayMode3回連続確認済み)、正式6Scene`manage_scene validate`でMissing Script/Broken Prefab 0件 | T-012 |
| T-023 | 戦闘報酬・経験値・レベルアップ統合(完了) | `Application/Progression/`新設(`GrantBattleRewardUseCase`等)でのEnemy MasterData RewardExperience取得・`PlayerSessionState`経験値加算・レベル/CharacterStats再計算 | 通常戦・Boss戦勝利で経験値を1回だけ加算し、閾値到達でレベルとCharacterStatsを更新する。敗北時は経験値を付与しない。最大レベルを超えず、オーバーフローを安全に扱う。Retryやイベント多重発火で重複取得しない | EditModeテスト新規18件・PlayModeテスト新規11件がPassed(全EditMode284件/全PlayMode195件Passed、PlayMode3回連続確認済み) | T-006, T-022 |
| T-024 | アイテム・装備・所持品管理(完了) | `Domain`/`Application`新設(`Inventory`, `EquipmentLoadout`, `EquipmentStatCalculator`等)、Field/Dungeon/戦闘報酬からの取得経路、`PlayerSessionState`/セーブ拡張(SaveVersion 2→3) | ItemId単位の数量管理(追加・消費・数量不足拒否・負数拒否・不正ID拒否)、Weapon/Armorの装備・解除(カテゴリ不一致・未所持拒否)、装備補正のCharacterStatsへの反映(二重加算なし)。Sceneをまたいで状態を保持し、New Game/Continue/Retryで不正に増減しない | EditMode/PlayModeテストでAdd/Consume/Equip/Unequip/拒否ケース/二重加算防止/Save・Load往復/Retryを検証。EditModeテスト新規77件・PlayModeテスト新規19件がPassed(全EditMode361件/全PlayMode214件Passed、PlayMode3回連続確認済み) | T-012, T-022, T-023 |
| T-025 | サブクエスト2本の実装(完了) | `Application/Quests/`新設(`StartSubQuestUseCase`, `CompleteSubQuestUseCase`)、Village既存NPC(Villager/Merchant)への`_subQuest1Giver`/`_subQuest2Giver`配線、Field/Dungeon到達での自動完了 | メインクエストと独立に受注・完了できる | EditModeテスト新規12件・PlayModeテスト新規14件がPassed(全EditMode392件/全PlayMode249件Passed、PlayMode3回連続確認済み)、手動プレイでMainQuest未着手のまま両サブクエストを受注・完了できることを確認 | T-007, T-018 |
| T-026 | アイテム・装備UIの本格実装(完了、採用範囲を限定) | `Presentation/Menu/`新設(`GameMenuController`, `MenuActivityGate`等)、`Composition/Scenes/MenuInstaller`新設、共有Prefab`GameMenu.prefab`をVillage/Field/Dungeonへ配置。ショップ・クラフトは対象外(4.設計「T-026」参照) | アイテム使用・装備変更がUI上で快適に操作できる | EditModeテスト新規0件・PlayModeテスト新規28件がPassed(全EditMode392件/全PlayMode266件Passed、PlayMode3回連続確認済み)、手動プレイでVillage/Field/Dungeonの共有メニューによるアイテム使用・装備変更・PlayerMovement等の停止/再開を確認 | T-015, T-024 |
| T-027 | 通し結線・E2E確認(完了) | `Tests/EditMode/EndToEnd/FullFlowIntegrationTests.cs`新規6件、`Tests/PlayMode/EndToEnd/FullPlaythroughE2ETests.cs`新規1件(実SceneをロードするE2Eテスト)、`Editor/SceneWiringValidator.cs`新設、既存`PlayerMovementTests.cs`のCLIバッチモード安定化 | タイトル→村→フィールド→ダンジョン→ボス撃破→クリア画面が、Consoleにエラーを出さず完走できる | 全EditMode398件・全PlayMode267件Passed(PlayMode3回連続確認済み)、正式6ScenesすべてMissing Script/Missing Reference/Broken Prefab 0件、Unity MCP不使用のためCLIバッチモードで実行した実SceneベースPlayModeテストが手動通しプレイを代替 | T-016〜T-026 |
| T-028 | READMEおよびLICENSEの整備(完了) | `README.md`(全面更新)、`LICENSE`(変更なし、既存MIT維持)、`.gitignore`(ローカル生成物2件の除外を追加) | プロジェクト概要・セットアップ手順が記載されたREADMEと、ライセンス方針に沿ったLICENSEが存在する | README.md/LICENSEの内容レビュー、`git diff --check`によるtrailing whitespace確認、`git diff -- LICENSE/ProjectSettings/Packages`で対象外差分がないことを確認 | なし |
