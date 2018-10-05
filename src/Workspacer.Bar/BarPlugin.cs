﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Workspacer.Bar
{
    public class BarPlugin : IPlugin
    {
        private List<BarForm> _bars;
        private BarPluginConfig _config;

        public BarPlugin()
        {
            _config = new BarPluginConfig();
        }

        public BarPlugin(BarPluginConfig config)
        {
            _config = config;
        }

        private class MyAppContext : ApplicationContext
        {
            public MyAppContext(BarPluginConfig config, IConfigContext context)
            {
                var bars = new List<BarForm>();

                foreach (var m in context.Workspaces.Monitors)
                {
                    var bar = new BarForm(m, config);

                    var left = config.LeftWidgets();
                    var right = config.RightWidgets();

                    bar.Initialize(left, right, context);

                    bar.Show();
                    bars.Add(bar);
                }
            }

            
        }

        public void AfterConfig(IConfigContext context)
        {
            Task.Run(() =>
            {
                Application.EnableVisualStyles();
                Application.Run(new MyAppContext(_config, context));
            });
        }

        public ILayoutEngine[] WrapLayouts(params ILayoutEngine[] inners)
        {
            return inners.Select(i => new MenuBarLayoutEngine(i, _config.BarTitle, _config.BarHeight)).ToArray();
        }
    }
}
