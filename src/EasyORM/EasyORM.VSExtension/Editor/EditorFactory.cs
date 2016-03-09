using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.VSExtension.Editor
{
    [Guid(Guids.guidEditorFactoryString)]
    public class EditorFactory : IVsEditorFactory, IDisposable
    {
        private VSExtensionPackage _editorPackage;
        private ServiceProvider _vsServiceProvider;
        private Project _project;
        public EditorFactory(VSExtensionPackage package,Project project)
        {
            _project = project;
            this._editorPackage = package;
        }
        public void Dispose()
        {
            if (_vsServiceProvider != null)
            {
                _vsServiceProvider.Dispose();
            }
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }
        public object GetService(Type serviceType)
        {
            return _vsServiceProvider.GetService(serviceType);
        }

        public int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView, IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            // Initialize to null
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = Guids.guidEditorFactory;
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }
            if (punkDocDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Create the Document (editor)
            var NewEditor = new EditorPanel(_editorPackage,_project);
            ppunkDocView = Marshal.GetIUnknownForObject(NewEditor);
            ppunkDocData = Marshal.GetIUnknownForObject(NewEditor);
            pbstrEditorCaption = "";
            return VSConstants.S_OK;
        }

        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;

            // we support only a single physical view
            if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
            {
                return VSConstants.S_OK; 
            }
            else if (VSConstants.LOGVIEWID.TextView_guid == rguidLogicalView)
            {
                pbstrPhysicalView = null; 
                return VSConstants.S_OK;
            }
            else
            {
                return VSConstants.E_NOTIMPL; 
            }
        }

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            _vsServiceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }
    }
}
