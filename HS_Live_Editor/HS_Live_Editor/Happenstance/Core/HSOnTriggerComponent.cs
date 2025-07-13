using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Engine;

namespace Happenstance.SE.Core
{
    /// <summary>
    /// Unity-style trigger events for current Stride Bullet physics
    /// Will be updated when Bepu becomes stable
    /// </summary>
    public class HSOnTriggerComponent : AsyncScript
    {
        [DataMember]
        public bool EnableTrigger { get; set; } = false;
        
        private HashSet<Entity> _entitiesInside = new HashSet<Entity>();
        
        
        public Action<Entity> OnTriggerEnter;
        public Action<Entity> OnTriggerExit;

        public override async Task Execute()
        {
            if (!EnableTrigger) return;

            var trigger = Entity.Get<PhysicsComponent>();
            if (trigger == null)
            {
                Log.Error("HSOnTrigger requires PhysicsComponent");
                return;
            }

            trigger.ProcessCollisions = true;

            // Handle new collisions and periodic cleanup
            var collisionTask = HandleCollisions(trigger);
            var cleanupTask = PeriodicCleanup();
            
            await Task.WhenAll(collisionTask, cleanupTask);
        }
        
        private async Task HandleCollisions(PhysicsComponent trigger)
        {
            while (Game.IsRunning)
            {
                // Wait for collision
                var collision = await trigger.NewCollision();
                var otherCollider = trigger == collision.ColliderA ? collision.ColliderB : collision.ColliderA;
                var otherEntity = otherCollider.Entity;
                
                OnTriggerEnter?.Invoke(otherEntity);
                _entitiesInside.Add(otherEntity);
                
                // Wait for exit
                await collision.Ended();
                
                OnTriggerExit?.Invoke(otherEntity);
                _entitiesInside.Remove(otherEntity);
            }
        }
        
        private async Task PeriodicCleanup()
        {
            while (Game.IsRunning)
            {
                await Task.Delay(100); // Check every 100ms
                
                if (_entitiesInside.Count == 0)
                    continue;
                
                // Find dead entities that never properly exited
                var deadEntities = _entitiesInside
                    .Where(entity => entity.Scene == null) // Dead/destroyed entities
                    .ToList();
                
                foreach (var deadEntity in deadEntities)
                {
                    _entitiesInside.Remove(deadEntity);
                    OnTriggerExit?.Invoke(deadEntity); // Manual exit event for dead entities
                }
            }
        }
    }
}