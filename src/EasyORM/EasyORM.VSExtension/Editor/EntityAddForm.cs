using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyORM.Provider;
using EasyORM.SchemaModel;

namespace EasyORM.VSExtension.Editor
{
    public partial class EntityAddForm : Form
    {
        List<Table> _existsEntities;
        ProviderBase _provider;
        List<Table> _addingEntities;
        private List<string> _entities;
        public List<string> SelectedTables
        {
            get
            {
                return _entities;
            }
        }
        public EntityAddForm(ProviderBase provider, List<Table> existsEntities)
        {
            InitializeComponent();
            _existsEntities = existsEntities;
            _provider = provider;
        }

        private async void EntityAddForm_Load(object sender, EventArgs e)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var schemaManager = _provider.CreateSchemaManager();
                    var allTables = schemaManager.GetTables();
                    _addingEntities = allTables.Where(x => !_existsEntities.Where(y => y.Name == x.Name).Any()).ToList();
                    foreach (var entity in _addingEntities)
                    {
                        Invoke(new Action(() =>
                        {
                            tvTables.Nodes.Add(entity.Name);
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Load tables faild：" + ex.Message);
            }
        }
        private void txtFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            string filter = txtFilter.Text.Trim();
            tvTables.Nodes.Clear();
            foreach (var node in _addingEntities)
            {
                if (string.IsNullOrWhiteSpace(filter) || node.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tvTables.Nodes.Add(node.Name);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            _entities = new List<string>();
            foreach (TreeNode item in tvTables.Nodes)
            {
                if(item.Checked)
                {
                    _entities.Add(item.Text);
                }
            }

            if (!_entities.Any())
            {
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
