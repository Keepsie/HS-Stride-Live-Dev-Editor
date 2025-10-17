# Changelog

All notable changes to HS Live Dev Editor will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.9.0] - 2025-10-16

### Added
- **Ctrl+P Export Feature**: Export all modified entities from editing session to timestamped file on Desktop
  - Exports Position, Rotation (Euler), and Scale for each modified entity
  - File format: `DevEditor_Export_YYYY-MM-DD_HH-mm-ss.txt`
  - Tracks all entities modified during session via history system

- **Ctrl+Shift+R Reverse Alignment**: Align selected entity with camera view with reversed rotation
  - Specifically designed for Blender models that export with reversed forward direction
  - Applies automatic 180° Y-axis rotation and inverted pitch
  - Works alongside existing Ctrl+Shift+F for normal alignment

- **IsFilterInputActive Property**: New public property on DevEditorManager
  - Exposes whether filter input field currently has focus
  - Allows other systems to check if user is typing in search

### Fixed
- **Filter Input Focus Issues**: Dev camera no longer responds to WASD when typing in filter field
  - DevCamera now checks `IsFilterInputActive` before processing movement input
  - Prevents unintended camera movement while searching for entities

- **Filter Deselection on Entity Click**: Clicking an entity in browser now automatically unfocuses filter
  - Clears focus from search field when entity is selected
  - Improves workflow by removing need to manually click away from filter

- **Placeholder Text Handling**: Filter placeholder "Filter entities..." only clears on first actual focus
  - Previous behavior cleared text every time filter gained focus
  - Now only clears when text is the placeholder value (case-insensitive check)
  - Preserves user's search text when clicking in/out of filter field

- **Camera Toggle Safety**: F10 dev camera toggle now only works when DevEditor is active
  - Prevents accidental camera toggling when editor is closed
  - Auto-disables dev camera when editor is deactivated

- **Transform Alignment Improvements**: Enhanced Ctrl+Shift+F/R alignment for parented entities
  - Now uses world-to-local coordinate conversion for accurate positioning
  - Fixed alignment for entities with parents in hierarchy
  - Properly calculates local transform from world position and rotation

- **Focus Camera Distance**: F key focus now moves camera to 0.8 units from target
  - Previous behavior used fixed offset that didn't always frame entity well
  - Now calculates direction and moves to specific distance from target

### Changed
- **Movement Speed Defaults**: Adjusted default camera movement speeds
  - Base movement speed: 10.0 → 1.0
  - Fast movement speed: 25.0 → 5.0
  - Allows finer control for precise positioning

- **Rotation Speed Multiplier**: Increased rotation speed for entity manipulation
  - Rotation multiplier: 7.0 → 50.0
  - Makes rotation adjustments more responsive

- **Editor Movement Step**: Reduced base position step for finer control
  - Base step: 0.05 → 0.01
  - Slow modifier multiplier: 0.05 → 0.25
  - Provides more precise movement control

- **Entity Destruction Method**: Changed from `Destroy()` to `Destroy_HS()`
  - Uses HS Core helper method for consistent entity cleanup
  - Aligns with HS framework conventions

### Technical
- **History System Enhancement**: Added `GetAllModifiedEntities()` method to EditorHistory
  - Collects unique entities from both undo and redo stacks
  - Powers new export functionality
  - Returns List<Entity> of all modified entities

- **DevSceneEntities Public Property**: Exposed `IsFilterInputActive` as public property
  - Replaced internal `_filterHasFocus` tracking with public accessor
  - Allows external systems to query filter state

- **Transform Calculation**: Uses `CalculateLocalTransform_HS()` for parent-child transforms
  - Properly converts world coordinates to local space
  - Fixes alignment issues with nested entities

## [0.6.0] - Previous Release

### Initial Features
- Live entity transform manipulation
- Dev camera with free-fly controls
- Scene browser with hierarchy
- Property inspector with copy functionality
- Undo/redo command history system
- Keyboard-based entity manipulation
- Filter/search for entities

---

**Note**: Version 0.6.0 and earlier changes are documented for historical reference.
Detailed changelogs begin with version 0.9.0.
