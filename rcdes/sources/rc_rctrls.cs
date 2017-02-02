using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace ReinCorpDesign
{
    //class RCKeyboard
    //{
    //    #region Imports
    //    [DllImport("user32.dll")]
    //    static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] RCSstructures.INPUT[] pInputs, Int32 cbSize);
    //    #endregion
    //    public static IntPtr hForm;

    //    public static void ServerKeyboardThreadF(object SPH)
    //    {
    //        MainForm f = (MainForm)Control.FromHandle((IntPtr)SPH);
    //        bool error = false;
    //        while (!error) {
    //            byte[] buffer = new byte[1024];
    //            try {
    //                MainForm.KeyClient.Receive(buffer);
    //            }
    //            catch (SocketException ex) {
    //            }
    //            string mes = System.Text.Encoding.UTF8.GetString(buffer);
    //            //MessageBox.Show(mes);
    //            /*try
    //            {
    //                if (f.InvokeRequired)
    //                    f.BeginInvoke(new MethodInvoker(delegate
    //                    {
    //                        f.ShowMouse(m,0);
    //                    }));
    //                else
    //                {
    //                    f.ShowMouse(m, 0);
    //                }
    //            }
    //            catch (Exception ex1)
    //            {
    //                MessageBox.Show(ex1.Message);
    //            }*/
    //            string[] arr = new string[4];
    //            arr = mes.Split(' ');
    //            //Send input mouse
    //            RCSstructures.INPUT[] inp = new RCSstructures.INPUT[1];
    //            inp[0].type = (int)RCSstructures.InputType.KEYBOARD;
    //            RCSstructures.KEYBDINPUT kb = new RCSstructures.KEYBDINPUT();
    //            kb.wVk = Convert.ToInt16(arr[0]);
    //            kb.dwFlags = Convert.ToInt16(arr[1]);
    //            //f.ShowMouse(ms);
    //            inp[0].inputUnion.ki = kb;
    //            SendInput(1, inp, Marshal.SizeOf(inp[0]));
    //        }
    //    }

    //    public static void ev_keyboard(RCHLib.RCHookManager.KeyboardRCHStruct m, int wParam)
    //    {
    //        ClientForm f = (ClientForm)Control.FromHandle(hForm);

    //        string mes = m.VirtualKey.ToString() + " " + m.Flags.ToString() + " ";
    //        //f.ShowMouse(mes);
    //        try {
    //            ClientForm.KeyClient.SendBufferSize = mes.Length;
    //            ClientForm.KeyClient.Send(Encoding.UTF8.GetBytes(mes));
    //        }
    //        catch (SocketException ex) {
    //            if ((ex.SocketErrorCode != SocketError.TimedOut) &&
    //                        (ex.SocketErrorCode != SocketError.WouldBlock))
    //                return;
    //            //MessageBox.Show("MouseClient failed");
    //        }
    //    }
    //}
}
