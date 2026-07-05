using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Presentation.Cameras;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.Cameras
{
    public sealed class FollowCameraTests
    {
        private GameObject _cameraObject;
        private GameObject _targetObject;

        [TearDown]
        public void TearDown()
        {
            if (_cameraObject != null)
            {
                Object.DestroyImmediate(_cameraObject);
            }

            if (_targetObject != null)
            {
                Object.DestroyImmediate(_targetObject);
            }
        }

        [UnityTest]
        public IEnumerator LateUpdate_WhenTargetMoves_CameraFollowsTowardOffsetPosition()
        {
            _targetObject = new GameObject("Target");

            _cameraObject = new GameObject("TestCamera");
            _cameraObject.SetActive(false);
            var followCamera = _cameraObject.AddComponent<FollowCamera>();
            followCamera.SetTarget(_targetObject.transform);
            _cameraObject.SetActive(true);

            _targetObject.transform.position = new Vector3(10f, 0f, 10f);

            var initialDistance = Vector3.Distance(_cameraObject.transform.position, _targetObject.transform.position);

            for (var i = 0; i < 30; i++)
            {
                yield return null;
            }

            var finalDistance = Vector3.Distance(_cameraObject.transform.position, _targetObject.transform.position);

            Assert.Less(finalDistance, initialDistance);
        }

        [UnityTest]
        public IEnumerator Awake_WhenTargetNotAssigned_DoesNotLogOrThrow()
        {
            _cameraObject = new GameObject("TestCameraNoTarget", typeof(FollowCamera));

            yield return null;

            Assert.IsNotNull(_cameraObject);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator SetTarget_AssignsPrivateTargetField()
        {
            _targetObject = new GameObject("Target");

            _cameraObject = new GameObject("TestCamera");
            _cameraObject.SetActive(false);
            var followCamera = _cameraObject.AddComponent<FollowCamera>();
            followCamera.SetTarget(_targetObject.transform);
            _cameraObject.SetActive(true);

            yield return null;

            var targetField = typeof(FollowCamera).GetField("_target", BindingFlags.NonPublic | BindingFlags.Instance);
            var assignedTarget = targetField.GetValue(followCamera) as Transform;

            Assert.AreSame(_targetObject.transform, assignedTarget);
        }
    }
}
