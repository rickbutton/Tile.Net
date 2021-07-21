using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace workspacer
{
    public class WindowRouter : IWindowRouter
    {
        private IConfigContext _context;
        private List<Func<IWindow, bool>> _filters;
        private List<Func<IWindow, IWorkspace>> _routes;

        public WindowRouter(IConfigContext context)
        {
            _context = context;
            _filters = new List<Func<IWindow, bool>>();
            _routes = new List<Func<IWindow, IWorkspace>>();

            IgnoreWindowClass("TaskManagerWindow");
            IgnoreWindowClass("MSCTFIME UI");
            IgnoreWindowClass("SHELLDLL_DefView");
            IgnoreProcessName("SearchUI");
            IgnoreProcessName("ShellExperienceHost");
            IgnoreProcessName("LockApp");
            IgnoreWindowClass("LockScreenBackstopFrame");
            IgnoreProcessName("PeopleExperienceHost");
            IgnoreWindowClass("Progman");
            IgnoreProcessName("StartMenuExperienceHost");
            IgnoreProcessName("SearchApp");
            IgnoreProcessName("SearchHost"); // Windows 11 search
            IgnoreWindowClass("Shell_TrayWnd"); // Windows 11 start
            IgnoreProcessName("ScreenClippingHost");
            _filters.Add((window) => !(window.ProcessId == Process.GetCurrentProcess().Id));
        }

        public IWorkspace RouteWindow(IWindow window, IWorkspace defaultWorkspace = null)
        {
            foreach (var filter in _filters)
            {
                if (!filter(window))
                    return null;
            }

            foreach (var route in _routes)
            {
                var workspace = route(window);
                if (workspace != null)
                    return workspace;
            }
            return defaultWorkspace ?? _context.Workspaces.FocusedWorkspace;
        }

        public void ClearFilters()
        {
            _filters.Clear();
        }

        public void ClearRoutes()
        {
            _routes.Clear();
        }

        public void AddFilter(Func<IWindow, bool> filter)
        {
            _filters.Add(filter);
        }

        public void AddRoute(Func<IWindow, IWorkspace> route)
        {
            _routes.Add(route);
        }

        public void IgnoreWindowClass(string windowClass) { _filters.Add((w) => w.Class != windowClass); }
        public void IgnoreProcessName(string processName) { _filters.Add((w) => w.ProcessName != processName); }
        public void IgnoreTitle(string title) { _filters.Add((w) => w.Title != title); }
        public void IgnoreTitleMatch(string match)
        {
            var regex = new Regex(match);
            _filters.Add((w) => !regex.IsMatch(w.Title));
        }

        private IWorkspace LookupWorkspace(string name)
        {
            return _context.WorkspaceContainer[name];
        }

        public void RouteWindowClass(string windowClass, string workspaceName)
        {
            _routes.Add(window => window.Class == windowClass ? LookupWorkspace(workspaceName) : null);
        }

        public void RouteWindowClass(string windowClass, IWorkspace workspace)
        {
            _routes.Add(window => window.Class == windowClass ? workspace : null);
        }

        public void RouteProcessName(string processName, string workspaceName)
        {
            _routes.Add(window => window.ProcessName == processName ? LookupWorkspace(workspaceName) : null);
        }

        public void RouteProcessName(string processName, IWorkspace workspace)
        {
            _routes.Add(window => window.ProcessName == processName ? workspace : null);
        }

        public void RouteTitle(string title, string workspaceName)
        {
            _routes.Add(window => window.Title == title ? LookupWorkspace(workspaceName) : null);
        }

        public void RouteTitle(string title, IWorkspace workspace)
        {
            _routes.Add(window => window.Title == title ? workspace : null);
        }

        public void RouteTitleMatch(string match, string workspaceName)
        {
            var regex = new Regex(match);
            _routes.Add(window => regex.IsMatch(window.Title) ? LookupWorkspace(workspaceName) : null);
        }

        public void RouteTitleMatch(string match, IWorkspace workspace)
        {
            var regex = new Regex(match);
            _routes.Add(window => regex.IsMatch(window.Title) ? workspace : null);
        }
    }
}
