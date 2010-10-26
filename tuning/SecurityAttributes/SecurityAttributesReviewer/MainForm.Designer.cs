namespace SecurityAttributesReviewer
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this._assemblyList = new System.Windows.Forms.ToolStripComboBox();
			this._refreshButton = new System.Windows.Forms.ToolStripButton();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this._browser = new System.Windows.Forms.WebBrowser();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this._log = new System.Windows.Forms.RichTextBox();
			this._contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this._menuItemAddDeclaringTypeToCriticalTypeList = new System.Windows.Forms.ToolStripMenuItem();
			this._menuItemAddToAuditedSafeList = new System.Windows.Forms.ToolStripMenuItem();
			this._menuItemAddToReviewedMethodList = new System.Windows.Forms.ToolStripMenuItem();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.browseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this._contextMenu.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._assemblyList,
            this._refreshButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(973, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// _assemblyList
			// 
			this._assemblyList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._assemblyList.Name = "_assemblyList";
			this._assemblyList.Size = new System.Drawing.Size(121, 25);
			this._assemblyList.SelectedIndexChanged += new System.EventHandler(this._assemblyList_SelectedIndexChanged);
			// 
			// _refreshButton
			// 
			this._refreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this._refreshButton.Image = ((System.Drawing.Image)(resources.GetObject("_refreshButton.Image")));
			this._refreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this._refreshButton.Name = "_refreshButton";
			this._refreshButton.Size = new System.Drawing.Size(23, 22);
			this._refreshButton.Text = "Refresh";
			this._refreshButton.ToolTipText = "Detects method privileges and reloads the public api information";
			this._refreshButton.Click += new System.EventHandler(this._refreshButton_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 25);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(973, 355);
			this.tabControl1.TabIndex = 1;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this._browser);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(965, 329);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Public Apis";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// _browser
			// 
			this._browser.Dock = System.Windows.Forms.DockStyle.Fill;
			this._browser.Location = new System.Drawing.Point(3, 3);
			this._browser.MinimumSize = new System.Drawing.Size(20, 20);
			this._browser.Name = "_browser";
			this._browser.Size = new System.Drawing.Size(959, 323);
			this._browser.TabIndex = 0;
			this._browser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this._browser_Navigating);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this._log);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(965, 329);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Behind the Curtains";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// _log
			// 
			this._log.Dock = System.Windows.Forms.DockStyle.Fill;
			this._log.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._log.Location = new System.Drawing.Point(3, 3);
			this._log.Name = "_log";
			this._log.ReadOnly = true;
			this._log.Size = new System.Drawing.Size(959, 323);
			this._log.TabIndex = 0;
			this._log.Text = "";
			// 
			// _contextMenu
			// 
			this._contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuItemAddDeclaringTypeToCriticalTypeList,
            this._menuItemAddToAuditedSafeList,
            this._menuItemAddToReviewedMethodList,
            this.browseToolStripMenuItem});
			this._contextMenu.Name = "_contextMenu";
			this._contextMenu.Size = new System.Drawing.Size(298, 114);
			// 
			// _menuItemAddDeclaringTypeToCriticalTypeList
			// 
			this._menuItemAddDeclaringTypeToCriticalTypeList.Image = ((System.Drawing.Image)(resources.GetObject("_menuItemAddDeclaringTypeToCriticalTypeList.Image")));
			this._menuItemAddDeclaringTypeToCriticalTypeList.Name = "_menuItemAddDeclaringTypeToCriticalTypeList";
			this._menuItemAddDeclaringTypeToCriticalTypeList.Size = new System.Drawing.Size(297, 22);
			this._menuItemAddDeclaringTypeToCriticalTypeList.Text = "Add <declaring type> to Critical Type List";
			this._menuItemAddDeclaringTypeToCriticalTypeList.Click += new System.EventHandler(this._menuItemAddDeclaringTypeToCriticalTypeList_Click);
			// 
			// _menuItemAddToAuditedSafeList
			// 
			this._menuItemAddToAuditedSafeList.Image = ((System.Drawing.Image)(resources.GetObject("_menuItemAddToAuditedSafeList.Image")));
			this._menuItemAddToAuditedSafeList.Name = "_menuItemAddToAuditedSafeList";
			this._menuItemAddToAuditedSafeList.Size = new System.Drawing.Size(297, 22);
			this._menuItemAddToAuditedSafeList.Text = "Add <signature> to Audited Safe List";
			this._menuItemAddToAuditedSafeList.Click += new System.EventHandler(this._menuItemAddToAuditedSafeList_Click);
			// 
			// _menuItemAddToReviewedMethodList
			// 
			this._menuItemAddToReviewedMethodList.Image = ((System.Drawing.Image)(resources.GetObject("_menuItemAddToReviewedMethodList.Image")));
			this._menuItemAddToReviewedMethodList.Name = "_menuItemAddToReviewedMethodList";
			this._menuItemAddToReviewedMethodList.Size = new System.Drawing.Size(297, 22);
			this._menuItemAddToReviewedMethodList.Text = "Add <signature> to Reviewed Method List";
			this._menuItemAddToReviewedMethodList.Click += new System.EventHandler(this._menuItemAddToKnownUnsafe_Click);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "Refresh.png");
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._statusLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 358);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(973, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// _statusLabel
			// 
			this._statusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this._statusLabel.Name = "_statusLabel";
			this._statusLabel.Size = new System.Drawing.Size(39, 17);
			this._statusLabel.Text = "Ready";
			// 
			// browseToolStripMenuItem
			// 
			this.browseToolStripMenuItem.Name = "browseToolStripMenuItem";
			this.browseToolStripMenuItem.Size = new System.Drawing.Size(297, 22);
			this.browseToolStripMenuItem.Text = "Browse Classlibrary Source";
			this.browseToolStripMenuItem.Click += new System.EventHandler(this.browseToolStripMenuItem_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(973, 380);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.toolStrip1);
			this.Name = "MainForm";
			this.Text = "CoreCLR Reviewer 6000";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this._contextMenu.ResumeLayout(false);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.WebBrowser _browser;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ContextMenuStrip _contextMenu;
		private System.Windows.Forms.ToolStripMenuItem _menuItemAddDeclaringTypeToCriticalTypeList;
		private System.Windows.Forms.ToolStripMenuItem _menuItemAddToAuditedSafeList;
		private System.Windows.Forms.ToolStripButton _refreshButton;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ToolStripComboBox _assemblyList;
		private System.Windows.Forms.RichTextBox _log;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
		private System.Windows.Forms.ToolStripMenuItem _menuItemAddToReviewedMethodList;
		private System.Windows.Forms.ToolStripMenuItem browseToolStripMenuItem;
	}
}

