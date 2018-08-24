﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tile.Net.Shared;

namespace Tile.Net
{
    class Program
    {
        public static void Main(string[] args)
        {
            Win32.SetProcessDPIAware();

            var app = new TileNet(args[0]);
            Thread.GetDomain().UnhandledException += ((s, e) =>
                {
                    Console.Write(e.ExceptionObject);
                    app.DoLauncherAction(LauncherAction.Quit);
                });

            app.Start();
        }
    }
}
