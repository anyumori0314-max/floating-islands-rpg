using System.Reflection;
using FloatingIslandsRpg.Presentation.Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Items
{
    public sealed class InventoryPanelControllerTests
    {
        private GameObject _controllerObject;
        private InventoryPanelController _controller;
        private Text _statusText;
        private Button _usePotionButton;
        private Button _equipWeaponButton;
        private Button _equipArmorButton;

        [SetUp]
        public void SetUp()
        {
            _controllerObject = new GameObject("InventoryPanelController");
            _controllerObject.SetActive(false);

            _statusText = CreateText("StatusText");
            _usePotionButton = CreateButton("UsePotionButton");
            _equipWeaponButton = CreateButton("EquipWeaponButton");
            _equipArmorButton = CreateButton("EquipArmorButton");

            _controller = _controllerObject.AddComponent<InventoryPanelController>();
            SetPrivateField(_controller, "_statusText", _statusText);
            SetPrivateField(_controller, "_usePotionButton", _usePotionButton);
            SetPrivateField(_controller, "_equipWeaponButton", _equipWeaponButton);
            SetPrivateField(_controller, "_equipArmorButton", _equipArmorButton);

            _controllerObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_controllerObject);
        }

        private Text CreateText(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_controllerObject.transform);
            return go.AddComponent<Text>();
        }

        private Button CreateButton(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_controllerObject.transform);
            go.AddComponent<Image>();
            return go.AddComponent<Button>();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        [Test]
        public void Refresh_SetsStatusText()
        {
            _controller.Refresh("Potion x2");

            Assert.AreEqual("Potion x2", _statusText.text);
        }

        [Test]
        public void UsePotionButtonClick_RaisesUsePotionRequested()
        {
            var raised = false;
            _controller.UsePotionRequested += () => raised = true;

            _usePotionButton.onClick.Invoke();

            Assert.IsTrue(raised);
        }

        [Test]
        public void EquipWeaponButtonClick_RaisesEquipWeaponRequested()
        {
            var raised = false;
            _controller.EquipWeaponRequested += () => raised = true;

            _equipWeaponButton.onClick.Invoke();

            Assert.IsTrue(raised);
        }

        [Test]
        public void EquipArmorButtonClick_RaisesEquipArmorRequested()
        {
            var raised = false;
            _controller.EquipArmorRequested += () => raised = true;

            _equipArmorButton.onClick.Invoke();

            Assert.IsTrue(raised);
        }

        [Test]
        public void OnDisable_RemovesListeners_ClicksNoLongerRaiseEvents()
        {
            var raised = false;
            _controller.UsePotionRequested += () => raised = true;

            _controllerObject.SetActive(false);
            _usePotionButton.onClick.Invoke();

            Assert.IsFalse(raised);
        }
    }
}
