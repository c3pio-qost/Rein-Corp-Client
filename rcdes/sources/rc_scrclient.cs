 using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace ReinCorpDesign
{
    class RCScreenClient
    {
        #region Global Vars
        public static Thread thr_Get_Img_From_UDP;                                                  //threads of different mode of connections
        public static IntPtr Ref_Bitmap = IntPtr.Zero;
        public static TcpClient Keyboard_Client;
        #endregion

        public static void send_to<T>(UdpClient _client,IPEndPoint _remotePoint, T _msg)                            //send messages by UDP
        {
            XmlSerializer msg_serializer = new XmlSerializer(typeof(T));
            try
            {
                using (MemoryStream tmp_stream = new MemoryStream())
                {
                    msg_serializer.Serialize(tmp_stream, _msg);
                    _client.Send(
                        tmp_stream.ToArray(),
                        (int) tmp_stream.Length,
                        _remotePoint
                        );                    
                }
            }
            catch (SocketException ex) { }
        }

        //refresh tile
        public static void set_interval(int interval, Action func)
        {
            int counter = System.Environment.TickCount;
            func();
            if(System.Environment.TickCount-counter<interval)
            {
                Thread.Sleep(interval-(System.Environment.TickCount-counter));
            }
        }

        public static void refresh_iterator()
        {
            lock (MainWindow.Client_Catalog)
            {
                foreach (var item in MainWindow.Client_Catalog)
                {
                    if ((System.DateTime.Now - item.LastConnection).Minutes > 1)
                    {
                    }
                    Object[] args = new Object[2];
                    args[0] = item;
                    args[1] = new GlobalTypes.ServiceMessageStruct("GET", 0);
                    get_query(args);
                }
            }
        }

        public static T receive_from<T>(UdpClient _client)                                                          //receive messages from UDP
        {
            IPEndPoint remote_point = null;
            XmlSerializer msg_serializer = new XmlSerializer(typeof(T));
            try
            {
                byte[] buffer = _client.Receive(ref remote_point);
                using (MemoryStream tmp_stream = new MemoryStream(buffer))
                {
                    return (T)msg_serializer.Deserialize(tmp_stream);
                }
            }
            catch (Exception ex) { /*System.Windows.Forms.MessageBox.Show(ex.ToString());*/return default(T);  }
        }

        //return bmp-image from buffer of bytes
        public static Bitmap buf_to_bitmap(byte[] _buffer)
        {
            using(MemoryStream stream = new MemoryStream(_buffer))
            {
                return new Bitmap(stream);
            }
        }

        //function which performs in thread
        //public static void one_to_one(Object Arg)
        //{
        //    UdpClient client = new UdpClient(5012);                                                                 //port for exchange picture in onetoone mode
        //    client.Client.ReceiveTimeout = 10000;

        //    while (true)
        //    {
        //        GlobalTypes.ScreenPart recent_struct = receive_from<GlobalTypes.ScreenPart>(client);
        //        if (recent_struct.Buffer != null)
        //        {
        //            Bitmap bmp = buf_to_bitmap(recent_struct.Buffer);
        //            XPorter.Bus.Main_Handle.Dispatcher.Invoke(new MethodInvoker(delegate()                          //paint received picture
        //                {
        //                    XPorter.Bus.Main_Handle.show_pic(
        //                        bmp,
        //                        recent_struct.Point.X,
        //                        recent_struct.Point.Y
        //                        );
        //                }));
        //        }
        //    }
        //}

        public static void start_one_to_one (object Args)
        {
            GlobalTypes.ClientInfo tmp = Args as GlobalTypes.ClientInfo;
            if ((!RCConnectLibrary.Threads_Already_Started) && (tmp != null)) {
                RCConnectLibrary.send_to_all_sys_msg(new GlobalTypes.SysMessageStruct("SCR", 11));

                tmp.HostInfo.ScreenPort = RCConnectLibrary.port_dealer(tmp.HostInfo.Addr, new GlobalTypes.SysMessageStruct
            ("PRT", 0), 8268); //TCP port + 1
                RCConnectLibrary.Opened_Ports.Add(tmp.HostInfo.ScreenPort);
                //Выполнить проверку если порты принялись нулевые....если так то сервер выеживается
                if (tmp.HostInfo.ScreenPort != 0) {
                    XPorter.Bus.Main_Handle.Dispatcher.Invoke(new MethodInvoker(delegate()
                    {
                        var tab = XPorter.Bus.Main_Handle.uni_tab_creator(null, true);
                        tab.Unloaded += delegate
                        {
                            TcpClient client = RCConnectLibrary.get_tcp(tmp.HostInfo.Addr, GlobalTypes.Settings.TCPPort);
                            if (client != null) {
                                RCConnectLibrary.sys_msg_analyzer(
                                    client,
                                    null,
                                    new GlobalTypes.SysMessageStruct("SCR", 11)
                                    );
                                client.Close();
                            }
                            //MainWindow.thr_killer(RCScreenClient.thr_One_To_One);
                        };

                        tab.Loaded += delegate
                        {
                            Grid target_grid = new Grid();
                            tab.Content = target_grid;
                            tab.Header = (GlobalTypes.Settings.TabHeader == "IP") ?
                                tmp.HostInfo.Addr.ToString() : tmp.HostInfo.HostName;
                            scrfrm _win = new scrfrm(target_grid, tab, tmp);
                            ((FrameworkElement)tab.Template.FindName("Cross", tab)).MouseLeftButtonDown += delegate(
                                object ss,
                                MouseButtonEventArgs ee
                                )
                                {
                                    _win.Close();
                                    XPorter.Bus.Main_Handle.MainTab.Items.Remove(tab);
                                };


                            Bitmap bmp = new Bitmap(800, 600);
                            using (var gr = Graphics.FromImage(bmp)) {
                                gr.Clear(System.Drawing.Color.Black);
                            }
                            _win.Screen_Box.Image = bmp;
                        };
                        //_Win.GotFocus += delegate
                        //{
                        //    XPorter.Bus.MDI.curentAddress = tmp.HostInfo.Addr;
                        //    IPEndPoint point = new IPEndPoint(XPorter.Bus.MDI.curentAddress, 5002);
                        //    keyboardClient = new TcpClient();
                        //    keyboardClient.Connect(point);
                        //    RCHLib.RCHookManager.Keyboard += KeyboardControl;
                        //};
                        //_Win.LostFocus += delegate
                        //{
                        //   // keyboardClient.Close();
                        //    XPorter.Bus.MDI.curentAddress = null;
                        //    RCHLib.RCHookManager.Keyboard -= KeyboardControl;
                        //};


                        //IntPtr refBitmap = bmp.GetHbitmap();
                        //BitmapSource bmpsrc;
                        //bmpsrc = Imaging.CreateBitmapSourceFromHBitmap(
                        //refBitmap, 
                        //IntPtr.Zero, 
                        //Int32Rect.Empty, 
                        //BitmapSizeOptions.FromEmptyOptions()
                        //);
                        //MainWindow.ScreenBox.Source = bmpsrc;
                        //TAB_item.Content=MainWindow.ScreenBox;
                        //Grid.SetRowSpan(MainWindow.ScreenBox, 2);
                        //MainWindow.DeleteObject(refBitmap);
                    }));
                    MainWindow.thr_killer(RCScreenClient.thr_Get_Img_From_UDP);
                    TcpClient _client = RCConnectLibrary.get_tcp(tmp.HostInfo.Addr, GlobalTypes.Settings.TCPPort);
                    RCConnectLibrary.sys_msg_analyzer(_client, tmp, new GlobalTypes.SysMessageStruct("SCR", 10));
                    //Object[] args = new Object[2];
                    //args[0] = tmp;
                    //args[1] = new GlobalTypes.ServiceMessageStruct("GET", 3);
                    //RCScreenClient.get_query(args);
                }
                else {
                    System.Windows.Forms.MessageBox.Show("Lost Connection!");
                }
            }
        }

        private static void unicast(object sender, RoutedEventArgs e)
        {
            GlobalTypes.ClientInfo tmp = null;

            if (VisualEffectsAndComponents.Connector.Cntr == null)
            {
                foreach (var _tmp in MainWindow.Client_Catalog)
                {
                    if (_tmp.Image == (System.Windows.Controls.Image)sender)
                    {
                        tmp = _tmp;
                    }
                }
                Thread thr_tmp = new Thread(start_one_to_one);
                thr_tmp.Start(tmp);
            }
        }

        public static void reload_thmb_scr(byte[] _buffer, GlobalTypes.ClientInfo item)
        {
            try
            {
                Ref_Bitmap = buf_to_bitmap(_buffer).GetHbitmap();
                if (item.Image != null)
                {
                    XPorter.Bus.Main_Handle.Dispatcher.Invoke(new MethodInvoker(delegate
                    {
                        try
                        {
                            BitmapSource bmpsrc;
                            bmpsrc = Imaging.CreateBitmapSourceFromHBitmap(
                                Ref_Bitmap, 
                                IntPtr.Zero, 
                                Int32Rect.Empty, 
                                BitmapSizeOptions.FromEmptyOptions()
                                );
                            item.Image.Source = bmpsrc;
                            item.LastConnection = System.DateTime.Now;
                        }
                        catch (Exception ex) { }
                    }));
                }
            }
            catch (Exception ex) { }
            finally
            {
                if (Ref_Bitmap != IntPtr.Zero)
                {
                    MainWindow.DeleteObject(Ref_Bitmap);
                    Ref_Bitmap = IntPtr.Zero;
                }
            }
        }

        public static void paint_thmb_scr(GlobalTypes.ClientInfo item)
        {
            XPorter.Bus.Main_Handle.Dispatcher.Invoke(new MethodInvoker(delegate
                {
                    lock (MainWindow.Client_Catalog)
                    {
                        System.Windows.Controls.Image tmp = new System.Windows.Controls.Image();
                        tmp.MouseLeftButtonUp += unicast;
                        tmp.Stretch = Stretch.Fill;
                        tmp.Margin = new Thickness(12, 15, 12, 0);
                        tmp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        tmp.Width = 170;
                        tmp.Height = 100;
                        item.Image = tmp;
                        DockPanel pic_container = new DockPanel();
                        TextBlock name = new TextBlock();
                        name.FontSize = 12;
                        name.FontFamily = new System.Windows.Media.FontFamily("Arial");
                        name.MaxWidth = 170;
                        name.TextTrimming = TextTrimming.CharacterEllipsis;
                        name.Text = item.HostInfo.HostName;
                        name.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        pic_container.Children.Add(tmp);
                        pic_container.Children.Add(name);
                        name.Foreground = System.Windows.Media.Brushes.White;
                        tmp.SetValue(DockPanel.DockProperty, Dock.Top);
                        name.SetValue(DockPanel.DockProperty, Dock.Bottom);
                        tmp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        XPorter.Bus.Main_Handle.Scr_Cont.Children.Add(pic_container);
                        //В другой модуль Create context menu
                        System.Windows.Controls.ContextMenu menu = ReinCorpDesign.sources.UserContextMenu.get_contx_menu(item);
                        item.Image.ContextMenu = menu;
                    }
                }));

        }

        //get small picture into thumb
        public static void try_get_thumb(GlobalTypes.ClientInfo item, GlobalTypes.ServiceMessageStruct msg)
        {
            UdpClient client = null;
            client = new UdpClient();
            lock (MainWindow.Client_Catalog)
            {
                IPEndPoint rem_pnt = new IPEndPoint(item.HostInfo.Addr, 5011);
                send_to<GlobalTypes.ServiceMessageStruct>(client, rem_pnt, msg);
                rem_pnt = null;
                client.Client.ReceiveTimeout = 1000;
                byte[] buffer = client.Receive(ref rem_pnt);
                if (buffer != null)
                {
                    reload_thmb_scr(buffer, item);
                }
            }
        }

        ////try to get picture from one to one mode
        //public static void try_get_screen(GlobalTypes.ClientInfo item, GlobalTypes.ServiceMessageStruct msg)
        //{
        //    UdpClient client = null;
        //    if ((msg.Flag > 0) && (msg.Flag < 11))
        //    {
        //        client = new UdpClient(5012);
        //        IPEndPoint rem_pnt = new IPEndPoint(item.HostInfo.Addr, 5012);
        //        client.Client.ReceiveTimeout = 1000;
        //        client.Client.SendTimeout = 1000;
        //        for (int i = 0; i < 10; i++)
        //        {
        //            if (thr_One_To_One == null)
        //            {
        //                send_to<GlobalTypes.ServiceMessageStruct>(client, rem_pnt, msg);
        //                rem_pnt = null;
        //                byte[] buffer = client.Receive(ref rem_pnt);
        //                if (buffer != null)
        //                {
        //                    System.Windows.Forms.MessageBox.Show("TYeah");
        //                    thr_One_To_One = new Thread(one_to_one);
        //                    thr_One_To_One.SetApartmentState(ApartmentState.STA);
        //                    thr_One_To_One.Start();
        //                    break;
        //                }
        //            }
        //        }

        //        if (thr_One_To_One == null)
        //        {
        //            System.Windows.Forms.MessageBox.Show("Disable unicast connect");
        //        }
        //    }
        //}

        //function which get some data from UDP
        public static void get_query(Object Args)
        {
            UdpClient client = null;
            try
            {
                Object[] args = (Object[])Args;
                GlobalTypes.ClientInfo item = (GlobalTypes.ClientInfo)args[0];
                GlobalTypes.ServiceMessageStruct msg = (GlobalTypes.ServiceMessageStruct)args[1];
                switch (Encoding.UTF8.GetString(msg.Msg))
                {
                    case "GET":
                        switch (msg.Flag)
                        {
                            case 0:
                                try_get_thumb(item, msg);
                                break;
                            default:
                                //try_get_screen(item, msg);
                                break;
                        }
                        break;
                }
            }
            catch (SocketException ex) { }
            catch (Exception ex1) { }
            finally
            {
                if(client!=null)
                client.Close();
            }
        }

        //thumb refresher
        public static void auto_tile_refr()
        {
            while ((RCConnectLibrary.Threads_Done < GlobalTypes.Settings.ThreadCount) &&
                (RCConnectLibrary.Threads_Done < RCConnectLibrary.Ip_Range_List.Count)) { }

            RCConnectLibrary.Threads_Already_Started = false;

            XPorter.Bus.Main_Handle.Dispatcher.BeginInvoke(new MethodInvoker(delegate
            {
                if (VisualEffectsAndComponents.MainWindowVisualEffects.Curtain.Load_Curtain != null)
                {
                    ((System.Windows.Controls.Panel)XPorter.Bus.Main_Handle.Work_Page.Content).Children.Remove(
                        VisualEffectsAndComponents.MainWindowVisualEffects.Curtain.Load_Curtain
                        );
                    VisualEffectsAndComponents.MainWindowVisualEffects.Curtain.Load_Curtain = null;
                }
                XPorter.Bus.Main_Handle.StatusBarText.Text = "Done";
            }));

            if (RCConnectLibrary.Abort_Thread)
            {
                RCConnectLibrary.Threads_Already_Started = false;
                RCConnectLibrary.Abort_Thread = false;
                RCConnectLibrary.Multi_Reconnect.Set();
                return;
            }
            GC.Collect();
            while (MainWindow.Client_Catalog.Count > 0)
            {
                set_interval(GlobalTypes.Settings.Interval,refresh_iterator);
            }
        }
    }
}