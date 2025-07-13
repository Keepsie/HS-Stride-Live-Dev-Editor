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
        /// Use when you know the world matrix is already current
        /// </summary>
        /// <param name="transform">Transform to get world position from</param>
        /// <returns>World position</returns>
        public static Vector3 GetWorldPositionFast_HS(this TransformComponent transform)
        {
            if (transform == null) return Vector3.Zero;
            return transform.WorldMatrix.TranslationVector;
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