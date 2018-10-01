﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Workspacer.ActionMenu
{
    public class ActionMenuPlugin : IPlugin
    {
        private ActionMenuForm _menu;
        private ActionMenuPluginConfig _config;

        public ActionMenuPlugin(ActionMenuPluginConfig config)
        {
            _config = config;
        }

        public void AfterConfig(IConfigContext context)
        {
            _menu = new ActionMenuForm(context, _config);
        }

        public void ShowMenu(string message, ActionMenuItem[] items)
        {
            _menu.SetItems(message, items);
            _menu.Show();
            _menu.Activate();
        }

        public void ShowMenu(ActionMenuItem[] items)
        {
            _menu.SetItems("", items);
            _menu.Show();
            _menu.Activate();
        }

        public void ShowFreeForm(string message, Action<string> callback)
        {
            _menu.SetFreeForm(message, callback);
            _menu.Show();
            _menu.Activate();
        }

        public ActionMenuItemBuilder CreateDefault(IConfigContext context)
        {
            return new ActionMenuItemBuilder(this)
                .Add("restart workspacer", () => context.Restart())
                .Add("quit workspacer", () => context.Quit());
        }

        public ActionMenuItemBuilder Create()
        {
            return new ActionMenuItemBuilder(this);
        }
    }
}
