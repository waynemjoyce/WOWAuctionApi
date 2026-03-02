using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows;

namespace WOWApi
{
    class Win32
    {
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CursorInfo
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public Point ptScreenPos;
        }

        public enum keyState
        {
            KEYDOWN = 0,
            EXTENDEDKEY = 1,
            KEYUP = 2
        };

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CursorInfo pci);

        [DllImport("user32.dll")]
        private static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        [DllImport("user32.dll")]
        private static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(
                  IntPtr hWnd,      // handle to destination window
                  uint Msg,       // message
                  IntPtr wParam,  // first message parameter
                  IntPtr lParam   // second message parameter
                  );

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator System.Windows.Point(POINT point)
            {
                return new System.Windows.Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);



        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_MOVE = 0x0001;


        private const uint WM_LBUTTONDOWN = 513;
        private const uint WM_LBUTTONUP = 514;

        private const uint WM_RBUTTONDOWN = 516;
        private const uint WM_RBUTTONUP = 517;

        public static System.Windows.Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            // NOTE: If you need error handling
            // bool success = GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

        public static Rectangle GetWowRectangle()
        {
            IntPtr Wow = FindWindow("GxWindowClass", "World Of Warcraft");
            Rect Win32ApiRect = new Rect();
            GetWindowRect(Wow, ref Win32ApiRect);
            Rectangle myRect = new Rectangle();
            myRect.X = Win32ApiRect.Left;
            myRect.Y = Win32ApiRect.Top;
            myRect.Width = (Win32ApiRect.Right - Win32ApiRect.Left);
            myRect.Height = (Win32ApiRect.Bottom - Win32ApiRect.Top);
            return myRect;
        }

        public static Bitmap GetCursorIcon(CursorInfo actualCursor, int width = 35, int height = 35)
        {
            Bitmap actualCursorIcon = null;

            try
            {
                actualCursorIcon = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(actualCursorIcon);
                Win32.DrawIcon(g.GetHdc(), 0, 0, actualCursor.hCursor);
                g.ReleaseHdc();
            }
            catch (Exception) { }

            return actualCursorIcon;
        }

        static public void ActivateWow()
        {
            ActivateApp("WoW");
            ActivateApp("WoW-64");
            ActivateApp("World Of Warcraft");
        }

        static public void ActivateWow(int processId)
        {
            ActivateApp(processId);
        }

        static public void ActivateApp(string processName)
        {
            Process[] p = Process.GetProcessesByName(processName);
            
            // Activate the first application we find with this name
            if (p.Count() > 0)
            {
                SetForegroundWindow(p[0].MainWindowHandle);
            }
        }

        static public int CheckProcessId(int processId)
        {
            int returnVal = 0;

            foreach (Process p in Process.GetProcessesByName("Wow"))
            {
               if (p.Id == processId)
               {
                    returnVal = p.Id;
                    break;
               }
            }

            return returnVal;
        }

        static public void ActivateThis()
        {
            Win32.ActivateApp(Process.GetCurrentProcess());
        }

        static public int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        static public Process[] GetProcesses(string processName)
        {
            return Process.GetProcessesByName(processName);
        }

        static public bool ProcessExists(int processId)
        {
            try
            {
                Process test = Process.GetProcessById(processId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static public void ActivateApp(int processId)
        {
            ActivateApp(Process.GetProcessById(processId));
        }

        static public void ActivateApp(Process WowProcess)
        {
            SetForegroundWindow(WowProcess.MainWindowHandle);
        }

        static public void SetForegroundByHandle(IntPtr handle)
        {
            SetForegroundWindow(handle);
        }

        public static void MoveMouse(int x, int y)
        {
            if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
        }

        public static CursorInfo GetNoFishCursor()
        {
            Rectangle WoWRect = Win32.GetWowRectangle();
            Win32.MoveMouse((WoWRect.X + 10), (WoWRect.Y + 45));
            LastRectX = WoWRect.X;
            LastRectY = WoWRect.Y;
            Thread.Sleep(15);
            CursorInfo myInfo = new CursorInfo();
            myInfo.cbSize = Marshal.SizeOf(myInfo);
            GetCursorInfo(out myInfo);
            return myInfo;
        }

        public static CursorInfo GetCurrentCursor()
        {
            CursorInfo myInfo = new CursorInfo();
            myInfo.cbSize = Marshal.SizeOf(myInfo);
            GetCursorInfo(out myInfo);
            return myInfo;
        }

        public static void SendKey(string sKeys)
        {
            SendKeys.SendWait(sKeys);
        }

        public static void SendLeftMouseClick()
        {
            IntPtr Wow = GetForegroundWindow();
            long dWord = MakeDWord((LastX - LastRectX), (LastY - LastRectY));

            SendNotifyMessage(Wow, WM_LBUTTONDOWN, (UIntPtr)1, (IntPtr)dWord);
            Thread.Sleep(25);
            SendNotifyMessage(Wow, WM_LBUTTONUP, (UIntPtr)1, (IntPtr)dWord);
        }

        public static void SendRightMouseClick()
        {
            IntPtr Wow = GetForegroundWindow();
            long dWord = MakeDWord((LastX - LastRectX), (LastY - LastRectY));

            SendMessage(Wow, WM_RBUTTONDOWN, (IntPtr)1, (IntPtr)dWord);
            Thread.Sleep(25);
            SendMessage(Wow, WM_RBUTTONUP, (IntPtr)1, (IntPtr)dWord);
        }

        public static void MoveAndRightClick(int x, int y, int delayBetween)
        {
            /*if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
            System.Threading.Thread.Sleep(delayBetween);
            */
            if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
            System.Threading.Thread.Sleep(delayBetween);
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
        }

        public static void MoveAndRightClickDownOnly(int x, int y, int delayBetween)
        {
            /*if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
            System.Threading.Thread.Sleep(delayBetween);
            */
            if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
            System.Threading.Thread.Sleep(delayBetween);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0);
        }

        public static void MoveAndRightClickUpOnly(int x, int y, int delayBetween)
        {
            /*if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
            System.Threading.Thread.Sleep(delayBetween);
            */
            if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
            System.Threading.Thread.Sleep(delayBetween);
            mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
        }



        public static void MoveAndLeftClick(int x, int y, int delayBetween)
        {
            /*if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
            System.Threading.Thread.Sleep(delayBetween);
            */
            if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
            System.Threading.Thread.Sleep(delayBetween);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }


        public static void MouseEventRightClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        public static void MouseEventLeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static bool SendKeyboardAction(Keys key, keyState state)
        {
            return SendKeyboardAction((byte)key.GetHashCode(), state);
        }

        public static bool SendKeyboardAction(byte key, keyState state)
        {
            return keybd_event(key, 0, (uint)state, (UIntPtr)0);
        }

        private static long MakeDWord(int LoWord, int HiWord)
        {
            return (HiWord << 16) | (LoWord & 0xFFFF);
        }

        static private int LastRectX;
        static private int LastRectY;

        static private int LastX;
        static private int LastY;
    }
}
