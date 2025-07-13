using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Happenstance.SE.Logger.Core;

namespace Happenstance.SE.Logger.Interfaces
{
    public interface ILogComponent
    {
        void HSLog(HSLogLevel level, string message);
    }
}
