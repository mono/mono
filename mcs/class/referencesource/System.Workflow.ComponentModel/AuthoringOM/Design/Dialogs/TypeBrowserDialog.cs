namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.Resources;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Collections.Specialized;
    using System.Windows.Forms.Design;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Interop;
    using System.Text;
    using System.Globalization;
    using Microsoft.Win32;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class TypeBrowserDialog : Form, ISite
    {
        private static ResourceManager ResMgr;

        private IServiceProvider serviceProvider;
        private TypeProvider localTypeProvider;
        private ImageList artifactImages;
        private string selectedTypeName;
        private IntPtr bitmapSortUp;
        private IntPtr bitmapSortDown;
        private HelpTextWindow helpTextWindow;
        private ITypeFilterProvider typeFilterProvider;
        private bool sortListViewAscending;
        private bool refreshTreeView;
        private bool refreshTypeTextBox;
        private string lastComboboxValue = null;
        private Type selectedType = null;
        private GenericParameters genericParameters = new GenericParameters();

        #region Designer Generated Variables

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox helpTextHolder;
        private System.Windows.Forms.TabControl tabControl;
        private TabPage typeTabPage;
        private TabPage advancedTabPage;
        private TextBox typeTextBox;
        private Button buttonBrowse;
        private PropertyGrid genericParametersPropertyGrid;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private TableLayoutPanel typeNameTableLayoutPanel;
        private TextBox artifactLabel;
        private SplitContainer typeSplitContainer;
        private TreeView artifactTreeView;
        private ListView artifactListView;
        private ColumnHeader typeName;
        private ColumnHeader fullyQualifiedName;
        private Label typeNameLabel;
        #endregion

        #region Construction and Destruction
        static TypeBrowserDialog()
        {
            TypeBrowserDialog.ResMgr = new ResourceManager("System.Workflow.ComponentModel.Design.ArtifactReference", System.Reflection.Assembly.GetExecutingAssembly());
        }

        public TypeBrowserDialog(IServiceProvider serviceProvider, ITypeFilterProvider filterProvider, string selectedTypeName, TypeProvider typeProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            this.localTypeProvider = typeProvider;
            this.serviceProvider = serviceProvider;
            // Load assemblies specified in the registry
            Helpers.AddTypeProviderAssembliesFromRegistry(this.localTypeProvider, serviceProvider);

            InitializeDialog(serviceProvider, filterProvider, selectedTypeName);

            this.buttonBrowse.Visible = true;
            this.buttonBrowse.Enabled = true;
            this.buttonBrowse.BringToFront();
        }

        public TypeBrowserDialog(IServiceProvider serviceProvider, ITypeFilterProvider filterProvider, string selectedTypeName)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            InitializeDialog(serviceProvider, filterProvider, selectedTypeName);
        }

        internal void InitializeDialog(IServiceProvider serviceProvider, ITypeFilterProvider filterProvider, string selectedTypeName)
        {
            this.serviceProvider = serviceProvider;
            this.sortListViewAscending = true;
            this.refreshTreeView = false;
            this.refreshTypeTextBox = false;
            this.selectedTypeName = selectedTypeName;
            this.typeFilterProvider = filterProvider;

            IDesignerHost designerHost = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            WorkflowDesignerLoader loader = this.serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (designerHost == null || designerHost.RootComponent == null || loader == null || loader.InDebugMode)
                throw new Exception(DR.GetString(DR.Error_WorkflowNotLoaded));

            InitializeComponent();
            CustomInitializeComponent();

            this.genericParametersPropertyGrid.Site = new DummySite(this.serviceProvider);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion

        #region Designer Generated Code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor OR WITH THE DESIGNER.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TypeBrowserDialog));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.helpTextHolder = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.typeTabPage = new System.Windows.Forms.TabPage();
            this.typeSplitContainer = new System.Windows.Forms.SplitContainer();
            this.artifactTreeView = new System.Windows.Forms.TreeView();
            this.artifactListView = new System.Windows.Forms.ListView();
            this.typeName = new System.Windows.Forms.ColumnHeader();
            this.fullyQualifiedName = new System.Windows.Forms.ColumnHeader();
            this.advancedTabPage = new System.Windows.Forms.TabPage();
            this.genericParametersPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.typeTextBox = new System.Windows.Forms.TextBox();
            this.typeNameLabel = new System.Windows.Forms.Label();
            this.okCancelTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.typeNameTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.artifactLabel = new System.Windows.Forms.TextBox();
            this.tabControl.SuspendLayout();
            this.typeTabPage.SuspendLayout();
            this.typeSplitContainer.Panel1.SuspendLayout();
            this.typeSplitContainer.Panel2.SuspendLayout();
            this.typeSplitContainer.SuspendLayout();
            this.advancedTabPage.SuspendLayout();
            this.okCancelTableLayoutPanel.SuspendLayout();
            this.typeNameTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            // 
            // helpTextHolder
            // 
            resources.ApplyResources(this.helpTextHolder, "helpTextHolder");
            this.helpTextHolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.helpTextHolder.Name = "helpTextHolder";
            this.helpTextHolder.ReadOnly = true;
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Click += new System.EventHandler(this.OkButtonClicked);
            // 
            // tabControl
            // 
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Controls.Add(this.typeTabPage);
            this.tabControl.Controls.Add(this.advancedTabPage);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            // 
            // typeTabPage
            // 
            this.typeTabPage.BackColor = System.Drawing.Color.Transparent;
            this.typeTabPage.Controls.Add(this.typeSplitContainer);
            resources.ApplyResources(this.typeTabPage, "typeTabPage");
            this.typeTabPage.Name = "typeTabPage";
            // 
            // typeSplitContainer
            // 
            this.typeSplitContainer.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.typeSplitContainer, "typeSplitContainer");
            this.typeSplitContainer.Name = "typeSplitContainer";
            // 
            // typeSplitContainer.Panel1
            // 
            this.typeSplitContainer.Panel1.Controls.Add(this.artifactTreeView);
            // 
            // typeSplitContainer.Panel2
            // 
            this.typeSplitContainer.Panel2.Controls.Add(this.artifactListView);
            this.typeSplitContainer.TabStop = false;
            // 
            // artifactTreeView
            // 
            this.artifactTreeView.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.artifactTreeView, "artifactTreeView");
            this.artifactTreeView.ItemHeight = 16;
            this.artifactTreeView.Name = "artifactTreeView";
            this.artifactTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeSelectionChange);
            this.artifactTreeView.GotFocus += new System.EventHandler(this.OnTreeViewGotFocus);
            // 
            // artifactListView
            // 
            this.artifactListView.AllowColumnReorder = true;
            this.artifactListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.typeName,
            this.fullyQualifiedName});
            resources.ApplyResources(this.artifactListView, "artifactListView");
            this.artifactListView.Name = "artifactListView";
            this.artifactListView.UseCompatibleStateImageBehavior = false;
            this.artifactListView.View = System.Windows.Forms.View.Details;
            this.artifactListView.SelectedIndexChanged += new System.EventHandler(this.OnListViewSelectedIndexChanged);
            this.artifactListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.OnListViewColumnClick);
            // 
            // typeName
            // 
            resources.ApplyResources(this.typeName, "typeName");
            // 
            // fullyQualifiedName
            // 
            resources.ApplyResources(this.fullyQualifiedName, "fullyQualifiedName");
            // 
            // advancedTabPage
            // 
            this.advancedTabPage.BackColor = System.Drawing.Color.Transparent;
            this.advancedTabPage.Controls.Add(this.genericParametersPropertyGrid);
            resources.ApplyResources(this.advancedTabPage, "advancedTabPage");
            this.advancedTabPage.Name = "advancedTabPage";
            // 
            // genericParametersPropertyGrid
            // 
            resources.ApplyResources(this.genericParametersPropertyGrid, "genericParametersPropertyGrid");
            this.genericParametersPropertyGrid.Name = "genericParametersPropertyGrid";
            this.genericParametersPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.genericParametersPropertyGrid.ToolbarVisible = false;
            // 
            // buttonBrowse
            // 
            resources.ApplyResources(this.buttonBrowse, "buttonBrowse");
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Click += new System.EventHandler(this.OnButtonBrowse_Click);
            // 
            // typeTextBox
            // 
            resources.ApplyResources(this.typeTextBox, "typeTextBox");
            this.typeTextBox.Name = "typeTextBox";
            this.typeTextBox.TextChanged += new System.EventHandler(this.OnTypeTextBoxTextChanged);
            // 
            // typeNameLabel
            // 
            resources.ApplyResources(this.typeNameLabel, "typeNameLabel");
            this.typeNameLabel.Name = "typeNameLabel";
            // 
            // okCancelTableLayoutPanel
            // 
            resources.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.Controls.Add(this.buttonOK, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.buttonCancel, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            // 
            // typeNameTableLayoutPanel
            // 
            resources.ApplyResources(this.typeNameTableLayoutPanel, "typeNameTableLayoutPanel");
            this.typeNameTableLayoutPanel.Controls.Add(this.typeNameLabel, 0, 0);
            this.typeNameTableLayoutPanel.Controls.Add(this.typeTextBox, 1, 0);
            this.typeNameTableLayoutPanel.Name = "typeNameTableLayoutPanel";
            // 
            // artifactLabel
            // 
            this.artifactLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.artifactLabel.CausesValidation = false;
            resources.ApplyResources(this.artifactLabel, "artifactLabel");
            this.artifactLabel.Name = "artifactLabel";
            this.artifactLabel.ReadOnly = true;
            this.artifactLabel.TabStop = false;
            // 
            // TypeBrowserDialog
            // 
            this.AcceptButton = this.buttonOK;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.artifactLabel);
            this.Controls.Add(this.typeNameTableLayoutPanel);
            this.Controls.Add(this.okCancelTableLayoutPanel);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.helpTextHolder);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TypeBrowserDialog";
            this.ShowInTaskbar = false;
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.TypeBrowserDialog_HelpButtonClicked);
            this.tabControl.ResumeLayout(false);
            this.typeTabPage.ResumeLayout(false);
            this.typeSplitContainer.Panel1.ResumeLayout(false);
            this.typeSplitContainer.Panel2.ResumeLayout(false);
            this.typeSplitContainer.ResumeLayout(false);
            this.advancedTabPage.ResumeLayout(false);
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            this.typeNameTableLayoutPanel.ResumeLayout(false);
            this.typeNameTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region Properties and Methods
        public Type SelectedType
        {
            get
            {
                return this.selectedType;
            }
        }
        #endregion

        List<Type> GetTargetFrameworkTypes(ITypeProvider currentTypeProvider)
        {
            IExtendedUIService2 extendedUIService = (IExtendedUIService2)this.serviceProvider.GetService(typeof(IExtendedUIService2));
            List<Type> targetFrameworkTypes = new List<Type>();
            if (currentTypeProvider != null)
            {
                if (extendedUIService != null)
                {
                    List<Assembly> runtimeAssemblies = new List<Assembly>(currentTypeProvider.ReferencedAssemblies);
                    foreach (Assembly runtimeAssembly in runtimeAssemblies)
                    {
                        Assembly reflectionContextAssembly = extendedUIService.GetReflectionAssembly(runtimeAssembly.GetName());
                        if (reflectionContextAssembly != null)
                        {
                            foreach (Type type in reflectionContextAssembly.GetTypes())
                            {
                                if (type.IsPublic)
                                {
                                    targetFrameworkTypes.Add(type);
                                }
                            }
                        }
                    }

                    // add design time types from type provider to the list 
                    // and design time types are only for the current user assemblies.
                    foreach (Type type in currentTypeProvider.GetTypes())
                    {
                        if (type.Assembly == null)
                        {
                            targetFrameworkTypes.Add(type);
                        }
                    }
                    
                }
                else
                {
                    //if extendedUIService is null fall back to the type provider.
                    targetFrameworkTypes.AddRange(currentTypeProvider.GetTypes());
                }
            }
            return targetFrameworkTypes;
        }

        #region Events and overrides
        protected override void OnLoad(EventArgs e)
        {
            //Call base class's load
            base.OnLoad(e);

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // add current project node
                TreeNode currentProjectNode = null;
                if (this.localTypeProvider == null)
                    currentProjectNode = this.artifactTreeView.Nodes.Add(SR.CurrentProject, SR.GetString(SR.CurrentProject), 2, 2);

                // add references node
                TreeNode assembliesNode = this.artifactTreeView.Nodes.Add(SR.ReferencedAssemblies, SR.GetString(SR.ReferencedAssemblies), 2, 2);

                ITypeProvider typeProvider = this.TypeProvider;

                AutoCompleteStringCollection autoCompleteStringCollection = new AutoCompleteStringCollection();
                this.UpdateTreeView(GetTargetFrameworkTypes(this.TypeProvider).ToArray(), autoCompleteStringCollection);

                //Select the root node to show all the types.
                assembliesNode.Expand();
                TreeNode selectNode = (currentProjectNode == null) ? assembliesNode : currentProjectNode;
                this.artifactTreeView.SelectedNode = selectNode;
                TreeSelectionChanged(selectNode);
                selectNode.EnsureVisible();

                this.typeTextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                this.typeTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                this.typeTextBox.AutoCompleteCustomSource = autoCompleteStringCollection;

                // now look for selected type
                if (this.selectedTypeName != null)
                {
                    Type actualType = typeProvider.GetType(this.selectedTypeName);
                    if (actualType != null)
                        this.typeTextBox.Text = GetSimpleTypeFullName(actualType);
                }
            }
            catch (FileNotFoundException)
            {
                //Eat the fnf exception from missing dependencies and let the UI load.
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            //Update the state of the controls on the form
            UpdateControlState();
            this.typeTextBox.Select();
        }

        private void UpdateTreeView(Type[] types, AutoCompleteStringCollection autoCompleteStringCollection)
        {
            TreeNode assembliesNode = this.artifactTreeView.Nodes[SR.ReferencedAssemblies];
            Hashtable assemblyNodes = new Hashtable();
            IExtendedUIService2 extendedUIService = (IExtendedUIService2)this.serviceProvider.GetService(typeof(IExtendedUIService2));
            foreach (Type type in types)
            {
                if (this.typeFilterProvider != null && !this.typeFilterProvider.CanFilterType(extendedUIService != null ? extendedUIService.GetRuntimeType(type) : type, false))
                    continue;

                if (autoCompleteStringCollection.Contains(type.FullName))
                    continue;

                autoCompleteStringCollection.Add(type.FullName);

                TreeNode node = null;
                if (type.Assembly != null)
                {
                    node = assemblyNodes[type.Assembly] as TreeNode;
                    if (node == null)
                    {
                        node = new TreeNode(type.Assembly.GetName().Name, 3, 3);
                        node.Tag = type.Assembly;
                        assembliesNode.Nodes.Add(node);
                        assemblyNodes[type.Assembly] = node;
                    }
                }
                else
                {
                    node = this.artifactTreeView.Nodes[SR.CurrentProject];
                }

                if (type.Namespace != null && type.Namespace.Length > 0)
                {
                    bool found = false;
                    string namespaceName = type.Namespace;
                    foreach (TreeNode nsNode in node.Nodes)
                    {
                        if (nsNode.Text == namespaceName)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        TreeNode nsNode = new TreeNode(namespaceName, 49, 49);
                        node.Nodes.Add(nsNode);
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            //Destroy the bitmaps used to show the sorting order
            if (IntPtr.Zero != this.bitmapSortUp)
                NativeMethods.DeleteObject(this.bitmapSortUp);

            if (IntPtr.Zero != this.bitmapSortDown)
                NativeMethods.DeleteObject(this.bitmapSortDown);
        }

        protected override void OnPaint(PaintEventArgs paintArgs)
        {
            base.OnPaint(paintArgs);

            ///Draw the top devider line
            Rectangle lineRectangle = new Rectangle(this.ClientRectangle.Left, this.artifactLabel.Bottom + ((this.typeNameTableLayoutPanel.Top + this.typeTextBox.Top - this.artifactLabel.Bottom) / 2), this.ClientRectangle.Width, 1);
            paintArgs.Graphics.DrawLine(SystemPens.ControlDark, lineRectangle.Left, lineRectangle.Bottom, lineRectangle.Right, lineRectangle.Bottom);
            paintArgs.Graphics.DrawLine(SystemPens.ControlLightLight, lineRectangle.Left, lineRectangle.Bottom + 1, lineRectangle.Right, lineRectangle.Bottom + 1);

            ///Draw the bottom devider line
            lineRectangle = new Rectangle(this.ClientRectangle.Left, this.helpTextHolder.Bottom + ((this.okCancelTableLayoutPanel.Top + this.buttonOK.Top - this.helpTextHolder.Bottom) / 2), this.ClientRectangle.Width, 1);
            paintArgs.Graphics.DrawLine(SystemPens.ControlDark, lineRectangle.Left, lineRectangle.Bottom, lineRectangle.Right, lineRectangle.Bottom);
            paintArgs.Graphics.DrawLine(SystemPens.ControlLightLight, lineRectangle.Left, lineRectangle.Bottom + 1, lineRectangle.Right, lineRectangle.Bottom + 1);

            //Draw help text border
            paintArgs.Graphics.DrawLine(SystemPens.WindowFrame, this.helpTextHolder.Left - 1, this.helpTextHolder.Top - 1, this.helpTextHolder.Left - 1, this.helpTextHolder.Bottom);
            paintArgs.Graphics.DrawLine(SystemPens.WindowFrame, this.helpTextHolder.Left - 1, this.helpTextHolder.Bottom, this.helpTextHolder.Right, this.helpTextHolder.Bottom);
            paintArgs.Graphics.DrawLine(SystemPens.WindowFrame, this.helpTextHolder.Right, this.helpTextHolder.Bottom, this.helpTextHolder.Right, this.helpTextHolder.Top - 1);
            paintArgs.Graphics.DrawLine(SystemPens.WindowFrame, this.helpTextHolder.Right, this.helpTextHolder.Top - 1, this.helpTextHolder.Left - 1, this.helpTextHolder.Top - 1);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (this.lastComboboxValue != this.typeTextBox.Text)
                {
                    this.lastComboboxValue = this.typeTextBox.Text;
                    this.typeTextBox.Text = string.Empty;
                    this.typeTextBox.Text = this.lastComboboxValue;
                    this.typeTextBox.SelectionStart = this.typeTextBox.Text.Length;
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OkButtonClicked(object sender, System.EventArgs e)
        {
            try
            {
                ITypeProvider typeProvider = this.TypeProvider;
                Type type = typeProvider.GetType(this.typeTextBox.Text);
                if ((type != null) && (this.typeFilterProvider == null || this.typeFilterProvider.CanFilterType(type, false)))
                {
                    this.selectedTypeName = type.AssemblyQualifiedName;
                    this.selectedType = type;
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    DialogResult = DialogResult.None;
                }
            }
            catch (Exception ex)
            {
                DesignerHelpers.ShowError(this.serviceProvider, ex);
            }
        }

        private void OnTreeSelectionChange(object sender, TreeViewEventArgs e)
        {
            TreeSelectionChanged(e.Node);
        }

        private void OnTreeViewGotFocus(object sender, EventArgs e)
        {
            //We always ignore the first set focus as treeview is first in the tab order
            //We need to refresh the list contents based on tree selection whenever focus changes from listview to treeview
            if (this.refreshTreeView)
            {
                this.refreshTreeView = false;
                if (this.artifactTreeView.SelectedNode != null)
                    TreeSelectionChanged(this.artifactTreeView.SelectedNode);
            }
        }

        private void OnListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if ((!this.refreshTypeTextBox) && (this.artifactListView.SelectedItems.Count > 0))
                    this.typeTextBox.Text = (this.artifactListView.SelectedItems[0].Tag as Type).FullName;

                if (this.artifactListView.SelectedItems.Count > 0)
                    this.artifactListView.SelectedItems[0].EnsureVisible();

                //Update the namespace selection in tree based on listview selection
                if (!this.artifactListView.Focused || this.artifactListView.SelectedItems.Count == 0)
                    return;

                Type type = this.artifactListView.SelectedItems[0].Tag as Type;
                if (type != null)
                    ListSelectionChanged(type);
            }
            catch (Exception ex)
            {
                DesignerHelpers.ShowError(this.serviceProvider, ex);
            }
        }

        private void OnListViewMouseDown(object sender, MouseEventArgs mouseArgs)
        {
            //Close the dialog on double click
            if (mouseArgs.Clicks > 1 && 
            this.artifactListView.SelectedItems.Count > 0 && 
            this.artifactListView.SelectedItems[0].Tag is Type &&
            this.buttonOK.Enabled == true)
                OkButtonClicked(this.buttonOK, EventArgs.Empty);
        }

        private void OnListViewColumnClick(object sender, ColumnClickEventArgs e)
        {
            //Sort the items in listview
            this.sortListViewAscending = !this.sortListViewAscending;
            SortListViewItems(e.Column);
        }

        private void TypeBrowserDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            GetHelp();
        }

        protected override void OnHelpRequested(HelpEventArgs e)
        {
            e.Handled = true;
            GetHelp();
        }

        private void GetHelp()
        {
            DesignerHelpers.ShowHelpFromKeyword(this.serviceProvider, typeof(TypeBrowserDialog).FullName + ".UI");
        }

        private void OnTypeTextBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            OnTypeTextBoxTextChanged(sender, e);
        }

        private void OnTypeTextBoxTextChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.refreshTypeTextBox)
                    return;
                this.refreshTypeTextBox = true;
                ITypeProvider typeProvider = this.TypeProvider;
                Type actualType = typeProvider.GetType(this.typeTextBox.Text);

                if (actualType != null)
                {
                    this.lastComboboxValue = this.typeTextBox.Text;

                    Type baseType = null;
                    ParameterData[] parameterDataArray = null;
                    int[] arrayRanks = null;

                    GetTypeParts(actualType, out baseType, out parameterDataArray, out arrayRanks);
                    this.genericParameters.Parameters = parameterDataArray != null ? parameterDataArray : new ParameterData[0];
                    this.genericParametersPropertyGrid.Refresh();

                    ListSelectionChanged(baseType);

                    this.genericParametersPropertyGrid.Enabled = baseType.IsGenericTypeDefinition;

                    foreach (ListViewItem lvItem in this.artifactListView.Items)
                    {
                        Type type = lvItem.Tag as Type;
                        if (type != null && (type.FullName.Equals(baseType.FullName)))
                        {
                            if (!lvItem.Selected)
                                lvItem.Selected = true;
                            break;
                        }
                        else
                            lvItem.Selected = false;
                    }
                }
                else
                {
                    if (this.artifactListView.SelectedItems.Count != 0)
                        this.artifactListView.SelectedItems[0].Selected = false;

                    this.genericParameters.Parameters = new ParameterData[0];
                    this.genericParametersPropertyGrid.Enabled = false;
                }

                //Update the state of the controls on the form
                UpdateControlState();

                this.refreshTypeTextBox = false;
            }
            catch (Exception ex)
            {
                DesignerHelpers.ShowError(this.serviceProvider, ex);
            }
        }

        private void OnButtonBrowse_Click(object Sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = DR.GetString(DR.OpenfileDialogTitle);
            fileDialog.AddExtension = true;
            fileDialog.DefaultExt = "dll";
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            fileDialog.DereferenceLinks = true;
            fileDialog.ValidateNames = true;
            fileDialog.Filter = DR.GetString(DR.PackageAssemblyReferenceFilter);
            fileDialog.RestoreDirectory = false;
            if (fileDialog.ShowDialog(this) == DialogResult.OK)
            {
                //this is an inline delegate who'll be handling the type loader event
                EventHandler typeLoadErrorsChangedHandler = delegate(object sender, EventArgs ea)
                {
                    Exception exception = null;
                    if (this.localTypeProvider.TypeLoadErrors.ContainsKey(fileDialog.FileName))
                        exception = this.localTypeProvider.TypeLoadErrors[fileDialog.FileName];

                    if (exception != null)
                    {
                        string errorMessage = (exception is ReflectionTypeLoadException || (exception.InnerException != null && exception.InnerException is ReflectionTypeLoadException)) ? DR.GetString(DR.TypeBrowser_UnableToLoadOneOrMoreTypes) : DR.GetString(DR.TypeBrowser_ProblemsLoadingAssembly);
                        errorMessage = string.Format(CultureInfo.CurrentCulture, errorMessage, new object[] { fileDialog.FileName });
                        DesignerHelpers.ShowError(this.serviceProvider, errorMessage);
                    }
                };

                try
                {
                    ITypeProviderCreator typeProviderCreator = this.serviceProvider.GetService(typeof(ITypeProviderCreator)) as ITypeProviderCreator;
                    if (typeProviderCreator != null)
                        this.localTypeProvider.AddAssembly(typeProviderCreator.GetTransientAssembly(AssemblyName.GetAssemblyName(fileDialog.FileName)));
                    else
                        this.localTypeProvider.AddAssemblyReference(fileDialog.FileName);

                    Helpers.UpdateTypeProviderAssembliesRegistry(fileDialog.FileName);
                    this.localTypeProvider.TypeLoadErrorsChanged += typeLoadErrorsChangedHandler;

                    UpdateTreeView(GetTargetFrameworkTypes(this.localTypeProvider).ToArray(), this.typeTextBox.AutoCompleteCustomSource);
                }
                catch (FileNotFoundException fnf)
                {
                    DesignerHelpers.ShowError(this.serviceProvider, fnf.Message);
                }
                catch (BadImageFormatException)
                {
                    DesignerHelpers.ShowError(this.serviceProvider, SR.GetString(SR.Error_AssemblyBadImage, fileDialog.FileName));
                }
                catch (FileLoadException)
                {
                    DesignerHelpers.ShowError(this.serviceProvider, SR.GetString(SR.Error_AssemblyBadImage, fileDialog.FileName));
                }
                catch (Exception ex)
                {
                    DesignerHelpers.ShowError(this.serviceProvider, SR.GetString(SR.Error_AddAssemblyRef, fileDialog.FileName, ex.Message));
                }
                finally
                {
                    this.localTypeProvider.TypeLoadErrorsChanged -= typeLoadErrorsChangedHandler;
                }
            }
        }

        #endregion

        #region Helpers

        private void CustomInitializeComponent()
        {
            this.SuspendLayout();
            this.artifactTreeView.Sorted = true;

            //Load the bitmap
            Bitmap bitmap = TypeBrowserDialog.ResMgr.GetObject("IDB_SORTUP") as Bitmap;
            this.bitmapSortUp = bitmap.GetHbitmap();

            bitmap = TypeBrowserDialog.ResMgr.GetObject("IDB_SORTDOWN") as Bitmap;
            this.bitmapSortDown = bitmap.GetHbitmap();

            //Create imagelist for tree and list
            this.artifactImages = new ImageList();
            this.artifactImages.TransparentColor = Color.FromArgb(0, 255, 0);
            this.artifactImages.Images.AddStrip((Image)TypeBrowserDialog.ResMgr.GetObject("IDB_ARTIFACTIMAGES"));

            //DO NOT CHANGE THE CREATION SEQUENCE OF FOLLOWING CONTROLS

            //Set the listview style
            this.artifactListView.Dock = DockStyle.Fill;
            this.artifactListView.FullRowSelect = true;
            this.artifactListView.HideSelection = false;
            this.artifactListView.MultiSelect = false;
            this.artifactListView.SmallImageList = this.artifactImages;
            this.artifactListView.MouseDown += new MouseEventHandler(OnListViewMouseDown);

            //Set the treeview styles
            this.artifactTreeView.HideSelection = false;
            this.artifactTreeView.ImageList = this.artifactImages;
            //DO NOT CHANGE THE CREATION SEQUENCE OF ABOVE CONTROLS

            //Change the HelpText holder to a Rich Edit Control
            this.helpTextHolder.Visible = false;
            this.helpTextWindow = new HelpTextWindow();
            this.helpTextWindow.Parent = this;
            this.helpTextWindow.Location = new Point(this.helpTextHolder.Location.X + 3, this.helpTextHolder.Location.Y + 3);
            this.helpTextWindow.Size = new Size(this.helpTextHolder.Size.Width - 6, this.helpTextHolder.Size.Height - 6);

            //Set the text based on the browsed type
            if (this.typeFilterProvider != null)
                this.artifactLabel.Text = this.typeFilterProvider.FilterDescription;

            //Set dialog fonts
            IUIService uisvc = (IUIService)this.serviceProvider.GetService(typeof(IUIService));
            if (uisvc != null)
                this.Font = (Font)uisvc.Styles["DialogFont"];

            //Set the font for the artifact label
            this.artifactLabel.Font = new Font(this.Font.Name, this.Font.SizeInPoints, FontStyle.Bold);

            this.genericParametersPropertyGrid.SelectedObject = this.genericParameters;
            this.genericParametersPropertyGrid.Site = this;
            this.genericParametersPropertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(GenericParameterChanged);
            this.ResumeLayout(false);
        }

        private void TreeSelectionChanged(TreeNode treeNode)
        {
            try
            {
                if (this.artifactListView.Focused)
                    return;

                this.artifactListView.Items.Clear();
                this.artifactListView.ListViewItemSorter = null;

                string namespaceToFilter = null;
                ArrayList selectedAssemblies = new ArrayList();

                if (treeNode == this.artifactTreeView.Nodes[SR.CurrentProject])
                {
                    // current project node
                    // do nothing
                }
                else if (treeNode == this.artifactTreeView.Nodes[SR.ReferencedAssemblies])
                {
                    // assemblies node
                    foreach (TreeNode treeNode2 in treeNode.Nodes)
                        selectedAssemblies.Add(treeNode2.Tag);
                }
                else if (treeNode.Tag is Assembly)
                {
                    selectedAssemblies.Add(treeNode.Tag);
                }
                else
                {
                    if (treeNode.Parent.Tag != null)
                        selectedAssemblies.Add(treeNode.Parent.Tag);
                    namespaceToFilter = treeNode.Text;
                }

                ITypeProvider typeProvider = this.TypeProvider;
                IExtendedUIService2 extendedUIService = (IExtendedUIService2)this.serviceProvider.GetService(typeof(IExtendedUIService2));
                foreach (Type type in GetTargetFrameworkTypes(typeProvider))
                {
                    try
                    {
                        object[] attributes = type.GetCustomAttributes(typeof(ObsoleteAttribute), false);
                        if ((attributes != null) && attributes.Length > 0)
                            continue;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Count not retrieve attributes from type:" + e.Message);
                    }

                    if ((namespaceToFilter == null || type.Namespace == namespaceToFilter) && ((selectedAssemblies.Count == 0 && type.Assembly == null) || selectedAssemblies.Contains(type.Assembly)) && (this.typeFilterProvider == null || this.typeFilterProvider.CanFilterType(extendedUIService != null ? extendedUIService.GetRuntimeType(type) : type, false)))
                    {
                        ListViewItem listItem = new ListViewItem();

                        listItem.Text = type.Name;
                        listItem.SubItems.Add(type.FullName);
                        listItem.Tag = type;
                        listItem.ImageIndex = 0;
                        this.artifactListView.Items.Add(listItem);
                    }
                }
                SortListViewItems(0);
                this.artifactListView.SelectedIndices.Clear();

                //Select first type by default
                if (this.artifactListView.Items.Count > 0)
                    this.artifactListView.Items[0].Selected = true;
            }
            catch (Exception ex)
            {
                DesignerHelpers.ShowError(this.serviceProvider, ex);
            }

        }

        private void GenericParameterChanged(object sender, PropertyValueChangedEventArgs e)
        {
            bool fullyConfigured = true;
            foreach (ParameterData parameterData in this.genericParameters.Parameters)
            {
                if (parameterData.Type == null)
                {
                    fullyConfigured = false;
                    break;
                }
            }
            if (fullyConfigured)
                UpdateTypeTextBox();
        }

        private void ListSelectionChanged(Type selectedType)
        {
            if (this.artifactTreeView.Focused)
                return;

            string typeName = selectedType.FullName;
            string assemblyName = string.Empty;
            if (selectedType.Assembly != null)
                assemblyName = selectedType.Assembly.GetName().Name;

            TreeNode selectedAssemblyNode = null;
            if (assemblyName.Length == 0)
            {
                selectedAssemblyNode = this.artifactTreeView.Nodes[SR.CurrentProject];
            }
            else
            {
                TreeNode assembliesNode = this.artifactTreeView.Nodes[SR.ReferencedAssemblies];
                foreach (TreeNode assemblyNode in assembliesNode.Nodes)
                {
                    Assembly assembly = assemblyNode.Tag as Assembly;
                    if (assembly.FullName == assemblyName ||
                        assembly.GetName().Name == assemblyName)
                    {
                        selectedAssemblyNode = assemblyNode;
                        break;
                    }
                }
            }

            TreeNode selectedNsNode = null;
            if (selectedAssemblyNode != null)
            {
                string nsName = string.Empty;
                int lastIndexOfDot = typeName.LastIndexOf('.');
                if (lastIndexOfDot != -1)
                    nsName = typeName.Substring(0, lastIndexOfDot);

                if (nsName.Length > 0)
                {
                    foreach (TreeNode nsNode in selectedAssemblyNode.Nodes)
                    {
                        if (nsNode.Text == nsName)
                        {
                            selectedNsNode = nsNode;
                            break;
                        }
                    }
                }
            }

            TreeNode selectedNode = selectedNsNode;
            if (selectedNode == null)
                selectedNode = selectedAssemblyNode;

            if (selectedNode != null && this.artifactTreeView.CanFocus) 
            {
                this.artifactTreeView.SelectedNode = selectedNode;
                selectedNode.EnsureVisible();
            }
        }

        private void SortListViewItems(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= this.artifactListView.Columns.Count)
                return;

            ListItemComparer listItemComparer = new ListItemComparer((columnIndex == 0) ? true : false, this.sortListViewAscending);
            this.artifactListView.ListViewItemSorter = listItemComparer;

            if (this.artifactListView.SelectedItems.Count > 0)
                this.artifactListView.SelectedItems[0].EnsureVisible();

            //Update the bitmap for the sorted column
            IntPtr headerWindow = NativeMethods.ListView_GetHeader(this.artifactListView.Handle);
            NativeMethods.HDITEM headerItem = new NativeMethods.HDITEM();
            headerItem.mask = NativeMethods.HDI_FORMAT | NativeMethods.HDI_BITMAP;
            for (int i = 0; i < this.artifactListView.Columns.Count; i++)
            {
                if (NativeMethods.Header_GetItem(headerWindow, i, headerItem))
                {
                    headerItem.fmt &= ~(NativeMethods.HDF_BITMAP | NativeMethods.HDF_BITMAP_ON_RIGHT);
                    headerItem.hbm = IntPtr.Zero;
                    NativeMethods.Header_SetItem(headerWindow, i, headerItem);
                }
            }

            if (NativeMethods.Header_GetItem(headerWindow, columnIndex, headerItem))
            {
                headerItem.mask = NativeMethods.HDI_FORMAT | NativeMethods.HDI_BITMAP;
                headerItem.fmt |= NativeMethods.HDF_BITMAP | NativeMethods.HDF_BITMAP_ON_RIGHT;
                headerItem.hbm = (this.sortListViewAscending) ? this.bitmapSortUp : this.bitmapSortDown;
                NativeMethods.Header_SetItem(headerWindow, columnIndex, headerItem);
            }
        }

        private void UpdateControlState()
        {
            ITypeProvider typeProvider = this.TypeProvider;
            Type selectedType = null;

            selectedType = typeProvider.GetType(this.typeTextBox.Text);
            if ((null != selectedType) && (this.typeFilterProvider == null || (this.typeFilterProvider.CanFilterType(selectedType, false))) && !selectedType.IsGenericTypeDefinition)
            {
                this.buttonOK.Enabled = true;
                this.AcceptButton = this.buttonOK;
            }
            else
            {
                this.buttonOK.Enabled = false;
                this.AcceptButton = null;
            }

            this.helpTextWindow.UpdateHelpText(selectedType);
        }

        private void UpdateTypeTextBox()
        {
            string typeName = String.Empty;

            if (this.artifactListView.SelectedItems.Count > 0)
            {
                Type type = this.artifactListView.SelectedItems[0].Tag as Type;
                typeName = type.FullName;
                bool appendParameteres = true;
                string parameters = string.Empty;
                if ((type.IsGenericType) && (this.genericParameters.Parameters.Length > 0))
                {
                    parameters = "[";
                    int index = 0;
                    foreach (ParameterData parameterData in this.genericParameters.Parameters)
                    {
                        parameters += "[";
                        index++;
                        Type parameterType = parameterData.Type;
                        if (parameterType != null)
                        {
                            parameters += parameterType.FullName;
                        }
                        else
                        {
                            appendParameteres = false;
                            break;
                        }

                        parameters += "]";
                        if (index < this.genericParameters.Parameters.Length)
                            parameters += ",";
                    }
                    parameters += "]";
                }
                if (appendParameteres)
                    typeName += parameters;

            }
            this.typeTextBox.Text = typeName;
        }

        private void GetTypeParts(Type type, out Type baseType, out ParameterData[] parameterDataArray, out int[] arrayRanks)
        {
            baseType = null;
            parameterDataArray = null;
            arrayRanks = null;

            if (type.IsArray == true)
            {
                ArrayList arrayRankList = new ArrayList();
                GetTypeParts(type.GetElementType(), out baseType, out parameterDataArray, out arrayRanks);
                if (arrayRanks != null)
                    arrayRankList.AddRange(arrayRanks);
                arrayRankList.Add(type.GetArrayRank());
                arrayRanks = (int[])arrayRankList.ToArray(typeof(int));
            }
            else if (type.IsGenericType)
            {
                Type unboundedType = null;
                Type boundedType = null;

                if (type.ContainsGenericParameters)
                {
                    boundedType = null;
                    unboundedType = type.UnderlyingSystemType;
                }
                else
                {
                    boundedType = type;
                    unboundedType = type.GetGenericTypeDefinition().UnderlyingSystemType;
                }
                ArrayList parameterDataList = new ArrayList();
                for (int loop = 0; loop < unboundedType.GetGenericArguments().Length; loop++)
                {
                    ParameterData parameterData = new ParameterData();
                    parameterData.ParameterType = unboundedType.GetGenericArguments()[loop];
                    if (boundedType != null)
                        parameterData.Type = type.GetGenericArguments()[loop];
                    parameterDataList.Add(parameterData);
                }
                parameterDataArray = (ParameterData[])parameterDataList.ToArray(typeof(ParameterData));
                baseType = unboundedType;
            }
            else
            {
                baseType = type;
            }
        }

        private string GetSimpleTypeFullName(Type type)
        {
            StringBuilder typeName = new StringBuilder(type.FullName);
            Stack<Type> types = new Stack<Type>();
            types.Push(type);

            while (types.Count > 0)
            {
                type = types.Pop();

                while (type.IsArray)
                    type = type.GetElementType();

                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                {
                    foreach (Type parameterType in type.GetGenericArguments())
                    {
                        typeName.Replace("[" + parameterType.AssemblyQualifiedName + "]", parameterType.FullName);
                        types.Push(parameterType);
                    }
                }
            }

            return typeName.ToString();
        }

        private ITypeProvider TypeProvider
        {
            get
            {
                ITypeProvider typeProvider = this.localTypeProvider as ITypeProvider;
                if (typeProvider == null)
                    typeProvider = (ITypeProvider)this.serviceProvider.GetService(typeof(ITypeProvider));

                if (typeProvider == null)
                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                return typeProvider;
            }
        }
        #endregion

        #region Class ListItemComparer
        private sealed class ListItemComparer : IComparer
        {
            private bool compareTypeName;
            private bool ascending;
            internal ListItemComparer(bool compareTypeName, bool ascending)
            {
                this.compareTypeName = compareTypeName;
                this.ascending = ascending;
            }
            
            
            [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.Compare(System.String,System.String)", Justification = "This is not a security issue because this is a design time class")]
            public int Compare(object first, object second)
            {
                int result = 0;
                if (this.compareTypeName)
                    result = string.Compare(((ListViewItem)first).Text, ((ListViewItem)second).Text);
                else
                    result = string.Compare(((ListViewItem)first).SubItems[1].Text, ((ListViewItem)second).SubItems[1].Text);
                return ((this.ascending) ? 1 * result : -1 * result);
            }
        }
        #endregion

        #region Class HelpTextWindow
        private sealed class HelpTextWindow : RichTextBox
        {
            internal HelpTextWindow()
            {
                this.TabStop = false;
                this.BorderStyle = BorderStyle.None;
                this.ReadOnly = true;
                this.BackColor = SystemColors.Control;
                this.Multiline = true;
                this.ScrollBars = RichTextBoxScrollBars.Both;
                this.Cursor = Cursors.Default;
            }

            protected override void WndProc(ref Message msg)
            {
                const int WM_MOUSEFIRST = 0x0200;
                const int WM_MOUSELAST = 0x020D;
                if (msg.Msg >= WM_MOUSEFIRST && msg.Msg <= WM_MOUSELAST)
                    return;
                else
                    base.WndProc(ref msg);
            }

            internal void UpdateHelpText(Type selectedType)
            {
                using (Font helpFontBold = new Font(this.Font.FontFamily, this.Font.SizeInPoints, FontStyle.Bold))
                {
                    this.Clear();

                    if (null != selectedType)
                    {
                        string[] keywords = new string[3];
                        keywords[0] = selectedType.Name;
                        try
                        {
                            keywords[1] = (selectedType.Namespace != null && selectedType.Namespace.Length > 0) ? selectedType.Namespace : TypeBrowserDialog.ResMgr.GetString("IDS_GLOBALNS");
                        }
                        catch (NullReferenceException)
                        {
                            // Work around: for some reason RuntimeType.Namespace throws exception for array of generic
                            //       Remove the try-catch when the 
                        }
                        keywords[1] = "{" + keywords[1] + "}";
                        keywords[2] = (selectedType.Assembly != null) ? selectedType.Assembly.GetName().FullName : "<Current Project>";

                        Color[] keywordColors = new Color[3];
                        keywordColors[0] = Color.DarkRed;
                        keywordColors[1] = Color.Green;
                        keywordColors[2] = Color.Blue;

                        this.Text = TypeBrowserDialog.ResMgr.GetString("IDS_SELECTEDTYPE") + " " + keywords[0] + " " +
                        TypeBrowserDialog.ResMgr.GetString("IDS_MEMBEROF") + " " + keywords[1] + "\r\n" +
                        TypeBrowserDialog.ResMgr.GetString("IDS_CONTAINEDINASM") + " " + keywords[2];

                        int nStartSearch = 0;
                        for (int i = 0; i < keywords.GetLength(0); i++)
                        {
                            int nPrevStart = nStartSearch;
                            nStartSearch = this.Find(keywords[i], nStartSearch, RichTextBoxFinds.MatchCase | RichTextBoxFinds.WholeWord);
                            this.SelectionColor = keywordColors[i];
                            this.SelectionFont = helpFontBold;
                            nStartSearch += keywords[i].Length;
                        }
                    }
                    else
                    {
                        this.Text = TypeBrowserDialog.ResMgr.GetString("IDS_NOTYPESSELECTED");
                        this.SelectionStart = 0;
                        this.SelectionLength = this.Text.Length;
                        this.SelectionColor = Color.DarkRed;
                        this.SelectionFont = helpFontBold;
                    }
                }
            }
        }
        #endregion

        #region ISite Members

        IComponent System.ComponentModel.ISite.Component
        {
            get
            {
                return null;
            }
        }
        bool System.ComponentModel.ISite.DesignMode
        {
            get
            {
                return true;
            }
        }
        string System.ComponentModel.ISite.Name
        {
            get
            {
                return "";
            }
            set
            {
            }
        }
        IContainer System.ComponentModel.ISite.Container
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region IServiceProvider Members

        object System.IServiceProvider.GetService(Type serviceType)
        {
            object service = null;

            if (serviceType == typeof(ITypeProvider))
                service = this.TypeProvider;

            return service;
        }

        #endregion

        #region Class ParameterData
        [TypeConverter(typeof(ParamaeterDataConverter))]
        [Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor))]
        private sealed class ParameterData : ITypeFilterProvider
        {
            private Type parameterType;
            private Type type;

            public Type Type
            {
                get
                {
                    return type;
                }

                set
                {
                    type = value;
                }
            }

            public Type ParameterType
            {
                get
                {
                    return this.parameterType;
                }

                set
                {
                    this.parameterType = value;
                }
            }

            #region ITypeFilterProvider Members
            public bool CanFilterType(Type type, bool throwOnError)
            {
                bool validType = true;

                if ((type.IsByRef) || (!System.Workflow.ComponentModel.Compiler.TypeProvider.IsAssignable(this.parameterType.BaseType, type)))
                    validType = false;

                if (throwOnError && !validType)
                    throw new Exception(SR.GetString(SR.Error_ArgumentTypeNotMatchParameter));

                return validType;
            }

            public string FilterDescription
            {
                get
                {
                    return SR.GetString(SR.FilterDescription_GenericArgument, this.parameterType.Name);
                }
            }
            #endregion
        }
        #endregion

        #region Class GenericParameters

        [TypeConverter(typeof(GenericParametersConverter))]
        private class GenericParameters
        {
            private ParameterData[] parameters = new ParameterData[0];

            public ParameterData[] Parameters
            {
                get
                {
                    return parameters;
                }
                set
                {
                    this.parameters = value;
                }
            }
        }

        #endregion

        #region Class GenericParametersConverter

        private class GenericParametersConverter : TypeConverter
        {
            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                PropertyDescriptorCollection newProps = new PropertyDescriptorCollection(null);
                GenericParameters genericParameters = value as GenericParameters;

                foreach (ParameterData parameterData in genericParameters.Parameters)
                    newProps.Add(new ParameterDataPropertyDescriptor(context, TypeDescriptor.CreateProperty(typeof(GenericParameters), parameterData.ParameterType.Name, typeof(ParameterData))));

                return newProps;
            }

        }

        #endregion

        #region Class ParamaeterDataConverter

        private class ParamaeterDataConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                return base.CanConvertFrom(context, sourceType);
            }
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;
                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null) 
                    return new ParameterData();
                else if (value is string)
                {
                    ParameterData parameterData = new ParameterData();

                    if ((string)value != string.Empty)
                    {
                        ITypeProvider typeProvider = context.GetService(typeof(ITypeProvider)) as ITypeProvider;
                        parameterData.Type = typeProvider.GetType(value as string, true);
                    }
                    return parameterData;
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    ParameterData parameterData = value as ParameterData;
                    if (parameterData.Type != null)
                        return (parameterData.Type.AssemblyQualifiedName);
                    else
                        return string.Empty;
                }
                else
                    if (destinationType == null)
                        return string.Empty;

                return base.ConvertTo(context, culture, value, destinationType);
            }

        }

        #endregion

        #region Class ParameterDataPropertyDescriptor

        private class ParameterDataPropertyDescriptor : PropertyDescriptor
        {
            private PropertyDescriptor realPropertyDescriptor = null;
            private IServiceProvider serviceProvider = null;

            internal ParameterDataPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor desc)
                : base(desc, null)
            {
                this.realPropertyDescriptor = desc;
                this.serviceProvider = serviceProvider;
            }
            public override string Category
            {
                get
                {
                    return SR.GetString(SR.GenericParameters);
                }
            }
            public override AttributeCollection Attributes
            {
                get
                {
                    return this.realPropertyDescriptor.Attributes;
                }
            }
            public override TypeConverter Converter
            {
                get
                {
                    return this.realPropertyDescriptor.Converter;
                }
            }
            public override string Description
            {
                get
                {
                    return this.realPropertyDescriptor.Description;
                }
            }
            public override Type ComponentType
            {
                get
                {
                    return this.realPropertyDescriptor.ComponentType;
                }
            }
            public override Type PropertyType
            {
                get
                {
                    return this.realPropertyDescriptor.PropertyType;
                }
            }
            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }
            public override void ResetValue(object component)
            {
                this.realPropertyDescriptor.ResetValue(component);
            }
            public override bool CanResetValue(object component)
            {
                return this.realPropertyDescriptor.CanResetValue(component);
            }
            public override bool ShouldSerializeValue(object component)
            {
                return this.realPropertyDescriptor.ShouldSerializeValue(component);
            }
            public override object GetValue(object component)
            {
                GenericParameters genericParameters = component as GenericParameters;
                foreach (ParameterData parameterData in genericParameters.Parameters)
                {
                    if (parameterData.ParameterType.Name == this.Name)
                        return parameterData;
                }

                return null;
            }
            public override void SetValue(object component, object value)
            {
                GenericParameters genericParameters = component as GenericParameters;
                foreach (ParameterData parameterData in genericParameters.Parameters)
                {
                    if (parameterData.ParameterType.Name == this.Name)
                    {
                        parameterData.Type = ((ParameterData)value).Type;
                        break;
                    }
                }
            }
        }

        #endregion

        private class DummySite : ISite
        {
            private IServiceProvider serviceProvider;

            internal DummySite(IServiceProvider serviceProvider)
            {
                this.serviceProvider = serviceProvider;
            }

            public IComponent Component { get { return null; } }
            public IContainer Container { get { return null; } }
            public bool DesignMode { get { return true; } }
            public string Name { get { return string.Empty; } set { } }
            public object GetService(Type type) { return this.serviceProvider.GetService(type); }
        }
    }

}
