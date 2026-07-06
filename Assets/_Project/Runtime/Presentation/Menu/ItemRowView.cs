using System;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Presentation.Menu
{
    // One pre-built inventory row's UI references (PROJECT.md T-026). GameMenuController owns a
    // fixed-size array of these, assigned in the Inspector; Composition's actual item catalog can
    // be smaller (unused rows are hidden) but never larger than this array without a Scene change.
    // Name, quantity, and effect description share a single Text (e.g. "Small Potion x3 - Restores
    // 20 HP") to keep the row layout simple; Composition formats the full line via
    // ItemRowViewModel before Refresh() is called.
    [Serializable]
    public struct ItemRowView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _infoText;
        [SerializeField] private Button _useButton;

        public GameObject Root => _root;
        public Text InfoText => _infoText;
        public Button UseButton => _useButton;
    }
}
