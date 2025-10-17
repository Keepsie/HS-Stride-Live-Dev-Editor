// HS Stride Engine Core (c) 2025 Happenstance Games LLC - MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Engine;
using Stride.UI;

namespace Happenstance.SE.Core
{
    /// <summary>
    /// Entity extension methods providing Unity-style entity finding and component access
    /// </summary>
    public static class HSEntity
    {
        /// <summary>
        /// Find child by name in immediate children
        /// </summary>
        public static Entity FindChildByName_HS(this Entity parent, string childName)
        {
            if (parent == null || string.IsNullOrEmpty(childName))
                return null;

            return parent.GetChildren().FirstOrDefault(child => child.Name == childName);
        }

        /// <summary>
        /// Find child by name anywhere in hierarchy (deep search)
        /// </summary>
        public static Entity FindChildByNameRecursive_HS(this Entity parent, string childName)
        {
            if (parent == null || string.IsNullOrEmpty(childName))
                return null;

            foreach (var child in parent.GetChildren())
            {
                if (child.Name == childName)
                    return child;

                var foundInChild = FindChildByNameRecursive_HS(child, childName);
                if (foundInChild != null)
                    return foundInChild;
            }

            return null;
        }

        /// <summary>
        /// Get component from child by name
        /// </summary>
        public static T GetComponentFromChild_HS<T>(this Entity parent, string childName) where T : EntityComponent
        {
            var child = parent.FindChildByName_HS(childName);
            return child?.Get<T>();
        }

        /// <summary>
        /// Get component from child by name (recursive)
        /// </summary>
        public static T GetComponentFromChildRecursive_HS<T>(this Entity parent, string childName) where T : EntityComponent
        {
            var child = parent.FindChildByNameRecursive_HS(childName);
            return child?.Get<T>();
        }
        
        /// <summary>
        /// Gets the first component that implements the specified interface or class from this entity
        /// </summary>
        public static T Get_HS<T>(this Entity entity) where T : class
        {
            if (entity == null) return null;
    
            // Check current entity's components for the interface
            return entity.Components.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the first component that implements the specified interface or class from this entity or its children (recursive)
        /// </summary>
        public static T GetInChildren_HS<T>(this Entity entity) where T : class
        {
            if (entity == null) return null;
    
            // Check current entity first
            var component = entity.Components.OfType<T>().FirstOrDefault();
            if (component != null) return component;
    
            // Search children recursively
            foreach (var child in entity.Transform.Children)
            {
                var childComponent = GetInChildren_HS<T>(child.Entity);
                if (childComponent != null) return childComponent;
            }
    
            return null;
        }
        

        // ================== UNITY-STYLE COMPONENT EXTENSIONS ==================

        /// <summary>
        /// Gets all components of specified type in immediate children
        /// Unity equivalent of GetComponentsInChildren() with includeInactive = false
        /// </summary>
        /// <param name="parent">Parent entity to search in</param>
        /// <param name="includeParent">Whether to include the parent entity in search</param>
        /// <returns>List of components found</returns>
        public static List<T> GetComponentsInChildren_HS<T>(this Entity parent, bool includeParent = true) where T : EntityComponent
        {
            var results = new List<T>();
            if (parent == null) return results;

            // Include parent if requested
            if (includeParent)
            {
                var parentComponent = parent.Get<T>();
                if (parentComponent != null)
                    results.Add(parentComponent);
            }

            // Search immediate children
            foreach (var child in parent.GetChildren())
            {
                var component = child.Get<T>();
                if (component != null)
                    results.Add(component);
            }

            return results;
        }

        /// <summary>
        /// Gets all components of specified type in all descendants (recursive)
        /// Unity equivalent of GetComponentsInChildren() with deep search
        /// </summary>
        /// <param name="parent">Parent entity to search in</param>
        /// <param name="includeParent">Whether to include the parent entity in search</param>
        /// <returns>List of components found</returns>
        public static List<T> GetComponentsInAllDescendants_HS<T>(this Entity parent, bool includeParent = true) where T : EntityComponent
        {
            var results = new List<T>();
            if (parent == null) return results;

            GetComponentsInAllDescendantsRecursive_HS<T>(parent, results, includeParent);
            return results;
        }

        /// <summary>
        /// Gets component of specified type in parent entities (walking up the hierarchy)
        /// Unity equivalent of GetComponentInParent()
        /// </summary>
        /// <param name="entity">Entity to start search from</param>
        /// <param name="includeself">Whether to include the starting entity in search</param>
        /// <returns>Component if found, null otherwise</returns>
        public static T GetComponentInParent_HS<T>(this Entity entity, bool includeself = true) where T : EntityComponent
        {
            if (entity == null) return null;

            // Check self first if requested
            if (includeself)
            {
                var component = entity.Get<T>();
                if (component != null) return component;
            }

            // Walk up the parent hierarchy
            var currentTransform = entity.Transform.Parent;
            while (currentTransform != null)
            {
                var component = currentTransform.Entity.Get<T>();
                if (component != null) return component;
                
                currentTransform = currentTransform.Parent;
            }

            return null;
        }

        /// <summary>
        /// Gets all components of specified type in parent entities (walking up the hierarchy)
        /// </summary>
        /// <param name="entity">Entity to start search from</param>
        /// <param name="includeself">Whether to include the starting entity in search</param>
        /// <returns>List of components found</returns>
        public static List<T> GetComponentsInParents_HS<T>(this Entity entity, bool includeself = true) where T : EntityComponent
        {
            var results = new List<T>();
            if (entity == null) return results;

            // Check self first if requested
            if (includeself)
            {
                var component = entity.Get<T>();
                if (component != null) results.Add(component);
            }

            // Walk up the parent hierarchy
            var currentTransform = entity.Transform.Parent;
            while (currentTransform != null)
            {
                var component = currentTransform.Entity.Get<T>();
                if (component != null) results.Add(component);
                
                currentTransform = currentTransform.Parent;
            }

            return results;
        }
        
        /// <summary>
        /// Recursive helper for GetComponentsInAllDescendants
        /// </summary>
        private static void GetComponentsInAllDescendantsRecursive_HS<T>(Entity entity, List<T> results, bool includeEntity) where T : EntityComponent
        {
            // Check current entity
            if (includeEntity)
            {
                var component = entity.Get<T>();
                if (component != null)
                    results.Add(component);
            }

            // Search children
            foreach (var child in entity.Transform.Children)
            {
                GetComponentsInAllDescendantsRecursive_HS<T>(child.Entity, results, true);
            }
        }
        
        /// <summary>
        /// Makes target Entity a parent of this entity
        /// </summary>
        /// <param name="child"></param>
        public static void SetParent_HS(this Entity entity, Entity parent)
        {
            entity.Transform.Parent = parent?.Transform;
        }

        /// <summary>
        /// Makes target Entity a child of this entity
        /// </summary>
        /// <param name="child"></param>
        public static void SetChild_HS(this Entity entity,Entity child)
        {
            if (child != null)
            {
                child.Transform.Parent = entity.Transform;
            }
        }

        /// <summary>
        /// Removes this entity's parent (makes it a root entity)
        /// </summary>
        public static void ClearParent_HS(this Entity entity)
        {
            if (entity?.Transform.Parent == null) return;

            var scene = entity.Scene;
            entity.Transform.Parent = null;

            // Add back to scene root entities if not already there
            if (scene != null && !scene.Entities.Contains(entity))
            {
                scene.Entities.Add(entity);
            }
        }

        /// <summary>
        /// Removes a specific child from this entity (makes it a root entity)
        /// </summary>
        /// <param name="child">The child entity to remove</param>
        public static void ClearChild_HS(this Entity entity, Entity child)
        {
            if (child != null && child.Transform.Parent == entity.Transform)
            {
                var scene = child.Scene;
                child.Transform.Parent = null;

                // Add back to scene root entities if not already there
                if (scene != null && !scene.Entities.Contains(child))
                {
                    scene.Entities.Add(child);
                }
            }
        }

        /// <summary>
        /// Removes all children from this entity (makes them root entities)
        /// Note: This clears ALL children - no selective removal
        /// </summary>
        public static void ClearChildren_HS(this Entity entity)
        {
            // Need to iterate through a copy since we're modifying the collection
            var children = entity.Transform.Children.ToList();

            foreach (var child in children)
            {
                child.Parent = null;
            }
        }

        /// <summary>
        /// Destroys this entity by removing it from the scene
        /// </summary>
        public static void Destroy_HS(this Entity entity)
        {
            if (entity?.Scene != null)
            {
                entity.Scene.Entities.Remove(entity);
            }
        }

        public static void SetActive_HS(this Entity entity,bool active)
        {
            entity.EnableAll(active, true);
        }

        // ================== UI EXTENSIONS ==================

        /// <summary>
        /// Finds first UI element of specified type in the entity's UI hierarchy
        /// </summary>
        public static T GetUIElement_HS<T>(this Entity entity) where T : UIElement
        {
            var uiPage = entity.Get<UIComponent>()?.Page;
            return uiPage?.RootElement.FindVisualChildOfType<T>();
        }

        /// <summary>
        /// Finds all UI elements of specified type in the entity's UI hierarchy
        /// </summary>
        public static List<T> GetUIElements_HS<T>(this Entity entity) where T : UIElement
        {
            var uiPage = entity.Get<UIComponent>()?.Page;
            if (uiPage?.RootElement != null)
            {
                return uiPage.RootElement.FindVisualChildrenOfType<T>().ToList();
            }
            return new List<T>();
        }

        /// <summary>
        /// Finds UI element of specified type and name in the entity's UI hierarchy
        /// </summary>
        public static T GetUIElement_HS<T>(this Entity entity, string name) where T : UIElement
        {
            var uiPage = entity.Get<UIComponent>()?.Page;
            return uiPage?.RootElement.FindVisualChildOfType<T>(name);
        }
        
        // UIElement Extensions (work on Canvas, StackPanel, etc.)
        public static T GetUIElement_HS<T>(this UIElement parentElement, string name) where T : UIElement
        {
            return parentElement?.FindVisualChildOfType<T>(name);
        }

        public static T GetUIElement_HS<T>(this UIElement parentElement) where T : UIElement
        {
            return parentElement?.FindVisualChildOfType<T>();
        }

        public static List<T> GetUIElements_HS<T>(this UIElement parentElement) where T : UIElement
        {
            if (parentElement != null)
            {
                return parentElement.FindVisualChildrenOfType<T>().ToList();
            }
            return new List<T>();
        }
        
    }
}