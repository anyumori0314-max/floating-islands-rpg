using System;
using System.IO;

namespace FloatingIslandsRpg.Infrastructure.Save
{
    public sealed class FileSystemSaveStorage
    {
        private const string SaveFileName = "save.json";
        private const string BackupFileName = "save.backup";
        private const string TempFileName = "save.tmp";

        private readonly string _directoryPath;

        public FileSystemSaveStorage(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("directoryPath must not be null, empty, or whitespace.", nameof(directoryPath));
            }

            _directoryPath = directoryPath;
        }

        private string SavePath => Path.Combine(_directoryPath, SaveFileName);
        private string BackupPath => Path.Combine(_directoryPath, BackupFileName);
        private string TempPath => Path.Combine(_directoryPath, TempFileName);

        public void Write(string content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Directory.CreateDirectory(_directoryPath);

            File.WriteAllText(TempPath, content);

            var verifyContent = File.ReadAllText(TempPath);
            if (verifyContent != content)
            {
                throw new IOException("Failed to verify temporary save file contents after writing.");
            }

            if (File.Exists(SavePath))
            {
                File.Replace(TempPath, SavePath, BackupPath);
            }
            else
            {
                File.Move(TempPath, SavePath);
            }
        }

        public bool TryReadPrimary(out string content)
        {
            return TryRead(SavePath, out content);
        }

        public bool TryReadBackup(out string content)
        {
            return TryRead(BackupPath, out content);
        }

        private static bool TryRead(string path, out string content)
        {
            if (!File.Exists(path))
            {
                content = null;
                return false;
            }

            content = File.ReadAllText(path);
            return true;
        }
    }
}
