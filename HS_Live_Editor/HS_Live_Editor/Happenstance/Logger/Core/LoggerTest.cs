using Happenstance.SE.Core;
using Stride.Core.Diagnostics;
using Stride.Engine;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Happenstance.SE.Logger.Core
{
    public class LoggerTest : HSSyncScript
    {
        protected override void OnStart()
        {
            Logger.Info("Logger test component initialized");
        }

        protected override void OnUpdate()
        {
            var input = Input;

            // Press Q to test debug logging
            if (input.IsKeyPressed(Keys.Q))
            {
                Logger.Debug("Testing debug log from LoggerTest");
                Logger.Info("Just some info logging");
            }

            // Press W to test error logging
            if (input.IsKeyPressed(Keys.W))
            {
                Logger.Error("Test error message!");
                Logger.Warning("Also testing warning message");
            }

            // Press E to test a mix of logs (simulating game events)
            if (input.IsKeyPressed(Keys.E))
            {
                Logger.Debug("Player position updated: x=100, y=50");
                Logger.Info("Scene transition started: MainMenu -> GameLevel");
                Logger.Warning("Performance drop detected: FPS=45");
            }
        }
    }
}
