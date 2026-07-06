using System;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Presentation.Menu
{
    // One pre-built weapon/armor row's UI references (PROJECT.md T-026). Mirrors ItemRowView;
    // GameMenuController owns fixed-size arrays of these for weapons and armors separately.
    [Serializable]
    public struct EquipmentRowView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _nameText;
        [SerializeField] private Button _equipButton;

        public GameObject Root => _root;
        public Text NameText => _nameText;
        public Button EquipButton => _equipButton;
    }
}
