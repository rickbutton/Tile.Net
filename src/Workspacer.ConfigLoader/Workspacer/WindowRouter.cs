﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workspacer
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

            _filters.Add((window) => window.Class != "TaskManagerWindow");
            _filters.Add((window) => window.Class != "MSCTFIME UI");
            _filters.Add((window) => window.Class != "SHELLDLL_DefView");
            _filters.Add((window) => window.Process.ProcessName != "SearchUI");
            _filters.Add((window) => window.Process.ProcessName != "ShellExperienceHost");
            _filters.Add((window) => window.Process.ProcessName != "LockApp");
            _filters.Add((window) => window.Class != "LockScreenBackstopFrame");
            _filters.Add((window) => window.Process.ProcessName != "PeopleExperienceHost");
            _filters.Add((window) => !(window.Process.Id == Process.GetCurrentProcess().Id));
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
    }
}
