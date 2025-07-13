namespace Happenstance.SE.Logger.Interfaces
{
    public interface IHSLogger
    {
        bool DebugMode { get; set; }
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void AddComponent(ILogComponent component);
        void RemoveComponent(ILogComponent component);
        void ClearLogFile(string fileName);
    }
}