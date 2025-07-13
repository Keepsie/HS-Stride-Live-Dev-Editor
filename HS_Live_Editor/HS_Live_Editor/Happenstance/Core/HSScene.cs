using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Engine;

namespace Happenstance.SE.Core
{
    /// <summary>
    /// Entity extension methods providing Unity-style Scene finding and component access
    /// </summary>
    public static class HSScene
    {
        /// <summary>
        /// Finds an entity by name in the current scene (recursive through all children)
        /// Converted from HSEntityFinder.FindEntityByName(string name, Scene scene = null)
        /// </summary>
        public static Entity FindEntityByName_HS(this Scene scene, string name)
        {
            if (scene == null || string.IsNullOrEmpty(name)) return null;

            // Search through all top-level entities recursively
            foreach (var entity in scene.Entities)
            {
                var found = FindEntityByNameRecursive_HS(entity, name);
                if (found != null) return found;
            }

            return null;
        }

        /// <summary>
        /// Finds all entities with a specific component type (recursive through all children)
        /// Converted from HSEntityFinder.FindEntitiesWithComponent<T>(Scene scene = null)
        /// </summary>
        public static List<Entity> FindEntitiesWithComponent_HS<T>(this Scene scene) where T : EntityComponent
        {
            if (scene == null) return new List<Entity>();
            
            var results = new List<Entity>();

            // Search through all top-level entities recursively
            foreach (var entity in scene.Entities)
            {
                FindEntitiesWithComponentRecursive_HS<T>(entity, results);
            }

            return results;
        }

        /// <summary>
        /// Gets all components of a specific type in the scene (recursive)
        /// Converted from HSEntityFinder.FindAllComponents<T>(Scene scene = null)
        /// </summary>
        public static List<T> FindAllComponents_HS<T>(this Scene scene) where T : EntityComponent
        {
            if (scene == null) return new List<T>();
            
            var results = new List<T>();

            foreach (var entity in scene.Entities)
            {
                FindAllComponentsRecursive_HS<T>(entity, results);
            }

            return results;
        }
        
        
        /// <summary>
        /// Recursive helper for finding entity by name
        /// </summary>
        private static Entity FindEntityByNameRecursive_HS(Entity entity, string name)
        {
            // Check current entity
            if (string.Equals(entity.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return entity;
            }

            // Search children
            foreach (var child in entity.Transform.Children)
            {
                var found = FindEntityByNameRecursive_HS(child.Entity, name);
                if (found != null) return found;
            }

            return null;
        }

        /// <summary>
        /// Finds all components that implement a specific interface (recursive)
        /// Converted from HSEntityFinder.FindAllComponentsWithInterface<T>(Scene scene = null)
        /// </summary>
        public static List<T> FindAllComponentsWithInterface_HS<T>(this Scene scene) where T : class
        {
            if (scene == null) return new List<T>();
            
            var results = new List<T>();

            foreach (var entity in scene.Entities)
            {
                FindAllComponentsWithRecursive_HS<T>(entity, results);
            }

            return results;
        }
        
        
        /// <summary>
        /// Recursive helper for finding entities with component
        /// </summary>
        private static void FindEntitiesWithComponentRecursive_HS<T>(Entity entity, List<Entity> results) where T : EntityComponent
        {
            // Check current entity
            if (entity.Get<T>() != null)
            {
                results.Add(entity);
            }

            // Search children
            foreach (var child in entity.Transform.Children)
            {
                FindEntitiesWithComponentRecursive_HS<T>(child.Entity, results);
            }
        }

        /// <summary>
        /// Recursive helper for finding all components
        /// </summary>
        private static void FindAllComponentsRecursive_HS<T>(Entity entity, List<T> results) where T : EntityComponent
        {
            // Check current entity
            var component = entity.Get<T>();
            if (component != null)
            {
                results.Add(component);
            }

            // Search children
            foreach (var child in entity.Transform.Children)
            {
                FindAllComponentsRecursive_HS<T>(child.Entity, results);
            }
        }

        /// <summary>
        /// Recursive helper for finding components with a specific interface or class
        /// </summary>
        private static void FindAllComponentsWithRecursive_HS<T>(Entity entity, List<T> results) where T : class
        {
            // Check current entity
            var components = entity.Components.OfType<T>();
            results.AddRange(components);

            // Search children
            foreach (var child in entity.Transform.Children)
            {
                FindAllComponentsWithRecursive_HS<T>(child.Entity, results);
            }
        }
    }

}