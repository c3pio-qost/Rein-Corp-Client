using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ReinCorpDesign
{
    /// <summary>
    /// Логика взаимодействия для ScreenWindow.xaml
    /// </summary>
    public partial class ScreenWindow : Window
    {
        #region Global Vars
        public PictureBox Screen_Box { get { return _ScreenBox; } }                                         //return local element
        protected TabItem Item;                                                                             //store tabitem
        protected FrameworkElement Target_Place;
        #endregion
        public ScreenWindow(FrameworkElement trgt_place, TabItem item)
        {
            InitializeComponent();
            Target_Place = trgt_place;
            Item = item;
            Window owner = Window.GetWindow(trgt_place);
            owner.LocationChanged += delegate { on_size_and_location_changing(); };
            owner.SizeChanged += delegate { on_size_and_location_changing(); };
            XPorter.Bus.Current_Form = this;
            if (item.IsVisible)
            {
                XPorter.Bus.Main_Handle.MainTab.SelectionChanged += delegate
                {
                    {
                        Hide();
                    }
                };
                Item.RequestBringIntoView += delegate
                {
                    Show();
                    XPorter.Bus.Current_Form = this;
                    on_size_and_location_changing();
                };
                Owner = owner;
                Show();
                on_size_and_location_changing();
            }

        }

        private void on_size_and_location_changing()
        {
            if ((XPorter.Bus.Main_Handle.MainTab.SelectedItem == Item)&&(this.IsVisible))
            {
                HwndSource hwnd_source = (HwndSource)HwndSource.FromVisual(Owner);
                CompositionTarget compose_target = hwnd_source.CompositionTarget;
                Point offset = compose_target.TransformToDevice.Transform(XPorter.Bus.Global_Offset_Tabs);
                Point size = compose_target.TransformToDevice.Transform(XPorter.Bus.Global_Size_Tabs);
                Win32.POINT screen_location = new Win32.POINT(offset);
                Win32.ClientToScreen(hwnd_source.Handle, ref screen_location);
                Win32.POINT screen_size = new Win32.POINT(size);
                Win32.MoveWindow(((HwndSource)HwndSource.FromVisual(this)).Handle, screen_location.X, screen_location.Y, screen_size.X, screen_size.Y, true);
            }
        }

    }
}
