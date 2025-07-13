// HS Stride Engine Core (c) 2025 Happenstance Games LLC - MIT License

using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Happenstance.SE.Core
{
    public class HSEnableComponent : ActivableEntityComponent
    {
        private bool _enabled = true;
        private Action _onEnable;
        private Action _onDisable;
        private Action<bool> _enableWatcher;

        public override bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;

                    //Watcher for IsEnabled
                    _enableWatcher(_enabled);

                    //Watchers for OnEnable/Disable
                    if (_enabled)
                        _onEnable?.Invoke();
                    else
                        _onDisable?.Invoke();
                }
            }
        }

        public void RegisterCallbacks(Action onEnable, Action onDisable, Action<bool> enableWatcher)
        {
            _onEnable = onEnable;
            _onDisable = onDisable;
            _enableWatcher = enableWatcher;
        }
    }
}
