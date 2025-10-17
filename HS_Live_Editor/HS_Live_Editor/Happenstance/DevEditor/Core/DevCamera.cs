// HS Live Dev Editor (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Core;
using Happenstance.SE.DevEditor.Core;
using Happenstance.SE.Logger.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using System;
using System.Linq;

namespace Happenstance.SE.DevEditor.Core
{
    public class DevCamera : HSSyncScript
    {
        // Settings
        public float MovementSpeed = 1.0f;
        public float FastMovementSpeed = 5.0f;
        public float SlowMovementSpeed = 2.0f;
        public float RotationSpeed = 5f;

        // References
        private DevEditorManager _editorManager;
        private CameraComponent _devCameraComponent;

        public Entity MainCameraEntity;
        private CameraComponent _mainCameraComponent;

        // Camera control state
        private bool _isActive = false;
        private float _yaw;
        private float _pitch;

        // Save states for dev camera persistence
        private Vector3 _lastDevCameraPosition;
        private Quaternion _lastDevCameraRotation;
        private bool _hasDevCameraState = false;

        /// <summary>
        /// Indicates whether the development camera is currently active and controlling input.
        /// Other systems should check this property to avoid input conflicts when the dev camera is enabled.
        /// When true, systems like player controllers, console input, etc. should suspend their input handling.
        /// </summary>
        public bool DevCameraActive { get; private set; }

        protected override void OnStart()
        {
          
            // Get our own camera component
            _devCameraComponent = Entity.Get<CameraComponent>();
            if (_devCameraComponent == null)
            {
                _devCameraComponent = Entity.GetOrCreate<CameraComponent>();
                Logger.Info("Created dev camera component");
            }

            // Find main camera in scene (exclude our own dev camera)
            if (MainCameraEntity != null)
            {
                _mainCameraComponent = MainCameraEntity.Get<CameraComponent>();
                Logger.Info($"Using manually assigned main camera: {MainCameraEntity.Name}");
            }
            else
            {
                // Try to find automatically (exclude dev cameras)
                var allCameraEntities = Entity.Scene.FindEntitiesWithComponent_HS<CameraComponent>();
                MainCameraEntity = allCameraEntities.FirstOrDefault(e => e.Get<DevCamera>() == null);

                if (MainCameraEntity != null)
                {
                    _mainCameraComponent = MainCameraEntity.Get<CameraComponent>();
                    Logger.Info($"Auto-found main camera: {MainCameraEntity.Name}");
                }
            }

            // If we still don't have a main camera, destroy ourselves
            if (_mainCameraComponent == null)
            {
                Logger.Error("DevCamera: No main camera found and none assigned. Destroying dev camera.");
                Entity.Destroy_HS();
                return;
            }
            else
            {
                //Slots might change per project import so reset just incase
                _devCameraComponent.Slot = _mainCameraComponent.Slot;
            }

                // Find editor manager
                _editorManager = Entity.Scene.FindAllComponents_HS<DevEditorManager>().FirstOrDefault();

            // Initially disabled
            _devCameraComponent.Enabled = false;

            // Set initial position if no saved state
            if (!_hasDevCameraState)
            {
                Entity.Transform.Position = new Vector3(0, 5, 10);
                Entity.Transform.Rotation = Quaternion.Identity;
            }

            // Calculate initial rotation using Stride's method
            Reset();
        }

        protected override void OnUpdate()
        {
            // Check for toggle
            if (Input.IsKeyPressed(Keys.F10) && _editorManager.DevEditorActive)
            {
                ToggleDevCamera();
            }

            // Skip if not active
            if (!_isActive) return;

            // Handle scroll wheel speed adjustment for camera movement
            float scrollDelta = Input.MouseWheelDelta;
            if (Math.Abs(scrollDelta) > 0.01f)
            {
                // Adjust camera movement speed with scroll wheel
                float speedChange = scrollDelta * 1.0f; // Sensitivity
                MovementSpeed = MathUtil.Clamp(MovementSpeed + speedChange, 0.1f, 100.0f);

                // Also adjust the fast/slow speeds proportionally
                FastMovementSpeed = MovementSpeed * 2.5f;
                SlowMovementSpeed = MovementSpeed * 0.2f;

                Logger.Debug($"Dev camera speed: {MovementSpeed:F1} (Fast: {FastMovementSpeed:F1}, Slow: {SlowMovementSpeed:F1})");
            }

            // Update camera
            UpdateCamera();

            // Handle focus on selected entity
            if (!Input.IsKeyDown(Keys.LeftCtrl) && !Input.IsKeyDown(Keys.LeftShift) && Input.IsKeyPressed(Keys.F))
            {
                FocusOnSelectedEntity();
            }
        }

        // Stride's proven Reset method
        public void Reset()
        {
            _yaw = (float)Math.Asin(2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.Y +
                                   2 * Entity.Transform.Rotation.Z * Entity.Transform.Rotation.W);

            _pitch = (float)Math.Atan2(
                2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.W -
                2 * Entity.Transform.Rotation.Y * Entity.Transform.Rotation.Z,
                1 - 2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.X -
                2 * Entity.Transform.Rotation.Z * Entity.Transform.Rotation.Z);
        }

        protected virtual void UpdateCamera()
        {
            if (Input.IsMouseButtonDown(MouseButton.Right))
            {
                Vector2 cameraMovement = Input.MouseDelta;

                // Log the current speed calculations
                float effectiveYawSpeed = cameraMovement.X * RotationSpeed;
                float effectivePitchSpeed = cameraMovement.Y * RotationSpeed;

                _yaw -= effectiveYawSpeed;
                _pitch = MathUtil.Clamp(_pitch - effectivePitchSpeed,
                           -MathUtil.PiOverTwo, MathUtil.PiOverTwo);

                var rotation = HSTransform.FromEulerAngles_HS(new Vector3(_pitch * MathUtil.RadiansToDegrees(1f), _yaw * MathUtil.RadiansToDegrees(1f), 0));
                Entity.Transform.Rotation = rotation;
            }

            if (Input.IsKeyDown(Keys.LeftCtrl) && Input.IsKeyDown(Keys.LeftShift) && Input.IsKeyPressed(Keys.F))
            {
                AlignSelectedEntityWithView(false);
            }

            if (Input.IsKeyDown(Keys.LeftCtrl) && Input.IsKeyDown(Keys.LeftShift) && Input.IsKeyPressed(Keys.R))
            {
                AlignSelectedEntityWithView(true);
            }

            UpdateMovement();
        }

        private void AlignSelectedEntityWithView(bool reverseRotation = false)
        {
            if (_editorManager == null || _editorManager.SelectedEntity == null)
            {
                Logger.Warning("No entity selected for alignment");
                return;
            }

            Entity targetEntity = _editorManager.SelectedEntity;
            if (targetEntity == null) return;

            // Get current world position for undo using HS helper
            var oldWorldPosition = targetEntity.Transform.GetWorldPosition_HS();
            var oldRotation = targetEntity.Transform.GetWorldRotation_HS();

            // Get desired world position and rotation from camera using HS helper
            var cameraWorldPos = Entity.Transform.GetWorldPosition_HS();
            var cameraWorldRotation = Entity.Transform.GetWorldRotation_HS();

            // Apply 180-degree Y rotation offset for Blender models if requested
            if (reverseRotation)
            {
                var euler = cameraWorldRotation.ToEulerAngles_HS();
                euler.X = -euler.X;  // Flip pitch (invert up/down)
                euler.Y += 180f;     // Rotate 180 degrees around Y (turn around)
                cameraWorldRotation = HSTransform.FromEulerAngles_HS(euler);
            }

            // Use manual coordinate calculation to bypass Stride's broken system
            if (targetEntity.Transform.Parent != null)
            {
                var (localPos, localRot) = targetEntity.Transform.Parent.CalculateLocalTransform_HS(cameraWorldPos, cameraWorldRotation);
                targetEntity.Transform.Position = localPos;
                targetEntity.Transform.Rotation = localRot;

                Logger.Info($"Aligned {targetEntity.Name} to camera{(reverseRotation ? " (reversed)" : "")} - Parent: {targetEntity.Transform.Parent.Entity.Name}, Local Pos: {localPos}");
            }
            else
            {
                // No parent, can set world coordinates directly
                targetEntity.Transform.Position = cameraWorldPos;
                targetEntity.Transform.Rotation = cameraWorldRotation;

                Logger.Info($"Aligned {targetEntity.Name} to camera{(reverseRotation ? " (reversed)" : "")} - Root entity, World Pos: {cameraWorldPos}");
            }

            // Create undo command if the editor manager has history
            if (_editorManager.History != null)
            {
                var newWorldPos = targetEntity.Transform.GetWorldPosition_HS();
                var command = TransformCommand.CreatePositionRotationCommand(
                    targetEntity,
                    oldWorldPosition, newWorldPos,
                    oldRotation, targetEntity.Transform.Rotation,
                    "Align with View");
                _editorManager.History.StoreChange(command);
            }
        }

        private void UpdateMovement()
        {
            // Don't process movement if user is typing in filter field
            if (_editorManager != null && _editorManager.IsFilterInputActive)
            {
                return;
            }

            // Get input
            bool moveForward = Input.IsKeyDown(Keys.W);
            bool moveBackward = Input.IsKeyDown(Keys.S);
            bool moveLeft = Input.IsKeyDown(Keys.A);
            bool moveRight = Input.IsKeyDown(Keys.D);
            bool moveUp = Input.IsKeyDown(Keys.E);
            bool moveDown = Input.IsKeyDown(Keys.Q);

            // Check modifiers
            bool fastModifier = Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift);
            bool slowModifier = Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl);

            // Determine speed
            float effectiveSpeed = MovementSpeed;
            if (fastModifier) effectiveSpeed = FastMovementSpeed;
            if (slowModifier) effectiveSpeed = SlowMovementSpeed;

            // Apply delta time
            effectiveSpeed *= (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // Calculate movement using transform matrix (Stride way)
            Vector3 movement = Vector3.Zero;
            var worldMatrix = Entity.Transform.WorldMatrix;

            if (moveForward) movement += (Vector3)worldMatrix.Forward;
            if (moveBackward) movement -= (Vector3)worldMatrix.Forward;
            if (moveRight) movement += (Vector3)worldMatrix.Right;
            if (moveLeft) movement -= (Vector3)worldMatrix.Right;
            if (moveUp) movement += (Vector3)worldMatrix.Up;
            if (moveDown) movement -= (Vector3)worldMatrix.Up;

            // Apply movement
            if (movement.Length() > 0.001f)
            {
                movement.Normalize();
                movement *= effectiveSpeed;
                Entity.Transform.Position += movement;
            }
        }

        private void FocusOnSelectedEntity()
        {
            if (_editorManager == null || _editorManager.SelectedEntity == null) return;

            Entity target = _editorManager.SelectedEntity;

            // Get TRUE world position using HS helper (handles parented entities correctly)
            Vector3 targetWorldPos = target.Transform.GetWorldPosition_HS();

            // Get current camera world position to calculate direction
            Vector3 currentCameraWorldPos = Entity.Transform.GetWorldPosition_HS();

            // Calculate direction from camera to target
            Vector3 directionToTarget = targetWorldPos - currentCameraWorldPos;
            float distanceToTarget = directionToTarget.Length();

            // Move camera closer to target
            Vector3 desiredWorldPos;
            if (distanceToTarget > 0.8f)
            {
                directionToTarget.Normalize();
                desiredWorldPos = targetWorldPos - (directionToTarget * 0.8f);
            }
            else
            {
                // Already close enough, don't move
                desiredWorldPos = currentCameraWorldPos;
            }

            // Set position - handle parented camera
            if (Entity.Transform.Parent != null)
            {
                Entity.Transform.Position = Entity.Transform.Parent.WorldToLocal_HS(desiredWorldPos);
            }
            else
            {
                Entity.Transform.Position = desiredWorldPos;
            }

            // Look at the target's actual world position
            Entity.Transform.LookAt_HS(targetWorldPos);

            // Update yaw/pitch for smooth control
            Reset();

            Logger.Info($"Dev camera focused on {target.Name} at world pos: {targetWorldPos}");
        }

        public void ToggleDevCamera()
        {
            _isActive = !_isActive;
            DevCameraActive = _isActive;

            if (_isActive)
            {
                // Restore dev camera state if we have it
                if (_hasDevCameraState)
                {
                    Entity.Transform.Position = _lastDevCameraPosition;
                    Entity.Transform.Rotation = _lastDevCameraRotation;
                    Reset();
                }

                // Disable main camera only (keep player controller active)
                if (_mainCameraComponent != null)
                {
                    _mainCameraComponent.Enabled = false;
                }

                // Enable our dev camera
                _devCameraComponent.Enabled = true;

                // REMOVE the mouse lock lines - keep mouse free!

                Logger.Info("Dev camera enabled - WASD to move, RIGHT-CLICK + drag to look, scroll wheel for speed, F to focus");
            }
            else
            {
                // Save our current dev camera state
                _lastDevCameraPosition = Entity.Transform.Position;
                _lastDevCameraRotation = Entity.Transform.Rotation;
                _hasDevCameraState = true;

                // Disable our dev camera
                _devCameraComponent.Enabled = false;

                // Re-enable main camera
                if (_mainCameraComponent != null)
                {
                    _mainCameraComponent.Enabled = true;
                }

                // REMOVE any mouse state restoration - let player controller handle it

                Logger.Info("Dev camera disabled - returned to player camera");
            }
        }
    }
}