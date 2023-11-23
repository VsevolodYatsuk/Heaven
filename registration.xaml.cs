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
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;

namespace Heaven
{
    public partial class registration : Window
    {

        DataBase dataBase = new DataBase();


        public registration()
        {
            InitializeComponent();

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string email = emailBox.Text.ToLower().Trim();
            string login = loginBox.Text.Trim();
            string password = passwordBox.Text.Trim();
            string password_repetition = password_repetitionBox.Text.Trim();

            bool hasError = false;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            if (login.Length < 5)
            {
                loginBox.ToolTip = "Это поле введено не корректно";
                loginBox.Background = Brushes.DarkRed;
                hasError = true;
            }
            else
            {
                loginBox.ToolTip = null;
                loginBox.Background = Brushes.Transparent;
            }

            if (password.Length < 5)
            {
                passwordBox.ToolTip = "Это поле введено не корректно";
                passwordBox.Background = Brushes.DarkRed;
                hasError = true;
            }
            else
            {
                passwordBox.ToolTip = null;
                passwordBox.Background = Brushes.Transparent;
            }

            if (password != password_repetition)
            {
                password_repetitionBox.ToolTip = "Второй пароль не правильный";
                password_repetitionBox.Background = Brushes.DarkRed;
                hasError = true;
            }
            else
            {
                password_repetitionBox.ToolTip = null;
                password_repetitionBox.Background = Brushes.Transparent;
            }

            if (email.Length < 5 || !email.Contains("@") || !email.Contains("."))
            {
                emailBox.ToolTip = "Это поле введено не корректно";
                emailBox.Background = Brushes.DarkRed;
                hasError = true;
            }
            else
            {
                emailBox.ToolTip = null;
                emailBox.Background = Brushes.Transparent;
            }

            if (!hasError)
            {
                string querystring = $"insert into Users(login_user, password_user, email_user) values('{login}', '{password}','{email}')";

                SqlCommand command = new SqlCommand(querystring, dataBase.getConnection());

                dataBase.openConnection();
                if (command.ExecuteNonQuery() == 1)
                {
                    MessageBox.Show("Все корректно");
                }
                else
                {
                    MessageBox.Show("День писи попы");
                }

            }
        }
    }
} 
