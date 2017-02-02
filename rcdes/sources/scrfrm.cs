using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Controls;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace ReinCorpDesign
{
    public partial class scrfrm : Form
    {
        private float Scale;
        private UdpClient Client;
        private TcpClient Ctrl_Client;
        public Thread thr_One_To_One;
        public Thread thr_Cursor_Control;
        public Thread thr_Key_Control;
        private System.Drawing.Bitmap M_Scr_Cnvs;
        FrameworkElement Placement_Target;
        Window owner;
        protected TabItem Item;
        public PictureBox Screen_Box { get { return scr_container; } }
        public GlobalTypes.ClientInfo Tmp;
        private bool Flagok;
        private int Counter;
        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();
        public Queue<string> cursor_bufffer = new Queue<string>();
        public Queue<string> key_buffer = new Queue<string>();
        public scrfrm(FrameworkElement placementTarget, TabItem item, GlobalTypes.ClientInfo tmp)
        {
            InitializeComponent();
            var color = ((SolidColorBrush)XPorter.Bus.Main_Handle.MainTab.Background).Color;
            this.BackColor =System.Drawing.Color.FromArgb(color.R,color.G,color.B);
            this.FormClosing += delegate
            {
                MainWindow.thr_killer(thr_One_To_One);
                RCHOOKLIB.HookMng.Mouse -= mouse_hook_proc;
                RCHOOKLIB.HookMng.Keyboard -= keyboard_hook_proc;
            };

            //---------------------here was delegate -----------------------------//
            Placement_Target = placementTarget;
            Item = item;
            Tmp = tmp;
            owner = Window.GetWindow(placementTarget);
            owner.LocationChanged += delegate { on_size_and_location_changing(); };
            Placement_Target.SizeChanged += delegate { on_size_and_location_changing(); };
            Placement_Target.IsVisibleChanged += visible_changing;


            XPorter.Bus.Main_Handle.StateChanged += delegate
            {
                if (thr_One_To_One.ThreadState != ThreadState.Running)
                {
                    on_size_and_location_changing();
                    thr_One_To_One = new Thread(one_to_one);
                    thr_One_To_One.Start(Tmp);
                    this.Visible = true;
                }
            };
            if (item.IsVisible)
            {
                Show(new Wpf32Window(owner));
            }
        }
        public void change_mode_menu(object sender, RoutedEventArgs e)
        {
            if (!Flagok)
            {
                Tmp.HostInfo.ControlPort = RCConnectLibrary.port_dealer(Tmp.HostInfo.Addr,
                        new GlobalTypes.SysMessageStruct("PRT", 1), GlobalTypes.Settings.TCPPort + 1);
                RCConnectLibrary.send_to(
                    RCConnectLibrary.get_tcp(Tmp.HostInfo.Addr, GlobalTypes.Settings.TCPPort).GetStream(),
                    new GlobalTypes.SysMessageStruct("CTR", 0));
                Ctrl_Client = RCConnectLibrary.get_tcp(Tmp.HostInfo.Addr, Tmp.HostInfo.ControlPort);
                Counter = System.Environment.TickCount;
                thr_Cursor_Control = new Thread(cur_buf_sender);
                thr_Cursor_Control.Start();
                RCHOOKLIB.HookMng.Mouse += mouse_hook_proc;
                thr_Key_Control = new Thread(key_buf_sender);
                thr_Key_Control.Start();
                RCHOOKLIB.HookMng.Keyboard += keyboard_hook_proc;
                XPorter.Bus.Main_Handle.Menu_Mode.Header = "Normal Mode";
                Flagok = true;
            }
            else
            {
                RCHOOKLIB.HookMng.Mouse -= mouse_hook_proc;
                RCHOOKLIB.HookMng.Keyboard -= keyboard_hook_proc;
                RCConnectLibrary.send_to(RCConnectLibrary.get_tcp(Tmp.HostInfo.Addr, GlobalTypes.Settings.TCPPort).GetStream(),
                    new GlobalTypes.SysMessageStruct("CTR", 1));
                XPorter.Bus.Main_Handle.Menu_Mode.Header = "Control Mode";
                MainWindow.thr_killer(thr_Cursor_Control);
                Flagok = false;
                Ctrl_Client.Client.Shutdown(SocketShutdown.Both);
                Ctrl_Client.Close();
            }
        }

        public void cur_buf_sender(object sender)
        {
            try
            {
                Console.WriteLine("thread started");
                while (true)
                {
                    if (cursor_bufffer.Count > 0)
                    {
                        string tmp = cursor_bufffer.Dequeue();
                        Ctrl_Client.GetStream().Write(Encoding.UTF8.GetBytes(tmp), 0, tmp.Length);
                    }
                }
            }
            catch (Exception ex) { }
        }

        public void key_buf_sender(object sender)
        {
            try
            {
                while (true)
                {
                    if (key_buffer.Count > 0)
                    {
                        string tmp = key_buffer.Dequeue();
                        Ctrl_Client.GetStream().Write(Encoding.UTF8.GetBytes(tmp), 0, tmp.Length);
                    }
                }
            }
            catch (Exception ex) { }
        }

        public void mouse_hook_proc(RCHOOKLIB.HookMng.MouseStruct e, int wparam)
        {
            System.Drawing.Point send_point = Screen_Box.PointToClient(new System.Drawing.Point(e.Point.X, e.Point.Y));
            if((send_point.X>0)&&(send_point.X<Screen_Box.Width)&&(send_point.Y>0)&&(send_point.Y<Screen_Box.Height))
            {
                string str = "m "+ ((double)send_point.X / Screen_Box.Width).ToString() + " " +
                    ((double)send_point.Y / Screen_Box.Height).ToString() + " " + wparam.ToString() + " " +
                    (e.Mouse_Data >> 16).ToString() + " ";
                cursor_bufffer.Enqueue(str);
            }
        }

        public void keyboard_hook_proc(RCHOOKLIB.HookMng.KeyboardStruct e, int wparam)
        {
            string str = "k " + e.Virtual_Key.ToString() + " " + wparam.ToString()+" ";
            Console.WriteLine("k");
            key_buffer.Enqueue(str);
        }

        void visible_changing(object sender, DependencyPropertyChangedEventArgs e)
        {
            Visible = (bool)e.NewValue;
            if ((bool)e.NewValue)
            {
                XPorter.Bus.Main_Handle.Dispatcher.BeginInvoke(new MethodInvoker(delegate
                {
                    XPorter.Bus.Main_Handle.Menu_Mode.IsEnabled = true;
                    if (Flagok)
                    {
                        XPorter.Bus.Main_Handle.Menu_Mode.Header = "Normal Mode";
                    }
                    else
                    {
                        XPorter.Bus.Main_Handle.Menu_Mode.Header = "Control Mode";
                    }
                    XPorter.Bus.Main_Handle.Menu_Mode.Click += change_mode_menu;
                }));
                on_size_and_location_changing();
                thr_One_To_One = new Thread(one_to_one);
                thr_One_To_One.Start(Tmp);
            }
            else
            {
                XPorter.Bus.Main_Handle.Dispatcher.BeginInvoke(new MethodInvoker(delegate
                    {
                        XPorter.Bus.Main_Handle.Menu_Mode.Header = "Control Mode";
                        XPorter.Bus.Main_Handle.Menu_Mode.Click -= change_mode_menu;
                        XPorter.Bus.Main_Handle.Menu_Mode.IsEnabled = false;
                    }));
            }
        }

        void on_size_and_location_changing()
        {
            if ((XPorter.Bus.Main_Handle.MainTab.SelectedItem == Item) && (this.Visible))
            {
                System.Windows.Point offset = Placement_Target.TranslatePoint(new System.Windows.Point(), owner);
                System.Windows.Point size = new System.Windows.Point(Placement_Target.ActualWidth, Placement_Target.ActualHeight);
                HwndSource hwndSource = (HwndSource)HwndSource.FromVisual(owner);
                CompositionTarget ct = hwndSource.CompositionTarget;
                offset = ct.TransformToDevice.Transform(offset);
                size = ct.TransformToDevice.Transform(size);
                Win32.POINT screenLocation = new Win32.POINT(offset);
                Win32.ClientToScreen(hwndSource.Handle, ref screenLocation);
                Win32.POINT screenSize = new Win32.POINT(size);
                Win32.MoveWindow(Handle, screenLocation.X, screenLocation.Y, screenSize.X, screenSize.Y, true);
            }
            //if (this.Width < 20) Visible = false;
            //else
            //    Visible = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            base.OnClosing(e);
            if (!e.Cancel)
                owner.Dispatcher.BeginInvoke((Action)delegate
                {
                    owner.Close();
                });
        }

        //function for paint all received pictures
        public void show_pic(Bitmap bmp, int source_x, int source_y)
        {
            float scale_x = (float)Width / (bmp.Width * 8);
            float scale_y = (float)Height / (bmp.Height * 8);

            if (scale_x > scale_y)
            {
                Scale = scale_y;
            }
            else
                Scale = scale_x;

            if (M_Scr_Cnvs == null)
            {
                M_Scr_Cnvs = new Bitmap((int)(bmp.Width * 8 * Scale), (int)(bmp.Height * 8 * Scale));
            }

            int width = (int)(bmp.Width * 8 * Scale);
            int height = (int)(bmp.Height * 8 * Scale);

            if ((M_Scr_Cnvs.Width != width) || (M_Scr_Cnvs.Height != height))
            {
                Bitmap tmp_bitmap = new Bitmap(M_Scr_Cnvs);
                M_Scr_Cnvs = new Bitmap(tmp_bitmap, width, height);
            }

            int x = (int)(source_x * Scale);
            int y = (int)(source_y * Scale);
            Graphics gr = Graphics.FromImage(M_Scr_Cnvs);

            System.Drawing.Rectangle Destination_Rect = new System.Drawing.Rectangle(
                    x,
                    y,
                    (int)(bmp.Width * Scale) + 2,
                    (int)(bmp.Height * Scale) + 2
                    );
            gr.DrawImage(bmp, Destination_Rect);
            bmp.Dispose();

            //FULLSCREEN = MainScreenCanvas.GetHbitmap();
            //BitmapSource bmpsrc;
            //bmpsrc = Imaging.CreateBitmapSourceFromHBitmap(
            //FULLSCREEN, 
            //IntPtr.Zero, 
            //Int32Rect.Empty,  
            //BitmapSizeOptions.FromEmptyOptions()
            //);
            //ScreenBox.Source = bmpsrc;
            //DeleteObject(FULLSCREEN);
            //FULLSCREEN = IntPtr.Zero;

            Screen_Box.Image = M_Scr_Cnvs;
        }

        public void one_to_one(Object Arg)
        {
            try
            {
                GlobalTypes.ClientInfo tmp = Arg as GlobalTypes.ClientInfo;
                Client = new UdpClient(tmp.HostInfo.ScreenPort);                                                                 //port for exchange picture in onetoone mode
                Client.Client.ReceiveTimeout = 3000;

                while (Visible)
                {
                    GlobalTypes.ScreenPart recent_struct = RCScreenClient.receive_from<GlobalTypes.ScreenPart>(Client);

                    //if(thr_One_To_One.ThreadState!=ThreadState.AbortRequested)
                    if (recent_struct.Buffer != null)
                    {
                        Bitmap bmp = RCScreenClient.buf_to_bitmap(recent_struct.Buffer);
                        Screen_Box.BeginInvoke(new MethodInvoker(delegate()                          //paint received picture
                                    {
                                        show_pic(
                                            bmp,
                                            recent_struct.Point.X,
                                            recent_struct.Point.Y
                                            );
                                    }));
                    }

                }
                Client.Client.Shutdown(SocketShutdown.Both);
                Client.Close();
            }
            catch (Exception ex) { }
        }
    }
    public class Wpf32Window : System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle { get; private set; }

        public Wpf32Window(Window wpfWindow)
        {
            Handle = new WindowInteropHelper(wpfWindow).Handle;
        }
    }
}