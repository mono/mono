namespace System.Workflow.Activities.Rules.Design
{
    partial class BasicBrowserDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BasicBrowserDialog));
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.rulesListView = new System.Windows.Forms.ListView();
            this.nameColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.validColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.rulesPanel = new System.Windows.Forms.Panel();
            this.rulesToolStrip = new System.Windows.Forms.ToolStrip();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.newRuleToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.editToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.renameToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.preiviewPanel = new System.Windows.Forms.Panel();
            this.previewRichEditBoxPanel = new System.Windows.Forms.Panel();
            this.previewRichTextBox = new System.Windows.Forms.TextBox();
            this.previewLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.headerPictureBox = new System.Windows.Forms.PictureBox();
            this.okCancelTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.rulesPanel.SuspendLayout();
            this.rulesToolStrip.SuspendLayout();
            this.preiviewPanel.SuspendLayout();
            this.previewRichEditBoxPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerPictureBox)).BeginInit();
            this.okCancelTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Click += new System.EventHandler(this.OnCancel);
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.Click += new System.EventHandler(this.OnOk);
            // 
            // rulesListView
            // 
            this.rulesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumnHeader,
            this.validColumnHeader});
            resources.ApplyResources(this.rulesListView, "rulesListView");
            this.rulesListView.FullRowSelect = true;
            this.rulesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.rulesListView.HideSelection = false;
            this.rulesListView.MultiSelect = false;
            this.rulesListView.Name = "rulesListView";
            this.rulesListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.rulesListView.UseCompatibleStateImageBehavior = false;
            this.rulesListView.View = System.Windows.Forms.View.Details;
            this.rulesListView.DoubleClick += new System.EventHandler(this.OnDoubleClick);
            this.rulesListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.OnItemSelectionChanged);
            // 
            // nameColumnHeader
            // 
            resources.ApplyResources(this.nameColumnHeader, "nameColumnHeader");
            // 
            // validColumnHeader
            // 
            resources.ApplyResources(this.validColumnHeader, "validColumnHeader");
            // 
            // rulesPanel
            // 
            resources.ApplyResources(this.rulesPanel, "rulesPanel");
            this.rulesPanel.Controls.Add(this.rulesToolStrip);
            this.rulesPanel.Controls.Add(this.rulesListView);
            this.rulesPanel.Name = "rulesPanel";
            // 
            // rulesToolStrip
            // 
            this.rulesToolStrip.BackColor = System.Drawing.SystemColors.Control;
            this.rulesToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.rulesToolStrip.ImageList = this.imageList;
            this.rulesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRuleToolStripButton,
            this.editToolStripButton,
            this.renameToolStripButton,
            this.toolStripSeparator1,
            this.deleteToolStripButton});
            resources.ApplyResources(this.rulesToolStrip, "rulesToolStrip");
            this.rulesToolStrip.Name = "rulesToolStrip";
            this.rulesToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.rulesToolStrip.TabStop = true;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "NewRule.bmp");
            this.imageList.Images.SetKeyName(1, "EditRule.bmp");
            this.imageList.Images.SetKeyName(2, "RenameRule.bmp");
            this.imageList.Images.SetKeyName(3, "Delete.bmp");
            // 
            // newRuleToolStripButton
            // 
            resources.ApplyResources(this.newRuleToolStripButton, "newRuleToolStripButton");
            this.newRuleToolStripButton.Name = "newRuleToolStripButton";
            this.newRuleToolStripButton.Click += new System.EventHandler(this.OnNew);
            // 
            // editToolStripButton
            // 
            resources.ApplyResources(this.editToolStripButton, "editToolStripButton");
            this.editToolStripButton.Name = "editToolStripButton";
            this.editToolStripButton.Click += new System.EventHandler(this.OnEdit);
            // 
            // renameToolStripButton
            // 
            resources.ApplyResources(this.renameToolStripButton, "renameToolStripButton");
            this.renameToolStripButton.Name = "renameToolStripButton";
            this.renameToolStripButton.Click += new System.EventHandler(this.OnRename);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // deleteToolStripButton
            // 
            resources.ApplyResources(this.deleteToolStripButton, "deleteToolStripButton");
            this.deleteToolStripButton.Name = "deleteToolStripButton";
            this.deleteToolStripButton.Click += new System.EventHandler(this.OnDelete);
            // 
            // preiviewPanel
            // 
            this.preiviewPanel.Controls.Add(this.previewRichEditBoxPanel);
            this.preiviewPanel.Controls.Add(this.previewLabel);
            resources.ApplyResources(this.preiviewPanel, "preiviewPanel");
            this.preiviewPanel.Name = "preiviewPanel";
            // 
            // previewRichEditBoxPanel
            // 
            resources.ApplyResources(this.previewRichEditBoxPanel, "previewRichEditBoxPanel");
            this.previewRichEditBoxPanel.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.previewRichEditBoxPanel.Controls.Add(this.previewRichTextBox);
            this.previewRichEditBoxPanel.Name = "previewRichEditBoxPanel";
            // 
            // previewRichTextBox
            // 
            this.previewRichTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.previewRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.previewRichTextBox, "previewRichTextBox");
            this.previewRichTextBox.Name = "previewRichTextBox";
            this.previewRichTextBox.ReadOnly = true;
            this.previewRichTextBox.TabStop = false;
            // 
            // previewLabel
            // 
            resources.ApplyResources(this.previewLabel, "previewLabel");
            this.previewLabel.Name = "previewLabel";
            // 
            // descriptionLabel
            // 
            resources.ApplyResources(this.descriptionLabel, "descriptionLabel");
            this.descriptionLabel.Name = "descriptionLabel";
            // 
            // headerPictureBox
            // 
            resources.ApplyResources(this.headerPictureBox, "headerPictureBox");
            this.headerPictureBox.Name = "headerPictureBox";
            this.headerPictureBox.TabStop = false;
            // 
            // okCancelTableLayoutPanel
            // 
            resources.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            // 
            // BasicBrowserDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.okCancelTableLayoutPanel);
            this.Controls.Add(this.headerPictureBox);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.preiviewPanel);
            this.Controls.Add(this.rulesPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BasicBrowserDialog";
            this.ShowInTaskbar = false;
            this.rulesPanel.ResumeLayout(false);
            this.rulesPanel.PerformLayout();
            this.rulesToolStrip.ResumeLayout(false);
            this.rulesToolStrip.PerformLayout();
            this.preiviewPanel.ResumeLayout(false);
            this.preiviewPanel.PerformLayout();
            this.previewRichEditBoxPanel.ResumeLayout(false);
            this.previewRichEditBoxPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerPictureBox)).EndInit();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ListView rulesListView;
        private System.Windows.Forms.Panel rulesPanel;
        private System.Windows.Forms.ToolStrip rulesToolStrip;
        private System.Windows.Forms.Panel preiviewPanel;
        private System.Windows.Forms.Label previewLabel;
        private System.Windows.Forms.ToolStripButton newRuleToolStripButton;
        private System.Windows.Forms.ToolStripButton renameToolStripButton;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.PictureBox headerPictureBox;
        private System.Windows.Forms.ToolStripButton editToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton deleteToolStripButton;
        private System.Windows.Forms.Panel previewRichEditBoxPanel;
        private System.Windows.Forms.TextBox previewRichTextBox;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ColumnHeader validColumnHeader;
        private System.Windows.Forms.TableLayoutPanel okCancelTableLayoutPanel;
    }
}
