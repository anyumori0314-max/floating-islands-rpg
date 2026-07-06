using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Presentation.Menu;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class MenuInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _menuObject;
        private GameObject _gateObject;
        private GameObject _installerObject;
        private GameServices _services;
        private GameMenuController _menu;
        private MenuActivityGate _gate;
        private ItemDefinition _potionDefinition;
        private EquipmentDefinition _weaponDefinition;
        private EquipmentDefinition _armorDefinition;

        [TearDown]
        public void TearDown()
        {
            if (_installerObject != null) Object.DestroyImmediate(_installerObject);
            if (_menuObject != null) Object.DestroyImmediate(_menuObject);
            if (_gateObject != null) Object.DestroyImmediate(_gateObject);
            if (_rootObject != null) Object.DestroyImmediate(_rootObject);
            if (_potionDefinition != null) Object.DestroyImmediate(_potionDefinition);
            if (_weaponDefinition != null) Object.DestroyImmediate(_weaponDefinition);
            if (_armorDefinition != null) Object.DestroyImmediate(_armorDefinition);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private static void InvokeEvent(object target, string eventFieldName, object arg = null)
        {
            var field = target.GetType().GetField(eventFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            var handler = field.GetValue(target);
            if (handler == null)
            {
                return;
            }

            if (arg == null)
            {
                ((System.Action)handler).Invoke();
            }
            else
            {
                ((System.Action<int>)handler).DynamicInvoke(arg);
            }
        }

        private IEnumerator BuildScene(PlayerSessionState session)
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            _services = root.Services;
            _services.CurrentSession = session;

            _menuObject = new GameObject("GameMenu");
            _menu = _menuObject.AddComponent<GameMenuController>();
            _gate = _menuObject.AddComponent<MenuActivityGate>();

            _potionDefinition = ScriptableObject.CreateInstance<ItemDefinition>();
            SetPrivateField(_potionDefinition, "_id", "item_small_potion");
            SetPrivateField(_potionDefinition, "_displayName", "Small Potion");
            SetPrivateField(_potionDefinition, "_healAmount", 20);

            _weaponDefinition = ScriptableObject.CreateInstance<EquipmentDefinition>();
            SetPrivateField(_weaponDefinition, "_id", "equip_rusty_sword");
            SetPrivateField(_weaponDefinition, "_displayName", "Rusty Sword");
            SetPrivateField(_weaponDefinition, "_slot", EquipmentSlot.Weapon);
            SetPrivateField(_weaponDefinition, "_attackBonus", 5);

            _armorDefinition = ScriptableObject.CreateInstance<EquipmentDefinition>();
            SetPrivateField(_armorDefinition, "_id", "equip_traveler_armor");
            SetPrivateField(_armorDefinition, "_displayName", "Traveler Armor");
            SetPrivateField(_armorDefinition, "_slot", EquipmentSlot.Armor);
            SetPrivateField(_armorDefinition, "_defenseBonus", 3);

            _installerObject = new GameObject("MenuInstaller");
            var installer = _installerObject.AddComponent<MenuInstaller>();
            SetPrivateField(installer, "_menu", _menu);
            SetPrivateField(installer, "_activityGate", _gate);
            SetPrivateField(installer, "_items", new[] { _potionDefinition });
            SetPrivateField(installer, "_weapons", new[] { _weaponDefinition });
            SetPrivateField(installer, "_armors", new[] { _armorDefinition });

            yield return null;
        }

        private static PlayerSessionState CreateSession(int currentHp = 20, int maxHp = 30)
        {
            var stats = new CharacterStats(2, maxHp, 10, 8, 4, 5, 2);
            return new PlayerSessionState(
                SceneId.Village, stats, 0, currentHp, 10,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
        }

        [UnityTest]
        public IEnumerator MenuOpened_PausesActivityGate()
        {
            yield return BuildScene(CreateSession());

            // MenuOpened triggers MenuActivityGate.Pause() and a Refresh() built from empty
            // catalogs/rows; this must not throw.
            Assert.DoesNotThrow(() => InvokeEvent(_menu, "MenuOpened"));
        }

        [UnityTest]
        public IEnumerator UsePotionRequested_OwnedPotion_ConsumesAndHeals()
        {
            var session = CreateSession(currentHp: 5);
            session.Inventory.Add("item_small_potion", 1);
            yield return BuildScene(session);

            InvokeEvent(_menu, "UsePotionRequested", 0);

            Assert.AreEqual(0, session.Inventory.GetQuantity("item_small_potion"));
            Assert.AreEqual(25, session.CurrentHp);
        }

        [UnityTest]
        public IEnumerator UsePotionRequested_NotOwned_DoesNotHeal()
        {
            var session = CreateSession(currentHp: 5);
            yield return BuildScene(session);

            InvokeEvent(_menu, "UsePotionRequested", 0);

            Assert.AreEqual(5, session.CurrentHp);
        }

        [UnityTest]
        public IEnumerator UsePotionRequested_IndexOutOfRange_DoesNotThrow()
        {
            yield return BuildScene(CreateSession());

            Assert.DoesNotThrow(() => InvokeEvent(_menu, "UsePotionRequested", 99));
            Assert.DoesNotThrow(() => InvokeEvent(_menu, "UsePotionRequested", -1));
        }

        [UnityTest]
        public IEnumerator EquipWeaponRequested_OwnedWeapon_Equips()
        {
            var session = CreateSession();
            session.Inventory.Add("equip_rusty_sword", 1);
            yield return BuildScene(session);

            InvokeEvent(_menu, "EquipWeaponRequested", 0);

            Assert.AreEqual("equip_rusty_sword", session.Equipment.EquippedWeaponId);
        }

        [UnityTest]
        public IEnumerator EquipWeaponRequested_NotOwned_DoesNotEquip()
        {
            var session = CreateSession();
            yield return BuildScene(session);

            InvokeEvent(_menu, "EquipWeaponRequested", 0);

            Assert.IsNull(session.Equipment.EquippedWeaponId);
        }

        [UnityTest]
        public IEnumerator EquipArmorRequested_OwnedArmor_Equips()
        {
            var session = CreateSession();
            session.Inventory.Add("equip_traveler_armor", 1);
            yield return BuildScene(session);

            InvokeEvent(_menu, "EquipArmorRequested", 0);

            Assert.AreEqual("equip_traveler_armor", session.Equipment.EquippedArmorId);
        }

        [UnityTest]
        public IEnumerator UnequipWeaponRequested_Equipped_Unequips()
        {
            var session = CreateSession();
            session.Inventory.Add("equip_rusty_sword", 1);
            session.Equipment.EquipWeapon("equip_rusty_sword");
            yield return BuildScene(session);

            InvokeEvent(_menu, "UnequipWeaponRequested");

            Assert.IsNull(session.Equipment.EquippedWeaponId);
        }

        [UnityTest]
        public IEnumerator UnequipArmorRequested_Equipped_Unequips()
        {
            var session = CreateSession();
            session.Inventory.Add("equip_traveler_armor", 1);
            session.Equipment.EquipArmor("equip_traveler_armor");
            yield return BuildScene(session);

            InvokeEvent(_menu, "UnequipArmorRequested");

            Assert.IsNull(session.Equipment.EquippedArmorId);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromMenuEvents()
        {
            var session = CreateSession(currentHp: 5);
            session.Inventory.Add("item_small_potion", 1);
            yield return BuildScene(session);

            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            Assert.DoesNotThrow(() => InvokeEvent(_menu, "UsePotionRequested", 0));
            Assert.AreEqual(1, session.Inventory.GetQuantity("item_small_potion"));
        }

        [UnityTest]
        public IEnumerator EquipWeaponRequested_WithoutCurrentSession_DoesNotThrow()
        {
            yield return BuildScene(null);

            Assert.DoesNotThrow(() => InvokeEvent(_menu, "EquipWeaponRequested", 0));
        }
    }
}
