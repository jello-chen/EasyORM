using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.VSExtension.Editor
{

    public class EditorProperties
    {
        private EditorPanel _editorPanel;
        public EditorProperties(EditorPanel editorPanel)
        {
            _editorPanel = editorPanel;
        }

        EditorControl editor
        {
            get
            {
                if (_editorPanel.Window == null)
                {
                    return null;
                }
                return (EditorControl)_editorPanel.Window;
            }
        }

        [Description("Is the DataContext class generated while generating the Model layer. Note that if you manually modify the DataContext class, you will be lost when you re generated.")]
        [DefaultValue(false)]
        public bool GenerateAll
        {
            get
            {
                if (editor == null || editor.Model == null)
                {
                    return false;
                }
                return editor.Model.GenerateAll;
            }
            set
            {
                if (editor != null && editor.Model != null)
                {
                    editor.Model.GenerateAll = value;
                }
            }
        }
    }
}
