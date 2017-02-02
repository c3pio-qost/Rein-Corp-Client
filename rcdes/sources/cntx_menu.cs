using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Controls;

namespace ReinCorpDesign.sources
{
    class UserContextMenu
    {
        public static System.Windows.Controls.ContextMenu get_contx_menu(GlobalTypes.ClientInfo item)
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
                        shutdown.Click+= delegate
                        {
                            RCConnectLibrary.send_to(RCConnectLibrary.get_tcp(item.HostInfo.Addr, GlobalTypes.Settings.TCPPort).GetStream(),
                                new GlobalTypes.SysMessageStruct("SHT", 0));
                        };
                        _menu.Items.Add(shutdown);
                        System.Windows.Controls.MenuItem reboot = new System.Windows.Controls.MenuItem();
                        reboot.Header = "Reboot";
                        reboot.Click+= delegate
                        {
                            RCConnectLibrary.send_to(RCConnectLibrary.get_tcp(item.HostInfo.Addr, GlobalTypes.Settings.TCPPort).GetStream(),
                                new GlobalTypes.SysMessageStruct("SHT", 1));
                        };
                        _menu.Items.Add(reboot);
                        System.Windows.Controls.MenuItem hibernate = new System.Windows.Controls.MenuItem();
                        hibernate.Header = "Hibernate";
                        hibernate.Click+= delegate
                        {
                            RCConnectLibrary.send_to(RCConnectLibrary.get_tcp(item.HostInfo.Addr, GlobalTypes.Settings.TCPPort).GetStream(),
                                new GlobalTypes.SysMessageStruct("SHT", 2));
                        };
                        _menu.Items.Add(hibernate); 
                        System.Windows.Controls.MenuItem block = new System.Windows.Controls.MenuItem();
                        block.Header = "Lock System";
                        block.Click+= delegate
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
                                    foreach(var it in XPorter.Bus.Main_Handle.TreeUngroup.Items)
                                    {
                                        if (((TreeViewItem)it).Header == item.HostInfo.Addr.ToString())
                                        {
                                            comp_for_delete = it;
                                            break;
                                        }
                                    }
                                    XPorter.Bus.Main_Handle.TreeUngroup.Items.Remove(comp_for_delete);
                                    lock (MainWindow.Client_Catalog)
                                    {
                                        MainWindow.Client_Catalog.Remove(item);
                                    }
                                }));
                        };
                        _menu.Items.Add(remove_from_scan_list);
            return _menu;
        }
    }
}
