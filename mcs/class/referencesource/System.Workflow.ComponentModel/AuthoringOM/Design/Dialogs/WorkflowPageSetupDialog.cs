using System;
using System.IO;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.ComponentModel.Design;

namespace System.Workflow.ComponentModel.Design
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowPageSetupDialog : System.Windows.Forms.Form
    {
        #region [....] Desiger Generated Members
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.PictureBox landscapePicture;
        private System.Windows.Forms.PictureBox portraitPicture;
        private System.Windows.Forms.TabPage pageSettingsTab;
        private System.Windows.Forms.GroupBox marginsGroup;
        private NumericUpDown marginsBottomInput;
        private NumericUpDown marginsRightInput;
        private NumericUpDown marginsTopInput;
        private System.Windows.Forms.Label marginsTopLabel;
        private System.Windows.Forms.Label marginsLeftLabel;
        private System.Windows.Forms.Label marginsBottomLabel;
        private System.Windows.Forms.Label marginsRightLabel;
        private NumericUpDown marginsLeftInput;
        private System.Windows.Forms.GroupBox scalingGroup;
        private NumericUpDown adjustToScaleInput;
        private System.Windows.Forms.RadioButton adjustToRadioButton;
        private System.Windows.Forms.RadioButton fitToRadioButton;
        private NumericUpDown fitToPagesWideInput;
        private NumericUpDown fitToPagesTallInput;
        private System.Windows.Forms.Label fitToTallLabel;
        private System.Windows.Forms.Label fitToWideLabel;
        private System.Windows.Forms.GroupBox orientationGroup;
        private System.Windows.Forms.RadioButton portraitRadioButton;
        private System.Windows.Forms.RadioButton landscapeRadioButton;
        private System.Windows.Forms.GroupBox paperSettingsGroup;
        private System.Windows.Forms.ComboBox paperSizeComboBox;
        private System.Windows.Forms.Label paperSizeLabel;
        private System.Windows.Forms.Label paperSourceLabel;
        private System.Windows.Forms.ComboBox paperSourceComboBox;
        private System.Windows.Forms.TabPage headerFooterTab;
        private System.Windows.Forms.GroupBox footerGroup;
        private System.Windows.Forms.GroupBox headerGroup;
        private System.Windows.Forms.ComboBox headerAlignmentComboBox;
        private System.Windows.Forms.Label headerAlignmentLabel;
        private System.Windows.Forms.ComboBox headerTextComboBox;
        private System.Windows.Forms.Label headerTextLabel;
        private System.Windows.Forms.Label headerMarginLabel;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button printerButton;
        private NumericUpDown headerMarginInput;
        private NumericUpDown footerMarginInput;
        private System.Windows.Forms.ComboBox footerAlignmentComboBox;
        private System.Windows.Forms.Label footerAlignmentLabel;
        private System.Windows.Forms.ComboBox footerTextComboBox;
        private System.Windows.Forms.Label footerTextLabel;
        private System.Windows.Forms.Label footerMarginLabel;
        private System.Windows.Forms.Label scalingOfSizeLabel;
        private System.Windows.Forms.Label footerMarginUnitsLabel;
        private System.Windows.Forms.Label headerMarginUnitsLabel;
        private System.Windows.Forms.TextBox customHeaderText;
        private System.Windows.Forms.Label customHeaderLabel;
        private System.Windows.Forms.Label customFooterLabel;
        private System.Windows.Forms.TextBox customFooterText;
        private System.Windows.Forms.GroupBox centerGroup;
        private System.Windows.Forms.CheckBox CenterHorizontallyCheckBox;
        private System.Windows.Forms.CheckBox CenterVerticallyCheckBox;
        #endregion

        #region Members and Constructor/Destructor
        private IServiceProvider serviceProvider;
        private WorkflowPrintDocument printDocument = null;
        private string headerFooterNone = null;
        private string headerFooterCustom = null;
        private string[] headerFooterTemplates = null;
        private bool headerCustom = false;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private TableLayoutPanel paperTableLayoutPanel;
        private TableLayoutPanel centerTableLayoutPanel;
        private TableLayoutPanel marginsTableLayoutPanel;
        private TableLayoutPanel orientationTableLayoutPanel;
        private TableLayoutPanel scalingTableLayoutPanel;
        private TableLayoutPanel headerTableLayoutPanel;
        private TableLayoutPanel footerTableLayoutPanel;
        private bool footerCustom = false;

        public WorkflowPageSetupDialog(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            this.serviceProvider = serviceProvider;

            WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(WorkflowView).FullName));

            if (!(workflowView.PrintDocument is WorkflowPrintDocument))
                throw new InvalidOperationException(DR.GetString(DR.WorkflowPrintDocumentNotFound, typeof(WorkflowPrintDocument).Name));

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                InitializeComponent();

                this.printDocument = workflowView.PrintDocument as WorkflowPrintDocument;
                //deserialize state of the dialog from the page setup data scaling:

                //set the values scaling controls
                this.adjustToScaleInput.Value = this.printDocument.PageSetupData.ScaleFactor;
                this.fitToPagesWideInput.Value = this.printDocument.PageSetupData.PagesWide;
                this.fitToPagesTallInput.Value = this.printDocument.PageSetupData.PagesTall;

                //select the right mode
                if (this.printDocument.PageSetupData.AdjustToScaleFactor)
                    this.adjustToRadioButton.Checked = true;
                else
                    this.fitToRadioButton.Checked = true;

                //set the orientation
                if (this.printDocument.PageSetupData.Landscape)
                    this.landscapeRadioButton.Checked = true;
                else
                    this.portraitRadioButton.Checked = true;

                //margins
                SetMarginsToUI(this.printDocument.PageSetupData.Margins);

                //centering
                this.CenterHorizontallyCheckBox.Checked = this.printDocument.PageSetupData.CenterHorizontally;
                this.CenterVerticallyCheckBox.Checked = this.printDocument.PageSetupData.CenterVertically;

                //Initialize the paper
                InitializePaperInformation();

                //read standard header/footer formats
                this.headerFooterNone = DR.GetString(DR.HeaderFooterStringNone); //"(none)"
                this.headerFooterCustom = DR.GetString(DR.HeaderFooterStringCustom); //"(none)"
                this.headerFooterTemplates = new string[] {
                    DR.GetString(DR.HeaderFooterFormat1), //"Page %Page%",//
                    DR.GetString(DR.HeaderFooterFormat2), //"Page %Page% of %Pages%",//
                    DR.GetString(DR.HeaderFooterFormat3), //"%Path%%File, Page %Page% of %Pages%", //
                    DR.GetString(DR.HeaderFooterFormat4), //"%Path%%File, Page %Page%",//
                    DR.GetString(DR.HeaderFooterFormat5), //"%File, %Date% %Time%, Page %Page%",//
                    DR.GetString(DR.HeaderFooterFormat6), //"%File, Page %Page% of %Pages%",//
                    DR.GetString(DR.HeaderFooterFormat7), //"%File, Page %Page%",//
                    DR.GetString(DR.HeaderFooterFormat8), //"Prepated by %User% %Date%",//
                    DR.GetString(DR.HeaderFooterFormat9), //"%User%, Page %Page%, %Date%"//
                };

                //header inputs
                this.headerTextComboBox.Items.Add(this.headerFooterNone);
                this.headerTextComboBox.Items.AddRange(this.headerFooterTemplates);
                this.headerTextComboBox.Items.Add(this.headerFooterCustom);
                this.headerTextComboBox.SelectedIndex = 0;

                string userHeader = this.printDocument.PageSetupData.HeaderTemplate;
                this.headerCustom = this.printDocument.PageSetupData.HeaderCustom;
                if (userHeader.Length == 0)
                {
                    this.headerTextComboBox.SelectedIndex = 0; //none
                }
                else
                {
                    int userHeaderIndex = this.headerTextComboBox.Items.IndexOf(userHeader);

                    if (-1 == userHeaderIndex || this.headerCustom)
                    {
                        //this is an unknown template, put it into custom field
                        this.headerTextComboBox.SelectedIndex = this.headerTextComboBox.Items.IndexOf(this.headerFooterCustom);
                        this.customHeaderText.Text = userHeader;
                    }
                    else
                    {
                        this.headerTextComboBox.SelectedIndex = userHeaderIndex;
                    }
                }

                this.headerAlignmentComboBox.Items.AddRange(new object[] { HorizontalAlignment.Left, HorizontalAlignment.Center, HorizontalAlignment.Right });
                if (this.headerAlignmentComboBox.Items.IndexOf(this.printDocument.PageSetupData.HeaderAlignment) != -1)
                    this.headerAlignmentComboBox.SelectedItem = this.printDocument.PageSetupData.HeaderAlignment;
                else
                    this.headerAlignmentComboBox.SelectedItem = HorizontalAlignment.Center;

                this.headerMarginInput.Value = PrinterUnitToUIUnit(this.printDocument.PageSetupData.HeaderMargin);

                //footer inputs
                this.footerTextComboBox.Items.Add(this.headerFooterNone);
                this.footerTextComboBox.SelectedIndex = 0;
                this.footerTextComboBox.Items.AddRange(this.headerFooterTemplates);
                this.footerTextComboBox.Items.Add(this.headerFooterCustom);

                string userFooter = this.printDocument.PageSetupData.FooterTemplate;
                this.footerCustom = this.printDocument.PageSetupData.FooterCustom;
                if (userFooter.Length == 0)
                {
                    this.footerTextComboBox.SelectedIndex = 0; //none
                }
                else
                {
                    int userFooterIndex = this.footerTextComboBox.Items.IndexOf(userFooter);

                    if (-1 == userFooterIndex || this.footerCustom)
                    {
                        //this is an unknown template, put it into custom field
                        this.footerTextComboBox.SelectedIndex = this.footerTextComboBox.Items.IndexOf(this.headerFooterCustom);
                        this.customFooterText.Text = userFooter;
                    }
                    else
                    {
                        this.footerTextComboBox.SelectedIndex = userFooterIndex;
                    }
                }

                this.footerAlignmentComboBox.Items.AddRange(new object[] { HorizontalAlignment.Left, HorizontalAlignment.Center, HorizontalAlignment.Right });
                if (this.footerAlignmentComboBox.Items.IndexOf(this.printDocument.PageSetupData.FooterAlignment) != -1)
                    this.footerAlignmentComboBox.SelectedItem = this.printDocument.PageSetupData.FooterAlignment;
                else
                    this.footerAlignmentComboBox.SelectedItem = HorizontalAlignment.Center;

                this.footerMarginInput.Value = PrinterUnitToUIUnit(this.printDocument.PageSetupData.FooterMargin);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowPageSetupDialog));
            this.tabs = new System.Windows.Forms.TabControl();
            this.pageSettingsTab = new System.Windows.Forms.TabPage();
            this.centerGroup = new System.Windows.Forms.GroupBox();
            this.centerTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.CenterVerticallyCheckBox = new System.Windows.Forms.CheckBox();
            this.CenterHorizontallyCheckBox = new System.Windows.Forms.CheckBox();
            this.marginsGroup = new System.Windows.Forms.GroupBox();
            this.marginsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.marginsRightInput = new System.Windows.Forms.NumericUpDown();
            this.marginsBottomInput = new System.Windows.Forms.NumericUpDown();
            this.marginsTopLabel = new System.Windows.Forms.Label();
            this.marginsLeftLabel = new System.Windows.Forms.Label();
            this.marginsRightLabel = new System.Windows.Forms.Label();
            this.marginsBottomLabel = new System.Windows.Forms.Label();
            this.marginsTopInput = new System.Windows.Forms.NumericUpDown();
            this.marginsLeftInput = new System.Windows.Forms.NumericUpDown();
            this.scalingGroup = new System.Windows.Forms.GroupBox();
            this.scalingTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.fitToTallLabel = new System.Windows.Forms.Label();
            this.scalingOfSizeLabel = new System.Windows.Forms.Label();
            this.fitToWideLabel = new System.Windows.Forms.Label();
            this.adjustToRadioButton = new System.Windows.Forms.RadioButton();
            this.fitToPagesTallInput = new System.Windows.Forms.NumericUpDown();
            this.fitToPagesWideInput = new System.Windows.Forms.NumericUpDown();
            this.adjustToScaleInput = new System.Windows.Forms.NumericUpDown();
            this.fitToRadioButton = new System.Windows.Forms.RadioButton();
            this.orientationGroup = new System.Windows.Forms.GroupBox();
            this.orientationTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.landscapeRadioButton = new System.Windows.Forms.RadioButton();
            this.landscapePicture = new System.Windows.Forms.PictureBox();
            this.portraitRadioButton = new System.Windows.Forms.RadioButton();
            this.portraitPicture = new System.Windows.Forms.PictureBox();
            this.paperSettingsGroup = new System.Windows.Forms.GroupBox();
            this.paperTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.paperSourceComboBox = new System.Windows.Forms.ComboBox();
            this.paperSizeComboBox = new System.Windows.Forms.ComboBox();
            this.paperSizeLabel = new System.Windows.Forms.Label();
            this.paperSourceLabel = new System.Windows.Forms.Label();
            this.headerFooterTab = new System.Windows.Forms.TabPage();
            this.footerGroup = new System.Windows.Forms.GroupBox();
            this.footerTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.footerTextLabel = new System.Windows.Forms.Label();
            this.footerAlignmentLabel = new System.Windows.Forms.Label();
            this.footerMarginUnitsLabel = new System.Windows.Forms.Label();
            this.footerMarginLabel = new System.Windows.Forms.Label();
            this.footerMarginInput = new System.Windows.Forms.NumericUpDown();
            this.footerTextComboBox = new System.Windows.Forms.ComboBox();
            this.footerAlignmentComboBox = new System.Windows.Forms.ComboBox();
            this.customFooterText = new System.Windows.Forms.TextBox();
            this.customFooterLabel = new System.Windows.Forms.Label();
            this.headerGroup = new System.Windows.Forms.GroupBox();
            this.headerTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.headerTextLabel = new System.Windows.Forms.Label();
            this.headerAlignmentLabel = new System.Windows.Forms.Label();
            this.headerMarginUnitsLabel = new System.Windows.Forms.Label();
            this.headerMarginLabel = new System.Windows.Forms.Label();
            this.headerMarginInput = new System.Windows.Forms.NumericUpDown();
            this.headerTextComboBox = new System.Windows.Forms.ComboBox();
            this.headerAlignmentComboBox = new System.Windows.Forms.ComboBox();
            this.customHeaderText = new System.Windows.Forms.TextBox();
            this.customHeaderLabel = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.printerButton = new System.Windows.Forms.Button();
            this.okCancelTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tabs.SuspendLayout();
            this.pageSettingsTab.SuspendLayout();
            this.centerGroup.SuspendLayout();
            this.centerTableLayoutPanel.SuspendLayout();
            this.marginsGroup.SuspendLayout();
            this.marginsTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.marginsRightInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginsBottomInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginsTopInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginsLeftInput)).BeginInit();
            this.scalingGroup.SuspendLayout();
            this.scalingTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fitToPagesTallInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fitToPagesWideInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.adjustToScaleInput)).BeginInit();
            this.orientationGroup.SuspendLayout();
            this.orientationTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.landscapePicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.portraitPicture)).BeginInit();
            this.paperSettingsGroup.SuspendLayout();
            this.paperTableLayoutPanel.SuspendLayout();
            this.headerFooterTab.SuspendLayout();
            this.footerGroup.SuspendLayout();
            this.footerTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.footerMarginInput)).BeginInit();
            this.headerGroup.SuspendLayout();
            this.headerTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerMarginInput)).BeginInit();
            this.okCancelTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            resources.ApplyResources(this.tabs, "tabs");
            this.tabs.Controls.Add(this.pageSettingsTab);
            this.tabs.Controls.Add(this.headerFooterTab);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            // 
            // pageSettingsTab
            // 
            this.pageSettingsTab.Controls.Add(this.centerGroup);
            this.pageSettingsTab.Controls.Add(this.marginsGroup);
            this.pageSettingsTab.Controls.Add(this.scalingGroup);
            this.pageSettingsTab.Controls.Add(this.orientationGroup);
            this.pageSettingsTab.Controls.Add(this.paperSettingsGroup);
            resources.ApplyResources(this.pageSettingsTab, "pageSettingsTab");
            this.pageSettingsTab.Name = "pageSettingsTab";
            // 
            // centerGroup
            // 
            resources.ApplyResources(this.centerGroup, "centerGroup");
            this.centerGroup.Controls.Add(this.centerTableLayoutPanel);
            this.centerGroup.Name = "centerGroup";
            this.centerGroup.TabStop = false;
            // 
            // centerTableLayoutPanel
            // 
            resources.ApplyResources(this.centerTableLayoutPanel, "centerTableLayoutPanel");
            this.centerTableLayoutPanel.Controls.Add(this.CenterVerticallyCheckBox, 1, 0);
            this.centerTableLayoutPanel.Controls.Add(this.CenterHorizontallyCheckBox, 0, 0);
            this.centerTableLayoutPanel.Name = "centerTableLayoutPanel";
            // 
            // CenterVerticallyCheckBox
            // 
            resources.ApplyResources(this.CenterVerticallyCheckBox, "CenterVerticallyCheckBox");
            this.CenterVerticallyCheckBox.Name = "CenterVerticallyCheckBox";
            // 
            // CenterHorizontallyCheckBox
            // 
            resources.ApplyResources(this.CenterHorizontallyCheckBox, "CenterHorizontallyCheckBox");
            this.CenterHorizontallyCheckBox.Name = "CenterHorizontallyCheckBox";
            // 
            // marginsGroup
            // 
            resources.ApplyResources(this.marginsGroup, "marginsGroup");
            this.marginsGroup.Controls.Add(this.marginsTableLayoutPanel);
            this.marginsGroup.Name = "marginsGroup";
            this.marginsGroup.TabStop = false;
            // 
            // marginsTableLayoutPanel
            // 
            resources.ApplyResources(this.marginsTableLayoutPanel, "marginsTableLayoutPanel");
            this.marginsTableLayoutPanel.Controls.Add(this.marginsRightInput, 3, 1);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsBottomInput, 3, 0);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsTopLabel, 0, 0);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsLeftLabel, 0, 1);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsRightLabel, 2, 1);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsBottomLabel, 2, 0);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsTopInput, 1, 0);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsLeftInput, 1, 1);
            this.marginsTableLayoutPanel.Name = "marginsTableLayoutPanel";
            // 
            // marginsRightInput
            // 
            resources.ApplyResources(this.marginsRightInput, "marginsRightInput");
            this.marginsRightInput.DecimalPlaces = 2;
            this.marginsRightInput.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.marginsRightInput.Name = "marginsRightInput";
            this.marginsRightInput.Value = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.marginsRightInput.Validating += new System.ComponentModel.CancelEventHandler(this.Margins_Validating);
            // 
            // marginsBottomInput
            // 
            resources.ApplyResources(this.marginsBottomInput, "marginsBottomInput");
            this.marginsBottomInput.DecimalPlaces = 2;
            this.marginsBottomInput.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.marginsBottomInput.Name = "marginsBottomInput";
            this.marginsBottomInput.Value = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.marginsBottomInput.Validating += new System.ComponentModel.CancelEventHandler(this.Margins_Validating);
            // 
            // marginsTopLabel
            // 
            resources.ApplyResources(this.marginsTopLabel, "marginsTopLabel");
            this.marginsTopLabel.Name = "marginsTopLabel";
            // 
            // marginsLeftLabel
            // 
            resources.ApplyResources(this.marginsLeftLabel, "marginsLeftLabel");
            this.marginsLeftLabel.Name = "marginsLeftLabel";
            // 
            // marginsRightLabel
            // 
            resources.ApplyResources(this.marginsRightLabel, "marginsRightLabel");
            this.marginsRightLabel.Name = "marginsRightLabel";
            // 
            // marginsBottomLabel
            // 
            resources.ApplyResources(this.marginsBottomLabel, "marginsBottomLabel");
            this.marginsBottomLabel.Name = "marginsBottomLabel";
            // 
            // marginsTopInput
            // 
            resources.ApplyResources(this.marginsTopInput, "marginsTopInput");
            this.marginsTopInput.DecimalPlaces = 2;
            this.marginsTopInput.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.marginsTopInput.Name = "marginsTopInput";
            this.marginsTopInput.Value = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.marginsTopInput.Validating += new System.ComponentModel.CancelEventHandler(this.Margins_Validating);
            // 
            // marginsLeftInput
            // 
            resources.ApplyResources(this.marginsLeftInput, "marginsLeftInput");
            this.marginsLeftInput.DecimalPlaces = 2;
            this.marginsLeftInput.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.marginsLeftInput.Name = "marginsLeftInput";
            this.marginsLeftInput.Value = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.marginsLeftInput.Validating += new System.ComponentModel.CancelEventHandler(this.Margins_Validating);
            // 
            // scalingGroup
            // 
            resources.ApplyResources(this.scalingGroup, "scalingGroup");
            this.scalingGroup.Controls.Add(this.scalingTableLayoutPanel);
            this.scalingGroup.Name = "scalingGroup";
            this.scalingGroup.TabStop = false;
            // 
            // scalingTableLayoutPanel
            // 
            resources.ApplyResources(this.scalingTableLayoutPanel, "scalingTableLayoutPanel");
            this.scalingTableLayoutPanel.Controls.Add(this.fitToTallLabel, 2, 2);
            this.scalingTableLayoutPanel.Controls.Add(this.scalingOfSizeLabel, 2, 0);
            this.scalingTableLayoutPanel.Controls.Add(this.fitToWideLabel, 2, 1);
            this.scalingTableLayoutPanel.Controls.Add(this.adjustToRadioButton, 0, 0);
            this.scalingTableLayoutPanel.Controls.Add(this.fitToPagesTallInput, 1, 2);
            this.scalingTableLayoutPanel.Controls.Add(this.fitToPagesWideInput, 1, 1);
            this.scalingTableLayoutPanel.Controls.Add(this.adjustToScaleInput, 1, 0);
            this.scalingTableLayoutPanel.Controls.Add(this.fitToRadioButton, 0, 1);
            this.scalingTableLayoutPanel.Name = "scalingTableLayoutPanel";
            // 
            // fitToTallLabel
            // 
            resources.ApplyResources(this.fitToTallLabel, "fitToTallLabel");
            this.fitToTallLabel.Name = "fitToTallLabel";
            // 
            // scalingOfSizeLabel
            // 
            resources.ApplyResources(this.scalingOfSizeLabel, "scalingOfSizeLabel");
            this.scalingOfSizeLabel.Name = "scalingOfSizeLabel";
            // 
            // fitToWideLabel
            // 
            resources.ApplyResources(this.fitToWideLabel, "fitToWideLabel");
            this.fitToWideLabel.Name = "fitToWideLabel";
            // 
            // adjustToRadioButton
            // 
            resources.ApplyResources(this.adjustToRadioButton, "adjustToRadioButton");
            this.adjustToRadioButton.Name = "adjustToRadioButton";
            // 
            // fitToPagesTallInput
            // 
            resources.ApplyResources(this.fitToPagesTallInput, "fitToPagesTallInput");
            this.fitToPagesTallInput.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.fitToPagesTallInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fitToPagesTallInput.Name = "fitToPagesTallInput";
            this.fitToPagesTallInput.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fitToPagesTallInput.ValueChanged += new System.EventHandler(this.fitToInputs_ValueChanged);
            // 
            // fitToPagesWideInput
            // 
            resources.ApplyResources(this.fitToPagesWideInput, "fitToPagesWideInput");
            this.fitToPagesWideInput.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.fitToPagesWideInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fitToPagesWideInput.Name = "fitToPagesWideInput";
            this.fitToPagesWideInput.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fitToPagesWideInput.ValueChanged += new System.EventHandler(this.fitToInputs_ValueChanged);
            // 
            // adjustToScaleInput
            // 
            resources.ApplyResources(this.adjustToScaleInput, "adjustToScaleInput");
            this.adjustToScaleInput.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.adjustToScaleInput.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.adjustToScaleInput.Name = "adjustToScaleInput";
            this.adjustToScaleInput.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.adjustToScaleInput.ValueChanged += new System.EventHandler(this.adjustToInput_ValueChanged);
            // 
            // fitToRadioButton
            // 
            resources.ApplyResources(this.fitToRadioButton, "fitToRadioButton");
            this.fitToRadioButton.Name = "fitToRadioButton";
            // 
            // orientationGroup
            // 
            resources.ApplyResources(this.orientationGroup, "orientationGroup");
            this.orientationGroup.Controls.Add(this.orientationTableLayoutPanel);
            this.orientationGroup.Name = "orientationGroup";
            this.orientationGroup.TabStop = false;
            // 
            // orientationTableLayoutPanel
            // 
            resources.ApplyResources(this.orientationTableLayoutPanel, "orientationTableLayoutPanel");
            this.orientationTableLayoutPanel.Controls.Add(this.landscapeRadioButton, 3, 0);
            this.orientationTableLayoutPanel.Controls.Add(this.landscapePicture, 2, 0);
            this.orientationTableLayoutPanel.Controls.Add(this.portraitRadioButton, 1, 0);
            this.orientationTableLayoutPanel.Controls.Add(this.portraitPicture, 0, 0);
            this.orientationTableLayoutPanel.Name = "orientationTableLayoutPanel";
            // 
            // landscapeRadioButton
            // 
            resources.ApplyResources(this.landscapeRadioButton, "landscapeRadioButton");
            this.landscapeRadioButton.Name = "landscapeRadioButton";
            this.landscapeRadioButton.CheckedChanged += new System.EventHandler(this.landscapeRadioButton_CheckedChanged);
            // 
            // landscapePicture
            // 
            resources.ApplyResources(this.landscapePicture, "landscapePicture");
            this.landscapePicture.Name = "landscapePicture";
            this.landscapePicture.TabStop = false;
            // 
            // portraitRadioButton
            // 
            resources.ApplyResources(this.portraitRadioButton, "portraitRadioButton");
            this.portraitRadioButton.Name = "portraitRadioButton";
            this.portraitRadioButton.CheckedChanged += new System.EventHandler(this.portraitRadioButton_CheckedChanged);
            // 
            // portraitPicture
            // 
            resources.ApplyResources(this.portraitPicture, "portraitPicture");
            this.portraitPicture.Name = "portraitPicture";
            this.portraitPicture.TabStop = false;
            // 
            // paperSettingsGroup
            // 
            resources.ApplyResources(this.paperSettingsGroup, "paperSettingsGroup");
            this.paperSettingsGroup.Controls.Add(this.paperTableLayoutPanel);
            this.paperSettingsGroup.Name = "paperSettingsGroup";
            this.paperSettingsGroup.TabStop = false;
            // 
            // paperTableLayoutPanel
            // 
            resources.ApplyResources(this.paperTableLayoutPanel, "paperTableLayoutPanel");
            this.paperTableLayoutPanel.Controls.Add(this.paperSourceComboBox, 1, 1);
            this.paperTableLayoutPanel.Controls.Add(this.paperSizeComboBox, 1, 0);
            this.paperTableLayoutPanel.Controls.Add(this.paperSizeLabel, 0, 0);
            this.paperTableLayoutPanel.Controls.Add(this.paperSourceLabel, 0, 1);
            this.paperTableLayoutPanel.Name = "paperTableLayoutPanel";
            // 
            // paperSourceComboBox
            // 
            resources.ApplyResources(this.paperSourceComboBox, "paperSourceComboBox");
            this.paperSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.paperSourceComboBox.FormattingEnabled = true;
            this.paperSourceComboBox.Name = "paperSourceComboBox";
            // 
            // paperSizeComboBox
            // 
            resources.ApplyResources(this.paperSizeComboBox, "paperSizeComboBox");
            this.paperSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.paperSizeComboBox.FormattingEnabled = true;
            this.paperSizeComboBox.Name = "paperSizeComboBox";
            this.paperSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.paperSizeComboBox_SelectedIndexChanged);
            // 
            // paperSizeLabel
            // 
            resources.ApplyResources(this.paperSizeLabel, "paperSizeLabel");
            this.paperSizeLabel.Name = "paperSizeLabel";
            // 
            // paperSourceLabel
            // 
            resources.ApplyResources(this.paperSourceLabel, "paperSourceLabel");
            this.paperSourceLabel.Name = "paperSourceLabel";
            // 
            // headerFooterTab
            // 
            this.headerFooterTab.Controls.Add(this.footerGroup);
            this.headerFooterTab.Controls.Add(this.headerGroup);
            resources.ApplyResources(this.headerFooterTab, "headerFooterTab");
            this.headerFooterTab.Name = "headerFooterTab";
            // 
            // footerGroup
            // 
            resources.ApplyResources(this.footerGroup, "footerGroup");
            this.footerGroup.Controls.Add(this.footerTableLayoutPanel);
            this.footerGroup.Controls.Add(this.customFooterText);
            this.footerGroup.Controls.Add(this.customFooterLabel);
            this.footerGroup.Name = "footerGroup";
            this.footerGroup.TabStop = false;
            // 
            // footerTableLayoutPanel
            // 
            resources.ApplyResources(this.footerTableLayoutPanel, "footerTableLayoutPanel");
            this.footerTableLayoutPanel.Controls.Add(this.footerTextLabel, 0, 0);
            this.footerTableLayoutPanel.Controls.Add(this.footerAlignmentLabel, 0, 1);
            this.footerTableLayoutPanel.Controls.Add(this.footerMarginUnitsLabel, 2, 2);
            this.footerTableLayoutPanel.Controls.Add(this.footerMarginLabel, 0, 2);
            this.footerTableLayoutPanel.Controls.Add(this.footerMarginInput, 1, 2);
            this.footerTableLayoutPanel.Controls.Add(this.footerTextComboBox, 1, 0);
            this.footerTableLayoutPanel.Controls.Add(this.footerAlignmentComboBox, 1, 1);
            this.footerTableLayoutPanel.Name = "footerTableLayoutPanel";
            // 
            // footerTextLabel
            // 
            resources.ApplyResources(this.footerTextLabel, "footerTextLabel");
            this.footerTextLabel.Name = "footerTextLabel";
            // 
            // footerAlignmentLabel
            // 
            resources.ApplyResources(this.footerAlignmentLabel, "footerAlignmentLabel");
            this.footerAlignmentLabel.Name = "footerAlignmentLabel";
            // 
            // footerMarginUnitsLabel
            // 
            resources.ApplyResources(this.footerMarginUnitsLabel, "footerMarginUnitsLabel");
            this.footerMarginUnitsLabel.Name = "footerMarginUnitsLabel";
            // 
            // footerMarginLabel
            // 
            resources.ApplyResources(this.footerMarginLabel, "footerMarginLabel");
            this.footerMarginLabel.Name = "footerMarginLabel";
            // 
            // footerMarginInput
            // 
            resources.ApplyResources(this.footerMarginInput, "footerMarginInput");
            this.footerMarginInput.DecimalPlaces = 2;
            this.footerMarginInput.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.footerMarginInput.Name = "footerMarginInput";
            this.footerMarginInput.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.footerMarginInput.Validating += new System.ComponentModel.CancelEventHandler(this.footerMarginInput_Validating);
            // 
            // footerTextComboBox
            // 
            resources.ApplyResources(this.footerTextComboBox, "footerTextComboBox");
            this.footerTableLayoutPanel.SetColumnSpan(this.footerTextComboBox, 2);
            this.footerTextComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.footerTextComboBox.FormattingEnabled = true;
            this.footerTextComboBox.Name = "footerTextComboBox";
            this.footerTextComboBox.SelectedIndexChanged += new System.EventHandler(this.footerTextComboBox_SelectedIndexChanged);
            // 
            // footerAlignmentComboBox
            // 
            resources.ApplyResources(this.footerAlignmentComboBox, "footerAlignmentComboBox");
            this.footerTableLayoutPanel.SetColumnSpan(this.footerAlignmentComboBox, 2);
            this.footerAlignmentComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.footerAlignmentComboBox.FormattingEnabled = true;
            this.footerAlignmentComboBox.Name = "footerAlignmentComboBox";
            // 
            // customFooterText
            // 
            resources.ApplyResources(this.customFooterText, "customFooterText");
            this.customFooterText.Name = "customFooterText";
            // 
            // customFooterLabel
            // 
            resources.ApplyResources(this.customFooterLabel, "customFooterLabel");
            this.customFooterLabel.Name = "customFooterLabel";
            // 
            // headerGroup
            // 
            resources.ApplyResources(this.headerGroup, "headerGroup");
            this.headerGroup.Controls.Add(this.headerTableLayoutPanel);
            this.headerGroup.Controls.Add(this.customHeaderText);
            this.headerGroup.Controls.Add(this.customHeaderLabel);
            this.headerGroup.Name = "headerGroup";
            this.headerGroup.TabStop = false;
            // 
            // headerTableLayoutPanel
            // 
            resources.ApplyResources(this.headerTableLayoutPanel, "headerTableLayoutPanel");
            this.headerTableLayoutPanel.Controls.Add(this.headerTextLabel, 0, 0);
            this.headerTableLayoutPanel.Controls.Add(this.headerAlignmentLabel, 0, 1);
            this.headerTableLayoutPanel.Controls.Add(this.headerMarginUnitsLabel, 2, 2);
            this.headerTableLayoutPanel.Controls.Add(this.headerMarginLabel, 0, 2);
            this.headerTableLayoutPanel.Controls.Add(this.headerMarginInput, 1, 2);
            this.headerTableLayoutPanel.Controls.Add(this.headerTextComboBox, 1, 0);
            this.headerTableLayoutPanel.Controls.Add(this.headerAlignmentComboBox, 1, 1);
            this.headerTableLayoutPanel.Name = "headerTableLayoutPanel";
            // 
            // headerTextLabel
            // 
            resources.ApplyResources(this.headerTextLabel, "headerTextLabel");
            this.headerTextLabel.Name = "headerTextLabel";
            // 
            // headerAlignmentLabel
            // 
            resources.ApplyResources(this.headerAlignmentLabel, "headerAlignmentLabel");
            this.headerAlignmentLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.headerAlignmentLabel.Name = "headerAlignmentLabel";
            // 
            // headerMarginUnitsLabel
            // 
            resources.ApplyResources(this.headerMarginUnitsLabel, "headerMarginUnitsLabel");
            this.headerMarginUnitsLabel.Name = "headerMarginUnitsLabel";
            // 
            // headerMarginLabel
            // 
            resources.ApplyResources(this.headerMarginLabel, "headerMarginLabel");
            this.headerMarginLabel.Name = "headerMarginLabel";
            // 
            // headerMarginInput
            // 
            resources.ApplyResources(this.headerMarginInput, "headerMarginInput");
            this.headerMarginInput.DecimalPlaces = 2;
            this.headerMarginInput.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.headerMarginInput.Name = "headerMarginInput";
            this.headerMarginInput.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.headerMarginInput.Validating += new System.ComponentModel.CancelEventHandler(this.headerMarginInput_Validating);
            // 
            // headerTextComboBox
            // 
            resources.ApplyResources(this.headerTextComboBox, "headerTextComboBox");
            this.headerTableLayoutPanel.SetColumnSpan(this.headerTextComboBox, 2);
            this.headerTextComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.headerTextComboBox.FormattingEnabled = true;
            this.headerTextComboBox.Name = "headerTextComboBox";
            this.headerTextComboBox.SelectedIndexChanged += new System.EventHandler(this.headerTextComboBox_SelectedIndexChanged);
            // 
            // headerAlignmentComboBox
            // 
            resources.ApplyResources(this.headerAlignmentComboBox, "headerAlignmentComboBox");
            this.headerTableLayoutPanel.SetColumnSpan(this.headerAlignmentComboBox, 2);
            this.headerAlignmentComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.headerAlignmentComboBox.FormattingEnabled = true;
            this.headerAlignmentComboBox.Name = "headerAlignmentComboBox";
            // 
            // customHeaderText
            // 
            resources.ApplyResources(this.customHeaderText, "customHeaderText");
            this.customHeaderText.Name = "customHeaderText";
            // 
            // customHeaderLabel
            // 
            resources.ApplyResources(this.customHeaderLabel, "customHeaderLabel");
            this.customHeaderLabel.Name = "customHeaderLabel";
            // 
            // OKButton
            // 
            resources.ApplyResources(this.OKButton, "OKButton");
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Name = "OKButton";
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            // 
            // printerButton
            // 
            resources.ApplyResources(this.printerButton, "printerButton");
            this.printerButton.Name = "printerButton";
            this.printerButton.Click += new System.EventHandler(this.printerButton_Click);
            // 
            // okCancelTableLayoutPanel
            // 
            resources.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.Controls.Add(this.OKButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.printerButton, 2, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            // 
            // WorkflowPageSetupDialog
            // 
            this.AcceptButton = this.OKButton;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.okCancelTableLayoutPanel);
            this.Controls.Add(this.tabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkflowPageSetupDialog";
            this.ShowInTaskbar = false;
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.WorkflowPageSetupDialog_HelpButtonClicked);
            this.tabs.ResumeLayout(false);
            this.pageSettingsTab.ResumeLayout(false);
            this.centerGroup.ResumeLayout(false);
            this.centerTableLayoutPanel.ResumeLayout(false);
            this.centerTableLayoutPanel.PerformLayout();
            this.marginsGroup.ResumeLayout(false);
            this.marginsTableLayoutPanel.ResumeLayout(false);
            this.marginsTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.marginsRightInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginsBottomInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginsTopInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginsLeftInput)).EndInit();
            this.scalingGroup.ResumeLayout(false);
            this.scalingTableLayoutPanel.ResumeLayout(false);
            this.scalingTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fitToPagesTallInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fitToPagesWideInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.adjustToScaleInput)).EndInit();
            this.orientationGroup.ResumeLayout(false);
            this.orientationTableLayoutPanel.ResumeLayout(false);
            this.orientationTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.landscapePicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.portraitPicture)).EndInit();
            this.paperSettingsGroup.ResumeLayout(false);
            this.paperTableLayoutPanel.ResumeLayout(false);
            this.paperTableLayoutPanel.PerformLayout();
            this.headerFooterTab.ResumeLayout(false);
            this.footerGroup.ResumeLayout(false);
            this.footerGroup.PerformLayout();
            this.footerTableLayoutPanel.ResumeLayout(false);
            this.footerTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.footerMarginInput)).EndInit();
            this.headerGroup.ResumeLayout(false);
            this.headerGroup.PerformLayout();
            this.headerTableLayoutPanel.ResumeLayout(false);
            this.headerTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerMarginInput)).EndInit();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region Events
        private void OKButton_Click(object sender, System.EventArgs e)
        {
            //serialize state of the dialog into the pageSetupData object
            Margins margins = GetMarginsFromUI();

            //scaling
            this.printDocument.PageSetupData.AdjustToScaleFactor = this.adjustToRadioButton.Checked;

            this.printDocument.PageSetupData.ScaleFactor = (int)this.adjustToScaleInput.Value;
            this.printDocument.PageSetupData.PagesWide = (int)this.fitToPagesWideInput.Value;
            this.printDocument.PageSetupData.PagesTall = (int)this.fitToPagesTallInput.Value;

            //Set the orientation
            this.printDocument.PageSetupData.Landscape = this.landscapeRadioButton.Checked;
            this.printDocument.PageSetupData.Margins = margins;

            //centering
            this.printDocument.PageSetupData.CenterHorizontally = this.CenterHorizontallyCheckBox.Checked;
            this.printDocument.PageSetupData.CenterVertically = this.CenterVerticallyCheckBox.Checked;

            //header inputs
            if (this.headerTextComboBox.SelectedIndex == 0)
                this.printDocument.PageSetupData.HeaderTemplate = string.Empty;
            else
                if (!this.headerTextComboBox.Text.Equals(this.headerFooterCustom))
                    this.printDocument.PageSetupData.HeaderTemplate = this.headerTextComboBox.Text;
                else
                    this.printDocument.PageSetupData.HeaderTemplate = this.customHeaderText.Text;
            this.printDocument.PageSetupData.HeaderCustom = this.headerCustom;
            this.printDocument.PageSetupData.HeaderAlignment = (HorizontalAlignment)this.headerAlignmentComboBox.SelectedItem;
            this.printDocument.PageSetupData.HeaderMargin = UIUnitToPrinterUnit(this.headerMarginInput.Value);

            //footer inputs
            if (this.footerTextComboBox.SelectedIndex == 0)
                this.printDocument.PageSetupData.FooterTemplate = string.Empty;
            else
                if (!this.footerTextComboBox.Text.Equals(this.headerFooterCustom))
                    this.printDocument.PageSetupData.FooterTemplate = this.footerTextComboBox.Text;
                else
                    this.printDocument.PageSetupData.FooterTemplate = this.customFooterText.Text;
            this.printDocument.PageSetupData.FooterCustom = this.footerCustom;
            this.printDocument.PageSetupData.FooterAlignment = (HorizontalAlignment)this.footerAlignmentComboBox.SelectedItem;
            this.printDocument.PageSetupData.FooterMargin = UIUnitToPrinterUnit(this.footerMarginInput.Value);

            // Set the paper size based upon the selection in the combo box.
            if (PrinterSettings.InstalledPrinters.Count > 0)
            {
                if (this.paperSizeComboBox.SelectedItem != null)
                    this.printDocument.DefaultPageSettings.PaperSize = (PaperSize)this.paperSizeComboBox.SelectedItem;

                // Set the paper source based upon the selection in the combo box.
                if (this.paperSourceComboBox.SelectedItem != null)
                    this.printDocument.DefaultPageSettings.PaperSource = (PaperSource)this.paperSourceComboBox.SelectedItem;

                this.printDocument.DefaultPageSettings.Landscape = this.printDocument.PageSetupData.Landscape;
                this.printDocument.DefaultPageSettings.Margins = margins;

                //Make sure that printer setting are changed
                this.printDocument.PrinterSettings.DefaultPageSettings.PaperSize = this.printDocument.DefaultPageSettings.PaperSize;
                this.printDocument.PrinterSettings.DefaultPageSettings.PaperSource = this.printDocument.DefaultPageSettings.PaperSource;
                this.printDocument.PrinterSettings.DefaultPageSettings.Landscape = this.printDocument.PageSetupData.Landscape;
                this.printDocument.PrinterSettings.DefaultPageSettings.Margins = margins;
            }

            this.printDocument.PageSetupData.StorePropertiesToRegistry();
            DialogResult = DialogResult.OK;
        }

        private void printerButton_Click(object sender, System.EventArgs e)
        {
            PrintDialog printDialog = new System.Windows.Forms.PrintDialog();

            printDialog.AllowPrintToFile = false;
            printDialog.Document = this.printDocument;
            try
            {
                if (DialogResult.OK == printDialog.ShowDialog())
                {
                    this.printDocument.PrinterSettings = printDialog.PrinterSettings;
                    this.printDocument.DefaultPageSettings = printDialog.Document.DefaultPageSettings;

                    if (this.printDocument.DefaultPageSettings.Landscape)
                        this.landscapeRadioButton.Checked = true;
                    else
                        this.portraitRadioButton.Checked = true;

                    InitializePaperInformation();

                    this.printDocument.Print();
                }
                else
                {
                    //todo: copy updated settings from the dialog to the print doc
                    //in the worst case it's a no-op, in case user clicked apply/cancel it's the only way to
                    //update the settings (see Winoe#3129 and VSWhidbey#403124 for more details)
                }
            }
            catch (Exception exception)
            {
                string errorString = DR.GetString(DR.SelectedPrinterIsInvalidErrorMessage);
                errorString += "\n" + exception.Message;
                DesignerHelpers.ShowError(this.serviceProvider, errorString);
            }
        }

        private void Margins_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Margins margins = GetMarginsFromUI();

            //get the current paper size
            Size physicalPageSize;
            PaperSize paperSize = this.paperSizeComboBox.SelectedItem as PaperSize;
            if (null != paperSize)
                physicalPageSize = new Size(paperSize.Width, paperSize.Height);
            else
                physicalPageSize = this.printDocument.DefaultPageSettings.Bounds.Size;

            //check the constrains
            int horizontalMarginsSum = margins.Left + margins.Right;
            int verticalMarginsSum = margins.Top + margins.Bottom;

            if (horizontalMarginsSum < physicalPageSize.Width && verticalMarginsSum < physicalPageSize.Height)
                return; //we are good

            //cancelling the change - constrains are not satisfied
            string errorString = DR.GetString(DR.EnteredMarginsAreNotValidErrorMessage);
            DesignerHelpers.ShowError(this.serviceProvider, errorString);
            e.Cancel = true;
        }

        private void headerTextComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.headerCustom = this.headerTextComboBox.Text.Equals(this.headerFooterCustom);
            this.customHeaderText.Enabled = this.headerCustom;
            if (!this.headerCustom)
                this.customHeaderText.Text = this.headerTextComboBox.Text;
        }
        private void footerTextComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.footerCustom = this.footerTextComboBox.Text.Equals(this.headerFooterCustom);
            this.customFooterText.Enabled = this.footerCustom;
            if (!this.footerCustom)
                this.customFooterText.Text = this.footerTextComboBox.Text;
        }

        private void paperSizeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            UpdateHeaderFooterMarginLimit();
        }
        private void landscapeRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            UpdateHeaderFooterMarginLimit();
        }
        private void portraitRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            UpdateHeaderFooterMarginLimit();
        }

        private void UpdateHeaderFooterMarginLimit()
        {
            PaperSize paperSize = this.paperSizeComboBox.SelectedItem as PaperSize;
            if (paperSize != null)
                this.footerMarginInput.Maximum = this.headerMarginInput.Maximum = PrinterUnitToUIUnit(this.landscapeRadioButton.Checked ? paperSize.Width : paperSize.Height);
        }


        private void headerMarginInput_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void footerMarginInput_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void adjustToInput_ValueChanged(object sender, System.EventArgs e)
        {
            this.adjustToRadioButton.Checked = true;
        }
        private void fitToInputs_ValueChanged(object sender, System.EventArgs e)
        {
            this.fitToRadioButton.Checked = true;
        }
        #endregion

        #region Helpers
        private void InitializePaperInformation()
        {
            PrinterSettings.PaperSizeCollection paperSizeCollection = this.printDocument.PrinterSettings.PaperSizes;
            PrinterSettings.PaperSourceCollection paperSourceCollection = this.printDocument.PrinterSettings.PaperSources;

            this.paperSizeComboBox.Items.Clear();
            this.paperSizeComboBox.DisplayMember = "PaperName";
            foreach (PaperSize paperSize in paperSizeCollection)
            {
                if (paperSize.PaperName != null && paperSize.PaperName.Length > 0)
                {
                    this.paperSizeComboBox.Items.Add(paperSize);
                    if (null == this.paperSizeComboBox.SelectedItem &&
                    this.printDocument.DefaultPageSettings.PaperSize.Kind == paperSize.Kind &&
                    this.printDocument.DefaultPageSettings.PaperSize.Width == paperSize.Width &&
                    this.printDocument.DefaultPageSettings.PaperSize.Height == paperSize.Height)
                    {
                        this.paperSizeComboBox.SelectedItem = paperSize;
                        this.printDocument.DefaultPageSettings.PaperSize = paperSize;
                    }
                }
            }

            if (null == this.paperSizeComboBox.SelectedItem)
            {
                PaperKind paperKind = this.printDocument.DefaultPageSettings.PaperSize.Kind;

                this.printDocument.DefaultPageSettings = new PageSettings(this.printDocument.PrinterSettings);
                foreach (PaperSize paperSize in this.paperSizeComboBox.Items)
                {
                    if (null == this.paperSizeComboBox.SelectedItem &&
                    paperKind == paperSize.Kind &&
                    this.printDocument.DefaultPageSettings.PaperSize.Width == paperSize.Width &&
                    this.printDocument.DefaultPageSettings.PaperSize.Height == paperSize.Height)
                    {
                        this.paperSizeComboBox.SelectedItem = paperSize;
                        this.printDocument.DefaultPageSettings.PaperSize = paperSize;
                    }
                }

                //We still did not find matching paper so not select first in the list
                if (null == this.paperSizeComboBox.SelectedItem &&
                this.paperSizeComboBox.Items.Count > 0)
                {
                    this.paperSizeComboBox.SelectedItem = this.paperSizeComboBox.Items[0] as PaperSize;
                    this.printDocument.DefaultPageSettings.PaperSize = this.paperSizeComboBox.SelectedItem as PaperSize;
                }
            }

            ///////////////Select the appropriate paper source based on the pageSettings
            this.paperSourceComboBox.Items.Clear();
            this.paperSourceComboBox.DisplayMember = "SourceName";
            foreach (PaperSource paperSource in paperSourceCollection)
            {
                this.paperSourceComboBox.Items.Add(paperSource);
                if (null == this.paperSourceComboBox.SelectedItem &&
                this.printDocument.DefaultPageSettings.PaperSource.Kind == paperSource.Kind &&
                this.printDocument.DefaultPageSettings.PaperSource.SourceName == paperSource.SourceName)
                    this.paperSourceComboBox.SelectedItem = paperSource;
            }

            if (null == this.paperSourceComboBox.SelectedItem &&
            this.paperSourceComboBox.Items.Count > 0)
            {
                this.paperSourceComboBox.SelectedItem = this.paperSourceComboBox.Items[0] as PaperSource;
                this.printDocument.DefaultPageSettings.PaperSource = this.paperSourceComboBox.SelectedItem as PaperSource;
            }
        }

        //

        private void SetMarginsToUI(Margins margins)
        {
            this.marginsLeftInput.Value = PrinterUnitToUIUnit(margins.Left);
            this.marginsRightInput.Value = PrinterUnitToUIUnit(margins.Right);
            this.marginsTopInput.Value = PrinterUnitToUIUnit(margins.Top);
            this.marginsBottomInput.Value = PrinterUnitToUIUnit(margins.Bottom);
        }

        private Margins GetMarginsFromUI()
        {
            Margins margins = new Margins(
                UIUnitToPrinterUnit(this.marginsLeftInput.Value),
                UIUnitToPrinterUnit(this.marginsRightInput.Value),
                UIUnitToPrinterUnit(this.marginsTopInput.Value),
                UIUnitToPrinterUnit(this.marginsBottomInput.Value));
            return margins;
        }

        private decimal PrinterUnitToUIUnit(int printerValue)
        {
            return Convert.ToDecimal((double)printerValue / 100.0d); //in 1/100 of inch
        }

        private int UIUnitToPrinterUnit(decimal uiValue)
        {
            return Convert.ToInt32((double)uiValue * 100.0d); //in 1/100 of inch
        }
        #endregion

        private void WorkflowPageSetupDialog_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            GetHelp();
        }

        protected override void OnHelpRequested(HelpEventArgs hlpevent)
        {
            hlpevent.Handled = true;
            GetHelp();
        }

        private void GetHelp()
        {
            DesignerHelpers.ShowHelpFromKeyword(this.serviceProvider, typeof(WorkflowPageSetupDialog).FullName + ".UI");
        }
    }
}
