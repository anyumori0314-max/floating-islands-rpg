using System.Reflection;
using FloatingIslandsRpg.Presentation.Encounters;
using FloatingIslandsRpg.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FloatingIslandsRpg.Tests.PlayMode.Encounters
{
    public sealed class FieldActivityGateTests
    {
        private InputActionAsset _moveActionAsset;
        private InputActionReference _moveActionReference;

        private GameObject _gateObject;
        private FieldActivityGate _gate;

        private GameObject _cameraObject;
        private Camera _camera;
        private AudioListener _audioListener;

        private GameObject _eventSystemObject;

        private GameObject _playerObject;
        private PlayerMovement _playerMovement;

        private GameObject _encounterObject;
        private FieldEncounterController _encounterController;

        [SetUp]
        public void SetUp()
        {
            _moveActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = _moveActionAsset.AddActionMap("Player");
            var moveAction = actionMap.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            _moveActionReference = InputActionReference.Create(moveAction);

            _cameraObject = new GameObject("FieldCamera");
            _camera = _cameraObject.AddComponent<Camera>();
            _audioListener = _cameraObject.AddComponent<AudioListener>();

            _eventSystemObject = new GameObject("EventSystem");
            _eventSystemObject.AddComponent<EventSystem>();

            _playerObject = new GameObject("Player");
            _playerObject.SetActive(false);
            _playerObject.AddComponent<CharacterController>();
            _playerMovement = _playerObject.AddComponent<PlayerMovement>();
            SetPrivateField(_playerMovement, "_moveAction", _moveActionReference);
            _playerObject.SetActive(true);

            _encounterObject = new GameObject("FieldEncounterController");
            _encounterController = _encounterObject.AddComponent<FieldEncounterController>();

            _gateObject = new GameObject("FieldActivityGate");
            _gate = _gateObject.AddComponent<FieldActivityGate>();
            SetPrivateField(_gate, "_fieldCamera", _camera);
            SetPrivateField(_gate, "_fieldAudioListener", _audioListener);
            SetPrivateField(_gate, "_eventSystem", _eventSystemObject);
            SetPrivateField(_gate, "_playerMovement", _playerMovement);
            SetPrivateField(_gate, "_encounterController", _encounterController);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gateObject);
            Object.DestroyImmediate(_cameraObject);
            Object.DestroyImmediate(_eventSystemObject);
            Object.DestroyImmediate(_playerObject);
            Object.DestroyImmediate(_encounterObject);

            if (_moveActionReference != null)
            {
                Object.DestroyImmediate(_moveActionReference);
            }

            if (_moveActionAsset != null)
            {
                Object.DestroyImmediate(_moveActionAsset);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private static bool GetEncounterActiveState(FieldEncounterController controller)
        {
            var field = typeof(FieldEncounterController).GetField("_isActive", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)field.GetValue(controller);
        }

        [Test]
        public void Pause_DisablesCameraAudioListenerEventSystemPlayerMovementAndEncounters()
        {
            _gate.Pause();

            Assert.IsFalse(_camera.enabled);
            Assert.IsFalse(_audioListener.enabled);
            Assert.IsFalse(_eventSystemObject.activeSelf);
            Assert.IsFalse(_playerMovement.enabled);
            Assert.IsFalse(GetEncounterActiveState(_encounterController));
        }

        [Test]
        public void Resume_AfterPause_ReenablesEverything()
        {
            _gate.Pause();

            _gate.Resume();

            Assert.IsTrue(_camera.enabled);
            Assert.IsTrue(_audioListener.enabled);
            Assert.IsTrue(_eventSystemObject.activeSelf);
            Assert.IsTrue(_playerMovement.enabled);
            Assert.IsTrue(GetEncounterActiveState(_encounterController));
        }
    }
}
