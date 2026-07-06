using System;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Presentation.Menu
{
    // The shared Status/Inventory/Equipment menu (PROJECT.md T-026), used identically from
    // Village, Field, and Dungeon. Holds no Inventory/Equipment/Save business rules itself --
    // Refresh() is told exactly what to display, and every button only raises an event for
    // Composition to act on via the existing Application UseCases (AddItem/ConsumeItem/
    // EquipItem/UnequipItem, all from T-024). Row button listeners are added once in OnEnable
    // (this GameObject itself is never disabled -- only the child _root panel is shown/hidden
    // for open/close), so there is no double-registration risk from repeated open/close cycles.
    public sealed class GameMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private ItemRowView[] _itemRows;
        [SerializeField] private EquipmentRowView[] _weaponRows;
        [SerializeField] private EquipmentRowView[] _armorRows;
        [SerializeField] private Text _equippedWeaponText;
        [SerializeField] private Text _equippedArmorText;
        [SerializeField] private Button _unequipWeaponButton;
        [SerializeField] private Button _unequipArmorButton;
        [SerializeField] private Text _statsText;

        private bool _subscribed;

        public bool IsOpen => _root != null && _root.activeSelf;

        public event Action MenuOpened;
        public event Action MenuClosed;
        public event Action<int> UsePotionRequested;
        public event Action<int> EquipWeaponRequested;
        public event Action<int> EquipArmorRequested;
        public event Action UnequipWeaponRequested;
        public event Action UnequipArmorRequested;

        private void Awake()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_subscribed)
            {
                return;
            }

            if (_openButton != null)
            {
                _openButton.onClick.AddListener(Open);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }

            if (_unequipWeaponButton != null)
            {
                _unequipWeaponButton.onClick.AddListener(OnUnequipWeaponClicked);
            }

            if (_unequipArmorButton != null)
            {
                _unequipArmorButton.onClick.AddListener(OnUnequipArmorClicked);
            }

            SubscribeItemRowButtons(_itemRows, OnItemUseClicked);
            SubscribeEquipmentRowButtons(_weaponRows, OnWeaponEquipClicked);
            SubscribeEquipmentRowButtons(_armorRows, OnArmorEquipClicked);

            _subscribed = true;
        }

        private void OnDisable()
        {
            if (!_subscribed)
            {
                return;
            }

            if (_openButton != null)
            {
                _openButton.onClick.RemoveListener(Open);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Close);
            }

            if (_unequipWeaponButton != null)
            {
                _unequipWeaponButton.onClick.RemoveListener(OnUnequipWeaponClicked);
            }

            if (_unequipArmorButton != null)
            {
                _unequipArmorButton.onClick.RemoveListener(OnUnequipArmorClicked);
            }

            UnsubscribeItemRowButtons(_itemRows);
            UnsubscribeEquipmentRowButtons(_weaponRows);
            UnsubscribeEquipmentRowButtons(_armorRows);

            _subscribed = false;
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            if (_root != null)
            {
                _root.SetActive(true);
            }

            MenuOpened?.Invoke();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            if (_root != null)
            {
                _root.SetActive(false);
            }

            MenuClosed?.Invoke();
        }

        // Renders exactly what it is given; every quantity/description/enabled flag (including
        // the "quantity just dropped to 0" case) is Composition's responsibility to compute
        // correctly before calling this (PROJECT.md T-026: "数量0になったItemの安全な表示更新").
        public void Refresh(MenuViewModel model)
        {
            if (model == null)
            {
                return;
            }

            RefreshItemRows(model);
            RefreshEquipmentRows(_weaponRows, model.Weapons);
            RefreshEquipmentRows(_armorRows, model.Armors);

            if (_equippedWeaponText != null)
            {
                _equippedWeaponText.text = $"Weapon: {model.EquippedWeaponName}";
            }

            if (_equippedArmorText != null)
            {
                _equippedArmorText.text = $"Armor: {model.EquippedArmorName}";
            }

            if (_unequipWeaponButton != null)
            {
                _unequipWeaponButton.interactable = model.CanUnequipWeapon;
            }

            if (_unequipArmorButton != null)
            {
                _unequipArmorButton.interactable = model.CanUnequipArmor;
            }

            if (_statsText != null)
            {
                _statsText.text = $"HP: {model.CurrentHp}/{model.MaxHp}  ATK: {model.CurrentAttack}  DEF: {model.CurrentDefense}";
            }
        }

        private void RefreshItemRows(MenuViewModel model)
        {
            if (_itemRows == null)
            {
                return;
            }

            for (var i = 0; i < _itemRows.Length; i++)
            {
                var row = _itemRows[i];
                var hasData = model.Items != null && i < model.Items.Count;

                if (row.Root != null)
                {
                    row.Root.SetActive(hasData);
                }

                if (!hasData)
                {
                    continue;
                }

                var data = model.Items[i];

                if (row.InfoText != null)
                {
                    row.InfoText.text = $"{data.Name} x{data.Quantity} - {data.Description}";
                }

                if (row.UseButton != null)
                {
                    row.UseButton.interactable = data.CanUse;
                }
            }
        }

        private static void RefreshEquipmentRows(EquipmentRowView[] rows, System.Collections.Generic.IReadOnlyList<EquipmentRowViewModel> data)
        {
            if (rows == null)
            {
                return;
            }

            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                var hasData = data != null && i < data.Count;

                if (row.Root != null)
                {
                    row.Root.SetActive(hasData);
                }

                if (!hasData)
                {
                    continue;
                }

                var entry = data[i];

                if (row.NameText != null)
                {
                    row.NameText.text = entry.IsEquipped ? $"{entry.Name} (Equipped)" : entry.Name;
                }

                if (row.EquipButton != null)
                {
                    row.EquipButton.interactable = entry.CanEquip;
                }
            }
        }

        // Each row's button is bound to its own fixed index via a per-index closure, which
        // UnityEvent cannot remove by reference later -- OnDisable instead calls
        // RemoveAllListeners() on every row button, which is safe because Composition never
        // attaches any other listener to these buttons.
        private void SubscribeItemRowButtons(ItemRowView[] rows, Action<int> handler)
        {
            if (rows == null)
            {
                return;
            }

            for (var i = 0; i < rows.Length; i++)
            {
                var index = i;
                rows[i].UseButton?.onClick.AddListener(() => handler(index));
            }
        }

        private static void UnsubscribeItemRowButtons(ItemRowView[] rows)
        {
            if (rows == null)
            {
                return;
            }

            foreach (var row in rows)
            {
                row.UseButton?.onClick.RemoveAllListeners();
            }
        }

        private void SubscribeEquipmentRowButtons(EquipmentRowView[] rows, Action<int> handler)
        {
            if (rows == null)
            {
                return;
            }

            for (var i = 0; i < rows.Length; i++)
            {
                var index = i;
                rows[i].EquipButton?.onClick.AddListener(() => handler(index));
            }
        }

        private static void UnsubscribeEquipmentRowButtons(EquipmentRowView[] rows)
        {
            if (rows == null)
            {
                return;
            }

            foreach (var row in rows)
            {
                row.EquipButton?.onClick.RemoveAllListeners();
            }
        }

        private void OnItemUseClicked(int index) => UsePotionRequested?.Invoke(index);
        private void OnWeaponEquipClicked(int index) => EquipWeaponRequested?.Invoke(index);
        private void OnArmorEquipClicked(int index) => EquipArmorRequested?.Invoke(index);
        private void OnUnequipWeaponClicked() => UnequipWeaponRequested?.Invoke();
        private void OnUnequipArmorClicked() => UnequipArmorRequested?.Invoke();
    }
}
