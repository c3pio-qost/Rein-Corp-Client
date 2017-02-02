using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace ReinCorpDesign
{
    class RCConnectLibrary
    {
        #region Global Vars
        public static ManualResetEvent Multi_Reconnect = new ManualResetEvent(true);
        public static List<IPAddress> Ip_Range_List = new List<IPAddress>();                                                //list of all ip addresses
        public static bool Threads_Already_Started = false;                                                                 //flag for wait end of multiconnect process
        public static bool Abort_Thread = false;
        public static byte Threads_Done = 0;                                                                                //number of ended threads
        public static List<int> Opened_Ports = new List<int>();
        #endregion

        //function which send msg by TCP protocol to all users
        public static void send_to_all_sys_msg(GlobalTypes.SysMessageStruct msg)
        {
            if (!Threads_Already_Started)                                                                                   //if user push multiconnect a couple of time
            {
                MainWindow.thr_killer(RCScreenClient.thr_Get_Img_From_UDP);
                foreach (var item in MainWindow.Client_Catalog)
                {
                    TcpClient client = get_tcp(item.HostInfo.Addr, GlobalTypes.Settings.TCPPort);
                    if(client!=null)
                    {
                        send_to<GlobalTypes.SysMessageStruct>(client.GetStream(), msg);                                                               //analyze msg for further actions
                        client.Close();
                    }
                }
            }
        }

        //function which send serialized msgs by TCP protocol to one user
        public static void send_to<T>(NetworkStream net_stream, T msg)
        {
            XmlSerializer msg_serializer = new XmlSerializer(typeof(T));
            try
            {
                using (MemoryStream tmp_stream = new MemoryStream())
                {
                    msg_serializer.Serialize(tmp_stream, msg);
                    net_stream.Write(tmp_stream.ToArray(), 0, (int)tmp_stream.Length);                                      //send the msg
                }
            }
            catch (SocketException ex)
            {
                /*MessageBox.Show(ex.ToString());*/
            }
        }

        //function which receive serialized msgs from TCP protocol
        public static T receive_from<T>(NetworkStream net_stream)
        {
            XmlSerializer msg_serializer = new XmlSerializer(typeof(T));
            T msg = default(T); 
            byte[] buffer = new byte[1024];
            try
            {
                net_stream.Read(buffer, 0, 1024);                                                                               //receive msg and try serialize it from stream
                if (buffer.Length>0)
                {
                    using (MemoryStream tmp_stream = new MemoryStream(buffer))
                    {
                        msg = (T)msg_serializer.Deserialize(tmp_stream);
                    }
                }
            }
            catch (Exception ex)
            {
                /*MessageBox.Show(ex.ToString());*/
                return msg;
            }
            return msg;
        }

        private static void cnt_elem_refr(GlobalTypes.HostInfo host_inf,
                                          GlobalTypes.SysMessageStruct msg,
                                          GlobalTypes.ClientInfo item, 
                                          bool flag)
        {
            if ((Encoding.UTF8.GetString(msg.Msg) == "SYN") && (msg.Flag == 2))
            {
                if (!flag)
                {
                    item = new GlobalTypes.ClientInfo(host_inf);
                    lock (MainWindow.Client_Catalog)
                    {
                        MainWindow.Client_Catalog.Add(item);
                    }
                    RCScreenClient.paint_thmb_scr(item);
                }
                else
                {
                    lock (MainWindow.Client_Catalog)
                    {
                        item.HostInfo = host_inf;
                        item.LastConnection = DateTime.Now;
                    }
                }
                    lock (XPorter.Bus.Main_Handle.TreeUngroup)
                    {
                        XPorter.Bus.Main_Handle.TreeUngroup.Dispatcher.BeginInvoke(new MethodInvoker(delegate()
                        {
                            System.Windows.Controls.ContextMenu item_menu = ReinCorpDesign.sources.UserContextMenu.get_contx_menu(item);
                            if (XPorter.Bus.Main_Handle.TreeUngroup.IsExpanded == false)
                            {
                                XPorter.Bus.Main_Handle.TreeUngroup.IsExpanded = true;
                            }
                            TreeViewItem tr_item = new TreeViewItem();
                            tr_item.ContextMenu = item_menu;
                            tr_item.Header = item.HostInfo.Addr.ToString();
                            tr_item.Tag = "3";
                            XPorter.Bus.Main_Handle.TreeUngroup.Items.Add(tr_item);
                        }));
                    }
                    Thread thr_tmp = new Thread(RCScreenClient.get_query);
                    Object[] args = new Object[2];
                    args[0] = item;
                    args[1] = new GlobalTypes.ServiceMessageStruct("GET", 0);
                    thr_tmp.Name = "get_query";
                    thr_tmp.SetApartmentState(ApartmentState.STA);
                    //thr_tmp.Start(args);
                }
        }

        public static int port_dealer(IPAddress address, GlobalTypes.SysMessageStruct msg, int start_port)
        {
            List<int> busy_ports = get_busy_port();
            byte counter = 0;
            int port = 0;
            while (counter < 10)
            {
                try
                {
                    TcpClient connection = get_tcp(address,GlobalTypes.Settings.TCPPort);
                    NetworkStream ns = connection.GetStream();
                    while ((busy_ports.IndexOf(start_port + counter) > -1)||(Opened_Ports.IndexOf(start_port+counter))>-1)
                    {
                        start_port++;
                    }
                    msg.Info = Encoding.UTF8.GetBytes((start_port + counter).ToString());
                    send_to<GlobalTypes.SysMessageStruct>(ns, msg);
                    GlobalTypes.SysMessageStruct referer = receive_from<GlobalTypes.SysMessageStruct>(ns);
                    if ((Encoding.UTF8.GetString(referer.Msg).Equals("SYN")) && (referer.Flag == 3))
                    {
                        port = start_port + counter;
                        return port;
                    }
                    if ((Encoding.UTF8.GetString(referer.Msg).Equals("ERR")) && (referer.Flag == 102))
                    {
                        counter++;
                    }
                    else
                    {
                        counter++;
                    }
                    connection.Client.Shutdown(SocketShutdown.Both);
                    connection.Close();
                }
                catch (SocketException ex) { counter++; break; }
                catch (Exception ex) { counter++; break; }
            }
            return port;
        }

        //analyze msg for futher action
        public static void sys_msg_analyzer(TcpClient tcp_client, GlobalTypes.ClientInfo item, GlobalTypes.SysMessageStruct main_msg)
        {
            try {
                NetworkStream net_stream = tcp_client.GetStream();
                send_to<GlobalTypes.SysMessageStruct>(net_stream, main_msg);
                switch (Encoding.UTF8.GetString(main_msg.Msg)) {
                    #region Screen
                    case "SCR": {
                            switch (main_msg.Flag) {
                                case 0: {
                                        tcp_client.Client.ReceiveTimeout = 5000;
                                        GlobalTypes.SysMessageStruct host_inf = receive_from<GlobalTypes.SysMessageStruct>(net_stream);
                                        GlobalTypes.SysMessageStruct msg = receive_from<GlobalTypes.SysMessageStruct>(net_stream);
                                        if ((host_inf != null) && (msg != null)) {
                                            cnt_elem_refr(new GlobalTypes.HostInfo(
                                            ((IPEndPoint)tcp_client.Client.RemoteEndPoint).Address,
                                            host_inf.Info), msg, item, item != null);
                                        }
                                        else {
                                            MessageBox.Show("Не удалось установить соединение"); //Дописать в лог
                                        }
                                    } break;
                                case 11: {

                                    }
                                    break;
                                default: {
                                        if ((main_msg.Flag > 0) && (main_msg.Flag < 11)) {
                                        }
                                    } break;
                            }
                        } break;
                    #endregion
                }
            }
            catch (Exception ex) { MessageBox.Show("Lost Connection"); }
        }

	//get all busied port registered in the System
        private static List<int> get_busy_port()
        {
            List<int> port_arr = new List<int>();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] info_con = properties.GetActiveTcpConnections();
            foreach (var item in info_con)
            {
                if ((item.LocalEndPoint.Port > 8267) && (port_arr.IndexOf(item.LocalEndPoint.Port) < 0))
                    port_arr.Add(item.LocalEndPoint.Port);
            }
            IPEndPoint[] info_t_l = properties.GetActiveTcpListeners();
            foreach (var item in info_t_l)
            {
                if ((item.Port > 8267) && (port_arr.IndexOf(item.Port) < 0))
                    port_arr.Add(item.Port);
            }
            IPEndPoint[] info_u_l = properties.GetActiveUdpListeners();
            foreach (var item in info_u_l)
            {
                if ((item.Port > 8267) && (port_arr.IndexOf(item.Port) < 0))
                    port_arr.Add(item.Port);
            }
            port_arr.Sort();
            return port_arr;
        }

        //create connection between two point
        public static TcpClient cnt_estblsh(TcpClient tcp_client)
        {
            tcp_client.Client.ReceiveTimeout = 5000;
            tcp_client.Client.SendTimeout = 5000;
                try
                {
                    GlobalTypes.SysMessageStruct msg = new GlobalTypes.SysMessageStruct();
                    NetworkStream net_stream = tcp_client.GetStream();
                    send_to<GlobalTypes.SysMessageStruct>(net_stream, new GlobalTypes.SysMessageStruct("CNT", 0));
                    msg = receive_from<GlobalTypes.SysMessageStruct>(net_stream);

                    if ((Encoding.UTF8.GetString(msg.Msg) == "PAS") && (msg.Flag == 0))
                    {
                        send_to<GlobalTypes.SysMessageStruct>(net_stream, new GlobalTypes.SysMessageStruct("PAS", 1, GlobalTypes.Settings.Password));
                        msg = receive_from<GlobalTypes.SysMessageStruct>(net_stream);

                        if ((Encoding.UTF8.GetString(msg.Msg) == "SYN"))
                        {
                            switch (msg.Flag)
                            {
                                case 1:
                                    return tcp_client;
                                            break;
                                case 100:
                                    MessageBox.Show(DateTime.Now.ToString() + " IPAddress:" + 
                                        ((IPEndPoint)tcp_client.Client.RemoteEndPoint).Address.ToString() + " - Wrong Password"
                                        );
                                    return null;
                                    //Сюда создать логи
                                    break;
                                default:
                                    return null;
                                    break;
                            }
                        }
                    }
                }
                catch (SocketException ex) { /*MessageBox.Show(ex.ToString()); */}
                catch (Exception ex) { /*MessageBox.Show(ex.ToString());*/ }
                return null;
        }

        //enumerate range of addresses and add it to catalog
        public static IEnumerable<IPAddress> enum_range_ip(IPAddress lvalue, IPAddress rvalue)
        {
            var buffer = lvalue.GetAddressBytes();
            do
            {
                yield return lvalue = new IPAddress(buffer);
                int i = buffer.Length - 1;
                while (i >= 0 && ++buffer[i] == 0) i--;
            } while (!lvalue.Equals(rvalue));
        }

        public static void form_ip_list(GlobalTypes.IPInterval interval)
        {
            if((Convert.ToInt32(String.Concat(interval.lvalue.ToString().Split('.'))))>=                                        //check addresses [if(addr1>=addr2)]
                (Convert.ToInt32(String.Concat(interval.lvalue.ToString().Split('.')))))
            {
                foreach (var item in enum_range_ip(interval.lvalue, interval.rvalue))
                {
                    Ip_Range_List.Add(item);
                }
            }
        }

        public static void set_default_settings()
        {
            GlobalTypes.Settings.IPCatalog.Clear();
            GlobalTypes.Settings.ThreadCount = 2;
            GlobalTypes.Settings.TimeToConnect = 1000;
            GlobalTypes.Settings.Password = "123qwe";
            GlobalTypes.Settings.TabHeader = "HostName";
            GlobalTypes.Settings.Interval = 1000;
            GlobalTypes.Settings.TCPPort = 8267;
            GlobalTypes.Settings.IPCatalog.Add(new GlobalTypes.IPInterval("192.168.43.79", "192.168.43.79"));
        }

        public static void multi_cnt()
        {
            if (Abort_Thread) return;
            if (Threads_Already_Started)
            {
                Abort_Thread = true;
                Multi_Reconnect.Reset();
                Multi_Reconnect.WaitOne();
            }
            Threads_Already_Started = true;

            MainWindow.thr_killer(RCScreenClient.thr_Get_Img_From_UDP);
            Ip_Range_List.Clear();
            Threads_Done = 0;
            GC.Collect();
            for (int i = 0; i < GlobalTypes.Settings.IPCatalog.Count; i++)                                                      //create ip-address list from range
            {
                form_ip_list(GlobalTypes.Settings.IPCatalog[i]);
            }

            RCScreenClient.thr_Get_Img_From_UDP = new Thread(RCScreenClient.auto_tile_refr);
            RCScreenClient.thr_Get_Img_From_UDP.Name = "AutoTileRefresher";
            RCScreenClient.thr_Get_Img_From_UDP.Start();
            for (int i = 0; (i < GlobalTypes.Settings.ThreadCount)&&(i<Ip_Range_List.Count); i++)
            {
                    Thread thr_tmp = new Thread(cnt_cycle);
                    thr_tmp.Name = "Thread " + i.ToString();
                    thr_tmp.Start(i);
            }
        }

        public static TcpClient get_tcp(IPAddress ip, int port)
        {
            List<ManualResetEvent> event_res_list = new List<ManualResetEvent>();
            event_res_list.Add(new ManualResetEvent(false));                                                                    //TimeObject
            event_res_list.Add(new ManualResetEvent(false));                                                                    //WaitCloseTCP
            IntPtr fl_close_tcp = Marshal.AllocHGlobal(1);                                                                      //allocate mem to close_tcp_flag
            IntPtr fl_cnt_established = Marshal.AllocHGlobal(1);                                                                //allocate mem to cnt_establsh_flag
            Marshal.WriteByte(fl_close_tcp,0);
            Marshal.WriteByte(fl_cnt_established,0);
            TcpClient tcp_client = new TcpClient();
            Object[] args = new Object[4];                                                                                      //create list of args to send in param
            args[0] = tcp_client;
            args[1] = event_res_list;
            args[2] = fl_close_tcp;
            args[3] = fl_cnt_established;
            tcp_client.BeginConnect(ip, port, new AsyncCallback(cnt_clback), args);                                             //call async cnt and set clbck_func
            event_res_list[0].WaitOne(GlobalTypes.Settings.TimeToConnect, true);

            if(Marshal.ReadByte(fl_cnt_established)==0)                                                                         //cnt wasn't establised
            {
                Marshal.WriteByte(fl_close_tcp, 1);
                tcp_client.Close();
                event_res_list[1].WaitOne();
                return null;
            }
            else
            {
                return cnt_estblsh(tcp_client);
            }
        }

        //clbck function which call after connect or disable to cnt
        private static void cnt_clback(IAsyncResult IA)
        {
            Object[] args = (Object[])IA.AsyncState;
            TcpClient tcp_client = args[0] as TcpClient;
            List<ManualResetEvent> event_res_list = args[1] as List<ManualResetEvent>;
            IntPtr fl_close_tcp = (IntPtr)args[2];
            IntPtr fl_cnt_established = (IntPtr)args[3];
            try
            {
                if (Marshal.ReadByte(fl_close_tcp) == 0)
                {
                    if (tcp_client.Client != null)
                    {
                        tcp_client.EndConnect(IA);
                        Marshal.WriteByte(fl_cnt_established,1);
                    }
                }
            }
            catch (SocketException ex) { /*MessageBox.Show(ex.ToString()); */}
            catch (Exception ex) { /*MessageBox.Show(ex.ToString());*/ }
            finally
            {
                Marshal.FreeHGlobal(fl_close_tcp);
                event_res_list[0].Set();
                event_res_list[1].Set();
            }
        }

        public static GlobalTypes.ClientInfo find_item_in_cli_ctlg(IPAddress key)
        {
            lock (MainWindow.Client_Catalog)
            {
                foreach (var tmp in MainWindow.Client_Catalog)
                {
                    if (tmp.HostInfo.Addr.Equals(key))
                    {
                        return tmp;
                    }
                }
            }
            return null;
        }

        public static void cnt_cycle(object sender)
        {
            try
            {
                int thr_number = (int)sender;

                for (int i = 0; 
                    (i <= ((Ip_Range_List.Count - (thr_number + 1)) / GlobalTypes.Settings.ThreadCount) && 
                    (!Abort_Thread)); i++)
                {
                    int index = i * GlobalTypes.Settings.ThreadCount + thr_number;                                                  //pop address for current thread
                    TcpClient sys_cmd_client = get_tcp(Ip_Range_List[index],GlobalTypes.Settings.TCPPort);
                    GlobalTypes.ClientInfo item = find_item_in_cli_ctlg(Ip_Range_List[index]);
                    if (sys_cmd_client != null)
                    {
                        sys_msg_analyzer(sys_cmd_client, item, new GlobalTypes.SysMessageStruct("SCR", 0));
                        sys_cmd_client.Close();
                    }
                    else
                    {
                        if (item != null)
                        {
                            XPorter.Bus.Main_Handle.Dispatcher.BeginInvoke(
                               new MethodInvoker(delegate
                               {
                                   if ((DockPanel)item.Image.Parent != null)
                                   {
                                       XPorter.Bus.Main_Handle.Scr_Cont.Children.Remove((DockPanel)item.Image.Parent);
                                       lock (MainWindow.Client_Catalog)
                                       {
                                           MainWindow.Client_Catalog.Remove(item);
                                       }
                                       //Написать Мягкое обновление элементов TreeViewItems
                                       //lock (XPorter.Bus.MDI.Main_Handle.TreeUngroup)
                                       //{
                                       //    foreach(var it in XPorter.Bus.MDI.Main_Handle.TreeUngroup.Items)
                                       //    {
                                       //        if (((TreeViewItem)it).Header == item.HostInfo.Addr.ToString())
                                       //            XPorter.Bus.MDI.Main_Handle.TreeUngroup.Items.Remove(it);
                                       //    }
                                       //}
                                   }
                               }));
                        }
                    }
                }
                Threads_Done++;
            }
            catch (SocketException ex) { /*MessageBox.Show(ex.ToString());*/ }
            catch (Exception ex) { /*MessageBox.Show(ex.ToString()); */}
        }
    }
}