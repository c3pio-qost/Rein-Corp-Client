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
using System.IO;
using System.Windows.Resources;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace ProgrammAgent
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    /*
     * version witout database login check
     */

    public partial class m_win : Window
    {
        public m_win()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Check user information and run main form of App
        /// </summary>
        /// <param name="sender">Handle of invoked object</param>
        /// <param name="e">Event Type</param>
        private void login_click(object sender, RoutedEventArgs e)
        {
            ReinCorpDesign.XPorter.Bus.Current_User = Login.Text;
            ReinCorpDesign.MainWindow m_win = new ReinCorpDesign.MainWindow();
            m_win.Show();
            this.Close();
        }

        /*private int connector_to_my_SQLBase()
        {
            string connect = "Database=reincorp;Data Source=localhost;User Id=root;Password=''";
            MySql.Data.MySqlClient.MySqlConnection cnt = new MySql.Data.MySqlClient.MySqlConnection(connect);
            try
            {
                cnt.Open();
                MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(
                    "SELECT `password` FROM `users` WHERE `login`='" + Login.Text + "'",
                    cnt
                    );
                string result = cmd.ExecuteScalar().ToString();
                if (Password.Password == result)
                {
                    ReinCorpDesign.XPorter.Bus.Current_User = Login.Text;
                    ReinCorpDesign.MainWindow m_win = new ReinCorpDesign.MainWindow();
                    m_win.Show();
                    this.Close();
                }
                else
                {
                    AttentionBackground.Visibility = System.Windows.Visibility.Visible;
                    AttentionText.Visibility = System.Windows.Visibility.Visible;
                    cnt.Close();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                AttentionBackground.Visibility = System.Windows.Visibility.Visible;
                AttentionText.Visibility = System.Windows.Visibility.Visible;
                cnt.Close();
                return 0;
            }
            return 1;
        }*/

        /// <summary>
        /// Cancel agent form and close App
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancel_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Add shortcut for agent form to enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void login_prev_key_up(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(login_btn);
                IInvokeProvider login_pattern = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                login_pattern.Invoke();
                //connector_to_my_SQLBase();
            }
        }
    }
}
