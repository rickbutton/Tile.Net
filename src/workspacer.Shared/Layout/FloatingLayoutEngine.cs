using System.Collections.Generic;
using System.Linq;

namespace workspacer
{
    public class FloatingLayoutEngine : ILayoutEngine
    {
        // we need to account for the Bar height (which is by default 30 height), otherwise, windows will be "dragged down"
        private readonly int _barHeight;

        public FloatingLayoutEngine() : this(30) { }

        public FloatingLayoutEngine(int barHeight)
        {
            _barHeight = barHeight;
        }

        public IEnumerable<IWindowLocation> CalcLayout(IEnumerable<IWindow> windows, int spaceWidth, int spaceHeight)
        {
            var list = new List<IWindowLocation>();
            var numWindows = windows.Count();

            if (numWindows == 0)
                return list;

            var windowList = windows.ToList();

            for (var i = 0; i < numWindows; i++)
            {
                var window = windowList[i];

                list.Add(new WindowLocation(
                            window.Location.X-window.Offset.X,
                            window.Location.Y-window.Offset.Y-_barHeight,
                            window.Location.Width-window.Offset.Width,
                            window.Location.Height-window.Offset.Height,
                            window.Location.State
                ));
            }
            return list;
        }

        public string Name => "float";

        public void ShrinkPrimaryArea() { }
        public void ExpandPrimaryArea() { }
        public void ResetPrimaryArea() { }
        public void IncrementNumInPrimary() { }
        public void DecrementNumInPrimary() { }

    }
}
