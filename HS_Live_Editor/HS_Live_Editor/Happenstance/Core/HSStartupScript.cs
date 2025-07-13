// HS Stride Engine Core (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Logger.Core;
using Happenstance.SE.Logger.Interfaces;
using Stride.Engine;
using System.Linq;
using System.Runtime.Serialization;

namespace Happenstance.SE.Core
{
    public abstract class HSStartupScript : StartupScript
    {
        //
        [DataMember]
        public bool StartDisabled { get; set; }
        
        [DataMember]
        public bool CollisionDetection { get; set; } = false;
        
        
        //
        private bool _hasInitialized = false;

        //
        protected IHSLogger Logger { get; private set; }
        private HSEnableComponent _enableComponent;
        private HSOnTriggerComponent _triggerComponentHandler;
        private bool _isDestroyed = false;
        public bool IsEnabled { get; private set; }

        public sealed override void Start()
        {
            Logger = Entity.Scene.FindAllComponents_HS<HSLogger>().FirstOrDefault();
            if (Logger == null)
            {
                Log.Warning($"[{GetType().Name}] Could not find HSLogger");
                Logger = new HSLoggerDummy();
            }

            //If starting diabled call before connecting the on disable or enable and awake
            if (StartDisabled)
            {
                OnStartDisabled();
            }

            //Connect OnEnable and OnDisable
            _enableComponent = Entity.GetOrCreate<HSEnableComponent>();
            _enableComponent.RegisterCallbacks(
                () => HandleEnable(),
                () => OnDisable(),
                EnableWatcher
            );

            IsEnabled = _enableComponent.Enabled;
            
            //
            _triggerComponentHandler = Entity.GetOrCreate<HSOnTriggerComponent>();
            _triggerComponentHandler.EnableTrigger = CollisionDetection;
            _triggerComponentHandler.OnTriggerEnter += OnTriggerEnter;
            _triggerComponentHandler.OnTriggerExit += OnTriggerExit;

            
            if (!StartDisabled)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (!_hasInitialized)
            {
                OnAwake();
                OnStart();
                _hasInitialized = true;
            }
        }


        public void Destroy()
        {
            Entity.Scene.Entities.Remove(Entity);
        }

        private void OnStartDisabled() => SetActive(false);

        public void SetActive(bool active)
        {
            Entity.EnableAll(active, true);
        }

        public sealed override void Cancel()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            OnDestroy();
            base.Cancel();
        }

        private void HandleEnable()
        {
            // Run initialization if this is first enable
            if (!_hasInitialized)
            {
                Initialize();
            }

            // Always run normal enable logic
            OnEnable();
        }

        private void EnableWatcher(bool enable)
        {
            IsEnabled = enable;
        }


        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnTriggerEnter(Entity other){}
        protected virtual void OnTriggerExit(Entity other) { }
    }
}
