using System.Collections;
using System.Collections.Generic;
using FloatingIslandsRpg.Presentation.Menu;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Menu
{
    public sealed class GameMenuControllerTests
    {
        private GameObject _rootObject;
        private GameObject _panelObject;
        private Button _openButton;
        private Button _closeButton;
        private GameObject[] _itemRowRoots;
        private Text[] _itemRowTexts;
        private Button[] _itemRowButtons;
        private GameObject[] _weaponRowRoots;
        private Text[] _weaponRowTexts;
        private Button[] _weaponRowButtons;
        private Text _equippedWeaponText;
        private Text _equippedArmorText;
        private Button _unequipWeaponButton;
        private Button _unequipArmorButton;
        private Text _statsText;
        private GameMenuController _controller;

        [TearDown]
        public void TearDown()
        {
            if (_rootObject != null)
            {
                Object.DestroyImmediate(_rootObject);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private static object CreateItemRow(GameObject root, Text infoText, Button useButton)
        {
            var row = (object)default(ItemRowView);
            row = System.Activator.CreateInstance(typeof(ItemRowView));
            SetPrivateField(row, "_root", root);
            SetPrivateField(row, "_infoText", infoText);
            SetPrivateField(row, "_useButton", useButton);
            return row;
        }

        private static object CreateEquipmentRow(GameObject root, Text nameText, Button equipButton)
        {
            var row = System.Activator.CreateInstance(typeof(EquipmentRowView));
            SetPrivateField(row, "_root", root);
            SetPrivateField(row, "_nameText", nameText);
            SetPrivateField(row, "_equipButton", equipButton);
            return row;
        }

        private GameObject CreateButtonRow(string name, out Text infoText, out Button button)
        {
            var root = new GameObject(name);
            root.transform.SetParent(_rootObject.transform);
            infoText = root.AddComponent<Text>();

            var buttonObject = new GameObject(name + "Button");
            buttonObject.transform.SetParent(root.transform);
            buttonObject.AddComponent<Image>();
            button = buttonObject.AddComponent<Button>();

            return root;
        }

        private IEnumerator BuildScene(int itemRowCount = 2, int weaponRowCount = 2)
        {
            _rootObject = new GameObject("Root");
            _rootObject.SetActive(false);

            _panelObject = new GameObject("Panel");
            _panelObject.transform.SetParent(_rootObject.transform);

            var openButtonObject = new GameObject("OpenButton");
            openButtonObject.transform.SetParent(_rootObject.transform);
            openButtonObject.AddComponent<Image>();
            _openButton = openButtonObject.AddComponent<Button>();

            var closeButtonObject = new GameObject("CloseButton");
            closeButtonObject.transform.SetParent(_panelObject.transform);
            closeButtonObject.AddComponent<Image>();
            _closeButton = closeButtonObject.AddComponent<Button>();

            _itemRowRoots = new GameObject[itemRowCount];
            _itemRowTexts = new Text[itemRowCount];
            _itemRowButtons = new Button[itemRowCount];
            var itemRows = System.Array.CreateInstance(typeof(ItemRowView), itemRowCount);
            for (var i = 0; i < itemRowCount; i++)
            {
                _itemRowRoots[i] = CreateButtonRow("Item" + i, out var infoText, out var useButton);
                _itemRowTexts[i] = infoText;
                _itemRowButtons[i] = useButton;
                itemRows.SetValue(CreateItemRow(_itemRowRoots[i], infoText, useButton), i);
            }

            _weaponRowRoots = new GameObject[weaponRowCount];
            _weaponRowTexts = new Text[weaponRowCount];
            _weaponRowButtons = new Button[weaponRowCount];
            var weaponRows = System.Array.CreateInstance(typeof(EquipmentRowView), weaponRowCount);
            for (var i = 0; i < weaponRowCount; i++)
            {
                _weaponRowRoots[i] = CreateButtonRow("Weapon" + i, out var nameText, out var equipButton);
                _weaponRowTexts[i] = nameText;
                _weaponRowButtons[i] = equipButton;
                weaponRows.SetValue(CreateEquipmentRow(_weaponRowRoots[i], nameText, equipButton), i);
            }

            var armorRows = System.Array.CreateInstance(typeof(EquipmentRowView), 0);

            var equippedWeaponObject = new GameObject("EquippedWeaponText");
            equippedWeaponObject.transform.SetParent(_panelObject.transform);
            _equippedWeaponText = equippedWeaponObject.AddComponent<Text>();

            var equippedArmorObject = new GameObject("EquippedArmorText");
            equippedArmorObject.transform.SetParent(_panelObject.transform);
            _equippedArmorText = equippedArmorObject.AddComponent<Text>();

            var unequipWeaponObject = new GameObject("UnequipWeaponButton");
            unequipWeaponObject.transform.SetParent(_panelObject.transform);
            unequipWeaponObject.AddComponent<Image>();
            _unequipWeaponButton = unequipWeaponObject.AddComponent<Button>();

            var unequipArmorObject = new GameObject("UnequipArmorButton");
            unequipArmorObject.transform.SetParent(_panelObject.transform);
            unequipArmorObject.AddComponent<Image>();
            _unequipArmorButton = unequipArmorObject.AddComponent<Button>();

            var statsObject = new GameObject("StatsText");
            statsObject.transform.SetParent(_panelObject.transform);
            _statsText = statsObject.AddComponent<Text>();

            _controller = _rootObject.AddComponent<GameMenuController>();
            SetPrivateField(_controller, "_root", _panelObject);
            SetPrivateField(_controller, "_openButton", _openButton);
            SetPrivateField(_controller, "_closeButton", _closeButton);
            SetPrivateField(_controller, "_itemRows", itemRows);
            SetPrivateField(_controller, "_weaponRows", weaponRows);
            SetPrivateField(_controller, "_armorRows", armorRows);
            SetPrivateField(_controller, "_equippedWeaponText", _equippedWeaponText);
            SetPrivateField(_controller, "_equippedArmorText", _equippedArmorText);
            SetPrivateField(_controller, "_unequipWeaponButton", _unequipWeaponButton);
            SetPrivateField(_controller, "_unequipArmorButton", _unequipArmorButton);
            SetPrivateField(_controller, "_statsText", _statsText);

            _rootObject.SetActive(true);

            yield return null;
        }

        private static MenuViewModel CreateViewModel(
            IReadOnlyList<ItemRowViewModel> items = null,
            IReadOnlyList<EquipmentRowViewModel> weapons = null,
            IReadOnlyList<EquipmentRowViewModel> armors = null)
        {
            return new MenuViewModel(
                items ?? new List<ItemRowViewModel>(),
                weapons ?? new List<EquipmentRowViewModel>(),
                armors ?? new List<EquipmentRowViewModel>(),
                "None", "None", false, false, 20, 20, 5, 3);
        }

        [UnityTest]
        public IEnumerator Start_MenuStartsClosed()
        {
            yield return BuildScene();

            Assert.IsFalse(_controller.IsOpen);
        }

        [UnityTest]
        public IEnumerator OpenButtonClick_OpensMenuAndRaisesMenuOpened()
        {
            yield return BuildScene();
            var opened = false;
            _controller.MenuOpened += () => opened = true;

            _openButton.onClick.Invoke();

            Assert.IsTrue(_controller.IsOpen);
            Assert.IsTrue(opened);
        }

        [UnityTest]
        public IEnumerator CloseButtonClick_ClosesMenuAndRaisesMenuClosed()
        {
            yield return BuildScene();
            _controller.Open();
            var closed = false;
            _controller.MenuClosed += () => closed = true;

            _closeButton.onClick.Invoke();

            Assert.IsFalse(_controller.IsOpen);
            Assert.IsTrue(closed);
        }

        [UnityTest]
        public IEnumerator OpenButtonClick_WhileAlreadyOpen_DoesNotRaiseMenuOpenedTwice()
        {
            yield return BuildScene();
            var openCount = 0;
            _controller.MenuOpened += () => openCount++;

            _openButton.onClick.Invoke();
            _openButton.onClick.Invoke();

            Assert.AreEqual(1, openCount);
        }

        [UnityTest]
        public IEnumerator Refresh_PopulatesItemRowWithNameQuantityAndDescription()
        {
            yield return BuildScene();
            var items = new List<ItemRowViewModel> { new ItemRowViewModel("Small Potion", 3, "Restores 20 HP", true) };

            _controller.Refresh(CreateViewModel(items: items));

            StringAssert.Contains("Small Potion", _itemRowTexts[0].text);
            StringAssert.Contains("x3", _itemRowTexts[0].text);
            StringAssert.Contains("Restores 20 HP", _itemRowTexts[0].text);
            Assert.IsTrue(_itemRowButtons[0].interactable);
        }

        [UnityTest]
        public IEnumerator Refresh_ItemQuantityZero_ShowsZeroAndDisablesUseButton()
        {
            yield return BuildScene();
            var items = new List<ItemRowViewModel> { new ItemRowViewModel("Small Potion", 0, "Restores 20 HP", false) };

            _controller.Refresh(CreateViewModel(items: items));

            StringAssert.Contains("x0", _itemRowTexts[0].text);
            Assert.IsFalse(_itemRowButtons[0].interactable);
        }

        [UnityTest]
        public IEnumerator Refresh_FewerItemsThanRows_HidesUnusedRows()
        {
            yield return BuildScene(itemRowCount: 2);
            var items = new List<ItemRowViewModel> { new ItemRowViewModel("Small Potion", 1, "Restores 20 HP", true) };

            _controller.Refresh(CreateViewModel(items: items));

            Assert.IsTrue(_itemRowRoots[0].activeSelf);
            Assert.IsFalse(_itemRowRoots[1].activeSelf);
        }

        [UnityTest]
        public IEnumerator UseButtonClick_RaisesUsePotionRequestedWithRowIndex()
        {
            yield return BuildScene();
            var items = new List<ItemRowViewModel>
            {
                new ItemRowViewModel("Small Potion", 1, "Restores 20 HP", true),
                new ItemRowViewModel("Large Potion", 1, "Restores 50 HP", true)
            };
            _controller.Refresh(CreateViewModel(items: items));
            var requestedIndex = -1;
            _controller.UsePotionRequested += index => requestedIndex = index;

            _itemRowButtons[1].onClick.Invoke();

            Assert.AreEqual(1, requestedIndex);
        }

        [UnityTest]
        public IEnumerator EquipButtonClick_RaisesEquipWeaponRequestedWithRowIndex()
        {
            yield return BuildScene();
            var weapons = new List<EquipmentRowViewModel>
            {
                new EquipmentRowViewModel("Rusty Sword", false, true),
                new EquipmentRowViewModel("Sky Blade", false, true)
            };
            _controller.Refresh(CreateViewModel(weapons: weapons));
            var requestedIndex = -1;
            _controller.EquipWeaponRequested += index => requestedIndex = index;

            _weaponRowButtons[0].onClick.Invoke();

            Assert.AreEqual(0, requestedIndex);
        }

        [UnityTest]
        public IEnumerator Refresh_EquippedWeapon_ShowsEquippedSuffixAndDisablesEquipButton()
        {
            yield return BuildScene();
            var weapons = new List<EquipmentRowViewModel> { new EquipmentRowViewModel("Rusty Sword", true, false) };

            _controller.Refresh(CreateViewModel(weapons: weapons));

            StringAssert.Contains("Equipped", _weaponRowTexts[0].text);
            Assert.IsFalse(_weaponRowButtons[0].interactable);
        }

        [UnityTest]
        public IEnumerator UnequipWeaponButtonClick_RaisesUnequipWeaponRequested()
        {
            yield return BuildScene();
            var requested = false;
            _controller.UnequipWeaponRequested += () => requested = true;

            _unequipWeaponButton.onClick.Invoke();

            Assert.IsTrue(requested);
        }

        [UnityTest]
        public IEnumerator Refresh_UpdatesStatsTextWithHpAttackDefense()
        {
            yield return BuildScene();

            _controller.Refresh(CreateViewModel());

            StringAssert.Contains("20/20", _statsText.text);
            StringAssert.Contains("5", _statsText.text);
            StringAssert.Contains("3", _statsText.text);
        }

        [UnityTest]
        public IEnumerator Refresh_NullModel_DoesNotThrow()
        {
            yield return BuildScene();

            Assert.DoesNotThrow(() => _controller.Refresh(null));
        }
    }
}
