using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Infrastructure.MasterData;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.MasterData
{
    public sealed class InitialPlayerDefinitionTests
    {
        private InitialPlayerDefinition _definition;

        [TearDown]
        public void TearDown()
        {
            if (_definition != null)
            {
                Object.DestroyImmediate(_definition);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private InitialPlayerDefinition CreateDefinition()
        {
            _definition = ScriptableObject.CreateInstance<InitialPlayerDefinition>();
            SetPrivateField(_definition, "_displayName", "Hero");
            SetPrivateField(_definition, "_minLevel", 1);
            SetPrivateField(_definition, "_maxLevel", 10);
            SetPrivateField(_definition, "_baseMaxHp", 20);
            SetPrivateField(_definition, "_baseMaxMp", 5);
            SetPrivateField(_definition, "_baseAttack", 5);
            SetPrivateField(_definition, "_baseDefense", 3);
            SetPrivateField(_definition, "_baseAgility", 5);
            SetPrivateField(_definition, "_baseMagic", 2);
            SetPrivateField(_definition, "_growthMaxHp", 4);
            SetPrivateField(_definition, "_growthMaxMp", 1);
            SetPrivateField(_definition, "_growthAttack", 2);
            SetPrivateField(_definition, "_growthDefense", 1);
            SetPrivateField(_definition, "_growthAgility", 1);
            SetPrivateField(_definition, "_growthMagic", 1);
            SetPrivateField(_definition, "_cumulativeExperienceByLevel", new[] { 0, 10, 30, 60, 100, 150, 210, 280, 360, 450 });
            return _definition;
        }

        [UnityTest]
        public IEnumerator ToGrowthProfile_ReturnsProfileMatchingSerializedFields()
        {
            var definition = CreateDefinition();

            var profile = definition.ToGrowthProfile();

            Assert.AreEqual(1, profile.MinLevel);
            Assert.AreEqual(10, profile.MaxLevel);
            Assert.AreEqual(20, profile.BaseMaxHp);
            Assert.AreEqual(4, profile.GrowthMaxHp);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ToInitialCharacterStats_ReturnsLevelOneStatsWithNoGrowthApplied()
        {
            var definition = CreateDefinition();

            var stats = definition.ToInitialCharacterStats();

            Assert.AreEqual(1, stats.Level);
            Assert.AreEqual(20, stats.MaxHp);
            Assert.AreEqual(5, stats.MaxMp);
            Assert.AreEqual(5, stats.Attack);
            Assert.AreEqual(3, stats.Defense);
            Assert.AreEqual(5, stats.Agility);
            Assert.AreEqual(2, stats.Magic);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DisplayName_ReturnsSerializedValue()
        {
            var definition = CreateDefinition();

            Assert.AreEqual("Hero", definition.DisplayName);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ToExperienceTable_ReturnsTableMatchingSerializedValues()
        {
            var definition = CreateDefinition();

            var table = definition.ToExperienceTable();

            Assert.AreEqual(10, table.MaxLevel);
            Assert.AreEqual(0, table.GetRequiredExperience(1));
            Assert.AreEqual(10, table.GetRequiredExperience(2));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ExperienceTableMaxLevel_MatchesGrowthProfileMaxLevel()
        {
            // PROJECT.md T-022/T-023: the experience curve and the growth data must agree on
            // how many levels exist.
            var definition = CreateDefinition();

            var table = definition.ToExperienceTable();
            var profile = definition.ToGrowthProfile();

            Assert.AreEqual(profile.MaxLevel - profile.MinLevel + 1, table.MaxLevel);
            yield return null;
        }
    }
}
