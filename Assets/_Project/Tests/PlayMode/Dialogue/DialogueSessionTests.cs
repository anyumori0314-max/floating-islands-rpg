using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Presentation.Dialogue;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.PlayMode.Dialogue
{
    public sealed class DialogueSessionTests
    {
        [Test]
        public void Constructor_NullLines_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DialogueSession(null));
        }

        [Test]
        public void Constructor_EmptyLines_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new DialogueSession(new List<string>()));
        }

        [Test]
        public void Start_SetsIsActiveAndFirstLine()
        {
            var session = new DialogueSession(new[] { "Line1", "Line2" });

            session.Start();

            Assert.IsTrue(session.IsActive);
            Assert.AreEqual("Line1", session.CurrentLine);
        }

        [Test]
        public void Start_WhenAlreadyActive_ThrowsInvalidOperationException()
        {
            var session = new DialogueSession(new[] { "Line1" });
            session.Start();

            Assert.Throws<InvalidOperationException>(() => session.Start());
        }

        [Test]
        public void Advance_NotOnLastPage_MovesToNextLineAndReturnsTrue()
        {
            var session = new DialogueSession(new[] { "Line1", "Line2" });
            session.Start();

            var stillActive = session.Advance();

            Assert.IsTrue(stillActive);
            Assert.IsTrue(session.IsActive);
            Assert.AreEqual("Line2", session.CurrentLine);
        }

        [Test]
        public void Advance_OnLastPage_EndsSessionAndReturnsFalse()
        {
            var session = new DialogueSession(new[] { "Line1" });
            session.Start();

            var stillActive = session.Advance();

            Assert.IsFalse(stillActive);
            Assert.IsFalse(session.IsActive);
            Assert.IsNull(session.CurrentLine);
        }

        [Test]
        public void Advance_WhenNotActive_ThrowsInvalidOperationException()
        {
            var session = new DialogueSession(new[] { "Line1" });

            Assert.Throws<InvalidOperationException>(() => session.Advance());
        }

        [Test]
        public void IsLastPage_ReflectsCurrentPage()
        {
            var session = new DialogueSession(new[] { "Line1", "Line2" });
            session.Start();

            Assert.IsFalse(session.IsLastPage);

            session.Advance();

            Assert.IsTrue(session.IsLastPage);
        }
    }
}
