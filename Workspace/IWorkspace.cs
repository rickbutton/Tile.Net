﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tile.Net
{
    public interface IWorkspace
    {
        IEnumerable<IWindow> Windows { get; }
        void DoLayout();
    }
}
