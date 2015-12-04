using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.CodeDom.Compiler;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using Microsoft.Win32;
using System.Workflow.ComponentModel.Compiler;

namespace System.Workflow.ComponentModel.Design
{
    /// <summary>
    /// Summary description for ThemeConfigurationDialog.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ThemeConfigurationDialog : System.Windows.Forms.Form
    {
        #region [....] Generated Members
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TreeView designerTreeView;
        private System.Windows.Forms.Label themeNameLabel;
        private System.Windows.Forms.Label themeLocationLabel;
        private System.Windows.Forms.TextBox themeNameTextBox;
        private System.Windows.Forms.Panel themePanel;
        private System.Windows.Forms.Panel themeConfigPanel;
        private System.Windows.Forms.Panel dummyPreviewPanel;
        private System.Windows.Forms.TextBox themeLocationTextBox;
        private System.Windows.Forms.Label previewLabel;
        private System.Windows.Forms.Label selectDesignerLabel;
        private System.Windows.Forms.PropertyGrid propertiesGrid;
        private System.Windows.Forms.Button themeLocationButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button previewButton;

        private System.ComponentModel.IContainer components = null;
        #endregion

        #region Member Variables
        private IServiceProvider serviceProvider;
        private bool previewShown = false;
        private WorkflowTheme bufferedTheme;
        private DesignerPreview designerPreview;
        private Splitter splitter;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private TableLayoutPanel nameLocationTableLayoutPanel;
        private bool themeDirty = false;
        #endregion

        #region Constructor/Destructor
        public ThemeConfigurationDialog(IServiceProvider serviceProvider)
            : this(serviceProvider, null)
        {
        }

        public ThemeConfigurationDialog(IServiceProvider serviceProvider, WorkflowTheme theme)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            this.serviceProvider = serviceProvider;

            if (theme == null)
            {
                this.bufferedTheme = new WorkflowTheme();
                this.themeDirty = true;
            }
            else
            {
                this.bufferedTheme = theme;
                this.themeDirty = false;
            }

            this.bufferedTheme.ReadOnly = false;

            InitializeComponent();
            this.themeLocationButton.AutoSize = true;

            //Set dialog fonts
            Font = StandardFont;
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(OnOperatingSystemSettingsChanged);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(OnOperatingSystemSettingsChanged);

                if (this.designerPreview != null)
                {
                    this.designerPreview.Dispose();
                    this.designerPreview = null;
                }

                if (this.bufferedTheme != null)
                {
                    ((IDisposable)this.bufferedTheme).Dispose();
                    this.bufferedTheme = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ThemeConfigurationDialog));
            this.designerTreeView = new System.Windows.Forms.TreeView();
            this.themeNameLabel = new System.Windows.Forms.Label();
            this.themeLocationLabel = new System.Windows.Forms.Label();
            this.themeNameTextBox = new System.Windows.Forms.TextBox();
            this.nameLocationTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.themeLocationButton = new System.Windows.Forms.Button();
            this.themeLocationTextBox = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.themePanel = new System.Windows.Forms.Panel();
            this.themeConfigPanel = new System.Windows.Forms.Panel();
            this.propertiesGrid = new System.Windows.Forms.PropertyGrid();
            this.previewLabel = new System.Windows.Forms.Label();
            this.selectDesignerLabel = new System.Windows.Forms.Label();
            this.dummyPreviewPanel = new System.Windows.Forms.Panel();
            this.previewButton = new System.Windows.Forms.Button();
            this.okCancelTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.nameLocationTableLayoutPanel.SuspendLayout();
            this.themePanel.SuspendLayout();
            this.themeConfigPanel.SuspendLayout();
            this.okCancelTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // designerTreeView
            // 
            resources.ApplyResources(this.designerTreeView, "designerTreeView");
            this.designerTreeView.Name = "designerTreeView";
            // 
            // themeNameLabel
            // 
            resources.ApplyResources(this.themeNameLabel, "themeNameLabel");
            this.themeNameLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.themeNameLabel.Name = "themeNameLabel";
            // 
            // themeLocationLabel
            // 
            resources.ApplyResources(this.themeLocationLabel, "themeLocationLabel");
            this.themeLocationLabel.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.themeLocationLabel.Name = "themeLocationLabel";
            // 
            // themeNameTextBox
            // 
            resources.ApplyResources(this.themeNameTextBox, "themeNameTextBox");
            this.nameLocationTableLayoutPanel.SetColumnSpan(this.themeNameTextBox, 2);
            this.themeNameTextBox.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.themeNameTextBox.Name = "themeNameTextBox";
            // 
            // nameLocationTableLayoutPanel
            // 
            resources.ApplyResources(this.nameLocationTableLayoutPanel, "nameLocationTableLayoutPanel");
            this.nameLocationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.nameLocationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.nameLocationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeNameLabel, 0, 0);
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeNameTextBox, 1, 0);
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeLocationButton, 2, 1);
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeLocationLabel, 0, 1);
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeLocationTextBox, 1, 1);
            this.nameLocationTableLayoutPanel.Name = "nameLocationTableLayoutPanel";
            this.nameLocationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.nameLocationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            // 
            // themeLocationButton
            // 
            resources.ApplyResources(this.themeLocationButton, "themeLocationButton");
            this.themeLocationButton.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.themeLocationButton.Name = "themeLocationButton";
            // 
            // themeLocationTextBox
            // 
            resources.ApplyResources(this.themeLocationTextBox, "themeLocationTextBox");
            this.themeLocationTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.themeLocationTextBox.Name = "themeLocationTextBox";
            // 
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.okButton.Name = "okButton";
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cancelButton.Name = "cancelButton";
            // 
            // themePanel
            // 
            this.themePanel.Controls.Add(this.themeConfigPanel);
            this.themePanel.Controls.Add(this.previewLabel);
            this.themePanel.Controls.Add(this.selectDesignerLabel);
            this.themePanel.Controls.Add(this.dummyPreviewPanel);
            resources.ApplyResources(this.themePanel, "themePanel");
            this.themePanel.Margin = new System.Windows.Forms.Padding(4);
            this.themePanel.Name = "themePanel";
            // 
            // themeConfigPanel
            // 
            this.themeConfigPanel.Controls.Add(this.designerTreeView);
            this.themeConfigPanel.Controls.Add(this.propertiesGrid);
            resources.ApplyResources(this.themeConfigPanel, "themeConfigPanel");
            this.themeConfigPanel.Name = "themeConfigPanel";
            // 
            // propertiesGrid
            // 
            this.propertiesGrid.CommandsVisibleIfAvailable = true;
            resources.ApplyResources(this.propertiesGrid, "propertiesGrid");
            this.propertiesGrid.Name = "propertiesGrid";
            this.propertiesGrid.ToolbarVisible = false;
            // 
            // previewLabel
            // 
            resources.ApplyResources(this.previewLabel, "previewLabel");
            this.previewLabel.Name = "previewLabel";
            // 
            // selectDesignerLabel
            // 
            resources.ApplyResources(this.selectDesignerLabel, "selectDesignerLabel");
            this.selectDesignerLabel.Name = "selectDesignerLabel";
            // 
            // dummyPreviewPanel
            // 
            resources.ApplyResources(this.dummyPreviewPanel, "dummyPreviewPanel");
            this.dummyPreviewPanel.Name = "dummyPreviewPanel";
            // 
            // previewButton
            // 
            resources.ApplyResources(this.previewButton, "previewButton");
            this.previewButton.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.previewButton.Name = "previewButton";
            // 
            // okCancelTableLayoutPanel
            // 
            resources.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.previewButton, 2, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            this.okCancelTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            // 
            // ThemeConfigurationDialog
            // 
            this.AcceptButton = this.okButton;
            this.CancelButton = this.cancelButton;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.nameLocationTableLayoutPanel);
            this.Controls.Add(this.okCancelTableLayoutPanel);
            this.Controls.Add(this.themePanel);
            this.Controls.Add(this.button3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ThemeConfigurationDialog";
            this.ShowInTaskbar = false;
            this.HelpButton = true;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.nameLocationTableLayoutPanel.ResumeLayout(false);
            this.nameLocationTableLayoutPanel.PerformLayout();
            this.themePanel.ResumeLayout(false);
            this.themeConfigPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region Properties and Methods
        public WorkflowTheme ComposedTheme
        {
            get
            {
                return this.bufferedTheme;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                InitializeControls();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            this.bufferedTheme.ReadOnly = true;
        }
        #endregion

        #region Helper Functions
        private Font StandardFont
        {
            get
            {
                Font font = SystemInformation.MenuFont;
                if (this.serviceProvider != null)
                {
                    IUIService uisvc = (IUIService)this.serviceProvider.GetService(typeof(IUIService));
                    if (uisvc != null)
                        font = (Font)uisvc.Styles["DialogFont"];
                }
                return font;
            }
        }

        private void InitializeControls()
        {
            HelpButtonClicked += new CancelEventHandler(OnHelpClicked);

            this.themeNameTextBox.Text = this.bufferedTheme.Name;
            this.themeLocationTextBox.Text = this.bufferedTheme.FilePath;

            this.propertiesGrid.PropertySort = PropertySort.Categorized;

            //Make sure that size and location are changed after adding the control to the parent
            //this will autoscale the control correctly
            this.designerPreview = new DesignerPreview(this);
            this.dummyPreviewPanel.Parent.Controls.Add(this.designerPreview);
            this.designerPreview.TabStop = false;
            this.designerPreview.Location = this.dummyPreviewPanel.Location;
            this.designerPreview.Size = this.dummyPreviewPanel.Size;
            this.dummyPreviewPanel.Visible = false;
            this.designerPreview.Parent.Controls.Remove(this.dummyPreviewPanel);

            this.designerTreeView.ShowLines = false;
            this.designerTreeView.ShowPlusMinus = false;
            this.designerTreeView.ShowRootLines = false;
            this.designerTreeView.ShowNodeToolTips = true;
            this.designerTreeView.HideSelection = false;
            this.designerTreeView.ItemHeight = Math.Max(this.designerTreeView.ItemHeight, 18);
            ThemeConfigHelpers.PopulateActivities(this.serviceProvider, this.designerTreeView);

            this.themeConfigPanel.Controls.Remove(this.designerTreeView);
            this.themeConfigPanel.Controls.Remove(this.propertiesGrid);
            this.designerTreeView.Dock = DockStyle.Left;
            this.splitter = new Splitter();
            this.splitter.Dock = DockStyle.Left;
            this.propertiesGrid.Dock = DockStyle.Fill;
            this.themeConfigPanel.Controls.AddRange(new Control[] { this.propertiesGrid, this.splitter, this.designerTreeView });

            this.themePanel.Paint += new PaintEventHandler(OnThemePanelPaint);
            this.previewButton.Click += new EventHandler(OnPreviewClicked);
            this.designerTreeView.AfterSelect += new TreeViewEventHandler(OnDesignerSelectionChanged);
            this.themeLocationButton.Click += new EventHandler(OnThemeLocationClicked);
            this.okButton.Click += new EventHandler(OnOk);
            this.propertiesGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(OnThemePropertyChanged);
            this.themeNameTextBox.TextChanged += new EventHandler(OnThemeChanged);
            this.themeLocationTextBox.TextChanged += new EventHandler(OnThemeChanged);

            this.designerTreeView.SelectedNode = (this.designerTreeView.Nodes.Count > 0) ? this.designerTreeView.Nodes[0] : null;
            this.designerTreeView.SelectedNode.EnsureVisible();

            ShowPreview = true;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            this.themeDirty = true;
        }

        private void OnThemePropertyChanged(object sender, PropertyValueChangedEventArgs e)
        {
            this.themeDirty = true;
        }

        private bool ValidateControls(out string error, out Control control)
        {
            error = String.Empty;
            control = null;

            if (this.themeNameTextBox.Text == null || this.themeNameTextBox.Text.Trim().Length == 0)
            {
                error = DR.GetString(DR.ThemeNameNotValid);
                control = this.themeNameTextBox;
                return false;
            }

            if (this.themeLocationTextBox.Text == null)
            {
                error = DR.GetString(DR.ThemePathNotValid);
                control = this.themeNameTextBox;
                return false;
            }

            string path = this.themeLocationTextBox.Text.Trim();
            if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ||
                !Path.IsPathRooted(path) ||
                !Path.HasExtension(path))
            {
                error = DR.GetString(DR.ThemePathNotValid);
                control = this.themeLocationTextBox;
                return false;
            }

            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            if (fileName == null || fileName.Trim().Length == 0 ||
                extension == null || extension.Trim().Length == 0)
            {
                error = DR.GetString(DR.ThemePathNotValid);
                control = this.themeLocationTextBox;
                return false;
            }

            if (!extension.Equals(WorkflowTheme.DefaultThemeFileExtension.Replace("*", ""), StringComparison.Ordinal))
            {
                error = DR.GetString(DR.ThemeFileNotXml);
                control = this.themeLocationTextBox;
                return false;
            }

            return true;
        }

        private void OnOk(object sender, EventArgs e)
        {
            string error = String.Empty;
            Control control = null;
            if (!ValidateControls(out error, out control))
            {
                DialogResult = DialogResult.None;
                DesignerHelpers.ShowError(this.serviceProvider, error);
                if (control != null)
                {
                    TextBox textBox = control as TextBox;
                    if (textBox != null)
                    {
                        textBox.SelectionStart = 0;
                        textBox.SelectionLength = (textBox.Text != null) ? textBox.Text.Length : 0;
                    }
                    control.Focus();
                }
                return;
            }

            //Before we try saving show the warning if the user has changed the theme path
            if (!this.bufferedTheme.FilePath.Equals(this.themeLocationTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                if (DialogResult.No == DesignerHelpers.ShowMessage(this.serviceProvider, DR.GetString(DR.UpdateRelativePaths), DR.GetString(DR.WorkflowDesignerTitle), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                {
                    DialogResult = DialogResult.None;
                    return;
                }
            }

            if (this.themeDirty)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    ThemeConfigHelpers.EnsureDesignerThemes(this.serviceProvider, this.bufferedTheme, ThemeConfigHelpers.GetAllTreeNodes(this.designerTreeView));
                    this.bufferedTheme.ReadOnly = false;
                    this.bufferedTheme.Name = this.themeNameTextBox.Text.Trim();
                    this.bufferedTheme.Description = DR.GetString(DR.ThemeDescription);
                    this.bufferedTheme.Save(this.themeLocationTextBox.Text.Trim());
                    this.themeDirty = false;
                    this.bufferedTheme.ReadOnly = true;
                }
                catch
                {
                    DesignerHelpers.ShowError(this.serviceProvider, DR.GetString(DR.ThemeFileCreationError));
                    this.themeLocationTextBox.SelectionStart = 0;
                    this.themeLocationTextBox.SelectionLength = (this.themeLocationTextBox.Text != null) ? this.themeLocationTextBox.Text.Length : 0;
                    this.themeLocationTextBox.Focus();
                    DialogResult = DialogResult.None;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void OnHelpClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            ShowHelp();
        }

        protected override void OnHelpRequested(HelpEventArgs e)
        {
            ShowHelp();
            e.Handled = true;
        }

        private void ShowHelp()
        {
            DesignerHelpers.ShowHelpFromKeyword(this.serviceProvider, typeof(ThemeConfigurationDialog).FullName + ".UI");
        }

        private void OnThemePanelPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(SystemPens.ControlDark, 0, 0, this.themePanel.ClientSize.Width - 1, this.themePanel.ClientSize.Height - 2);

            if (this.previewShown)
            {
                Point top = new Point(this.propertiesGrid.Right + (this.dummyPreviewPanel.Left - this.propertiesGrid.Right) / 2, this.themePanel.Margin.Top);
                Point bottom = new Point(top.X, this.themePanel.Height - this.themePanel.Margin.Bottom);
                e.Graphics.DrawLine(SystemPens.ControlDark, top, bottom);
            }

            Size margin = new Size(8, 8);
            using (Pen framePen = new Pen(Color.Black, 1))
            {
                framePen.DashStyle = DashStyle.Dot;
                e.Graphics.DrawLine(framePen, this.designerPreview.Left - margin.Width, this.designerPreview.Top - 1, this.designerPreview.Right + margin.Width, this.designerPreview.Top - 1);
                e.Graphics.DrawLine(framePen, this.designerPreview.Left - margin.Width, this.designerPreview.Bottom + 1, this.designerPreview.Right + margin.Width, this.designerPreview.Bottom + 1);
                e.Graphics.DrawLine(framePen, this.designerPreview.Left - 1, this.designerPreview.Top - margin.Height, this.designerPreview.Left - 1, this.designerPreview.Bottom + margin.Height);
                e.Graphics.DrawLine(framePen, this.designerPreview.Right + 1, this.designerPreview.Top - margin.Height, this.designerPreview.Right + 1, this.designerPreview.Bottom + margin.Height);
            }
        }

        private void OnDesignerSelectionChanged(object sender, TreeViewEventArgs eventArgs)
        {
            //We need to select the theme of the selected designer
            Type activityType = (eventArgs.Node != null && typeof(Activity).IsAssignableFrom(eventArgs.Node.Tag as System.Type)) ? eventArgs.Node.Tag as System.Type : null;
            IDesigner previewedDesigner = this.designerPreview.UpdatePreview(activityType);

            object[] selectedObjects = null;
            if (activityType == null)
            {
                if (eventArgs.Node != null)
                    selectedObjects = (eventArgs.Node.Parent == null) ? new object[] { this.bufferedTheme.AmbientTheme } : ThemeConfigHelpers.GetDesignerThemes(this.serviceProvider, this.bufferedTheme, eventArgs.Node);
            }
            else
            {
                selectedObjects = (previewedDesigner != null) ? new object[] { this.bufferedTheme.GetDesignerTheme(previewedDesigner as ActivityDesigner) } : null;
            }

            this.propertiesGrid.SelectedObjects = selectedObjects;
        }

        private void OnPreviewClicked(object sender, EventArgs e)
        {
            ShowPreview = !ShowPreview;
        }

        private void OnThemeLocationClicked(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.AddExtension = true;
            fileDialog.DefaultExt = WorkflowTheme.DefaultThemeFileExtension;
            fileDialog.Filter = DR.GetString(DR.ThemeFileFilter);
            fileDialog.RestoreDirectory = false;
            if (fileDialog.ShowDialog(this) == DialogResult.OK)
            {
                this.themeLocationTextBox.Text = fileDialog.FileName;
            }
        }

        private bool ShowPreview
        {
            get
            {
                return this.previewShown;
            }

            set
            {
                this.previewShown = value;
                this.previewLabel.Visible = this.previewShown;
                this.designerPreview.Visible = this.previewShown;

                if (this.previewShown)
                {
                    this.themePanel.Width = this.designerPreview.Right + ((this.designerPreview.Left - this.propertiesGrid.Right) / 2);
                    this.previewButton.Text = DR.GetString(DR.Preview) + " <<";
                }
                else
                {
                    this.themePanel.Width = this.themeConfigPanel.Right + this.themeConfigPanel.Left;
                    this.previewButton.Text = DR.GetString(DR.Preview) + " >>";
                }
                Width = this.themePanel.Right + this.themePanel.Left + Margin.Left + Margin.Right;
                this.themePanel.Invalidate();
            }
        }

        private void OnOperatingSystemSettingsChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            //

            if (e.Category == UserPreferenceCategory.Color || e.Category == UserPreferenceCategory.VisualStyle)
                Font = StandardFont;
        }
        #endregion

        #region Class ThemeHelpers
        private static class ThemeConfigHelpers
        {
            internal static void PopulateActivities(IServiceProvider serviceProvider, TreeView treeView)
            {
                List<Type> activityTypes = new List<Type>();

                //***************STOCK TYPES*************
                List<String> stockActivityTypeNames = new List<string>();
                stockActivityTypeNames.Add(DesignerHelpers.SequentialWorkflowTypeRef);
                stockActivityTypeNames.Add(DesignerHelpers.StateMachineWorkflowTypeRef);
                stockActivityTypeNames.Add(DesignerHelpers.IfElseBranchTypeRef);
                stockActivityTypeNames.Add(typeof(FaultHandlersActivity).AssemblyQualifiedName);
                stockActivityTypeNames.Add(DesignerHelpers.EventHandlersTypeRef);
                stockActivityTypeNames.Add(typeof(CompensationHandlerActivity).AssemblyQualifiedName);
                stockActivityTypeNames.Add(typeof(CancellationHandlerActivity).AssemblyQualifiedName);

                foreach (string stockTypeName in stockActivityTypeNames)
                {
                    Type stockType = Type.GetType(stockTypeName, false);
                    if (stockType == null)
                        Debug.Assert(false, string.Format(CultureInfo.CurrentCulture, "Could not load type '{0}'", stockTypeName));
                    else
                        activityTypes.Add(stockType);
                }

                //***************NON PREVIWABLE DESIGNER TYPES*************
                IList<Type> nonpreviewableDesignerTypes = new List<Type>();

                //These designer might be designers such as CADesigner which we eliminated
                //We have just kept the code so that in future if this functionality is needed
                //we can add it

                //Populate the designer combobox
                treeView.BeginUpdate();
                treeView.Nodes.Clear();

                //Work around: ***WE DISPLAY THE COMMON PROPERTIES FOR WORKFLOW AND APPLY THEM RECURSIVELY TO DESIGNERS
                TreeNode workflowNode = new TreeNode(DR.GetString(DR.WorkflowDesc));
                treeView.Nodes.Add(workflowNode);

                //Now we go thru the toolbox items and get all the items which are not in our assembly
                IToolboxService toolboxService = serviceProvider.GetService(typeof(IToolboxService)) as IToolboxService;
                ITypeProviderCreator typeProviderCreator = serviceProvider.GetService(typeof(ITypeProviderCreator)) as ITypeProviderCreator;
                if (toolboxService != null && typeProviderCreator != null)
                {
                    ToolboxItemCollection toolboxItems = toolboxService.GetToolboxItems();
                    foreach (ToolboxItem toolboxItem in toolboxItems)
                    {
                        bool customWinOEActivityType = (toolboxItem is ActivityToolboxItem);
                        if (!customWinOEActivityType)
                        {
                            foreach (ToolboxItemFilterAttribute filter in toolboxItem.Filter)
                            {
                                if (filter.FilterString.StartsWith("Microsoft.Workflow.VSDesigner", StringComparison.OrdinalIgnoreCase) ||
                                    filter.FilterString.StartsWith("System.Workflow.ComponentModel", StringComparison.OrdinalIgnoreCase))
                                {
                                    customWinOEActivityType = true;
                                    break;
                                }
                            }
                        }

                        if (customWinOEActivityType)
                        {
                            Type type = null;
                            Assembly assembly = typeProviderCreator.GetTransientAssembly(toolboxItem.AssemblyName);
                            if (assembly != null)
                                type = assembly.GetType(toolboxItem.TypeName);
                            if (type != null)
                            {
                                ConstructorInfo[] constructors = type.GetConstructors();
                                foreach (ConstructorInfo constructor in constructors)
                                {
                                    if (constructor.IsPublic && constructor.GetParameters().GetLength(0) == 0)
                                        activityTypes.Add(type);
                                }
                            }
                        }
                    }
                }

                foreach (Type type in activityTypes)
                {
                    Type designerBaseType = (type.FullName.Equals(DesignerHelpers.SequentialWorkflowTypeRef, StringComparison.OrdinalIgnoreCase)) ? typeof(IRootDesigner) : typeof(IDesigner);
                    Type designerType = ActivityDesigner.GetDesignerType(serviceProvider, type, designerBaseType);
                    if (designerType != null && !nonpreviewableDesignerTypes.Contains(designerType))
                    {
                        object[] attribs = designerType.GetCustomAttributes(typeof(ActivityDesignerThemeAttribute), true);
                        ActivityDesignerThemeAttribute themeAttrib = (attribs != null && attribs.GetLength(0) > 0) ? attribs[0] as ActivityDesignerThemeAttribute : null;
                        if (themeAttrib != null)
                        {
                            Image image = ActivityToolboxItem.GetToolboxImage(type);
                            if (treeView.ImageList == null)
                            {
                                treeView.ImageList = new ImageList();
                                treeView.ImageList.ColorDepth = ColorDepth.Depth32Bit;
                                Image standardImage = DR.GetImage(DR.Activity) as Image;
                                treeView.ImageList.Images.Add(standardImage, AmbientTheme.TransparentColor);
                            }

                            TreeNode parentNode = ThemeConfigHelpers.GetCatagoryNodeForDesigner(designerType, ThemeConfigHelpers.GetAllTreeNodes(treeView));
                            if (parentNode != null)
                            {
                                int imageIndex = (image != null) ? treeView.ImageList.Images.Add(image, AmbientTheme.TransparentColor) : 0;
                                TreeNode nodeToInsert = (imageIndex >= 0) ? new TreeNode(ActivityToolboxItem.GetToolboxDisplayName(type), imageIndex, imageIndex) : new TreeNode(ActivityToolboxItem.GetToolboxDisplayName(type));
                                nodeToInsert.Tag = type;

                                //We always make sure that cata----es are at the end
                                int index = parentNode.Nodes.Count - 1;
                                while (index >= 0 && parentNode.Nodes[index].Tag is System.Type)
                                    index = index - 1;
                                parentNode.Nodes.Insert(index, nodeToInsert);
                            }
                        }
                    }
                }

                treeView.TreeViewNodeSorter = new ThemeTreeNodeComparer();
                treeView.Sort();
                treeView.Nodes[0].ExpandAll();
                treeView.EndUpdate();
            }

            internal static TreeNode GetCatagoryNodeForDesigner(Type designerType, TreeNode[] treeNodes)
            {
                if (designerType == null)
                    throw new ArgumentNullException("designerType");
                if (treeNodes == null)
                    throw new ArgumentNullException("treeNodes");
                if (treeNodes.Length == 0)
                    throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "treeNodes");

                CategoryAttribute parentCatagoryAttribute = null;
                CategoryAttribute designerCatagoryAttribute = null;
                Type baseType = designerType;
                while (baseType != typeof(object) && parentCatagoryAttribute == null)
                {
                    object[] attribs = baseType.GetCustomAttributes(typeof(CategoryAttribute), false);
                    if (attribs != null && attribs.GetLength(0) > 0)
                    {
                        if (designerCatagoryAttribute == null)
                            designerCatagoryAttribute = attribs[0] as CategoryAttribute;
                        else
                            parentCatagoryAttribute = attribs[0] as CategoryAttribute;
                    }
                    baseType = baseType.BaseType;
                }

                if (designerCatagoryAttribute == null)
                    return null;

                //Search for the catagory
                TreeNode catagoryNode = null;
                TreeNode parentCatagoryTreeNode = treeNodes[0];

                foreach (TreeNode item in treeNodes)
                {
                    if (parentCatagoryAttribute != null && parentCatagoryAttribute.Category == item.Text && (item.Tag == null || !typeof(Activity).IsAssignableFrom(item.Tag.GetType())))
                        parentCatagoryTreeNode = item;

                    //We found the catagory
                    if (designerCatagoryAttribute.Category == item.Text && (item.Tag == null || !typeof(Activity).IsAssignableFrom(item.Tag.GetType())))
                    {
                        catagoryNode = item;
                        break;
                    }
                }

                if (catagoryNode == null)
                {
                    Debug.Assert(parentCatagoryTreeNode != null);
                    if (parentCatagoryTreeNode != null)
                    {
                        //Work around : ***WE DISPLAY THE COMMON PROPERTIES FROM KNOWN DESIGNERCATA----ES
                        //WE WILL EVENTUALLY REMOVE THIS WHEN WE CREATE AN MECHANISM TO SHARE COMMON
                        //PROPERTIES IN THEMES
                        catagoryNode = new TreeNode(designerCatagoryAttribute.Category);
                        parentCatagoryTreeNode.Nodes.Add(catagoryNode);
                    }
                }

                return catagoryNode;
            }

            internal static DesignerTheme[] GetDesignerThemes(IServiceProvider serviceProvider, WorkflowTheme workflowTheme, TreeNode selectedNode)
            {
                ArrayList designerThemes = new ArrayList();
                Queue<TreeNode> nodes = new Queue<TreeNode>();
                nodes.Enqueue(selectedNode);
                while (nodes.Count > 0)
                {
                    TreeNode treeNode = nodes.Dequeue();
                    Type activityType = treeNode.Tag as System.Type;
                    if (activityType != null)
                    {
                        Type designerBaseType = (activityType.FullName.Equals(DesignerHelpers.SequentialWorkflowTypeRef, StringComparison.OrdinalIgnoreCase)) ? typeof(IRootDesigner) : typeof(IDesigner);
                        Type designerType = ActivityDesigner.GetDesignerType(serviceProvider, activityType, designerBaseType);
                        if (designerType != null)
                        {
                            DesignerTheme designerTheme = workflowTheme.GetTheme(designerType);
                            if (designerTheme != null)
                                designerThemes.Add(designerTheme);
                        }
                    }
                    else
                    {
                        foreach (TreeNode childNode in treeNode.Nodes)
                            nodes.Enqueue(childNode);
                    }
                }

                return ((DesignerTheme[])designerThemes.ToArray(typeof(DesignerTheme)));
            }

            internal static TreeNode[] GetAllTreeNodes(TreeView treeView)
            {
                List<TreeNode> items = new List<TreeNode>();
                Queue<TreeNodeCollection> nodeCollections = new Queue<TreeNodeCollection>();
                nodeCollections.Enqueue(treeView.Nodes);
                while (nodeCollections.Count > 0)
                {
                    TreeNodeCollection nodeCollection = nodeCollections.Dequeue();
                    foreach (TreeNode treeNode in nodeCollection)
                    {
                        items.Add(treeNode);
                        if (treeNode.Nodes.Count > 0)
                            nodeCollections.Enqueue(treeNode.Nodes);
                    }
                }

                return items.ToArray();
            }

            internal static void EnsureDesignerThemes(IServiceProvider serviceProvider, WorkflowTheme workflowTheme, TreeNode[] items)
            {
                //We need to recurse thru the themes and make sure that we have all the designer themes created
                foreach (TreeNode item in items)
                {
                    DesignerTheme designerTheme = null;
                    Type activityType = item.Tag as Type;
                    if (activityType != null)
                    {
                        Type designerBaseType = (activityType.FullName.Equals(DesignerHelpers.SequentialWorkflowTypeRef, StringComparison.OrdinalIgnoreCase)) ? typeof(IRootDesigner) : typeof(IDesigner);
                        Type designerType = ActivityDesigner.GetDesignerType(serviceProvider, activityType, designerBaseType);
                        if (designerType != null)
                            designerTheme = workflowTheme.GetTheme(designerType);
                    }
                }
            }
        }
        #endregion

        #region Class ThemeTreeNodeComparer
        internal sealed class ThemeTreeNodeComparer : IComparer
        {
            #region IComparer Members
            int IComparer.Compare(object x, object y)
            {
                TreeNode treeNode1 = x as TreeNode;
                TreeNode treeNode2 = y as TreeNode;

                if (treeNode1.Nodes.Count > treeNode2.Nodes.Count)
                    return 1;
                else
                    return String.Compare(treeNode1.Text, treeNode2.Text, StringComparison.CurrentCulture);
            }
            #endregion
        }
        #endregion

        #region Class DesignerPreview
        internal sealed class DesignerPreview : UserControl
        {
            private ThemeConfigurationDialog parent = null;
            private PreviewDesignSurface surface = null;

            internal DesignerPreview(ThemeConfigurationDialog parent)
            {
                BackColor = Color.White;
                this.parent = parent;
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                SuspendLayout();

                this.surface = new PreviewDesignSurface(this.parent.serviceProvider);
                PreviewWorkflowDesignerLoader loader = new PreviewWorkflowDesignerLoader();
                this.surface.BeginLoad(loader);

                //Add the root activity
                IDesignerHost host = this.surface.GetService(typeof(IDesignerHost)) as IDesignerHost;
                Debug.Assert(host != null);

                //
                Activity rootDecl = host.CreateComponent(Type.GetType(DesignerHelpers.SequentialWorkflowTypeRef)) as Activity;
                rootDecl.Name = "ThemeSequentialWorkflow";
                WorkflowDesignerLoader.AddActivityToDesigner(this.surface, rootDecl as Activity);

                //Create the readonly workflow
                ReadonlyWorkflow workflowView = new ReadonlyWorkflow(this.parent, this.surface as IServiceProvider);
                workflowView.TabStop = false;
                workflowView.Dock = DockStyle.Fill;
                Controls.Add(workflowView);

                host.Activate();

                ResumeLayout(true);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && this.surface != null)
                {
                    IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (host != null && host.RootComponent != null)
                        WorkflowDesignerLoader.RemoveActivityFromDesigner(this.surface, host.RootComponent as Activity);

                    ReadonlyWorkflow workflowView = (Controls.Count > 0) ? Controls[0] as ReadonlyWorkflow : null;
                    Controls.Clear();
                    if (workflowView != null)
                    {
                        workflowView.Dispose();
                        workflowView = null;
                    }

                    this.surface.Dispose();
                    this.surface = null;
                }

                base.Dispose(disposing);
            }

            internal IDesigner UpdatePreview(Type activityType)
            {
                bool dummyPreview = false; //if we have a dummy preview activity
                IDesignerHost host = this.surface.GetService(typeof(IDesignerHost)) as IDesignerHost;
                Debug.Assert(host != null);

                CompositeActivity rootDecl = host.RootComponent as CompositeActivity;
                Debug.Assert(rootDecl != null);
                if (host == null || rootDecl == null)
                    return null;

                IComponent previewActivity = null;
                try
                {
                    //Remove earlier activities
                    while (rootDecl.Activities.Count > 0)
                    {
                        Activity declToDelete = rootDecl.Activities[0];
                        rootDecl.Activities.Remove(declToDelete);
                        WorkflowDesignerLoader.RemoveActivityFromDesigner(this.surface, declToDelete);
                    }

                    //Add new activities to preview
                    if (activityType == null || activityType.FullName.Equals(DesignerHelpers.SequentialWorkflowTypeRef, StringComparison.OrdinalIgnoreCase))
                    {
                        AddDummyActivity(rootDecl as CompositeActivity, Type.GetType(DesignerHelpers.CodeActivityTypeRef));
                        dummyPreview = true;
                    }
                    else
                    {
                        IComponent[] components = null;
                        object[] attribs = activityType.GetCustomAttributes(typeof(ToolboxItemAttribute), false);
                        ToolboxItemAttribute toolboxItemAttrib = (attribs != null && attribs.GetLength(0) > 0) ? attribs[0] as ToolboxItemAttribute : null;
                        if (toolboxItemAttrib != null && toolboxItemAttrib.ToolboxItemType != null && typeof(ActivityToolboxItem).IsAssignableFrom(toolboxItemAttrib.ToolboxItemType))
                        {
                            ActivityToolboxItem item = Activator.CreateInstance(toolboxItemAttrib.ToolboxItemType, new object[] { activityType }) as ActivityToolboxItem;
                            components = item.CreateComponents(host);
                        }

                        if (components == null)
                            components = new IComponent[] { Activator.CreateInstance(activityType) as IComponent };

                        Activity activity = (components != null && components.Length > 0) ? components[0] as Activity : null;
                        if (activity != null)
                        {
                            rootDecl.Activities.Add(activity);
                            EnsureUniqueId(activity);

                            WorkflowDesignerLoader.AddActivityToDesigner(this.surface, activity);
                            CompositeActivityDesigner compositeDesigner = host.GetDesigner(rootDecl) as CompositeActivityDesigner;
                            ActivityDesigner activityDesigner = host.GetDesigner(activity) as ActivityDesigner;
                            if (compositeDesigner != null && activityDesigner != null)
                                compositeDesigner.EnsureVisibleContainedDesigner(activityDesigner);
                            /*
                                                        //






*/
                        }
                    }


                    ISelectionService selectionService = host.GetService(typeof(ISelectionService)) as ISelectionService;
                    if (selectionService != null)
                        selectionService.SetSelectedComponents(new IComponent[] { rootDecl });

                    ReadonlyWorkflow workflowView = (Controls.Count > 0) ? Controls[0] as ReadonlyWorkflow : null;
                    if (workflowView != null)
                        workflowView.PerformLayout();

                    previewActivity = (rootDecl.Activities.Count > 0 && !dummyPreview) ? rootDecl.Activities[0] : rootDecl;
                }
                catch
                {
                }

                return (previewActivity != null) ? host.GetDesigner(previewActivity) : null;
            }

            private void AddDummyActivity(CompositeActivity parentActivity, Type activityType)
            {
                IDesignerHost host = this.surface.GetService(typeof(IDesignerHost)) as IDesignerHost;
                Debug.Assert(host != null);
                if (host == null)
                    return;

                Activity dummyActivity = Activator.CreateInstance(activityType) as Activity;
                Debug.Assert(dummyActivity != null);
                if (dummyActivity == null)
                    return;

                parentActivity.Activities.Add(dummyActivity);
                EnsureUniqueId(dummyActivity);
                WorkflowDesignerLoader.AddActivityToDesigner(this.surface, dummyActivity);
            }

            private void EnsureUniqueId(Activity addedActivity)
            {
                Dictionary<string, int> identifiers = new Dictionary<string, int>();
                Queue<Activity> Activities = new Queue<Activity>();
                Activities.Enqueue(addedActivity);
                while (Activities.Count > 0)
                {
                    Activity Activity = Activities.Dequeue();
                    string fullTypeName = Activity.GetType().FullName;

                    int id = (identifiers.ContainsKey(fullTypeName)) ? identifiers[fullTypeName] : 1;
                    Activity.Name = Activity.GetType().Name + id.ToString(CultureInfo.InvariantCulture);
                    id += 1;

                    if (identifiers.ContainsKey(fullTypeName))
                        identifiers[fullTypeName] = id;
                    else
                        identifiers.Add(fullTypeName, id);

                    CompositeActivity compositeActivity = Activity as CompositeActivity;
                    if (compositeActivity != null)
                    {
                        foreach (Activity activity in compositeActivity.Activities)
                            Activities.Enqueue(activity);
                    }
                }
            }

            #region Class PreviewDesignSurface
            private sealed class PreviewDesignSurface : DesignSurface
            {
                internal PreviewDesignSurface(IServiceProvider parentProvider)
                    : base(new PreviewDesignerServiceProvider(parentProvider))
                {
                    ITypeProvider typeProvider = GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (typeProvider == null)
                    {
                        TypeProvider provider = new TypeProvider(this);
                        provider.AddAssemblyReference(typeof(string).Assembly.Location);
                        ServiceContainer.AddService(typeof(ITypeProvider), provider, true);
                    }
                }

                protected override IDesigner CreateDesigner(IComponent component, bool rootDesigner)
                {
                    IDesigner designer = base.CreateDesigner(component, rootDesigner);
                    Activity activity = component as Activity;
                    if (designer == null && !rootDesigner && activity != null)
                        designer = ActivityDesigner.CreateDesigner(activity.Site, activity);
                    return designer;
                }

                #region Class PreviewDesignerServiceProvider
                private sealed class PreviewDesignerServiceProvider : IServiceProvider
                {
                    private IServiceProvider serviceProvider;

                    internal PreviewDesignerServiceProvider(IServiceProvider serviceProvider)
                    {
                        this.serviceProvider = serviceProvider;
                    }

                    #region IServiceProvider Members
                    object IServiceProvider.GetService(Type serviceType)
                    {
                        if (serviceType == typeof(IPropertyValueUIService))
                            return null;
                        return this.serviceProvider.GetService(serviceType);
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Class PreviewWorkflowDesignerLoader
            private class PreviewWorkflowDesignerLoader : WorkflowDesignerLoader
            {
                public override TextReader GetFileReader(string filePath)
                {
                    return null;
                }

                public override TextWriter GetFileWriter(string filePath)
                {
                    return null;
                }

                public override string FileName
                {
                    get
                    {
                        return String.Empty;
                    }
                }
            }
            #endregion

            #region Class ReadOnly Workflow
            private class ReadonlyWorkflow : WorkflowView
            {
                private ThemeConfigurationDialog themeConfigDialog = null;

                internal ReadonlyWorkflow(ThemeConfigurationDialog themeConfigDialog, IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                    this.themeConfigDialog = themeConfigDialog;
                    this.themeConfigDialog.propertiesGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(OnThemePropertyChanged);
                    this.EnableFitToScreen = false;

                    AddDesignerMessageFilter(new ReadonlyMessageFilter());
                }

                protected override void Dispose(bool disposing)
                {
                    base.Dispose(disposing);

                    if (this.themeConfigDialog != null && this.themeConfigDialog.propertiesGrid != null)
                        this.themeConfigDialog.propertiesGrid.PropertyValueChanged -= new PropertyValueChangedEventHandler(OnThemePropertyChanged);
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    if (this.themeConfigDialog == null)
                    {
                        base.OnPaint(e);
                        return;
                    }

                    using (BufferedTheme bufferedTheme = new BufferedTheme(this.themeConfigDialog.bufferedTheme))
                        base.OnPaint(e);
                }

                protected override void OnLayout(LayoutEventArgs levent)
                {
                    if (this.themeConfigDialog != null)
                    {
                        using (BufferedTheme bufferedTheme = new BufferedTheme(this.themeConfigDialog.bufferedTheme))
                            base.OnLayout(levent);

                        Size maxExtent = ActiveLayout.Extent;
                        Size size = Size;
                        PointF zoom = new PointF((float)size.Width / (float)maxExtent.Width, (float)size.Height / (float)maxExtent.Height);
                        Zoom = Convert.ToInt32((Math.Min(zoom.X, zoom.Y) * 100));
                    }
                }

                private void OnThemePropertyChanged(object sender, PropertyValueChangedEventArgs e)
                {
                    if (this.themeConfigDialog != null)
                    {
                        using (BufferedTheme bufferedTheme = new BufferedTheme(this.themeConfigDialog.bufferedTheme))
                            base.OnThemeChange(WorkflowTheme.CurrentTheme, EventArgs.Empty);
                    }
                }

                #region Class BufferedTheme
                private sealed class BufferedTheme : IDisposable
                {
                    private WorkflowTheme oldTheme = null;

                    internal BufferedTheme(WorkflowTheme themeToApply)
                    {
                        if (themeToApply != null && WorkflowTheme.CurrentTheme != themeToApply)
                        {
                            WorkflowTheme.EnableChangeNotification = false;
                            this.oldTheme = WorkflowTheme.CurrentTheme;
                            WorkflowTheme.CurrentTheme = themeToApply;
                        }
                    }

                    void IDisposable.Dispose()
                    {
                        if (this.oldTheme != null && WorkflowTheme.CurrentTheme != this.oldTheme)
                        {
                            WorkflowTheme.CurrentTheme.ReadOnly = false; //this was themeToApply passed into constructor, need to make it r/w again
                            WorkflowTheme.CurrentTheme = this.oldTheme;
                            WorkflowTheme.EnableChangeNotification = true;
                        }
                    }
                }
                #endregion
            }
            #endregion
        }
        #endregion
    }
}
