using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ReinCorpDesign
{
    public class RCSstructures
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct MTranferStruct
        {
            public double XCoord;
            public double YCoord;
            public int Flag;
            public int Data;

            public MTranferStruct(double x_coord, double y_coord, int flag, int data)
            {
                XCoord = x_coord;
                YCoord = y_coord;
                Flag = flag;
                Data = data;
            }
        }

        public enum InputType
        {
            MOUSE = 0,
            KEYBOARD = 1,
            HARDWARE = 2,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public INPUTUNION inputUnion;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION
        {
            // Fields
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [Serializable]
        public struct Point
        {
            public int X;
            public int Y;
        }
    }

    public class GlobalTypes
    {
        [Serializable]
        public struct ScreenPart
        {
            public Point Point;
            public byte[] Buffer;
        }

	public class SystemMessagesList
	{
	    string Msg;
	    List<IndexationClass> Flag;
	}

	private class IndexationClass
	{
	    byte Flag;
	    Delegate Del;
	}

        public class ServiceMessageStruct
        {
            public byte[] Msg;
            public byte Flag;
            public ServiceMessageStruct()
            {
                Msg = new byte[3];
                Flag = new byte();
            }

            public ServiceMessageStruct(string _Msg, byte _Flag)
            {
                Flag = _Flag;
                if (_Msg.Length == 3)
                {
                    Msg = Encoding.UTF8.GetBytes(_Msg);
                }
                else
                {
                    Msg = Encoding.UTF8.GetBytes(_Msg.Substring(0, 3));
                }
            }
        }

        public class HostInfo
        {
            public IPAddress Addr;
            public string HostName;
            public int ScreenPort;
            public int ControlPort;
            public HostInfo()
            {

            }

            public HostInfo(IPAddress _addr, byte[] _hostname)
            {
                Addr = _addr;
                HostName = Encoding.UTF8.GetString(_hostname);
            }

            public HostInfo(IPAddress _addr, string _hostname)
            {
                Addr = _addr;
                HostName = _hostname;
            }
        }
        public static string[] SysMsg = { "CNT", "PAS", "SYN", "ERR", "INF", "SCR"};
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class SysMessageStruct
        {
            public byte[] Msg;
            public byte Flag;
            public byte[] Info;
            public SysMessageStruct(string _Msg, byte _Flag, string _Info)
            {
                Flag = _Flag;
                if (_Info.Length <= 32)
                {
                    Info = Encoding.UTF8.GetBytes(_Info);
                }
                else
                {
                    Info = Encoding.UTF8.GetBytes(_Info.Substring(0, 32));
                }
                if (_Msg.Length <= 3)
                {
                    Msg = Encoding.UTF8.GetBytes(_Msg);
                }
                else
                {
                    Msg = Encoding.UTF8.GetBytes(_Msg.Substring(0, 3));
                }
            }

            public SysMessageStruct()
            {
                Msg = new byte[3];
                Flag = new byte();
                Info = new byte[32];
            }

            public SysMessageStruct(string _Msg, byte _Flag)
            {
                Flag = _Flag;
                Info = new byte[32];
                Array.Clear(Info, 0, 32);
                if (_Msg.Length <= 3)
                {
                    Msg = Encoding.UTF8.GetBytes(_Msg);
                }
                else
                {
                    Msg = Encoding.UTF8.GetBytes(_Msg.Substring(0, 3));
                }
            }
        }

        public class IPInterval
        {
            public IPAddress lvalue;
            public IPAddress rvalue;
            public IPInterval(string _lvalue, string _rvalue)
            {
                if ((IPAddress.TryParse(_lvalue, out lvalue)) && (IPAddress.TryParse(_rvalue, out rvalue)))
                {

                }
                else
                {
                    lvalue = null;
                    rvalue = null;
                }
            }
        }

        public static class Settings
        {
            public static List<IPInterval> IPCatalog = new List<IPInterval>();
            public static int ThreadCount;
            public static int TimeToConnect;
            public static string Password;
            public static string TabHeader;
            public static int Interval;
            public static int TCPPort;
        }

        public class ClientInfo
        {
            public HostInfo HostInfo;
            public System.Windows.Controls.Image Image;
            public DateTime LastConnection;
            public ClientInfo(HostInfo _hostinfo)
            {
                HostInfo = _hostinfo;
                Image = null;
                LastConnection = default(DateTime);
            }
            public ClientInfo()
            {
                HostInfo = default(HostInfo);
                Image = null;
                LastConnection = default(DateTime);
            }
        }
    }
}
