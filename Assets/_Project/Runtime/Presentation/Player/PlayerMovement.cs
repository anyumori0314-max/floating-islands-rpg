using UnityEngine;
using UnityEngine.InputSystem;

namespace FloatingIslandsRpg.Presentation.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        private const float GravityAcceleration = -9.81f;

        [SerializeField] private float _moveSpeed = 4f;
        [SerializeField] private InputActionReference _moveAction;

        private CharacterController _controller;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (_moveAction == null)
            {
                Debug.LogError($"{nameof(PlayerMovement)} on '{name}' requires a Move InputActionReference to be assigned.", this);
            }
        }

        private void OnEnable()
        {
            _moveAction?.action.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.action.Disable();
        }

        private void Update()
        {
            var rawInput = _moveAction != null ? _moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            var direction = new Vector3(rawInput.x, 0f, rawInput.y);
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            var deltaTime = Time.deltaTime;

            _verticalVelocity = _controller.isGrounded
                ? 0f
                : _verticalVelocity + GravityAcceleration * deltaTime;

            var motion = (direction * _moveSpeed) + (Vector3.up * _verticalVelocity);
            _controller.Move(motion * deltaTime);

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }
}
