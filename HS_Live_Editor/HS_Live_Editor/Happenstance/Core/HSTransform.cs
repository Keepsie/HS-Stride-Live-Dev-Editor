// HS Stride Engine Core (c) 2025 Happenstance Games LLC - MIT License

using Stride.Core.Mathematics;
using Stride.Engine;
using System;

namespace Happenstance.SE.Core
{
    /// <summary>
    /// Static utility class providing Unity-style transform operations for Stride transforms
    /// All operations are transform-focused for consistency and clarity
    /// </summary>
    public static class HSTransform
    {
        // ================== CORE TRANSFORM EXTENSIONS ==================

        /// <summary>
        /// Rotates the transform so its forward vector points at the target transform's position
        /// </summary>
        /// <param name="transform">The transform to rotate</param>
        /// <param name="target">Transform to point towards</param>
        /// <param name="worldUp">Vector specifying the upward direction (defaults to Vector3.UnitY)</param>
        public static void LookAt_HS(this TransformComponent transform, TransformComponent target, Vector3? worldUp = null)
        {
            if (transform == null || target == null) return;
            
            var targetPosition = target.WorldMatrix.TranslationVector;
            transform.LookAt_HS(targetPosition, worldUp);
        }

        /// <summary>
        /// Rotates the transform so its forward vector points at the target entity's transform
        /// Convenience overload that extracts the transform from the entity
        /// </summary>
        /// <param name="transform">The transform to rotate</param>
        /// <param name="targetEntity">Entity to point towards</param>
        /// <param name="worldUp">Vector specifying the upward direction (defaults to Vector3.UnitY)</param>
        public static void LookAt_HS(this TransformComponent transform, Entity targetEntity, Vector3? worldUp = null)
        {
            if (transform == null || targetEntity == null) return;
            transform.LookAt_HS(targetEntity.Transform, worldUp);
        }

        /// <summary>
        /// Rotates the transform so its forward vector points at the specified world position
        /// </summary>
        /// <param name="transform">The transform to rotate</param>
        /// <param name="worldPosition">Point to look at</param>
        /// <param name="worldUp">Vector specifying the upward direction (defaults to Vector3.UnitY)</param>
        public static void LookAt_HS(this TransformComponent transform, Vector3 worldPosition, Vector3? worldUp = null)
        {
            if (transform == null) return;

            var currentPosition = transform.WorldMatrix.TranslationVector;
            var direction = worldPosition - currentPosition;

            // If positions are the same, no rotation needed
            if (direction.LengthSquared() < 0.0001f) return;

            direction.Normalize();

            // Use default up vector if none provided
            var upVector = worldUp ?? Vector3.UnitY;

            // Calculate the rotation matrix that looks in the direction
            var rotationMatrix = Matrix.LookAtRH(Vector3.Zero, direction, upVector);
            
            // Convert to quaternion and apply
            // Note: LookAtRH creates a view matrix, so we need to invert it for object rotation
            var rotation = Quaternion.RotationMatrix(rotationMatrix);
            rotation.Invert();
            
            transform.Rotation = rotation;
        }

        /// <summary>
        /// Calculates the distance between this transform and another transform
        /// </summary>
        /// <param name="transform">Source transform</param>
        /// <param name="target">Target transform</param>
        /// <returns>Distance between the transforms</returns>
        public static float DistanceFrom_HS(this TransformComponent transform, TransformComponent target)
        {
            if (transform == null || target == null) return float.MaxValue;

            var pos1 = transform.WorldMatrix.TranslationVector;
            var pos2 = target.WorldMatrix.TranslationVector;
            
            return Vector3.Distance(pos1, pos2);
        }

        /// <summary>
        /// Calculates the distance between this transform and a target entity's transform
        /// Convenience overload that extracts the transform from the entity
        /// </summary>
        /// <param name="transform">Source transform</param>
        /// <param name="targetEntity">Target entity</param>
        /// <returns>Distance between the transforms</returns>
        public static float DistanceFrom_HS(this TransformComponent transform, Entity targetEntity)
        {
            if (transform == null || targetEntity == null) return float.MaxValue;
            return transform.DistanceFrom_HS(targetEntity.Transform);
        }

        /// <summary>
        /// Calculates the distance between this transform and a world position
        /// </summary>
        /// <param name="transform">Source transform</param>
        /// <param name="position">Target world position</param>
        /// <returns>Distance to the position</returns>
        public static float DistanceFrom_HS(this TransformComponent transform, Vector3 position)
        {
            if (transform == null) return float.MaxValue;

            var transformPosition = transform.WorldMatrix.TranslationVector;
            return Vector3.Distance(transformPosition, position);
        }

        /// <summary>
        /// Calculates the squared distance between this transform and another transform
        /// Faster than DistanceFrom when you only need to compare distances
        /// </summary>
        /// <param name="transform">Source transform</param>
        /// <param name="target">Target transform</param>
        /// <returns>Squared distance between the transforms</returns>
        public static float DistanceSquaredFrom_HS(this TransformComponent transform, TransformComponent target)
        {
            if (transform == null || target == null) return float.MaxValue;

            var pos1 = transform.WorldMatrix.TranslationVector;
            var pos2 = target.WorldMatrix.TranslationVector;
            
            return Vector3.DistanceSquared(pos1, pos2);
        }

        /// <summary>
        /// Calculates the squared distance between this transform and a target entity's transform
        /// Convenience overload that extracts the transform from the entity
        /// </summary>
        /// <param name="transform">Source transform</param>
        /// <param name="targetEntity">Target entity</param>
        /// <returns>Squared distance between the transforms</returns>
        public static float DistanceSquaredFrom_HS(this TransformComponent transform, Entity targetEntity)
        {
            if (transform == null || targetEntity == null) return float.MaxValue;
            return transform.DistanceSquaredFrom_HS(targetEntity.Transform);
        }

        /// <summary>
        /// Calculates the squared distance between this transform and a world position
        /// Faster than DistanceFrom when you only need to compare distances
        /// </summary>
        /// <param name="transform">Source transform</param>
        /// <param name="position">Target world position</param>
        /// <returns>Squared distance to the position</returns>
        public static float DistanceSquaredFrom_HS(this TransformComponent transform, Vector3 position)
        {
            if (transform == null) return float.MaxValue;

            var transformPosition = transform.WorldMatrix.TranslationVector;
            return Vector3.DistanceSquared(transformPosition, position);
        }

        /// <summary>
        /// Calculates the angle between this transform's forward direction and a target transform
        /// Useful for vision cone calculations
        /// </summary>
        /// <param name="transform">Source transform</param>
        /// <param name="target">Target transform</param>
        /// <returns>Angle in degrees</returns>
        public static float AngleTo_HS(this TransformComponent transform, TransformComponent target)
        {
            if (transform == null || target == null) return 0f;

            var targetPosition = target.WorldMatrix.TranslationVector;
            return transform.AngleTo_HS(targetPosition);
        }

        /// <summary>
        /// Calculates the angle between this transform's forward direction and a target entity's transform
        /// Convenience overload that extracts the transform from the entity
        /// </summary>
        /// <param name="transform">Source transform</param>
        /// <param name="targetEntity">Target entity</param>
        /// <returns>Angle in degrees</returns>
        public static float AngleTo_HS(this TransformComponent transform, Entity targetEntity)
        {
            if (transform == null || targetEntity == null) return 0f;
            return transform.AngleTo_HS(targetEntity.Transform);
        }

        /// <summary>
        /// Calculates the angle between this transform's forward direction and a target position
        /// Useful for vision cone calculations
        /// </summary>
        /// <param name="transform">Source transform</param>
        /// <param name="position">Target world position</param>
        /// <returns>Angle in degrees</returns>
        public static float AngleTo_HS(this TransformComponent transform, Vector3 position)
        {
            if (transform == null) return 0f;

            // Ensure world matrix is up to date (like PlayerController does)
            transform.UpdateWorldMatrix();

            var currentPosition = transform.WorldMatrix.TranslationVector;
            var direction = position - currentPosition;

            if (direction.LengthSquared() < 0.0001f) return 0f;

            direction.Normalize();

            var forward = transform.WorldMatrix.Forward;
            var dot = Vector3.Dot(direction, forward);

            // Clamp dot product to valid range for acos
            dot = MathUtil.Clamp(dot, -1.0f, 1.0f);

            var angleRadians = (float)Math.Acos(dot);
            return MathUtil.RadiansToDegrees(angleRadians);
        }

        /// <summary>
        /// Gets Euler angles in degrees from this transform's rotation
        /// </summary>
        /// <param name="transform">Transform to get rotation from</param>
        /// <returns>Euler angles in degrees (X=Pitch, Y=Yaw, Z=Roll)</returns>
        public static Vector3 GetEulerAngles_HS(this TransformComponent transform)
        {
            if (transform == null) return Vector3.Zero;
            return transform.Rotation.ToEulerAngles_HS();
        }

        /// <summary>
        /// Sets transform rotation using Euler angles in degrees
        /// </summary>
        /// <param name="transform">Transform to rotate</param>
        /// <param name="eulerDegrees">Euler angles in degrees (X=Pitch, Y=Yaw, Z=Roll)</param>
        public static void SetEulerAngles_HS(this TransformComponent transform, Vector3 eulerDegrees)
        {
            if (transform == null) return;
            transform.Rotation = FromEulerAngles_HS(eulerDegrees);
        }

        /// <summary>
        /// Gets the forward direction of a transform as a normalized Vector3
        /// </summary>
        /// <param name="transform">Transform to get forward direction from</param>
        /// <returns>Normalized forward direction</returns>
        public static Vector3 GetForward_HS(this TransformComponent transform)
        {
            if (transform == null) return Vector3.UnitZ;
            return (Vector3)transform.WorldMatrix.Forward;
        }

        /// <summary>
        /// Gets the forward direction from a model entity (typically a child entity with visual mesh).
        /// Useful for getting facing direction when model entity is separate from parent logic entity.
        /// </summary>
        /// <param name="transform">Transform to get forward direction from (typically a child model entity)</param>
        /// <param name="reverseForBlender">If true, inverts the forward direction to account for Blender models facing -Z in model space (default: true)</param>
        /// <returns>Normalized forward direction (inverted if reverseForBlender is true)</returns>
        public static Vector3 GetForwardFromModel_HS(this TransformComponent transform, bool reverseForBlender = true)
        {
            if (transform == null) return Vector3.UnitZ;

            // Force matrix update to get current rotation (Stride quirk - see TROUBLESHOOTING.md)
            transform.UpdateWorldMatrix();

            var forward = (Vector3)transform.WorldMatrix.Forward;

            // Invert for Blender models that face -Z in model space
            return reverseForBlender ? -forward : forward;
        }

        /// <summary>
        /// Gets the right direction of a transform as a normalized Vector3
        /// </summary>
        /// <param name="transform">Transform to get right direction from</param>
        /// <returns>Normalized right direction</returns>
        public static Vector3 GetRight_HS(this TransformComponent transform)
        {
            if (transform == null) return Vector3.UnitX;
            return (Vector3)transform.WorldMatrix.Right;
        }

        /// <summary>
        /// Gets the up direction of a transform as a normalized Vector3
        /// </summary>
        /// <param name="transform">Transform to get up direction from</param>
        /// <returns>Normalized up direction</returns>
        public static Vector3 GetUp_HS(this TransformComponent transform)
        {
            if (transform == null) return Vector3.UnitY;
            return (Vector3)transform.WorldMatrix.Up;
        }

        /// <summary>
        /// Forces world matrix update and returns world position
        /// Useful when you need the absolute latest position
        /// </summary>
        /// <param name="transform">Transform to get world position from</param>
        /// <returns>World position</returns>
        public static Vector3 GetWorldPosition_HS(this TransformComponent transform)
        {
            if (transform == null) return Vector3.Zero;
            transform.UpdateWorldMatrix();
            return transform.WorldMatrix.TranslationVector;
        }

        /// <summary>
        /// Gets world position without forcing update (faster)
        /// Use only when getting something static. Do not use for tracking player or something moving use normal one.
        /// </summary>
        /// <param name="transform">Transform to get world position from</param>
        /// <returns>World position</returns>
        public static Vector3 GetWorldPositionFast_HS(this TransformComponent transform)
        {
            if (transform == null) return Vector3.Zero;
            return transform.WorldMatrix.TranslationVector;
        }

        /// <summary>
        /// Forces world matrix update and returns world rotation
        /// Useful when you need the absolute latest rotation
        /// </summary>
        /// <param name="transform">Transform to get world rotation from</param>
        /// <returns>World rotation</returns>
        public static Quaternion GetWorldRotation_HS(this TransformComponent transform)
        {
            if (transform == null) return Quaternion.Identity;
            transform.UpdateWorldMatrix();
            return Quaternion.RotationMatrix(transform.WorldMatrix);
        }

        /// <summary>
        /// Smoothly rotates transform towards target rotation
        /// </summary>
        /// <param name="transform">Transform to rotate</param>
        /// <param name="target">Target rotation</param>
        /// <param name="speed">Rotation speed (higher = faster)</param>
        /// <param name="deltaTime">Frame delta time</param>
        public static void SmoothRotateTo_HS(this TransformComponent transform, Quaternion target, float speed, float deltaTime)
        {
            if (transform == null) return;
            transform.Rotation = SmoothRotateTo_HS(transform.Rotation, target, speed, deltaTime);
        }

        /// <summary>
        /// Smoothly rotates transform to look at target position
        /// </summary>
        /// <param name="transform">Transform to rotate</param>
        /// <param name="targetPosition">Position to look at</param>
        /// <param name="speed">Rotation speed (higher = faster)</param>
        /// <param name="deltaTime">Frame delta time</param>
        /// <param name="worldUp">Up vector (defaults to Vector3.UnitY)</param>
        public static void SmoothLookAt_HS(this TransformComponent transform, Vector3 targetPosition, float speed, float deltaTime, Vector3? worldUp = null)
        {
            if (transform == null) return;
            
            var currentPosition = transform.WorldMatrix.TranslationVector;
            var direction = targetPosition - currentPosition;
            
            if (direction.LengthSquared() < 0.0001f) return;
            
            var targetRotation = LookRotation_HS(direction, worldUp);
            transform.SmoothRotateTo_HS(targetRotation, speed, deltaTime);
        }

        /// <summary>
        /// Smoothly rotates transform to look at target transform
        /// </summary>
        /// <param name="transform">Transform to rotate</param>
        /// <param name="target">Transform to look at</param>
        /// <param name="speed">Rotation speed (higher = faster)</param>
        /// <param name="deltaTime">Frame delta time</param>
        /// <param name="worldUp">Up vector (defaults to Vector3.UnitY)</param>
        public static void SmoothLookAt_HS(this TransformComponent transform, TransformComponent target, float speed, float deltaTime, Vector3? worldUp = null)
        {
            if (transform == null || target == null) return;
            
            var targetPosition = target.WorldMatrix.TranslationVector;
            transform.SmoothLookAt_HS(targetPosition, speed, deltaTime, worldUp);
        }

        /// <summary>
        /// Smoothly rotates transform to look at target entity's transform
        /// Convenience overload that extracts the transform from the entity
        /// </summary>
        /// <param name="transform">Transform to rotate</param>
        /// <param name="targetEntity">Entity to look at</param>
        /// <param name="speed">Rotation speed (higher = faster)</param>
        /// <param name="deltaTime">Frame delta time</param>
        /// <param name="worldUp">Up vector (defaults to Vector3.UnitY)</param>
        public static void SmoothLookAt_HS(this TransformComponent transform, Entity targetEntity, float speed, float deltaTime, Vector3? worldUp = null)
        {
            if (transform == null || targetEntity == null) return;
            transform.SmoothLookAt_HS(targetEntity.Transform, speed, deltaTime, worldUp);
        }

        /// <summary>
        /// Rotates transform to look at target position using Euler angles (more reliable than quaternion LookAt)
        /// Uses same method as HSNavigator for consistent rotation behavior
        /// </summary>
        /// <param name="transform">Transform to rotate</param>
        /// <param name="targetPosition">Position to look at</param>
        public static void LookAtEuler_HS(this TransformComponent transform, Vector3 targetPosition)
        {
            if (transform == null) return;

            var currentPosition = transform.WorldMatrix.TranslationVector;
            var direction = targetPosition - currentPosition;

            if (direction.LengthSquared() < 0.0001f) return;

            direction.Normalize();

            // Convert direction to yaw angle (same as HSNavigator)
            float yaw = DirectionToYaw_HS(direction);
            float yawDegrees = MathUtil.RadiansToDegrees(yaw);

            // Apply rotation using Euler angles
            transform.SetEulerAngles_HS(new Vector3(0, yawDegrees, 0));
        }

        /// <summary>
        /// Smoothly rotates transform to look at target position using Euler angles
        /// Uses same smooth rotation logic as HSNavigator for consistent behavior
        /// </summary>
        /// <param name="transform">Transform to rotate</param>
        /// <param name="targetPosition">Position to look at</param>
        /// <param name="rotationSpeed">Rotation speed in degrees per second</param>
        /// <param name="deltaTime">Frame delta time</param>
        public static void SmoothLookAtEuler_HS(this TransformComponent transform, Vector3 targetPosition, float rotationSpeed, float deltaTime)
        {
            if (transform == null) return;

            var currentPosition = transform.WorldMatrix.TranslationVector;
            var direction = targetPosition - currentPosition;

            if (direction.LengthSquared() < 0.0001f) return;

            direction.Normalize();

            // Get current yaw from transform
            var currentEuler = transform.GetEulerAngles_HS();
            float currentYaw = MathUtil.DegreesToRadians(currentEuler.Y);

            // Calculate target yaw (same as HSNavigator)
            float targetYaw = DirectionToYaw_HS(direction);

            // Calculate shortest rotation path (same as HSNavigator)
            float angleDiff = targetYaw - currentYaw;
            while (angleDiff > MathUtil.Pi) angleDiff -= MathUtil.TwoPi;
            while (angleDiff < -MathUtil.Pi) angleDiff += MathUtil.TwoPi;

            // Apply smooth rotation with threshold (same as HSNavigator)
            if (Math.Abs(angleDiff) > 0.01f)
            {
                float rotationDelta = MathUtil.DegreesToRadians(rotationSpeed) * deltaTime;
                float rotationAmount = Math.Sign(angleDiff) * Math.Min(Math.Abs(angleDiff), rotationDelta);
                float newYaw = currentYaw + rotationAmount;
                float newYawDegrees = MathUtil.RadiansToDegrees(newYaw);

                transform.SetEulerAngles_HS(new Vector3(0, newYawDegrees, 0));
            }
        }

        /// <summary>
        /// Check if target position is within vision cone based on distance and angle.
        /// Uses inverted forward direction to account for Blender models facing backwards in model space.
        /// Useful for AI vision, security cameras, detection systems, etc.
        /// </summary>
        /// <param name="transform">Transform doing the vision check (enemy, camera, etc.)</param>
        /// <param name="targetPosition">World position to check visibility for</param>
        /// <param name="visionDistance">Maximum vision distance</param>
        /// <param name="visionAngle">Vision cone angle in degrees (full cone, not half-angle)</param>
        /// <param name="invertForward">If true, inverts forward direction for Blender models (default: true)</param>
        /// <returns>True if target is within vision distance and vision cone</returns>
        public static bool VisionConeTarget_HS(this TransformComponent transform, Vector3 targetPosition, float visionDistance, float visionAngle, bool invertForward = true)
        {
            if (transform == null) return false;

            var observerPos = transform.GetWorldPosition_HS();

            // Check distance first (cheaper than angle check)
            float distance = transform.DistanceFrom_HS(targetPosition);
            if (distance > visionDistance) return false;

            // Calculate angle to target
            var forward = invertForward ? -transform.GetForward_HS() : transform.GetForward_HS();
            var directionToTarget = Vector3.Normalize(targetPosition - observerPos);

            var dot = Vector3.Dot(directionToTarget, forward);
            dot = MathUtil.Clamp(dot, -1.0f, 1.0f);

            var angleRadians = (float)Math.Acos(dot);
            var angle = MathUtil.RadiansToDegrees(angleRadians);

            // Check if within vision cone (half-angle comparison)
            return angle <= visionAngle * 0.5f;
        }

        // ================== SURFACE PLACEMENT UTILITIES ==================

        /// <summary>
        /// Places a decal transform on a surface with proper position and rotation alignment
        /// Useful for bullet holes, blood splatters, scorch marks, etc.
        /// The decal's local Z axis will align with the surface normal (pointing away from surface)
        /// </summary>
        /// <param name="transform">Transform of the decal to place</param>
        /// <param name="hitPoint">World position where the surface was hit</param>
        /// <param name="hitNormal">Normal vector of the surface at hit point</param>
        /// <param name="normalOffset">Distance to offset decal along normal to prevent z-fighting (default: 0.01f)</param>
        /// <param name="randomRotation">If true, applies random roll rotation (Z-axis) for visual variety (default: false)</param>
        public static void PlaceDecalOnSurface_HS(this TransformComponent transform, Vector3 hitPoint, Vector3 hitNormal, float normalOffset = 0.03f, bool randomRotation = false)
        {
            if (transform == null) return;

            // Position: offset along surface normal to prevent z-fighting
            transform.Position = hitPoint + (hitNormal * normalOffset);

            // Rotation: align decal's local Z axis to point along the surface normal
            transform.Rotation = Quaternion.BetweenDirections(Vector3.UnitZ, hitNormal);

            // Optional: Add random Z rotation (roll) for visual variety
            // This works by directly modifying the Euler angle Z component (roll)
            // which rotates around the local Z-axis (surface normal) regardless of wall orientation
            if (randomRotation)
            {
                var euler = transform.GetEulerAngles_HS();
                euler.Z = HSRandom.Range(0f, 360f);
                transform.SetEulerAngles_HS(euler);
            }
        }

        // ================== PARENTING UTILITIES ==================

        /// <summary>
        /// Safely unparents a transform while preserving its world position, rotation, and scale
        /// Fixes Stride's issue where unparenting treats local position as world position
        /// </summary>
        /// <param name="transform">Transform to unparent</param>
        public static void UnparentPreserveWorld_HS(this TransformComponent transform)
        {
            if (transform == null || transform.Parent == null) return;

            // Store current world position, rotation, scale, and scene reference
            var worldPos = transform.GetWorldPosition_HS();
            var worldRot = transform.GetWorldRotation_HS();
            var worldScale = transform.Scale; // Scale is already world scale
            var scene = transform.Entity.Scene;

            // Unparent (this breaks world position and rotation in Stride)
            transform.Parent = null;

            // Restore correct world position, rotation, and scale
            transform.Position = worldPos;
            transform.Rotation = worldRot;
            transform.Scale = worldScale;

            // Add back to scene root entities if not already there
            if (scene != null && !scene.Entities.Contains(transform.Entity))
            {
                scene.Entities.Add(transform.Entity);
            }
        }

        /// <summary>
        /// Sets parent while maintaining current world position, rotation, and scale
        /// Item will appear in same world location after parenting
        /// </summary>
        /// <param name="transform">Transform to parent</param>
        /// <param name="newParent">New parent transform</param>
        public static void SetParentPreserveWorld_HS(this TransformComponent transform, TransformComponent newParent)
        {
            if (transform == null || newParent == null) return;

            // Store current world position, rotation, and scale
            var worldPos = transform.GetWorldPosition_HS();
            var worldRot = transform.GetWorldRotation_HS();
            var worldScale = transform.Scale;

            // Set parent
            transform.Parent = newParent;

            // Calculate local position and rotation
            transform.Position = newParent.WorldToLocal_HS(worldPos);
            transform.Rotation = newParent.WorldToLocalRotation_HS(worldRot);
            transform.Scale = worldScale; // Preserve scale
        }
        
        /// <summary>
        /// Converts a world space position to the local space of this transform.
        /// </summary>
        /// <param name="transform">The parent transform defining the local space.</param>
        /// <param name="worldPosition">The world position to convert.</param>
        /// <returns>The position in local space.</returns>
        public static Vector3 WorldToLocal_HS(this TransformComponent transform, Vector3 worldPosition)
        {
            transform.UpdateWorldMatrix();
            Matrix.Invert(ref transform.WorldMatrix, out var inverseWorldMatrix);
            Vector3.Transform(ref worldPosition, ref inverseWorldMatrix, out Vector3 localPosition);
            return localPosition;
        }

        /// <summary>
        /// Converts a local space position of this transform to a world space position.
        /// </summary>
        /// <param name="transform">The parent transform defining the local space.</param>
        /// <param name="localPosition">The local position to convert.</param>
        /// <returns>The position in world space.</returns>
        public static Vector3 LocalToWorld_HS(this TransformComponent transform, Vector3 localPosition)
        {
            transform.UpdateWorldMatrix();
            Vector3.Transform(ref localPosition, ref transform.WorldMatrix, out Vector3 worldPosition);
            return worldPosition;
        }

        /// <summary>
        /// Converts a world space rotation to the local space of this transform.
        /// </summary>
        /// <param name="transform">The parent transform defining the local space.</param>
        /// <param name="worldRotation">The world rotation to convert.</param>
        /// <returns>The rotation in local space.</returns>
        public static Quaternion WorldToLocalRotation_HS(this TransformComponent transform, Quaternion worldRotation)
        {
            var parentWorldRotation = transform.GetWorldRotation_HS();
            Quaternion.Invert(ref parentWorldRotation, out var invParentWorldRotation);
            Quaternion.Multiply(ref invParentWorldRotation, ref worldRotation, out var localRotation);
            return localRotation;
        }

        /// <summary>
        /// Converts a local space rotation of this transform to a world space rotation.
        /// </summary>
        /// <param name="transform">The parent transform defining the local space.</param>
        /// <param name="localRotation">The local rotation to convert.</param>
        /// <returns>The rotation in world space.</returns>
        public static Quaternion LocalToWorldRotation_HS(this TransformComponent transform, Quaternion localRotation)
        {
            var parentWorldRotation = transform.GetWorldRotation_HS();
            Quaternion.Multiply(ref parentWorldRotation, ref localRotation, out var worldRotation);
            return worldRotation;
        }

        /// <summary>
        /// Calculates what local position and rotation would result in the target world transform relative to a parent.
        /// </summary>
        /// <param name="parent">The parent transform to calculate relative to</param>
        /// <param name="targetWorldPos">The desired world position</param>
        /// <param name="targetWorldRot">The desired world rotation</param>
        /// <returns>Local position and rotation that would achieve the target world transform</returns>
        public static (Vector3 localPos, Quaternion localRot) CalculateLocalTransform_HS(
            this TransformComponent parent,
            Vector3 targetWorldPos,
            Quaternion targetWorldRot)
        {
            if (parent == null)
            {
                // No parent, world and local are the same
                return (targetWorldPos, targetWorldRot);
            }

            // Get parent's world transform
            var parentWorldPos = parent.GetWorldPosition_HS();
            var parentWorldRot = parent.GetWorldRotation_HS();

            // Calculate relative position
            var relativePos = targetWorldPos - parentWorldPos;

            // Rotate relative position by inverse of parent rotation to get local position
            Quaternion.Invert(ref parentWorldRot, out var invParentRot);
            Vector3.Transform(ref relativePos, ref invParentRot, out Vector3 localPos);

            // Calculate relative rotation
            Quaternion.Multiply(ref invParentRot, ref targetWorldRot, out var localRot);

            return (localPos, localRot);
        }

        // ================== STATIC QUATERNION & MATH UTILITIES ==================
        // Pure math functions that don't belong to any specific object

        /// <summary>
        /// Converts a quaternion to Euler angles in degrees (Yaw, Pitch, Roll)
        /// Uses the same proven math from DevEditorManager
        /// </summary>
        /// <param name="quaternion">Quaternion to convert</param>
        /// <returns>Euler angles in degrees (X=Pitch, Y=Yaw, Z=Roll)</returns>
        public static Vector3 ToEulerAngles_HS(this Quaternion quaternion)
        {
            float pitch = (float)Math.Asin(2.0f * (quaternion.W * quaternion.X - quaternion.Y * quaternion.Z));
            float yaw = (float)Math.Atan2(2.0f * (quaternion.W * quaternion.Y + quaternion.X * quaternion.Z),
                                         1.0f - 2.0f * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y));
            float roll = (float)Math.Atan2(2.0f * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y),
                                          1.0f - 2.0f * (quaternion.X * quaternion.X + quaternion.Z * quaternion.Z));

            return new Vector3(
                MathUtil.RadiansToDegrees(pitch),   // X = Pitch
                MathUtil.RadiansToDegrees(yaw),     // Y = Yaw  
                MathUtil.RadiansToDegrees(roll)     // Z = Roll
            );
        }

        /// <summary>
        /// Creates a quaternion from Euler angles in degrees
        /// </summary>
        /// <param name="eulerDegrees">Euler angles in degrees (X=Pitch, Y=Yaw, Z=Roll)</param>
        /// <returns>Quaternion rotation</returns>
        public static Quaternion FromEulerAngles_HS(Vector3 eulerDegrees)
        {
            return Quaternion.RotationYawPitchRoll(
                MathUtil.DegreesToRadians(eulerDegrees.Y),  // Yaw
                MathUtil.DegreesToRadians(eulerDegrees.X),  // Pitch
                MathUtil.DegreesToRadians(eulerDegrees.Z)   // Roll
            );
        }

        /// <summary>
        /// Creates a rotation that looks in the specified direction
        /// </summary>
        /// <param name="direction">Direction to look (should be normalized)</param>
        /// <param name="up">Up vector (defaults to Vector3.UnitY)</param>
        /// <returns>Quaternion rotation</returns>
        public static Quaternion LookRotation_HS(Vector3 direction, Vector3? up = null)
        {
            if (direction.LengthSquared() < 0.0001f) return Quaternion.Identity;
            
            direction.Normalize();
            var upVector = up ?? Vector3.UnitY;
            
            var rotationMatrix = Matrix.LookAtRH(Vector3.Zero, direction, upVector);
            var rotation = Quaternion.RotationMatrix(rotationMatrix);
            rotation.Invert();
            
            return rotation;
        }

        /// <summary>
        /// Converts a movement direction to a yaw angle (useful for 2D-style rotation)
        /// </summary>
        /// <param name="direction">Movement direction</param>
        /// <returns>Yaw angle in radians</returns>
        public static float DirectionToYaw_HS(Vector3 direction)
        {
            if (direction.LengthSquared() < 0.0001f) return 0f;
            return (float)Math.Atan2(-direction.Z, direction.X) + MathUtil.PiOverTwo;
        }

        /// <summary>
        /// Smoothly rotates from current rotation towards target rotation
        /// </summary>
        /// <param name="current">Current rotation</param>
        /// <param name="target">Target rotation</param>
        /// <param name="speed">Rotation speed (higher = faster)</param>
        /// <param name="deltaTime">Frame delta time</param>
        /// <returns>Interpolated rotation</returns>
        public static Quaternion SmoothRotateTo_HS(Quaternion current, Quaternion target, float speed, float deltaTime)
        {
            return Quaternion.Slerp(current, target, speed * deltaTime);
        }
        
    }
}