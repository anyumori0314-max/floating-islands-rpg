# PROJECT.md

> floating-islands-rpg の設計方針・スコープ・実装タスクを一元管理するドキュメント。
> 最終更新: 2026-07-04 (T-004 キャラクターステータス計算ロジック完了を反映)

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

現在存在する実体は `Assets/Scenes/SampleScene.unity` のみで、上記はすべて未作成。

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
- パーティ全滅でゲームオーバー画面へ遷移し、直近のセーブ地点からの再開を選べる。

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
- **T-003: 完了(`feature/scene-identifiers`ブランチ、Codex第三者レビュー指摘対応完了、未コミット)**。Scene識別子`SceneId`とScene名解決`SceneNameCatalog`をApplication層に作成。PROJECT.md「3.仕様 Scene一覧」の正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)へ統一済み。EditModeテスト11件すべてPassed(Unity Editor Test Runnerで実行確認済み)。
- **T-004: 完了(`feature/character-stats`ブランチ、未コミット)**。キャラクターステータス計算ロジック(`CharacterStats`, `StatGrowthProfile`, `CharacterStatsCalculator`)をDomain層に作成。EditModeテスト31件すべてPassed(Unity Editor Test Runnerで実行確認済み)。
- **ゲーム実装**: Scene識別子(T-003)、キャラクターステータス計算(T-004)以外のゲーム機能・C#クラス(Infrastructure/Presentationの実装コード、およびDomainのその他のロジック)は未実装。

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
- **T-003 Scene識別子・Scene名定義の作成(完了)**: `SceneId` (enum) と `SceneNameCatalog` (静的クラス) をApplication層に作成。EditModeテスト`SceneNameCatalogTests`を作成。詳細は4.設計「Scene識別子」参照。**現時点では未コミット**(`feature/scene-identifiers`ブランチのワーキングツリーに存在)。
- **T-003 Codex第三者レビュー指摘対応(完了、本セッション)**: Major指摘(`SceneId`/`SceneNameCatalog`がPROJECT.md正式Scene一覧と不一致。`Sample`/`Bootstrap`を含み`Village`/`Dungeon`を欠いていた)を解消し、`Title`/`Village`/`Field`/`Dungeon`/`Battle`/`GameClear`の6件へ統一。Minor指摘(Scene名検証が`string.IsNullOrEmpty`で空白文字列を検出できない)を`string.IsNullOrWhiteSpace`へ修正。テストはScene名個別対応6件(Title/Village/Field/Dungeon/Battle/GameClear)とenum/カタログ過不足検証2件の計8件を追加し、`SceneId.Sample`専用テスト1件を削除、既存の空白検証テストを`IsNullOrWhiteSpace`化。Unity Editor Test Runnerで全11件がPassed(failed 0, skipped 0)であることをユーザーが実行・確認済み。詳細は4.設計「Scene識別子」参照。
- **T-004 キャラクターステータス計算ロジックの作成(完了)**: `CharacterStats`(不変の値オブジェクト)、`StatGrowthProfile`(基礎値・成長値・レベル範囲プロファイル)、`CharacterStatsCalculator`(静的計算関数)をDomain層(`Assets/_Project/Runtime/Domain/Characters/Stats/`)に作成。採用ステータスは`MaxHp`/`MaxMp`/`Attack`/`Defense`/`Agility`/`Magic`の6種+`Level`。成長計算式は`stat = baseValue + perLevelGrowth * (level - profile.MinLevel)`、レベル上限は`StatGrowthProfile.MaxLevel`としてプロファイルごとに指定可能。EditModeテスト(`CharacterStatsTests`8件、`StatGrowthProfileTests`14件、`CharacterStatsCalculatorTests`9件、計31件)を作成し、Unity Editor Test Runnerで全件Passed(failed 0, skipped 0)、Console Error 0件・Warning 0件をユーザーが実行・確認済み。詳細は4.設計「キャラクターステータス計算」参照。**現時点では未コミット**(`feature/character-stats`ブランチのワーキングツリーに存在)。

### 未完了
- Domain/Infrastructure/Presentationの実装コード(C#クラス)が1つも存在しない(SceneId/SceneNameCatalog以外はasmdefの外枠のみ)。
- ゲームロジック・UI・Prefab・ScriptableObjectは一切未実装。
- PlayModeテストコードが1つも存在しない(Test Assembly自体はT-002で作成済み)。
- T-003の変更(`SceneId.cs`, `SceneNameCatalog.cs`, `SceneNameCatalogTests.cs`)が未コミット(現在の作業ブランチ`feature/scene-identifiers`のワーキングツリーに存在)。
- `feature/scene-identifiers`ブランチが`origin`へ未push。
- CIの実行結果は本セッションでは未確認。
- T-004の変更(`CharacterStats.cs`, `StatGrowthProfile.cs`, `CharacterStatsCalculator.cs`, および対応するEditModeテスト3ファイル)が未コミット(現在の作業ブランチ`feature/character-stats`のワーキングツリーに存在)。
- `feature/character-stats`ブランチが`origin`へ未push。

### 既知の問題
- (解消済み・記録として保持)過去セッションでUnity MCP用ツールが一時的に利用できず、GameObject削除をシーンYAMLの直接編集で行った回があった。現在はUnity MCP接続を再確認済みであり、5.規約「Unity MCP運用方針」により今後はSceneの直接テキスト編集を禁止し、MCP経由での変更を必須とする。
- Unity MCPパッケージ自体に起因すると見られる`[WebSocket] Unexpected receive error`という1件のConsole Warningが、`refresh_unity`実行時などに断続的に発生することがある(ゲーム側のコード・アセットには起因しない)。発生の有無はセッションごとに変動するため、作業前後で都度Console確認を行う。
- Unity Test Framework(および同梱のUnity.PerformanceTesting)は、テスト実行時にそれ自体の内部ログとして`Exception`種別1件("Saving results to: ...")と`Warning`種別2件(`IPrebuildSetup`/`IPostBuildCleanup`実行ログ)をConsoleへ出力することがある。テスト対象コードの不具合ではなく、EditModeテストを実行した場合に付随するUnity側の既知の挙動(テスト自体はすべてPassed)。テスト実行前後でConsoleを確認し、実際のテスト結果(passed/failed/skipped)と合わせて判断する。

### 次に行うこと
- 本セッションのT-003の変更(`SceneId.cs`, `SceneNameCatalog.cs`, `SceneNameCatalogTests.cs`, PROJECT.md更新分)をコミットする(人間の判断・実行を待つ)。
- `feature/scene-identifiers`ブランチを`origin`へpushする。
- push後、CIが正しく実行され成功することを確認する。
- Pull Requestを作成し、レビューを経て`main`へマージする。
- T-004(ステータス計算ロジック)が完了したため、次のタスクは **T-005(戦闘計算ロジック)**(T-004に依存)。**T-009(Scene遷移ユースケース)** もT-003完了により引き続き着手可能。
- 将来的にCIへ EditMode Test / PlayMode Test / Unity Build の自動実行を追加する(Unityライセンスの用意が前提)。
- `Assets/Scenes/SampleScene.unity` は、正式なTitle/Village/Field/Dungeon/Battle/GameClear Sceneが作成・検証されるまで保持する(削除しない)。`Bootstrap`は現在のMVP正式Scene一覧には含めない(必要になった場合はPROJECT.md更新・承認後に別Taskで追加する)。
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

> 本タスク一覧はPhase 1以降の実装計画。T-001(基盤ディレクトリ作成)・T-002(レイヤー別asmdef作成)は`main`にマージ済み。T-003(Scene識別子・Scene名定義)は完了(`feature/scene-identifiers`ブランチ、未コミット)。T-004(ステータス計算ロジック)も完了(`feature/character-stats`ブランチ、未コミット)。次はT-005以降に進める状態。
> Scene/Prefabの変更を伴うタスク(T-001, T-009, T-013〜T-020等)は、5.規約「Unity MCP運用方針」に従いUnity MCP接続を前提として実施する。

| Task ID | 目的 | 変更対象 | 完了条件 | 確認方法 | 依存タスク |
|---------|------|----------|----------|----------|------------|
| T-001 | プロジェクト基盤ディレクトリの作成(完了) | `Assets/_Project/Runtime/{Domain,Application,Presentation,Infrastructure}`, `Assets/_Project/Editor`, `Assets/_Project/Tests/{EditMode,PlayMode}`, `Assets/_Project/{Scenes,Prefabs,ScriptableObjects,UI,Art,Audio,Settings}` | 上記フォルダがすべて作成され、空でもUnityにエラーなく認識される | Unity Editorでフォルダ構成を目視確認、Consoleにエラーが出ないこと | なし |
| T-002 | レイヤー別asmdefの作成(完了・コミット済み: `a823f77`) | `FloatingIslandsRpg.Domain.asmdef`, `FloatingIslandsRpg.Application.asmdef`, `FloatingIslandsRpg.Infrastructure.asmdef`, `FloatingIslandsRpg.Presentation.asmdef`, `FloatingIslandsRpg.Editor.asmdef`, `FloatingIslandsRpg.Tests.EditMode.asmdef`, `FloatingIslandsRpg.Tests.PlayMode.asmdef` | 7個のasmdefが作成され、依存方向(4.設計参照)通りに参照設定されている | Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-001 |
| T-003 | Scene識別子・Scene名定義の作成(完了、Codex第三者レビュー指摘対応完了) | `Assets/_Project/Runtime/Application/Scenes/SceneId.cs`, `SceneNameCatalog.cs`, `Assets/_Project/Tests/EditMode/Scenes/SceneNameCatalogTests.cs` | マジックストリングでのSceneManager呼び出しを避けられる定義が用意されている。SceneIdはPROJECT.md「3.仕様 Scene一覧」の正式6Scene(Title/Village/Field/Dungeon/Battle/GameClear)と一致する | EditModeテスト11件がPassed、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-002 |
| T-004 | ステータス計算ロジック(Domain)(完了) | `Assets/_Project/Runtime/Domain/Characters/Stats/CharacterStats.cs`, `StatGrowthProfile.cs`, `CharacterStatsCalculator.cs`, `Assets/_Project/Tests/EditMode/Characters/Stats/`配下のEditModeテスト3ファイル | レベル1(MinLevel)〜プロファイルごとのMaxLevelまでのステータスが決定的に計算できる | EditModeテスト31件がPassed、Unity Editorでコンパイルが通り、Consoleにエラーが出ないこと | T-002 |
| T-005 | 戦闘計算ロジック(Domain) | ダメージ計算、命中/回避、行動順決定 | 攻撃側/防御側のステータスからダメージ量・行動順が一意に決定できる | EditModeテストで既知の入力に対する出力を検証 | T-004 |
| T-006 | 経験値・レベルアップ計算(Domain) | 経験値テーブル、レベルアップ判定 | 経験値加算により正しいタイミングでレベルアップが発生する | EditModeテストで境界値(閾値ちょうど等)を検証 | T-004 |
| T-007 | クエスト状態管理(Domain) | クエストの未受注/進行中/完了の状態遷移 | メイン1本・サブ2本分の状態遷移が矛盾なく行える | EditModeテストで状態遷移の網羅的なパターンを検証 | T-002 |
| T-008 | 戦闘進行ユースケース(Application) | ターン制戦闘のコマンド受付〜勝敗判定までの進行制御 | コマンド入力から勝利/敗北いずれかの結果が返るまで一連の流れが完結する | EditModeテストでシナリオ(勝利/全滅)を検証 | T-005 |
| T-009 | Scene遷移ユースケース(Application) | エリア間・戦闘への遷移制御 | 村→フィールド→ダンジョン→戦闘→復帰の遷移がコード上で表現できる | PlayModeテストまたは手動確認でシーン遷移がエラーなく行われる | T-003 |
| T-010 | セーブ/ロードユースケース(Application) | パーティ・クエスト進行等のシリアライズ/デシリアライズ | セーブしたデータをロードした際に元の状態と一致する | EditModeテストでセーブ→ロードの往復一致を検証 | T-004, T-007 |
| T-011 | セーブデータ保存基盤(Infrastructure) | `Application.persistentDataPath`へのバージョン付きJSON読み書き(一時ファイル→本ファイル置換、1世代バックアップ) | ファイルの書き込み・読み込みが成功し、破損データ読込時はバックアップ復旧または安全な初期状態へ戻せる | EditModeテストで正常系・破損データ系(バックアップ復旧含む)の両方を検証 | T-010 |
| T-012 | 敵/アイテム/装備マスターデータ定義(Infrastructure) | ScriptableObjectベースのマスターデータクラス(Addressables不使用) | 通常敵3種、ボス1体、アイテム、装備のデータ定義クラスが用意されている | Unity Editorでアセット作成が可能なことを確認 | T-002 |
| T-013 | プレイヤー移動・カメラ(Presentation) | 新Input Systemを用いた3D移動、追従カメラ | フィールド上でプレイヤーが移動でき、カメラが追従する | 手動プレイで移動・カメラ挙動を確認 | T-001 |
| T-014 | NPC会話UI(Presentation) | 会話ウィンドウ、テキスト送り | NPCに話しかけると会話ウィンドウが開き、読み進められる | 手動プレイで会話開始〜終了までを確認 | T-013 |
| T-015 | 戦闘UI(Presentation) | コマンド選択UI、HP/MP表示、戦闘ログ | コマンド入力でT-008のユースケースを呼び出し、結果が画面に反映される | 手動プレイで1戦闘を最初から最後まで実行し確認 | T-008, T-013 |
| T-016 | タイトル画面(Presentation) | はじめから/つづきからの選択UI | 「はじめから」で新規開始、「つづきから」でT-011のセーブデータをロードできる | 手動プレイで両方の分岐を確認 | T-011 |
| T-017 | ゲームクリア/ゲームオーバー画面(Presentation) | クリア時・全滅時の専用画面 | ボス撃破でクリア画面、全滅でゲームオーバー画面が表示される | 手動プレイで両方のケースを確認 | T-008, T-016 |
| T-018 | 村エリアの実装 | Village Scene、NPC3体以上、フィールドへの接続 | 村シーンが単独でロード可能で、NPC会話とフィールドへの移動ができる | 手動プレイでシーン内を一巡して確認 | T-013, T-014 |
| T-019 | フィールドエリアの実装 | Field Scene、通常敵エンカウント、ダンジョン入口 | フィールドを探索でき、エンカウントが発生し、ダンジョンへ入れる | 手動プレイでエンカウント発生とダンジョン入口到達を確認 | T-015, T-018 |
| T-020 | ダンジョンの実装 | Dungeon Scene、通常敵エンカウント、ボス部屋 | ダンジョンを進めて道中戦闘を経てボスに到達できる | 手動プレイで入口からボス部屋まで到達を確認 | T-019 |
| T-021 | メインクエストの実装 | メインクエスト1本のトリガー・進行・完了 | 村での受注からボス撃破での完了までが一連で成立する | 手動プレイで開始から完了までを確認 | T-007, T-009, T-020 |
| T-022 | サブクエスト2本の実装 | サブクエスト2本のトリガー・進行・完了 | メインクエストと独立に受注・完了できる | 手動プレイでメインクエストと絡めず完了できることを確認 | T-007, T-018 |
| T-023 | アイテム・装備システムの実装 | インベントリUI、装備切り替えUI | アイテム使用・装備変更がステータス/戦闘に反映される | 手動プレイで装備変更前後のステータス変化と、アイテム使用効果を確認 | T-012, T-015 |
| T-024 | 通し結線・E2E確認 | 上記全タスクの統合 | タイトル→村→フィールド→ダンジョン→ボス撃破→クリア画面が、Consoleにエラーを出さず30〜60分で完走できる | 手動プレイでの通しプレイ、およびConsole監視 | T-016〜T-023 |
| T-025 | READMEおよびLICENSEの整備 | `README.md`, `LICENSE` | プロジェクト概要・セットアップ手順が記載されたREADMEと、ライセンス方針に沿ったLICENSEが存在する | ファイルの存在とNotion内容のレビュー | なし |
