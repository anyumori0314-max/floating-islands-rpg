using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Presentation.Battle;
using FloatingIslandsRpg.Presentation.Encounters;
using FloatingIslandsRpg.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class BattleSceneInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _battleUiObject;
        private GameObject _installerObject;
        private Button _attackButton;
        private GameObject _resultPanel;
        private Text _enemyHpText;
        private Text _logText;
        private FakeSceneLoader _fakeSceneLoader;
        private GameServices _services;
        private BattleUIController _controller;
        private GameObject _defaultBattleCameraObject;
        private GameObject _defaultBattleEventSystemObject;
        private InitialPlayerDefinition _fallbackPlayerDefinition;
        private EnemyDefinition[] _regularEnemies;
        private EnemyDefinition _bossEnemy;

        [TearDown]
        public void TearDown()
        {
            if (_installerObject != null)
            {
                Object.DestroyImmediate(_installerObject);
            }

            if (_battleUiObject != null)
            {
                Object.DestroyImmediate(_battleUiObject);
            }

            if (_resultPanel != null)
            {
                Object.DestroyImmediate(_resultPanel);
            }

            if (_rootObject != null)
            {
                Object.DestroyImmediate(_rootObject);
            }

            if (_defaultBattleCameraObject != null)
            {
                Object.DestroyImmediate(_defaultBattleCameraObject);
            }

            if (_defaultBattleEventSystemObject != null)
            {
                Object.DestroyImmediate(_defaultBattleEventSystemObject);
            }

            if (_fallbackPlayerDefinition != null)
            {
                Object.DestroyImmediate(_fallbackPlayerDefinition);
            }

            if (_regularEnemies != null)
            {
                foreach (var enemy in _regularEnemies)
                {
                    Object.DestroyImmediate(enemy);
                }
            }

            if (_bossEnemy != null)
            {
                Object.DestroyImmediate(_bossEnemy);
            }
        }

        private static EnemyDefinition CreateEnemyDefinition(
            string id, string displayName, int maxHp, int maxMp, int attack, int defense, int agility, int magic, int rewardExperience)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            SetPrivateField(definition, "_id", id);
            SetPrivateField(definition, "_displayName", displayName);
            SetPrivateField(definition, "_maxHp", maxHp);
            SetPrivateField(definition, "_maxMp", maxMp);
            SetPrivateField(definition, "_attack", attack);
            SetPrivateField(definition, "_defense", defense);
            SetPrivateField(definition, "_agility", agility);
            SetPrivateField(definition, "_magic", magic);
            SetPrivateField(definition, "_rewardExperience", rewardExperience);
            return definition;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private Text CreateText(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_battleUiObject.transform);
            return go.AddComponent<Text>();
        }

        private IEnumerator BuildScene(PlayerSessionState currentSession)
        {
            yield return BuildScene(currentSession, null);
        }

        // Provides valid (if otherwise unused) Battle-scene presentation references by default,
        // so tests that aren't specifically about the presentation-reference wiring don't incur
        // the "missing reference" error logs from ValidateBattlePresentationReferences(). Tests
        // that care about that wiring call the 5-arg overload directly instead.
        private IEnumerator BuildScene(PlayerSessionState currentSession, PendingBattleContext pendingBattle)
        {
            _defaultBattleCameraObject = new GameObject("DefaultBattleCamera");
            var battleCamera = _defaultBattleCameraObject.AddComponent<Camera>();
            var battleAudioListener = _defaultBattleCameraObject.AddComponent<AudioListener>();

            _defaultBattleEventSystemObject = new GameObject("DefaultBattleEventSystem");
            var battleEventSystem = _defaultBattleEventSystemObject.AddComponent<EventSystem>();

            yield return BuildScene(currentSession, pendingBattle, battleCamera, battleAudioListener, battleEventSystem);
        }

        private IEnumerator BuildScene(
            PlayerSessionState currentSession,
            PendingBattleContext pendingBattle,
            Camera battleCamera,
            AudioListener battleAudioListener,
            EventSystem battleEventSystem,
            bool includeRegularEnemies = true,
            EquipmentDefinition[] equipmentCatalog = null)
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            _services = root.Services;
            _services.CurrentSession = currentSession;
            _services.PendingBattle = pendingBattle;
            _fakeSceneLoader = new FakeSceneLoader();
            _services.SceneTransitionUseCase = new SceneTransitionUseCase(_fakeSceneLoader);

            _battleUiObject = new GameObject("BattleUI");
            _battleUiObject.SetActive(false);

            var buttonObject = new GameObject("AttackButton");
            buttonObject.transform.SetParent(_battleUiObject.transform);
            buttonObject.AddComponent<Image>();
            _attackButton = buttonObject.AddComponent<Button>();

            var playerHpText = CreateText("PlayerHpText");
            _enemyHpText = CreateText("EnemyHpText");
            var logText = CreateText("LogText");
            _logText = logText;
            var resultText = CreateText("ResultText");

            _resultPanel = new GameObject("ResultPanel");
            _resultPanel.transform.SetParent(_battleUiObject.transform);
            resultText.transform.SetParent(_resultPanel.transform);

            _controller = _battleUiObject.AddComponent<BattleUIController>();
            SetPrivateField(_controller, "_attackButton", _attackButton);
            SetPrivateField(_controller, "_playerHpText", playerHpText);
            SetPrivateField(_controller, "_enemyHpText", _enemyHpText);
            SetPrivateField(_controller, "_logText", logText);
            SetPrivateField(_controller, "_resultPanel", _resultPanel);
            SetPrivateField(_controller, "_resultText", resultText);

            _battleUiObject.SetActive(true);

            _fallbackPlayerDefinition = ScriptableObject.CreateInstance<InitialPlayerDefinition>();
            SetPrivateField(_fallbackPlayerDefinition, "_displayName", "Hero");
            SetPrivateField(_fallbackPlayerDefinition, "_minLevel", 1);
            SetPrivateField(_fallbackPlayerDefinition, "_maxLevel", 10);
            SetPrivateField(_fallbackPlayerDefinition, "_baseMaxHp", 20);
            SetPrivateField(_fallbackPlayerDefinition, "_baseMaxMp", 5);
            SetPrivateField(_fallbackPlayerDefinition, "_baseAttack", 5);
            SetPrivateField(_fallbackPlayerDefinition, "_baseDefense", 3);
            SetPrivateField(_fallbackPlayerDefinition, "_baseAgility", 5);
            SetPrivateField(_fallbackPlayerDefinition, "_baseMagic", 2);
            SetPrivateField(_fallbackPlayerDefinition, "_growthMaxHp", 4);
            SetPrivateField(_fallbackPlayerDefinition, "_growthMaxMp", 1);
            SetPrivateField(_fallbackPlayerDefinition, "_growthAttack", 2);
            SetPrivateField(_fallbackPlayerDefinition, "_growthDefense", 1);
            SetPrivateField(_fallbackPlayerDefinition, "_growthAgility", 1);
            SetPrivateField(_fallbackPlayerDefinition, "_growthMagic", 1);
            SetPrivateField(_fallbackPlayerDefinition, "_cumulativeExperienceByLevel", new[] { 0, 10, 25, 45, 70, 100, 140, 190, 250, 320 });

            // All three regular-enemy fixtures share identical stats so that the random
            // pick among them (BattleSceneInstaller.PickRegularEnemy) does not make HP-display
            // assertions flaky; PickRegularEnemy itself is covered deterministically elsewhere.
            _regularEnemies = new[]
            {
                CreateEnemyDefinition("test_regular_a", "RegularA", 12, 0, 6, 1, 4, 0, 5),
                CreateEnemyDefinition("test_regular_b", "RegularB", 12, 0, 6, 1, 4, 0, 5),
                CreateEnemyDefinition("test_regular_c", "RegularC", 12, 0, 6, 1, 4, 0, 5),
            };
            _bossEnemy = CreateEnemyDefinition("test_boss", "Boss", 40, 10, 8, 4, 3, 2, 50);

            _installerObject = new GameObject("BattleSceneInstaller");
            var installer = _installerObject.AddComponent<BattleSceneInstaller>();
            SetPrivateField(installer, "_battleCamera", battleCamera);
            SetPrivateField(installer, "_battleAudioListener", battleAudioListener);
            SetPrivateField(installer, "_battleEventSystem", battleEventSystem);
            SetPrivateField(installer, "_fallbackPlayerDefinition", _fallbackPlayerDefinition);
            SetPrivateField(installer, "_regularEnemies", includeRegularEnemies ? _regularEnemies : null);
            SetPrivateField(installer, "_bossEnemy", _bossEnemy);
            SetPrivateField(installer, "_equipmentCatalog", equipmentCatalog);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Start_BindsBattleSessionUsingCurrentSessionStats()
        {
            var playerStats = new CharacterStats(3, 40, 10, 12, 4, 8, 3);
            var session = new PlayerSessionState(
                SceneId.Battle, playerStats, 50, 40, 10,
                new FloatingIslandsRpg.Domain.Quests.MainQuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress());

            yield return BuildScene(session);

            Assert.AreEqual(BattleOutcome.InProgress, _controller.CurrentOutcome);
            Assert.IsTrue(_attackButton.interactable);
        }

        [UnityTest]
        public IEnumerator BattleEnded_SetsLastBattleOutcomeAndRequestsGameClearTransition()
        {
            yield return BuildScene(null);

            while (_controller.CurrentOutcome == BattleOutcome.InProgress)
            {
                _attackButton.onClick.Invoke();
            }

            Assert.IsTrue(_services.LastBattleOutcome.HasValue);
            Assert.AreEqual(_controller.CurrentOutcome, _services.LastBattleOutcome.Value);
            Assert.AreEqual(SceneId.GameClear, _fakeSceneLoader.LastLoadedSceneId);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromBattleEnded()
        {
            yield return BuildScene(null);

            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            while (_controller.CurrentOutcome == BattleOutcome.InProgress)
            {
                _attackButton.onClick.Invoke();
            }

            Assert.IsFalse(_services.LastBattleOutcome.HasValue);
            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
        }

        [UnityTest]
        public IEnumerator Start_WithPartialHpCurrentSession_RematchSnapshotPreservesPreBattleHp()
        {
            var playerStats = new CharacterStats(3, 40, 10, 12, 4, 8, 3);
            var session = new PlayerSessionState(
                SceneId.Village, playerStats, 50, 17, 6,
                new FloatingIslandsRpg.Domain.Quests.MainQuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress());

            yield return BuildScene(session);

            Assert.IsNotNull(_services.RematchSnapshot);
            Assert.AreEqual(17, _services.RematchSnapshot.CurrentHp);
            Assert.AreEqual(6, _services.RematchSnapshot.CurrentMp);
            Assert.AreEqual(SceneId.Battle, _services.RematchSnapshot.CurrentSceneId);
        }

        [UnityTest]
        public IEnumerator Start_WithoutCurrentSession_RematchSnapshotUsesFallbackFullHp()
        {
            yield return BuildScene(null);

            Assert.IsNotNull(_services.RematchSnapshot);
            Assert.AreEqual(_services.RematchSnapshot.Stats.MaxHp, _services.RematchSnapshot.CurrentHp);
        }

        [UnityTest]
        public IEnumerator BattleEnded_TransitionFails_ControllerOutcomeRemainsStable()
        {
            yield return BuildScene(null);
            _fakeSceneLoader.FailWith = new System.Exception("Simulated transition failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated transition failure"));

            while (_controller.CurrentOutcome == BattleOutcome.InProgress)
            {
                _attackButton.onClick.Invoke();
            }

            var outcomeAfterBattle = _controller.CurrentOutcome;
            yield return null;

            Assert.AreEqual(outcomeAfterBattle, _controller.CurrentOutcome);
            Assert.IsTrue(_resultPanel.activeSelf);
        }

        [UnityTest]
        public IEnumerator Start_WithBossPendingBattle_UsesStrongerEnemyStats()
        {
            var playerStats = new CharacterStats(3, 40, 10, 12, 4, 8, 3);
            var session = new PlayerSessionState(
                SceneId.Battle, playerStats, 50, 40, 10,
                new FloatingIslandsRpg.Domain.Quests.MainQuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress());

            yield return BuildScene(session, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true));

            Assert.AreEqual("HP 40/40", _enemyHpText.text);
        }

        [UnityTest]
        public IEnumerator Start_WithRegularPendingBattle_UsesRegularEnemyStats()
        {
            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            Assert.AreEqual("HP 12/12", _enemyHpText.text);
        }

        // Drives BattleUIController.BattleEnded directly with a chosen outcome, bypassing the
        // real BattleSession/SystemRandomSource entirely. This keeps outcome-routing tests
        // (which branch of OnBattleEnded runs) fully deterministic without depending on RNG,
        // Attack-click timing, or overwhelming one side's stats to make an outcome "likely".
        private static void InvokeBattleEnded(BattleUIController controller, BattleOutcome outcome)
        {
            var field = typeof(BattleUIController).GetField("BattleEnded", BindingFlags.NonPublic | BindingFlags.Instance);
            var handler = (System.Action<BattleOutcome>)field.GetValue(controller);
            handler?.Invoke(outcome);
        }

        [UnityTest]
        public IEnumerator BattleEnded_RegularEncounterVictory_UnloadSucceeds_ResumesFieldGateAndClearsPendingBattle()
        {
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();

            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(SceneId.Battle, _fakeSceneLoader.LastUnloadedSceneId);
            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
            Assert.IsFalse(_services.LastBattleOutcome.HasValue);
            Assert.IsNull(_services.PendingBattle);

            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_RegularEncounterVictory_UnloadFails_ResumesFieldGateAnyway()
        {
            var gateObject = new GameObject("Gate");
            var gate = gateObject.AddComponent<FieldActivityGate>();
            var cameraObject = new GameObject("FieldCamera");
            var camera = cameraObject.AddComponent<Camera>();
            SetPrivateField(gate, "_fieldCamera", camera);
            camera.enabled = false;

            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false));
            _fakeSceneLoader.UnloadFailWith = new System.Exception("Simulated unload failure (Field)");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated unload failure \\(Field\\)"));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.IsNull(_services.PendingBattle);
            Assert.IsFalse(_services.LastBattleOutcome.HasValue);
            Assert.IsTrue(camera.enabled, "Field's Camera must be re-enabled even though the additive unload failed.");

            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_DungeonRegularEncounterVictory_UnloadFails_ResumesDungeonGateAnyway()
        {
            var gateObject = new GameObject("Gate");
            var gate = gateObject.AddComponent<FieldActivityGate>();
            var cameraObject = new GameObject("DungeonCamera");
            var camera = cameraObject.AddComponent<Camera>();
            SetPrivateField(gate, "_fieldCamera", camera);
            camera.enabled = false;

            yield return BuildScene(null, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: false));
            _fakeSceneLoader.UnloadFailWith = new System.Exception("Simulated unload failure (Dungeon)");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated unload failure \\(Dungeon\\)"));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.IsNull(_services.PendingBattle);
            Assert.IsTrue(camera.enabled, "Dungeon's Camera must be re-enabled even though the additive unload failed.");

            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(gateObject);
        }

        // Builds a Field/Dungeon-side FieldActivityGate wired to its own Camera, AudioListener,
        // EventSystem, PlayerMovement and FieldEncounterController -- a distinct set of
        // GameObjects from whatever Battle-side presentation objects a test wires separately --
        // so tests can assert that only the intended (Battle- or field-side) objects change.
        private sealed class FieldSidePresentation
        {
            public GameObject GateObject;
            public FieldActivityGate Gate;
            public GameObject CameraObject;
            public Camera Camera;
            public AudioListener AudioListener;
            public GameObject EventSystemObject;
            public EventSystem EventSystem;
            public GameObject PlayerObject;
            public PlayerMovement PlayerMovement;
            public GameObject EncounterObject;
            public FieldEncounterController EncounterController;

            public void DestroyAll()
            {
                Object.DestroyImmediate(GateObject);
                Object.DestroyImmediate(CameraObject);
                Object.DestroyImmediate(EventSystemObject);
                Object.DestroyImmediate(PlayerObject);
                Object.DestroyImmediate(EncounterObject);
            }
        }

        private static FieldSidePresentation CreateFieldSidePresentation()
        {
            var presentation = new FieldSidePresentation();

            presentation.CameraObject = new GameObject("FieldCamera");
            presentation.Camera = presentation.CameraObject.AddComponent<Camera>();
            presentation.AudioListener = presentation.CameraObject.AddComponent<AudioListener>();
            presentation.Camera.enabled = false;
            presentation.AudioListener.enabled = false;

            presentation.EventSystemObject = new GameObject("FieldEventSystem");
            presentation.EventSystem = presentation.EventSystemObject.AddComponent<EventSystem>();
            presentation.EventSystemObject.SetActive(false);

            // Kept inactive throughout: PlayerMovement.Awake() logs an error when its
            // InputActionReference is unset, and only the enabled/active flags matter here.
            presentation.PlayerObject = new GameObject("FieldPlayer");
            presentation.PlayerObject.SetActive(false);
            presentation.PlayerObject.AddComponent<CharacterController>();
            presentation.PlayerMovement = presentation.PlayerObject.AddComponent<PlayerMovement>();
            presentation.PlayerMovement.enabled = false;

            presentation.EncounterObject = new GameObject("FieldEncounter");
            presentation.EncounterController = presentation.EncounterObject.AddComponent<FieldEncounterController>();
            presentation.EncounterController.SetActive(false);

            presentation.GateObject = new GameObject("Gate");
            presentation.Gate = presentation.GateObject.AddComponent<FieldActivityGate>();
            SetPrivateField(presentation.Gate, "_fieldCamera", presentation.Camera);
            SetPrivateField(presentation.Gate, "_fieldAudioListener", presentation.AudioListener);
            SetPrivateField(presentation.Gate, "_eventSystem", presentation.EventSystemObject);
            SetPrivateField(presentation.Gate, "_playerMovement", presentation.PlayerMovement);
            SetPrivateField(presentation.Gate, "_encounterController", presentation.EncounterController);

            return presentation;
        }

        private static (GameObject cameraObject, Camera camera, AudioListener audioListener, GameObject eventSystemObject, EventSystem eventSystem) CreateBattleSidePresentation()
        {
            var cameraObject = new GameObject("BattleCamera");
            var camera = cameraObject.AddComponent<Camera>();
            var audioListener = cameraObject.AddComponent<AudioListener>();

            var eventSystemObject = new GameObject("BattleEventSystem");
            var eventSystem = eventSystemObject.AddComponent<EventSystem>();

            return (cameraObject, camera, audioListener, eventSystemObject, eventSystem);
        }

        [UnityTest]
        public IEnumerator BattleEnded_FieldRegularEncounterUnloadFails_DisablesBattleSideAndResumesFieldSide()
        {
            var field = CreateFieldSidePresentation();
            var battle = CreateBattleSidePresentation();

            yield return BuildScene(
                null,
                new PendingBattleContext(SceneId.Field, isBossEncounter: false),
                battle.camera,
                battle.audioListener,
                battle.eventSystem);

            _fakeSceneLoader.UnloadFailWith = new System.Exception("Simulated unload failure (field presentation)");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated unload failure \\(field presentation\\)"));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.IsFalse(battle.camera.enabled, "Battle's own Camera must be disabled when the unload fails.");
            Assert.IsFalse(battle.audioListener.enabled, "Battle's own AudioListener must be disabled when the unload fails.");
            Assert.IsFalse(battle.eventSystemObject.activeSelf, "Battle's own EventSystem must be disabled when the unload fails.");

            Assert.IsTrue(field.Camera.enabled, "Field's Camera must end up resumed.");
            Assert.IsTrue(field.AudioListener.enabled, "Field's AudioListener must end up resumed.");
            Assert.IsTrue(field.EventSystemObject.activeSelf, "Field's EventSystem must end up resumed.");
            Assert.IsTrue(field.PlayerMovement.enabled, "Field's PlayerMovement must end up resumed.");
            Assert.IsTrue((bool)typeof(FieldEncounterController)
                .GetField("_isActive", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(field.EncounterController), "Field's FieldEncounterController must end up resumed.");
            Assert.IsNull(_services.PendingBattle);

            field.DestroyAll();
            Object.DestroyImmediate(battle.cameraObject);
            Object.DestroyImmediate(battle.eventSystemObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_DungeonRegularEncounterUnloadFails_DisablesBattleSideOnlyAndResumesDungeonSide()
        {
            var dungeon = CreateFieldSidePresentation();
            var battle = CreateBattleSidePresentation();

            yield return BuildScene(
                null,
                new PendingBattleContext(SceneId.Dungeon, isBossEncounter: false),
                battle.camera,
                battle.audioListener,
                battle.eventSystem);

            _fakeSceneLoader.UnloadFailWith = new System.Exception("Simulated unload failure (dungeon presentation)");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated unload failure \\(dungeon presentation\\)"));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.IsFalse(battle.camera.enabled, "Battle's own Camera must be disabled when the unload fails.");
            Assert.IsFalse(battle.audioListener.enabled, "Battle's own AudioListener must be disabled when the unload fails.");
            Assert.IsFalse(battle.eventSystemObject.activeSelf, "Battle's own EventSystem must be disabled when the unload fails.");

            Assert.IsTrue(dungeon.Camera.enabled, "Dungeon's Camera must end up resumed.");
            Assert.IsTrue(dungeon.AudioListener.enabled, "Dungeon's AudioListener must end up resumed.");
            Assert.IsTrue(dungeon.EventSystemObject.activeSelf, "Dungeon's EventSystem must end up resumed.");
            Assert.IsNull(_services.PendingBattle);

            // Next operation possible: re-arming the boss/encounter path is exercised elsewhere
            // (BattleEnded_UnloadFails_ThenNextEncounterCanStillProceed); here we additionally
            // confirm the dungeon gate itself is left in a state that permits it.
            Assert.IsTrue((bool)typeof(FieldEncounterController)
                .GetField("_isActive", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(dungeon.EncounterController));

            dungeon.DestroyAll();
            Object.DestroyImmediate(battle.cameraObject);
            Object.DestroyImmediate(battle.eventSystemObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_UnloadFails_NoFieldActivityGatePresent_DoesNotTouchUnrelatedCameraFoundInScene()
        {
            // No FieldActivityGate is created here. If DisableBattlePresentation() (or anything
            // else in the failure path) ever fell back to a global Camera/AudioListener/
            // EventSystem search instead of using the Battle-scene-only serialized references,
            // this stray "unrelated" Camera -- the only one FindFirstObjectByType could find --
            // would be the one that gets disabled. It must be left completely untouched.
            var unrelatedCameraObject = new GameObject("UnrelatedSceneCamera");
            var unrelatedCamera = unrelatedCameraObject.AddComponent<Camera>();
            unrelatedCamera.enabled = true;

            // Battle's own presentation references are deliberately left unset (null) so that,
            // if a global-find fallback existed, it would have nothing else to find except the
            // unrelated camera above.
            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false), null, null, null);
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_battleCamera"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_battleAudioListener"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_battleEventSystem"));

            _fakeSceneLoader.UnloadFailWith = new System.Exception("Simulated unload failure (no gate)");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated unload failure \\(no gate\\)"));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.IsTrue(unrelatedCamera.enabled, "An unrelated Camera must never be touched by the Battle-side disable path.");

            Object.DestroyImmediate(unrelatedCameraObject);
        }

        [UnityTest]
        public IEnumerator Start_MissingBattlePresentationReferences_LogsErrorsButDoesNotThrowAndBattleStillProceeds()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_battleCamera"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_battleAudioListener"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_battleEventSystem"));

            yield return BuildScene(null, null, null, null, null);

            Assert.AreEqual(BattleOutcome.InProgress, _controller.CurrentOutcome);
            Assert.IsTrue(_attackButton.interactable);
        }

        [UnityTest]
        public IEnumerator BattleEnded_UnloadFails_DisablesBattlesOwnCameraAudioListenerAndEventSystem()
        {
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();

            var battle = CreateBattleSidePresentation();

            yield return BuildScene(
                null,
                new PendingBattleContext(SceneId.Field, isBossEncounter: false),
                battle.camera,
                battle.audioListener,
                battle.eventSystem);

            _fakeSceneLoader.UnloadFailWith = new System.Exception("Simulated unload failure (presentation)");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated unload failure \\(presentation\\)"));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.IsFalse(battle.camera.enabled);
            Assert.IsFalse(battle.audioListener.enabled);
            Assert.IsFalse(battle.eventSystemObject.activeSelf);

            Object.DestroyImmediate(battle.cameraObject);
            Object.DestroyImmediate(battle.eventSystemObject);
            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_UnloadFails_ThenNextEncounterCanStillProceed()
        {
            var gateObject = new GameObject("Gate");
            var gate = gateObject.AddComponent<FieldActivityGate>();
            var encounterObject = new GameObject("Encounter");
            var encounterController = encounterObject.AddComponent<FieldEncounterController>();
            SetPrivateField(gate, "_encounterController", encounterController);
            encounterController.SetActive(false);

            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false));
            _fakeSceneLoader.UnloadFailWith = new System.Exception("Simulated unload failure (retry path)");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated unload failure \\(retry path\\)"));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.IsTrue((bool)typeof(FieldEncounterController)
                .GetField("_isActive", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(encounterController), "The next encounter must not be permanently disabled after a failed unload.");

            Object.DestroyImmediate(encounterObject);
            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_BossEncounterVictory_TransitionsToGameClearInstead()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.DefeatBoss);
            yield return BuildScene(session, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(SceneId.GameClear, _fakeSceneLoader.LastLoadedSceneId);
            Assert.IsNull(_fakeSceneLoader.LastUnloadedSceneId);
            Assert.AreEqual(BattleOutcome.PlayerVictory, _services.LastBattleOutcome);
            Assert.IsNull(_services.PendingBattle);
            Assert.AreEqual(FloatingIslandsRpg.Domain.Quests.MainQuestStage.Completed, session.MainQuest.CurrentStage);
        }

        private static PlayerSessionState CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage stage)
        {
            var mainQuest = new FloatingIslandsRpg.Domain.Quests.MainQuestProgress();
            if (stage >= FloatingIslandsRpg.Domain.Quests.MainQuestStage.ExploreField)
            {
                mainQuest.Start();
            }

            if (stage >= FloatingIslandsRpg.Domain.Quests.MainQuestStage.EnterDungeon)
            {
                mainQuest.AdvanceToEnterDungeon();
            }

            if (stage >= FloatingIslandsRpg.Domain.Quests.MainQuestStage.DefeatBoss)
            {
                mainQuest.AdvanceToDefeatBoss();
            }

            var stats = new CharacterStats(3, 40, 10, 12, 4, 8, 3);
            return new PlayerSessionState(
                SceneId.Battle, stats, 50, 40, 10,
                mainQuest,
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress());
        }

        [UnityTest]
        public IEnumerator BattleEnded_BossVictoryWithMainQuestNotAtDefeatBoss_ReturnsInsteadOfGameClear()
        {
            // The main quest was never advanced to DefeatBoss (e.g. the player skipped the
            // quest NPC / Field / Dungeon gating entirely); PROJECT.md T-021 requires both the
            // boss win AND a Completed main quest before GameClear is reached.
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted);
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();
            yield return BuildScene(session, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("boss defeated without the main quest"));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(SceneId.Battle, _fakeSceneLoader.LastUnloadedSceneId);
            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
            Assert.IsFalse(_services.LastBattleOutcome.HasValue);
            Assert.IsNull(_services.PendingBattle);
            Assert.AreEqual(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted, session.MainQuest.CurrentStage);

            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_BossVictoryWithoutCurrentSession_ReturnsInsteadOfGameClear()
        {
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();
            yield return BuildScene(null, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("boss defeated without the main quest"));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(SceneId.Battle, _fakeSceneLoader.LastUnloadedSceneId);
            Assert.IsFalse(_services.LastBattleOutcome.HasValue);

            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_BossEncounterDefeat_TransitionsToGameClearAsGameOver()
        {
            yield return BuildScene(null, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerDefeat);
            yield return null;

            Assert.AreEqual(SceneId.GameClear, _fakeSceneLoader.LastLoadedSceneId);
            Assert.IsNull(_fakeSceneLoader.LastUnloadedSceneId);
            Assert.AreEqual(BattleOutcome.PlayerDefeat, _services.LastBattleOutcome);
            Assert.IsNull(_services.PendingBattle);
        }

        [UnityTest]
        public IEnumerator BattleEnded_RegularEncounterDefeat_TransitionsToGameClearAsGameOver()
        {
            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerDefeat);
            yield return null;

            Assert.AreEqual(SceneId.GameClear, _fakeSceneLoader.LastLoadedSceneId);
            Assert.IsNull(_fakeSceneLoader.LastUnloadedSceneId);
            Assert.AreEqual(BattleOutcome.PlayerDefeat, _services.LastBattleOutcome);
            Assert.IsNull(_services.PendingBattle);
        }

        // --- T-022: real MasterData asset wiring ---

        [Test]
        public void PickRegularEnemy_RollAtLowerBound_ReturnsFirstCandidate()
        {
            var candidates = _regularEnemies = new[]
            {
                CreateEnemyDefinition("a", "A", 1, 0, 0, 0, 0, 0, 0),
                CreateEnemyDefinition("b", "B", 1, 0, 0, 0, 0, 0, 0),
                CreateEnemyDefinition("c", "C", 1, 0, 0, 0, 0, 0, 0),
            };

            Assert.AreSame(candidates[0], BattleSceneInstaller.PickRegularEnemy(candidates, 0.0));
        }

        [Test]
        public void PickRegularEnemy_RollJustBelowUpperBound_ReturnsLastCandidate()
        {
            var candidates = _regularEnemies = new[]
            {
                CreateEnemyDefinition("a", "A", 1, 0, 0, 0, 0, 0, 0),
                CreateEnemyDefinition("b", "B", 1, 0, 0, 0, 0, 0, 0),
                CreateEnemyDefinition("c", "C", 1, 0, 0, 0, 0, 0, 0),
            };

            Assert.AreSame(candidates[2], BattleSceneInstaller.PickRegularEnemy(candidates, 0.999));
        }

        [Test]
        public void PickRegularEnemy_RollAtExactUpperBound_ClampsToLastCandidate()
        {
            var candidates = _regularEnemies = new[]
            {
                CreateEnemyDefinition("a", "A", 1, 0, 0, 0, 0, 0, 0),
                CreateEnemyDefinition("b", "B", 1, 0, 0, 0, 0, 0, 0),
            };

            // NextDouble()'s contract is [0.0, 1.0), but 1.0 is defended against anyway so an
            // out-of-range roll can never index past the array.
            Assert.AreSame(candidates[1], BattleSceneInstaller.PickRegularEnemy(candidates, 1.0));
        }

        [Test]
        public void PickRegularEnemy_MiddleRoll_ReturnsMiddleCandidate()
        {
            var candidates = _regularEnemies = new[]
            {
                CreateEnemyDefinition("a", "A", 1, 0, 0, 0, 0, 0, 0),
                CreateEnemyDefinition("b", "B", 1, 0, 0, 0, 0, 0, 0),
                CreateEnemyDefinition("c", "C", 1, 0, 0, 0, 0, 0, 0),
            };

            Assert.AreSame(candidates[1], BattleSceneInstaller.PickRegularEnemy(candidates, 0.5));
        }

        [UnityTest]
        public IEnumerator Start_MissingRegularEnemies_LogsErrorAndDoesNotBindBattle()
        {
            // ValidateBattlePresentationReferences() runs before ValidateMasterDataReferences()
            // in Start(), so LogAssert's expectations must be registered in that emission order.
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_battleCamera"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_battleAudioListener"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_battleEventSystem"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_regularEnemies"));

            yield return BuildScene(null, null, null, null, null, includeRegularEnemies: false);

            // Bind() was never reached: BattleUIController's HP text is only ever populated by
            // RefreshHpDisplay(), which Bind() calls. An empty string proves Start() aborted
            // before constructing/binding a BattleSession.
            Assert.AreEqual(string.Empty, _enemyHpText.text);
        }

        [UnityTest]
        public IEnumerator Start_WithMultipleRegularEnemyOptions_AllOptionsAreValidCandidates()
        {
            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            // All three fixtures share the same stats (see BuildScene), so regardless of which
            // one SystemRandomSource happens to pick, the resulting enemy HP must be one of the
            // configured candidates' MaxHp values.
            var expectedHp = _regularEnemies[0].ToMasterData().MaxHp;
            Assert.AreEqual($"HP {expectedHp}/{expectedHp}", _enemyHpText.text);
        }

        // --- T-023: battle reward / experience / level-up integration ---

        [UnityTest]
        public IEnumerator BattleEnded_RegularEncounterVictory_GrantsRewardExperienceOnce()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted);
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();
            var startingExperience = session.TotalExperience;
            yield return BuildScene(session, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(startingExperience + _regularEnemies[0].ToMasterData().RewardExperience, session.TotalExperience);
            StringAssert.Contains("EXP +", _logText.text);

            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_BossVictory_GrantsBossRewardExperience()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.DefeatBoss);
            var startingExperience = session.TotalExperience;
            yield return BuildScene(session, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(startingExperience + _bossEnemy.ToMasterData().RewardExperience, session.TotalExperience);
        }

        [UnityTest]
        public IEnumerator BattleEnded_Defeat_DoesNotGrantExperience()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted);
            var startingExperience = session.TotalExperience;
            yield return BuildScene(session, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerDefeat);
            yield return null;

            Assert.AreEqual(startingExperience, session.TotalExperience);
        }

        [UnityTest]
        public IEnumerator BattleEnded_InvokedTwice_DoesNotGrantRewardTwice()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted);
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();
            var startingExperience = session.TotalExperience;
            yield return BuildScene(session, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;
            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(startingExperience + _regularEnemies[0].ToMasterData().RewardExperience, session.TotalExperience);

            Object.DestroyImmediate(gateObject);
        }

        // --- Codex review Major 1: OnBattleEnded must run its full sequence at most once ---

        [UnityTest]
        public IEnumerator BattleEnded_InvokedTwice_BossVictory_GameClearTransitionRequestedOnce()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.DefeatBoss);
            yield return BuildScene(session, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;
            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(1, _fakeSceneLoader.LoadCallCount);
            Assert.AreEqual(SceneId.GameClear, _fakeSceneLoader.LastLoadedSceneId);
        }

        [UnityTest]
        public IEnumerator BattleEnded_InvokedTwice_SecondOutcomeDoesNotOverwriteLastBattleOutcome()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.DefeatBoss);
            yield return BuildScene(session, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;
            InvokeBattleEnded(_controller, BattleOutcome.PlayerDefeat);
            yield return null;

            Assert.AreEqual(BattleOutcome.PlayerVictory, _services.LastBattleOutcome);
            Assert.AreEqual(1, _fakeSceneLoader.LoadCallCount);
        }

        [UnityTest]
        public IEnumerator BattleEnded_InvokedTwice_RegularVictory_ReturnToFieldRequestedOnce()
        {
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();
            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;
            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(1, _fakeSceneLoader.UnloadCallCount);

            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_InvokedTwice_MainQuestAdvancedOnce()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.DefeatBoss);
            yield return BuildScene(session, new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;
            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(FloatingIslandsRpg.Domain.Quests.MainQuestStage.Completed, session.MainQuest.CurrentStage);
            // A second (buggy, unguarded) pass through OnBattleEnded would have re-entered the
            // PlayerVictory/IsBossEncounter branch and requested a second GameClear transition;
            // this is the externally observable symptom the guard prevents.
            Assert.AreEqual(1, _fakeSceneLoader.LoadCallCount);
        }

        [UnityTest]
        public IEnumerator BattleEnded_InvokedTwice_PendingBattleConsumedOnce()
        {
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();
            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            // Simulate a new encounter's PendingBattle having already been armed by the time a
            // stale, duplicated BattleEnded call for the *previous* battle arrives.
            var nextPending = new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true);
            _services.PendingBattle = nextPending;

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreSame(nextPending, _services.PendingBattle);

            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_NewInstallerInstance_HandlesOwnBattleEndIndependently()
        {
            var sessionA = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted);
            var gateObjectA = new GameObject("GateA");
            gateObjectA.AddComponent<FieldActivityGate>();
            var startingExperienceA = sessionA.TotalExperience;
            yield return BuildScene(sessionA, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(startingExperienceA + _regularEnemies[0].ToMasterData().RewardExperience, sessionA.TotalExperience);
            Object.DestroyImmediate(gateObjectA);

            // Tear down and rebuild a brand new BattleSceneInstaller instance, exactly as a fresh
            // Battle Scene load would (new encounter, or a Retry). Its own _battleEndHandled must
            // start false again -- there is no shared/static state across instances.
            Object.DestroyImmediate(_installerObject);
            _installerObject = null;
            Object.DestroyImmediate(_battleUiObject);
            _battleUiObject = null;
            Object.DestroyImmediate(_resultPanel);
            _resultPanel = null;
            Object.DestroyImmediate(_rootObject);
            _rootObject = null;
            Object.DestroyImmediate(_defaultBattleCameraObject);
            _defaultBattleCameraObject = null;
            Object.DestroyImmediate(_defaultBattleEventSystemObject);
            _defaultBattleEventSystemObject = null;

            var sessionB = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted);
            var gateObjectB = new GameObject("GateB");
            gateObjectB.AddComponent<FieldActivityGate>();
            var startingExperienceB = sessionB.TotalExperience;
            yield return BuildScene(sessionB, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(startingExperienceB + _regularEnemies[0].ToMasterData().RewardExperience, sessionB.TotalExperience);

            Object.DestroyImmediate(gateObjectB);
        }

        [UnityTest]
        public IEnumerator BattleEnded_RewardCausesLevelUp_UpdatesStatsAndDisplaysLevelUp()
        {
            // Level 1 with 0 experience; the fixture's cumulative table requires 10 XP for
            // level 2, and RegularA/B/C fixtures each grant 5 XP -- not enough by itself, so
            // stack two battles' worth via a session already sitting at 9 XP.
            var stats = new CharacterStats(1, 20, 5, 5, 3, 5, 2);
            var session = new PlayerSessionState(
                SceneId.Battle, stats, 9, 20, 5,
                new FloatingIslandsRpg.Domain.Quests.MainQuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress());
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();
            yield return BuildScene(session, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(14, session.TotalExperience);
            Assert.AreEqual(2, session.Stats.Level);
            StringAssert.Contains("Level Up! Lv.2", _logText.text);

            Object.DestroyImmediate(gateObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_WithoutCurrentSession_GrantRewardDoesNotThrow()
        {
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();
            yield return BuildScene(null, new PendingBattleContext(SceneId.Field, isBossEncounter: false));

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(SceneId.Battle, _fakeSceneLoader.LastUnloadedSceneId);

            Object.DestroyImmediate(gateObject);
        }

        // --- T-024: equipment bonus / battle item reward integration ---

        private static EquipmentDefinition CreateEquipmentDefinition(string id, string displayName, EquipmentSlot slot, int attackBonus, int defenseBonus)
        {
            var definition = ScriptableObject.CreateInstance<EquipmentDefinition>();
            SetPrivateField(definition, "_id", id);
            SetPrivateField(definition, "_displayName", displayName);
            SetPrivateField(definition, "_slot", slot);
            SetPrivateField(definition, "_attackBonus", attackBonus);
            SetPrivateField(definition, "_defenseBonus", defenseBonus);
            return definition;
        }

        private static BattleSession GetBoundSession(BattleUIController controller)
        {
            var field = typeof(BattleUIController).GetField("_session", BindingFlags.NonPublic | BindingFlags.Instance);
            return (BattleSession)field.GetValue(controller);
        }

        [UnityTest]
        public IEnumerator Start_EquippedWeapon_AppliesAttackBonusToBattleParticipantStats()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted);
            var baseAttack = session.Stats.Attack;
            session.Inventory.Add("equip_test_sword", 1);
            session.Equipment.EquipWeapon("equip_test_sword");
            var weapon = CreateEquipmentDefinition("equip_test_sword", "TestSword", EquipmentSlot.Weapon, 8, 0);
            var battle = CreateBattleSidePresentation();

            yield return BuildScene(
                session, new PendingBattleContext(SceneId.Field, isBossEncounter: false),
                battle.camera, battle.audioListener, battle.eventSystem, equipmentCatalog: new[] { weapon });

            var boundSession = GetBoundSession(_controller);
            Assert.AreEqual(baseAttack + 8, boundSession.Player.Stats.Attack);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(battle.cameraObject);
            Object.DestroyImmediate(battle.eventSystemObject);
        }

        [UnityTest]
        public IEnumerator Start_NoEquipmentEquipped_UsesBaseAttackUnchanged()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted);
            var baseAttack = session.Stats.Attack;
            var weapon = CreateEquipmentDefinition("equip_test_sword", "TestSword", EquipmentSlot.Weapon, 8, 0);
            var battle = CreateBattleSidePresentation();

            yield return BuildScene(
                session, new PendingBattleContext(SceneId.Field, isBossEncounter: false),
                battle.camera, battle.audioListener, battle.eventSystem, equipmentCatalog: new[] { weapon });

            var boundSession = GetBoundSession(_controller);
            Assert.AreEqual(baseAttack, boundSession.Player.Stats.Attack);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(battle.cameraObject);
            Object.DestroyImmediate(battle.eventSystemObject);
        }

        [UnityTest]
        public IEnumerator BattleEnded_Victory_GrantsConfiguredItemRewardOnce()
        {
            var session = CreateSessionWithMainQuestAtStage(FloatingIslandsRpg.Domain.Quests.MainQuestStage.NotStarted);
            var gateObject = new GameObject("Gate");
            gateObject.AddComponent<FieldActivityGate>();
            var itemReward = ScriptableObject.CreateInstance<ItemDefinition>();
            SetPrivateField(itemReward, "_id", "item_small_potion");
            SetPrivateField(itemReward, "_displayName", "Small Potion");
            SetPrivateField(itemReward, "_healAmount", 20);

            yield return BuildScene(session, new PendingBattleContext(SceneId.Field, isBossEncounter: false));
            SetPrivateField(_installerObject.GetComponent<BattleSceneInstaller>(), "_victoryItemReward", itemReward);

            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;
            InvokeBattleEnded(_controller, BattleOutcome.PlayerVictory);
            yield return null;

            Assert.AreEqual(1, session.Inventory.GetQuantity("item_small_potion"));

            Object.DestroyImmediate(gateObject);
            Object.DestroyImmediate(itemReward);
        }
    }
}
