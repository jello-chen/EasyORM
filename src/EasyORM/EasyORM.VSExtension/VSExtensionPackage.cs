using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.IO;
using EasyORM.VSExtension.Editor;
using EnvDTE;

namespace EasyORM.VSExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.guidEasyORM_XLinq_VSExtensionPkgString)]
    //[ProvideOptionPage(typeof(Class1),
    //    "My Category", "My Custom Page", 0, 0, true)]
    [ProvideKeyBindingTable(Guids.guidEditorFactoryString, 102)]
    [ProvideEditorLogicalView(typeof(EditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideEditorExtension(typeof(EditorFactory), ".xdbm", 50,
              ProjectGuid = "{A2FE74E1-B743-11d0-AE1A-00A0C90FFFC3}",
              NameResourceID = 105,
              DefaultName = "EasyORM Model Editor")]
    public sealed class VSExtensionPackage : Package
    {
        private DTE dte;
        private Project _project;
        public VSExtensionPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            dte = GetService(typeof(SDTE)) as DTE;
            var projArray = dte.ActiveSolutionProjects as Array;
            _project = projArray.GetValue(0) as Project;
            base.RegisterEditorFactory(new EditorFactory(this, _project));
            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(Guids.guidEasyORM_XLinq_VSExtensionCmdSet, (int)PkgCmdIDList.cmdidaddeasyORMmodel);
                OleMenuCommand menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
            }
            base.Initialize();
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Show a Message Box to prove we were here
            var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (monitorSelection == null || solution == null)
            {

            }
            IVsMultiItemSelect multiItemSelect = null;
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainerPtr = IntPtr.Zero;

            IVsHierarchy hierarchy = null;
            var itemid = VSConstants.VSITEMID_NIL;
            int hr = VSConstants.S_OK;
            hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);
            hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
            var itemFullPath = string.Empty;
            var project = ((IVsProject)hierarchy);
            project.GetMkDocument(itemid, out itemFullPath);
            WizardWindow window = new WizardWindow(Path.GetDirectoryName(itemFullPath),_project);
            var r = window.ShowDialog();
            if (r.HasValue && r.Value)
            {
                foreach (var item in window.GeneratedItems)
                {
                    VSADDRESULT[] results = new VSADDRESULT[1];
                    var i = project.AddItem((uint)itemid, VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE, item, (uint)1, new string[] { item }, IntPtr.Zero, results);
                    if (results[0] != VSADDRESULT.ADDRESULT_Success)
                    {
                        throw new Exception("Add file or directory " + item + " failed");
                    }
                }
            }
            dte.ExecuteCommand("SaveAll");
        }

    }
}
