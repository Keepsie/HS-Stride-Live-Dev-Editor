using Stride.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Happenstance.SE.Logger.Interfaces;

namespace Happenstance.SE.Logger.Core
{
    public class FileLogComponent : ILogComponent
    {
        private readonly string _filePath;
        private readonly string _fileName;
        private readonly HSLogLevel _logLevel;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);


        public FileLogComponent(string fileName, HSLogLevel logLevel = HSLogLevel.All)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new System.ArgumentException("Invalid file name", nameof(fileName));

            _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Happenstance", "DesertStrides", "Logs", fileName);

            _fileName = fileName;
            _logLevel = logLevel;

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
        }

        public void HSLog(HSLogLevel level, string message)
        {
            if (_logLevel != HSLogLevel.All && level != _logLevel) return;
            string logEntry = $"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {HSLogger.RemoveTags(message)}";
            WriteLogSync(logEntry);
        }

        private void WriteLogSync(string logEntry)
        {
            try
            {
                _semaphore.Wait(); // Synchronous wait instead of async
                try
                {
                    using (StreamWriter writer = new StreamWriter(_filePath, true))
                    {
                        writer.WriteLine(logEntry);
                        writer.Flush();
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (IOException ex)
            {
                // Don't throw, just silently fail to avoid breaking the game
                // Could log to console here if needed
            }
        }
    }
}
