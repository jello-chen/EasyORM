using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasyORM.VSExtension.CodeGenerator;
using Newtonsoft.Json;
using System.IO;
using System.Data.Common;
using EnvDTE;
using EasyORM.Provider;
using EasyORM.VSExtension.Utils;
using EasyORM.VSExtension.UserControls;
using EasyORM.Utils;

namespace EasyORM.VSExtension
{
    /// <summary>
    /// WizardWindow.xaml
    /// </summary>
    public partial class WizardWindow : System.Windows.Window
    {
        public string Filter { get; set; }
        GeneratedCodeModel _model = new GeneratedCodeModel();
        Project _project;
        public List<string> GeneratedItems { get; private set; }
        public bool GenerateDataContext { get; set; }
        string _modelName;
        public string ModelName
        {
            get
            {
                return _modelName;
            }
            set
            {
                _modelName = value;
            }
        }
        string _folder;
        public WizardWindow(string folder, Project project)
        {
            _folder = folder;
            InitializeComponent();
            txtModelName.DataContext = this;
            txtFilter.DataContext = this;
            GenerateDataContext = true;
            chkGenerateDataContext.DataContext = this;
            GeneratedItems = new List<string>();
            _project = project;
            configFile = ProjectHelper.GetConfigFile(_folder);
            projectFile = ProjectHelper.GetProjectFile(_folder);
            _projectFolder = System.IO.Path.GetDirectoryName(projectFile);
        }

        DatabaseTypes _dataBase;
        private DbProviderFactory _factory;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lvDataBaseTypes.ItemsSource = EasyORM.DataContext.SupportProviders;
        }

        UserControlFactory _userControlFactory;
        private UserControls.InputDataBaseInfo.IGetDataBaseInfo _databaseInfoGetter;
        private Dictionary<string, SchemaModel.Table> _tables;
        private string _projectFolder;
        private string configFile;
        private string projectFile;
        private ProviderBase _provider;
        private void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            switch (wizard.CurrentPage.Name)
            {
                case "wzdSelectDataBase":
                    var dataBase = (KeyValuePair<DatabaseTypes, string>)lvDataBaseTypes.SelectedValue;
                    if (string.IsNullOrWhiteSpace(dataBase.Value))
                    {
                        MessageBox.Show(this, "Please select a database", "Tips");
                        return;
                    }
                    _dataBase = (DatabaseTypes)dataBase.Key;
                    var provider = EasyORM.DataContext.SupportProviders.GetOrDefault(_dataBase);
                    if (provider == null)
                    {
                        MessageBox.Show(this, "The database is not supported", "Tips");
                        e.Cancel = true;
                        return;
                    }
                    _model.DatabaseType = _dataBase;
                    _model.DbFactoryName = provider;
                    try
                    {
                        _factory = ProviderFactory.GetFactory(provider);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(this, "The database is not supported", "Tips");
                        e.Cancel = true;
                        return;
                    }
                    _userControlFactory = new UserControlFactory(_dataBase);
                    var grid = (Grid)wzdSelectDataBaseInfo.Content;
                    WizardContext.DataContext = new DataContext("", _model.DbFactoryName);
                    _provider = WizardContext.DataContext.Provider;
                    _databaseInfoGetter = _userControlFactory.CreateGetDataBaseInfoControl(_folder, _projectFolder, _provider);
                    var tb = (UserControl)_databaseInfoGetter;
                    Grid.SetColumn(tb, 0);
                    Grid.SetRow(tb, 1);
                    grid.Children.Clear();
                    grid.Children.Add(tb);
                    wizard.CanSelectPreviousPage = true;
                    break;
                case "wzdSelectDataBaseInfo":
                    Title = "Testing Connection - Model Wizard";
#if !DEBUG
                    try
                    {
#endif
                    var conn = _factory.CreateConnection();
                    _model.ConnectionStringName = _databaseInfoGetter.ConnectionStringName;
                    if (string.IsNullOrWhiteSpace(_model.ConnectionStringName))
                    {
                        throw new ApplicationException("ConnectionString should be required");
                    }
                    using (conn)
                    {
                        conn.ConnectionString = _databaseInfoGetter.ConnectionString;
                        conn.Open();
                    }
                    WizardContext.DataContext = new DataContext(_databaseInfoGetter.ConnectionString, _model.DbFactoryName);
                    _provider = WizardContext.DataContext.Provider;
#if !DEBUG
                    }
                    catch (ApplicationException ae)
                    {
                        MessageBox.Show(this, ae.Message, "Tips");
                        e.Cancel = true;
                        Title = "Test Failed - Model Wizard";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Test Connection Failed");
                        e.Cancel = true;
                        Title = "Test Failed - Model Wizard";
                    }
#endif
                    break;
            }
        }

        private void lvDataBaseTypes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            wizard.CanSelectNextPage = lvDataBaseTypes.SelectedValue != null;
        }

        TreeViewItem GetTreeViewItem(string header, Action<object, bool?> onChecked)
        {
            var tableNode = new TreeViewItem();
            var panel = new StackPanel();
            panel.Focusable = false;
            panel.Orientation = Orientation.Horizontal;
            var chkBox = new CheckBox();
            chkBox.Checked += (sedner, e) =>
            {
                onChecked(sedner, true);
            };
            chkBox.Unchecked += (sedner, e) =>
            {
                onChecked(sedner, false);
            };
            chkBox.Focusable = false;
            chkBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            panel.Children.Add(chkBox);
            panel.Children.Add(new TextBlock() { Text = header, Focusable = false });
            tableNode.Header = panel;
            return tableNode;
        }

        CheckBox GetCheckBox(TreeViewItem item)
        {
            var spPanel = item.Header as StackPanel;
            return spPanel.Children[0] as CheckBox;
        }
        TextBlock GetTextBlock(TreeViewItem item)
        {
            var spPanel = item.Header as StackPanel;
            return spPanel.Children[1] as TextBlock;
        }

        private async void wizard_PageChanged(object sender, RoutedEventArgs e)
        {
            switch (wizard.CurrentPage.Name)
            {
                case "wzdSelectContent":
                    Title = "Getting Database Information - Model Wizard";
                    await Task.Factory.StartNew(() =>
                    {
#if !DEBUG
                        try
                        {
#endif
                        var conn = _factory.CreateConnection();
                        //DataTable dt = null;
                        var connectionString = string.Empty;
                        Dispatcher.Invoke(() =>
                        {
                            connectionString = _databaseInfoGetter.ConnectionString;
                        });
                        using (conn)
                        {
                            conn.ConnectionString = connectionString;
                            conn.Open();
                        }
                        WizardContext.DataContext.DatabaseConfig.ConnectionString = connectionString;
                        var schemaMgr = _provider.CreateSchemaManager();
                        _tables = schemaMgr.GetTables().ToDictionary(x => x.Name);
                        Dispatcher.Invoke(() =>
                        {
                            tvContent.Items.Clear();
                            var tableRootNode = GetTreeViewItem("Table", (s, isChecked) =>
                            {
                                var parentChk = s as CheckBox;
                                var rootNode = ((StackPanel)parentChk.Parent).Parent as TreeViewItem;
                                foreach (TreeViewItem item in rootNode.Items)
                                {
                                    var spPanel = item.Header as StackPanel;
                                    var chkBox = spPanel.Children[0] as CheckBox;
                                    chkBox.IsChecked = isChecked;
                                }

                                //wizard.CanFinish = isChecked.Value;
                            });
                            var rootChk = (tableRootNode.Header as StackPanel).Children[0] as CheckBox;
                            tvContent.Items.Add(tableRootNode);
                            foreach (var row in _tables)
                            {
                                var tableNode = GetTreeViewItem(row.Key, (s, isChecked) =>
                                {
                                    ToggleFinishButton();
                                });
                                tableRootNode.Items.Add(tableNode);
                            }
                            Title = wizard.CurrentPage.Title + " - Model Wizard";
                        });
#if !DEBUG
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(this, ex.ToString());
                            });
                        }
#endif
                    });
                    wizard.CanFinish = true;
                    break;
                case "wzdSelectDataBase":
                case "wzdSelectDataBaseInfo":
                    Title = wizard.CurrentPage.Title + " - Model Wizard";
                    break;
            }
        }

        private void txtFilter_KeyUp(object sender, KeyEventArgs e)
        {
            Filter = txtFilter.Text.Trim().ToLower();
            var tablesNode = tvContent.Items[0] as TreeViewItem;
            foreach (TreeViewItem item in tablesNode.Items)
            {
                var viewItem = item;
                var textBlock = GetTextBlock(viewItem);
                if (string.IsNullOrWhiteSpace(Filter))
                {
                    viewItem.Visibility = Visibility.Visible;
                    continue;
                }
                if (textBlock.Text.ToLower().Contains(Filter))
                {
                    viewItem.Visibility = Visibility.Visible;
                }
                else
                {
                    viewItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        void ToggleFinishButton()
        {
            return;
            ModelName = txtModelName.Text.Trim();
            if (string.IsNullOrWhiteSpace(ModelName))
            {
                wizard.CanFinish = false;
                return;
            }
            foreach (TreeViewItem item in (tvContent.Items[0] as TreeViewItem).Items)
            {
                var isChecked = GetCheckBox(item).IsChecked;
                if (isChecked != null && isChecked.Value)
                {
                    wizard.CanFinish = true;
                    return;
                }
            }
            wizard.CanFinish = false;
        }

        private void wizard_Finish(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ModelName))
            {
                MessageBox.Show("The model name should be required");
                e.Handled = true;
                return;
            }
            if (char.IsNumber(ModelName[0]))
            {
                ModelName = "_" + ModelName;
            }
            var tablesItem = tvContent.Items[0] as TreeViewItem;
            var tables = new List<EasyORM.SchemaModel.Table>();
            foreach (TreeViewItem item in tablesItem.Items)
            {
                if (item.Visibility != System.Windows.Visibility.Visible)
                {
                    continue;
                }
                var chk = GetCheckBox(item);
                if (chk.IsChecked == null || !chk.IsChecked.Value)
                {
                    continue;
                }
                var tb = GetTextBlock(item);
                tables.Add(_tables.GetOrDefault(tb.Text.Trim()));
            }
            Title = "Generating model - Model Wizard";
            var defaultNamespace = ProjectHelper.GetNamespace(projectFile, _folder);
            var config = EasyORM.VSExtension.Configuration.ConfigurationManager.Open(configFile);
            var easyORMConfigSection = config.GetConfigSection("easyORM");

            if (easyORMConfigSection == null)
            {
                config.AddConfigSection("easyORM", "EasyORM.Configuration.ConfigSection, EasyORM");
                easyORMConfigSection = config.GetConfigSection("easyORM");
            }
            var easyORMSection = (EasyORM.Configuration.ConfigSection)config.GetSection("easyORM");
            if (easyORMSection == null)
            {
                easyORMSection = new EasyORM.Configuration.ConfigSection();
                easyORMSection.ConnectionStringName = _model.ConnectionStringName;
                easyORMSection.DataBase = _model.DatabaseType.ToString();
                easyORMSection.DbFactoryName = _model.DbFactoryName;
                easyORMSection.IsAutoCreateTables = true.ToString();
                easyORMSection.IsEnableAllwayAutoCreateTables = false.ToString();
                config.AddSection("easyORM", easyORMSection);
            }
            else
            {
                easyORMSection.ConnectionStringName = _model.ConnectionStringName;
                easyORMSection.DataBase = _model.DatabaseType.ToString();
                easyORMSection.DbFactoryName = _model.DbFactoryName;
                easyORMSection.IsAutoCreateTables = true.ToString();
                easyORMSection.IsEnableAllwayAutoCreateTables = false.ToString();
                config.UpdateSection("easyORM", easyORMSection);
            }
            var connectionString = _databaseInfoGetter.ConnectionString;
            config.SetConnectionString(_model.ConnectionStringName, connectionString);
            config.Save();
            var schemaManager = _provider.CreateSchemaManager();
            try
            {
                _model.Tables = tables;
                _model.Namespace = defaultNamespace;
                var content = JsonConvert.SerializeObject(_model);
                var dataContextName = StringHelper.ToPascal(ParserUtils.GetStandardIdentifier(ModelName));
                var modelPath = System.IO.Path.Combine(_folder, dataContextName + ".xdbm");
                File.WriteAllText(modelPath, content);
                GeneratedItems.Add(modelPath);
                if (chkGenerateDataContext.IsChecked.HasValue && chkGenerateDataContext.IsChecked.Value)
                {
                    var dataContextCode = GeneratorUtils.GenerateDataContext(defaultNamespace, _model.ConnectionStringName, dataContextName, tables);
                    var dataContextPath = System.IO.Path.Combine(_folder, dataContextName) + ".cs";
                    File.WriteAllText(dataContextPath, dataContextCode);
                    GeneratedItems.Add(dataContextPath);
                }
                var codes = GeneratorUtils.GenerateModels(defaultNamespace,WizardContext.DataContext.Provider.CreateTypeMapper(), tables.ToArray());
                GeneratedItems.AddRange(codes.Select(x =>
                {
                    var path = System.IO.Path.Combine(_folder, x.Key) + ".cs";
                    File.WriteAllText(path, x.Value);
                    return path;
                }));
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Generate model failed");
                DialogResult = false;
                //e.Handled = true;
            }
        }

        private void txtModelName_KeyUp(object sender, KeyEventArgs e)
        {
            ToggleFinishButton();
        }


    }
}
