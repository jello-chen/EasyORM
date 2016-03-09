using EnvDTE;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyORM.Utils;
using EasyORM.Provider;
using EasyORM.SchemaModel;
using EasyORM.VSExtension;
using EasyORM.VSExtension.CodeGenerator;
using EasyORM.VSExtension.Utils;

namespace EasyORM.VSExtension.Editor
{
    public class EditorControl : UserControl
    {
        private ContextMenuStrip contextMenuStrip1;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem btnAdd;
        private ToolStripMenuItem btnRemoveAll;
        private ToolStripMenuItem btnRemoveSelected;
        private FlowLayoutPanel flowLayoutPanel1;
        private GeneratedCodeModel _model;
        private List<string> _removedTables = new List<string>();

        public GeneratedCodeModel Model
        {
            get { return _model; }
        }
        private Control _currentControl;
        public bool IsSaved { get; set; }
        private ProviderBase _provider;
        EntityAddForm _addForm;
        private EnvDTE.Project _project;
        private string _folder;
        private string _defaultNamespace;
        private string _fileName;
        private ToolStripMenuItem btnUpdate;
        private string selectedFolder;
        private SchemaManagerBase _schemaManager;

        public EditorControl(EnvDTE.Project project)
        {
            InitializeComponent();
            IsSaved = true;
            this._project = project;
        }

        public string GetSavingContent()
        {
            IEnumerator enumerator = null;
            var relativePath = _folder.Replace(Path.GetDirectoryName(_project.FullName), string.Empty);
            if (string.IsNullOrWhiteSpace(relativePath) || relativePath == "\\")
            {
                enumerator = _project.ProjectItems.GetEnumerator();
            }
            else
            {
                var folders = relativePath.Split('\\').Where(x => !string.IsNullOrWhiteSpace(x));
                ProjectItem projectItem = null;
                foreach (var folder in folders)
                {
                    if (projectItem == null)
                    {
                        projectItem = (ProjectItem)_project.ProjectItems.Item(folder);
                    }
                    else
                    {
                        projectItem = (ProjectItem)projectItem.ProjectItems.Item(folder);
                    }
                }
                enumerator = projectItem.ProjectItems.GetEnumerator();
            }
            foreach (var removedTable in _removedTables)
            {
                var removeingTable = StringHelper.ToSingular(removedTable.ToUpper());
                while (enumerator.MoveNext())
                {
                    var projectItem = (ProjectItem)enumerator.Current;
                    if (projectItem.Name.Split('.').FirstOrDefault().ToUpper() == removeingTable)
                    {
                        projectItem.Delete();
                    }
                }
            }
            _removedTables.Clear();
            if (_model.GenerateAll)
            {
                var path = Path.Combine(selectedFolder, Path.GetFileNameWithoutExtension(_fileName)) + ".cs";
                var dataContextCode = GeneratorUtils.GenerateDataContext(_defaultNamespace, _model.ConnectionStringName, Path.GetFileNameWithoutExtension(_fileName), _model.Tables);
                File.WriteAllText(path, dataContextCode);
                _project.ProjectItems.AddFromFile(path);
            }

            var tableCodes = GeneratorUtils.GenerateModels(_defaultNamespace, WizardContext.DataContext.Provider.CreateTypeMapper(), _model.Tables.ToArray());
            foreach (var tableName in tableCodes.Keys)
            {
                var path = Path.Combine(selectedFolder, tableName) + ".cs";
                File.WriteAllText(path, tableCodes[tableName]);
                _project.ProjectItems.AddFromFile(path);
            }
            IsSaved = true;
            return JsonConvert.SerializeObject(_model);
        }

        #region InitializeComponent
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRemoveSelected = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.btnUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.ContextMenuStrip = this.contextMenuStrip1;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(540, 445);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.flowLayoutPanel1_Paint);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAdd,
            this.btnUpdate,
            this.btnRemoveSelected,
            this.btnRemoveAll});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(125, 92);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // btnAdd
            // 
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(152, 22);
            this.btnAdd.Text = "Add Model";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemoveSelected
            // 
            this.btnRemoveSelected.Name = "btnRemoveSelected";
            this.btnRemoveSelected.Size = new System.Drawing.Size(152, 22);
            this.btnRemoveSelected.Text = "Delete Selected";
            this.btnRemoveSelected.Click += new System.EventHandler(this.btnRemoveSelected_Click);
            // 
            // btnRemoveAll
            // 
            this.btnRemoveAll.Name = "btnRemoveAll";
            this.btnRemoveAll.Size = new System.Drawing.Size(152, 22);
            this.btnRemoveAll.Text = "Delete All";
            this.btnRemoveAll.Click += new System.EventHandler(this.btnRemoveAll_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(152, 22);
            this.btnUpdate.Text = "Update Model";
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // EditorControl
            // 
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "EditorControl";
            this.Size = new System.Drawing.Size(540, 445);
            this.Load += new System.EventHandler(this.EditorControl_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void EditorControl_Load(object sender, EventArgs e)
        {

        }

        internal void LoadModel(string pszFilename)
        {
            _folder = Path.GetDirectoryName(pszFilename);
            _fileName = pszFilename;
            flowLayoutPanel1.Controls.Clear();
            _model = JsonConvert.DeserializeObject<GeneratedCodeModel>(File.ReadAllText(pszFilename));
            var configFile = string.Empty;
            selectedFolder = Path.GetDirectoryName(pszFilename);
            var projectFile = ProjectHelper.GetProjectFile(selectedFolder);
            try
            {
                configFile = ProjectHelper.GetConfigFile(selectedFolder);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Not found the configuration file");
                return;
            }
            if (string.IsNullOrWhiteSpace(_model.Namespace))
            {
                _model.Namespace = ProjectHelper.GetNamespace(projectFile, _folder);
            }
            _defaultNamespace = _model.Namespace;// ProjectHelper.GetDefaultNamespace(projectFile);
            var connectionString = Configuration.ConfigurationManager.Open(configFile).GetConnectionStrings().GetOrDefault(_model.ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show(this, "Not found the connection string : " + _model.ConnectionStringName);
                return;
            }
            var providerName = string.Empty;
            foreach (var item in DataContext.SupportProviders)
            {
                if (item.Key == _model.DatabaseType)
                {
                    providerName = item.Value;
                    break;
                }
            }
            var context = new DataContext(connectionString, providerName);
            WizardContext.DataContext = context;
            _provider = WizardContext.DataContext.Provider;
            _schemaManager = _provider.CreateSchemaManager();
            foreach (Table item in _model.Tables)
            {
                AddTable(item);
            }
        }

        void AddTable(Table item)
        {
            GroupBox box = new GroupBox();
            box.Text = item.Name;
            box.Height = 300;
            box.Width = 150;
            box.Padding = new Padding(10);
            var lb = new ListBox();
            foreach (var column in item.Columns)
            {
                lb.Items.Add(column.Value.Name);
            }
            lb.Dock = DockStyle.Fill;
            box.Controls.Add(lb);
            flowLayoutPanel1.Controls.Add(box);
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            if (_currentControl == null)
            {
                MessageBox.Show(this, "Not select model");
                return;
            }
            if (_currentControl is ListBox)
            {
                _currentControl = _currentControl.Parent;
            }
            var name = _currentControl.Text;
            if (MessageBox.Show(this, "Are you sure to delete model " + name + "（not delete from database）?", "Tips", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                flowLayoutPanel1.Controls.Remove(_currentControl);
                _model.Tables.Remove(_model.Tables.FirstOrDefault(x => x.Name == name));
                IsSaved = false;
                _removedTables.Add(name);
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _currentControl = flowLayoutPanel1.GetChildAtPoint(flowLayoutPanel1.PointToClient(Control.MousePosition));
            btnRemoveSelected.Enabled = _currentControl is GroupBox || _currentControl is ListBox;
            btnRemoveAll.Enabled = flowLayoutPanel1.Controls.Count > 0;
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure to delete all models（not delete from database）?", "Tips", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                flowLayoutPanel1.Controls.Clear();
                _model.Tables.Clear();
                IsSaved = false;
            }
        }

        GroupBox GetTableGroupBox(string tableName)
        {
            foreach (Control item in flowLayoutPanel1.Controls)
            {
                if (item.Text == tableName)
                {
                    return (GroupBox)item;
                }
            }
            return null;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            _addForm = new EntityAddForm(_provider, _model.Tables);
            if (_addForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            foreach (var item in _addForm.SelectedTables)
            {
                var table = _schemaManager.GetTable(item);
                _model.Tables.Add(table);
                AddTable(table);
            }
            IsSaved = false;
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_currentControl == null)
            {
                MessageBox.Show(this, "Not select models");
                return;
            }
            if (_currentControl is ListBox)
            {
                _currentControl = _currentControl.Parent;
            }
            var name = _currentControl.Text;
            var table = _schemaManager.GetTable(name);
            var rawTable = _model.Tables.FirstOrDefault(x => x.Name == name);
            if (rawTable == null)
            {
                MessageBox.Show(this, "Error occurred, please try to delete model and generate again");
                return;
            }
            var box = GetTableGroupBox(name);
            if (box == null)
            {
                MessageBox.Show(this, "Error occurred, please try to delete model and generate again");
                return;
            }
            if (box.Controls.Count <= 0)
            {
                MessageBox.Show(this, "Error occurred, please try to delete model and generate again");
                return;
            }
            var listBox = (ListBox)box.Controls[0];
            listBox.Items.Clear();
            foreach (var column in table.Columns)
            {
                listBox.Items.Add(column.Value.Name);
            }
            IsSaved = false;
            rawTable.Columns = table.Columns;
        }
    }
}
