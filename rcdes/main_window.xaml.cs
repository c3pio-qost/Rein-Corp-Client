/*****
 *       Reincorp Studio is Network Tools for System Administrators  
 *                                                                   
 *                                                                   
 *        Copyright (C)  2017
 *                                                                   
 *                                                                   
 * This program is free software; you can redistribute it and/or     
 * modify it under the terms of the GNU General Public License       
 * as published by the Free Software Foundation; either version 3    
 * of the License, or any later version.                             
 *                                                                   
 *                                                                   
 * This program is distributed in the hope that it will be useful,   
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of    
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the     
 * GNU General Public License for more details.                      
 *                                                                   
 * You should have received a copy of the GNU General Public License 
 * along with this program; if not, write to the Free Software       
 * RCgroup, Inc., Republic of Kazakhstan, Kostanay,                  
 ****/

 /*************************************************************
 *                                                            *
 * Included Library                                           *
 *                                                            *
 *                                                            *
 **************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Automation.Peers;
using System.Windows.Shell;
using System.Windows.Threading;
using MS.Win32;
using Microsoft.Win32;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Threading;
using System.Xml.Serialization;
using System.Resources;


namespace ReinCorpDesign
{
    //namespace to access from another module
    namespace XPorter
    {
        public static class Bus
        {
            public static System.Windows.Point Global_Offset_Tabs;
            public static System.Windows.Point Global_Size_Tabs;
            public static scrfrm Current_Form = null;                                                        //descriptor of current window above the tab
            public static IPAddress Current_Address;                                                        //return current address of connection
            public static ReinCorpDesign.MainWindow Main_Handle { get; set; }                               //descriptor of mainwindow
            public static int Bottom { get; set; }
            public static int Top { get; set; }
            public static double Inner_Width { get; set; }
            public static double Inner_Height { get; set; }
            public static string Current_User;
            public static string hardware_remote_address = "192.168.43.2";
            public static bool Is_Main_Menu_Enabled
            {
                get
                {
                    return XPorter.Bus.Main_Handle.MainMenu.IsEnabled;
                }
                set
                {
                    XPorter.Bus.Main_Handle.MainMenu.IsEnabled = (bool)value;
                }
            }

        }
    }
    
    public partial class MainWindow : Window
    {
        #region global vars and import
        //import function from another libs
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr Wparam, IntPtr Lparam);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);                                             //hard delete object from memory

        //constants
        private const UInt32 WM_SYSCOMMAND = 0x0112;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_RESTORE = 0xF120;

        //
        public static List<GlobalTypes.ClientInfo> Client_Catalog = new List<GlobalTypes.ClientInfo>();     //list of all connected clients include [addr,pic,hostname,connect_time]
        public static System.Windows.Controls.Image Scr_Box = new System.Windows.Controls.Image();
        public ResourceDictionary Dict_Res = new ResourceDictionary();                                      //all resource for current window
        public static IntPtr F_SCR = IntPtr.Zero;
        public WrapPanel Scr_Cont = null;
        public TabItem Work_Page = null;
        public Thread thr_Multi_Cnt;
        public static Grid Grid_In;
        public ContentControl Settings;
        public ContentControl About;
        public static TabControl TabSettings;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            User_Background.Background = System.Windows.Media.Brushes.Green;
            if (XPorter.Bus.Current_User.Length > 0) {
                User.Text = "    " + XPorter.Bus.Current_User + "    ";
            }
            //connect resources to window
            add_res_dict(new Uri(RCResourceLibs.vsresources.mainstylelib,  UriKind.Relative));                 //style of current window
            //add_res_dict(new Uri(RCResourceLibs.vsresources.menu_style,  UriKind.Relative));                 //style of menu
            add_res_dict(new Uri(RCResourceLibs.vsresources.scrl_vr_style, UriKind.Relative));                 //style of scrollbar
            add_res_dict(new Uri(RCResourceLibs.vsresources.tb_ctrl_style, UriKind.Relative));                 //style of tabcontrol
            add_res_dict(new Uri(RCResourceLibs.vsresources.tr_v_style,    UriKind.Relative));                 //style of treeview
        }

        //function retranslator resource from uri
        public int add_res_dict(Uri _uri)
        {
            using (MemoryStream _tmpStream = new MemoryStream(Encoding.UTF8.GetBytes(_uri.ToString())))
            {
                System.Windows.Markup.XamlReader xaml_reader = new System.Windows.Markup.XamlReader();
                try
                {
                    Dict_Res.MergedDictionaries.Add((ResourceDictionary)xaml_reader.LoadAsync(_tmpStream));
                }
                catch (Exception ex) { return 0; }
                this.Resources = Dict_Res;
            }
            return 1;
        }

        private void window_loaded(object sender, RoutedEventArgs e)
        {
            this.LocationChanged += delegate
            {
                XPorter.Bus.Global_Offset_Tabs = MainTab.TranslatePoint(new System.Windows.Point(), this);
                XPorter.Bus.Global_Offset_Tabs.Y += 28;
            };

            this.SizeChanged += delegate
            {
                XPorter.Bus.Global_Size_Tabs = new System.Windows.Point(MainTab.ActualWidth, 
                    MainTab.ActualHeight - 28);
            };

            XPorter.Bus.Global_Offset_Tabs = MainTab.TranslatePoint(new System.Windows.Point(), this);

            XPorter.Bus.Global_Size_Tabs = new 
                System.Windows.Point(MainTab.ActualWidth, MainTab.ActualHeight - 28);

            var element = ((Panel)MainTab.Template.FindName("HeaderPanel",MainTab));
            element.MouseLeftButtonDown+= main_tab_mouse_dbl_click;
            XPorter.Bus.Global_Offset_Tabs.Y += 28;
            //XPorter.Bus.Main_Handle = new MainWindow();                                                             //initialize descriptor of mainwindow
            XPorter.Bus.Main_Handle = this;
            RCConnectLibrary.set_default_settings();                                                                  //read all setting for connection and control
        }

        private void maximize_click(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            if (WindowState == WindowState.Normal)
            {
                SendMessage(helper.Handle, WM_SYSCOMMAND, (IntPtr)SC_MAXIMIZE, IntPtr.Zero);
            }
            else SendMessage(helper.Handle, WM_SYSCOMMAND, (IntPtr)SC_RESTORE, IntPtr.Zero);
        }

        private void minimize_click(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void cross_click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void show_connector(object sender, RoutedEventArgs e)
        {
            if (MainTab.SelectedIndex > -1)
            {
                TabEmptyPanel.Margin = new Thickness(0, 26, 0, 0);
            }
            else
            {
                TabEmptyPanel.Margin = new Thickness(0);
            }
            VisualEffectsAndComponents.Connector.show_connector(TabEmptyPanel);
        }

        //universal function to kill threads
        public static void thr_killer(Thread thr)
        {
            if (thr != null)
            {
                thr.Abort();
                thr.Join();
                thr = null;
            }
        }

        private void multi_cnt(object sender, RoutedEventArgs e)
        {
            TreeUngroup.Items.Clear();
            if (GlobalTypes.Settings.IPCatalog.Count == 0)
            {
                XPorter.Bus.Main_Handle.StatusBarText.Text = "Empty IP List";
            }
            else
            {
                if (!MainTab.Items.Contains(Work_Page))
                {
                    Work_Page = uni_tab_creator("WorkPage", true);
                    Work_Page.Unloaded += delegate
                    {
                        XPorter.Bus.Main_Handle.Dispatcher.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate
                        {
                            XPorter.Bus.Main_Handle.TreeUngroup.Items.Clear();
                            lock (MainWindow.Client_Catalog) {
                                Client_Catalog.Clear();
                            }
                        }));
                    };
                    Grid_In = new Grid();
                    ScrollViewer main_scrl_viewer = new ScrollViewer();
                    main_scrl_viewer.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
                    Scr_Cont = new WrapPanel();
                    Scr_Cont.Margin = new Thickness(7.5, 0, 7.5, 0);
                    main_scrl_viewer.Content = Scr_Cont;
                    Grid_In.Children.Add(main_scrl_viewer);
                    Work_Page.Content = Grid_In;

                    //MainTab.Items.Add(Work_Page);
                }
                else {
                    MainTab.SelectedItem = Work_Page;
                }

                VisualEffectsAndComponents.MainWindowVisualEffects.Curtain.curtain_fx(
                    (Panel)Work_Page.Content,
                    System.Windows.Media.Brushes.White,
                    rcresource.ajax_loader
                    );

                MainWindow.thr_killer(thr_Multi_Cnt);
                XPorter.Bus.Main_Handle.StatusBarText.Text = "";                                                    //write analyzer for status bar
                thr_Multi_Cnt = new Thread(RCConnectLibrary.multi_cnt);                                             //create thread for multiconnect
                thr_Multi_Cnt.SetApartmentState(ApartmentState.STA);
                thr_Multi_Cnt.Name = "MultiConnect";
                thr_Multi_Cnt.Start();
            }
        }

        private void main_tab_mouse_dbl_click(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                uni_tab_creator(null,true);                                                               //create simpe bookmark
            }
        }

        public TabItem uni_tab_creator(string caption, bool in_active)
        {
            //switch (flag)
            //{
            //    case 0:
            //        {
            //            var tab = simple_tab_create(sender, caption);
            //            return tab;
            //            break;
            //        }
            //    case 1:
            //        {
            //            work_page_create(sender);
            //            return Work_Page;
            //            break;
            //        }
            //    case 2:
            //        {
            //            var tab = per_cnt_tab_create(sender);
            //            return tab;
            //            break;
            //        }
            //}
            //return null;
            TabItem tmp_tab = new TabItem();
            if (caption != null)
            {
                tmp_tab.Header = caption;
            }
            else
            {
                tmp_tab.Header = "Untitled" + (MainTab.Items.Count + 1).ToString();
            }
            MainTab.Items.Add(tmp_tab);

            if (in_active)
            {
                MainTab.SelectedItem = tmp_tab;
            }

            tmp_tab.Loaded += delegate(object s, RoutedEventArgs e1)
            {
                var element = ((FrameworkElement)tmp_tab.Template.FindName("Cross", tmp_tab));
                element.MouseLeftButtonDown += delegate(object ss, MouseButtonEventArgs ee)
                    {
                        MainTab.Items.Remove(tmp_tab);
                    };
            };
            return tmp_tab;
        }

        private void menu_exit_item_click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void about_click(object sender, RoutedEventArgs e)
        {
            if (About == null) { 
                var tab = uni_tab_creator("About",true);
                var grid = new WrapPanel();
                About = new ContentControl();
                Uri uri = new Uri(RCResourceLibs.vsresources.about, UriKind.Relative);                                              //get content of connector from uri
                ResourceDictionary dict_res = new ResourceDictionary();
                using (MemoryStream tmp_stream = new MemoryStream(Encoding.UTF8.GetBytes(uri.ToString())))                          //translate stream to xaml markup
                {
                    System.Windows.Markup.XamlReader xaml_reader = new System.Windows.Markup.XamlReader();
                    try
                    {
                        dict_res.MergedDictionaries.Add((ResourceDictionary)xaml_reader.LoadAsync(tmp_stream));
                    }
                    catch (Exception ex) { }
                    About.Resources = dict_res;
                }
                tab.Unloaded += delegate
                {
                    About = null;
                };
                About.Loaded += delegate
                {
                    Hyperlink link = (Hyperlink)About.Template.FindName("Link",About);
                    link.RequestNavigate += delegate(object _sender, System.Windows.Navigation.RequestNavigateEventArgs _e)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_e.Uri.AbsoluteUri));
                        e.Handled = true;
                    };
                };
                About.SetValue(Panel.ZIndexProperty, 110);
                grid.Children.Add(About);
                tab.Content = grid;
            }
            else {
                foreach (TabItem it in MainTab.Items) {
                    if (it.Header == "About") {
                        MainTab.SelectedItem = it;
                    }
                }
            }
        }

        private void show_settings(object sender, RoutedEventArgs e)
        {
            if (Settings == null)
            {
                Settings = new ContentControl();
                var tab = uni_tab_creator("Settings", true);
                var grid = new Grid();
                grid.Background = System.Windows.Media.Brushes.AliceBlue;
                Uri _uri = new Uri(RCResourceLibs.vsresources.settings_style, UriKind.Relative);
                ResourceDictionary local_dict = new ResourceDictionary();
                using (MemoryStream tmp_stream = new MemoryStream(Encoding.UTF8.GetBytes(_uri.ToString())))
                {
                    System.Windows.Markup.XamlReader reader = new System.Windows.Markup.XamlReader();
                    try
                    {
                        local_dict.MergedDictionaries.Add((ResourceDictionary)reader.LoadAsync(tmp_stream));
                        Settings.Resources = local_dict;
                    }
                    catch (Exception ex) { }
                }
                Uri uri = new Uri(RCResourceLibs.vsresources.win_settings, UriKind.Relative);
                //ResourceDictionary dict_res = new ResourceDictionary();
                using (MemoryStream tmp_stream = new MemoryStream(Encoding.UTF8.GetBytes(uri.ToString())))
                {
                    System.Windows.Markup.XamlReader xaml_reader = new System.Windows.Markup.XamlReader();
                    try
                    {
                        local_dict.MergedDictionaries.Add((ResourceDictionary)xaml_reader.LoadAsync(tmp_stream));
                    }
                    catch (Exception ex) { }
                    Settings.Resources = local_dict;
                    Settings.SetValue(Panel.ZIndexProperty, 110);
                    grid.Children.Add(Settings);
                    tab.Content = grid;
                }
                tab.Loaded += delegate
                {
                        ((TabItem)Settings.Template.FindName("General", Settings)).PreviewMouseLeftButtonDown += label_click;
                        ((TabItem)Settings.Template.FindName("Sounds", Settings)).PreviewMouseLeftButtonDown += label_click;
                        ((TabItem)Settings.Template.FindName("Video", Settings)).PreviewMouseLeftButtonDown += label_click;
                        ((TabItem)Settings.Template.FindName("Privacy", Settings)).PreviewMouseLeftButtonDown += label_click;
                        ((TabItem)Settings.Template.FindName("Notifications", Settings)).PreviewMouseLeftButtonDown += label_click;
                        ((TabItem)Settings.Template.FindName("Advanced", Settings)).PreviewMouseLeftButtonDown += label_click;
                        TabSettings = (TabControl)Settings.Template.FindName("Settings", Settings);

                    ((Button)Settings.Template.FindName("Default_General1", Settings)).Click += delegate
                    {
                            RCConnectLibrary.set_default_settings();
                        //refresh all settings values
                    };
                    ((Button)Settings.Template.FindName("Default_General2", Settings)).Click += delegate
                    {
                            RCConnectLibrary.set_default_settings();
                        //refresh all settings values
                    };

                    ((Button)Settings.Template.FindName("Apply_General1", Settings)).Click += delegate
                    {
                        GlobalTypes.Settings.IPCatalog.Clear();
                        GlobalTypes.Settings.IPCatalog.Add(new GlobalTypes.IPInterval(
                           ((TextBox)Settings.Template.FindName("Left_Range_Ip", Settings)).Text, ((TextBox)Settings.Template.FindName("Left_Range_Ip", Settings)).Text
                            ));
                        if(((PasswordBox)Settings.Template.FindName("Password",Settings)).Password==((PasswordBox)Settings.Template.FindName("Confirm_Password",Settings)).Password)
                        {
                            StatusBarText.Text = "Password Accepted";
                            GlobalTypes.Settings.Password = ((PasswordBox)Settings.Template.FindName("Password", Settings)).Password;
                        }
                        else
                        {
                            StatusBarText.Text = "Passwords doesn't equals";
                        }
                        GlobalTypes.Settings.TabHeader = ((ComboBoxItem)((ComboBox)Settings.Template.FindName("Host_Name_Format", Settings)).SelectedItem).Content.ToString();
                    };
                    ((Button)Settings.Template.FindName("Apply_General2", Settings)).Click += delegate
                    {
                        GlobalTypes.Settings.TCPPort = Convert.ToInt32(((TextBox)Settings.Template.FindName("TCP_Port", Settings)).Text);
                        GlobalTypes.Settings.Interval = Convert.ToInt32(((Slider)Settings.Template.FindName("Refresh_Period", Settings)).Value);
                        GlobalTypes.Settings.TimeToConnect = Convert.ToInt32(((TextBox)Settings.Template.FindName("Connection_Timeout",Settings)).Text);
                        GlobalTypes.Settings.ThreadCount = Convert.ToInt32(((Slider)Settings.Template.FindName("Thread_Count", Settings)).Value); 
                    };
                };
                tab.Unloaded += delegate
                {
                    Settings = null;
                };
            }
            else {
                foreach (TabItem it in MainTab.Items) {
                    if (it.Header == "Settings") {
                        MainTab.SelectedItem = it;
                    }
                }
            }
        }
        public bool[] Visible = { true, false, false, false, false, false };
        public int Last_Selection = 1;
        int[][] Indexes = { new int[] { 1, 2 }, new int[] { 4, 5 }, new int[] { 7, 8 }, new int[] { 10 }, new int[] { 12 }, new int[] { 14 } };

        private void label_click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            int cur_ind = Convert.ToInt32(((FrameworkElement)sender).Tag);
            if (!Visible[cur_ind])
            {
                for (int i = 0; i < Indexes[cur_ind].Length; i++)
                {
                    ((TabItem)TabSettings.Items[Indexes[cur_ind][i]]).Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                for (int i = 0; i < Indexes[cur_ind].Length; i++)
                {
                    ((TabItem)TabSettings.Items[Indexes[cur_ind][i]]).Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            Visible[cur_ind] = !Visible[cur_ind];
        }

        private void twitter_click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(new Uri("https://twitter.com/rcgroupcompany/").AbsoluteUri));
        }

        private void documentation_click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(new Uri("http://rc.ru").AbsoluteUri));
        }

        private void hide_show_sidebar(object sender, RoutedEventArgs e)
        {
            if (Sidebar.ActualWidth > 0)
            {
                Hide_Sidebar.Header = "Show Side Bar";
                GridM.ColumnDefinitions[0].MinWidth = 0;
                Sidebar.Width = 0;
                Side_Splitter.Width = 0;
                GridM.ColumnDefinitions[0].Width = System.Windows.GridLength.Auto;
            }
            else
            {
                Hide_Sidebar.Header = "Hide Side Bar";
                GridM.ColumnDefinitions[0].MinWidth = 120;
                Sidebar.Width = 200;
                Side_Splitter.Width = 3;
                GridM.ColumnDefinitions[0].Width = System.Windows.GridLength.Auto;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
                if(Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if(e.Key == Key.W)
                    {
                        if (MainTab.Items.Count < 1) Application.Current.Shutdown();
                        else
                        {
                            MainTab.Items.Remove(MainTab.SelectedItem);
                        }
                    }
                    if(e.Key == Key.N)
                    {
                        uni_tab_creator(null, true);
                    }
                    if(e.Key == Key.M)
                    {
                        multi_cnt(sender, null);
                    }
                    if(Keyboard.IsKeyDown(Key.LeftAlt))
                    {
                        if(e.Key==Key.C)
                        {
                            if (MainMenu.IsEnabled) { 
                                show_connector(sender, null);
                            }
                        }
                    }
                    if(e.Key==Key.P)
                    {
                        show_settings(sender, null);
                    }
                    if(Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        if(e.Key==Key.S)
                        {
                            hide_show_sidebar(sender, null);
                        }
                    }
                }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            thr_killer(RCScreenClient.thr_Get_Img_From_UDP);
            Application.Current.Shutdown();
        }
    }
}
