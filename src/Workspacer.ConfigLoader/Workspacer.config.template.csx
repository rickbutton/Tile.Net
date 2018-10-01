﻿#r "WORKSPACER_PATH\Workspacer.Shared.dll"
#r "WORKSPACER_PATH\Workspacer.ConfigLoader.dll"
#r "WORKSPACER_PATH\plugins\Workspacer.Bar\Workspacer.Bar.dll"
#r "WORKSPACER_PATH\plugins\Workspacer.ActionMenu\Workspacer.ActionMenu.dll"

using System;
using System.Linq;
using Workspacer.Shared;
using Workspacer.ConfigLoader;
using Workspacer.Bar;
using Workspacer.Bar.Widgets;
using Workspacer.ActionMenu;
using System.Diagnostics;

namespace Workspacer.Config
{
  
	public class Config : IConfig
    {
        public void Configure(IConfigContext context)
        {
            var mod = KeyModifiers.LAlt;

            var barConfig = new BarPluginConfig()
            {
                LeftWidgets = () => new IBarWidget[] { new WorkspaceWidget(), new TextWidget(": "), new TitleWidget() },
                RightWidgets = () => new IBarWidget[] { new TimeWidget(), new ActiveLayoutWidget() },
            };
            context.Plugins.RegisterPlugin(new BarPlugin(barConfig));

            var actionMenu = new ActionMenuPlugin(new ActionMenuPluginConfig());
            context.Plugins.RegisterPlugin(actionMenu);

            Func<ILayoutEngine[]> createLayouts = () => new ILayoutEngine[]
            {
                barConfig.CreateWrapperLayout(new TallLayoutEngine(1, 0.5, 0.03)),
                barConfig.CreateWrapperLayout(new FullLayoutEngine()),
            };

            context.Workspaces.WindowFilterFunc = (window) => 
            {
                if (window.Title.Contains("Task Manager"))
                    return false;
                if (window.Title.Contains("Program Manager"))
                    return false;
                if (window.Process.Id == Process.GetCurrentProcess().Id)
                    return false;

                return true;
            };

            var container = new WorkspaceContainer(context);
            container.CreateWorkspace("one", createLayouts());
            container.CreateWorkspace("two", createLayouts());
            container.CreateWorkspace("three", createLayouts());
            container.CreateWorkspace("four", createLayouts());
            container.CreateWorkspace("five", createLayouts());
            context.Workspaces.Container = container;

            context.Keybinds.SubscribeDefaults(context, mod);

            var defaultMenu = actionMenu.CreateDefault(context).Get();
            context.Keybinds.Subscribe(mod, Keys.P, () => actionMenu.ShowMenu(defaultMenu));
        }
    }
}
