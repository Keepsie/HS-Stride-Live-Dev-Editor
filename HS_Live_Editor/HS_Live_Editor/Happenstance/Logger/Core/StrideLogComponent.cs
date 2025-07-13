using Stride.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Happenstance.SE.Logger.Interfaces;

namespace Happenstance.SE.Logger.Core
{
    public class StrideLogComponent : ILogComponent
    {
        private readonly HSLogger _logger;
        private static readonly ILogger Log = GlobalLogger.GetLogger("HSLogger");

        public StrideLogComponent(HSLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void HSLog(HSLogLevel level, string message)
        {
            // Don't log debug messages if debug mode is off
            if (level == HSLogLevel.Debug && !_logger.DebugMode) return;

            switch (level)
            {
                case HSLogLevel.Debug:
                    Log.Debug(message);
                    break;
                case HSLogLevel.Info:
                    Log.Info(message);
                    break;
                case HSLogLevel.Warning:
                    Log.Warning(message);
                    break;
                case HSLogLevel.Error:
                    Log.Error(message);
                    break;
                default:
                    Log.Info(message);
                    break;
            }
        }
    }
}
