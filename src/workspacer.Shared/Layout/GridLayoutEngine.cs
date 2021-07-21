using System;
using System.Collections.Generic;
using System.Linq;

namespace workspacer
{
    public class GridLayoutEngine : ILayoutEngine
    {
        public string Name => "grid";

        public IEnumerable<IWindowLocation> CalcLayout(IEnumerable<IWindow> windows, int spaceWidth, int spaceHeight)
        {
            var list = new List<IWindowLocation>();
            var numWindows = windows.Count();

            if (numWindows == 0)
                return list;
 
            // use next nearest perfect square as base width
            var gridWidth = (int) Math.Ceiling(Math.Sqrt(numWindows));
            // minimize number of rows to fit the windows tightly in a grid (no empty rows).
            var gridHeight = (int) Math.Ceiling((double) numWindows / gridWidth);

            int width = spaceWidth / gridWidth;
            int height = spaceHeight / gridHeight;

            for (var i = 0; i < numWindows; i++)
            {
                list.Add(new WindowLocation(i % gridWidth * width, i / gridWidth * height, width, height, WindowState.Normal));
            }

            return list;
        }

        public void DecrementNumInPrimary() { }
        public void ExpandPrimaryArea() { }
        public void IncrementNumInPrimary() { }
        public void ResetPrimaryArea() { }
        public void ShrinkPrimaryArea() { } 
    }
}
