# PROJECT.md

> floating-islands-rpg の設計方針・スコープ・実装タスクを一元管理するドキュメント。
> 最終更新: 2026-07-05 (T-013〜T-017完了、およびCodex第三者レビュー指摘[Major4件・Minor1件]対応(Composition Root新設等)を反映。`feature/presentation-gameplay-slice`ブランチ、`main`へは未マージ・未コミット)

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

**現状(2026-07-05更新)**: `Title`/`Village`/`Battle`/`GameClear`の4Sceneが`Assets/_Project/Scenes/`に実体として作成済み(Title/GameClearはT-016/T-017、Villageは配線検証用の最小スキャフォールド[NPC1体、T-018の完了条件は未達]、BattleはT-015の本番配線用に作成)。`Field`/`Dungeon`は未作成(T-019/T-020で対応予定)。旧`Assets/Scenes/SampleScene.unity`はAsset自体を保持しているが、Build Settingsからは除外済み(下記「Build Settings」参照)。

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

### Build Settings(Codex第三者レビューMajor 4指摘、および最終再レビュー指摘対応)
- `ProjectSettings/EditorBuildSettings.asset`を、Unity MCP(`manage_build` action=scenes)経由で以下の順序へ更新した(直接テキスト編集はしていない)。
  - Build Index 0: `Title`
  - Build Index 1: `Village`
  - Build Index 2: `Battle`
  - Build Index 3: `GameClear`
- `SampleScene`はBuild Settingsから除外した(Build実行時に正式なTitleから開始されるようにするため)。Field/Dungeonは未作成のため登録していない。
- `SampleScene.unity`のAsset自体は5.規約・7.要承認事項の既定方針により削除していない(Build対象外になっただけで、Assetとしては保持)。
- `SceneNameCatalog`(T-003)が保持するScene名(`Title`/`Village`/`Field`/`Dungeon`/`Battle`/`GameClear`)とBuild Settings登録パスのScene名は一致している。

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
- **T-013: 完了(`feature/presentation-gameplay-slice`ブランチ、`main`へ未マージ・未コミット)**。プレイヤー移動・追従カメラ(`PlayerMovement`, `FollowCamera`)をPresentation層に作成。PlayModeテスト6件すべてPassed。
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
- **ゲーム実装**: T-003〜T-012(Scene識別子、キャラクターステータス計算、戦闘計算・進行、経験値・レベルアップ計算、クエスト状態管理、Scene遷移、セーブ/ロード、マスターデータ定義)に加え、T-013〜T-017でPresentation層の実装コード・Composition Root・Title/Village/Battle/GameClear Scene(Village/Battleは最小スキャフォールド)・EditMode/PlayModeテストが追加された。Field/Dungeon Sceneの実体、および実データAsset(敵/アイテム/装備・開始時ステータス等)は未実装(T-018以降で対応予定)。

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
- Field/Dungeon Sceneの実体、および実データAsset(敵/アイテム/装備・開始時プレイヤーステータス等)は未実装(T-018以降で対応予定)。`Village.unity`は配線検証専用の最小スキャフォールド(NPC1体)であり、T-018の完了条件(NPC3体以上、Field接続)は満たしていない。
- CIの実行結果は本セッションでは未確認。
- 経験値獲得からのレベル再計算・ステータス反映(T-004/T-006とT-008/T-010の統合)、実際のマスターデータAssetの作成は未実装(将来Task想定、推測での先行実装は行っていない)。
- `Title.unity`/`GameClear.unity`のUIはPrefab化していない(Codex第三者レビューによりMVPでは必須ではないと判定済み。各Scene専用のため)。
- 遷移失敗時のUI復旧は自動テスト(Fake ISceneLoaderによる例外注入)で検証済みだが、実際のSceneManagerレベルでの失敗(例: Build Settings不整合、Scene破損)を意図的に再現した実機確認はしていない(実Sceneを破壊する検証は危険なため見送った)。

### 既知の問題
- (解消済み・記録として保持)過去セッションでUnity MCP用ツールが一時的に利用できず、GameObject削除をシーンYAMLの直接編集で行った回があった。現在はUnity MCP接続を再確認済みであり、5.規約「Unity MCP運用方針」により今後はSceneの直接テキスト編集を禁止し、MCP経由での変更を必須とする。
- Unity MCPパッケージ自体に起因すると見られる`[WebSocket] Unexpected receive error`という1件のConsole Warningが、`refresh_unity`実行時などに断続的に発生することがある(ゲーム側のコード・アセットには起因しない)。発生の有無はセッションごとに変動するため、作業前後で都度Console確認を行う。
- `.vscode/`は開発者個人のエディタ設定であり、コミット対象外とする(未追跡のまま維持)。
- Unity Test Framework(および同梱のUnity.PerformanceTesting)は、テスト実行時にそれ自体の内部ログとして`Exception`種別1件("Saving results to: ...")と`Warning`種別2件(`IPrebuildSetup`/`IPostBuildCleanup`実行ログ)をConsoleへ出力することがある。テスト対象コードの不具合ではなく、EditModeテストを実行した場合に付随するUnity側の既知の挙動(テスト自体はすべてPassed)。テスト実行前後でConsoleを確認し、実際のテスト結果(passed/failed/skipped)と合わせて判断する。
- Composition Root導入セッションでの手動確認中、MCP経由で`manage_editor`のpause切替と`execute_code`を短時間に連続実行した際、`PlayerLoop internal function has been called recursively`というUnity Editor内部の警告と、スクリプト参照欠落を示すConsoleメッセージを1件ずつ観測した。直後に全4正式Scene(Title/Village/Battle/GameClear)を`manage_scene validate`した結果はいずれも0件(Missing Script/Broken Prefab)であり、プロジェクトコード・Asset側の永続的な不具合ではなく、MCP自動化ツールによる高速なPlay Mode操作に起因する一過性の事象と判断した。人間が通常操作でPlay Modeに出入りする分には発生しない想定。
- `Assets/Screenshots/`はレビュー証跡(スクリーンショット)の保存先であり、コミット方針が定義されていないためコミット対象外とする(未追跡のまま維持、本番Asset・テストからは参照しない)。

### 次に行うこと
- T-008〜T-012(`feature/gameplay-application-foundation`ブランチ)はPull Request #5を経て`main`へマージ済み(マージコミット`35ac111`)。
- T-013〜T-017(`feature/presentation-gameplay-slice`ブランチ)は本セッションで完了。Codex第三者レビュー指摘(1回目: Major4件・Minor1件、2回目最終レビュー: Major2件・Minor2件)への対応もすべて完了。`main`へは未マージ・未コミット(commit/pushは本セッションでは行っていない)。
- Codex再レビューが可能な状態(実装・テスト・手動確認・PROJECT.md更新が完了済み)。
- 次のタスクは **T-018(村エリアの実装)**(T-013, T-014に依存、着手可能)。既存の最小`Village.unity`スキャフォールドを土台に、NPC3体以上への拡張とField接続を行う想定。
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

> 本タスク一覧はPhase 1以降の実装計画。T-001〜T-012は`main`にマージ済み(T-008〜T-012は`feature/gameplay-application-foundation`ブランチ、PR #5)。T-013〜T-017は`feature/presentation-gameplay-slice`ブランチで完了済み(`main`へは未マージ・未コミット)。次はT-018以降に進める状態。
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
| T-018 | 村エリアの実装 | Village Scene、NPC3体以上、フィールドへの接続 | 村シーンが単独でロード可能で、NPC会話とフィールドへの移動ができる | 手動プレイでシーン内を一巡して確認 | T-013, T-014 |
| T-019 | フィールドエリアの実装 | Field Scene、通常敵エンカウント、ダンジョン入口 | フィールドを探索でき、エンカウントが発生し、ダンジョンへ入れる | 手動プレイでエンカウント発生とダンジョン入口到達を確認 | T-015, T-018 |
| T-020 | ダンジョンの実装 | Dungeon Scene、通常敵エンカウント、ボス部屋 | ダンジョンを進めて道中戦闘を経てボスに到達できる | 手動プレイで入口からボス部屋まで到達を確認 | T-019 |
| T-021 | メインクエストの実装 | メインクエスト1本のトリガー・進行・完了 | 村での受注からボス撃破での完了までが一連で成立する | 手動プレイで開始から完了までを確認 | T-007, T-009, T-020 |
| T-022 | サブクエスト2本の実装 | サブクエスト2本のトリガー・進行・完了 | メインクエストと独立に受注・完了できる | 手動プレイでメインクエストと絡めず完了できることを確認 | T-007, T-018 |
| T-023 | アイテム・装備システムの実装 | インベントリUI、装備切り替えUI | アイテム使用・装備変更がステータス/戦闘に反映される | 手動プレイで装備変更前後のステータス変化と、アイテム使用効果を確認 | T-012, T-015 |
| T-024 | 通し結線・E2E確認 | 上記全タスクの統合 | タイトル→村→フィールド→ダンジョン→ボス撃破→クリア画面が、Consoleにエラーを出さず30〜60分で完走できる | 手動プレイでの通しプレイ、およびConsole監視 | T-016〜T-023 |
| T-025 | READMEおよびLICENSEの整備 | `README.md`, `LICENSE` | プロジェクト概要・セットアップ手順が記載されたREADMEと、ライセンス方針に沿ったLICENSEが存在する | ファイルの存在とNotion内容のレビュー | なし |
