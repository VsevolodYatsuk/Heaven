using Heaven;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
using System.Data.Entity;

namespace Heaven
{
    public partial class MainWindow : Window
    {

        DataBase dataBase = new DataBase();

        public MainWindow()
        {
            InitializeComponent();
        }

        private MainWindow mainWindow;
        private void Registration_Click(object sender, RoutedEventArgs e)
        {

            if (mainWindow != null)
            {
                mainWindow.Close();
            }

            mainWindow = new MainWindow();

            this.Hide();

            registration registrationWindow = new registration();
            registrationWindow.Closed += (s, args) =>
            {
                mainWindow.Show();

                registrationWindow.Close();
            };

            registrationWindow.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var loginUser = loginBox.Text;
            var passwordUser = passwordBox.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string querystring = $"select id_user, login_user, password_user, email_user from Users where login_user = '{loginUser}' and password_user = '{passwordUser}'";

            SqlCommand command = new SqlCommand(querystring, dataBase.getConnection());
            adapter.SelectCommand = command;
            adapter.Fill(table);

            if(table.Rows.Count == 1)
            {
                mainWindow = new MainWindow();

                this.Hide();

                heaven heavenWindow = new heaven();
                heavenWindow.Show();

            }
            else
            {
                MessageBox.Show("Неправильный логин или пароль");
            }

        }
    }
}
