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
using System.Windows.Interop;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Resources;

namespace ReinCorpDesign
{
   public class VisualEffectsAndComponents
    {
       //player of gif animation
        public class GifPlayer: System.Windows.Controls.Image
        {
            #region global vars
            private System.Windows.Media.Imaging.BitmapSource[] Frames_Array_Source = null;
            private int Current_Frame = 0;                                                                                      //current frame of picture
            private bool B_Is_Animated = false;
            public event RoutedPropertyChangedEventHandler<Bitmap> Animated_Source_Changed                                      //dependency property
            {
                add { AddHandler(Animated_Bitmap_Changed_Event, value); }
                remove { RemoveHandler(Animated_Bitmap_Changed_Event, value); }
            }

            public bool Is_Animated                                                                                             //return local value
            {
                get { return B_Is_Animated; }
            }

            public Bitmap Animated_Bitmap                                                                                       //return value from object param
            {
                get { return (Bitmap)GetValue(Animated_Bitmap_Property); }
                set { SetValue(Animated_Bitmap_Property, value); }
            }

            #endregion
            #region dependency_properties_and_routed_events

            public static readonly DependencyProperty Animated_Bitmap_Property = DependencyProperty.Register(                   //register property of pic's src
                "AnimatedBitmap",
                typeof(Bitmap),
                typeof(GifPlayer),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(on_animated_changed)));

            public static readonly RoutedEvent Animated_Bitmap_Changed_Event = EventManager.RegisterRoutedEvent(                //register event which call clbck function
                "AnimatedBitmapChanged",                                                                                        //after change src
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Bitmap>),
                typeof(GifPlayer)); 

            #endregion
            #region constructors

            static GifPlayer()
            {
                DefaultStyleKeyProperty.OverrideMetadata(
                    typeof(GifPlayer),
                    new FrameworkPropertyMetadata(typeof(GifPlayer)));
            }

            public GifPlayer(Bitmap bmp)
            {
                Animated_Bitmap = bmp;
            }
            #endregion
            #region events

            //callback function which calls after change src
            private static void on_animated_changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
            {
                GifPlayer player = (GifPlayer)obj;
                player.update_src();
                RoutedPropertyChangedEventArgs<Bitmap> e = new RoutedPropertyChangedEventArgs<Bitmap>((Bitmap)args.OldValue,    //call listeners functions
                    (Bitmap)args.NewValue,
                    Animated_Bitmap_Changed_Event);
                player.on_animated_source_changed(e);
            }
            #endregion
            #region mainfunc

            //raise function which listen this event
            protected virtual void on_animated_source_changed(RoutedPropertyChangedEventArgs<Bitmap> e)
            {
                RaiseEvent(e);
            }

            private void update_src()
            {
                int count_of_frames = Animated_Bitmap.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
                Current_Frame = 0;
                if(count_of_frames>0)
                {
                    Frames_Array_Source = new System.Windows.Media.Imaging.BitmapSource[count_of_frames];                       //initialize arr of frames
                    for(int i=0;i<count_of_frames;i++)
                    {
                        Animated_Bitmap.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time, i);
                        Bitmap tmp_bitmap = new Bitmap(Animated_Bitmap);
                        tmp_bitmap.MakeTransparent();

                        Frames_Array_Source[i] = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(                  //translate frame of pic to bmpsource
                            tmp_bitmap.GetHbitmap(),
                            IntPtr.Zero,
                            Int32Rect.Empty, 
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    }

                    strt_anim();
                }
            }

            //function call every change of frame
            private void on_frame_chngd(Object obj, EventArgs e)
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Render,
                    new Action(delegate() { chng_src(); })
                    );
            }

            private void strt_anim()
            { 
                if(!B_Is_Animated)
                {
                    ImageAnimator.Animate(Animated_Bitmap, new EventHandler(this.on_frame_chngd));
                    B_Is_Animated = true;
                }
            }

            private void stop_anim()
            {
                if(B_Is_Animated)
                {
                    ImageAnimator.StopAnimate(Animated_Bitmap, new EventHandler(on_frame_chngd));
                    B_Is_Animated = false;
                }
            }

            void chng_src()
            {
                Source = Frames_Array_Source[Current_Frame++];
                Current_Frame = Current_Frame % Frames_Array_Source.Length;
                ImageAnimator.UpdateFrames();
            }
            #endregion
        }

       //fx and other design actions
        public class MainWindowVisualEffects
        {
            public static class Curtain
            {
                public static DockPanel Load_Curtain;
                public static void curtain_fx(Panel Element, System.Windows.Media.Brush CurtainColor, Bitmap bmp)                   //show curtain fx with animation
                {
                    if (Load_Curtain == null)
                    {
                        Load_Curtain = new DockPanel();
                        Load_Curtain.SetValue(Panel.ZIndexProperty, 99);
                        GifPlayer player = new GifPlayer(bmp);
                        player.Stretch = System.Windows.Media.Stretch.None;
                        Load_Curtain.Background = CurtainColor;
                        Load_Curtain.Opacity = 0.4;
                        Load_Curtain.Children.Add(player);
                        Element.Children.Add(Load_Curtain);
                    }
                }
                public static void curtain_fx(Panel Element, System.Windows.Media.Brush CurtainColor)                               //show curtain fx without animation
                {
                    if (Load_Curtain == null)
                    {
                        Load_Curtain = new DockPanel();
                        Load_Curtain.SetValue(Panel.ZIndexProperty, 99);
                        Load_Curtain.Background = CurtainColor;
                        Load_Curtain.Opacity = 0.4;
                        Element.Children.Add(Load_Curtain);
                    }
                }
            }
        }

       //mdi window to connect in one_to_one mode
        public static class Connector
        {
            #region global vars
            public static ContentControl Cntr;
            public static System.Windows.Point Old_P;                                                                               //old points of position of connector
            delegate void Set_Margin_Delegate(ContentControl dock, Thickness th);
            public static Panel Target_Panel;
            public static double Old_Height, Old_Width;
            #endregion
            #region mainfunc

            public static void show_connector(Panel target_panel)                                                                   //show connector into target_panel
            {
                Target_Panel = target_panel;
                VisualEffectsAndComponents.MainWindowVisualEffects.Curtain.curtain_fx(
                    Target_Panel,
                    System.Windows.Media.Brushes.White
                    );

                Cntr = new ContentControl();
                Uri uri = new Uri(RCResourceLibs.vsresources.connector, UriKind.Relative);                                          //get content of connector from uri
                ResourceDictionary dict_res = new ResourceDictionary();

                using (MemoryStream tmp_stream = new MemoryStream(Encoding.UTF8.GetBytes(uri.ToString())))                          //translate stream to xaml markup
                {
                    System.Windows.Markup.XamlReader xaml_reader = new System.Windows.Markup.XamlReader();
                    try
                    {
                        dict_res.MergedDictionaries.Add((ResourceDictionary)xaml_reader.LoadAsync(tmp_stream));
                    }
                    catch (Exception ex) { }
                    Cntr.Resources = dict_res;
                }

                Cntr.SetValue(Panel.ZIndexProperty, 100);
                Target_Panel.Children.Add(Cntr);
                Grid.SetRow(Cntr, 0);
                Cntr.Margin = new Thickness(
                    ((Target_Panel.ActualWidth / 2) - (Cntr.Width / 2)),
                    ((Target_Panel.ActualHeight / 2) - (Cntr.Height / 2)),
                    0,
                    0);
                XPorter.Bus.Main_Handle.MainMenu.IsEnabled = false;
                XPorter.Bus.Main_Handle.Sidebar.IsEnabled = false;
                XPorter.Bus.Inner_Height = Target_Panel.ActualHeight - Cntr.Height - 1;
                XPorter.Bus.Inner_Width = Target_Panel.ActualWidth - Cntr.Width;
                VisualEffectsAndComponents.MainWindowVisualEffects.Curtain.Load_Curtain.IsHitTestVisible = false;

                VisualEffectsAndComponents.Connector.Cntr.Unloaded += delegate(object _object, RoutedEventArgs _e)
                {
                    Target_Panel.Children.Remove(VisualEffectsAndComponents.MainWindowVisualEffects.Curtain.Load_Curtain);          //delete curtain fx after destroying
                    VisualEffectsAndComponents.MainWindowVisualEffects.Curtain.Load_Curtain = null;
                    if (VisualEffectsAndComponents.Connector.Cntr != null)
                    {
                        VisualEffectsAndComponents.Connector.Cntr = null;
                    }
                    Target_Panel.SizeChanged -= target_panel_resize;
                };

                Cntr.Loaded += delegate(object s, RoutedEventArgs ee)
                {
                    var element = ((FrameworkElement)Cntr.Template.FindName("DockHeader", Cntr));
                    element.MouseLeftButtonDown += dock_panel_prev_mouse_down;                                                      //add listener functions for move
                    element.MouseLeftButtonUp += dock_panel_Prev_mouse_up;
                    element.MouseMove += dock_panel_prev_mouse_move;
                    ((Button)Cntr.Template.FindName("Cancel", Cntr)).Click += cn_cancel_click;                            //destroy form button
                    ((Button)Cntr.Template.FindName("Connect", Cntr)).Click += cn_connect_click;
                    Target_Panel.SizeChanged += target_panel_resize;
                    Old_Height = Target_Panel.ActualHeight - Cntr.ActualHeight;
                    Old_Width = Target_Panel.ActualWidth - Cntr.ActualWidth;
                };
            }

            public static void target_panel_resize(object sender,SizeChangedEventArgs e)
            {
                double x = Cntr.Margin.Left;
                double y = Cntr.Margin.Top;
                double width = Target_Panel.ActualWidth - Cntr.ActualWidth;
                double height = Target_Panel.ActualHeight - Cntr.ActualHeight;
                if (width <= 0) width = 1;
                if (height <= 0) height = 1;
                Cntr.Margin = new Thickness(x / Old_Width * width, y / Old_Height * height, 0, 0);
                Old_Height = height;
                Old_Width = width;
            }

            public static void dock_panel_prev_mouse_down(object sender, MouseButtonEventArgs e)
            {
                ContentControl connector_panel = (ContentControl)((DockPanel)sender).TemplatedParent;
                Panel trgt_panel = connector_panel.Parent as Panel;
                MainWindow m_window = (MainWindow)MainWindow.GetWindow((DockPanel)sender);
                Old_P = e.GetPosition(m_window);
                System.Windows.Point cn_xy = e.GetPosition(connector_panel);

                //------------------------------------------------------------------------------------//
                System.Windows.Point offset = Target_Panel.TranslatePoint(new System.Windows.Point(), XPorter.Bus.Main_Handle);
                System.Windows.Point size = new System.Windows.Point(Target_Panel.ActualWidth, Target_Panel.ActualHeight);
                HwndSource hwndSource = (HwndSource)HwndSource.FromVisual(XPorter.Bus.Main_Handle);
                CompositionTarget ct = hwndSource.CompositionTarget;
                offset = ct.TransformToDevice.Transform(offset);
                size = ct.TransformToDevice.Transform(size);
                Win32.POINT screenLocation = new Win32.POINT(offset);
                Win32.ClientToScreen(hwndSource.Handle, ref screenLocation);
                Win32.POINT screenSize = new Win32.POINT(size);

                System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle(screenLocation.X + (int)cn_xy.X,
                    screenLocation.Y + (int)cn_xy.Y, screenSize.X - (int)connector_panel.Width+1,
                    screenSize.Y - (int)connector_panel.Height+1);
                //if (m_window.WindowState == System.Windows.WindowState.Normal)                                                      //if position is normal we can clip mouse
                //{
                //    System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle(
                //        (int)m_window.Left + (int)cn_xy.X + 4,
                //        (int)m_window.Top + (int)cn_xy.Y + 2 + XPorter.Bus.Top,
                //        (int)m_window.Width - (int)connector_panel.Width - 7,
                //        (int)m_window.Height - (int)connector_panel.Height - 4 - XPorter.Bus.Top - XPorter.Bus.Bottom);
                //}
                //else
                //{
                //    System.Drawing.Rectangle MaximizePadding = new System.Drawing.Rectangle(
                //        (int)SystemParameters.WorkArea.Left, (int)SystemParameters.WorkArea.Top,
                //        (int)(SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Right),
                //        (int)(SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Bottom));

                //    System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle(
                //       MaximizePadding.Left - 3 + (int)cn_xy.X,
                //       MaximizePadding.Top + (int)cn_xy.Y + XPorter.Bus.Top - 5,
                //       (int)trgt_panel.ActualWidth - MaximizePadding.Width - (int)connector_panel.ActualWidth - 7,
                //       (int)trgt_panel.ActualHeight - MaximizePadding.Height - 
                //       (int)connector_panel.ActualHeight - 4 - XPorter.Bus.Top - XPorter.Bus.Bottom);
                //}
                ((DockPanel)sender).CaptureMouse();
            }

            private static void set_margin(ContentControl dock, Thickness th)
            {
                dock.Margin = th;
            }

            public static void cn_connect_click(object sender, RoutedEventArgs e)
            {
                System.Net.IPAddress address;
                System.Net.IPAddress.TryParse(
                    ((TextBox)Cntr.Template.FindName("Address", Cntr)).Text, out address);
                if(address!=null)
                { 
                ContentControl connector_panel = (ContentControl)((Button)sender).TemplatedParent;
                ((Panel)connector_panel.Parent).Children.Remove(connector_panel);
                Cntr = null;
                XPorter.Bus.Is_Main_Menu_Enabled = true;
                XPorter.Bus.Main_Handle.Sidebar.IsEnabled = true;

                RCScreenClient.start_one_to_one(
                    new GlobalTypes.ClientInfo(new GlobalTypes.HostInfo(address, "Here was getInfo")));
                    }
            }

            public static void cn_cancel_click(object sender, RoutedEventArgs e)
            {
                ContentControl connector_panel = (ContentControl)((Button)sender).TemplatedParent;
                ((Panel)connector_panel.Parent).Children.Remove(connector_panel);
                Cntr = null;
                XPorter.Bus.Is_Main_Menu_Enabled = true;
                XPorter.Bus.Main_Handle.Sidebar.IsEnabled = true;
            }

            public static void dock_panel_prev_mouse_move(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Window m_window = MainWindow.GetWindow((DockPanel)sender);
                    ContentControl connector_panel = (ContentControl)((DockPanel)sender).TemplatedParent;
                    e.Handled = false;
                    System.Windows.Point cur_point = e.GetPosition(m_window);
                    double dx = cur_point.X - Old_P.X;
                    double dy = cur_point.Y - Old_P.Y;
                    Thickness thick = new Thickness(connector_panel.Margin.Left + dx, connector_panel.Margin.Top + dy, 0, 0);
                    object[] args = new object[2];
                    args[0] = connector_panel;
                    args[1] = thick;
                    Set_Margin_Delegate margin_setter = set_margin;
                    m_window.Dispatcher.BeginInvoke(margin_setter, System.Windows.Threading.DispatcherPriority.Render, args);
                    Old_P = cur_point;
                }
            }

            public static void dock_panel_Prev_mouse_up(object sender, MouseButtonEventArgs e)
            {
                System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle();
                ((DockPanel)sender).ReleaseMouseCapture();
            }
            #endregion
        }
    }
}
