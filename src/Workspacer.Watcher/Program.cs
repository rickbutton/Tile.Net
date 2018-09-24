﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Workspacer.Shared;

namespace Workspacer.Watcher
{
    class Program
    {
        static void Main(string[] args)
        {
            List<long> activeHandles = null;

            using (var client = new PipeClient())
            {
                while (true)
                {
                    string line;
                    line = client.ReadLine();
                    LauncherResponse response = null;
                    try
                    {
                        response = JsonConvert.DeserializeObject<LauncherResponse>(line);

                        switch (response.Action)
                        {
                            case LauncherAction.Quit:
                                CleanupWindowHandles(activeHandles);
                                Quit();
                                break;
                            case LauncherAction.Restart:
                                CleanupWindowHandles(activeHandles);
                                Restart();
                                break;
                            case LauncherAction.UpdateHandles:
                                activeHandles = response.ActiveHandles;
                                break;
                            default:
                                throw new Exception(
                                    $"unknown Workspacer response action {response.Action.ToString()}");
                        }
                    }
                    catch (Exception e)
                    {
                        CleanupWindowHandles(activeHandles);
                        Quit();
                    }
                }
            }
        }

        static void CleanupWindowHandles(List<long> handles)
        {
            if (handles != null)
            {
                foreach (var handle in handles)
                {
                    var window = new WindowsWindow(new IntPtr(handle));
                    window.ShowInCurrentState();
                }
            }
        }

        static void Quit()
        {
            Environment.Exit(0);
        }

        static void Restart()
        {
            Process process = new Process();
            process.StartInfo.FileName = "Workspacer.exe";
            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            process.Start();
            Environment.Exit(0);
        }
    }
}
