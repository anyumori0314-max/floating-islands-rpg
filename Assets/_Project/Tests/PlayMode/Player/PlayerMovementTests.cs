using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.Player
{
    public sealed class PlayerMovementTests : InputTestFixture
    {
        private const int SampleFrameCount = 10;
        private const float TestMoveSpeed = 4f;
        private const float SpeedToleranceRatio = 0.05f;

        private Keyboard _keyboard;
        private InputActionAsset _moveActionAsset;
        private InputAction _moveTestAction;
        private InputActionReference _moveActionReference;
        private GameObject _playerObject;

        public override void Setup()
        {
            base.Setup();

            _keyboard = InputSystem.AddDevice<Keyboard>();

            _moveActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = _moveActionAsset.AddActionMap("Player");
            _moveTestAction = actionMap.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            _moveTestAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _moveActionReference = InputActionReference.Create(_moveTestAction);

            _playerObject = new GameObject("TestPlayer");
            _playerObject.SetActive(false);
            _playerObject.AddComponent<CharacterController>();
            var movement = _playerObject.AddComponent<PlayerMovement>();
            var moveActionField = typeof(PlayerMovement).GetField("_moveAction", BindingFlags.NonPublic | BindingFlags.Instance);
            moveActionField.SetValue(movement, _moveActionReference);

            _playerObject.SetActive(true);
            _moveTestAction.Enable();
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(_playerObject);

            _moveTestAction.Disable();

            if (_moveActionReference != null)
            {
                Object.DestroyImmediate(_moveActionReference);
            }

            if (_moveActionAsset != null)
            {
                Object.DestroyImmediate(_moveActionAsset);
            }

            if (_keyboard != null)
            {
                InputSystem.RemoveDevice(_keyboard);
            }

            base.TearDown();
        }

        [UnityTest]
        public IEnumerator Update_WhenForwardHeld_MovesAlongPositiveZOnly()
        {
            var startPosition = _playerObject.transform.position;

            Press(_keyboard.wKey);
            yield return WaitFrames(SampleFrameCount);
            Release(_keyboard.wKey);

            var displacement = _playerObject.transform.position - startPosition;

            Assert.Greater(displacement.z, 0f);
            Assert.AreEqual(0f, displacement.x, 0.0001f);
        }

        [UnityTest]
        public IEnumerator Update_WhenForwardHeld_RotatesToFaceMovementDirection()
        {
            Press(_keyboard.wKey);
            yield return WaitFrames(SampleFrameCount);
            Release(_keyboard.wKey);

            var forward = _playerObject.transform.forward;
            Assert.Greater(Vector3.Dot(forward, Vector3.forward), 0.99f);
        }

        [UnityTest]
        public IEnumerator Update_WhenDiagonalHeld_DoesNotExceedAxisAlignedDisplacementMagnitude()
        {
            // This measurement integrates real CharacterController displacement over
            // Time.deltaTime across several frames. Under `-batchmode` (no vsync/display to
            // pace the frame loop), per-frame Time.deltaTime is extremely small and volatile
            // -- unlike in the interactive Editor -- which starves CharacterController.Move()
            // of enough distance-per-frame to register at all (observed: total displacement
            // collapsing to 0 despite a nonzero, tiny accumulated deltaTime). Time.captureDeltaTime
            // is Unity's own supported mechanism for pinning Time.deltaTime to a fixed, known
            // value regardless of real frame pacing/rendering environment, so this test measures
            // the same real Update()/CharacterController.Move() production code path at a
            // deterministic, representative frame time (1/60s) instead of depending on
            // wall-clock/frame-rate/rendering conditions. PlayerMovement itself is unchanged.
            Time.captureDeltaTime = 1f / 60f;
            try
            {
                var axisResult = new HorizontalSpeedResult();
                yield return MeasureHorizontalSpeed(new[] { _keyboard.wKey }, SampleFrameCount, TestMoveSpeed, axisResult);

                var diagonalResult = new HorizontalSpeedResult();
                yield return MeasureHorizontalSpeed(new[] { _keyboard.wKey, _keyboard.dKey }, SampleFrameCount, TestMoveSpeed, diagonalResult);

                var failureMessage = $"axisSpeed={axisResult.Speed}, diagonalSpeed={diagonalResult.Speed}, " +
                    $"moveSpeed={TestMoveSpeed}, frameCount={SampleFrameCount}, " +
                    $"axisDeltaTimeTotal={axisResult.TotalDeltaTime}, diagonalDeltaTimeTotal={diagonalResult.TotalDeltaTime}, " +
                    $"ratio={diagonalResult.Speed / axisResult.Speed}";

                Assert.That(axisResult.Speed, Is.EqualTo(TestMoveSpeed).Within(TestMoveSpeed * SpeedToleranceRatio), failureMessage);
                Assert.That(diagonalResult.Speed, Is.EqualTo(TestMoveSpeed).Within(TestMoveSpeed * SpeedToleranceRatio), failureMessage);
                Assert.LessOrEqual(diagonalResult.Speed, axisResult.Speed * (1f + SpeedToleranceRatio), failureMessage);
            }
            finally
            {
                Time.captureDeltaTime = 0f;
            }
        }

        private static IEnumerator WaitFrames(int frameCount)
        {
            for (var i = 0; i < frameCount; i++)
            {
                yield return null;
            }
        }

        private sealed class HorizontalSpeedResult
        {
            public float Speed;
            public float TotalDeltaTime;
        }

        private IEnumerator MeasureHorizontalSpeed(KeyControl[] keysToHold, int frameCount, float moveSpeed, HorizontalSpeedResult result)
        {
            var moveActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = moveActionAsset.AddActionMap("Player");
            var moveAction = actionMap.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            var moveActionReference = InputActionReference.Create(moveAction);

            var playerObject = new GameObject("SpeedMeasurementPlayer");
            playerObject.SetActive(false);
            playerObject.AddComponent<CharacterController>();
            var movement = playerObject.AddComponent<PlayerMovement>();

            var moveActionField = typeof(PlayerMovement).GetField("_moveAction", BindingFlags.NonPublic | BindingFlags.Instance);
            moveActionField.SetValue(movement, moveActionReference);

            var moveSpeedField = typeof(PlayerMovement).GetField("_moveSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            moveSpeedField.SetValue(movement, moveSpeed);

            playerObject.SetActive(true);
            moveAction.Enable();

            yield return null;

            foreach (var key in keysToHold)
            {
                Press(key);
            }

            InputSystem.Update();

            yield return null;

            var startPosition = playerObject.transform.position;
            var totalDeltaTime = 0f;

            for (var i = 0; i < frameCount; i++)
            {
                yield return null;
                totalDeltaTime += Time.deltaTime;
            }

            var delta = playerObject.transform.position - startPosition;
            var horizontalDistance = new Vector2(delta.x, delta.z).magnitude;

            result.TotalDeltaTime = totalDeltaTime;
            result.Speed = totalDeltaTime > 0f ? horizontalDistance / totalDeltaTime : 0f;

            foreach (var key in keysToHold)
            {
                Release(key);
            }

            InputSystem.Update();

            Object.DestroyImmediate(playerObject);
            moveAction.Disable();
            Object.DestroyImmediate(moveActionReference);
            Object.DestroyImmediate(moveActionAsset);
        }
    }
}
