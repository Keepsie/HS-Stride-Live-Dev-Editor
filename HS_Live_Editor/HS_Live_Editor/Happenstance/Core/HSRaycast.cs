using System.Collections.Generic;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Happenstance.SE.Core
{
    public static class HSRaycast
    {
        /// <summary>
        /// Performs a penetrating raycast with collision filtering and distance limits
        /// Uses RaycastPenetrating for robust hit detection and automatic distance sorting
        /// </summary>
        /// <param name="simulation">Physics simulation to raycast in</param>
        /// <param name="startPosition">World position to start the raycast from</param>
        /// <param name="direction">Normalized direction vector for the raycast</param>
        /// <param name="maxDistance">Maximum distance to raycast</param>
        /// <param name="minDistance">Minimum distance to accept hits (default: 0)</param>
        /// <param name="filterGroup">Collision filter group to use (default: DefaultFilter)</param>
        /// <param name="filterFlags">Collision filter flags to include/exclude (default: AllFilter)</param>
        /// <returns>HSRaycastResult containing hit information</returns>
        public static HSRaycastResult Cast(
            Simulation simulation,
            Vector3 startPosition,
            Vector3 direction,
            float maxDistance,
            float minDistance = 0f,
            CollisionFilterGroups filterGroup = CollisionFilterGroups.DefaultFilter,
            CollisionFilterGroupFlags filterFlags = CollisionFilterGroupFlags.AllFilter)
        {
            var raycastEnd = startPosition + direction * maxDistance;

            // Use RaycastPenetrating to get all hits
            var hitResults = new List<HitResult>();
            simulation.RaycastPenetrating(startPosition, raycastEnd, hitResults, filterGroup, filterFlags);

            var result = new HSRaycastResult();

            // Find the first valid hit within distance range
            foreach (var hit in hitResults.OrderBy(h => Vector3.Distance(startPosition, h.Point)))
            {
                float distance = Vector3.Distance(startPosition, hit.Point);
                if (distance < minDistance) continue;

                // Valid hit found
                result.Succeeded = true;
                result.HitResult = hit;
                result.Entity = hit.Collider.Entity;
                result.Point = hit.Point;
                result.Normal = hit.Normal;
                result.Distance = distance;
                break;
            }

            return result;
        }

        /// <summary>
        /// Convenience method to perform raycast from camera entity forward
        /// Automatically extracts position and direction from camera transform
        /// </summary>
        /// <param name="cameraEntity">Camera entity to raycast from</param>
        /// <param name="simulation">Physics simulation to raycast in</param>
        /// <param name="maxDistance">Maximum distance to raycast</param>
        /// <param name="minDistance">Minimum distance to accept hits (default: 0)</param>
        /// <param name="filterGroup">Collision filter group to use (default: DefaultFilter)</param>
        /// <param name="filterFlags">Collision filter flags to include/exclude (default: AllFilter)</param>
        /// <returns>HSRaycastResult containing hit information</returns>
        public static HSRaycastResult CastFromCamera(
            Entity cameraEntity,
            Simulation simulation,
            float maxDistance,
            float minDistance = 0f,
            CollisionFilterGroups filterGroup = CollisionFilterGroups.DefaultFilter,
            CollisionFilterGroupFlags filterFlags = CollisionFilterGroupFlags.AllFilter)
        {
            var startPosition = cameraEntity.Transform.GetWorldPosition_HS();
            var direction = cameraEntity.Transform.WorldMatrix.Forward;

            return Cast(simulation, startPosition, direction, maxDistance, minDistance, filterGroup, filterFlags);
        }

        // ================== ENTITY FILTERING HELPERS ==================

        /// <summary>
        /// Checks if hit entity should be ignored based on hierarchy relationship
        /// Returns true if hitEntity is ignoreEntity or a child of ignoreEntity
        /// </summary>
        /// <param name="hitEntity">Entity that was hit by raycast</param>
        /// <param name="ignoreEntity">Entity to ignore (including its children)</param>
        /// <returns>True if hit should be ignored, false otherwise</returns>
        public static bool EntityFiltering(Entity hitEntity, Entity ignoreEntity)
        {
            return IsEntityOrChild(hitEntity, ignoreEntity);
        }

        /// <summary>
        /// Checks if hit entity should be ignored based on list of entities
        /// Returns true if hitEntity matches any entity in ignoreEntities or is a child of any
        /// </summary>
        /// <param name="hitEntity">Entity that was hit by raycast</param>
        /// <param name="ignoreEntities">List of entities to ignore (including their children)</param>
        /// <returns>True if hit should be ignored, false otherwise</returns>
        public static bool EntityFiltering(Entity hitEntity, List<Entity> ignoreEntities)
        {
            foreach (var ignore in ignoreEntities)
            {
                if (IsEntityOrChild(hitEntity, ignore))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if hit entity has a specific component type
        /// Returns true if hitEntity contains component T, false otherwise
        /// </summary>
        /// <typeparam name="T">Component type to check for (must inherit from EntityComponent)</typeparam>
        /// <param name="hitEntity">Entity that was hit by raycast</param>
        /// <returns>True if entity has component T, false otherwise</returns>
        public static bool EntityFiltering<T>(Entity hitEntity) where T : EntityComponent
        {
            return hitEntity.Get<T>() != null;
        }

        private static bool IsEntityOrChild(Entity entity, Entity sourceEntity)
        {
            if (sourceEntity == null) return false;

            var current = entity;
            while (current != null)
            {
                if (current == sourceEntity) return true;
                current = current.GetParent();
            }

            return false;
        }
    }

    public class HSRaycastResult
    {
        public bool Succeeded { get; set; } = false;
        public HitResult HitResult { get; set; }
        public Entity Entity { get; set; }
        public Vector3 Point { get; set; }
        public Vector3 Normal { get; set; }
        public float Distance { get; set; }
    }
}