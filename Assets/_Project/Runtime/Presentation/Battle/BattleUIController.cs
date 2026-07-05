using System;
using System.Text;
using FloatingIslandsRpg.Application.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Presentation.Battle
{
    public sealed class BattleUIController : MonoBehaviour
    {
        [SerializeField] private Button _attackButton;
        [SerializeField] private Text _playerHpText;
        [SerializeField] private Text _enemyHpText;
        [SerializeField] private Text _logText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private Text _resultText;

        private BattleSession _session;
        private bool _subscribed;

        public BattleOutcome CurrentOutcome => _session != null ? _session.Outcome : BattleOutcome.InProgress;

        public event Action<BattleOutcome> BattleEnded;

        private void OnEnable()
        {
            if (_attackButton != null && !_subscribed)
            {
                _attackButton.onClick.AddListener(OnAttackClicked);
                _subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (_attackButton != null && _subscribed)
            {
                _attackButton.onClick.RemoveListener(OnAttackClicked);
                _subscribed = false;
            }
        }

        public void Bind(BattleSession session)
        {
            _session = session;

            if (_resultPanel != null)
            {
                _resultPanel.SetActive(false);
            }

            if (_logText != null)
            {
                _logText.text = string.Empty;
            }

            RefreshHpDisplay();
            RefreshAttackButtonInteractable();
        }

        private void OnAttackClicked()
        {
            if (_session == null || _session.Outcome != BattleOutcome.InProgress)
            {
                return;
            }

            if (_attackButton != null)
            {
                _attackButton.interactable = false;
            }

            var turnResult = _session.ExecuteTurn(BattleCommand.Attack);

            RefreshHpDisplay();
            AppendLog(turnResult);

            if (turnResult.Outcome == BattleOutcome.InProgress)
            {
                RefreshAttackButtonInteractable();
            }
            else
            {
                ShowResult(turnResult.Outcome);
                BattleEnded?.Invoke(turnResult.Outcome);
            }
        }

        private void RefreshHpDisplay()
        {
            if (_session == null)
            {
                return;
            }

            if (_playerHpText != null)
            {
                _playerHpText.text = $"HP {_session.Player.CurrentHp}/{_session.Player.Stats.MaxHp}";
            }

            if (_enemyHpText != null)
            {
                _enemyHpText.text = $"HP {_session.Enemy.CurrentHp}/{_session.Enemy.Stats.MaxHp}";
            }
        }

        private void RefreshAttackButtonInteractable()
        {
            if (_attackButton != null)
            {
                _attackButton.interactable = _session != null && _session.Outcome == BattleOutcome.InProgress;
            }
        }

        private void AppendLog(BattleTurnResult turnResult)
        {
            if (_logText == null)
            {
                return;
            }

            var builder = new StringBuilder(_logText.text);

            foreach (var action in turnResult.Actions)
            {
                var actorName = action.ActorIsPlayer ? "Player" : "Enemy";
                var line = action.WasHit
                    ? $"{actorName} hits for {action.DamageDealt} damage."
                    : $"{actorName} misses.";

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(line);
            }

            _logText.text = builder.ToString();
        }

        // Minimal reward display (PROJECT.md T-023: "獲得経験値、レベルアップ有無、新レベルを最低限
        //表示する" -- no dedicated UI, animation, or effects). Composition calls this after
        // granting the reward; this controller performs no reward calculation itself.
        public void ShowReward(int experienceGained, bool leveledUp, int newLevel)
        {
            if (_logText == null)
            {
                return;
            }

            var builder = new StringBuilder(_logText.text);
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append($"EXP +{experienceGained}");

            if (leveledUp)
            {
                builder.AppendLine();
                builder.Append($"Level Up! Lv.{newLevel}");
            }

            _logText.text = builder.ToString();
        }

        private void ShowResult(BattleOutcome outcome)
        {
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(true);
            }

            if (_resultText != null)
            {
                _resultText.text = outcome == BattleOutcome.PlayerVictory ? "Victory!" : "Defeat...";
            }
        }
    }
}
