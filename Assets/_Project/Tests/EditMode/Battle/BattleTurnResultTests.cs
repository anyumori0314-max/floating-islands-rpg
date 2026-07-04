using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Battle;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Battle
{
    public class BattleTurnResultTests
    {
        [Test]
        public void Constructor_NullActions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BattleTurnResult(1, null, BattleOutcome.InProgress));
        }

        [Test]
        public void Constructor_MutatingSourceListAfterConstruction_DoesNotAffectActions()
        {
            // Arrange
            var source = new List<BattleActionResult> { new BattleActionResult(true, true, 10, 20) };
            var result = new BattleTurnResult(1, source, BattleOutcome.InProgress);

            // Act
            source.Add(new BattleActionResult(false, false, 0, 20));
            source.Clear();

            // Assert
            Assert.AreEqual(1, result.Actions.Count);
        }

        [Test]
        public void Actions_CastToList_ThrowsInvalidCastException()
        {
            // Arrange
            var source = new List<BattleActionResult> { new BattleActionResult(true, true, 10, 20) };
            var result = new BattleTurnResult(1, source, BattleOutcome.InProgress);

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => _ = (List<BattleActionResult>)result.Actions);
        }

        [Test]
        public void Actions_CastToIListThenAdd_ThrowsNotSupportedException()
        {
            // Arrange
            var source = new List<BattleActionResult> { new BattleActionResult(true, true, 10, 20) };
            var result = new BattleTurnResult(1, source, BattleOutcome.InProgress);
            var asIList = (IList<BattleActionResult>)result.Actions;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => asIList.Add(new BattleActionResult(false, false, 0, 20)));
        }

        [Test]
        public void Actions_PreservesOrderAndContent()
        {
            // Arrange
            var first = new BattleActionResult(true, true, 10, 20);
            var second = new BattleActionResult(false, false, 0, 20);
            var source = new List<BattleActionResult> { first, second };

            // Act
            var result = new BattleTurnResult(2, source, BattleOutcome.PlayerVictory);

            // Assert
            Assert.AreEqual(2, result.Actions.Count);
            Assert.AreSame(first, result.Actions[0]);
            Assert.AreSame(second, result.Actions[1]);
        }
    }
}
