using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Net;
using System.IO;

namespace ReinCorpDesign.sources
{
    class UserContextMenu
    {
        public static string POST (string Url, string Data)
        {
            WebRequest req = WebRequest.Create(Url);
            req.Method = "POST";
            req.Timeout = 10000;
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] sentData = Encoding.GetEncoding(1251).GetBytes(Data);
            req.ContentLength = sentData.Length;
            Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();
            WebResponse res = req.GetResponse();
            Stream ReceiveStream = res.GetResponseStream();
            StreamReader sr = new StreamReader(ReceiveStream, Encoding.UTF8);
            //Кодировка указывается в зависимости от кодировки ответа сервера
            Char[] read = new Char[256];
            int count = sr.Read(read, 0, 256);
            string Out = String.Empty;
            while (count > 0) {
                String str = new String(read, 0, count);
                Out += str;
                count = sr.Read(read, 0, 256);
            }
            return Out;
        }

        public static System.Windows.Controls.ContextMenu get_contx_menu (GlobalTypes.ClientInfo item)
        {
            System.Windows.Controls.ContextMenu _menu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem connect = new System.Windows.Controls.MenuItem();
            connect.Header = "Connect";
            connect.Click += delegate
            {
                RCScreenClient.start_one_to_one(item);
            };
            _menu.Items.Add(connect);
            System.Windows.Controls.MenuItem shutdown = new System.Windows.Controls.MenuItem();
            shutdown.Header = "Shutdown";
            shutdown.Click += delegate
            {
                //RCConnectLibrary.send_to(RCConnectLibrary.get_tcp(item.HostInfo.Addr, GlobalTypes.Settings.TCPPort).GetStream(),
                    //new GlobalTypes.SysMessageStruct("SHT", 0));
            };
            _menu.Items.Add(shutdown);
            System.Windows.Controls.MenuItem reboot = new System.Windows.Controls.MenuItem();
            reboot.Header = "Reboot";
            reboot.Click += delegate
            {
                RCConnectLibrary.send_to(RCConnectLibrary.get_tcp(item.HostInfo.Addr, GlobalTypes.Settings.TCPPort).GetStream(),
                    new GlobalTypes.SysMessageStruct("SHT", 1));
            };
            _menu.Items.Add(reboot);
            System.Windows.Controls.MenuItem hibernate = new System.Windows.Controls.MenuItem();
            hibernate.Header = "Hibernate";
            hibernate.Click += delegate
            {
                RCConnectLibrary.send_to(RCConnectLibrary.get_tcp(item.HostInfo.Addr, GlobalTypes.Settings.TCPPort).GetStream(),
                    new GlobalTypes.SysMessageStruct("SHT", 2));
            };
            _menu.Items.Add(hibernate);
            System.Windows.Controls.MenuItem block = new System.Windows.Controls.MenuItem();
            block.Header = "Lock System";
            block.Click += delegate
            {
                RCConnectLibrary.send_to(RCConnectLibrary.get_tcp(item.HostInfo.Addr, GlobalTypes.Settings.TCPPort).GetStream(),
                    new GlobalTypes.SysMessageStruct("SHT", 3));
            };
            _menu.Items.Add(block);
            System.Windows.Controls.MenuItem remove_from_scan_list = new System.Windows.Controls.MenuItem();
            remove_from_scan_list.Header = "Remove From Scan List";
            remove_from_scan_list.Click += delegate
            {
                XPorter.Bus.Main_Handle.Dispatcher.BeginInvoke(new MethodInvoker(delegate
                    {
                        XPorter.Bus.Main_Handle.Scr_Cont.Children.Remove((DockPanel)item.Image.Parent);
                        Object comp_for_delete = null;
                        foreach (var it in XPorter.Bus.Main_Handle.TreeUngroup.Items) {
                            if (((TreeViewItem)it).Header == item.HostInfo.Addr.ToString()) {
                                comp_for_delete = it;
                                break;
                            }
                        }
                        XPorter.Bus.Main_Handle.TreeUngroup.Items.Remove(comp_for_delete);
                        lock (MainWindow.Client_Catalog) {
                            MainWindow.Client_Catalog.Remove(item);
                        }
                    }));
            };
            _menu.Items.Add(remove_from_scan_list);
            return _menu;
        }
    }
}