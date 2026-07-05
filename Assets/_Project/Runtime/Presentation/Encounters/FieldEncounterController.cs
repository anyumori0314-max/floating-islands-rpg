using System;
using FloatingIslandsRpg.Application.Battle;
using UnityEngine;

namespace FloatingIslandsRpg.Presentation.Encounters
{
    public sealed class FieldEncounterController : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _distancePerCheck = 5f;
        [SerializeField] private double _encounterChancePerCheck = 0.3;

        private IRandomSource _randomSource;
        private Vector3 _lastPosition;
        private float _accumulatedDistance;
        private bool _isActive = true;

        public event Action EncounterTriggered;

        public void Bind(IRandomSource randomSource)
        {
            _randomSource = randomSource;
        }

        public void SetActive(bool isActive)
        {
            _isActive = isActive;

            // Resuming after a battle resets the baseline so the (irrelevant) distance the
            // player's transform may have moved while paused isn't counted as field travel.
            if (isActive && _playerTransform != null)
            {
                _lastPosition = _playerTransform.position;
            }
        }

        private void OnEnable()
        {
            if (_playerTransform != null)
            {
                _lastPosition = _playerTransform.position;
            }
        }

        private void Update()
        {
            if (!_isActive || _playerTransform == null || _randomSource == null)
            {
                return;
            }

            var currentPosition = _playerTransform.position;
            _accumulatedDistance += Vector3.Distance(_lastPosition, currentPosition);
            _lastPosition = currentPosition;

            if (_accumulatedDistance < _distancePerCheck)
            {
                return;
            }

            _accumulatedDistance -= _distancePerCheck;

            if (_randomSource.NextDouble() < _encounterChancePerCheck)
            {
                EncounterTriggered?.Invoke();
            }
        }
    }
}
