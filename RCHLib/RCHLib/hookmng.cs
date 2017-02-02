using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RCHOOKLIB
{
    public static partial class HookMng
    {
        #region callback function
        /// Define type of universal delegate pattern 
        private delegate int RCH_Proc_Delegate(int nCode, int wParam, IntPtr lParam);

        /// Define keyboard and mouse delegate pattern
        public delegate void Keyboard_Event_Handler(KeyboardStruct e, int wParam);
        public delegate void Mouse_Event_Handler(MouseStruct e, int wParam);

        /// Delegates of API system hook
        private static RCH_Proc_Delegate Mouse_Callback;
        private static RCH_Proc_Delegate Keyboard_Callback;

        /// Set and Unset mouse hook
        private static event Mouse_Event_Handler _Mouse;
        public static event Mouse_Event_Handler Mouse                                                           //mouse event listener
        {
            add
            {
                _Mouse += value;
                set_mouse_hook();
            }
            remove
            {
                _Mouse -= value;
                unhook_mouse();
            }
        }
        
        /// Set and Unset keyboard hook
        private static event Keyboard_Event_Handler _Keyboard;
        public static event Keyboard_Event_Handler Keyboard                                                     //keyboard event listener
        {
            add 
            {
                _Keyboard += value;
                set_keyboard_hook();
            }
            remove 
            {
                _Keyboard -= value;
                unhook_keyboard();
            }
        }

        //raise function every mouse hook
        private static int RCH_Mouse_Hook_Proc(int nCode, int wParam, IntPtr lParam)
        {
            MouseStruct m_struct = (MouseStruct)Marshal.PtrToStructure(lParam, typeof(MouseStruct));
            _Mouse.Invoke(m_struct, wParam);
            return CallNextHookEx(RCH_Mouse_Handler, nCode, wParam, lParam);
        }

        //raise function every keyboard hook
        private static int RCH_Keyboard_Hook_Proc(int nCode, int wParam, IntPtr lParam)
        {
            KeyboardStruct k_struct = (KeyboardStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardStruct));
            _Keyboard.Invoke(k_struct,wParam);
            return CallNextHookEx(RCH_Keyboard_Handler, nCode, wParam, lParam);
        }

        #endregion
        #region hook operations
        private static int RCH_Mouse_Handler = 0;                                                               //mouse descriptor
        private static int RCH_Keyboard_Handler = 0;                                                            //keyboard descriptor
        
        private static void set_mouse_hook()
        {
            if(_Mouse != null)
            if (RCH_Mouse_Handler == 0)
            {
                Mouse_Callback = RCH_Mouse_Hook_Proc;
                RCH_Mouse_Handler = SetWindowsHookEx(
                    WH_MOUSE_LL, 
                    Mouse_Callback, 
                    IntPtr.Zero, 0
                    );                                                                                          //return descriptor or null

                if (RCH_Mouse_Handler == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }

        }

        private static void unhook_mouse()
        {
            if (RCH_Mouse_Handler != 0)
            {
                int result = UnhookWindowsHookEx(RCH_Mouse_Handler);
                RCH_Mouse_Handler = 0;
                Mouse_Callback = null;
                if (result == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static void set_keyboard_hook()
        {
            if (_Keyboard != null)
            {
                if (RCH_Keyboard_Handler == 0)
                {
                    Keyboard_Callback = RCH_Keyboard_Hook_Proc;
                    RCH_Keyboard_Handler = SetWindowsHookEx(
                        WH_KEYBOARD_LL, 
                        Keyboard_Callback, 
                        IntPtr.Zero, 0
                        );                                                                                      //return descriptor or null

                    if (RCH_Keyboard_Handler == 0)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode);
                    }
                }
            }
        }

        private static void unhook_keyboard()
        {
            if (RCH_Keyboard_Handler != 0)
            {
                int result = UnhookWindowsHookEx(RCH_Keyboard_Handler);
                RCH_Keyboard_Handler = 0;
                Keyboard_Callback = null;
                if (result == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }
        #endregion
    }
}
