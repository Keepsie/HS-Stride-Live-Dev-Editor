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
using Happenstance.SE.DevEditor.Core;

namespace Happenstance.SE.DevEditor.UI
{
    public class DevSceneEntities : HSSyncScript
    {
        // References
        private DevEditorManager _editorManager;
        private Canvas _entityListCanvas;

        // UI elements
        private ScrollViewer _scrollViewer;
        private StackPanel _entityStackPanel;
        private EditText _filterInput;
        private Button _refreshButton;
        private Button _findButton;
        private TextBlock _selectedText;

        // Font reference
        private SpriteFont _defaultFont;

        // State tracking
        private List<Entity> _sceneEntities = new List<Entity>();
        private List<Button> _entityButtons = new List<Button>();
        private bool _filterHasFocus = false;
        private bool _filterWasCleared = false;

        protected override void OnStart()
        {
            // Find the editor manager
            _editorManager = Entity.Scene.FindAllComponents_HS<DevEditorManager>().FirstOrDefault();
            if (_editorManager == null)
            {
                Logger.Error("DevEditorManager not found - scene objects panel will not function");
                return;
            }

            // Subscribe to editor events
            _editorManager.OnEntitySelected += OnEntitySelected;

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

            // Find the entity list canvas
            _entityListCanvas = Entity.GetUIElement_HS<Canvas>("entity_list_holder");
            if (_entityListCanvas == null)
            {
                Logger.Error("entity_list_holder canvas not found");
                return;
            }

            // Find UI elements within the canvas
            _scrollViewer = _entityListCanvas.GetUIElement_HS<ScrollViewer>("entity_scrollview");
            if (_scrollViewer == null)
            {
                Logger.Error("ScrollViewer not found - entity list will not function properly");
                return;
            }

            // Configure ScrollViewer
            _scrollViewer.ScrollMode = ScrollingMode.Vertical;
            _scrollViewer.TouchScrollingEnabled = true;

            // Make scrollbar always visible
            var alwaysVisibleColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
            _scrollViewer.ScrollBarColor = alwaysVisibleColor;

            // Find the stack panel inside the ScrollViewer
            _entityStackPanel = _scrollViewer.GetUIElement_HS<StackPanel>("entity_stackpanel");

            // Find other UI elements
            _filterInput = _entityListCanvas.GetUIElement_HS<EditText>( "filter_input");
            _refreshButton = _entityListCanvas.GetUIElement_HS<Button>("refresh_button");
            _findButton = _entityListCanvas.GetUIElement_HS<Button>("find_button");
            _selectedText = _entityListCanvas.GetUIElement_HS<TextBlock>("selected_text");

            // Set font for selected text
            if (_selectedText != null && _defaultFont != null)
            {
                _selectedText.Font = _defaultFont;
            }

            // Verify required elements
            if (_entityStackPanel == null)
            {
                Logger.Error("entity_stackpanel not found in ScrollViewer");
                return;
            }

            // Set up event handlers
            if (_refreshButton != null)
            {
                _refreshButton.Click += OnRefreshButtonClicked;
            }

            if (_findButton != null)
            {
                _findButton.Click += OnFindButtonClicked;
            }

            if (_filterInput != null)
            {
                _filterInput.TextChanged += OnFilterTextChanged;
            }

            // Initial refresh
            RefreshEntityList();
        }

        protected override void OnUpdate()
        {
            // Handle mouse wheel scrolling for the entity list
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

                HandleFilterInput();
            }
        }

        private void HandleFilterInput()
        {
            if (_filterInput == null) return;

            // Check if filter input is now active
            bool isFilterActive = _filterInput.IsSelectionActive;

            // If filter just gained focus and hasn't been cleared yet
            if (isFilterActive && !_filterHasFocus && !_filterWasCleared)
            {
                // Clear the filter text
                _filterInput.Text = string.Empty;
                ApplyFilter(string.Empty);
                _filterWasCleared = true;

                Logger.Debug("Filter input cleared on focus");
            }

            // Update focus state
            _filterHasFocus = isFilterActive;

            // Reset the cleared flag when focus is lost
            if (!isFilterActive)
            {
                _filterWasCleared = false;
            }
        }

        private void OnEntitySelected(Entity entity)
        {
            // Highlight the selected entity in the list
            UpdateSelectedHighlight(entity);
        }

        private void OnRefreshButtonClicked(object sender, EventArgs e)
        {
            RefreshEntityList();
        }

        private void OnFindButtonClicked(object sender, EventArgs e)
        {
            // Implement find functionality (could open dialog or highlight selected entity)
            Logger.Info("Find button clicked - functionality to be implemented");
        }

        private void OnFilterTextChanged(object sender, EventArgs e)
        {
            if (_filterInput != null)
            {
                ApplyFilter(_filterInput.Text);
            }
        }

        public void RefreshEntityList()
        {
            if (_entityStackPanel == null) return;

            // Clear existing list
            _entityStackPanel.Children.Clear();
            _entityButtons.Clear();
            _sceneEntities.Clear();

            // Get all entities in scene (top level only)
            var topLevelEntities = _editorManager.GetAllEntities();

            // Add all entities and their children recursively
            foreach (var entity in topLevelEntities)
            {
                // Add the entity itself
                _sceneEntities.Add(entity);
                AddEntityToList(entity);

                // Add all its children recursively with indentation
                AddChildEntitiesRecursively(entity, 1);
            }

            // Log result
            Logger.Info($"Refreshed entity list - {_sceneEntities.Count} entities found");

            // Highlight selected entity if any
            UpdateSelectedHighlight(_editorManager.SelectedEntity);

            // Scroll to top
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollTo(Vector3.Zero);
            }
        }

        private void AddChildEntitiesRecursively(Entity parentEntity, int depth)
        {
            // Process all children
            foreach (var childEntity in parentEntity.GetChildren())
            {
                // Add the child entity
                _sceneEntities.Add(childEntity);

                // Create indentation prefix based on depth
                string indentPrefix = new string(' ', depth * 2) + "└─ ";

                // Add to UI with indentation
                AddEntityToListWithPrefix(childEntity, indentPrefix);

                // Recursively process its children
                AddChildEntitiesRecursively(childEntity, depth + 1);
            }
        }

        // Keep the original method signature intact for compatibility
        private void AddEntityToList(Entity entity)
        {
            AddEntityToListWithPrefix(entity, string.Empty);
        }

        // New method with prefix support
        private void AddEntityToListWithPrefix(Entity entity, string prefix)
        {
            // Create a TextBlock for the entity with font
            var textBlock = new TextBlock
            {
                Text = prefix + entity.Name,
                TextSize = 14,
                TextColor = new Color(0.9f, 0.9f, 0.9f, 1.0f),
                Font = _defaultFont,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
            };

            // Create a button for the entity with no background initially
            var button = new Button
            {
                Content = textBlock,
                Margin = new Thickness(0, 1, 0, 1),
                Padding = new Thickness(5, 5, 5, 5),
                Height = 30,
                BackgroundColor = new Color(0, 0, 0, 0), // Transparent background by default
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Handle click to select entity
            button.Click += (s, e) => _editorManager.SelectEntity(entity);

            // Store reference in our tracking list
            _entityButtons.Add(button);

            // Add to panel
            _entityStackPanel.Children.Add(button);
        }

        private void UpdateSelectedHighlight(Entity selectedEntity)
        {
            if (_entityStackPanel == null) return;

            // Reset all highlights
            foreach (var button in _entityButtons)
            {
                button.BackgroundColor = new Color(0, 0, 0, 0); // Transparent background
            }

            // Highlight selected entity
            if (selectedEntity != null)
            {
                int index = _sceneEntities.IndexOf(selectedEntity);
                if (index >= 0 && index < _entityButtons.Count)
                {
                    _entityButtons[index].BackgroundColor = new Color(0.2f, 0.4f, 0.8f, 1.0f); // Blue highlight for selected
                }
            }
        }

        private void ApplyFilter(string filterText)
        {
            if (_entityStackPanel == null) return;

            // If empty filter, show all
            if (string.IsNullOrWhiteSpace(filterText))
            {
                for (int i = 0; i < _entityButtons.Count; i++)
                {
                    _entityButtons[i].Visibility = Visibility.Visible;
                }
                return;
            }

            // Filter the list
            for (int i = 0; i < _entityButtons.Count; i++)
            {
                var button = _entityButtons[i];
                var entity = _sceneEntities[i];

                button.Visibility = entity.Name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        protected override void OnEnable()
        {
            RefreshEntityList();
        }

        protected override void OnDestroy()
        {
            // Unsubscribe from events
            if (_editorManager != null)
            {
                _editorManager.OnEntitySelected -= OnEntitySelected;
            }

            if (_refreshButton != null)
            {
                _refreshButton.Click -= OnRefreshButtonClicked;
            }

            if (_findButton != null)
            {
                _findButton.Click -= OnFindButtonClicked;
            }

            if (_filterInput != null)
            {
                _filterInput.TextChanged -= OnFilterTextChanged;
            }
        }
    }
}