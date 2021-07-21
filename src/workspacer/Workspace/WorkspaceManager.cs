using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspacer
{
    public class WorkspaceManager : IWorkspaceManager
    {
        private static Logger Logger = Logger.Create();

        private IConfigContext _context;
        private IWorkspace _lastWorkspace;
        public IWorkspace FocusedWorkspace => _context.WorkspaceContainer
            .GetWorkspaceForMonitor(_context.MonitorContainer.FocusedMonitor);

        private Dictionary<IWindow, IWorkspace> _windowsToWorkspaces;

        public event WorkspaceUpdatedDelegate WorkspaceUpdated;
        public event WindowAddedDelegate WindowAdded;
        public event WindowUpdatedDelegate WindowUpdated;
        public event WindowRemovedDelegate WindowRemoved;
        public event WindowMovedDelegate WindowMoved;
        public event FocusedMonitorUpdatedDelegate FocusedMonitorUpdated;

        public WorkspaceManager(IConfigContext context)
        {
            _context = context;
            _windowsToWorkspaces = new Dictionary<IWindow, IWorkspace>();
        }

        public void SwitchToWindow(IWindow window)
        {
            Logger.Debug("SwitchToWindow({0})", window);

            if (_windowsToWorkspaces.ContainsKey(window))
            {
                var workspace = _windowsToWorkspaces[window];
                SwitchToWorkspace(workspace);
                window.Focus();
            }
        }

        public void SwitchToWorkspace(int index)
        {
            Logger.Debug("SwitchToWorkspace({0})", index);
            var currentWorkspace = FocusedWorkspace;
            var targetWorkspace = _context.WorkspaceContainer.GetWorkspaceAtIndex(currentWorkspace, index);
            SwitchToWorkspace(targetWorkspace);
        }

        public void SwitchToWorkspace(IWorkspace targetWorkspace)
        {
            Logger.Debug("SwitchToWorkspace({0})", targetWorkspace);
            if (targetWorkspace != null)
            {
                var focusedMonitor = _context.MonitorContainer.FocusedMonitor;
                var destMonitor = _context.WorkspaceContainer.GetDesiredMonitorForWorkspace(targetWorkspace) ?? focusedMonitor;
                var currentWorkspace = _context.WorkspaceContainer.GetWorkspaceForMonitor(destMonitor);
                var sourceMonitor = _context.WorkspaceContainer.GetCurrentMonitorForWorkspace(targetWorkspace);

                if (targetWorkspace != currentWorkspace)
                {
                    _lastWorkspace = currentWorkspace;
                    _context.WorkspaceContainer.AssignWorkspaceToMonitor(currentWorkspace, sourceMonitor);
                    _context.WorkspaceContainer.AssignWorkspaceToMonitor(targetWorkspace, destMonitor);

                    currentWorkspace.DoLayout();
                    targetWorkspace.DoLayout();

                    WorkspaceUpdated?.Invoke();

                    targetWorkspace.FocusLastFocusedWindow();
                }
            }
        }

        public void SwitchToLastFocusedWorkspace()
        {
            Logger.Debug("SwitchToLastWorkspace({0})", _lastWorkspace);
            var targetWorkspace = _lastWorkspace;
            _lastWorkspace = FocusedWorkspace;
            SwitchToWorkspace(targetWorkspace);
        }

        public void SwitchMonitorToWorkspace(int monitorIndex, int workspaceIndex)
        {
            Logger.Debug("SwitchMonitorToWorkspace(monitorIndex: {0}, workspaceIndex: {1})", monitorIndex, workspaceIndex);
            if (monitorIndex >= _context.MonitorContainer.NumMonitors)
                return;

            var destMonitor = _context.MonitorContainer.GetMonitorAtIndex(monitorIndex);
            var currentWorkspace = _context.WorkspaceContainer.GetWorkspaceForMonitor(destMonitor);
            var targetWorkspace = _context.WorkspaceContainer.GetWorkspaceAtIndex(currentWorkspace, workspaceIndex);
            var sourceMonitor = _context.WorkspaceContainer.GetCurrentMonitorForWorkspace(targetWorkspace);

            _lastWorkspace = currentWorkspace;
            _context.WorkspaceContainer.AssignWorkspaceToMonitor(currentWorkspace, sourceMonitor);
            _context.WorkspaceContainer.AssignWorkspaceToMonitor(targetWorkspace, destMonitor);

            currentWorkspace.DoLayout();
            targetWorkspace.DoLayout();

            WorkspaceUpdated?.Invoke();

            targetWorkspace.FocusLastFocusedWindow();
        }

        public void SwitchToNextWorkspace()
        {
            Logger.Debug("SwitchToNextWorkspace");
            var destMonitor = _context.MonitorContainer.FocusedMonitor;
            var currentWorkspace = _context.WorkspaceContainer.GetWorkspaceForMonitor(destMonitor);
            var targetWorkspace = _context.WorkspaceContainer.GetNextWorkspace(currentWorkspace);
            var sourceMonitor = _context.WorkspaceContainer.GetCurrentMonitorForWorkspace(targetWorkspace);

            _lastWorkspace = currentWorkspace;
            _context.WorkspaceContainer.AssignWorkspaceToMonitor(currentWorkspace, sourceMonitor);
            _context.WorkspaceContainer.AssignWorkspaceToMonitor(targetWorkspace, destMonitor);

            currentWorkspace.DoLayout();
            targetWorkspace.DoLayout();

            WorkspaceUpdated?.Invoke();

            targetWorkspace.FocusLastFocusedWindow();
        }

        public void SwitchToPreviousWorkspace()
        {
            Logger.Debug("SwitchToPreviousWorkspace");
            var destMonitor = _context.MonitorContainer.FocusedMonitor;
            var currentWorkspace = _context.WorkspaceContainer.GetWorkspaceForMonitor(destMonitor);
            var targetWorkspace = _context.WorkspaceContainer.GetPreviousWorkspace(currentWorkspace);
            var sourceMonitor = _context.WorkspaceContainer.GetCurrentMonitorForWorkspace(targetWorkspace);

            _lastWorkspace = currentWorkspace;
            _context.WorkspaceContainer.AssignWorkspaceToMonitor(currentWorkspace, sourceMonitor);
            _context.WorkspaceContainer.AssignWorkspaceToMonitor(targetWorkspace, destMonitor);

            currentWorkspace.DoLayout();
            targetWorkspace.DoLayout();

            WorkspaceUpdated?.Invoke();

            targetWorkspace.FocusLastFocusedWindow();
        }

        public void SwitchFocusedMonitor(int index)
        {
            Logger.Debug("SwitchFocusedMonitor({0})", index);

            var focusedMonitor = _context.MonitorContainer.FocusedMonitor;
            if (index < _context.MonitorContainer.NumMonitors && index >= 0)
            {
                var monitor = _context.MonitorContainer.GetMonitorAtIndex(index);
                if (focusedMonitor != monitor)
                {
                    _context.MonitorContainer.FocusedMonitor = monitor;
                    FocusedWorkspace.FocusLastFocusedWindow();

                    FocusedMonitorUpdated?.Invoke();
                }
            }
        }

        public void SwitchFocusToNextMonitor()
        {
            var focusedMonitor = _context.MonitorContainer.FocusedMonitor;
            var targetMonitor = _context.MonitorContainer.GetNextMonitor();
            if (focusedMonitor != targetMonitor)
            {
                _context.MonitorContainer.FocusedMonitor = targetMonitor;
                FocusedWorkspace.FocusLastFocusedWindow();

                FocusedMonitorUpdated?.Invoke();
            }
        }

        public void SwitchFocusToPreviousMonitor()
        {
            var focusedMonitor = _context.MonitorContainer.FocusedMonitor;
            var targetMonitor = _context.MonitorContainer.GetPreviousMonitor();
            if (focusedMonitor != targetMonitor)
            {
                _context.MonitorContainer.FocusedMonitor = targetMonitor;
                FocusedWorkspace.FocusLastFocusedWindow();

                FocusedMonitorUpdated?.Invoke();
            }
        }

        public void SwitchFocusedMonitorToMouseLocation()
        {
            Logger.Debug("SwitchFocusedMonitorToMouseLocation");
            var loc = Control.MousePosition;
            var screen = Screen.FromPoint(new Point(loc.X, loc.Y));
            var monitor = _context.MonitorContainer.GetMonitorAtPoint(loc.X, loc.Y);
            _context.MonitorContainer.FocusedMonitor = monitor;
            FocusedMonitorUpdated?.Invoke();
        }

        public void MoveFocusedWindowToWorkspace(int index)
        {
            Logger.Debug("MoveFocusedWindowToWorkspace({0})", index);
            var window = FocusedWorkspace.FocusedWindow;
            var targetWorkspace = _context.WorkspaceContainer.GetWorkspaceAtIndex(FocusedWorkspace, index);

            if (window != null && targetWorkspace != null)
            {
                var windows = FocusedWorkspace.ManagedWindows;
                // get next window
                var nextWindow = windows.SkipWhile(x => x != window).Skip(1).FirstOrDefault();
                if (nextWindow == null)
                {
                    // get previous window
                    nextWindow = windows.TakeWhile(x => x != window).LastOrDefault();
                }

                FocusedWorkspace.RemoveWindow(window);
                targetWorkspace.AddWindow(window);

                _windowsToWorkspaces[window] = targetWorkspace;
                WindowMoved?.Invoke(window, FocusedWorkspace, targetWorkspace);

                nextWindow?.Focus();
            }
        }
        public void MoveFocusedWindowAndSwitchToNextWorkspace()
        {
            Logger.Debug("MoveFocusedWindowAndSwitchToNextWorkspace()");
            var targetWorkspaceIndex = _context.WorkspaceContainer.GetNextWorkspaceIndex(FocusedWorkspace);
            _context.Workspaces.MoveFocusedWindowToWorkspace(targetWorkspaceIndex);
            _context.Workspaces.SwitchToNextWorkspace();
        }

        public void MoveFocusedWindowAndSwitchToPreviousWorkspace()
        {
            Logger.Debug("MoveFocusedWindowAndSwitchToPreviousWorkspace()");
            var targetWorkspaceIndex = _context.WorkspaceContainer.GetPreviousWorkspaceIndex(FocusedWorkspace);
            _context.Workspaces.MoveFocusedWindowToWorkspace(targetWorkspaceIndex);
            _context.Workspaces.SwitchToPreviousWorkspace();
        }

        public void MoveFocusedWindowToMonitor(int index)
        {
            Logger.Debug("MoveFocusedWindowToMonitor({0})", index);
            if (index >= _context.MonitorContainer.NumMonitors)
                return;

            var window = FocusedWorkspace.FocusedWindow;
            var targetMonitor = _context.MonitorContainer.GetMonitorAtIndex(index);
            var targetWorkspace = _context.WorkspaceContainer.GetWorkspaceForMonitor(targetMonitor);

            if (window != null && targetWorkspace != null)
            {
                var windows = FocusedWorkspace.ManagedWindows;
                // get next window
                var nextWindow = windows.SkipWhile(x => x != window).Skip(1).FirstOrDefault();
                if (nextWindow == null)
                {
                    // get previous window
                    nextWindow = windows.TakeWhile(x => x != window).LastOrDefault();
                }

                FocusedWorkspace.RemoveWindow(window);
                targetWorkspace.AddWindow(window);

                _windowsToWorkspaces[window] = targetWorkspace;
                WindowMoved?.Invoke(window, FocusedWorkspace, targetWorkspace);

                nextWindow?.Focus();
            }
        }

        public void MoveFocusedWindowToNextMonitor()
        {
            Logger.Debug("MoveFocusedWindowToNextMonitor");
            var window = FocusedWorkspace.FocusedWindow;
            var focusedMonitor = _context.MonitorContainer.FocusedMonitor;
            var targetMonitor = _context.MonitorContainer.GetNextMonitor();
            var targetWorkspace = _context.WorkspaceContainer.GetWorkspaceForMonitor(targetMonitor);

            if (window != null && targetWorkspace != null)
            {
                FocusedWorkspace.RemoveWindow(window);
                targetWorkspace.AddWindow(window);

                _windowsToWorkspaces[window] = targetWorkspace;
                WindowMoved?.Invoke(window, FocusedWorkspace, targetWorkspace);
                if (focusedMonitor != targetMonitor)
                {
                    window.Focus();
                    _context.MonitorContainer.FocusedMonitor = targetMonitor;
                    FocusedMonitorUpdated?.Invoke();
                }
            }
        }

        public void MoveFocusedWindowToPreviousMonitor()
        {
            Logger.Debug("MoveFocusedWindowToPreviousMonitor");
            var window = FocusedWorkspace.FocusedWindow;
            var focusedMonitor = _context.MonitorContainer.FocusedMonitor;
            var targetMonitor = _context.MonitorContainer.GetPreviousMonitor();
            var targetWorkspace = _context.WorkspaceContainer.GetWorkspaceForMonitor(targetMonitor);

            if (window != null && targetWorkspace != null)
            {
                FocusedWorkspace.RemoveWindow(window);
                targetWorkspace.AddWindow(window);

                _windowsToWorkspaces[window] = targetWorkspace;
                WindowMoved?.Invoke(window, FocusedWorkspace, targetWorkspace);
                if (focusedMonitor != targetMonitor)
                {
                    _context.MonitorContainer.FocusedMonitor = targetMonitor;
                    window.Focus();
                    FocusedMonitorUpdated?.Invoke();
                }
            }
        }

        public void MoveAllWindows(IWorkspace source, IWorkspace dest)
        {
            var toMove = source.Windows.ToList();
            foreach (var window in toMove)
            {
                RemoveWindow(window);
            }
            foreach (var window in toMove)
            {
                AddWindowToWorkspace(window, dest);
            }
        }

        public void ForceWorkspaceUpdate()
        {
            WorkspaceUpdated?.Invoke();
        }

        public void AddWindow(IWindow window, bool firstCreate)
        {
            AddWindow(window, false, firstCreate);
        }

        private void AddWindow(IWindow window, bool switchToWorkspace, bool firstCreate)
        {
            Logger.Debug("AddWindow({0})", window);

            if (!_windowsToWorkspaces.ContainsKey(window))
            {
                var workspace = firstCreate ? _context.WindowRouter.RouteWindow(window) : GetWorkspaceForWindowLocation(window);

                if (workspace != null)
                {
                    AddWindowToWorkspace(window, workspace);

                    if (switchToWorkspace && window.CanLayout)
                    {
                        SwitchToWorkspace(workspace);
                    }
                }
            }
        }

        private void AddWindowToWorkspace(IWindow window, IWorkspace workspace)
        {
            Logger.Debug("AddWindowToWorkspace({0}, {1})", window, workspace);
            workspace.AddWindow(window);
            _windowsToWorkspaces[window] = workspace;

            if (window.IsFocused)
            {
                var monitor = _context.WorkspaceContainer.GetCurrentMonitorForWorkspace(workspace);
                if (monitor != null)
                {
                    _context.MonitorContainer.FocusedMonitor = monitor;
                }
            }
            WindowAdded?.Invoke(window, workspace);
        }

        public void RemoveWindow(IWindow window)
        {
            if (_windowsToWorkspaces.ContainsKey(window))
            {
                Logger.Debug("RemoveWindow({0})", window);
                var workspace = _windowsToWorkspaces[window];
                _windowsToWorkspaces[window].RemoveWindow(window);
                _windowsToWorkspaces.Remove(window);
                WindowRemoved?.Invoke(window, workspace);
            }
        }

        public void UpdateWindow(IWindow window, WindowUpdateType type)
        {
            if (_windowsToWorkspaces.ContainsKey(window))
            {
                Logger.Trace("UpdateWindow({0})", window);
                var workspace = _windowsToWorkspaces[window];
                if (window.IsFocused)
                {
                    var monitor = _context.WorkspaceContainer.GetCurrentMonitorForWorkspace(workspace);
                    if (monitor != null)
                    {
                        _context.MonitorContainer.FocusedMonitor = monitor;
                    }
                    else
                    {
                        if (type == WindowUpdateType.Foreground)
                        {
                            // TODO: show flash for workspace (in bar?)
                            Logger.Trace("workspace.IsIndicating = true for workspace {0}", workspace);
                            workspace.IsIndicating = true;
                            WorkspaceUpdated?.Invoke();
                        }
                    }

                    if (type == WindowUpdateType.Move)
                    {
                        TrySwapWindowToMouse(window);
                    }
                    _windowsToWorkspaces[window].UpdateWindow(window, type);
                    WindowUpdated?.Invoke(window, workspace);
                }
            }
        }

        private void TrySwapWindowToMouse(IWindow window)
        {
            var point = Control.MousePosition;
            int x = point.X;
            int y = point.Y;

            var currentWorkspace = _windowsToWorkspaces[window];

            if (currentWorkspace.IsPointInside(x, y))
            {
                currentWorkspace.SwapWindowToPoint(window, x, y);
            } else
            {
                foreach (var workspace in _context.WorkspaceContainer.GetAllWorkspaces())
                {
                    var monitor = _context.WorkspaceContainer.GetCurrentMonitorForWorkspace(workspace);
                    if (monitor != null && workspace.IsPointInside(x, y))
                    {
                        currentWorkspace.RemoveWindow(window, false);
                        workspace.AddWindow(window, false);
                        _windowsToWorkspaces[window] = workspace;

                        workspace.SwapWindowToPoint(window, x, y);
                        currentWorkspace.DoLayout();
                    }
                }
            }
        }

        private IWorkspace GetWorkspaceForWindowLocation(IWindow window)
        {
            var location = window.Location;
            var monitor = _context.MonitorContainer.GetMonitorAtRect(location.X, location.Y, location.Width, location.Height);
            return _context.WorkspaceContainer.GetWorkspaceForMonitor(monitor);
        }

        public WorkspaceState GetState()
        {
            var workspacesToWindows = new List<List<int>>();
            var monitorsToWorkspaces = new List<int>();

            var allWorkspaces = _context.WorkspaceContainer.GetAllWorkspaces().ToList();

            int focusedWindow = 0;
            foreach (var workspace in allWorkspaces)
            {
                var windows = new List<int>();
                foreach (var window in workspace.Windows)
                {
                    windows.Add((int)window.Handle);

                    if (window.IsFocused)
                    {
                        focusedWindow = (int)window.Handle;
                    }
                }
                workspacesToWindows.Add(windows);
            }

            var monitors = _context.MonitorContainer.GetAllMonitors();
            for (var i = 0; i < monitors.Length; i++)
            {
                for (var j = 0; j < allWorkspaces.Count; j++)
                {
                    var monitor = monitors[i];
                    var workspace = allWorkspaces[j];

                    if (_context.WorkspaceContainer.GetCurrentMonitorForWorkspace(workspace) == monitor)
                    {
                        monitorsToWorkspaces.Insert(i, j);
                    }
                }
            }

            var monitorIndex = _context.MonitorContainer.FocusedMonitor.Index;

            return new WorkspaceState()
            {
                WorkspacesToWindows = workspacesToWindows,
                MonitorsToWorkspaces = monitorsToWorkspaces,
                FocusedMonitor = monitorIndex,
                FocusedWindow = focusedWindow
            };
        }

        public void InitializeWithState(WorkspaceState state, IEnumerable<IWindow> allWindows)
        {
            var wtw = state.WorkspacesToWindows;
            var allWorkspaces = _context.WorkspaceContainer.GetAllWorkspaces().ToList();

            var focusedMonitor = _context.MonitorContainer.GetMonitorAtIndex(state.FocusedMonitor);
            _context.MonitorContainer.FocusedMonitor = focusedMonitor;

            var mtw = state.MonitorsToWorkspaces;
            if (mtw.Count == _context.MonitorContainer.NumMonitors)
            {
                for (var i = 0; i < _context.MonitorContainer.NumMonitors; i++)
                {
                    var workspaceIdx = mtw[i];
                    var workspace = allWorkspaces[workspaceIdx];
                    var monitor = _context.MonitorContainer.GetMonitorAtIndex(i);
                    _context.WorkspaceContainer.AssignWorkspaceToMonitor(workspace, monitor);
                }
            }
            else
            {
                for (var i = 0; i < _context.MonitorContainer.NumMonitors; i++)
                {
                    var m = _context.MonitorContainer.GetMonitorAtIndex(i);
                    var w = allWorkspaces[i];
                    _context.WorkspaceContainer.AssignWorkspaceToMonitor(w, m);
                }
            }

            for (var i = 0; i < wtw.Count; i++)
            {
                var workspaceWindows = wtw[i];
                for (var j = 0; j < wtw[i].Count; j++)
                {
                    var handle = workspaceWindows[j];
                    var window = allWindows.FirstOrDefault(w => (int)w.Handle == handle);

                    if (window == null)
                        continue;

                    var routedWorkspace = _context.WindowRouter.RouteWindow(window);
                    if (routedWorkspace == null)
                        continue;

                    if (i < allWorkspaces.Count)
                    {
                        // ignoring the routed workspace here, as the user probably put this window into
                        // the saved workspace on purpose
                        var workspace = allWorkspaces[i];
                        AddWindowToWorkspace(window, workspace);
                    }
                    else
                    {
                        AddWindowToWorkspace(window, routedWorkspace);
                    }

                    if (state.FocusedWindow == handle)
                    {
                        window.Focus();
                    }
                }
            }
        }

        public void Initialize(IEnumerable<IWindow> windows)
        {
            var allWorkspaces = _context.WorkspaceContainer.GetAllWorkspaces().ToList();
            for (var i = 0; i < _context.MonitorContainer.NumMonitors; i++)
            {
                var m = _context.MonitorContainer.GetMonitorAtIndex(i);
                var w = allWorkspaces[i];
                _context.WorkspaceContainer.AssignWorkspaceToMonitor(w, m);
            }

            foreach (var w in windows)
            {
                var location = w.Location;
                var locationWorkspace = GetWorkspaceForWindowLocation(w);
                var destWorkspace = _context.WindowRouter.RouteWindow(w, locationWorkspace);

                if (destWorkspace != null)
                {
                    AddWindowToWorkspace(w, destWorkspace);
                    AddWindow(w, false);
                }
            }
        }
    }
}
