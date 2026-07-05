using System;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Presentation.Items
{
    // Minimal inventory/equipment confirmation UI (PROJECT.md T-024: "現在の装備と所持数を確認
    // できる" / "本格的なメニュー画面は実装しない"). Holds no inventory/equipment rules itself --
    // Refresh() is told exactly what text to show, and button clicks only raise events for
    // Composition to act on.
    public sealed class InventoryPanelController : MonoBehaviour
    {
        [SerializeField] private Text _statusText;
        [SerializeField] private Button _usePotionButton;
        [SerializeField] private Button _equipWeaponButton;
        [SerializeField] private Button _equipArmorButton;

        private bool _subscribed;

        public event Action UsePotionRequested;
        public event Action EquipWeaponRequested;
        public event Action EquipArmorRequested;

        private void OnEnable()
        {
            if (_subscribed)
            {
                return;
            }

            if (_usePotionButton != null)
            {
                _usePotionButton.onClick.AddListener(OnUsePotionClicked);
            }

            if (_equipWeaponButton != null)
            {
                _equipWeaponButton.onClick.AddListener(OnEquipWeaponClicked);
            }

            if (_equipArmorButton != null)
            {
                _equipArmorButton.onClick.AddListener(OnEquipArmorClicked);
            }

            _subscribed = true;
        }

        private void OnDisable()
        {
            if (!_subscribed)
            {
                return;
            }

            if (_usePotionButton != null)
            {
                _usePotionButton.onClick.RemoveListener(OnUsePotionClicked);
            }

            if (_equipWeaponButton != null)
            {
                _equipWeaponButton.onClick.RemoveListener(OnEquipWeaponClicked);
            }

            if (_equipArmorButton != null)
            {
                _equipArmorButton.onClick.RemoveListener(OnEquipArmorClicked);
            }

            _subscribed = false;
        }

        public void Refresh(string statusText)
        {
            if (_statusText != null)
            {
                _statusText.text = statusText;
            }
        }

        private void OnUsePotionClicked() => UsePotionRequested?.Invoke();
        private void OnEquipWeaponClicked() => EquipWeaponRequested?.Invoke();
        private void OnEquipArmorClicked() => EquipArmorRequested?.Invoke();
    }
}
