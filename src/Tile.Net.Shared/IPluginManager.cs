﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tile.Net
{
    public interface IPluginManager
    {
        void RegisterPlugin<T>() where T : IPlugin;
        IEnumerable<Type> AvailablePlugins { get; }
    }
}
