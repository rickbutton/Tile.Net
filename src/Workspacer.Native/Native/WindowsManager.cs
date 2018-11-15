﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Workspacer
{
    public delegate void WindowDelegate(IWindow window);
    public delegate void WindowUpdateDelegate(IWindow window, WindowUpdateType type);

    public class WindowsManager : IWindowsManager
    {
        private IDictionary<IntPtr, WindowsWindow> _windows;

        private WinEventDelegate _hookDelegate;

        public event WindowDelegate WindowCreated;
        public event WindowDelegate WindowDestroyed;
        public event WindowUpdateDelegate WindowUpdated;

        public IEnumerable<IWindow> Windows => _windows.Values;

        public WindowsManager()
        {
            _windows = new Dictionary<IntPtr, WindowsWindow>();
            _hookDelegate = new WinEventDelegate(WindowHook);
        }

        public void Initialize()
        {
            Win32.SetWinEventHook(Win32.EVENT_CONSTANTS.EVENT_OBJECT_DESTROY, Win32.EVENT_CONSTANTS.EVENT_OBJECT_SHOW, IntPtr.Zero, _hookDelegate, 0, 0, 0);
            Win32.SetWinEventHook(Win32.EVENT_CONSTANTS.EVENT_OBJECT_CLOAKED, Win32.EVENT_CONSTANTS.EVENT_OBJECT_UNCLOAKED, IntPtr.Zero, _hookDelegate, 0, 0, 0);
            Win32.SetWinEventHook(Win32.EVENT_CONSTANTS.EVENT_SYSTEM_MINIMIZESTART, Win32.EVENT_CONSTANTS.EVENT_SYSTEM_MINIMIZEEND, IntPtr.Zero, _hookDelegate, 0, 0, 0);
            Win32.SetWinEventHook(Win32.EVENT_CONSTANTS.EVENT_SYSTEM_MOVESIZESTART, Win32.EVENT_CONSTANTS.EVENT_SYSTEM_MOVESIZEEND, IntPtr.Zero, _hookDelegate, 0, 0, 0);
            Win32.SetWinEventHook(Win32.EVENT_CONSTANTS.EVENT_SYSTEM_FOREGROUND, Win32.EVENT_CONSTANTS.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _hookDelegate, 0, 0, 0);
            Win32.SetWinEventHook(Win32.EVENT_CONSTANTS.EVENT_OBJECT_NAMECHANGE, Win32.EVENT_CONSTANTS.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, _hookDelegate, 0, 0, 0);

            Win32.EnumWindows((handle, param) =>
            {
                if (Win32Helper.IsAppWindow(handle))
                {
                    RegisterWindow(handle, false);
                }
                return true;
            }, IntPtr.Zero);
        }

        public IWindowsDeferPosHandle DeferWindowsPos(int count)
        {
            var info = Win32.BeginDeferWindowPos(count);
            return new WindowsDeferPosHandle(info);
        }

        private void WindowHook(IntPtr hWinEventHook, Win32.EVENT_CONSTANTS eventType, IntPtr hwnd, Win32.OBJID idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (EventWindowIsValid(idChild, idObject, hwnd))
            {
                switch (eventType)
                {
                    case Win32.EVENT_CONSTANTS.EVENT_OBJECT_SHOW:
                        RegisterWindow(hwnd);
                        break;
                    case Win32.EVENT_CONSTANTS.EVENT_OBJECT_DESTROY:
                        UnregisterWindow(hwnd);
                        break;
                    case Win32.EVENT_CONSTANTS.EVENT_OBJECT_CLOAKED:
                        UpdateWindow(hwnd, WindowUpdateType.Hide);
                        break;
                    case Win32.EVENT_CONSTANTS.EVENT_OBJECT_UNCLOAKED:
                        UpdateWindow(hwnd, WindowUpdateType.Show);
                        break;
                    case Win32.EVENT_CONSTANTS.EVENT_SYSTEM_MINIMIZESTART:
                        UpdateWindow(hwnd, WindowUpdateType.MinimizeStart);
                        break;
                    case Win32.EVENT_CONSTANTS.EVENT_SYSTEM_MINIMIZEEND:
                        UpdateWindow(hwnd, WindowUpdateType.MinimizeEnd);
                        break;
                    case Win32.EVENT_CONSTANTS.EVENT_SYSTEM_FOREGROUND:
                        UpdateWindow(hwnd, WindowUpdateType.Foreground);
                        break;
                    case Win32.EVENT_CONSTANTS.EVENT_OBJECT_NAMECHANGE:
                        UpdateWindow(hwnd, WindowUpdateType.TitleChange);
                        break;
                    case Win32.EVENT_CONSTANTS.EVENT_SYSTEM_MOVESIZESTART:
                        break;
                    case Win32.EVENT_CONSTANTS.EVENT_SYSTEM_MOVESIZEEND:
                        EndWindowMove(hwnd);
                        break;
                }
            }
        }

        private bool EventWindowIsValid(int idChild, Win32.OBJID idObject, IntPtr hwnd)
        {
            return idChild == Win32.CHILDID_SELF && idObject == Win32.OBJID.OBJID_WINDOW && hwnd != IntPtr.Zero;
        }
        
        private void RegisterWindow(IntPtr handle, bool emitEvent = true)
        {
            if (!_windows.ContainsKey(handle))
            {
                var window = new WindowsWindow(handle);
                _windows[handle] = window;

                if (emitEvent)
                {
                    WindowCreated?.Invoke(window);
                }
            }
        }

        private void UnregisterWindow(IntPtr handle)
        {
            if (_windows.ContainsKey(handle))
            {
                var window = _windows[handle];
                _windows.Remove(handle);
                WindowDestroyed?.Invoke(window);
            }
        }

        private void UpdateWindow(IntPtr handle, WindowUpdateType type)
        {
            if (type == WindowUpdateType.Show  && _windows.ContainsKey(handle))
            {
                var window = _windows[handle];
                WindowUpdated?.Invoke(window, type);
            }
            else if (type == WindowUpdateType.Show)
            {
                RegisterWindow(handle);
            }
            else if (type == WindowUpdateType.Hide && _windows.ContainsKey(handle))
            {
                var window = _windows[handle];
                if (!window.DidManualHide)
                {
                    UnregisterWindow(handle);
                } else
                {
                    WindowUpdated?.Invoke(window, type);
                }
            }
            else if (_windows.ContainsKey(handle))
            {
                var window = _windows[handle];
                WindowUpdated?.Invoke(window, type);
            }
        }

        private void EndWindowMove(IntPtr handle)
        {
            if (_windows.ContainsKey(handle))
            {
                var window = _windows[handle];
                WindowUpdated?.Invoke(window, WindowUpdateType.MoveEnd);
            }
        }
    }
}
