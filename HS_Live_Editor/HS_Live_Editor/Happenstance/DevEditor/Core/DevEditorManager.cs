// HS Live Dev Editor (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Core;
using Happenstance.SE.DevEditor.UI;
using Happenstance.SE.Logger.Core;
using Silk.NET.SDL;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Happenstance.SE.DevEditor.Core
{
    public class DevEditorManager : HSSyncScript
    {
        // State tracking
        private Entity _selectedEntity;
        private Entity _editorUIEntity;
        private bool _activateEditor;

        private EditMode _currentEditMode = EditMode.Position;
        private EditAxis _activeAxis = EditAxis.X;

        // Events
        public event Action<Entity> OnEntitySelected;
        public event Action<EditMode> OnEditModeChanged;
        public event Action<EditAxis> OnActiveAxisChanged;
        public event Action<Vector3> OnTransformChanged;

        // Properties
        public Entity SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                if (_selectedEntity != value)
                {
                    _selectedEntity = value;
                    OnEntitySelected?.Invoke(_selectedEntity);
                    Logger.Info($"Selected entity: {_selectedEntity?.Name ?? "None"}");
                }
            }
        }

        public EditMode CurrentEditMode
        {
            get => _currentEditMode;
            set
            {
                if (_currentEditMode != value)
                {
                    _currentEditMode = value;
                    OnEditModeChanged?.Invoke(_currentEditMode);
                    Logger.Info($"Edit mode changed to: {_currentEditMode}");
                }
            }
        }

        public EditAxis ActiveAxis
        {
            get => _activeAxis;
            set
            {
                if (_activeAxis != value)
                {
                    _activeAxis = value;
                    OnActiveAxisChanged?.Invoke(_activeAxis);
                    Logger.Info($"Active axis changed to: {_activeAxis}");
                }
            }
        }

        //History System
        private EditorHistory _editorHistory;
        public EditorHistory History => _editorHistory;

        //Action Batch System
        private TransformState? _actionBatchStart;
        private float _lastActionTime;
        private const float ACTION_BATCH_TIMEOUT = 0.5f;

        //Dev Cam
        private DevCamera _devCamera;

        //Dev UI Components
        private DevSceneEntities _devSceneEntities;

        /// <summary>
        /// Indicates whether the development editor is currently active and handling input.
        /// When true, the editor is processing keyboard input for entity manipulation (arrow keys, undo/redo, etc.).
        /// Other input systems should check this property to avoid conflicts with editor controls.
        /// Systems should suspend input handling for: arrow keys, Ctrl+Z/Y, R/S keys, and F11.
        /// </summary>
        public bool DevEditorActive { get; private set; }

        /// <summary>
        /// Returns true if the filter input field currently has focus (user is typing in search)
        /// </summary>
        public bool IsFilterInputActive => _devSceneEntities != null && _devSceneEntities.IsFilterInputActive;


        protected override void OnAwake()
        {
            _editorUIEntity = Entity.FindChildByName_HS("DevEditorUI");

            if (_editorUIEntity == null)
            {
                Logger.Warning("Editor UI entity not found. UI features will not be available.");
            }

            _devCamera = Entity.Scene.FindAllComponents_HS<DevCamera>().FirstOrDefault();
            _devSceneEntities = Entity.Scene.FindAllComponents_HS<DevSceneEntities>().FirstOrDefault();

            if (_devCamera == null)
            {
                Logger.Warning("Dev Camera entity not found. features will not be available.");
            }

            _editorUIEntity.EnableAll(false, true);
            _activateEditor = false;

            _editorHistory = new EditorHistory();
            _editorHistory.MaxHistorySize = 50;

        }

        protected override void OnUpdate()
        {
         
            if (Input.IsKeyPressed(Keys.F11))
            {
                ToggleEditor();
            }

            if (_activateEditor)
            {
                HandleUndoRedoInput();
                HandleExportInput();
            }

            // Handle keyboard manipulation if entity is selected
            if (_activateEditor && _selectedEntity != null)
            {
                HandleKeyboardInput();
            }

            CheckActionBatchTimeout();

        }

        private void HandleUndoRedoInput()
        {
            bool ctrlHeld = Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl);

            if (ctrlHeld)
            {
                // Ctrl+Z for undo
                if (Input.IsKeyPressed(Keys.Z))
                {
                    if (_editorHistory.UndoChange())
                    {
                        Logger.Info($"Undid: {_editorHistory.GetRedoDescription()}");
                        OnTransformChanged?.Invoke(_selectedEntity?.Transform.Position ?? Vector3.Zero);
                    }
                    else
                    {
                        Logger.Info("Nothing to undo");
                    }
                }

                // Ctrl+Y for redo (or Ctrl+Shift+Z as alternative)
                else if (Input.IsKeyPressed(Keys.Y) ||
                        (Input.IsKeyPressed(Keys.Z) && (Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift))))
                {
                    if (_editorHistory.RedoChange())
                    {
                        Logger.Info($"Redid: {_editorHistory.GetUndoDescription()}");
                        OnTransformChanged?.Invoke(_selectedEntity?.Transform.Position ?? Vector3.Zero);
                    }
                    else
                    {
                        Logger.Info("Nothing to redo");
                    }
                }
            }
        }

        private void HandleExportInput()
        {
            bool ctrlHeld = Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl);

            if (ctrlHeld && Input.IsKeyPressed(Keys.P))
            {
                ExportModifiedEntitiesToFile();
            }
        }

        private void HandleKeyboardInput()
        {
            // Determine edit mode based on held keys
            bool rotateMode = Input.IsKeyDown(Keys.R);
            bool scaleMode = Input.IsKeyDown(Keys.S);

            // Determine speed modifiers
            bool fastModifier = Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift);
            bool slowModifier = Input.IsKeyDown(Keys.LeftAlt) || Input.IsKeyDown(Keys.RightAlt);
            bool verticalModifier = Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl);

            // Base movement speed
            float baseSpeed = 0.01f; // Default movement step

            // Adjust speed based on modifiers
            float effectiveSpeed = baseSpeed;
            if (fastModifier) effectiveSpeed *= 2.3f;
            if (slowModifier) effectiveSpeed *= 0.25f;

            // Handle arrow keys for manipulation
            bool useRelativeMovement = _devCamera?.DevCameraActive == true;
            Vector3 delta = Vector3.Zero;

            if (useRelativeMovement)
            {
                // Camera-relative movement
                delta = CalculateCameraRelativeMovement(_devCamera, effectiveSpeed, rotateMode, scaleMode);
            }
            else
            {
                // Original world-space movement
                delta = CalculateWorldSpaceMovement(effectiveSpeed, rotateMode, scaleMode);
            }

            // Apply changes if there was any input
            if (delta != Vector3.Zero)
            {

                //Command history
                if (_actionBatchStart == null)
                {
                    _actionBatchStart = new TransformState(_selectedEntity);
                }
                _lastActionTime = (float)Game.UpdateTime.Total.TotalSeconds;

                //Make changes
                if (rotateMode)
                {
                    ApplyRotationDelta(delta);
                }
                else if (scaleMode)
                {
                    ApplyScaleDelta(delta);
                }
                else
                {
                    ApplyPositionDelta(delta);
                }
            }
        }

        private Vector3 CalculateCameraRelativeMovement(DevCamera devCamera, float effectiveSpeed, bool rotateMode, bool scaleMode)
        {
            // Get camera's world matrix for relative directions
            var cameraMatrix = devCamera.Entity.Transform.WorldMatrix;

            Vector3 cameraRight = (Vector3)cameraMatrix.Right;
            Vector3 cameraForward = (Vector3)cameraMatrix.Forward;
            Vector3 cameraUp = Vector3.UnitY; // Always use world up for vertical

            Vector3 delta = Vector3.Zero;
            bool verticalModifier = Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl);

            // Project camera directions onto horizontal plane (remove Y component)
            // This keeps horizontal movement truly horizontal regardless of camera angle
            Vector3 horizontalRight = new Vector3(cameraRight.X, 0, cameraRight.Z);
            Vector3 horizontalForward = new Vector3(cameraForward.X, 0, cameraForward.Z);

            // Normalize after projection (in case the vectors became very small)
            if (horizontalRight.Length() > 0.001f)
                horizontalRight.Normalize();
            else
                horizontalRight = Vector3.UnitX; // Fallback to world right if camera is looking straight up/down

            if (horizontalForward.Length() > 0.001f)
                horizontalForward.Normalize();
            else
                horizontalForward = -Vector3.UnitZ; // Fallback to world forward if camera is looking straight up/down

            // Left/Right movement - always horizontal
            if (Input.IsKeyDown(Keys.Right))
                delta += horizontalRight * effectiveSpeed;
            if (Input.IsKeyDown(Keys.Left))
                delta -= horizontalRight * effectiveSpeed;

            // Forward/Back and Up/Down movement
            if (Input.IsKeyDown(Keys.Up))
            {
                if (verticalModifier && !rotateMode && !scaleMode)
                    delta += cameraUp * effectiveSpeed;     // Ctrl+Up = true vertical up
                else
                    delta += horizontalForward * effectiveSpeed; // Up = horizontal forward (no Y drift)
            }

            if (Input.IsKeyDown(Keys.Down))
            {
                if (verticalModifier && !rotateMode && !scaleMode)
                    delta -= cameraUp * effectiveSpeed;     // Ctrl+Down = true vertical down
                else
                    delta -= horizontalForward * effectiveSpeed; // Down = horizontal backward (no Y drift)
            }

            return delta;
        }

        private Vector3 CalculateWorldSpaceMovement(float effectiveSpeed, bool rotateMode, bool scaleMode)
        {
            // Original world-space movement logic
            Vector3 delta = Vector3.Zero;
            bool verticalModifier = Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl);

            // X axis - same for all modes
            if (Input.IsKeyDown(Keys.Right)) delta.X += effectiveSpeed;
            if (Input.IsKeyDown(Keys.Left)) delta.X -= effectiveSpeed;

            // Y and Z axes depend on mode and modifiers - with your existing logic
            if (Input.IsKeyDown(Keys.Up))
            {
                if (verticalModifier && !rotateMode && !scaleMode)
                    delta.Y += effectiveSpeed; // Control+Up = Up (positive Y)
                else
                    delta.Z -= effectiveSpeed; // Up = Forward (negative Z)
            }

            if (Input.IsKeyDown(Keys.Down))
            {
                if (verticalModifier && !rotateMode && !scaleMode)
                    delta.Y -= effectiveSpeed; // Control+Down = Down (negative Y)
                else
                    delta.Z += effectiveSpeed; // Down = Backward (positive Z)
            }

            return delta;
        }

        private void CheckActionBatchTimeout()
        {
            //Batches mulitple actions in a small time frame as one complete action for easier undo/redo
            if (_actionBatchStart.HasValue && _selectedEntity != null)
            {
                float timeSinceInput = (float)Game.UpdateTime.Total.TotalSeconds - _lastActionTime;

                if (timeSinceInput > ACTION_BATCH_TIMEOUT)
                {
                    // Finalize the batch command
                    var startState = _actionBatchStart.Value;
                    var endState = new TransformState(_selectedEntity);

                    var command = new TransformCommand(_selectedEntity,
                        startState.Position, endState.Position,
                        startState.Rotation, endState.Rotation,
                        startState.Scale, endState.Scale,
                        "Keyboard Edit");

                    _editorHistory.StoreChange(command);
                    _actionBatchStart = null;

                    Logger.Debug("Finalized keyboard batch edit");
                }
            }
        }

        private void ApplyPositionDelta(Vector3 delta)
        {
            if (_selectedEntity == null) return;

            Vector3 newPosition = _selectedEntity.Transform.Position;
            newPosition.X += delta.X;
            newPosition.Y += delta.Y;
            newPosition.Z += delta.Z;

            _selectedEntity.Transform.Position = newPosition;
            OnTransformChanged?.Invoke(newPosition);
        }

        private void ApplyRotationDelta(Vector3 delta)
        {
            if (_selectedEntity == null) return;

            // Apply a rotation speed multiplier - make rotation faster
            float rotationMultiplier = 50.0f;
            delta.X *= rotationMultiplier;
            delta.Y *= rotationMultiplier;

            Vector3 euler = _selectedEntity.Transform.GetEulerAngles_HS();
            // X and Y rotation are what we typically want to adjust with arrow keys
            euler.Y += delta.X; // Left/Right changes Y rotation (yaw)
            euler.X += delta.Y; // Up/Down changes X rotation (pitch)

            _selectedEntity.Transform.SetEulerAngles_HS(euler);

            OnTransformChanged?.Invoke(euler);
        }

        private void ApplyScaleDelta(Vector3 delta)
        {
            if (_selectedEntity == null) return;

            Vector3 newScale = _selectedEntity.Transform.Scale;
            newScale.X += delta.X;
            newScale.Y += delta.Y;
            newScale.Z += delta.Z;

            _selectedEntity.Transform.Scale = newScale;
            OnTransformChanged?.Invoke(newScale);
        }


        // Direct editing methods for UI
        public void SetPositionX(float x)
        {
            if (_selectedEntity == null) return;

            var oldPos = _selectedEntity.Transform.Position;
            var newPos = new Vector3(x, oldPos.Y, oldPos.Z);
            _selectedEntity.Transform.Position = newPos;

            var command = TransformCommand.CreatePositionCommand(_selectedEntity, oldPos, newPos, "Set Position X");
            _editorHistory.StoreChange(command);

            OnTransformChanged?.Invoke(newPos);
        }

        public void SetPositionY(float y)
        {
            if (_selectedEntity == null) return;

            var oldPos = _selectedEntity.Transform.Position;
            var newPos = new Vector3(oldPos.X, y, oldPos.Z);

            _selectedEntity.Transform.Position = newPos;

            var command = TransformCommand.CreatePositionCommand(_selectedEntity, oldPos, newPos, "Set Position Y");
            _editorHistory.StoreChange(command);

            OnTransformChanged?.Invoke(newPos);
        }

        public void SetPositionZ(float z)
        {
            if (_selectedEntity == null) return;

            var oldPos = _selectedEntity.Transform.Position;
            var newPos = new Vector3(oldPos.X, oldPos.Y, z);

            _selectedEntity.Transform.Position = newPos;

            var command = TransformCommand.CreatePositionCommand(_selectedEntity, oldPos, newPos, "Set Position Z");
            _editorHistory.StoreChange(command);

            OnTransformChanged?.Invoke(newPos);
        }

        public void SetRotationX(float x)
        {
            if (_selectedEntity == null) return;

            var oldRot = _selectedEntity.Transform.Rotation;
            var eulerAngles = oldRot.ToEulerAngles_HS();
            eulerAngles.X = x;
            var newRot = HSTransform.FromEulerAngles_HS(eulerAngles);

            _selectedEntity.Transform.Rotation = newRot;

            var command = TransformCommand.CreateRotationCommand(_selectedEntity, oldRot, newRot, "Set Rotation X");
            _editorHistory.StoreChange(command);

            OnTransformChanged?.Invoke(eulerAngles);
        }

        public void SetRotationY(float y)
        {
            if (_selectedEntity == null) return;

            var oldRot = _selectedEntity.Transform.Rotation;
            var eulerAngles = oldRot.ToEulerAngles_HS();
            eulerAngles.Y = y;
            var newRot = HSTransform.FromEulerAngles_HS(eulerAngles);

            _selectedEntity.Transform.Rotation = newRot;

            var command = TransformCommand.CreateRotationCommand(_selectedEntity, oldRot, newRot, "Set Rotation Y");
            _editorHistory.StoreChange(command);

            OnTransformChanged?.Invoke(eulerAngles);
        }

        public void SetRotationZ(float z)
        {
            if (_selectedEntity == null) return;

            var oldRot = _selectedEntity.Transform.Rotation;
            var eulerAngles = oldRot.ToEulerAngles_HS();
            eulerAngles.Z = z;
            var newRot = HSTransform.FromEulerAngles_HS(eulerAngles);

            _selectedEntity.Transform.Rotation = newRot;

            var command = TransformCommand.CreateRotationCommand(_selectedEntity, oldRot, newRot, "Set Rotation Z");
            _editorHistory.StoreChange(command);

            OnTransformChanged?.Invoke(eulerAngles);
        }

        public void SetScaleX(float x)
        {
            if (_selectedEntity == null) return;

            var oldScale = _selectedEntity.Transform.Scale;
            var newScale = new Vector3(x, oldScale.Y, oldScale.Z);

            _selectedEntity.Transform.Scale = newScale;

            var command = TransformCommand.CreateScaleCommand(_selectedEntity, oldScale, newScale, "Set Scale X");
            _editorHistory.StoreChange(command);

            OnTransformChanged?.Invoke(newScale);
        }

        public void SetScaleY(float y)
        {
            if (_selectedEntity == null) return;

            var oldScale = _selectedEntity.Transform.Scale;
            var newScale = new Vector3(oldScale.X, y, oldScale.Z);

            _selectedEntity.Transform.Scale = newScale;

            var command = TransformCommand.CreateScaleCommand(_selectedEntity, oldScale, newScale, "Set Scale Y");
            _editorHistory.StoreChange(command);

            OnTransformChanged?.Invoke(newScale);
        }

        public void SetScaleZ(float z)
        {
            if (_selectedEntity == null) return;

            var oldScale = _selectedEntity.Transform.Scale;
            var newScale = new Vector3(oldScale.X, oldScale.Y, z);

            _selectedEntity.Transform.Scale = newScale;

            var command = TransformCommand.CreateScaleCommand(_selectedEntity, oldScale, newScale, "Set Scale Z");
            _editorHistory.StoreChange(command);

            OnTransformChanged?.Invoke(newScale);
        }


        public void SelectEntity(Entity entity)
        {
            SelectedEntity = entity;
        }

        public void ToggleEditor()
        {
            bool newState = !_activateEditor;

            // Toggle editor UI visibility
            if (_editorUIEntity != null)
            {
                _editorUIEntity.EnableAll(newState, true);
            }

            _activateEditor = newState;
            DevEditorActive = newState;

            // When turning off editor, also turn off dev camera if it's active
            if (!newState && _devCamera != null && _devCamera.DevCameraActive)
            {
                _devCamera.ToggleDevCamera();
                Logger.Info("Auto-disabled dev camera when editor was closed");
            }

            Logger.Info($"DevEditor {(newState ? "enabled" : "disabled")}");
        }

        public List<Entity> GetAllEntities()
        {
            return Entity.Scene.Entities.ToList();
        }

        public string FormatTransformForClipboard()
        {
            if (_selectedEntity == null)
                return string.Empty;

            var pos = _selectedEntity.Transform.Position;
            var rot = _selectedEntity.Transform.GetEulerAngles_HS();
            var scale = _selectedEntity.Transform.Scale;

            return $"// Transform values for {_selectedEntity.Name}\n" +
                   $"entity.Transform.Position = new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f);\n" +
                   $"entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(\n" +
                   $"    MathUtil.DegreesToRadians({rot.Y}f),  // Yaw (Y)\n" +
                   $"    MathUtil.DegreesToRadians({rot.X}f),  // Pitch (X)\n" +
                   $"    MathUtil.DegreesToRadians({rot.Z}f)); // Roll (Z)\n" +
                   $"entity.Transform.Scale = new Vector3({scale.X}f, {scale.Y}f, {scale.Z}f);";
        }

        //Command History helpers

        public void ConfigureHistorySize(int maxSize)
        {
            _editorHistory.MaxHistorySize = Math.Max(1, maxSize);
            Logger.Info($"History size set to {_editorHistory.MaxHistorySize}");
        }

        public void ClearHistory()
        {
            _editorHistory.ClearAll();
            Logger.Info("Editor history cleared");
        }

        public void PrintHistoryDebug()
        {
            _editorHistory.PrintHistory();
        }

        public void ExportModifiedEntitiesToFile()
        {
            try
            {
                // Get all unique entities from history
                var modifiedEntities = _editorHistory.GetAllModifiedEntities();

                if (modifiedEntities.Count == 0)
                {
                    Logger.Warning("No modified entities to export");
                    return;
                }

                // Build the export content
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var lines = new List<string>();

                lines.Add($"// DevEditor Session Export - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                lines.Add($"// Modified Entities: {modifiedEntities.Count}");
                lines.Add("");

                foreach (var entity in modifiedEntities)
                {
                    if (entity == null || entity.Scene == null) continue; // Skip destroyed entities

                    var pos = entity.Transform.Position;
                    var rot = entity.Transform.GetEulerAngles_HS();
                    var scale = entity.Transform.Scale;

                    lines.Add($"{entity.Name}:");
                    lines.Add($"  Position({pos.X:0.00}, {pos.Y:0.00}, {pos.Z:0.00})");
                    lines.Add($"  Rotation({rot.X:0.00}, {rot.Y:0.00}, {rot.Z:0.00})");
                    lines.Add($"  Scale({scale.X:0.00}, {scale.Y:0.00}, {scale.Z:0.00})");
                    lines.Add("");
                }

                // Get desktop path
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"DevEditor_Export_{timestamp}.txt";
                string filePath = Path.Combine(desktopPath, fileName);

                // Write to file
                File.WriteAllLines(filePath, lines);

                Logger.Info($"Exported {modifiedEntities.Count} entities to: {fileName}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to export entities: {ex.Message}");
            }
        }


    }

    public enum EditMode
    {
        Position,
        Rotation,
        Scale
    }

    public enum EditAxis
    {
        X,
        Y,
        Z,
        All
    }

    public struct TransformState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        
        public TransformState(Entity entity)
        {
            Position = entity.Transform.Position;
            Rotation = entity.Transform.Rotation;
            Scale = entity.Transform.Scale;
        }
    }
}