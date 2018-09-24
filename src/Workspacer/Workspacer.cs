﻿using System;
using System.Linq;
using Newtonsoft.Json;
using Workspacer.Shared;
using Workspacer.ConfigLoader;
using Timer = System.Timers.Timer;
using System.Reflection;

namespace Workspacer
{
    public class Workspacer
    {
        public static bool Enabled { get; set; }

        private PipeServer _pipeServer;
        private ConfigContext _context;
        private Timer _timer;

        public Workspacer()
        {
            _pipeServer = new PipeServer();
            _timer = new Timer();
            _timer.Elapsed += (s, e) => UpdateActiveHandles();
            _timer.Interval = 5000;
        }

        public void Start()
        {
            _pipeServer.Start();
            _timer.Enabled = true;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            WindowsDesktopManager.Instance.WindowCreated += WorkspaceManager.Instance.AddWindow;
            WindowsDesktopManager.Instance.WindowDestroyed += WorkspaceManager.Instance.RemoveWindow;
            WindowsDesktopManager.Instance.WindowUpdated += WorkspaceManager.Instance.UpdateWindow;

            _context = new ConfigContext(_pipeServer)
            {
                Keybinds = KeybindManager.Instance,
                Workspaces = WorkspaceManager.Instance,
                Layouts = LayoutManager.Instance,
                Plugins = PluginManager.Instance,
            };
            var config = GetConfig();
            config.Configure(_context);

            var state = StateManager.Instance.LoadState();

            WindowsDesktopManager.Instance.Initialize();
            if (state != null)
            {
                WorkspaceManager.Instance.InitializeWithState(state.WorkspaceState, WindowsDesktopManager.Instance.Windows);
                Enabled = true;
            }
            else
            {
                WorkspaceManager.Instance.Initialize(WindowsDesktopManager.Instance.Windows);
                Enabled = true;
                WorkspaceManager.Instance.SwitchToWorkspace(0);
            }
            foreach (var workspace in WorkspaceManager.Instance.Workspaces)
            {
                workspace.ForceLayout();
            }

            PluginManager.Instance.AfterConfig(_context);

            FocusStealer.Initialize();
            while(true) { }
        }

        private IConfig GetConfig()
        {
            return ConfigHelper.GetConfig(PluginManager.Instance.AvailablePlugins);
        }

        private void SendResponse(LauncherResponse response)
        {
            var str = JsonConvert.SerializeObject(response);
            _pipeServer.SendResponse(str);
        }

        public void Quit()
        {
            _context.Quit();
        }

        private void UpdateActiveHandles()
        {
            var response = new LauncherResponse()
            {
                Action = LauncherAction.UpdateHandles,
                ActiveHandles = WorkspaceManager.Instance.GetActiveHandles().Select(h => h.ToInt64()).ToList(),
            };
            SendResponse(response);
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var match = PluginManager.Instance.AvailablePlugins.Select(p => p.Assembly).SingleOrDefault(a => a.GetName().FullName == args.Name);
            if (match != null)
            {
                return Assembly.LoadFile(match.Location);
            }
            return null;
        }
    }
}
