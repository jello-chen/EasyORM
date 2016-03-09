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
using EasyORM.Provider;
using EasyORM.Utils;
using System.IO;
using Microsoft.Win32;
using EasyORM.VSExtension.Utils;
using EasyORM.VSExtension.Configuration;

namespace EasyORM.VSExtension.UserControls.InputDataBaseInfo
{
    /// <summary>
    /// SQLiteUserControl.xaml
    /// </summary>
    public partial class SQLiteUserControl : UserControl, IGetDataBaseInfo
    {
        private string _folder;
        private string _projectFolder;
        private ProviderBase _provider;
        private string _connectionStringName;
        public string Database { get; set; }

        public string ConnectionStringName
        {
            get { return _connectionStringName; }
        }
        private Dictionary<string, string> _connectionStrings;

        public SQLiteUserControl(string folder, string projectFolder)
        {
            _folder = folder;
            _projectFolder = projectFolder;
            InitializeComponent();
            _provider = WizardContext.DataContext.Provider;
            txtDatabase.DataContext = this;
        }

        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Database))
                {
                    throw new ApplicationException("Please select a database file");
                }
                var dbFileName = System.IO.Path.GetFileName(Database);
                var dbFilePath = System.IO.Path.Combine(_projectFolder, dbFileName);
                if (!File.Exists(dbFilePath))
                {
                    File.Copy(Database, dbFilePath);
                }
                return string.Format("Data Source=|DataDirectory|{0};Pooling=true;", dbFileName);
            }
        }

        private void comboConnectionStrings_KeyUp(object sender, KeyEventArgs e)
        {
            _connectionStringName = comboConnectionStrings.Text;
            txtDatabase.Clear();
            InitConnectionStringInfo();
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

        void InitConnectionStringInfo()
        {
            var connectionString = _connectionStrings.GetOrDefault(_connectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }
            var dbConnectionBuilder = _provider.CreateDbProviderFactory().CreateConnectionStringBuilder();
            dbConnectionBuilder.ConnectionString = connectionString;
            if (dbConnectionBuilder.ContainsKey("Data Source"))
            {
                txtDatabase.Text = Convert.ToString(dbConnectionBuilder["Data Source"]);
            }
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

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SQLite database file|*.db";
            var result = ofd.ShowDialog(Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive));
            if (result.HasValue && result.Value)
            {
                txtDatabase.Text = ofd.FileName;
                Database = ofd.FileName;
            }
        }
    }
}
