﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workspacer
{
    public enum StickyWorkspaceIndexMode
    {
        Local,
        Global
    }

    public class StickyWorkspaceContainer : IWorkspaceContainer
    {
        private IConfigContext _context;
        private StickyWorkspaceIndexMode _indexMode;
        private Dictionary<IMonitor, List<IWorkspace>> _workspaces;
        private Dictionary<IMonitor, List<IWorkspace>> _orderedWorkspaces;
        private List<IWorkspace> _allWorkspaces;
        private Dictionary<IWorkspace, IMonitor> _wtm;

        public StickyWorkspaceContainer(IConfigContext context) : this(context, StickyWorkspaceIndexMode.Global) { }

        public StickyWorkspaceContainer(IConfigContext context, StickyWorkspaceIndexMode indexMode)
        {
            _context = context;
            _indexMode = indexMode;
            _workspaces = new Dictionary<IMonitor, List<IWorkspace>>();
            _orderedWorkspaces = new Dictionary<IMonitor, List<IWorkspace>>();
            _allWorkspaces = new List<IWorkspace>();
            foreach (var monitor in context.Workspaces.Monitors)
            {
                _workspaces[monitor] = new List<IWorkspace>();
                _orderedWorkspaces[monitor] = new List<IWorkspace>();
                _wtm = new Dictionary<IWorkspace, IMonitor>();
            }
        }

        public void CreateWorkspace(IMonitor monitor, string name, params ILayoutEngine[] layouts)
        {
            var workspace = new Workspace(_context, name, layouts);
            _workspaces[monitor].Add(workspace);
            _orderedWorkspaces[monitor].Add(workspace);
            _allWorkspaces.Add(workspace);
            _wtm[workspace] = monitor;
        }

        public void AssignWorkspaceToMonitor(IWorkspace workspace, IMonitor monitor)
        {
            List<IWorkspace> workspaces;
            if (monitor != null)
            {
                workspaces = _workspaces[monitor];

                if (workspaces.Contains(workspace))
                {
                    workspaces.Remove(workspace);
                    workspaces.Insert(0, workspace);
                }
            }
            else
            {
                workspaces = _workspaces[_workspaces.Keys.First(m => _workspaces[m].Contains(workspace))];

                workspaces.Remove(workspace);
                workspaces.Add(workspace);
            }
        }

        public IEnumerable<IWorkspace> GetAllWorkspaces()
        {
            return _workspaces.SelectMany(kv => kv.Value);
        }

        public IMonitor GetCurrentMonitorForWorkspace(IWorkspace workspace)
        {
            VerifyExists(workspace);
            foreach (var monitor in _workspaces.Keys)
            {
                if (_workspaces[monitor].FirstOrDefault() == workspace)
                {
                    return monitor;
                }
            }
            return null;
        }

        public IMonitor GetDesiredMonitorForWorkspace(IWorkspace workspace)
        {
            VerifyExists(workspace);
            return _wtm[workspace];
        }

        public IWorkspace GetNextWorkspace(IWorkspace currentWorkspace)
        {
            VerifyExists(currentWorkspace);

            var monitor = _wtm[currentWorkspace];
            var workspaces = _orderedWorkspaces[monitor];

            var index = workspaces.IndexOf(currentWorkspace);
            if (index >= workspaces.Count - 1)
                index = 0;
            else
                index = index + 1;

            return workspaces[index];
        }

        public IWorkspace GetPreviousWorkspace(IWorkspace currentWorkspace)
        {
            VerifyExists(currentWorkspace);

            var monitor = _wtm[currentWorkspace];
            var workspaces = _orderedWorkspaces[monitor];

            var index = workspaces.IndexOf(currentWorkspace);
            if (index == 0)
                index = _workspaces.Count - 1;
            else
                index = index - 1;

            return workspaces[index];
        }

        public IWorkspace GetWorkspaceAtIndex(IWorkspace currentWorkspace, int index)
        {
            VerifyExists(currentWorkspace);

            if (_indexMode == StickyWorkspaceIndexMode.Local)
            {
                var monitor = _wtm[currentWorkspace];
                var workspaces = _orderedWorkspaces[monitor];
                if (index >= workspaces.Count)
                    return null;

                return workspaces[index];
            }
            else
            {
                if (index >= _allWorkspaces.Count)
                    return null;
                return _allWorkspaces[index];
            }
        }

        public IWorkspace GetWorkspaceByName(IWorkspace currentWorkspace, string name)
        {
            VerifyExists(currentWorkspace);
            var monitor = _wtm[currentWorkspace];
            var workspaces = _orderedWorkspaces[monitor];

            return workspaces.FirstOrDefault(w => w.Name == name);
        }

        public IWorkspace GetWorkspaceForMonitor(IMonitor monitor)
        {
            return _workspaces[monitor].FirstOrDefault();
        }

        public IEnumerable<IWorkspace> GetWorkspaces(IWorkspace currentWorkspace)
        {
            if (_indexMode == StickyWorkspaceIndexMode.Local)
            {
                VerifyExists(currentWorkspace);
                var monitor = _wtm[currentWorkspace];
                return _orderedWorkspaces[monitor];
            } else
            {
                return _allWorkspaces;
            }
        }

        public IEnumerable<IWorkspace> GetWorkspaces(IMonitor currentMonitor)
        {
            if (_indexMode == StickyWorkspaceIndexMode.Local)
            {
                return _orderedWorkspaces[currentMonitor];
            } else
            {
                return _allWorkspaces;
            }
        }

        private void VerifyExists(IWorkspace workspace)
        {
            if (!_wtm.ContainsKey(workspace))
                throw new Exception("attempted to access container using a workspace that isn't contained in it");
        }
    }
}
