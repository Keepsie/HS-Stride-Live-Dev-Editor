// HS Stride Engine Core (c) 2025 Happenstance Games LLC - MIT License

using Stride.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using Happenstance.SE.Logger.Interfaces;

namespace Happenstance.SE.Logger.Core
{
    public class HSLogger : StartupScript, IHSLogger
    {
        public bool DebugMode { get; set; } = false;

        private readonly List<ILogComponent> _logComponents = new List<ILogComponent>();

        public override void Start()
        {
            RegisterDefaultComponents();
        }

        private void RegisterDefaultComponents()
        {
            if (DebugMode)
            {
                AddComponent(new StrideLogComponent(this));
                ClearLogFile("game_log.txt");
                EnableFileLogging();
            }

            // Always log errors
            EnableFileLogging("error_log.txt", HSLogLevel.Error);
        }

        public void Debug(string message)
        {
            if (DebugMode) HSLog(HSLogLevel.Debug, message);
        }

        public void Info(string message) => HSLog(HSLogLevel.Info, message);
        public void Warning(string message) => HSLog(HSLogLevel.Warning, message);
        public void Error(string message) => HSLog(HSLogLevel.Error, message);

        private void HSLog(HSLogLevel level, string message)
        {
            // Only log to components if appropriate
            if (!DebugMode && level == HSLogLevel.Debug) return;

            foreach (var component in _logComponents)
            {
                component.HSLog(level, message);
            }
        }

        private void EnableFileLogging(string fileName = "game_log.txt", HSLogLevel logLevel = HSLogLevel.All)
        {
            AddComponent(new FileLogComponent(fileName, logLevel));
        }

        public void AddComponent(ILogComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            _logComponents.Add(component);
        }

        public void RemoveComponent(ILogComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            _logComponents.Remove(component);
        }

        public static string RemoveTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            int indexStart;
            while ((indexStart = input.IndexOf('<')) != -1)
            {
                int indexEnd = input.IndexOf('>', indexStart);
                if (indexEnd == -1) break;
                input = input.Remove(indexStart, indexEnd - indexStart + 1);
            }

            return input;
        }

        public void ClearLogFile(string fileName)
        {
            try
            {
                // Use same path logic as FileLogComponent
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Happenstance", "Desert_Strides", "Logs", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug($"Cleared log file: {fileName}");
                }
            }
            catch (Exception e)
            {
                Error($"Failed to clear log file {fileName}: {e.Message}");
            }
        }

    }
}
