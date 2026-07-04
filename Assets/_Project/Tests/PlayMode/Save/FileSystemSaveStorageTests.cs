using System;
using System.IO;
using FloatingIslandsRpg.Infrastructure.Save;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.PlayMode.Save
{
    public class FileSystemSaveStorageTests
    {
        private string _tempDirectory;

        [SetUp]
        public void SetUp()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "FloatingIslandsRpgTests_" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_NullOrWhitespaceDirectory_ThrowsArgumentException(string invalidDirectory)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new FileSystemSaveStorage(invalidDirectory));
        }

        [Test]
        public void Write_ThenTryReadPrimary_ReturnsWrittenContent()
        {
            // Arrange
            var storage = new FileSystemSaveStorage(_tempDirectory);

            // Act
            storage.Write("{\"value\":1}");
            var found = storage.TryReadPrimary(out var content);

            // Assert
            Assert.IsTrue(found);
            Assert.AreEqual("{\"value\":1}", content);
        }

        [Test]
        public void TryReadPrimary_NoFileExists_ReturnsFalse()
        {
            // Arrange
            var storage = new FileSystemSaveStorage(_tempDirectory);

            // Act
            var found = storage.TryReadPrimary(out var content);

            // Assert
            Assert.IsFalse(found);
            Assert.IsNull(content);
        }

        [Test]
        public void TryReadBackup_NoBackupExists_ReturnsFalse()
        {
            // Arrange
            var storage = new FileSystemSaveStorage(_tempDirectory);
            storage.Write("{\"value\":1}");

            // Act
            var found = storage.TryReadBackup(out var content);

            // Assert
            Assert.IsFalse(found);
        }

        [Test]
        public void Write_Twice_CreatesBackupOfPreviousContent()
        {
            // Arrange
            var storage = new FileSystemSaveStorage(_tempDirectory);
            storage.Write("{\"value\":1}");

            // Act
            storage.Write("{\"value\":2}");

            // Assert
            Assert.IsTrue(storage.TryReadPrimary(out var primary));
            Assert.AreEqual("{\"value\":2}", primary);
            Assert.IsTrue(storage.TryReadBackup(out var backup));
            Assert.AreEqual("{\"value\":1}", backup);
        }

        [Test]
        public void Write_NullContent_ThrowsArgumentNullException()
        {
            // Arrange
            var storage = new FileSystemSaveStorage(_tempDirectory);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => storage.Write(null));
        }

        [Test]
        public void Write_DirectoryDoesNotExist_CreatesDirectoryAndWrites()
        {
            // Arrange
            Assert.IsFalse(Directory.Exists(_tempDirectory));
            var storage = new FileSystemSaveStorage(_tempDirectory);

            // Act
            storage.Write("{\"value\":1}");

            // Assert
            Assert.IsTrue(Directory.Exists(_tempDirectory));
        }
    }
}
