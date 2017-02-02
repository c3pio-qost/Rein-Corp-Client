using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RCHOOKLIB
{
    public static partial class HookMng
    {
        #region set constant and import function from dll
        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int SetWindowsHookEx(int idHook, RCH_Proc_Delegate lpfn, IntPtr hMod, int dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int CallNextHookEx(int hhk, int nCode, int wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(int idHook);
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseStruct
        {
            public Point Point;
            public int Mouse_Data;
            public int Flags;
            public int Time;
            public int Extra_Info;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardStruct
        {
            public int Virtual_Key;
            public int Scan_Code;
            public int Flags;
            public int Time;
            public int Extra_Info;
        }
    }
}
