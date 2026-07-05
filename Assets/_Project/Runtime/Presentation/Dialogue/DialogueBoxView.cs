using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Presentation.Dialogue
{
    public sealed class DialogueBoxView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _lineText;
        [SerializeField] private InputActionReference _advanceAction;

        private DialogueSession _session;

        public bool IsOpen => _session != null && _session.IsActive;

        public event Action Closed;

        private void Awake()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_advanceAction != null)
            {
                _advanceAction.action.Enable();
                _advanceAction.action.performed += OnAdvancePerformed;
            }
        }

        private void OnDisable()
        {
            if (_advanceAction != null)
            {
                _advanceAction.action.performed -= OnAdvancePerformed;
                _advanceAction.action.Disable();
            }
        }

        public bool TryOpen(IReadOnlyList<string> lines)
        {
            if (IsOpen)
            {
                return false;
            }

            if (lines == null || lines.Count == 0)
            {
                return false;
            }

            _session = new DialogueSession(lines);
            _session.Start();

            if (_root != null)
            {
                _root.SetActive(true);
            }

            UpdateText();
            return true;
        }

        public bool Advance()
        {
            if (!IsOpen)
            {
                return false;
            }

            var stillActive = _session.Advance();
            if (stillActive)
            {
                UpdateText();
            }
            else
            {
                Close();
            }

            return stillActive;
        }

        private void OnAdvancePerformed(InputAction.CallbackContext context)
        {
            Advance();
        }

        private void Close()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }

            Closed?.Invoke();
        }

        private void UpdateText()
        {
            if (_lineText != null)
            {
                _lineText.text = _session.CurrentLine;
            }
        }
    }
}
