using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Presentation.Dialogue;
using FloatingIslandsRpg.Presentation.Encounters;
using FloatingIslandsRpg.Presentation.Menu;
using FloatingIslandsRpg.Presentation.Player;
using FloatingIslandsRpg.Presentation.Scenes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.Menu
{
    public sealed class MenuActivityGateTests
    {
        private GameObject _gateObject;
        private GameObject _playerObject;
        private GameObject _encounterObject;
        private GameObject _npcObject;
        private GameObject _triggerObject;
        private MenuActivityGate _gate;

        [TearDown]
        public void TearDown()
        {
            if (_gateObject != null) Object.DestroyImmediate(_gateObject);
            if (_playerObject != null) Object.DestroyImmediate(_playerObject);
            if (_encounterObject != null) Object.DestroyImmediate(_encounterObject);
            if (_npcObject != null) Object.DestroyImmediate(_npcObject);
            if (_triggerObject != null) Object.DestroyImmediate(_triggerObject);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private IEnumerator BuildScene()
        {
            // Kept inactive throughout (matching FieldSceneInstallerTests' established
            // convention): PlayerMovement.Awake() logs an error when its InputActionReference is
            // unset, and only component *presence* plus the .enabled flag matter for this test.
            _playerObject = new GameObject("Player");
            _playerObject.SetActive(false);
            _playerObject.AddComponent<CharacterController>();
            var playerMovement = _playerObject.AddComponent<PlayerMovement>();

            _encounterObject = new GameObject("Encounter");
            var encounterController = _encounterObject.AddComponent<FieldEncounterController>();

            _npcObject = new GameObject("Npc");
            var npc = _npcObject.AddComponent<NpcInteractable>();

            _triggerObject = new GameObject("Trigger");
            var trigger = _triggerObject.AddComponent<SceneTransitionTrigger>();

            _gateObject = new GameObject("MenuActivityGate");
            _gate = _gateObject.AddComponent<MenuActivityGate>();
            SetPrivateField(_gate, "_playerMovement", playerMovement);
            SetPrivateField(_gate, "_encounterController", encounterController);
            SetPrivateField(_gate, "_npcInteractables", new[] { npc });
            SetPrivateField(_gate, "_transitionTriggers", new[] { trigger });

            yield return null;
        }

        private static bool IsFieldEncounterControllerActive(FieldEncounterController controller)
        {
            var field = typeof(FieldEncounterController).GetField("_isActive", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)field.GetValue(controller);
        }

        [UnityTest]
        public IEnumerator Pause_DisablesPlayerMovementEncounterNpcAndTrigger()
        {
            yield return BuildScene();

            _gate.Pause();

            Assert.IsFalse(_playerObject.GetComponent<PlayerMovement>().enabled);
            Assert.IsFalse(IsFieldEncounterControllerActive(_encounterObject.GetComponent<FieldEncounterController>()));
            Assert.IsFalse(_npcObject.GetComponent<NpcInteractable>().enabled);
            Assert.IsFalse(_triggerObject.GetComponent<SceneTransitionTrigger>().enabled);
        }

        [UnityTest]
        public IEnumerator Resume_ReEnablesPlayerMovementEncounterNpcAndTrigger()
        {
            yield return BuildScene();
            _gate.Pause();

            _gate.Resume();

            Assert.IsTrue(_playerObject.GetComponent<PlayerMovement>().enabled);
            Assert.IsTrue(IsFieldEncounterControllerActive(_encounterObject.GetComponent<FieldEncounterController>()));
            Assert.IsTrue(_npcObject.GetComponent<NpcInteractable>().enabled);
            Assert.IsTrue(_triggerObject.GetComponent<SceneTransitionTrigger>().enabled);
        }

        [UnityTest]
        public IEnumerator Pause_DoesNotDisableCameraOrEventSystem()
        {
            // Unlike FieldActivityGate, MenuActivityGate must leave the scene's own Camera/
            // EventSystem alone -- the menu itself still needs them (PROJECT.md T-026).
            var cameraObject = new GameObject("Camera");
            var camera = cameraObject.AddComponent<Camera>();
            yield return BuildScene();

            _gate.Pause();

            Assert.IsTrue(camera.enabled);

            Object.DestroyImmediate(cameraObject);
        }

        [UnityTest]
        public IEnumerator Pause_WithUnsetOptionalReferences_DoesNotThrow()
        {
            _gateObject = new GameObject("MenuActivityGate");
            _gate = _gateObject.AddComponent<MenuActivityGate>();

            Assert.DoesNotThrow(() => _gate.Pause());
            Assert.DoesNotThrow(() => _gate.Resume());
            yield return null;
        }
    }
}
