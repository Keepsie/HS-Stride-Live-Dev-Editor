// HS Stride Engine Core (c) 2025 Happenstance Games LLC - MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using BulletSharp;
using Stride.Engine;
using Stride.UI;

namespace Happenstance.SE.Core
{
    public class HSEntityFinder
    {
        private readonly SceneSystem _sceneSystem;

        public HSEntityFinder(SceneSystem sceneSystem)
        {
            _sceneSystem = sceneSystem ?? throw new ArgumentNullException(nameof(sceneSystem));
        }

        /// <summary>
        /// Finds an entity by name in the current scene (recursive through all children)
        /// Unity equivalent of GameObject.Find() - slow but thorough
        /// </summary>
        public Entity FindEntityByName(string name, Scene scene = null)
        {
            scene ??= GetCurrentScene();

            // Search through all top-level entities recursively
            foreach (var entity in scene.Entities)
            {
                var found = FindEntityByNameRecursive(entity, name);
                if (found != null) return found;
            }

            return null;
        }

        // <summary>
        /// Finds all entities with a specific component type (recursive through all children)
        /// Unity equivalent of FindObjectsOfType() - slow but thorough
        /// </summary>
        public List<Entity> FindEntitiesWithComponent<T>(Scene scene = null) where T : EntityComponent
        {
            scene ??= GetCurrentScene();
            var results = new List<Entity>();

            // Search through all top-level entities recursively
            foreach (var entity in scene.Entities)
            {
                FindEntitiesWithComponentRecursive<T>(entity, results);
            }

            return results;
        }

        /// <summary>
        /// Gets all components of a specific type in the scene (recursive)
        /// Unity equivalent of FindObjectsOfType() but returns components
        /// </summary>
        public List<T> FindAllComponents<T>(Scene scene = null) where T : EntityComponent
        {
            scene ??= GetCurrentScene();
            var results = new List<T>();

            foreach (var entity in scene.Entities)
            {
                FindAllComponentsRecursive<T>(entity, results);
            }

            return results;
        }

        /// <summary>
        /// Finds all components that implement a specific interface (recursive)
        /// </summary>
        public List<T> FindAllComponentsWithInterface<T>(Scene scene = null) where T : class
        {
            scene ??= GetCurrentScene();
            var results = new List<T>();

            foreach (var entity in scene.Entities)
            {
                FindAllComponentsWithInterfaceRecursive<T>(entity, results);
            }

            return results;
        }

        /// <summary>
        /// Recursive helper for finding entity by name
        /// </summary>
        private Entity FindEntityByNameRecursive(Entity entity, string name)
        {
            // Check current entity
            if (string.Equals(entity.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return entity;
            }

            // Search children
            foreach (var child in entity.Transform.Children)
            {
                var found = FindEntityByNameRecursive(child.Entity, name);
                if (found != null) return found;
            }

            return null;
        }

        /// <summary>
        /// Recursive helper for finding entities with component
        /// </summary>
        private void FindEntitiesWithComponentRecursive<T>(Entity entity, List<Entity> results) where T : EntityComponent
        {
            // Check current entity
            if (entity.Get<T>() != null)
            {
                results.Add(entity);
            }

            // Search children
            foreach (var child in entity.Transform.Children)
            {
                FindEntitiesWithComponentRecursive<T>(child.Entity, results);
            }
        }

        /// <summary>
        /// Recursive helper for finding all components
        /// </summary>
        private void FindAllComponentsRecursive<T>(Entity entity, List<T> results) where T : EntityComponent
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
                FindAllComponentsRecursive<T>(child.Entity, results);
            }
        }

        /// <summary>
        /// Recursive helper for finding components with interface
        /// </summary>
        private void FindAllComponentsWithInterfaceRecursive<T>(Entity entity, List<T> results) where T : class
        {
            // Check current entity
            var components = entity.Components.OfType<T>();
            results.AddRange(components);

            // Search children
            foreach (var child in entity.Transform.Children)
            {
                FindAllComponentsWithInterfaceRecursive<T>(child.Entity, results);
            }
        }

        /// <summary>
        /// Gets the current root scene
        /// </summary>
        private Scene GetCurrentScene()
        {
            return _sceneSystem.SceneInstance.RootScene;
        }

        // Find child by name in immediate children
        public Entity FindChildByName(Entity parent, string childName)
        {
            if (parent == null || string.IsNullOrEmpty(childName))
                return null;

            return parent.GetChildren().FirstOrDefault(child => child.Name == childName);
        }

        // Find child by name anywhere in hierarchy (deep search)
        public Entity FindChildByNameRecursive(Entity parent, string childName)
        {
            if (parent == null || string.IsNullOrEmpty(childName))
                return null;

            foreach (var child in parent.GetChildren())
            {
                if (child.Name == childName)
                    return child;

                var foundInChild = FindChildByNameRecursive(child, childName);
                if (foundInChild != null)
                    return foundInChild;
            }

            return null;
        }

        // Get component from child by name
        public T GetComponentFromChild<T>(Entity parent, string childName) where T : EntityComponent
        {
            var child = FindChildByName(parent, childName);
            return child?.Get<T>();
        }

        // Get component from child by name (recursive)
        public T GetComponentFromChildRecursive<T>(Entity parent, string childName) where T : EntityComponent
        {
            var child = FindChildByNameRecursive(parent, childName);
            return child?.Get<T>();
        }

        //=========================== UI CRAP =========================================


        /// <summary>
        /// Finds first UI element of specified type in the entity's UI hierarchy
        /// </summary>
        public T GetUIElement<T>(UIPage uiPage) where T : UIElement
        {
            return uiPage?.RootElement.FindVisualChildOfType<T>();
        }

        /// <summary>
        /// Finds all UI elements of specified type in the entity's UI hierarchy
        /// </summary>
        public List<T> GetUIElements<T>(UIPage uiPage) where T : UIElement
        {
            if (uiPage.RootElement != null)
            {
                return uiPage.RootElement.FindVisualChildrenOfType<T>().ToList();
            }
            return new List<T>();
        }

        /// <summary>
        /// Finds UI element of specified type and name in the entity's UI hierarchy
        /// </summary>
        public T GetUIElement<T>(UIPage uiPage, string name) where T : UIElement
        {
            return uiPage?.RootElement.FindVisualChildOfType<T>(name);
        }

        /// <summary>
        /// Finds UI element of specified type and name within a parent UI element
        /// </summary>
        public T GetUIElement<T>(UIElement parentElement, string name) where T : UIElement
        {
            return parentElement?.FindVisualChildOfType<T>(name);
        }

        /// <summary>
        /// Finds first UI element of specified type within a parent UI element
        /// </summary>
        public T GetUIElement<T>(UIElement parentElement) where T : UIElement
        {
            return parentElement?.FindVisualChildOfType<T>();
        }

        /// <summary>
        /// Finds all UI elements of specified type within a parent UI element
        /// </summary>
        public List<T> GetUIElements<T>(UIElement parentElement) where T : UIElement
        {
            if (parentElement != null)
            {
                return parentElement.FindVisualChildrenOfType<T>().ToList();
            }
            return new List<T>();
        }


    }


}