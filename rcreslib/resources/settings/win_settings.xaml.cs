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

namespace RCResourceLibs
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public MainWindow()
        //{
        //   // Content.Content = ((object)((TabItem)Settings.Items[Last_Selection]).Content).;
        //}
        //public bool[] Visible = { true, false, false, false, false, false };
        //public int Last_Selection = 1;
        //int[][] Indexes = { new int[] { 1, 2 }, new int[] { 4, 5 }, new int[] { 7, 8 }, new int[] { 10 }, new int[] { 12 }, new int[] { 14 } };

        //private void label_click(object sender, MouseButtonEventArgs e)
        //{
        //    e.Handled = true;
        //    int cur_ind = Convert.ToInt32(((FrameworkElement)sender).Tag);
        //    if (!Visible[cur_ind])
        //    {
        //        for (int i = 0; i < Indexes[cur_ind].Length; i++)
        //        {
        //            ((TabItem)Settings.Items[Indexes[cur_ind][i]]).Visibility = System.Windows.Visibility.Visible;
        //        }
        //    }
        //    else
        //    {
        //            for (int i = 0; i < Indexes[cur_ind].Length; i++)
        //            {
        //                ((TabItem)Settings.Items[Indexes[cur_ind][i]]).Visibility = System.Windows.Visibility.Collapsed;
        //            }
        //    }
        //    Visible[cur_ind] = !Visible[cur_ind];
        //}
    }
}
