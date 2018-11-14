# v0.8.2

- fixed broken FullLayoutEngine broken by previous changes

# v0.8.1

- workspaces are now slightly sticky-er in their default configuration
  - WorkspaceContainer will now remember the last monitor assigned to a workspace, and will try to use that monitor when focusing a window on said workspace
- better handling of windows that don't emit proper events for window hiding

# v0.8.0

- allow override of WorkspaceWidget color selection logic via GetDisplayColor
- made SwitchToWorkspace(IWorkspace) public
- improved styling of action menu
- refactored configuration API, now using proper CSX scripting
- fixed bug in state saving
- restarts now persist window order inside a workspace

# v0.7.2

- fixed WorkspaceWidget to allow actually overriding GetDisplayName

# v0.7.1

- refactored WorkspaceSelectorFunc and WindowFilterFunc into IWindowRouter
- added "switch to window" menu action
- added better fuzzy find support to action menu
- improved focus handling for out-of-view windows

# v0.7

- cycle layouts via click on ActiveLayoutWidget 
- added ActionMenu plugin!

# v0.6

- refactored IWorkspaceManager, moving most selection logic into IWorkspaceContainer, which is provided by the user config
- added default keybind `alt-t` that toggles tiling for the focused window
- fixed bug in title widget that prevented titles on start for empty monitors

# v0.5

- fixes to focus defaults
- added default keybind `alt-left` and `alt-right` to cycle workspaces left and right
- added ability to specify click handlers for bar widget parts, added this functionality to workspace widget
- allow override of display name format in WorkspaceWidget

# v0.4

- Minor fixes

# v0.3

- Implemented installer via WiX

# v0.2

- Added support for colors in Workspacer.Bar
- Renamed to Workspacer


# v0.1

- Initial Release!