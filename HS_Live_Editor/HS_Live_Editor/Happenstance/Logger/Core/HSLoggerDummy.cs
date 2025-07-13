using Happenstance.SE.Logger.Interfaces;

namespace Happenstance.SE.Logger.Core
{
    public class HSLoggerDummy : IHSLogger
    {
        public bool DebugMode { get; set; } = false;

        public void Debug(string message) { }
        public void Info(string message) { }
        public void Warning(string message) { }
        public void Error(string message) { }
        public void AddComponent(ILogComponent component) { }
        public void RemoveComponent(ILogComponent component) { }
        public void ClearLogFile(string fileName) { }
    }
}