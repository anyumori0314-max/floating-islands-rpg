using UnityEngine;

namespace FloatingIslandsRpg.Presentation.Cameras
{
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 3f, -6f);
        [SerializeField] private float _followSmoothing = 8f;

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            var desiredPosition = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _followSmoothing * Time.deltaTime);
            transform.LookAt(_target.position + Vector3.up);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
