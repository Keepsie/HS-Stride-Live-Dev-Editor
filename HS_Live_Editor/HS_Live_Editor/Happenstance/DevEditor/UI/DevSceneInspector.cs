// HS Live Dev Editor (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Core;
using Happenstance.SE.Logger.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Happenstance.SE.DevEditor.Core;

namespace Happenstance.SE.DevEditor.UI
{
    public class DevSceneInspector : HSSyncScript
    {
        // References
        private DevEditorManager _editorManager;
        private Canvas _inspectorCanvas;

        // Font reference
        private SpriteFont _defaultFont;

        // UI Elements - Position
        private EditText _posX, _posY, _posZ;
        private Button _posXPlus, _posXMinus, _posYPlus, _posYMinus, _posZPlus, _posZMinus;
        private Button _posCopyAll, _posXCopy, _posYCopy, _posZCopy;

        // UI Elements - Rotation
        private EditText _rotX, _rotY, _rotZ;
        private Button _rotXPlus, _rotXMinus, _rotYPlus, _rotYMinus, _rotZPlus, _rotZMinus;
        private Button _rotCopyAll, _rotXCopy, _rotYCopy, _rotZCopy;

        // UI Elements - Scale
        private EditText _scaleX, _scaleY, _scaleZ;
        private Button _scaleXPlus, _scaleXMinus, _scaleYPlus, _scaleYMinus, _scaleZPlus, _scaleZMinus;
        private Button _scaleCopyAll, _scaleXCopy, _scaleYCopy, _scaleZCopy;

        // Inspector title
        private TextBlock _inspectorTitle;

        // Component list
        private StackPanel _componentsList;
        private ScrollViewer _scrollViewer;


        // Flag to prevent circular updates
        private bool _updatingUI = false;

        // Step values
        private const float POSITION_STEP = 0.1f;
        private const float ROTATION_STEP = 1.0f;
        private const float SCALE_STEP = 0.1f;

        protected override void OnStart()
        {
            // Find the editor manager
            _editorManager = Entity.Scene.FindAllComponents_HS<DevEditorManager>().FirstOrDefault();
            if (_editorManager == null)
            {
                Logger.Error("DevEditorManager not found - inspector will not function");
                return;
            }

            // Subscribe to events
            _editorManager.OnEntitySelected += OnEntitySelected;
            _editorManager.OnTransformChanged += OnTransformChanged;

            // Load default font
            try
            {
                _defaultFont = Content.Load<SpriteFont>("StrideDefaultFont");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load sprite font: {ex.Message}");
            }

            // Get UI component
            var uiComponent = Entity.Get<UIComponent>();
            if (uiComponent == null || uiComponent.Page == null)
            {
                Logger.Error("UI component or page missing");
                return;
            }
            
            // Find the inspector canvas
            _inspectorCanvas = Entity.GetUIElement_HS<Canvas>("entity_inspector_holder");
            if (_inspectorCanvas == null)
            {
                Logger.Error("entity_inspector_holder canvas not found");
                return;
            }

            // Find inspector title
            _inspectorTitle = _inspectorCanvas.GetUIElement_HS<TextBlock>("Entity Inspector");
            if (_inspectorTitle != null && _defaultFont != null)
            {
                _inspectorTitle.Font = _defaultFont;
            }
            
            _scrollViewer = _inspectorCanvas.GetUIElement_HS<ScrollViewer>("components_scroll");

            if (_scrollViewer == null)
            {
                Logger.Warning("Components ScrollViewer not found - scrolling will not work correctly");
            }
            else
            {
                // Configure ScrollViewer
                _scrollViewer.ScrollMode = ScrollingMode.Vertical;
                _scrollViewer.TouchScrollingEnabled = true;

                // Make scrollbar always visible
                var alwaysVisibleColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
                _scrollViewer.ScrollBarColor = alwaysVisibleColor;
            }

            // Find components list
            _componentsList = _inspectorCanvas.GetUIElement_HS<StackPanel>("entity_stackpanel");
            
            

            // Initialize UI elements
            InitializePositionElements();
            InitializeRotationElements();
            InitializeScaleElements();

            // Initial UI update
            UpdateInspectorUI(null);
        }

        protected override void OnUpdate()
        {
            // Handle mouse wheel scrolling for components list
            if (_scrollViewer != null && IsEnabled)
            {
                var mouseWheel = Input.MouseWheelDelta;
                if (mouseWheel != 0 && _scrollViewer.MouseOverState != MouseOverState.MouseOverNone)
                {
                    var scrollAmount = new Vector3(0, -mouseWheel * 50, 0); // Adjust multiplier as needed
                    _scrollViewer.ScrollOf(scrollAmount);
                }

                // Force scroll bar to stay visible
                _scrollViewer.ScrollBarColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
            }
        }

        private void InitializePositionElements()
        {
            // Get position inputs with error checking
            _posX = SafeGetUIElement<EditText>("pos_x_edit");
            _posY = SafeGetUIElement<EditText>("pos_y_edit");
            _posZ = SafeGetUIElement<EditText>("pos_z_edit");

            // Get position buttons with error checking
            _posXPlus = SafeGetUIElement<Button>("pos_x_plus_button");
            _posXMinus = SafeGetUIElement<Button>("pos_x_neg_button");
            _posYPlus = SafeGetUIElement<Button>("pos_y_plus_button");
            _posYMinus = SafeGetUIElement<Button>("pos_y_neg_button");
            _posZPlus = SafeGetUIElement<Button>("pos_z_plus_button");
            _posZMinus = SafeGetUIElement<Button>("pos_z_neg_button");

            // Get copy buttons with error checking
            _posCopyAll = SafeGetUIElement<Button>("pos_copy_all");
            _posXCopy = SafeGetUIElement<Button>("pos_x_copy_button");
            _posYCopy = SafeGetUIElement<Button>("pos_y_copy_button");
            _posZCopy = SafeGetUIElement<Button>("pos_z_copy_button");

            // Setup input event handlers (only if elements exist)
            if (_posX != null) _posX.TextChanged += (s, e) => OnPositionXChanged();
            if (_posY != null) _posY.TextChanged += (s, e) => OnPositionYChanged();
            if (_posZ != null) _posZ.TextChanged += (s, e) => OnPositionZChanged();

            // Setup button handlers (only if elements exist)
            if (_posXPlus != null) _posXPlus.Click += (s, e) => AdjustPositionX(POSITION_STEP);
            if (_posXMinus != null) _posXMinus.Click += (s, e) => AdjustPositionX(-POSITION_STEP);
            if (_posYPlus != null) _posYPlus.Click += (s, e) => AdjustPositionY(POSITION_STEP);
            if (_posYMinus != null) _posYMinus.Click += (s, e) => AdjustPositionY(-POSITION_STEP);
            if (_posZPlus != null) _posZPlus.Click += (s, e) => AdjustPositionZ(POSITION_STEP);
            if (_posZMinus != null) _posZMinus.Click += (s, e) => AdjustPositionZ(-POSITION_STEP);

            // Setup copy button handlers (only if elements exist)
            if (_posCopyAll != null) _posCopyAll.Click += (s, e) => CopyPositionToClipboard();
            if (_posXCopy != null) _posXCopy.Click += (s, e) => CopyPositionXToClipboard();
            if (_posYCopy != null) _posYCopy.Click += (s, e) => CopyPositionYToClipboard();
            if (_posZCopy != null) _posZCopy.Click += (s, e) => CopyPositionZToClipboard();

            // Summary log
            LogElementSummary("Position", new Dictionary<string, UIElement>
            {
                {"pos_x_edit", _posX},
                {"pos_y_edit", _posY},
                {"pos_z_edit", _posZ},
                {"pos_x_plus_button", _posXPlus},
                {"pos_x_neg_button", _posXMinus},
                {"pos_y_plus_button", _posYPlus},
                {"pos_y_neg_button", _posYMinus},
                {"pos_z_plus_button", _posZPlus},
                {"pos_z_neg_button", _posZMinus},
                {"pos_copy_all", _posCopyAll},
                {"pos_x_copy_button", _posXCopy},
                {"pos_y_copy_button", _posYCopy},
                {"pos_z_copy_button", _posZCopy}
            });
        }

        private void InitializeRotationElements()
        {
            // Get rotation inputs with error checking
            _rotX = SafeGetUIElement<EditText>("rot_x_edit");
            _rotY = SafeGetUIElement<EditText>("rot_y_edit");
            _rotZ = SafeGetUIElement<EditText>("rot_z_edit");

            // Get rotation buttons with error checking
            _rotXPlus = SafeGetUIElement<Button>("rot_x_plus_button");
            _rotXMinus = SafeGetUIElement<Button>("rot_x_neg_button");
            _rotYPlus = SafeGetUIElement<Button>("rot_y_plus_button");
            _rotYMinus = SafeGetUIElement<Button>("rot_y_neg_button");
            _rotZPlus = SafeGetUIElement<Button>("rot_z_plus_button");
            _rotZMinus = SafeGetUIElement<Button>("rot_z_neg_button");

            // Get copy buttons with error checking
            _rotCopyAll = SafeGetUIElement<Button>("rot_copy_all");
            _rotXCopy = SafeGetUIElement<Button>("rot_x_copy_button");
            _rotYCopy = SafeGetUIElement<Button>("rot_y_copy_button");
            _rotZCopy = SafeGetUIElement<Button>("rot_z_copy_button");

            // Setup input event handlers (only if elements exist)
            if (_rotX != null) _rotX.TextChanged += (s, e) => OnRotationXChanged();
            if (_rotY != null) _rotY.TextChanged += (s, e) => OnRotationYChanged();
            if (_rotZ != null) _rotZ.TextChanged += (s, e) => OnRotationZChanged();

            // Setup button handlers (only if elements exist)
            if (_rotXPlus != null) _rotXPlus.Click += (s, e) => AdjustRotationX(ROTATION_STEP);
            if (_rotXMinus != null) _rotXMinus.Click += (s, e) => AdjustRotationX(-ROTATION_STEP);
            if (_rotYPlus != null) _rotYPlus.Click += (s, e) => AdjustRotationY(ROTATION_STEP);
            if (_rotYMinus != null) _rotYMinus.Click += (s, e) => AdjustRotationY(-ROTATION_STEP);
            if (_rotZPlus != null) _rotZPlus.Click += (s, e) => AdjustRotationZ(ROTATION_STEP);
            if (_rotZMinus != null) _rotZMinus.Click += (s, e) => AdjustRotationZ(-ROTATION_STEP);

            // Setup copy button handlers (only if elements exist)
            if (_rotCopyAll != null) _rotCopyAll.Click += (s, e) => CopyRotationToClipboard();
            if (_rotXCopy != null) _rotXCopy.Click += (s, e) => CopyRotationXToClipboard();
            if (_rotYCopy != null) _rotYCopy.Click += (s, e) => CopyRotationYToClipboard();
            if (_rotZCopy != null) _rotZCopy.Click += (s, e) => CopyRotationZToClipboard();

            // Summary log
            LogElementSummary("Rotation", new Dictionary<string, UIElement>
            {
                {"rot_x_edit", _rotX},
                {"rot_y_edit", _rotY},
                {"rot_z_edit", _rotZ},
                {"rot_x_plus_button", _rotXPlus},
                {"rot_x_neg_button", _rotXMinus},
                {"rot_y_plus_button", _rotYPlus},
                {"rot_y_neg_button", _rotYMinus},
                {"rot_z_plus_button", _rotZPlus},
                {"rot_z_neg_button", _rotZMinus},
                {"rot_copy_all", _rotCopyAll},
                {"rot_x_copy_button", _rotXCopy},
                {"rot_y_copy_button", _rotYCopy},
                {"rot_z_copy_button", _rotZCopy}
            });
        }

        private void InitializeScaleElements()
        {
            // Get scale inputs with error checking
            _scaleX = SafeGetUIElement<EditText>("scale_x_edit");
            _scaleY = SafeGetUIElement<EditText>("scale_y_edit");
            _scaleZ = SafeGetUIElement<EditText>("scale_z_edit");

            // Get scale buttons with error checking
            _scaleXPlus = SafeGetUIElement<Button>("scale_x_plus_button");
            _scaleXMinus = SafeGetUIElement<Button>("scale_x_neg_button");
            _scaleYPlus = SafeGetUIElement<Button>("scale_y_plus_button");
            _scaleYMinus = SafeGetUIElement<Button>("scale_y_neg_button");
            _scaleZPlus = SafeGetUIElement<Button>("scale_z_plus_button");
            _scaleZMinus = SafeGetUIElement<Button>("scale_z_neg_button");

            // Get copy buttons with error checking
            _scaleCopyAll = SafeGetUIElement<Button>("scale_copy_all");
            _scaleXCopy = SafeGetUIElement<Button>("scale_x_copy_button");
            _scaleYCopy = SafeGetUIElement<Button>("scale_y_copy_button");
            _scaleZCopy = SafeGetUIElement<Button>("scale_z_copy_button");

            // Setup input event handlers (only if elements exist)
            if (_scaleX != null) _scaleX.TextChanged += (s, e) => OnScaleXChanged();
            if (_scaleY != null) _scaleY.TextChanged += (s, e) => OnScaleYChanged();
            if (_scaleZ != null) _scaleZ.TextChanged += (s, e) => OnScaleZChanged();

            // Setup button handlers (only if elements exist)
            if (_scaleXPlus != null) _scaleXPlus.Click += (s, e) => AdjustScaleX(SCALE_STEP);
            if (_scaleXMinus != null) _scaleXMinus.Click += (s, e) => AdjustScaleX(-SCALE_STEP);
            if (_scaleYPlus != null) _scaleYPlus.Click += (s, e) => AdjustScaleY(SCALE_STEP);
            if (_scaleYMinus != null) _scaleYMinus.Click += (s, e) => AdjustScaleY(-SCALE_STEP);
            if (_scaleZPlus != null) _scaleZPlus.Click += (s, e) => AdjustScaleZ(SCALE_STEP);
            if (_scaleZMinus != null) _scaleZMinus.Click += (s, e) => AdjustScaleZ(-SCALE_STEP);

            // Setup copy button handlers (only if elements exist)
            if (_scaleCopyAll != null) _scaleCopyAll.Click += (s, e) => CopyScaleToClipboard();
            if (_scaleXCopy != null) _scaleXCopy.Click += (s, e) => CopyScaleXToClipboard();
            if (_scaleYCopy != null) _scaleYCopy.Click += (s, e) => CopyScaleYToClipboard();
            if (_scaleZCopy != null) _scaleZCopy.Click += (s, e) => CopyScaleZToClipboard();

            // Summary log
            LogElementSummary("Scale", new Dictionary<string, UIElement>
            {
                {"scale_x_edit", _scaleX},
                {"scale_y_edit", _scaleY},
                {"scale_z_edit", _scaleZ},
                {"scale_x_plus_button", _scaleXPlus},
                {"scale_x_neg_button", _scaleXMinus},
                {"scale_y_plus_button", _scaleYPlus},
                {"scale_y_neg_button", _scaleYMinus},
                {"scale_z_plus_button", _scaleZPlus},
                {"scale_z_neg_button", _scaleZMinus},
                {"scale_copy_all", _scaleCopyAll},
                {"scale_x_copy_button", _scaleXCopy},
                {"scale_y_copy_button", _scaleYCopy},
                {"scale_z_copy_button", _scaleZCopy}
            });
        }


        // Helper method for safely getting UI elements with logging
        private T SafeGetUIElement<T>(string name) where T : UIElement
        {
            var element = _inspectorCanvas.GetUIElement_HS<T>(name);
            if (element == null)
            {
                Logger.Warning($"UI element not found: {name} (type: {typeof(T).Name})");
            }
            return element;
        }

        // Helper method to log a summary of found/missing elements
        private void LogElementSummary(string sectionName, Dictionary<string, UIElement> elements)
        {
            int foundCount = elements.Count(x => x.Value != null);
            int totalCount = elements.Count;

            if (foundCount == totalCount)
            {
                Logger.Info($"{sectionName} section: All {totalCount} UI elements found");
            }
            else
            {
                Logger.Warning($"{sectionName} section: Only {foundCount}/{totalCount} UI elements found");
                foreach (var entry in elements.Where(x => x.Value == null))
                {
                    Logger.Warning($"  Missing: {entry.Key}");
                }
            }
        }

        private void OnEntitySelected(Entity entity)
        {
            UpdateInspectorUI(entity);
        }

        private void OnTransformChanged(Vector3 vector)
        {
            // Update the UI when transform changes from keyboard input
            UpdateInspectorUI(_editorManager.SelectedEntity);
        }

        private void UpdateInspectorUI(Entity entity)
        {
            // Flag to prevent circular updates
            _updatingUI = true;

            // Enable/disable buttons and fields based on selection
            bool hasSelection = entity != null;
            SetControlsEnabled(hasSelection);

            if (entity == null)
            {
                ClearInputs();
                _updatingUI = false;
                return;
            }

            // Get transform values
            Vector3 position = entity.Transform.Position;
            Vector3 rotation = entity.Transform.GetEulerAngles_HS();
            Vector3 scale = entity.Transform.Scale;

            // Update position inputs
            if (_posX != null) _posX.Text = position.X.ToString("0.00");
            if (_posY != null) _posY.Text = position.Y.ToString("0.00");
            if (_posZ != null) _posZ.Text = position.Z.ToString("0.00");

            // Update rotation inputs
            if (_rotX != null) _rotX.Text = rotation.X.ToString("0.00");
            if (_rotY != null) _rotY.Text = rotation.Y.ToString("0.00");
            if (_rotZ != null) _rotZ.Text = rotation.Z.ToString("0.00");

            // Update scale inputs
            if (_scaleX != null) _scaleX.Text = scale.X.ToString("0.00");
            if (_scaleY != null) _scaleY.Text = scale.Y.ToString("0.00");
            if (_scaleZ != null) _scaleZ.Text = scale.Z.ToString("0.00");

            // Update components list
            UpdateComponentsList(entity);

            _updatingUI = false;
        }

        private void UpdateComponentsList(Entity entity)
        {
            if (_componentsList == null || entity == null) return;

            _componentsList.Children.Clear();

            foreach (var component in entity.Components)
            {
                var componentText = new TextBlock
                {
                    Text = component.GetType().Name,
                    TextSize = 12,
                    TextColor = new Color(0.9f, 0.9f, 0.9f, 1.0f),
                    Font = _defaultFont,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5, 2, 5, 2)
                };

                _componentsList.Children.Add(componentText);
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            // Position controls
            SetControlEnabled(_posX, enabled);
            SetControlEnabled(_posY, enabled);
            SetControlEnabled(_posZ, enabled);
            SetControlEnabled(_posXPlus, enabled);
            SetControlEnabled(_posXMinus, enabled);
            SetControlEnabled(_posYPlus, enabled);
            SetControlEnabled(_posYMinus, enabled);
            SetControlEnabled(_posZPlus, enabled);
            SetControlEnabled(_posZMinus, enabled);
            SetControlEnabled(_posCopyAll, enabled);
            SetControlEnabled(_posXCopy, enabled);
            SetControlEnabled(_posYCopy, enabled);
            SetControlEnabled(_posZCopy, enabled);

            // Rotation controls
            SetControlEnabled(_rotX, enabled);
            SetControlEnabled(_rotY, enabled);
            SetControlEnabled(_rotZ, enabled);
            SetControlEnabled(_rotXPlus, enabled);
            SetControlEnabled(_rotXMinus, enabled);
            SetControlEnabled(_rotYPlus, enabled);
            SetControlEnabled(_rotYMinus, enabled);
            SetControlEnabled(_rotZPlus, enabled);
            SetControlEnabled(_rotZMinus, enabled);
            SetControlEnabled(_rotCopyAll, enabled);
            SetControlEnabled(_rotXCopy, enabled);
            SetControlEnabled(_rotYCopy, enabled);
            SetControlEnabled(_rotZCopy, enabled);

            // Scale controls
            SetControlEnabled(_scaleX, enabled);
            SetControlEnabled(_scaleY, enabled);
            SetControlEnabled(_scaleZ, enabled);
            SetControlEnabled(_scaleXPlus, enabled);
            SetControlEnabled(_scaleXMinus, enabled);
            SetControlEnabled(_scaleYPlus, enabled);
            SetControlEnabled(_scaleYMinus, enabled);
            SetControlEnabled(_scaleZPlus, enabled);
            SetControlEnabled(_scaleZMinus, enabled);
            SetControlEnabled(_scaleCopyAll, enabled);
            SetControlEnabled(_scaleXCopy, enabled);
            SetControlEnabled(_scaleYCopy, enabled);
            SetControlEnabled(_scaleZCopy, enabled);
        }

        private void SetControlEnabled(UIElement control, bool enabled)
        {
            if (control != null)
            {
                control.IsEnabled = enabled;
                control.Opacity = enabled ? 1.0f : 0.5f;
            }
        }

        private void ClearInputs()
        {
            // Position inputs
            if (_posX != null) _posX.Text = "0.00";
            if (_posY != null) _posY.Text = "0.00";
            if (_posZ != null) _posZ.Text = "0.00";

            // Rotation inputs
            if (_rotX != null) _rotX.Text = "0.00";
            if (_rotY != null) _rotY.Text = "0.00";
            if (_rotZ != null) _rotZ.Text = "0.00";

            // Scale inputs
            if (_scaleX != null) _scaleX.Text = "0.00";
            if (_scaleY != null) _scaleY.Text = "0.00";
            if (_scaleZ != null) _scaleZ.Text = "0.00";
        }

        #region Value Change Handlers

        // Position change handlers
        private void OnPositionXChanged()
        {
            if (_updatingUI || _editorManager.SelectedEntity == null || _posX == null) return;

            if (float.TryParse(_posX.Text, out float value))
            {
                _editorManager.SetPositionX(value);
            }
        }

        private void OnPositionYChanged()
        {
            if (_updatingUI || _editorManager.SelectedEntity == null || _posY == null) return;

            if (float.TryParse(_posY.Text, out float value))
            {
                _editorManager.SetPositionY(value);
            }
        }

        private void OnPositionZChanged()
        {
            if (_updatingUI || _editorManager.SelectedEntity == null || _posZ == null) return;

            if (float.TryParse(_posZ.Text, out float value))
            {
                _editorManager.SetPositionZ(value);
            }
        }

        // Rotation change handlers
        private void OnRotationXChanged()
        {
            if (_updatingUI || _editorManager.SelectedEntity == null || _rotX == null) return;

            if (float.TryParse(_rotX.Text, out float value))
            {
                _editorManager.SetRotationX(value);
            }
        }

        private void OnRotationYChanged()
        {
            if (_updatingUI || _editorManager.SelectedEntity == null || _rotY == null) return;

            if (float.TryParse(_rotY.Text, out float value))
            {
                _editorManager.SetRotationY(value);
            }
        }

        private void OnRotationZChanged()
        {
            if (_updatingUI || _editorManager.SelectedEntity == null || _rotZ == null) return;

            if (float.TryParse(_rotZ.Text, out float value))
            {
                _editorManager.SetRotationZ(value);
            }
        }

        // Scale change handlers
        private void OnScaleXChanged()
        {
            if (_updatingUI || _editorManager.SelectedEntity == null || _scaleX == null) return;

            if (float.TryParse(_scaleX.Text, out float value))
            {
                _editorManager.SetScaleX(value);
            }
        }

        private void OnScaleYChanged()
        {
            if (_updatingUI || _editorManager.SelectedEntity == null || _scaleY == null) return;

            if (float.TryParse(_scaleY.Text, out float value))
            {
                _editorManager.SetScaleY(value);
            }
        }

        private void OnScaleZChanged()
        {
            if (_updatingUI || _editorManager.SelectedEntity == null || _scaleZ == null) return;

            if (float.TryParse(_scaleZ.Text, out float value))
            {
                _editorManager.SetScaleZ(value);
            }
        }

        #endregion

        #region Button Adjustment Handlers

        // Position adjustments
        private void AdjustPositionX(float delta)
        {
            if (_editorManager.SelectedEntity == null) return;

            float currentValue = _editorManager.SelectedEntity.Transform.Position.X;
            _editorManager.SetPositionX(currentValue + delta);
        }

        private void AdjustPositionY(float delta)
        {
            if (_editorManager.SelectedEntity == null) return;

            float currentValue = _editorManager.SelectedEntity.Transform.Position.Y;
            _editorManager.SetPositionY(currentValue + delta);
        }

        private void AdjustPositionZ(float delta)
        {
            if (_editorManager.SelectedEntity == null) return;

            float currentValue = _editorManager.SelectedEntity.Transform.Position.Z;
            _editorManager.SetPositionZ(currentValue + delta);
        }

        // Rotation adjustments
        private void AdjustRotationX(float delta)
        {
            if (_editorManager.SelectedEntity == null) return;

            Vector3 euler = _editorManager.SelectedEntity.Transform.GetEulerAngles_HS();
            _editorManager.SetRotationX(euler.X + delta);
        }

        private void AdjustRotationY(float delta)
        {
            if (_editorManager.SelectedEntity == null) return;

            Vector3 euler = _editorManager.SelectedEntity.Transform.GetEulerAngles_HS();
            _editorManager.SetRotationY(euler.Y + delta);
        }

        private void AdjustRotationZ(float delta)
        {
            if (_editorManager.SelectedEntity == null) return;

            Vector3 euler = _editorManager.SelectedEntity.Transform.GetEulerAngles_HS();
            _editorManager.SetRotationZ(euler.Z + delta);
        }

        // Scale adjustments
        private void AdjustScaleX(float delta)
        {
            if (_editorManager.SelectedEntity == null) return;

            float currentValue = _editorManager.SelectedEntity.Transform.Scale.X;
            _editorManager.SetScaleX(currentValue + delta);
        }

        private void AdjustScaleY(float delta)
        {
            if (_editorManager.SelectedEntity == null) return;

            float currentValue = _editorManager.SelectedEntity.Transform.Scale.Y;
            _editorManager.SetScaleY(currentValue + delta);
        }

        private void AdjustScaleZ(float delta)
        {
            if (_editorManager.SelectedEntity == null) return;

            float currentValue = _editorManager.SelectedEntity.Transform.Scale.Z;
            _editorManager.SetScaleZ(currentValue + delta);
        }

        #endregion

        #region Copy Functionality

        private void CopyPositionToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            Vector3 position = _editorManager.SelectedEntity.Transform.Position;
            string posText = $"Position({position.X:0.00}, {position.Y:0.00}, {position.Z:0.00})";

            try
            {
                CopyToClipboard(posText);
                Logger.Info("Position copied to clipboard successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Position value: {posText}");
            }
        }

        private void CopyPositionXToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            float value = _editorManager.SelectedEntity.Transform.Position.X;
            string valueText = value.ToString("F2");
    
            try
            {
                CopyToClipboard(valueText);
                Logger.Info($"Position X copied: {valueText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Position X value: {valueText}");
            }
        }

        private void CopyPositionYToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            float value = _editorManager.SelectedEntity.Transform.Position.Y;
            string valueText = value.ToString("F2");
    
            try
            {
                CopyToClipboard(valueText);
                Logger.Info($"Position Y copied: {valueText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Position Y value: {valueText}");
            }
        }

        private void CopyPositionZToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            float value = _editorManager.SelectedEntity.Transform.Position.Z;
            string valueText = value.ToString("F2");
    
            try
            {
                CopyToClipboard(valueText);
                Logger.Info($"Position Z copied: {valueText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Position Z value: {valueText}");
            }
        }

        private void CopyRotationToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            Vector3 euler = _editorManager.SelectedEntity.Transform.GetEulerAngles_HS();
            string rotText = $"Rotation({euler.X:0.00}, {euler.Y:0.00}, {euler.Z:0.00})";

            try
            {
                CopyToClipboard(rotText);
                Logger.Info("Rotation copied to clipboard successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Rotation value: {rotText}");
            }
        }

        private void CopyRotationXToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            Vector3 euler = _editorManager.SelectedEntity.Transform.GetEulerAngles_HS();
            string valueText = euler.X.ToString("F2");
    
            try
            {
                CopyToClipboard(valueText);
                Logger.Info($"Rotation X copied: {valueText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Rotation X value: {valueText}");
            }
        }

        private void CopyRotationYToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            Vector3 euler = _editorManager.SelectedEntity.Transform.GetEulerAngles_HS();
            string valueText = euler.Y.ToString("F2");
    
            try
            {
                CopyToClipboard(valueText);
                Logger.Info($"Rotation Y copied: {valueText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Rotation Y value: {valueText}");
            }
        }

        private void CopyRotationZToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            Vector3 euler = _editorManager.SelectedEntity.Transform.GetEulerAngles_HS();
            string valueText = euler.Z.ToString("F2");
    
            try
            {
                CopyToClipboard(valueText);
                Logger.Info($"Rotation Z copied: {valueText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Rotation Z value: {valueText}");
            }
        }

        private void CopyScaleToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            Vector3 scale = _editorManager.SelectedEntity.Transform.Scale;
            string valueText = $"Scale({scale.X:0.00}, {scale.Y:0.00}, {scale.Z:0.00})";

            try
            {
                CopyToClipboard(valueText);
                Logger.Info("Scale copied to clipboard successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Scale value: {valueText}");
            }
        }

        private void CopyScaleXToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            float value = _editorManager.SelectedEntity.Transform.Scale.X;
            string valueText = value.ToString("F2");
    
            try
            {
                CopyToClipboard(valueText);
                Logger.Info($"Scale X copied: {valueText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Scale X value: {valueText}");
            }
        }

        private void CopyScaleYToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            float value = _editorManager.SelectedEntity.Transform.Scale.Y;
            string valueText = value.ToString("F2");
    
            try
            {
                CopyToClipboard(valueText);
                Logger.Info($"Scale Y copied: {valueText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Scale Y value: {valueText}");
            }
        }

        private void CopyScaleZToClipboard()
        {
            if (_editorManager.SelectedEntity == null) return;

            float value = _editorManager.SelectedEntity.Transform.Scale.Z;
            string valueText = value.ToString("F2");
    
            try
            {
                CopyToClipboard(valueText);
                Logger.Info($"Scale Z copied: {valueText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                Logger.Info($"Scale Z value: {valueText}");
            }
        }

        private void CopyToClipboard(string text)
        {
            // Create a thread in STA mode for clipboard operations
            var thread = new Thread(() =>
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(text);
                    Logger.Info("Copied to clipboard successfully");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to copy to clipboard: {ex.Message}");
                }
            });

            // Set the thread to STA mode
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(); // Wait for the thread to complete
        }

        #endregion

        protected override void OnDestroy()
        {
            // Unsubscribe from events
            if (_editorManager != null)
            {
                _editorManager.OnEntitySelected -= OnEntitySelected;
                _editorManager.OnTransformChanged -= OnTransformChanged;
            }

            // Unsubscribe from TextChanged events
            UnsubscribeTextChangedEvents();

            // Unsubscribe from button click events
            UnsubscribeButtonClickEvents();
        }

        private void UnsubscribeTextChangedEvents()
        {
            // Position inputs
            if (_posX != null) _posX.TextChanged -= (s, e) => OnPositionXChanged();
            if (_posY != null) _posY.TextChanged -= (s, e) => OnPositionYChanged();
            if (_posZ != null) _posZ.TextChanged -= (s, e) => OnPositionZChanged();

            // Rotation inputs
            if (_rotX != null) _rotX.TextChanged -= (s, e) => OnRotationXChanged();
            if (_rotY != null) _rotY.TextChanged -= (s, e) => OnRotationYChanged();
            if (_rotZ != null) _rotZ.TextChanged -= (s, e) => OnRotationZChanged();

            // Scale inputs
            if (_scaleX != null) _scaleX.TextChanged -= (s, e) => OnScaleXChanged();
            if (_scaleY != null) _scaleY.TextChanged -= (s, e) => OnScaleYChanged();
            if (_scaleZ != null) _scaleZ.TextChanged -= (s, e) => OnScaleZChanged();
        }

        private void UnsubscribeButtonClickEvents()
        {
            // Position buttons
            if (_posXPlus != null) _posXPlus.Click -= (s, e) => AdjustPositionX(POSITION_STEP);
            if (_posXMinus != null) _posXMinus.Click -= (s, e) => AdjustPositionX(-POSITION_STEP);
            if (_posYPlus != null) _posYPlus.Click -= (s, e) => AdjustPositionY(POSITION_STEP);
            if (_posYMinus != null) _posYMinus.Click -= (s, e) => AdjustPositionY(-POSITION_STEP);
            if (_posZPlus != null) _posZPlus.Click -= (s, e) => AdjustPositionZ(POSITION_STEP);
            if (_posZMinus != null) _posZMinus.Click -= (s, e) => AdjustPositionZ(-POSITION_STEP);

            // Position copy buttons
            if (_posCopyAll != null) _posCopyAll.Click -= (s, e) => CopyPositionToClipboard();
            if (_posXCopy != null) _posXCopy.Click -= (s, e) => CopyPositionXToClipboard();
            if (_posYCopy != null) _posYCopy.Click -= (s, e) => CopyPositionYToClipboard();
            if (_posZCopy != null) _posZCopy.Click -= (s, e) => CopyPositionZToClipboard();

            // Rotation buttons
            if (_rotXPlus != null) _rotXPlus.Click -= (s, e) => AdjustRotationX(ROTATION_STEP);
            if (_rotXMinus != null) _rotXMinus.Click -= (s, e) => AdjustRotationX(-ROTATION_STEP);
            if (_rotYPlus != null) _rotYPlus.Click -= (s, e) => AdjustRotationY(ROTATION_STEP);
            if (_rotYMinus != null) _rotYMinus.Click -= (s, e) => AdjustRotationY(-ROTATION_STEP);
            if (_rotZPlus != null) _rotZPlus.Click -= (s, e) => AdjustRotationZ(ROTATION_STEP);
            if (_rotZMinus != null) _rotZMinus.Click -= (s, e) => AdjustRotationZ(-ROTATION_STEP);

            // Rotation copy buttons
            if (_rotCopyAll != null) _rotCopyAll.Click -= (s, e) => CopyRotationToClipboard();
            if (_rotXCopy != null) _rotXCopy.Click -= (s, e) => CopyRotationXToClipboard();
            if (_rotYCopy != null) _rotYCopy.Click -= (s, e) => CopyRotationYToClipboard();
            if (_rotZCopy != null) _rotZCopy.Click -= (s, e) => CopyRotationZToClipboard();

            // Scale buttons
            if (_scaleXPlus != null) _scaleXPlus.Click -= (s, e) => AdjustScaleX(SCALE_STEP);
            if (_scaleXMinus != null) _scaleXMinus.Click -= (s, e) => AdjustScaleX(-SCALE_STEP);
            if (_scaleYPlus != null) _scaleYPlus.Click -= (s, e) => AdjustScaleY(SCALE_STEP);
            if (_scaleYMinus != null) _scaleYMinus.Click -= (s, e) => AdjustScaleY(-SCALE_STEP);
            if (_scaleZPlus != null) _scaleZPlus.Click -= (s, e) => AdjustScaleZ(SCALE_STEP);
            if (_scaleZMinus != null) _scaleZMinus.Click -= (s, e) => AdjustScaleZ(-SCALE_STEP);

            // Scale copy buttons
            if (_scaleCopyAll != null) _scaleCopyAll.Click -= (s, e) => CopyScaleToClipboard();
            if (_scaleXCopy != null) _scaleXCopy.Click -= (s, e) => CopyScaleXToClipboard();
            if (_scaleYCopy != null) _scaleYCopy.Click -= (s, e) => CopyScaleYToClipboard();
            if (_scaleZCopy != null) _scaleZCopy.Click -= (s, e) => CopyScaleZToClipboard();
        }
    }

    
}