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

        public void AddDefaults()
        {
            _filters.Add((window) => !window.Title.Contains("Task Manager"));
            _filters.Add((window) => !window.Title.Contains("Program Manager"));
            _filters.Add((window) => !(window.Process.Id == Process.GetCurrentProcess().Id));
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
