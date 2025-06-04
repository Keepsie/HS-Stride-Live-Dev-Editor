// HS Live Dev Editor (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Core;
using Happenstance.SE.Logger.Core;
using Stride.Engine;
using Stride.Graphics;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using System;
using System.Linq;
using Happenstance.SE.DevEditor.Core;

namespace Happenstance.SE.DevEditor.UI
{
    public class DevSceneHelper : HSStartupScript
    {
        // References
        private DevEditorManager _editorManager;
        private UIPage _uiPage;
        private Canvas _helperBarCanvas;
        private TextBlock _selectedText;
        private SpriteFont _defaultFont;

        public override void OnStart()
        {
            // Find DevEditorManager
            _editorManager = EntityFinder.FindAllComponents<DevEditorManager>().FirstOrDefault();
            if (_editorManager == null)
            {
                Logger.Error("DevEditorManager not found - helper will not function");
                return;
            }

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

            _uiPage = uiComponent.Page;

            // Find the helper bar canvas
            _helperBarCanvas = EntityFinder.GetUIElement<Canvas>(_uiPage, "helper_bar_holder");
            if (_helperBarCanvas == null)
            {
                Logger.Error("helper_bar_holder canvas not found");
                return;
            }

            // Find the selected text element
            _selectedText = EntityFinder.GetUIElement<TextBlock>(_helperBarCanvas, "selected_text");
            if (_selectedText == null)
            {
                Logger.Error("selected_text element not found");
                return;
            }

            // Set font
            if (_defaultFont != null)
            {
                _selectedText.Font = _defaultFont;
            }

            // Subscribe to entity selection event
            _editorManager.OnEntitySelected += OnEntitySelected;

            // Initial update
            UpdateSelectedText(_editorManager.SelectedEntity);
        }

        private void OnEntitySelected(Entity entity)
        {
            UpdateSelectedText(entity);
        }

        private void UpdateSelectedText(Entity entity)
        {
            if (_selectedText != null)
            {
                _selectedText.Text = entity != null ? "Selected: " + entity.Name : "Selected: None";
            }
        }

        public override void OnDestroy()
        {
            // Unsubscribe from events
            if (_editorManager != null)
            {
                _editorManager.OnEntitySelected -= OnEntitySelected;
            }
        }
    }
}