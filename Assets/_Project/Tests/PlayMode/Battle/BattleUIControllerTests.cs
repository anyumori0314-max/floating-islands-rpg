using System.Reflection;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Presentation.Battle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Battle
{
    public sealed class BattleUIControllerTests
    {
        public sealed class FixedRandomSource : IRandomSource
        {
            private readonly double _value;

            public FixedRandomSource(double value)
            {
                _value = value;
            }

            public double NextDouble() => _value;
        }

        private GameObject _controllerObject;
        private BattleUIController _controller;
        private Button _attackButton;
        private Text _playerHpText;
        private Text _enemyHpText;
        private Text _logText;
        private GameObject _resultPanel;
        private Text _resultText;

        [SetUp]
        public void SetUp()
        {
            _controllerObject = new GameObject("BattleUIController");
            _controllerObject.SetActive(false);

            var buttonObject = new GameObject("AttackButton");
            buttonObject.transform.SetParent(_controllerObject.transform);
            buttonObject.AddComponent<Image>();
            _attackButton = buttonObject.AddComponent<Button>();

            _playerHpText = CreateText("PlayerHpText");
            _enemyHpText = CreateText("EnemyHpText");
            _logText = CreateText("LogText");
            _resultText = CreateText("ResultText");

            _resultPanel = new GameObject("ResultPanel");
            _resultPanel.transform.SetParent(_controllerObject.transform);
            _resultText.transform.SetParent(_resultPanel.transform);

            _controller = _controllerObject.AddComponent<BattleUIController>();

            SetPrivateField(_controller, "_attackButton", _attackButton);
            SetPrivateField(_controller, "_playerHpText", _playerHpText);
            SetPrivateField(_controller, "_enemyHpText", _enemyHpText);
            SetPrivateField(_controller, "_logText", _logText);
            SetPrivateField(_controller, "_resultPanel", _resultPanel);
            SetPrivateField(_controller, "_resultText", _resultText);

            _controllerObject.SetActive(true);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_controllerObject);
            Object.DestroyImmediate(_resultPanel);
        }

        private Text CreateText(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_controllerObject.transform);
            return go.AddComponent<Text>();
        }

        private static BattleSession CreateSession(
            int playerAgility = 10, int playerAttack = 10, int playerDefense = 2, int playerMaxHp = 20,
            int enemyAgility = 5, int enemyAttack = 8, int enemyDefense = 1, int enemyMaxHp = 15,
            double randomRoll = 0.0)
        {
            var playerStats = new CharacterStats(1, playerMaxHp, 5, playerAttack, playerDefense, playerAgility, 0);
            var enemyStats = new CharacterStats(1, enemyMaxHp, 0, enemyAttack, enemyDefense, enemyAgility, 0);
            var player = new BattleParticipantState(playerStats);
            var enemy = new BattleParticipantState(enemyStats);
            return new BattleSession(player, enemy, new FixedRandomSource(randomRoll));
        }

        [Test]
        public void Bind_InitialState_ShowsHpAndHidesResultPanelAndEnablesAttackButton()
        {
            var session = CreateSession();

            _controller.Bind(session);

            Assert.AreEqual("HP 20/20", _playerHpText.text);
            Assert.AreEqual("HP 15/15", _enemyHpText.text);
            Assert.IsFalse(_resultPanel.activeSelf);
            Assert.IsTrue(_attackButton.interactable);
            Assert.AreEqual(string.Empty, _logText.text);
        }

        [Test]
        public void AttackButtonClick_RollBelowHitChance_UpdatesHpAndLogsHit()
        {
            var session = CreateSession(randomRoll: 0.0);
            _controller.Bind(session);

            _attackButton.onClick.Invoke();

            Assert.AreEqual($"HP {session.Enemy.CurrentHp}/{session.Enemy.Stats.MaxHp}", _enemyHpText.text);
            StringAssert.Contains("hits for", _logText.text);
            Assert.IsTrue(_attackButton.interactable);
        }

        [Test]
        public void AttackButtonClick_RollAboveHitChance_LogsMiss()
        {
            var session = CreateSession(randomRoll: 0.99);
            _controller.Bind(session);

            _attackButton.onClick.Invoke();

            StringAssert.Contains("misses", _logText.text);
        }

        [Test]
        public void AttackButtonClick_EnemyDefeated_ShowsVictoryResultAndDisablesButton()
        {
            var session = CreateSession(enemyMaxHp: 5, randomRoll: 0.0);
            _controller.Bind(session);

            _attackButton.onClick.Invoke();

            Assert.AreEqual(BattleOutcome.PlayerVictory, _controller.CurrentOutcome);
            Assert.IsTrue(_resultPanel.activeSelf);
            Assert.AreEqual("Victory!", _resultText.text);
            Assert.IsFalse(_attackButton.interactable);
        }

        [Test]
        public void AttackButtonClick_PlayerDefeated_ShowsDefeatResultAndDisablesButton()
        {
            var session = CreateSession(
                playerAgility: 5, playerMaxHp: 10, playerDefense: 1,
                enemyAgility: 20, enemyAttack: 20,
                randomRoll: 0.0);
            _controller.Bind(session);

            _attackButton.onClick.Invoke();

            Assert.AreEqual(BattleOutcome.PlayerDefeat, _controller.CurrentOutcome);
            Assert.IsTrue(_resultPanel.activeSelf);
            Assert.AreEqual("Defeat...", _resultText.text);
            Assert.IsFalse(_attackButton.interactable);
        }

        [Test]
        public void AttackButtonClick_AfterBattleEnded_DoesNotExecuteAnotherTurn()
        {
            var session = CreateSession(enemyMaxHp: 5, randomRoll: 0.0);
            _controller.Bind(session);

            _attackButton.onClick.Invoke();
            var turnNumberAfterVictory = session.TurnNumber;

            _attackButton.onClick.Invoke();

            Assert.AreEqual(turnNumberAfterVictory, session.TurnNumber);
        }

        [Test]
        public void OnDisable_RemovesListener_ClickNoLongerExecutesTurn()
        {
            var session = CreateSession(randomRoll: 0.0);
            _controller.Bind(session);

            _controllerObject.SetActive(false);

            _attackButton.onClick.Invoke();

            Assert.AreEqual(0, session.TurnNumber);
        }

        [Test]
        public void CurrentOutcome_MatchesBattleSessionOutcomeThroughoutBattle()
        {
            var session = CreateSession(randomRoll: 0.0);
            _controller.Bind(session);

            Assert.AreEqual(session.Outcome, _controller.CurrentOutcome);

            _attackButton.onClick.Invoke();

            Assert.AreEqual(session.Outcome, _controller.CurrentOutcome);
        }

        [Test]
        public void ShowReward_WithoutLevelUp_AppendsExperienceLineOnly()
        {
            var session = CreateSession();
            _controller.Bind(session);

            _controller.ShowReward(experienceGained: 12, leveledUp: false, newLevel: 1);

            Assert.AreEqual("EXP +12", _logText.text);
        }

        [Test]
        public void ShowReward_WithLevelUp_AppendsExperienceAndLevelUpLines()
        {
            var session = CreateSession();
            _controller.Bind(session);

            _controller.ShowReward(experienceGained: 50, leveledUp: true, newLevel: 3);

            StringAssert.Contains("EXP +50", _logText.text);
            StringAssert.Contains("Level Up! Lv.3", _logText.text);
        }

        [Test]
        public void ShowReward_AfterBattleLog_AppendsRewardAfterExistingLog()
        {
            var session = CreateSession(randomRoll: 0.0);
            _controller.Bind(session);
            _attackButton.onClick.Invoke();
            var logAfterAttack = _logText.text;

            _controller.ShowReward(experienceGained: 5, leveledUp: false, newLevel: 1);

            StringAssert.StartsWith(logAfterAttack, _logText.text);
            StringAssert.Contains("EXP +5", _logText.text);
        }
    }
}
