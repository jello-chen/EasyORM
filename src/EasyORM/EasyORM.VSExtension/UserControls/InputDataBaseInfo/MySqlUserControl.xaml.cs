using System;
using System.Collections.Generic;
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
    /// MySqlUserControl.xaml
    /// </summary>
    public partial class MySqlUserControl : UserControl, IGetDataBaseInfo
    {
        public string Database { get; set; }

        public string Server { get; set; }
        public string UserId { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        private ProviderBase _provider;
        string _folder;

        string _connectionStringName;

        public string ConnectionStringName
        {
            get { return _connectionStringName; }
        }
        private Dictionary<string, string> _connectionStrings;
        public MySqlUserControl(string folder, ProviderBase provider)
        {
            Port = 3306;
            _provider = provider;
            InitializeComponent();
            _folder = folder;
            txtServer.DataContext = this;
            txtDataBase.DataContext = this;
            txtUserId.DataContext = this;
            txtPassword.DataContext = this;
            numPort.DataContext = this;
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
                var builder = new StringBuilder("Server=");
                builder.Append(Server);
                builder.Append(";Database=");
                builder.Append(Database);
                builder.AppendFormat(";Port={0}", Port);
                if (string.IsNullOrWhiteSpace(UserId))
                {
                    throw new ApplicationException("The user name should be required");
                }
                if (string.IsNullOrWhiteSpace(Password))
                {
                    throw new ApplicationException("The password should be required");
                }
                builder.Append(";UId=");
                builder.Append(UserId);
                builder.Append(";PWD=");
                builder.Append(Password);
                builder.Append(";");
                return builder.ToString();
            }
        }

        private void comboConnectionStrings_KeyUp(object sender, KeyEventArgs e)
        {
            _connectionStringName = comboConnectionStrings.Text;
            txtDataBase.Clear();
            txtPassword.Clear();
            txtServer.Clear();
            txtUserId.Clear();
            InitConnectionStringInfo();
        }

        void InitConnectionStringInfo()
        {
            var connectionString = _connectionStrings.GetOrDefault(_connectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }
            var builder = _provider.CreateDbProviderFactory().CreateConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            try
            {
                txtDataBase.Text = Convert.ToString(builder["Database"]);
                Database = txtDataBase.Text;
                Password = Convert.ToString(builder["Pwd"]);
                txtServer.Text = Convert.ToString(builder["Server"]);
                Server = txtServer.Text.Trim();
                UserId = Convert.ToString(builder["UID"]);
                txtPassword.Password = Password;
                txtUserId.Text = UserId;
            }
            catch
            {

            }
        }
        private void comboConnectionStrings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var keyItem = (ComboBoxItem)comboConnectionStrings.SelectedValue;
            if (keyItem == null)
            {
                return;
            }
            _connectionStringName = keyItem.Content.ToString();
            InitConnectionStringInfo();
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            Password = txtPassword.Password.Trim();
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
    }
}
