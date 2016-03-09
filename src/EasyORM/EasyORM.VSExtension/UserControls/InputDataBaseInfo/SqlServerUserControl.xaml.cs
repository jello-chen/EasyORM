using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
using EasyORM.Utils;
using EasyORM.VSExtension.Configuration;
using EasyORM.Provider;
using EasyORM.VSExtension.Utils;

namespace EasyORM.VSExtension.UserControls.InputDataBaseInfo
{
    /// <summary>
    /// SqlServerUserControl.xaml
    /// </summary>
    public partial class SqlServerUserControl : IGetDataBaseInfo
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        string _folder;

        string _connectionStringName;

        public string ConnectionStringName
        {
            get { return _connectionStringName; }
        }
        private Dictionary<string, string> _connectionStrings;
        public SqlServerUserControl(string folder)
        {
            _folder = folder;
            InitializeComponent();
            txtServer.DataContext = this;
            txtDataBase.DataContext = this;
            txtUserId.DataContext = this;
            txtPassword.DataContext = this;
        }

        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Server))
                {
                    throw new ApplicationException("The server name should be required");
                }
                if (string.IsNullOrWhiteSpace(Database))
                {
                    throw new ApplicationException("The database name should be required");
                }
                var builder = new StringBuilder("Data Source=");
                builder.Append(Server);
                builder.Append(";Initial Catalog=");
                builder.Append(Database);
                if (radioUseSqlServer.IsChecked.HasValue && radioUseSqlServer.IsChecked.Value)
                {
                    if (string.IsNullOrWhiteSpace(UserId))
                    {
                        throw new ApplicationException("The user name should be required");
                    }
                    if (string.IsNullOrWhiteSpace(Password))
                    {
                        throw new ApplicationException("The password should be required");
                    }
                    builder.Append(";User Id=");
                    builder.Append(UserId);
                    builder.Append(";Password=");
                    builder.Append(Password);
                    builder.Append(";");
                }
                else
                {
                    builder.Append(";Integrated Security=true;");
                }
                return builder.ToString();
            }
        }

        private void radioUseWindows_Checked(object sender, RoutedEventArgs e)
        {
            spUsername.IsEnabled = spPassword.IsEnabled = false;
        }

        private void radioUseSqlServer_Checked(object sender, RoutedEventArgs e)
        {
            spUsername.IsEnabled = spPassword.IsEnabled = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var configFile = ProjectHelper.GetConfigFile(_folder);
            comboConnectionStrings.Items.Clear();
            if (File.Exists(configFile))
            {
                var config = ConfigurationManager.Open(configFile);
                _connectionStrings = config.GetConnectionStrings();
                foreach (var key in _connectionStrings.Keys)
                {
                    comboConnectionStrings.Items.Add(new ComboBoxItem()
                    {
                        Content = key
                    });
                }
            }
        }

        private void comboConnectionStrings_Selected(object sender, RoutedEventArgs e)
        {
            var keyItem = (ComboBoxItem)comboConnectionStrings.SelectedValue;
            if (keyItem == null)
            {
                return;
            }
            _connectionStringName = keyItem.Content.ToString();
            InitConnectionStringInfo();
        }

        void InitConnectionStringInfo()
        {
            var connectionString = _connectionStrings.GetOrDefault(_connectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            txtDataBase.Text = builder.InitialCatalog;
            Database = txtDataBase.Text;
            Password = builder.Password;
            Server = builder.DataSource;
            UserId = builder.UserID;
            txtPassword.Password = builder.Password;
            txtServer.Text = builder.DataSource;
            txtUserId.Text = builder.UserID;
            radioUseSqlServer.IsChecked = !builder.IntegratedSecurity;
            radioUseWindows.IsChecked = builder.IntegratedSecurity;
        }

        private void comboConnectionStrings_KeyUp(object sender, KeyEventArgs e)
        {
            _connectionStringName = comboConnectionStrings.Text;
            txtDataBase.Clear();
            txtPassword.Clear();
            txtServer.Clear();
            txtUserId.Clear();
            radioUseSqlServer.IsChecked = false;
            radioUseWindows.IsChecked = false;
            InitConnectionStringInfo();
        }

        public string Database { get; set; }

        public string Server { get; set; }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            Password = txtPassword.Password.Trim();
        }
    }
}
